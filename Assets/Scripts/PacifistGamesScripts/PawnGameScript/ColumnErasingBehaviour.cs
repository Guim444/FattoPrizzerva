using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class ColumnErasingBehaviour : MonoBehaviour
{
    public GameObject parent;
    public GameObject squarePrefab;
    private void OnEnable()
    {
        for (int i = 0; i < BoardManager.instance.width; i++)
        {
            Instantiate(squarePrefab, parent.transform);
        }
    }
}
