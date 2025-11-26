
//READ ONLY SCRIPT. DO NOT USE.
using UnityEngine;

public class Notes : MonoBehaviour
{
    public RioTutteScript rioTutteScript;
    private void Start()
    {
        // Fire Dash is temporary fixed, but the solution I found is only provisional.
        // This script will always make RioTutte bounce back if hits a wall, but it's not the ideal solution. We want the enemy to never choose a path where
        // it gets stuck when fire dashing. Quick reference:
        rioTutteScript.FireDashBack();
    }

    //OLD THRUST HANDLER:

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
}