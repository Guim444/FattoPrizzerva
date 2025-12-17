using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class ColumnErasingBehaviour : MonoBehaviour
{
    public GameObject parent;
    public GameObject squarePrefab;
    public static ColumnErasingBehaviour instance;
    private void OnEnable()
    {
        if (instance == null)
            instance = this;

        for (int i = 0; i < BoardManager.instance.width; i++)
        {
            GameObject createdSquare = Instantiate(squarePrefab, parent.transform);
            createdSquare.name = ((char)('A' + i)).ToString();
        }
    }
    public void ResetSprites()
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.GetComponent<Image>().color = Color.white;
        }
    }
}
