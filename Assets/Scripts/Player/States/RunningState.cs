using UnityEngine;

public class RunningState : IStateActions
{

    public PlayerController player;
    public CharacterController controller;
    public float baseSpeed = 6f;
    public float gravity = -9.81f;
    public float staminaCostPerSecond = 10f;

    // Thrust phases
    private int currentThrustPhase = 1;
    public float phaseTime = 0f;
    public float phaseThreshold = 2f;

    private Vector3 velocity;
    private Vector3 lastDirection = Vector3.zero;

    public void Enter()
    {
        player.animator.SetFloat("Speed", 1f); // Set Speed to 1 for running animation
    }

    public void Update()
    {


        // TODO: Movement behavior here

        Vector3 toMove = player.GetDirectionalInput();

        // If no new input, continue last direction
        if (toMove == Vector3.zero && lastDirection != Vector3.zero)
            toMove = lastDirection;

        // Handle thrust progression
        HandleThrust(toMove);

        // Reduced Y axis adaptability in phase 3 and 4
        if (currentThrustPhase >= 3)
        {
            // toMove = ((toMove / player.resistenceToGirRunning) + lastDirection * 2).normalized;
        }

        if (controller.enabled == true) controller.Move(toMove * (baseSpeed + currentThrustPhase) * Time.deltaTime);

        /* Apply gravity
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            velocity.y = -1f;
        }

        lastDirection = toMove;
    */
    }
     
        void HandleThrust(Vector3 currentInput)
        {
            if (currentInput != Vector3.zero && lastDirection != Vector3.zero)
            {
                if (Vector3.Dot(currentInput, lastDirection) > 0.9f)
                {
                    phaseTime += Time.deltaTime;
                    if (phaseTime >= phaseThreshold && currentThrustPhase < 4)
                    {
                        currentThrustPhase++;
                        phaseTime = 0f;
                    }
                }
                else
                {
                    // Changed direction: reset thrust
                    currentThrustPhase = 1;
                    phaseTime = 0f;
                }
            }
        }
    
        public void Exit()
        {
            Debug.Log("Exited RunningState State");
        }
    } 

