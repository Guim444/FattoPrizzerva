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

        animator.SetBool("isPunching", player.currentState == State.Punching); //set isPunching parameter based on player state

        animator.SetBool("isRunning", player.currentState == State.Running); //set isRunning parameter based on player state
    }
}
