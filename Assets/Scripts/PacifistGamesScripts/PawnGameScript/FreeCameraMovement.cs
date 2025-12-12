using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCameraMovement : MonoBehaviour
{
    public float speed = 10;
    public float inertia = 7;

    public float orthoZoomSpeed = 20;

    public Vector3 focusPoint = new Vector3(-2.5f, 0, 7);

    public CinemachineCamera cam;

    public Vector2 inputMove;
    public float inputZoom;

    private Vector2 smoothVelocity;
    private float smoothZoom;

    void Awake()
    {
        if (cam == null) cam = GetComponent<CinemachineCamera>();

        transform.localPosition = new Vector3(-2.5f, 50, 7);
        transform.localRotation = Quaternion.Euler(90, 0, 0);

        smoothVelocity = Vector2.zero;
        smoothZoom = 0f;
    }

    public void OnFreeCamMove(InputValue value)
    {
        inputMove = value.Get<Vector2>();
    }

    public void OnZoom(InputValue value)
    {
        Vector2 scroll = Mouse.current.scroll.ReadValue();
        inputZoom = scroll.y > 0 ? 1 : scroll.y < 0 ? -1 : 0;
    }

    void LateUpdate()
    {
        MoveCam();
        ZoomCam();
    }

    private void MoveCam()
    {
        smoothVelocity = Vector2.Lerp(smoothVelocity, inputMove, Time.deltaTime * inertia);

        if (smoothVelocity != Vector2.zero)
        {
            Vector3 move = new Vector3(-smoothVelocity.x, 0f, -smoothVelocity.y) * speed * Time.deltaTime;
            transform.localPosition += move;
        }
    }

    private void ZoomCam()
    {
        if (cam == null || inputZoom == 0) return;

        smoothZoom = Mathf.Lerp(smoothZoom, inputZoom, Time.deltaTime * inertia);

        var lens = cam.Lens;

        if (lens.Orthographic)
        {
            lens.OrthographicSize -= smoothZoom * orthoZoomSpeed * Time.deltaTime;
            lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize, 5, 50);
            cam.Lens = lens;
        }
    }
}