using UnityEngine;

public class GlidingState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered GlidingState State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited GlidingState State");
    }
}