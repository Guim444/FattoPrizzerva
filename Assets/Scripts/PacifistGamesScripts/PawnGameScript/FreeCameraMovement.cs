using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCameraMovement : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float inertiaSlower = 5f;   // Qué tan rápido se frena (más alto = se detiene antes)
    private Vector2 input;
    private Vector3 velocity;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    public void OnFreeCamMove(InputValue value)
    {
        input = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity = transform.right * input.x * moveSpeed + transform.up * input.y * moveSpeed;

        CameraInertia(targetVelocity);

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void CameraInertia(Vector3 targetVelocity)
    {
        velocity = Vector3.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * inertiaSlower);
    }
}