using UnityEngine;

public class RingSlopeZone : MonoBehaviour
{
    public Transform ringCenter;

    [Header("Zone Type")]
    public bool isInsideZone = false; // ✅ inside = true, outside = false

    [Header("Debug")]
    public bool verbose = true;

    float timer;
    const float CHECK_RATE = 0.05f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // ✅ only react if player side matches zone side
        if (pc.isInsideRing != isInsideZone) return;

        if (verbose) Debug.Log($"ENTER {name}");
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        // ✅ ignore if wrong side
        if (pc.isInsideRing != isInsideZone) return;

        timer += Time.deltaTime;
        if (timer < CHECK_RATE) return;
        timer = 0f;

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 up = (ringCenter.position - other.transform.position);
        up.y = 0;
        if (up.sqrMagnitude > 0.001f) up.Normalize();

        // inside zone flips uphill
        if (isInsideZone) up = -up;

        float dot = Vector3.Dot(input.normalized, up);
        string state = dot > 0.5f ? "UP" : dot < -0.5f ? "DOWN" : "IDLE";

        if (verbose) Debug.Log($"{name} → {state}");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (verbose) Debug.Log($"EXIT {name}");
    }
}
