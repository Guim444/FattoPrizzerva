using UnityEngine;

public class JumpingState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered JumpingState State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited JumpingState State");
    }
}