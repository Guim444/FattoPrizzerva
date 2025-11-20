using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class RioTutteScript : EnemyController
{
    public float moveSpeed;

    public bool isUsingDashGrab = false, isGrabbing = false;
    public Vector3 finalPosition = Vector3.zero; //used to dash

    public float phaseTwoHP;
    public List<GameObject> phaseThreeCollisionedObjects = new List<GameObject>();
    public bool hasHitAnObjectAfterAPunch = false;

    public List<UnityAction> attackList = new List<UnityAction>();
    int randomAttackSlot = 0; //this is used to choose randomly one of the attacks

    private void Awake()
    {
        attackTime = Random.Range(3, 6);
        attackList.Add(DashGrab);
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
        enemyPhase++;
        if (enemyPhase == 1)
        {
            if (!attackList.Contains(DashGrab)) attackList.Add(DashGrab);
        }
        else if (enemyPhase == 2)
        {
            if (!attackList.Contains(FireDash)) attackList.Add(FireDash);
            player.canMove = false;

            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => !hasKnockback);

            RioTutteBattleManager.instance.battleIsActive = false;

            //A little pause between phases
            yield return new WaitForSeconds(1);
            player.canMove = true;
            RioTutteBattleManager.instance.battleIsActive = true;
            endurance++;
        }
        else if (enemyPhase == 3)
        {
            yield return null;
            RioTutteBattleManager.instance.TriggerCinematic();
        }
    }
    void OnTriggerEnter(Collider other)
    {

        /*if (hitTimer <= 0 && other.CompareTag("Player") && !other.isTrigger && !isAttacking && canHitPlayer)
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
        }*/

        if (!other.CompareTag("Player") || other.isTrigger || hitTimer > 0 || !canHitPlayer)
            return;

        if (isAttacking && !isUsingDashGrab)
            return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector3 knockbackDirection = (player.transform.position - transform.position).normalized;
            knockbackDirection.y = 0;

            if (isUsingDashGrab)
            {
                player.StopAllCoroutines();

                player.currentState = State.Idle;
                StateMachine.SetState(player.currentState);

                finalPosition = Vector3.zero;
                GrabPlayer();
                return;
            }
            else
            {
                PushForce(-knockbackDirection, player.endurance, other.gameObject);
                player.PushForce(knockbackDirection, endurance);

                hitTimer = maxHitTimer;
                canHitPlayer = false;
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
            attackTime = Random.Range(3, 6);
            float magnitude = Mathf.Max(finalPosition.magnitude, hit.transform.position.magnitude);
            finalPosition *= magnitude;
            StartCoroutine(DelayedPush(-finalPosition, hit.gameObject));
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

            Debug.Log(phaseThreeCollisionedObjects.Count);
            if (phaseThreeCollisionedObjects.Count >= 1)
            {
                StartCoroutine(ChangeBossPhase());
            }

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
        if (!isGrabbing)
        {
            int extraX = player.transform.position.x > transform.position.x ? 1 : -1;
            Vector3 grabbedPosition = new Vector3(transform.position.x + extraX, player.transform.position.y, transform.position.z);
            Vector3 displacement = grabbedPosition - player.transform.position;
            player.cc.Move(displacement);

            player.canMove = false;

            isGrabbing = true;
            moveSpeed = 0;
            isAttacking = true;
            isMoving = false;

            float length = animator.GetCurrentAnimatorStateInfo(0).length;
            StartCoroutine(HitPlayer(length));
        }
    }

    IEnumerator HitPlayer(float length)
    {
        yield return new WaitForSeconds(length);
        Vector3 pushDirection = new Vector3((player.transform.position - transform.position).x, 0, (player.transform.position - transform.position).z);
        player.PushForce(pushDirection, player.endurance + 2); //we ensure this value is always superior to the player's endurance

        player.currentState = State.Knockedback;
        StateMachine.SetState(player.currentState);

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
            if (attackChosen)
            {
                randomAttackSlot = Random.Range(0, attackList.Count + 1);
                attackChosen = true;
            }
            attackList[randomAttackSlot].Invoke();
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

    public void FireDash()
    {
        Debug.Log("Por implementar");
        attackTime = Random.Range(3, 6);
    }

    public override void DamagedBehaviour(int dmg)
    {
        if (isUsingDashGrab)
        {
            if (canBeKnockedback)
            {
                isMoving = true;
                isAttacking = false;
                moveSpeed = 1.5f;
                isUsingDashGrab = false;
                if (attackTime <= 0) attackTime = Random.Range(3, 6);
            }
        }

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
        else if (enemyPhase == 2)
        {

            hasHitAnObjectAfterAPunch = true; //We set it true so it can enter the collision controller if mandatory
            StartCoroutine(AutomaticDeactivationOfHitAfterPunch());
        }
    }
    IEnumerator StopDamageAnim(SpriteRenderer sr)
    {
        yield return null;
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

    IEnumerator DelayedPush(Vector3 dir, GameObject obj)
    {
        yield return null;
        PushForce(dir, endurance + 1, obj);
    }

    
}
