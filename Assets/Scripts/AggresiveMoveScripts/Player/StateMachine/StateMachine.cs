using UnityEngine; //Imports Unity features (like MonoBehaviour, Vector3, etc.).
using System.Collections.Generic; //Imports generic collections (like Dictionary).
using Unity.VisualScripting;
using System;

public static class StateMachine //Defines a new class called StateMachine.
{
    private static Dictionary<State, IStateActions> _states = new Dictionary<State, IStateActions>();
    //Creates a dictionary that maps a State enum value to an object that implements IStateActions (the logic for that state)
    private static IStateActions _currentState; // creates a private variable _currentState that keeps track of which state is active.

    public static void AddState(State key, IStateActions state) //Adds a state and its logic to the dictionary.
    {
        _states[key] = state;
    }

    public static void SetState(State newState)  //changes the current state
    {
        if (_currentState != null)
            _currentState.Exit();    //if there is another state active, calls exit to it

        _currentState = _states[newState];   //switches _currentstate into a new one
        //       _currentState = _states[State.Running]; //“Look inside the dictionary for the row where the key is State.Running, and make the current state the value of (the RunningState object).”
        _currentState.Enter(); //calls the function that makes it start working
    }

    public static void Update()
    {
        _currentState?.Update();   //every frame, it calls Update() on the current state
    }
}
