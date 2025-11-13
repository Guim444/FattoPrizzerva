using System.Collections;
using UnityEngine;

public class RingScript : MonoBehaviour
{
    public float slowMultiplier = 0.1f;
    public float jumpDistance;
    public GameObject ringArea;
    //public bool canExit;
    
    private bool isTeleporting = false; // Prevent multiple simultaneous teleportations
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.isInsideRing = true;
                
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
                StartCoroutine(GetIntoTheRing(other));
                
                // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
                // Phase 2: If slope system is not active, use teleportation as fallback
                // if (pc.ringSlopeHandler == null || !pc.isOnSlope)
                // {
                //     StartCoroutine(GetIntoTheRing(other));
                // }
            }
        }
    }
    IEnumerator GetIntoTheRing(Collider other)
    {
        Debug.Log("Getting into the ring");
        
        // CharacterController already disabled in OnTriggerEnter for immediate stop
        // other.gameObject.GetComponent<CharacterController>().enabled = false;
        
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 6;

        Vector3 direction = CalculateDirectionToTheCenter();
        direction.y = 0f;

        Vector3 desiredPosition = new Vector3(
            other.transform.position.x + direction.x * 2,
            other.transform.position.y,
            other.transform.position.z + direction.z * 2 + jumpDistance
        );
        
        // Set position immediately instead of Lerp (Lerp with single value doesn't animate)
        other.gameObject.transform.position = desiredPosition;
        
        ringArea.GetComponent<CapsuleCollider>().enabled = true;
        yield return new WaitForSeconds(0.5f);
        other.gameObject.GetComponent<CharacterController>().enabled = true;
        isTeleporting = false; // Allow teleportation again
    }


    Vector3 CalculateDirectionToTheCenter()
    {
        //using actual position of the player and the ring center to calculate distance

        Vector3 distance = transform.position - GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector3 direction = distance.normalized;
        return direction;
    }
}
