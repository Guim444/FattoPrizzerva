using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    public Animator animator;
    public PlayerController player;

    private void Awake()
    {
    }
    void Update()
    {
        animator.SetBool("hasKnockback", player.hasKnockback); //set hasKnockback parameter based on player state
        if (player.hasKnockback)
            return; //if player has knockback, do not play other animations

        bool isPunching = player.normalPunchTimer > 0; //set isPunching parameter based on player state
        animator.SetBool("isPunching", isPunching);

        bool isRunning = player.currentState == State.Running; //set isRunning parameter based on player state
        animator.SetBool("isRunning", isRunning);
    }
}
