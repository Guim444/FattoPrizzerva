using System.Collections;
using UnityEngine;

public class RioTutteScript : EnemyController
{
    public float moveSpeed;

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

    public override void TakeDamage()
    {
        hitCollider.enabled = false;
        StartCoroutine(ReactivateCollider());
    }

    IEnumerator ReactivateCollider()
    {
        yield return new WaitForSeconds(0.5f);
        hitCollider.enabled = true;
    }

    public override void ChangeBossPhase()
    {
        endurance += 1;
    }
    void OnTriggerEnter(Collider other)
    {
        if (hitTimer <= 0 && other.CompareTag("Player") && !other.isTrigger)
        {
            knockbackDirection = lastDir;
            knockbackDirection.y = 0;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                PushForce(-knockbackDirection, player.endurance);
                player.PushForce(knockbackDirection, endurance);

                hitTimer = maxHitTimer;
                hitCollider.enabled = false; 
            }
        }
    }
}
