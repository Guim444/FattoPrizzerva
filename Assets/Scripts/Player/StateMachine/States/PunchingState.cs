using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PunchingState : IStateActions
{
    public float speed = 2f; // Will be overridden by player.punchingSpeed
    public float staminaCostPunch = 2; // Will be overridden by player.punchStaminaCostPhase0
    public float baseDmg = 2;
    public PlayerController player;
    public SphereCollider punchCollider;
    public CharacterController controller;
    public PlayerStaminaManager stamina;

    bool punchExecuted = false;
    public PunchingState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    bool isMoving;
    public void Enter()
    {
        if (punchExecuted) return;
        staminaCostPunch = player.punchStaminaCostPhase0; // Use player's tuning value
        player.staminaManager.ModifyStamina(-staminaCostPunch); // Example stamina cost for punching
        player.normalPunchTimer = player.animator.GetCurrentAnimatorStateInfo(0).length; // Set punch timer based on animation length
        player.StartCoroutine(Punch(player.normalPunchTimer));
        player.playerDamage = player.basicPunchDamage;
    }

    public void Update()
    {
        isMoving = player.GetDirectionalInput().magnitude > 0.1f;
        Vector3 input;
        if (isMoving)
        {
            stamina.SetWalking();
            input = player.GetDirectionalInput();
            if (controller.enabled == true)
            {
                controller.Move(input * player.punchingSpeed * Time.deltaTime); //move the character based on input and speed, the same as moving state
            }
        }
        else
        {
            stamina.SetIdle();
            if (controller.enabled == true) controller.Move(Vector3.zero);
        }
    }
    IEnumerator Punch(float duration)
    {
        //Time is divided in 30 parts for better timing control with the frame-based animation

        yield return new WaitForSeconds(20 * duration / 30); // enable collider after 2/3 of the punch animation for better timing

        if (player.canAttack)
        {
            player.endurance += 1; // Gain endurance on punch
            punchCollider.enabled = true;

            yield return new WaitForSeconds(24 * duration / 30); // disable collider after 3/4 of the punch animation, for better timing
            player.endurance -= 1; // Remove gained endurance
            punchCollider.enabled = false;
            player.canAttack = false;
        }
    }
    public void Exit()
    {
        punchCollider.enabled = false;
        player.playerDamage = 0;
        player.canAttack = true;
    }
}