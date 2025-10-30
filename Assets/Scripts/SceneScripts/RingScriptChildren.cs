using UnityEngine;

public class RingChildTrigger : MonoBehaviour
{
    private RingScript parentZone;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (parentZone == null)
            {
                parentZone = GetComponentInParent<RingScript>();
            }
            if (parentZone != null)
            {
                parentZone.OnChildTriggerEnter(gameObject.name);
            }
        }
    }
}