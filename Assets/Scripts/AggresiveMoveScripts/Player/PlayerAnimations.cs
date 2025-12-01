using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public static PlayerAnimations instance;
    public Animator animator;
    public PlayerController player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void LateUpdate()
    {
        animator.SetBool("hasKnockback", player.currentState == State.Knockedback); //set hasKnockback parameter based on player state

        animator.SetBool("isMoving", player.currentState == State.Moving); //set isMoving parameter based on player state

        if (player.currentState == State.PunchRunning || player.currentState == State.Running) animator.SetInteger("RunPunchLvl", player.damageBoost);

        animator.SetBool("isPunching", player.currentState == State.Punching || player.currentState == State.PunchRunning); //set isPunching parameter based on player state

        animator.SetBool("isRunning", player.currentState == State.Running); //set isRunning parameter based on player state

        animator.SetBool("isTired", player.currentState == State.Tired); //set isTired parameter based on player state
    }
}
