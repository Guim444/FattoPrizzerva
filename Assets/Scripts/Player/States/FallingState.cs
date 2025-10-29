using UnityEngine;

public class FallingState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered FallingState State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited FallingState State");
    }
}