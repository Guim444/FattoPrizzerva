using UnityEngine;

public abstract class EnemyAnimationController : MonoBehaviour
{
    public Animator animator;
    public EnemyController enemy;
    // Update is called once per frame
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyController>();
    }
    protected virtual void Update()
    {
        animator.SetBool("isMoving", enemy.isMoving);
        animator.SetBool("isAttacking", enemy.isAttacking);
        animator.SetBool("hasKnockback", enemy.hasKnockback);
        animator.SetBool("damageCondition", enemy.damageCondition);
    }
}
