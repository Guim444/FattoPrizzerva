using UnityEngine;

public class TiredState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public PlayerStaminaManager stamina;
    public float speed = 1f; // Will be overridden by player.tiredSpeed

    public TiredState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }
    public void Enter()
    {
        stamina.SetTired();
        // Note: "Speed" parameter doesn't exist in animator - removed to prevent error
        // Animation is controlled by PlayerAnimations.cs based on state
    }

    public void Exit()
    {
    }


    // Update is called once per frame
    void Update()
    {
        // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
        // Phase 2: Don't handle movement if on slope (GlidingState handles it)
        // if (player.isOnSlope)
        // {
        //     return;
        // }

        Vector3 toMove = player.GetDirectionalInput();  //Variable that stores the final direction and speed of the character (calculated based on input and camera)
        toMove = player.ApplyInertia(toMove, Time.deltaTime, player.tiredTurnSpeed); //Apply inertia to the movement for smoother transitions

        if (toMove != Vector3.zero && controller.enabled == true)
        {
            controller.Move(toMove * player.tiredSpeed * Time.deltaTime); //Move the character based on the final calculated movement
        }
    }

    void IStateActions.Update()
    {
        Update();
    }
}
