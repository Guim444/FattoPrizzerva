using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PunchingState : IStateActions
{
    public float speed = 2f;
    public float staminaCostPunch = 5;
    public float baseDmg = 2;
    public PlayerController player;
    public SphereCollider punchCollider;
    public CharacterController controller;
    public PlayerStaminaManager stamina;

    public PunchingState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    bool isMoving;
    public void Enter()
    {
        Debug.Log("Entered PunchingState State");
        player.staminaManager.ModifyStamina(-staminaCostPunch); // Example stamina cost for punching
        player.normalPunchTimer = player.animator.GetCurrentAnimatorStateInfo(0).length; // Set punch timer based on animation length
        player.StartCoroutine(Punch(player.normalPunchTimer));
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
                controller.Move(input * speed * Time.deltaTime); //move the character based on input and speed, the same as moving state
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
        yield return new WaitForSeconds(2 * duration / 3); // enable collider after 2/3 of the punch animation for better timing
        punchCollider.enabled = true;

        yield return new WaitForSeconds(4 * duration / 5); // disable collider after 3/4 of the punch animation, for better timing
        punchCollider.enabled = false;
    }
    public void Exit()
    {
        Debug.Log("Exited PunchingState State");
        player.GetComponent<SpriteRenderer>().color = Color.white; // Reset color on exit
        punchCollider.enabled = false;
    }
}