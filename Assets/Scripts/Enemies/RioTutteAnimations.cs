using UnityEngine;

public class RioTutteAnimations : EnemyAnimationController
{
    protected override void Update()
    {
        base.Update();
        animator.SetBool("hasKnockback", enemy.hasKnockback);
        animator.SetBool("damageCondition", enemy.damageCondition);
    }
}
