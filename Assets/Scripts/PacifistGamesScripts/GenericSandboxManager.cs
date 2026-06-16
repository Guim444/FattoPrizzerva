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

        text.text = Modify(text, num, 60, 0, true);
    }
    public void ModifyBoardSize(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();
        text.text = Modify(text, num, 12, 4, false);
    }

    public void ModifyStormStartTurns(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();
        text.text = Modify(text, num, 10, 4, false);
    }
    public void ModifyStormExpandTurns(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();
        text.text = Modify(text, num, 5, 2, false);
    }

    public string Modify(TextMeshProUGUI text, int num, int max, int min, bool addZero)
    {
        if (text != null)
        {
            if (int.TryParse(text.text, out int currentValue))
            {
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, min - 1, max);

                if (currentValue == max)
                    currentValue = min;
                else if (currentValue == min - 1)
                    currentValue = max - Mathf.Abs(num);

                if (addZero)
                    text.text = currentValue.ToString("00");
                else
                    text.text = currentValue.ToString();

            }
        }
        Debug.Log(text.text);

        return text.text;
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
            if (int.TryParse(text.text, out int currentValue))
            {
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, -1, 10);

                if (currentValue == 10)
                    currentValue = 0;
                else if (currentValue == -1)
                    currentValue = 9;

                text.text = currentValue.ToString();
            }
        }
    }
    public void PawnCustomMoveSelect(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();
        if (text != null)
        {
            if (int.TryParse(text.text, out int currentValue))
            {
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, 0, 4);

                if (currentValue == 4)
                    currentValue = 1;
                else if (currentValue == 0)
                    currentValue = 3;

                text.text = currentValue.ToString();
            }
        }
    }
    public void PawnCustomStartingMoveSelect(int num)
    {
        GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        TextMeshProUGUI text = sender.GetComponentInParent<TextMeshProUGUI>();
        if (text != null)
        {
            if (int.TryParse(text.text, out int currentValue))
            {
                currentValue += num;
                currentValue = Mathf.Clamp(currentValue, 0, 5);

                if (currentValue == 5)
                    currentValue = 1;
                else if (currentValue == 0)
                    currentValue = 4;

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
        List<GameObject> list = PawnsGameManager.instance.activePlayer == 1? PawnsGameManager.instance.erasedColumnsPlayer2 : PawnsGameManager.instance.erasedColumnsPlayer1;

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
        if (PawnsGameManager.instance.erasedColumnsPlayer2.Count == 2) // You select the other's columns to erase, so this is the right way
        {
            PawnsGameManager.instance.activePlayer = PawnsGameManager.instance.activePlayer == 1 ? 2 : 1;
            int activePlayer = PawnsGameManager.instance.activePlayer;
            startButton.SetActive(true);
            ColumnErasingBehaviour.instance.ResetSprites();
            GameObject sender = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            GameObject msg = sender.transform.parent.Find("MSG").gameObject;

            if (int.TryParse(PawnsGameManager.instance.columnEliminationOffset.text, out int offset))
                PawnsGameManager.instance.offsetPlayer1 = offset;
            PawnsGameManager.instance.columnEliminationOffset.text = "0";

            if (msg != null)
                msg.GetComponent<TextMeshProUGUI>().text = "Player 2's turn to erase:\r\n\r\n\r\n\r\n\r\n\r\nPlayer 1, please turn around in order to not see the selection.";
            sender.SetActive(false);
        }
    }
    public void SetBoardData()
    {
        if (PawnsGameManager.instance.erasedColumnsPlayer1.Count == 2)
        {
            if (int.TryParse(PawnsGameManager.instance.columnEliminationOffset.text, out int offset))
                PawnsGameManager.instance.offsetPlayer2 = offset;
            ColumnErasingOffset();
            PawnsGameManager.instance.activePlayer = 1;
            PawnsGameManager.instance.StartGame();
            PawnsGameManager.instance.boardGenerationCanvas.SetActive(false);
        }
    }
    public void GoToCustomPawnMenu()
    {
        PawnsGameManager.instance.pawnConfigCanvas.SetActive(true);
        PawnsGameManager.instance.selectionCanvas.SetActive(false);
    }
    public void ColumnErasingOffset()
    {
        int boardWidth = (int)PawnBoardManager.instance.width;

        for (int i = 0; i < 2; i++)
        {
            List<GameObject> list = i == 0 ? PawnsGameManager.instance.erasedColumnsPlayer1 : PawnsGameManager.instance.erasedColumnsPlayer2;
            int offset = i == 0 ? PawnsGameManager.instance.offsetPlayer1 : PawnsGameManager.instance.offsetPlayer2;

            for (int j = 0; j < list.Count; j++)
            {
                char colName = char.Parse(list[j].name);
                int colIndex = colName - 'A' + 1;
                colIndex = ((colIndex - 1 + offset) % boardWidth + boardWidth) % boardWidth + 1;
                char newColName = (char)('A' + colIndex - 1);
                list[j].name = newColName.ToString();
            }
        }
    }
}
