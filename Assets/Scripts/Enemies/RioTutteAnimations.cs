using UnityEngine;

public class RioTutteAnimations : EnemyAnimationController
{
    RioTutteScript rioTutte;
    protected override void Awake()
    {
        base.Awake();
        rioTutte = enemy as RioTutteScript;
    }
    protected override void Update()
    {
        base.Update();
        animator.SetBool("dashGrab", rioTutte.isUsingDashGrab);
        animator.SetBool("fireDash", rioTutte.isUsingFireDash);
    }
}
