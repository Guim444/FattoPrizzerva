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

        player.animator.speed = 1;
        actualSpeed = baseSpeed;
        staminaCostPerSecond = 10;
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
        if (currentThrustPhase == 3) inertiaTurnSpeed = 2f; // less directional control in phase 3
        else inertiaTurnSpeed = 4;

            Vector3 toMove = player.ApplyInertia(input, dt, inertiaTurnSpeed);
        toMove.y = 0;

        // move controller
        if (controller.enabled)
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
            // Bajar thrust phase
            currentThrustPhase = 1;
            player.damageBoost = currentThrustPhase - 1;
            player.animator.speed -= 0.5f;
            player.animator.speed = 1;
            phaseTime = 0f;
            Debug.Log("Thrust decreased to " + currentThrustPhase);

            // Forzar animación de andar
            PlayerAnimations.instance.animator.SetBool("isRunning", false);
        }
        else
        {
            HandleThrust(input);
        }
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

                        player.animator.speed = 1.5f;

                        break;
                    case 3:
                        actualSpeed = baseSpeed * 1.8f;
                        player.damageBoost = 2;
                        staminaCostPerSecond = 18;
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
        player.animator.speed = 1;
    }
} 

