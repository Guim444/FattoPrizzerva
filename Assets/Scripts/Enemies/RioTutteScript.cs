using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class RioTutteScript : EnemyController
{
    public float moveSpeed;

    bool isUsingDashGrab = false, isGrabbing = false;
    public Vector3 finalPosition = Vector3.zero; //used to dash

    public float phaseTwoHP;
    public List<GameObject> phaseThreeCollisionedObjects = new List<GameObject>();
    public bool hasHitAnObjectAfterAPunch = false;

    private void Awake()
    {
        attackTime = Random.Range(3, 5);
    }

    protected override void Update()
    {
        base.Update();
        if (enemyPhase == 2 && hasHitAnObjectAfterAPunch && knockbackSpeed.magnitude <= 0.01f)
        {
            hasHitAnObjectAfterAPunch = false;
        }
        /*if (phaseTwoHP > 0) base.Update();
        else
        {
            isMoving = false;
            isAttacking = false;
            isGrabbing = false;
        }*/
    }
    public override void FollowPlayerLogic()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        Vector3 move = new Vector3(directionToPlayer.x, 0, directionToPlayer.z) * moveSpeed;
        controller.Move(move * Time.deltaTime);
    }

    public override IEnumerator ChangeBossPhase()
    {
        while (hasKnockback)
        {
            yield return null;
        }

        player.battleIsActive = false;
        player.canMove = false;
        
        //A little pause between phases
        yield return new WaitForSeconds(1);
        player.canMove = true;
        player.battleIsActive = true;
        enemyPhase += 1;

        if (enemyPhase == 1) endurance++;

    }
    void OnTriggerEnter(Collider other)
    {
        if (hitTimer <= 0 && other.CompareTag("Player") && !other.isTrigger && !isAttacking && canHitPlayer)
        {
            if (!isUsingDashGrab)
            {
                knockbackDirection = lastDir;
                knockbackDirection.y = 0;

                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    PushForce(-knockbackDirection, player.endurance, other.gameObject);
                    player.PushForce(knockbackDirection, endurance);

                    hitTimer = maxHitTimer;
                    canHitPlayer = false;
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
        if ((isUsingDashGrab || hasHitAnObjectAfterAPunch) && (hit.gameObject.CompareTag("Wall") || hit.gameObject.CompareTag("Collisionable element")))
        {
            isAttacking = false;
            moveSpeed = 1.5f;
            isUsingDashGrab = false;
            attackTime = Random.Range(4, 5);
            PushForce(-finalPosition, endurance + 1, hit.gameObject);
            if (enemyPhase == 2 && hasKnockback && hasHitAnObjectAfterAPunch && hit.gameObject.CompareTag("Collisionable element"))
            {
                if (!phaseThreeCollisionedObjects.Contains(hit.gameObject))
                {
                    phaseThreeCollisionedObjects.Add(hit.gameObject);

                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    StartCoroutine(StopDamageAnim(sr));
                    Debug.Log("Added object to the 'ban list'");
                }
            }
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

        player.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z);

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
        if (enemyPhase >= 1)
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
    public override void DamagedBehaviour(int dmg)
    {
        if (enemyPhase == 1)
        {
            if (dmg >= 30)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                StartCoroutine(StopDamageAnim(sr));
                phaseTwoHP = Mathf.Max(phaseTwoHP - dmg, 0);
                Debug.Log(phaseTwoHP);

                if (phaseTwoHP <= 0)
                {
                    StartCoroutine(ChangeBossPhase());
                }
            }
        }
        else
        {
            hasHitAnObjectAfterAPunch = true; //We set it true so it can enter the collision controller if mandatory
            StartCoroutine(AutomaticDeactivationOfHitAfterPunch());
        }
    }
    IEnumerator StopDamageAnim(SpriteRenderer sr)
    {
        sr.color = Color.red;
        while (hasKnockback)
        {
            yield return null;
        }
        sr.color = Color.white;
    }
    IEnumerator AutomaticDeactivationOfHitAfterPunch()
    {
        yield return new WaitForSeconds(1);
        hasHitAnObjectAfterAPunch = false;
    }
}
