using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    public GameObject stairDiagonal, stairBase;
    private Collider col;
    private void Awake()
    {
        col = GetComponent<BoxCollider>();
        stairDiagonal.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Enter");
            stairDiagonal.SetActive(true);
            stairBase.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Exit");
            stairDiagonal.SetActive(false);
            stairBase.SetActive(true);
        }
    }
}
