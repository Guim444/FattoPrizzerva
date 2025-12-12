using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GenericSandboxManager : MonoBehaviour
{
    public TextMeshProUGUI maxColumn;
    public void ModifyNum(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();

        if (text != null)
        {
            if (int.TryParse(text.text, out int currentValue))
            {
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, -1, 60);

                if (currentValue == 60)
                    currentValue = 0;
                else if (currentValue == -1)
                    currentValue = 60 - Mathf.Abs(num);

                text.text = currentValue.ToString("00");
            }
        }
    }
    public void ColumnSelect(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();

        if (text != null)
        {
            if (int.TryParse(text.text, out int currentValue))
            {
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, 2, 11);

                if (currentValue == 11)
                    currentValue = 3;
                else if (currentValue == 3)
                    currentValue = 11 - Mathf.Abs(num);

                text.text = currentValue.ToString();
            }
        }
    }
    public void ColumnElimination(int num)
    {
        //POR AQUI ME QUEDÉ
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();
        if (text != null)
        {
            if (int.TryParse(text.text, out int currentValue) && int.TryParse(maxColumn.text, out int maxCol))
            {
                Debug.Log(maxCol);
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, 0, maxCol + 1);

                if (currentValue == maxCol + 1)
                    currentValue = 1;
                else if (currentValue == 0)
                    currentValue = maxCol;

                text.text = currentValue.ToString();
            }
        }
    }
    public void EnableOption(GameObject objectToEnable)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInChildren<TextMeshProUGUI>();
        bool active;
        if (text != null)
        {
            active = text.text == "X";
            objectToEnable.SetActive(!active);
            text.text = active ? "" : "X"; //Toggle
        }
    }
}
