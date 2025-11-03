using UnityEngine;

public class PunchRunningState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered PunchRunning State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited PunchRunning State");
    }
}