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
        //We want the player to inherit the inertia from previous movement
        float currentVelocity = player.currentSpeed.magnitude * player.walkingSpeed;
        float turnSpeed = Mathf.Lerp(2f, 10f, currentVelocity / player.walkingSpeed);
        Vector3 inertia = player.ApplyInertia(Vector3.zero, Time.deltaTime, turnSpeed);
    }

    public void Exit()
    {
        player.currentSpeed = Vector3.zero;
    }
}