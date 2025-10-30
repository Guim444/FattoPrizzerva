using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;      // Assign player in Inspector
    public Transform center;

    [Header("Smoothness")]
    public float smoothSpeed = 5f;

    [Header("Clamps")]
    public float minZ = -20f;
    public float maxZ = 0f;
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Dead Zone Margins")]
    public float marginX = 2f;    // How far player can move left/right before camera moves
    public float marginZ = 2f;    // How far player can move up/down before camera moves

    private Vector3 initialOffset;

    void Start()
    {
        if (player != null)
            initialOffset = transform.position - center.position;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 targetPos = transform.position; // Start with current camera pos
        Vector3 desiredPos = player.position + initialOffset;

        // --- Horizontal check (X axis) ---
        if (Mathf.Abs(desiredPos.x - transform.position.x) > marginX)
            targetPos.x = Mathf.Clamp(desiredPos.x, minX, maxX);

        // --- Vertical check (Z axis) ---
        if (Mathf.Abs(desiredPos.z - transform.position.z) > marginZ)
            targetPos.z = Mathf.Clamp(desiredPos.z, minZ, maxZ);

        // --- Keep Y fixed (camera height) ---
        targetPos.y = transform.position.y;

        // Smoothly move towards target
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}