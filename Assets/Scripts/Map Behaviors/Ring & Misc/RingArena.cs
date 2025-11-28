using System.Collections;
using UnityEngine;

public class RingArena : MonoBehaviour
{
    public GameObject ringCollider;
    
    private bool isTeleporting = false; // Prevent multiple simultaneous teleportations
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isInsideRing = false;
                
                // TEMPORARY: Always use teleportation for now
                // TODO: Once teleportation is confirmed working from all angles, 
                // transition to gliding system by uncommenting the conditional check below
                // and commenting out the direct call
                
                // CRITICAL: Disable CharacterController IMMEDIATELY to prevent movement during teleport
                CharacterController cc = other.gameObject.GetComponent<CharacterController>();
                if (cc != null && cc.enabled)
                {
                    cc.enabled = false;
                }
                
                isTeleporting = true;
                StartCoroutine(GetOffTheRing(other));
                
                // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
                // Phase 2: If slope system is not active, use teleportation as fallback
                // if (pc.ringSlopeHandler == null || !pc.isOnSlope)
                // {
                //     StartCoroutine(GetOffTheRing(other));
                // }
            }
        }
    }
    IEnumerator GetOffTheRing(Collider other)
    {
        other.GetComponent<PlayerController>().isInsideRing = false;

        Debug.Log("Getting off the ring");
        
        // CharacterController already disabled in OnTriggerExit for immediate stop
        // other.gameObject.GetComponent<CharacterController>().enabled = false;
        
        GetComponent<CapsuleCollider>().enabled = false;
        other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        Vector3 direction = CalculateDirectionToTheCenter();

        direction.y = 0f;

        Vector3 desiredPosition = new Vector3(
            other.transform.position.x - direction.x * 3,
            other.transform.position.y,
            other.transform.position.z - (direction.z + ringCollider.GetComponent<RingScript>().jumpDistance)
        );

        // Set position immediately instead of Lerp (Lerp with single value doesn't animate)
        other.gameObject.transform.position = desiredPosition;
        
        ringCollider.GetComponent<CapsuleCollider>().enabled = true;
        yield return new WaitForSeconds(0.5f);
        other.gameObject.GetComponent<CharacterController>().enabled = true;
        isTeleporting = false; // Allow teleportation again
    }

    Vector3 CalculateDirectionToTheCenter()
    {
        Vector3 distance = transform.position - GameObject.FindGameObjectWithTag("Player").transform.position;
        return distance.normalized;
    }
}
