using TMPro;
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

        for (int i = 0; i < PawnBoardManager.instance.width; i++)
        {
            string character = ((char)('A' + i)).ToString();
            GameObject createdSquare = Instantiate(squarePrefab, parent.transform);
            createdSquare.name = character;
            TextMeshProUGUI text = createdSquare.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = character;
            }
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
