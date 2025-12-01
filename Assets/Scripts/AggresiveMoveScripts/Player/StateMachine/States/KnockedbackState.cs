using UnityEngine;

public class KnockedbackState : IStateActions
{
    public PlayerController player;
    public CharacterController controller;
    float timer;
    public void Enter()
    {
        Debug.Log("Entered Knockedback State");
    }

    public void Update()
    {
    }

    public void Exit()
    {
        Debug.Log("Exited Knockedback State");
    }
}