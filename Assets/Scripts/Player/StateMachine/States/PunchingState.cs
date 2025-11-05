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
        player.normalPunchTimer = player.gameObject.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length; // Set punch timer based on animation length
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
            controller.Move(Vector3.zero);
        }
    }
    IEnumerator Punch(float duration)
    {
        yield return new WaitForSeconds(duration/2);
        player.GetComponent<SpriteRenderer>().color = Color.red; // Visual cue for punching state, just for testing
        punchCollider.enabled = true;
    }
    public void Exit()
    {
        Debug.Log("Exited PunchingState State");
        player.GetComponent<SpriteRenderer>().color = Color.white; // Reset color on exit
        punchCollider.enabled = false;
    }
}