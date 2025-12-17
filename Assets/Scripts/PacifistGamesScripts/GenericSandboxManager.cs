using System.Collections.Generic;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

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
                    currentValue = 4;
                else if (currentValue == 3)
                    currentValue = 11 - Mathf.Abs(num);

                text.text = currentValue.ToString();
            }
        }
    }
    public void ColumnElimination(int num)
    {
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
        UnityEngine.UI.Toggle toggle = sender.GetComponent<UnityEngine.UI.Toggle>();

        bool active = toggle.isOn;

        if (objectToEnable != null)
            objectToEnable.SetActive(active);
    }

    public void ToggleButtonForColumnErasing()
    {
        List<GameObject> list = PawnsGameManager.instance.activePlayer == 1? PawnsGameManager.instance.erasedColumnsPlayer1 : PawnsGameManager.instance.erasedColumnsPlayer2;

        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        bool active = list.Contains(sender);

        if (active)
        {
            list.Remove(sender);
            sender.GetComponent<Image>().color = Color.white;
        }
        else
        {
            if (list.Count == 2)
            {
                list[0].GetComponent<Image>().color= Color.white;
                list.RemoveAt(0);
            }
            list.Add(sender);
            sender.GetComponent<Image>().color = Color.green;
        }
    }
    public void NextPlayer(GameObject startButton)
    {
        if (PawnsGameManager.instance.erasedColumnsPlayer1.Count == 2)
        {
            PawnsGameManager.instance.activePlayer = PawnsGameManager.instance.activePlayer == 1 ? 2 : 1;
            startButton.SetActive(true);
            ColumnErasingBehaviour.instance.ResetSprites();
            GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            GameObject msg = sender.transform.parent.Find("MSG").gameObject;
            if (msg != null)
                msg.GetComponent<TextMeshProUGUI>().text = "Player 2's turn to erase:\r\n\r\n\r\n\r\n\r\n\r\nPlayer 1, please turn around in order to not see the selection.";
            sender.SetActive(false);
        }
    }
    public void StartGame()
    {
        if (PawnsGameManager.instance.erasedColumnsPlayer2.Count == 2)
        {
            PawnsGameManager.instance.activePlayer = 1;
            PawnsGameManager.instance.StartGame();
        }
    }
}
