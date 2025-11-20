using UnityEngine;

public abstract class GenericBattleManager : MonoBehaviour
{
    public static GenericBattleManager instance;
    public PlayerController player;
    //we don't assign here the EnemyController, every class who inherits this script will have its own
    public bool battleIsActive = true;

    public abstract void TriggerCinematic();
}
