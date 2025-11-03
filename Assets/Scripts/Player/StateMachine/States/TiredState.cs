using UnityEngine;

public class TiredState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    public PlayerStaminaManager stamina;
    public float speed = 1f;

    public TiredState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }
    public void Enter()
    {
        stamina.SetTired();
        player.animator.SetFloat("Speed", 1f);
    }

    public void Exit()
    {
    }


    // Update is called once per frame
    void Update()
    {
        Vector3 toMove = player.GetDirectionalInput();  //Variable that stores the final direction and speed of the character (calculated based on input and camera)
        toMove = player.ApplyInertia(toMove, Time.deltaTime, 10f); //Apply inertia to the movement for smoother transitions

        if (toMove != Vector3.zero && controller.enabled == true)
        {
            controller.Move(toMove * speed * Time.deltaTime); //Move the character based on the final calculated movement
        }
    }

    void IStateActions.Update()
    {
        Update();
    }
}
