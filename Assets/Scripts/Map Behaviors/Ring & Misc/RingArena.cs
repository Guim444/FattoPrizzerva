using System.Collections;
using UnityEngine;

public class RingArena : MonoBehaviour
{
    public GameObject ringCollider;
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
    }
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exited Ring");
        //TP it where it's facing + some coords extra in Z
        if (other.CompareTag("Player"))
        {
            StartCoroutine(GetOffTheRing(other));
        }
    }
    IEnumerator GetOffTheRing(Collider other)
    {
        Debug.Log("Getting off the ring");
        other.gameObject.GetComponent<CharacterController>().enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
        other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
        Vector3 direction = CalculateDirectionToTheCenter();

        direction.y = 0f;

        Vector3 desiredPosition = new Vector3(
            other.transform.position.x - direction.x * 3,
            other.transform.position.y,
            other.transform.position.z - (direction.z + ringCollider.GetComponent<RingScript>().jumpDistance)
        );

        other.gameObject.transform.position = Vector3.Lerp(other.gameObject.transform.position, desiredPosition, 0.75f);
        ringCollider.GetComponent<CapsuleCollider>().enabled = true;
        yield return new WaitForSeconds(0.5f);
        other.gameObject.GetComponent<CharacterController>().enabled = true;
    }

    Vector3 CalculateDirectionToTheCenter()
    {
        Vector3 distance = transform.position - GameObject.FindGameObjectWithTag("Player").transform.position;
        return distance.normalized;
    }
}
