using UnityEngine;

/// <summary>
/// Helper class for determining player state based on input and conditions
/// Separated from PlayerController for better organization
/// </summary>
public static class PlayerStateHelper
{
    /// <summary>
    /// Determines the appropriate state based on input and player conditions
    /// </summary>
    public static State DetermineState(PlayerStaminaManager staminaManager, bool isOnSlope, State currentState)
    {
        // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
        // Don't change state if on slope (GlidingState handles it)
        // if (isOnSlope && currentState == State.Gliding)
        // {
        //     return State.Gliding;
        // }
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            // Movement input detected
            if (staminaManager.isTired)
            {
                return State.Tired;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && staminaManager.currentStamina > 0)
            {
                // Running input detected and enough stamina
                // Only transition to PunchRunning on mouse button DOWN, not while held
                // This allows Running state to progress through thrust phases even if mouse is held
                if (Input.GetMouseButtonDown(0))
                {
                    return State.PunchRunning; // Punching while running (only on press)
                }
                return State.Running;
            }
            else
            {
                // Normal movement
                if (Input.GetMouseButtonDown(0))
                {
                    return State.Punching; // Punching while moving
                }
                return State.Moving;
            }
        }
        else
        {
            // No movement input
            if (staminaManager.isTired)
            {
                return State.Tired;
            }
            else
            {
                if (Input.GetMouseButton(0))
                {
                    return State.Punching; // Punching while idle
                }
                return State.Idle;
            }
        }
    }
    
    /// <summary>
    /// Determines the correct state when exiting slope
    /// </summary>
    public static State DetermineStateFromInput(PlayerStaminaManager staminaManager)
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (staminaManager.isTired)
            {
                return State.Tired;
            }
            else if (Input.GetKey(KeyCode.LeftShift) && staminaManager.currentStamina > 0)
            {
                return State.Running;
            }
            else
            {
                return State.Moving;
            }
        }
        else
        {
            if (staminaManager.isTired)
            {
                return State.Tired;
            }
            else
            {
                return State.Idle;
            }
        }
    }
}

