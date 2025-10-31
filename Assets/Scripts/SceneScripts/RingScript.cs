using System.Collections;
using UnityEngine;

public class RingScript : MonoBehaviour
{
    public float slowMultiplier = 0.1f;
    public float jumpDistance;
    public GameObject ringArea;
    //public bool canExit;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(GetIntoTheRing(other));
        }
    }
    IEnumerator GetIntoTheRing(Collider other)
    {
        Debug.Log("Getting into the ring");
        other.gameObject.GetComponent<CharacterController>().enabled = false;
        gameObject.GetComponent<CapsuleCollider>().enabled = false;
        other.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 6;

        Vector3 direction = CalculateDirectionToTheCenter();
        direction.y = 0f;

        Vector3 desiredPosition = new Vector3(
            other.transform.position.x + direction.x * 2,
            other.transform.position.y,
            other.transform.position.z + direction.z * 2 + jumpDistance
        );
        other.gameObject.transform.position = Vector3.Lerp(other.gameObject.transform.position, desiredPosition, 0.75f);
        ringArea.GetComponent<CapsuleCollider>().enabled = true;
        yield return new WaitForSeconds(0.5f);
        other.gameObject.GetComponent<CharacterController>().enabled = true;
    }


    Vector3 CalculateDirectionToTheCenter()
    {
        //using actual position of the player and the ring center to calculate distance

        Vector3 distance = transform.position - GameObject.FindGameObjectWithTag("Player").transform.position;
        Vector3 direction = distance.normalized;
        return direction;
    }
}
