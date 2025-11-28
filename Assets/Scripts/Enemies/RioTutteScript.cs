using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class RioTutteScript : EnemyController
{
    public float moveSpeed;

    public bool isUsingDashGrab = false, isGrabbing = false, isUsingFireDash = false;
    public Vector3 finalPosition = Vector3.zero, startPosition = Vector3.zero; //used to dash

    public float phaseTwoHP;
    public List<GameObject> phaseThreeCollisionedObjects = new List<GameObject>();
    public int objectCountGoal; //max amount of phase three collisioned objects until it changes the phase
    public bool hasHitAnObjectAfterAPunch = false;

    public List<UnityAction> attackList = new List<UnityAction>();
    int randomAttackSlot = 0; //this is used to choose randomly one of the attacks
    GameObject lastObjectHit;

    public float groundedTimer = 0;
    public bool fallDirection; //true = front, false = back

    int layerNum;

    private void Awake()
    {
        layerNum = gameObject.layer;
        attackTime = Random.Range(3, 6);

        attackList.Add(DashGrab);
        attackList.Add(FireDash);

        if (enemyPhase < 1) canAttack = false;
    }

    protected override void Update()
    {
        base.Update();

        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }

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
        RioTutteBattleManager.instance.SetCheckpoint();

        if (enemyPhase == 1)
        {
            if (!attackList.Contains(DashGrab)) attackList.Add(DashGrab);
        }
        else if (enemyPhase == 2)
        {
            if (!attackList.Contains(FireDash)) attackList.Add(FireDash);
            player.canMove = false;

            RioTutteBattleManager.instance.TriggerCinematic();

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

        if (!other.CompareTag("Player") || other.isTrigger)
            return;

        if (isAttacking && !isUsingDashGrab)
            return;

        if (isUsingFireDash)
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
                hasBeenPushed = false;
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
            isUsingDashGrab = false;
            attackTime = Random.Range(3, 6);
            hasBeenPushed = true;

            if (enemyPhase == 2 && hasKnockback && hasHitAnObjectAfterAPunch && hit.gameObject.CompareTag("Collisionable element") && !phaseThreeCollisionedObjects.Contains(hit.gameObject))
            {
                canAttack = false;
                moveSpeed = 0;
                groundedTimer = 3;
                fallDirection = hit.transform.parent.CompareTag("Front fall");
                canHitPlayer = false;
                lastObjectHit = hit.gameObject;

                StartCoroutine(FallBackDuringAnimation(animator.GetCurrentAnimatorStateInfo(0).length, 2f));

                /*phaseThreeCollisionedObjects.Add(hit.gameObject);
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                StartCoroutine(StopDamageAnim(sr));
                Debug.Log("Added object to the 'ban list'");*/
            }
            else
            {
                moveSpeed = 1.5f;
                float magnitude = Mathf.Max(finalPosition.magnitude, hit.transform.position.magnitude);
                finalPosition *= magnitude;
                StartCoroutine(DelayedPush(-finalPosition, hit.gameObject));
            }
            finalPosition = Vector3.zero;

            if (phaseThreeCollisionedObjects.Count >= objectCountGoal)
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
            if (!attackChosen)
            {
                if (Mathf.Abs(Vector3.Distance(transform.position, player.transform.position)) > 5f) //we only want fire dash chance if RioTutte is close to the character
                {
                    Debug.DrawLine(transform.position, transform.position + (player.transform.position - transform.position).normalized * 10, Color.blue, 1f);
                    randomAttackSlot = 0; //Too far to do fire dash
                    Debug.Log("Too far");
                }
                else randomAttackSlot = Random.Range(0, attackList.Count);
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
        // Ejecutar solo la inicialización UNA VEZ
        if (!isUsingFireDash)
        {
            startPosition = transform.position;
            isUsingFireDash = true;
            isAttacking = true;

            float dashDistance = moveSpeed * (44f / 60f);

            Vector3 desiredDirection = -lastDir.normalized;

            bool foundDirection = TryGetValidDirection(desiredDirection, dashDistance, out finalPosition);

            int attempts = 0;
            while (!foundDirection && attempts < 20)
            {
                Vector3 rnd = RandomDirection();
                foundDirection = TryGetValidDirection(rnd, dashDistance, out finalPosition);
                attempts++;
            }

            if (!foundDirection)
            {
                isUsingFireDash = false;
                isAttacking = false;
                return;
            }

            gameObject.layer = LayerMask.NameToLayer("Invulnerable");
            dashCoroutineRunning = false;
        }

        moveSpeed = 15f;

        Vector3 dashDir = (finalPosition - startPosition).normalized;
        FlipCharacter(dashDir);

        controller.Move(dashDir * moveSpeed * Time.deltaTime);

        if (!dashCoroutineRunning)
        {
            dashCoroutineRunning = true;
            StartCoroutine(EndFireDash(animator.GetCurrentAnimatorStateInfo(0).length));
        }
    }

    bool dashCoroutineRunning = false;
    IEnumerator EndFireDash(float animLength)
    {
        yield return new WaitForSeconds(animLength);

        gameObject.layer = layerNum;

        isUsingFireDash = false;
        isAttacking = false;
        isMoving = true;

        moveSpeed = 1.5f;
        attackTime = Random.Range(3, 6);

        yield return null;

        dashCoroutineRunning = false;
    }
    private Vector3 RandomDirection()
    {
        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;
    }

    bool TryGetValidDirection(Vector3 dir, float dist, out Vector3 result)
    {
        float dashDuration = animator.GetCurrentAnimatorStateInfo(0).length;
        float realDashDistance = 15f * dashDuration;

        Debug.DrawRay(transform.position, dir.normalized * realDashDistance, Color.blue, 1f);

        RaycastHit hit;

        if (Physics.Raycast(transform.position, dir.normalized, out hit, realDashDistance, LayerMask.GetMask("Wall")))
        {
            Debug.Log("Wall: " + hit.collider.name);
            result = Vector3.zero;
            return false;
        }

        Debug.Log("No roadblock");
        result = transform.position + dir.normalized * dist;
        return true;
    }

    //PROVISIONAL FIX:
    /*public IEnumerator FireDashBack()
    {
        isUsingFireDash = false;
        isAttacking = true;

        Vector3 backDir = -finalPosition.normalized;
        float dashSpeed = 15f;
        float dashTime = 0.2f; 

        float t = 0f;
        while (t < dashTime)
        {
            controller.Move(backDir * dashSpeed * Time.deltaTime);
            t += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        isMoving = true;
        moveSpeed = 1.5f;
        attackTime = Random.Range(3, 6);
    }*/
    public override void DamagedBehaviour(int dmg)
    {
        if (dmg > 0) hasBeenPushed = true;

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

            //MUST FIX:
            /*if (groundedTimer <= 0)
            {
                phaseThreeCollisionedObjects.Add(lastObjectHit);
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                StartCoroutine(StopDamageAnim(sr));
                Debug.Log("Added object to the 'ban list'");
            }*/
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

    /*void FallingLogic()
    {
        float step = fallSpeed * Time.deltaTime;

        fallen += step;
        controller.Move(new Vector3(0, 0, -step));

        if (fallen >= fallDistance)
        {
            isFalling = false;
            fallen = 0f;
        }
    }*/
    IEnumerator FallBackDuringAnimation(float animationLength, float fallDistance)
    {
        string clipName = fallDirection ? "RioTutteFallFront" : "RioTutteFallBack";

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(clipName));

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float startZ = transform.position.z;
        float targetZ = startZ - fallDistance;

        while (state.normalizedTime < 1f)
        {
            state = animator.GetCurrentAnimatorStateInfo(0);
            float t = state.normalizedTime;
            float newZ = Mathf.Lerp(startZ, targetZ, t);
            controller.Move(new Vector3(0, 0, newZ - transform.position.z));
            yield return null;
        }
    }
}
