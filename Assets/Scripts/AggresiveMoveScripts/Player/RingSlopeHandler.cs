using UnityEngine;

/// <summary>
/// Handles all ring slope movement logic, including teleportation coordinate references
/// Separated from PlayerController for better organization
/// </summary>
public class RingSlopeHandler : MonoBehaviour
{
    [Header("Ring Slope Configuration")]
    public bool isOnSlope = false;
    public Vector3 slopeRadialDirection = Vector3.zero;
    public Transform ringCenter;
    
    // Teleportation reference coordinates (for boundary calculation)
    private RingScript ringScript;
    private RingArena ringArena;
    private Vector3 targetInsidePosition;
    private Vector3 targetOutsidePosition;
    private Vector3 slopeStartPosition;
    
    private PlayerController playerController;
    
    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("RingSlopeHandler requires PlayerController component!");
        }
        
        FindRingScripts();
    }
    
    private bool GetIsInsideRing()
    {
        return playerController != null ? playerController.isInsideRing : false;
    }
    
    /// <summary>
    /// Finds RingScript and RingArena components for teleportation coordinate references
    /// </summary>
    private void FindRingScripts()
    {
        // Try to find by name first (most reliable), then by component type
        GameObject ringColliderObj = GameObject.Find("RingCollider");
        if (ringColliderObj != null)
        {
            ringScript = ringColliderObj.GetComponent<RingScript>();
        }
        
        // If still not found, search by component type
        if (ringScript == null)
        {
            ringArena = Object.FindAnyObjectByType<RingArena>();

        }

        GameObject ringAreaObj = GameObject.Find("RingArea");
        if (ringAreaObj != null)
        {
            ringArena = ringAreaObj.GetComponent<RingArena>();
        }
        
        // If still not found, search by component type
        if (ringArena == null)
        {
            ringScript = Object.FindAnyObjectByType<RingScript>();
            
        }

        // Debug warnings if not found
        if (ringScript == null)
        {
            Debug.LogWarning("RingSlopeHandler: RingScript not found! Ring slope system may not work correctly.");
        }
        if (ringArena == null)
        {
            Debug.LogWarning("RingSlopeHandler: RingArena not found! Ring slope system may not work correctly.");
        }
    }
    
    /// <summary>
    /// Called when player enters a slope zone
    /// </summary>
    public void EnterSlopeZone(Transform center)
    {
        isOnSlope = true;
        ringCenter = center;
        slopeStartPosition = transform.position;
        
        CalculateTeleportationTargets();
    }
    
    /// <summary>
    /// Called when player exits a slope zone
    /// </summary>
    public void ExitSlopeZone()
    {
        isOnSlope = false;
        slopeRadialDirection = Vector3.zero;
        ringCenter = null;
    }
    
    /// <summary>
    /// Calculates target positions using the same logic as teleportation
    /// These define the boundaries where gliding should start/end
    /// </summary>
    private void CalculateTeleportationTargets()
    {
        if (ringCenter == null) return;
        
        Vector3 direction = Vector3.zero;
        
        // Calculate direction to center (same as teleportation logic)
        if (ringScript != null)
        {
            // Use RingScript's transform as reference (the ring collider)
            Vector3 distance = ringScript.transform.position - transform.position;
            direction = distance.normalized;
            direction.y = 0f;
            
            // Calculate target inside position (where teleportation would place player)
            // From RingScript: position + direction * 2 + (0, 0, jumpDistance)
            targetInsidePosition = new Vector3(
                transform.position.x + direction.x * 2,
                transform.position.y,
                transform.position.z + direction.z * 2 + ringScript.jumpDistance
            );
        }
        
        if (ringArena != null && ringScript != null)
        {
            // Calculate target outside position (where teleportation would place player when exiting)
            // From RingArena: position - direction * 3 - (0, 0, direction.z + jumpDistance)
            targetOutsidePosition = new Vector3(
                transform.position.x - direction.x * 3,
                transform.position.y,
                transform.position.z - (direction.z + ringScript.jumpDistance)
            );
        }
    }
    
    /// <summary>
    /// Calculates the radial direction (uphill/downhill) based on player position and ring center
    /// </summary>
    public Vector3 GetSlopeRadialDirection()
    {
        if (!isOnSlope || ringCenter == null) return Vector3.zero;

        Vector3 toCenter = ringCenter.position - transform.position;
        toCenter.y = 0; // Keep it horizontal
        if (toCenter.sqrMagnitude > 0.001f)
        {
            toCenter.Normalize();
        }

        // If inside ring, uphill is AWAY from center (going out)
        // If outside ring, uphill is TOWARD center (going in)
        if (GetIsInsideRing())
        {
            return -toCenter; // Uphill = outward
        }
        else
        {
            return toCenter; // Uphill = inward
        }
    }
    
    /// <summary>
    /// Restricts movement to radial direction when on slope
    /// Projects input onto the slope's radial axis (uphill/downhill)
    /// Constrains movement to teleportation target boundaries
    /// No automatic sliding - player stops when no input
    /// Returns the restricted direction vector (not including speed/deltaTime)
    /// </summary>
    public Vector3 RestrictToSlopeDirection(Vector3 inputDirection)
    {
        if (!isOnSlope || ringCenter == null)
        {
            // No slope: return input as-is
            return inputDirection;
        }

        // Handle zero input - player should stop
        if (inputDirection.magnitude < 0.01f)
        {
            return Vector3.zero;
        }

        // Calculate current radial direction (uphill/downhill)
        slopeRadialDirection = GetSlopeRadialDirection();
        if (slopeRadialDirection.magnitude < 0.01f)
        {
            return inputDirection; // Safety check
        }

        // Project input onto radial direction (clamp movement to slope axis)
        // This restricts sideways movement along the rim
        Vector3 normalizedInput = inputDirection.normalized;
        float inputDot = Vector3.Dot(normalizedInput, slopeRadialDirection);
        
        // Only move if there's meaningful input in the radial direction
        if (Mathf.Abs(inputDot) < 0.01f)
        {
            return Vector3.zero; // Input is perpendicular to slope, stop
        }
        
        // Project onto radial direction, maintaining the original input magnitude
        Vector3 radialInput = slopeRadialDirection * inputDot * inputDirection.magnitude;
        
        // Constrain movement to teleportation target boundaries
        radialInput = ConstrainToTeleportationBoundaries(radialInput);

        return radialInput;
    }
    
    /// <summary>
    /// Constrains movement to stay within teleportation target boundaries
    /// Prevents player from gliding beyond where teleportation would have placed them
    /// </summary>
    private Vector3 ConstrainToTeleportationBoundaries(Vector3 movement)
    {
        if (ringCenter == null || slopeRadialDirection.magnitude < 0.01f) return movement;
        
        Vector3 currentPos = transform.position;
        Vector3 projectedPos = currentPos + movement;
        
        // Determine which boundary to check based on inside/outside ring
        bool insideRing = GetIsInsideRing();
        Vector3 targetBoundary = insideRing ? targetOutsidePosition : targetInsidePosition;
        
        if (targetBoundary == Vector3.zero)
        {
            // Targets not calculated yet, recalculate
            CalculateTeleportationTargets();
            targetBoundary = insideRing ? targetOutsidePosition : targetInsidePosition;
        }
        
        if (targetBoundary == Vector3.zero) return movement; // Still can't calculate, allow movement
        
        // Calculate distances along radial direction from start position
        Vector3 toBoundary = targetBoundary - slopeStartPosition;
        Vector3 toCurrent = currentPos - slopeStartPosition;
        Vector3 toProjected = projectedPos - slopeStartPosition;
        
        // Project distances onto radial direction
        float boundaryDistance = Vector3.Dot(toBoundary, slopeRadialDirection);
        float currentDistance = Vector3.Dot(toCurrent, slopeRadialDirection);
        float projectedDistance = Vector3.Dot(toProjected, slopeRadialDirection);
        
        // Determine movement direction along radial axis
        float movementDistance = projectedDistance - currentDistance;
        
        // Clamp to boundary: if moving toward boundary and would exceed it, limit movement
        if (boundaryDistance > 0)
        {
            // Boundary is in positive direction
            if (projectedDistance > boundaryDistance)
            {
                // Would exceed boundary, clamp to boundary
                movementDistance = boundaryDistance - currentDistance;
            }
        }
        else if (boundaryDistance < 0)
        {
            // Boundary is in negative direction
            if (projectedDistance < boundaryDistance)
            {
                // Would exceed boundary, clamp to boundary
                movementDistance = boundaryDistance - currentDistance;
            }
        }
        
        // If movement would go beyond boundary, stop
        if (Mathf.Abs(movementDistance) < 0.01f && Mathf.Abs(currentDistance - boundaryDistance) < 0.1f)
        {
            return Vector3.zero; // At boundary, stop
        }
        
        return slopeRadialDirection * movementDistance;
    }
}

