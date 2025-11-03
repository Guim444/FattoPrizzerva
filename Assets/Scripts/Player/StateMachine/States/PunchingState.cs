using UnityEngine;

public class PunchingState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered PunchingState State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited PunchingState State");
    }
}