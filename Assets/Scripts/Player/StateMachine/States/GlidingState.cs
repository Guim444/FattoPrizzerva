using UnityEngine;

public class GlidingState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public PlayerStaminaManager stamina;
    public float speed = 3f; // Will be overridden by player.glidingSpeed

    public GlidingState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    public void Enter()
    {
        Debug.Log("Entered GlidingState State");
        if (stamina != null)
        {
            stamina.SetWalking(); // Use walking stamina consumption
        }
    }

    public void Update()
    {
        if (player == null || controller == null) return;

        // Get input from player
        Vector3 input = player.GetDirectionalInput();
        
        // Apply inertia for smooth movement
        Vector3 toMove = player.ApplyInertia(input, Time.deltaTime, player.glidingTurnSpeed);
        
        // Phase 2: Restrict movement to radial direction (slope axis only)
        // This delegates to RingSlopeHandler
        toMove = player.RestrictToSlopeDirection(toMove);
        
        // Move the character
        if (toMove != Vector3.zero && controller.enabled == true)
        {
            controller.Move(toMove * player.glidingSpeed * Time.deltaTime);
        }
    }

    public void Exit()
    {
        Debug.Log("Exited GlidingState State");
    }
}