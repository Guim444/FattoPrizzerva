using System.Collections;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class RioTutteScript : EnemyController
{
    public float moveSpeed;

    bool isUsingDashGrab = false, isGrabbing = false;
    public Vector3 finalPosition = Vector3.zero; //used to dash

    private void Awake()
    {
        attackTime = Random.Range(3, 5);
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

    public override void ChangeBossPhase()
    {
        endurance += 1;
    }
    void OnTriggerEnter(Collider other)
    {
        if (hitTimer <= 0 && other.CompareTag("Player") && !other.isTrigger && !isAttacking)
        {
            if (!isUsingDashGrab)
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
            else //this will happen when collides with the player.
            {
                finalPosition = Vector3.zero;
                GrabPlayer();
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isUsingDashGrab && hit.gameObject.CompareTag("Wall"))
        {
            isAttacking = false;
            moveSpeed = 1.5f;
            isUsingDashGrab = false;
            attackTime = Random.Range(4, 5);
            Debug.Log(attackTime);
            PushForce(-finalPosition, endurance + 1);
            finalPosition = Vector3.zero;
        }
        else if (isUsingDashGrab && hit.gameObject.CompareTag("Player"))
        {
            //In this case, it grabs the player when he's not looking at RioTutte. That's why player should flip.
            finalPosition = Vector3.zero;
            player.FlipCharacter(transform.position - player.transform.position);
            GrabPlayer();
        }
    }
    public void GrabPlayer()
    {
        player.canMove = false;

        transform.position = new Vector3(transform.position.x, transform.position.y, player.transform.position.z);
        isGrabbing = true;
        moveSpeed = 0;
        isAttacking = true;
        isMoving = false;
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        StartCoroutine(HitPlayer(length));
    }

    IEnumerator HitPlayer(float length)
    {
        yield return new WaitForSeconds(length);
        Vector3 pushDirection = new Vector3((player.transform.position - transform.position).x, 0, (player.transform.position - transform.position).z);
        player.PushForce(pushDirection, player.endurance + 2); //we ensure this value is always superior to the player's endurance
        player.TakeDamage(1);
        if (player.HP > 0) player.canMove = true;
        attackTime = Random.Range(3, 6);

        //reset values
        isMoving = true;
        isAttacking = false;
        isGrabbing = false;
        isUsingDashGrab = false;
        moveSpeed = 1.5f;
    }
    //Down here are the atack behaviours of RioTutte
    public override void Attack()
    {
        if (enemyPhase == 1)
        {
            DashGrab();
        }
    }

    public void DashGrab()
    {
        if (!isUsingDashGrab)
        {
            Vector3 distance = player.transform.position - transform.position;
            finalPosition = new Vector3(distance.x, 0, distance.z).normalized;
        }
        isAttacking = true;
        isUsingDashGrab = true;
        moveSpeed = 7.5f;
        FlipCharacter(finalPosition);
        controller.Move(finalPosition * moveSpeed * Time.deltaTime);
    }
}
