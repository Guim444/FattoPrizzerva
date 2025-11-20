using System.Collections;
using UnityEngine;

public class RioTutteBattleManager : GenericBattleManager
{
    public RioTutteScript rioTutte;


    private void Awake()
    {
        instance = this;
    }
    public override void TriggerCinematic()
    {
        if (rioTutte.enemyPhase == 3)
        {
            Debug.Log("A");
            player.canMove = false;

            StartCoroutine(Phase4Cinematic());
        }
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
        Vector3 dir = (player.transform.position - rioTutte.transform.position)/2;
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
}
