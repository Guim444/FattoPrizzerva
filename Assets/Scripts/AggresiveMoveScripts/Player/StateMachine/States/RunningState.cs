using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class RunningState : IStateActions
{

    public PlayerController player;
    public CharacterController controller;
    public float baseSpeed, actualSpeed; // baseSpeed will be overridden by player.runningBaseSpeed
    public float staminaCostPerSecond;

    // Thrust phases
    private int currentThrustPhase = 1;

    public PlayerStaminaManager stamina;

    public float turnSpeed, dt;

    public float[] topSpeed = { 9, 11f, 13.5f};
    public float[] currentAcceleration = { 0.0035f, 0.0045f, 0.006f };


    public RunningState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    public void Enter()
    {
        currentThrustPhase = 1; // Reset to phase 1 on entering running state
        CameraFollow.instance.smoothSpeed = 2;

        player.animator.speed = 1;
        baseSpeed = player.runningBaseSpeed; // Use player's tuning value
        actualSpeed = baseSpeed;
        staminaCostPerSecond = player.runningStaminaCost[0]; // Use player's tuning value
        stamina.SetRunning(staminaCostPerSecond);
    }

    public void Update()
    {
        float dt = Time.deltaTime;

        // get the real input direction (not lastDirection)
        Vector3 input = player.GetDirectionalInput();
        HandleTurn(input);

        // apply inertia to smooth turning
        float inertiaTurnSpeed;
        if (currentThrustPhase == 3) inertiaTurnSpeed = player.runningTurnSpeedPhase3; // less directional control in phase 3
        else inertiaTurnSpeed = player.runningTurnSpeedNormal;

        // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
        // Phase 2: Don't handle movement if on slope (GlidingState handles it)
        // if (player.isOnSlope)
        // {
        //     return;
        // }

        Vector3 toMove = player.ApplyInertia(input, dt, inertiaTurnSpeed);
        toMove.y = 0;


        // move controller
        if (controller.enabled && toMove != Vector3.zero)
        {
            float moveSpeed = actualSpeed + (currentThrustPhase - 1);
            controller.Move(toMove * moveSpeed * dt);
        }

        // pass thrust info to player

    }

    void HandleTurn(Vector3 input)
    {
        Vector3 previousDir = player.lastDirection;
        bool sharpTurn = previousDir.magnitude > 0.01f && Vector3.Angle(previousDir, input) > 90f;
        if (sharpTurn)
        {
            currentThrustPhase = 1;
            player.damageBoost = currentThrustPhase - 1;
            player.animator.speed = 1;
            PlayerAnimations.instance.animator.SetBool("isRunning", false);
        }
        else
        {
            HandleSpeed(input);
        }
    }

    void HandleSpeed(Vector3 input)
    {
        if (actualSpeed < topSpeed[currentThrustPhase - 1]) //if speed isn't bigger than the top speed corresponding to each phase, it will increase.
        {
            actualSpeed += currentAcceleration[currentThrustPhase - 1]; //we add acceleration each frame.
        }
        else if (currentThrustPhase < 3)
        {
            currentThrustPhase++; //we increase the phase
            player.damageBoost = currentThrustPhase - 1; //this will help to management
            staminaCostPerSecond = player.runningStaminaCost[currentThrustPhase - 1]; //we set the stamina cost
            stamina.SetRunning(staminaCostPerSecond);

            CameraFollow.instance.smoothSpeed++; //the camera speed will follow the increase of player's speed
            player.animator.speed += 0.5f; //animation speed will increase too
        }
    }
    public void Exit()
    {
        //we reset the values
        staminaCostPerSecond = player.runningStaminaCost[0];
        CameraFollow.instance.smoothSpeed = 2f;
        player.animator.speed = 1;
    }
} 

