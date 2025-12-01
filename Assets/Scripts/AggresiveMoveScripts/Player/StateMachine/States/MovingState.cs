using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MovingState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public float speed = 3f; // Will be overridden by player.walkingSpeed
    public float gravity = -9.81f;
    public PlayerStaminaManager stamina;
    public MovingState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    private Vector3 velocity;
    public void Enter()
    {
        stamina.SetWalking();
    }

    public void Update()
    {
        // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
        // Phase 2: Don't handle movement if on slope (GlidingState handles it)
        // if (player.isOnSlope)
        // {
        //     return;
        // }
        
        Vector3 input = player.GetDirectionalInput(); //get input from player
        Vector3 toMove = player.ApplyInertia(input, Time.deltaTime, player.walkingTurnSpeed); //set the inertia for the movement
        
        if (toMove != Vector3.zero && controller.enabled == true)
        {
            controller.Move(toMove * player.walkingSpeed * Time.deltaTime); //move the character based on input and speed
        }
    }
    public void Exit()
    {
    }

}