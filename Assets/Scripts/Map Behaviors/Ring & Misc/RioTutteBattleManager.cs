using System.Collections;
using UnityEngine;

public class RioTutteBattleManager : GenericBattleManager
{
    public RioTutteScript rioTutte;

    //CHECKPOINT DATA
    public Vector3 checkpointRioTuttePos;
    public float checkpointPhaseTwoHP;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
    }
    public override void TriggerCinematic()
    {
        if (rioTutte.enemyPhase == 2)
        {
            rioTutte.enemyPhase = 1; //We avoid issues
            player.canMove = false;
            StartCoroutine(Phase3Cinematic());
        }
        if (rioTutte.enemyPhase == 3)
        {
            player.canMove = false;
            StartCoroutine(Phase4Cinematic());
        }
    }

    IEnumerator Phase3Cinematic()
    {
        battleIsActive = false;
        float lenght = rioTutte.animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(lenght + 0.5f);
        player.canMove = true;
        battleIsActive = true;
        rioTutte.enemyPhase = 2;
    }
    IEnumerator Phase4Cinematic()
    {
        yield return new WaitForSeconds(0.5f); //We wait a bit
        yield return new WaitUntil(() => !rioTutte.hasKnockback); //We wait until it has no knockback
        instance.battleIsActive = false;

        Vector3 sceneCenter = new Vector3(8.5f, 0, -4f);
        Vector3 dir = (sceneCenter - rioTutte.transform.position).normalized;
        dir.y = 0;
        dir = dir.normalized;

        player.animator.SetBool("isMoving", true);

        float duration = 2f;
        float timer = 0;

        while (timer < duration)
        {
            player.cc.Move(dir * player.walkingSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        player.animator.SetBool("isMoving", false);
        StartCoroutine(RioTuttePhase4Approaching());
    }
    IEnumerator RioTuttePhase4Approaching()
    {
        rioTutte.animator.speed = 0.5f;
        yield return new WaitForSeconds(0.5f);
        Vector3 dir = (player.transform.position - rioTutte.transform.position)/1.25f;
        dir.y = 0f;
        dir = dir.normalized;


        for (int i = 0; i < 3; i++)
        {
            float timer = 0f;
            while (timer < 1f)
            {
                rioTutte.animator.speed = 1;
                rioTutte.animator.SetBool("isMoving", true);
                rioTutte.controller.Move(dir * rioTutte.moveSpeed * Time.deltaTime);
                timer += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(1f);
            rioTutte.animator.speed = 0.5f;
            rioTutte.animator.SetBool("isMoving", false);
            rioTutte.isMoving = false;
        }
    }

    public override void SetCheckpoint()
    {
        checkpointPlayerPos = player.transform.position;
        checkpointPlayerHP = player.HP;

        checkpointRioTuttePos = rioTutte.transform.position;
        if (rioTutte.enemyPhase == 1)
            checkpointPhaseTwoHP = 100;
        else
            checkpointPhaseTwoHP = 0;
    }

    public override void GetCheckpoint()
    {
        player.cc.enabled = false;
        player.transform.position = checkpointPlayerPos;
        player.cc.enabled = true;
        player.HP = checkpointPlayerHP;

        rioTutte.controller.enabled = false;
        rioTutte.transform.position = checkpointRioTuttePos;
        rioTutte.controller.enabled = true;
        rioTutte.phaseTwoHP = checkpointPhaseTwoHP;        
        if (rioTutte.enemyPhase == 2)
            rioTutte.phaseThreeCollisionedObjects.Clear();


        player.animator.SetBool("isDead", false);
        player.currentState = State.Idle;
        StateMachine.SetState(player.currentState);

        player.canMove = true;
        rioTutte.isMoving = true;
        rioTutte.isAttacking = false;
        player.knockbackVelocity = Vector3.zero;
        rioTutte.knockbackSpeed = Vector3.zero;

        battleIsActive = true;
    }
}
