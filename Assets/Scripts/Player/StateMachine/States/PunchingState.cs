using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PunchingState : IStateActions
{
    public float speed = 2f;
    public float staminaCostPunch = 2;
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
        //Time is divided in 30 parts for better timing control with the frame-based animation

        yield return new WaitForSeconds(20 * duration / 30); // enable collider after 2/3 of the punch animation for better timing
        player.GetComponent<SpriteRenderer>().color = Color.red; // Change color to red when punch hits
        player.endurance += 1; // Gain endurance on punch
        punchCollider.enabled = true;

        yield return new WaitForSeconds(24 * duration / 30); // disable collider after 3/4 of the punch animation, for better timing
        player.GetComponent<SpriteRenderer>().color = Color.white; // Reset color after punch
        player.endurance -= 1; // Remove the gained endurance
        punchCollider.enabled = false;
    }
    public void Exit()
    {
        Debug.Log("Exited PunchingState State");
        player.GetComponent<SpriteRenderer>().color = Color.white; // Reset color on exit
        punchCollider.enabled = false;
    }
}