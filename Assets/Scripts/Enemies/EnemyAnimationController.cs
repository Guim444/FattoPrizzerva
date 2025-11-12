using UnityEngine;

public abstract class EnemyAnimationController : MonoBehaviour
{
    public Animator animator;
    public EnemyController enemy;
    // Update is called once per frame
    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemy = GetComponent<EnemyController>();
    }
    void Update()
    {
        animator.SetBool("isMoving", enemy.isMoving);
        animator.SetBool("isAttacking", enemy.isAttacking);
    }
}
