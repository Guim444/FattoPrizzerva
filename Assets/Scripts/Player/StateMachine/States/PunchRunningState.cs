using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PunchRunningState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public PlayerStaminaManager stamina;
    public float staminaCost = 5f;
    public float baseSpeed = 6, actualSpeed;

    public BoxCollider punchCollider;

    public PunchRunningState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }
    public void Enter()
    {
        Debug.Log("Entered PunchingState State");
        player.staminaManager.ModifyStamina(-staminaCost); // Example stamina cost for punching
        player.normalPunchTimer = player.animator.GetCurrentAnimatorStateInfo(0).length; // Set punch timer based on animation length
        CalcSpeed();
        player.StartCoroutine(Punch(player.normalPunchTimer));
    }

    public void Update()
    {
        //will keep moving in X direction while punching, but you can modify Y direction with input.
        Vector3 input = player.lastDirection;
        Vector3 toMove = player.ApplyInertia(input, Time.deltaTime, 4f);
        toMove.y = 0; // Keep Y movement zero to maintain horizontal movement only

        if (player.damageBoost == 2)
        {
            toMove = Sprint(); // Override toMove with sprinting logic for the thrust phase 3
        }

        if (controller.enabled == true)
        {
            controller.Move(new Vector3(toMove.x, 0, Sprint().z) * actualSpeed * Time.deltaTime); //move the character based on input and speed, the same as moving state
        }
    }

    void CalcSpeed()
    {
        switch (player.damageBoost)
        {
            case 2:
                actualSpeed = baseSpeed * 1.4f;
                player.damageBoost = 1;
                CameraFollow.instance.smoothSpeed = 3f;
                break;
            case 3:
                actualSpeed = baseSpeed * 1.8f;
                player.damageBoost = 2;
                CameraFollow.instance.smoothSpeed = 4f;
                break;
            default:
                CameraFollow.instance.smoothSpeed = 2f;
                break;
        }
    }

    IEnumerator Punch(float duration)
    {
        yield return new WaitForSeconds(2 * duration / 3); // enable collider after 2/3 of the punch animation for better timing
        punchCollider.enabled = true;
        yield return new WaitForSeconds(3 * duration / 4); // disable collider after 3/4 of the punch animation, for better timing
        punchCollider.enabled = false;
    }

    Vector3 Sprint()
    {
        //only runs in X in this state
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical") / 3;

        Vector3 sprintDirection = new Vector3(moveX, 0, moveZ).normalized;

        return sprintDirection;
    }

    public void Exit()
    {
        Debug.Log("Exited PunchRunning State");
    }
}