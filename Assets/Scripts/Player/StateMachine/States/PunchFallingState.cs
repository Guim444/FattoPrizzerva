using UnityEngine;

public class PunchFallingState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered PunchFallingState State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited PunchFallingState State");
    }
}