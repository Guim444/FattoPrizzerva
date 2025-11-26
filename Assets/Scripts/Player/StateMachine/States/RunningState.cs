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
    public float phaseTime = 0f;
    public float phaseThreshold = 1.5f; // Will be overridden by player.thrustPhaseThreshold

    public PlayerStaminaManager stamina;

    public float turnSpeed, dt;

    public float[] topSpeed = { 9, 11f, 13.5f};
    public float[] currentAcceleration = { 0.0035f, 0.0045f, 0.006f };

    public TextMeshProUGUI speedControlText;

    public RunningState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    public void Enter()
    {
        GameObject speedControl = GameObject.Find("Speed");

        speedControlText = speedControl.GetComponent<TextMeshProUGUI>();

        currentThrustPhase = 1; // Reset to phase 1 on entering running state
        CameraFollow.instance.smoothSpeed = 2;

        player.animator.speed = 1;
        baseSpeed = player.runningBaseSpeed; // Use player's tuning value
        actualSpeed = baseSpeed;
        phaseThreshold = player.thrustPhaseThreshold; // Use player's tuning value
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
            //phaseTime = 0f;
            PlayerAnimations.instance.animator.SetBool("isRunning", false);
        }
        else
        {
            //HandleThrust(input);
            HandleSpeed(input);
        }
    }

    void HandleSpeed(Vector3 input)
    {
        if (actualSpeed < topSpeed[currentThrustPhase - 1])
        {
            actualSpeed += currentAcceleration[currentThrustPhase - 1];
        }
        else if (currentThrustPhase < 3)
        {
            currentThrustPhase++;
            player.damageBoost = currentThrustPhase - 1;
            staminaCostPerSecond = player.runningStaminaCost[currentThrustPhase - 1];
            stamina.SetRunning(staminaCostPerSecond);

            CameraFollow.instance.smoothSpeed++;
            player.animator.speed += 0.5f;
            Debug.Log(actualSpeed);
        }
    }

    /*void HandleThrust(Vector3 currentInput)
    {
        if (currentInput != Vector3.zero)
        {
            if (!controller.enabled) return;
            phaseTime += Time.deltaTime;
            if (phaseTime >= phaseThreshold && currentThrustPhase < 3)
            {
                currentThrustPhase++;

                switch (currentThrustPhase)
                {
                    case 2:
                        actualSpeed = baseSpeed * player.runningPhase2Multiplier; // Use player's tuning value
                        player.damageBoost = 1;
                        staminaCostPerSecond = player.runningStaminaCost[1]; // Use player's tuning value
                        stamina.SetRunning(staminaCostPerSecond);
                        CameraFollow.instance.smoothSpeed = 3f;

                        player.animator.speed = 1.5f;

                        break;
                    case 3:
                        actualSpeed = baseSpeed * player.runningPhase3Multiplier; // Use player's tuning value
                        player.damageBoost = 2;
                        staminaCostPerSecond = player.runningStaminaCost[2]; // Use player's tuning value
                        stamina.SetRunning(staminaCostPerSecond);

                        player.animator.speed = 2;

                        CameraFollow.instance.smoothSpeed = 4f;
                        break;
                    default:
                        CameraFollow.instance.smoothSpeed = 2f;

                        player.animator.speed = 1;

                        break;
                }
                phaseTime = 0f;
            }
        }
        else
        {
            currentThrustPhase = 1;
            phaseTime = 0f;
        }
    }*/
    public void Exit()
    {
        CameraFollow.instance.smoothSpeed = 2f;
        player.animator.speed = 1;

        speedControlText.text = "0";
    }
} 

