using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class FreeCameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 7f;

    [Header("Look Settings")]
    public Vector2 look;
    public float yaw, pitch;
    public float maxPitch = 80f;
    public float lookSensitivity = 100f;

    [Header("Zoom Settings")]
    public float inputZoom;

    [Header("References")]
    public CinemachineCamera cam;
    public GameObject camRef;

    public Vector2 inputMove;

    private void Awake()
    {
        yaw = transform.rotation.eulerAngles.y;
        pitch = transform.rotation.eulerAngles.x;
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
        if (inputMove == Vector2.zero) return;

        yaw -= inputMove.x * lookSensitivity * Time.deltaTime;
        pitch += inputMove.y * lookSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        camRef.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
    private void ZoomCam()
    {
        if (inputZoom == 0) return;

        var thirdPerson = cam.GetComponent<CinemachineThirdPersonFollow>();
        thirdPerson.CameraDistance = Mathf.Clamp(thirdPerson.CameraDistance - inputZoom * speed * 25 * Time.deltaTime, PawnBoardManager.instance.height + 5, 40f);
        inputZoom = 0;
    }
}