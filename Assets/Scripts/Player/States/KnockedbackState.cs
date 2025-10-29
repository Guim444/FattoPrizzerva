using UnityEngine;

public class KnockedbackState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered Knockedback State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited Knockedback State");
    }
}