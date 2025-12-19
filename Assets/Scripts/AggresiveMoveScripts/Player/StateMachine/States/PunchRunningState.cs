using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class PunchRunningState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public PlayerStaminaManager stamina;
    public float staminaCost = 5f; // Will be overridden by player's tuning values
    public float baseSpeed = 6, actualSpeed; // baseSpeed will be overridden by player.punchRunningBaseSpeed

    public BoxCollider punchCollider;
    public int tempEndurance = 0;

    bool punchExecuted = false;

    public PunchRunningState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }
    public void Enter()
    {
        if (punchExecuted) return;
        punchExecuted = true;


        tempEndurance = player.endurance;
        player.animator.speed = 1;
        
        baseSpeed = player.punchRunningBaseSpeed; // Use player's tuning value

        switch (player.damageBoost) //This will temporary change player's endurance for the push
        {
            case 0: staminaCost = player.punchRunningStaminaCostPhase1; player.endurance += 1; break;
            case 1: staminaCost = player.punchRunningStaminaCostPhase2; player.endurance += 2; break;
            case 2: staminaCost = player.punchRunningStaminaCostPhase3; player.endurance += 3; break;
        }

        if (player.staminaManager.currentStamina - staminaCost >= 0)
        {
            player.staminaManager.ModifyStamina(-staminaCost); // Example stamina cost for punching
            player.normalPunchTimer = player.animator.GetCurrentAnimatorStateInfo(0).length; // Set punch timer based on animation length
            CalcSpeed();
            player.StartCoroutine(Punch(player.normalPunchTimer));
        }

    }

    public void Update()
    {
        //will keep moving in X direction while punching, but you can modify Y direction with input.
        Vector3 input = player.lastDirection;
        Vector3 toMove = player.ApplyInertia(input, Time.deltaTime, player.punchRunningTurnSpeed);
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
        //Speed manager for the punch
        switch (player.damageBoost)
        {
            case 0:
                actualSpeed = baseSpeed * 1.2f;
                CameraFollow.instance.smoothSpeed = 2f;
                player.playerDamage = player.thrustDamage1;
                break;
            case 1:
                actualSpeed = baseSpeed * 1.4f;
                CameraFollow.instance.smoothSpeed = 3f;
                player.playerDamage = player.thrustDamage2;
                break;
            case 2:
                actualSpeed = baseSpeed * 1.8f;
                CameraFollow.instance.smoothSpeed = 4f;

                punchCollider.size = new Vector3 (0.85f, punchCollider.size.y, punchCollider.size.z);
                punchCollider.center = new Vector3 (0.6f, punchCollider.center.y, punchCollider.center.z);
                player.playerDamage = player.thrustDamage3;

                break;
            default:
                CameraFollow.instance.smoothSpeed = 2f;
                player.playerDamage = player.thrustDamage1;
                break;
        }
    }

    IEnumerator Punch(float duration)
    {
        yield return new WaitForSeconds(20 * duration / 30); // enable collider after 2/3 of the punch animation for better timing

        if (player.canAttack)
        {
            punchCollider.enabled = true;

            yield return new WaitForSeconds(24 * duration / 30); // disable collider after 3/4 of the punch animation, for better timing
            punchCollider.enabled = false;

        }
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
        punchCollider.size = new Vector3 (0.7f, punchCollider.size.y, punchCollider.size.z);
        punchCollider.center = new Vector3 (0.5f, punchCollider.center.y, punchCollider.center.z);

        player.playerDamage = 0;
        player.endurance = tempEndurance; //we reset the endurance
        punchExecuted = false;
    }
}