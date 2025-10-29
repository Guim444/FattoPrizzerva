using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    public GameObject stairDiagonal, stairBase;
    private Collider col;
    private bool isPlayerOnStairs = false;
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
            isPlayerOnStairs = true;
            stairDiagonal.SetActive(true);
            stairBase.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Exit");
            isPlayerOnStairs = false;
            stairDiagonal.SetActive(false);
            stairBase.SetActive(true);
        }
    }
}
