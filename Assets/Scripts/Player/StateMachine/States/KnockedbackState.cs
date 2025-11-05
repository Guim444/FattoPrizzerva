using UnityEngine;

public class KnockedbackState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    float timer;
    public void Enter()
    {
        Debug.Log("Entered Knockedback State");
    }

    public void Update()
    {
        if (timer < 0.5f)
        {
            // Apply knockback movement. We first check if there's significant knockback velocity to apply.
            if (player.knockbackVelocity.magnitude > 0.1f)
            {
                controller.Move(player.knockbackVelocity * Time.deltaTime);
                player.knockbackVelocity = Vector3.Lerp(player.knockbackVelocity, Vector3.zero, 10f * Time.deltaTime);
            }
            timer += Time.deltaTime;
        }
        else
        {
            //if timer exceeds 0.5 seconds, exit the knockedback state
            Exit();
        }
    }

    public void Exit()
    {
        Debug.Log("Exited Knockedback State");
        timer = 0;
    }
}