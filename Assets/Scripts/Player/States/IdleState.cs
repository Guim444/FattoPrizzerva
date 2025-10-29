using UnityEngine;

public class IdleState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;

    public void Enter()
    {
        player.animator.SetFloat("Speed", 0f); // Set Speed to 0 for idle animation
    }

    public void Update()
    {
        // No specific logic for idle state
    }

    public void Exit()
    {
        // Cleanup if needed
    }
}