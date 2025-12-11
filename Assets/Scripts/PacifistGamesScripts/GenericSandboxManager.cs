using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class GenericSandboxManager : MonoBehaviour
{
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
    public void EnableOption(GameObject objectToEnable)
    {
        bool active = !objectToEnable.activeSelf;
        objectToEnable.SetActive(active);

        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = active ? "X" : "";
    }
}
