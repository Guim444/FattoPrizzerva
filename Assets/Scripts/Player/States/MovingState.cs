using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MovingState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public float speed = 3f;
    public float gravity = -9.81f;

    private Vector3 velocity;
    public void Enter()
    {
        Debug.Log("Entered Moving State");
    }

    public void Update()
    {
        Vector3 toMove = player.GetDirectionalInput();  //Variable that stores the final direction and speed of the character (calculated based on input and camera)

        if (toMove != Vector3.zero && controller.enabled == true)
        {
            controller.Move(toMove * speed * Time.deltaTime);
        }

        /* Apply gravity
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            velocity.y = -1f;
        }
        Debug.Log("Direction: " + toMove);
    } */
    }
    public void Exit()
    {
        Debug.Log("Exited Moving State");
    }

}