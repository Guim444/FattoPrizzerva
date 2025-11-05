using UnityEngine;

public class RioTutteScript : EnemyController
{
    public float moveSpeed = 3f;
    public override void TakeDamage()
    {
        throw new System.NotImplementedException();
    }
    protected override void Update()
    {
        base.Update();
    }
    public override void FollowPlayerLogic()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        Vector3 move = new Vector3(directionToPlayer.x, 0, directionToPlayer.z) * moveSpeed;
        controller.Move(move * Time.deltaTime);
    }
}
