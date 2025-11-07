using UnityEngine;

public class IdleState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public PlayerStaminaManager stamina;
    public IdleState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }
    public void Enter()
    {
        stamina.SetIdle();
        player.animator.speed = 1.0f;
    }

    public void Update()
    {
        Vector3 toMove = player.ApplyInertia(Vector3.zero, Time.deltaTime, 10f); //reset the inertia while idle
        // No specific logic for idle state
    }

    public void Exit()
    {
        // Cleanup if needed
    }
}