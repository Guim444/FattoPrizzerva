using UnityEngine;

public abstract class GenericBattleManager : MonoBehaviour
{
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
}
