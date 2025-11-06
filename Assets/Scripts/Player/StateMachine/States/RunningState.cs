using UnityEngine;

public class RunningState : IStateActions
{

    public PlayerController player;
    public CharacterController controller;
    public float baseSpeed = 6f, actualSpeed;
    public float staminaCostPerSecond;

    // Thrust phases
    private int currentThrustPhase = 1;
    public float phaseTime = 0f;
    public float phaseThreshold = 1.5f;

    public PlayerStaminaManager stamina;

    public float turnSpeed, dt;

    public RunningState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    public void Enter()
    {
        currentThrustPhase = 1; // Reset to phase 1 on entering running state

        player.animator.SetFloat("Speed", 1f); // Set Speed to 1 for running animation
        actualSpeed = baseSpeed;
        staminaCostPerSecond = 10;
        stamina.SetRunning(staminaCostPerSecond);
    }

    public void Update()
    {
        // TODO: Movement behavior here

        dt = Time.deltaTime;

        Vector3 input = player.GetDirectionalInput();
        Vector3 toMove = player.ApplyInertia(input, Time.deltaTime, 4f);

        if (toMove.magnitude > 0.01f)
        {
            toMove.Normalize();
        }

        // Handle thrust progression
        HandleThrust(toMove);

        // Reduced Y axis adaptability in phase 3 and 4
        if (currentThrustPhase == 3)
        {
            input = Sprint();
            toMove = player.ApplyInertia(input, Time.deltaTime, 2f);
        }

        if (controller.enabled == true) controller.Move(toMove * (actualSpeed + currentThrustPhase) * Time.deltaTime);

        player.damageBoost = currentThrustPhase - 1;
    }

    void HandleThrust(Vector3 currentInput)
    {
        if (currentInput != Vector3.zero)
        {
            if (!controller.enabled) return;
            phaseTime += Time.deltaTime;
            if (phaseTime >= phaseThreshold && currentThrustPhase < 3)
            {
                currentThrustPhase++;
                Debug.Log("Thrust Phase Increased to: " + currentThrustPhase);

                switch (currentThrustPhase)
                {
                    case 2:
                        actualSpeed = baseSpeed * 1.4f;
                        player.damageBoost = 1;
                        staminaCostPerSecond = 14;
                        stamina.SetRunning(staminaCostPerSecond);
                        CameraFollow.instance.smoothSpeed = 3f;
                        break;
                    case 3:
                        actualSpeed = baseSpeed * 1.8f;
                        player.damageBoost = 2;
                        staminaCostPerSecond = 18;
                        stamina.SetRunning(staminaCostPerSecond);
                        CameraFollow.instance.smoothSpeed = 4f;
                        break;
                    default:
                        CameraFollow.instance.smoothSpeed = 2f;
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
    }
    Vector3 Sprint()
    {
        //only runs in X in this state
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical") / 3;

        Vector3 sprintDirection = new Vector3(moveX, 0, moveZ).normalized;

        return sprintDirection;
    }
    public void Exit()
    {
        Debug.Log("Exited RunningState State");
        CameraFollow.instance.smoothSpeed = 2f;
        //to do: apply inertia
    }
} 

