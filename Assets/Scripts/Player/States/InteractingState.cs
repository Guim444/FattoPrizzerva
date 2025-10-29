using Unity.VisualScripting;
using UnityEngine;

public class InteractingState : IStateActions
{
    public void Enter()
    {
        Debug.Log("Entered InteractingState State");
    }

    public void Update()
    {
        // TODO: Movement behavior here
    }

    public void Exit()
    {
        Debug.Log("Exited InteractingState State");
    }
}