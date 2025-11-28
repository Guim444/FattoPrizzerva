using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public abstract class GenericBattleManager : MonoBehaviour
{
    public GameObject deathMessage;
    public static GenericBattleManager instance;
    public PlayerController player;

    //CHECKPOINT DATA
    public Vector3 checkpointPlayerPos;
    public float checkpointPlayerHP;

    //we don't assign here the EnemyController, every class who inherits this script will have its own
    public bool battleIsActive = true;

    protected virtual void Awake()
    {
        SetCheckpoint();
    }
    public abstract void TriggerCinematic();

    public abstract void SetCheckpoint();
    public abstract void GetCheckpoint();

    public void ShowDeathMessage(bool deathMessageEnabled)
    {
        deathMessage.SetActive(deathMessageEnabled);
        if (deathMessageEnabled)
        {
            StartCoroutine(TextFades());
        }
    }
    IEnumerator TextFades()
    {
        TextMeshProUGUI[] texts = deathMessage.GetComponentsInChildren<TextMeshProUGUI>(true);

        List<Coroutine> running = new List<Coroutine>();

        foreach (var t in texts)
            running.Add(StartCoroutine(FadeInText(t)));

        foreach (var c in running)
            yield return c;

        StartCoroutine(BlinkText(texts[1]));
        
    }
    IEnumerator FadeInText(TextMeshProUGUI text)
    {
        text.alpha = 0;
        while (text.alpha < 2f)
        {
            text.alpha += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator BlinkText(TextMeshProUGUI text)
    {
        float speed = 2f;

        while (true)
        {
            text.alpha = Mathf.Abs(Mathf.Sin(Time.time * speed));
            yield return null;
        }
    }
}
