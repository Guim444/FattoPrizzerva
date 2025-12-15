using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 10f;
    public float inertia = 7f;

    [Header("Look Settings")]
    public Vector2 look;
    public float yaw, pitch;
    public float maxPitch = 80f;
    public float lookSensitivity = 100f;

    [Header("Zoom Settings")]
    public float orthoZoomSpeed = 20f;
    public float inputZoom;

    [Header("References")]
    public CinemachineCamera cam;
    public GameObject camRef;

    public Vector2 inputMove;
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
        if (inputMove == Vector2.zero) return;

        yaw -= inputMove.x * lookSensitivity * Time.deltaTime;
        pitch += inputMove.y * lookSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        Quaternion targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        camRef.transform.rotation = Quaternion.Lerp(camRef.transform.rotation, targetRotation, inertia * Time.deltaTime);

    }
    private void ZoomCam()
    {
        if (cam == null || inputZoom == 0) return;

        var lens = cam.Lens;

        if (lens.Orthographic)
        {
            lens.OrthographicSize -= orthoZoomSpeed * Time.deltaTime * inputZoom;
            lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize, 5f, 50f);
            cam.Lens = lens;
        }
    }
}