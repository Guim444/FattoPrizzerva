using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MovingState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public float speed = 3f;
    public float gravity = -9.81f;
    public PlayerStaminaManager stamina;
    public MovingState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    private Vector3 velocity;
    public void Enter()
    {
        Debug.Log("Entered Moving State");
        stamina.SetWalking();
    }

    public void Update()
    {
        Vector3 input = player.GetDirectionalInput(); //get input from player
        Vector3 toMove = player.ApplyInertia(input, Time.deltaTime, 5f); //set the inertia for the movement
        if (toMove != Vector3.zero && controller.enabled == true)
        {
            controller.Move(toMove * speed * Time.deltaTime); //move the character based on input and speed
        }
    }
    public void Exit()
    {
        Debug.Log("Exited Moving State");
    }

}