

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
}