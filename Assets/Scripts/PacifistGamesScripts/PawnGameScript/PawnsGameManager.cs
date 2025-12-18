using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PawnsGameManager : MonoBehaviour
{
    public bool gameStarted = false;
    public static PawnsGameManager instance;
    public List<GameObject> cameras = new List<GameObject>();
    public int activePlayer = 1; // 1 = Player 1, -1 = Player 2

    public List<int> playerTier = new List<int>() { 1, 1 };
    public List<int> playerPoints = new List<int>() { 0, 0 };
    public List<bool> waitingRowIsReady = new List<bool>() { false, false };

    public GameObject dataCanvas;
    public GameObject selectionCanvas;
    public GameObject boardGenerationCanvas;
    public GameObject pawnConfigCanvas;

    public List<int> pointsToNextTier = new List<int>();

    public TextMeshProUGUI playerTurn;
    public TextMeshProUGUI timer;

    public List<GameObject> playerInfo;

    public List<float> playerTimer = new List<float>();
    public int extraTimeAddedPerTurn;
    public PawnSets pawnRuleset;

    [Header("Settings")]
    public List<TextMeshProUGUI> timerSettings = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> tierPointsSettings = new List<TextMeshProUGUI>();
    public List<TextMeshProUGUI> boardSizeSettings = new List<TextMeshProUGUI>();
    public TextMeshProUGUI pawnSettings;
    public GameObject columnErasingEnabled;
    public List<GameObject> erasedColumnsPlayer1 = new List<GameObject>();
    public List<GameObject> erasedColumnsPlayer2 = new List<GameObject>();
    public TextMeshProUGUI columnEliminationOffset;
    public int offsetPlayer1, offsetPlayer2;
    bool timerActive;
    public CustomSetSO customRuleset;
    public List<TextMeshProUGUI> customRulesetSettings = new List<TextMeshProUGUI>();
    public List<GameObject> customRulesetKillRangeSettings = new List<GameObject>();

    public bool freeCamActive = false;

    private void Awake()
    {
        instance = this;
        cameras[0].SetActive(true);
        cameras[1].SetActive(false);
        cameras[2].SetActive(false);

        CheckIfCustomRulesetExists();
    }
    public void SetStartValues()
    {
        dataCanvas.SetActive(true);
        SetPlayerTimer();
        SetTierPoints();
        UpdateTierTexts();
        SetSize();
        SetPawnRuleset();
        selectionCanvas.SetActive(false);
        dataCanvas.SetActive(false);

        if (ColumnErasingCheck()) //if it's not needed to activate that canvas, just go to the game.
            boardGenerationCanvas.SetActive(true);
        else
            StartGame();
    }
    public void StartGame()
    {
        dataCanvas.SetActive(true);
        gameStarted = true;

        BoardManager.instance.GenerateBoard();
    }
    private void Update()
    {
        if (gameStarted && timerActive)
        {
            playerTimer[activePlayer - 1] -= Time.deltaTime;
            UpdatePlayerTimer();
        }
    }

    public void NextPlayerTurn()
    {
        activePlayer = activePlayer == 1 ? 2 : 1;
        if (timerActive) UpdatePlayerTimer();

        if (!CheckWinCondition(activePlayer))
        {
            playerTurn.text = "Player " + activePlayer + " Turn";
            if (waitingRowIsReady[activePlayer - 1])
            {
                StartCoroutine(BoardManager.instance.PushWaitingRowToBoard(activePlayer));
                waitingRowIsReady[activePlayer - 1] = false;
                UpdateTierTexts();
            }
            else
            {
                if (activePlayer == 2)
                {
                    foreach (PawnBehavior pawn in BoardManager.instance.whitePawns)
                    {
                        pawn.gameObject.GetComponent<BoxCollider>().enabled = false;
                    }
                    foreach (PawnBehavior pawn in BoardManager.instance.blackPawns)
                    {
                        pawn.gameObject.GetComponent<BoxCollider>().enabled = true;
                    }
                }
                else
                {
                    foreach (PawnBehavior pawn in BoardManager.instance.whitePawns)
                    {
                        pawn.gameObject.GetComponent<BoxCollider>().enabled = true;
                    }
                    foreach (PawnBehavior pawn in BoardManager.instance.blackPawns)
                    {
                        pawn.gameObject.GetComponent<BoxCollider>().enabled = false;
                    }
                }
                if (!freeCamActive) ChangeCamera();
            }
        }
        else
        {
            playerTurn.text = "Player " + (activePlayer == 1 ? 2 : 1) + " Wins!";
        }
    }
    public void SetTierPoints()
    {
        for (int i = 0; i < pointsToNextTier.Count; i++)
        {
            if (int.TryParse(tierPointsSettings[i].text, out int pts))
            {
                if (pts == 0)
                    pts = 3; //3 by default
                pointsToNextTier[i] = pts;
            }
        }
    }
    public void UpdateTierTexts()
    {
        foreach (GameObject info in playerInfo)
        {
            if (playerTier[playerInfo.IndexOf(info)] < 3)
            {
                info.GetComponent<TextMeshProUGUI>().text = "PLAYER " + (playerInfo.IndexOf(info) + 1) + "\nNext tier: " + (playerTier[playerInfo.IndexOf(info)] + 1) + "\nPoints: " + playerPoints[playerInfo.IndexOf(info)] + "/" + pointsToNextTier[playerTier[playerInfo.IndexOf(info)] - 1];
            }
            else
            {
                info.GetComponent<TextMeshProUGUI>().text = "Player " + (playerInfo.IndexOf(info) + 1) + "\nHas achieved\nall tiers.";
            }
        }
    }
    public void SetPlayerTimer()
    {
        int totalTime = 0;
        timerActive = timerSettings[0].transform.parent.transform.parent.gameObject.activeSelf;
        Debug.Log(timerActive);
        if (!timerActive)    
        {
            timer.text = "";
        }
        else
        {
            if (timerSettings[0] != null && int.TryParse(timerSettings[0].text, out int minutes))
            {
                minutes *= 60;
                totalTime += minutes;
            }
            if (timerSettings[1] != null && int.TryParse(timerSettings[1].text, out int seconds))
            {
                totalTime += seconds;
            }

            if (totalTime == 0)
            {
                totalTime += 180; //3min by default
            }

            if (timerSettings[2] != null && int.TryParse(timerSettings[2].text, out int extraSecs))
            {
                extraTimeAddedPerTurn = extraSecs;
            }

            for (int i = 0; i < playerTimer.Count; i++)
            {
                playerTimer[i] = totalTime;
            }
        }
    }
    public void UpdatePlayerTimer()
    {
        float time = playerTimer[activePlayer - 1];
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        string timerText = string.Format("{0:0}:{1:00}", minutes, seconds);
        timer.text = "Timer: \n" + timerText;
    }
    public void SetSize()
    {
        if (int.TryParse(boardSizeSettings[0].text, out int x))
        {
            if (x < 3 || x > 10)
                x = 8;
            BoardManager.instance.width = x;
        }
        if (int.TryParse(boardSizeSettings[1].text, out int y))
        {
            if (y < 3 || y > 10)
                y = 8;
            BoardManager.instance.height = y;
        }
    }
    public void SetPawnRuleset()
    {
        switch (pawnSettings.text)
        {
            case "Default set":
                pawnRuleset = PawnSets.DefaultSet;
                break;
            case "S Set":
                pawnRuleset = PawnSets.SSet;
                break;
            case "T Set":
                pawnRuleset = PawnSets.TSet;
                break;
            case "U Set":
                pawnRuleset = PawnSets.USet;
                break;
            case "V Set":
                pawnRuleset = PawnSets.VSet;
                break;
            case "W Set":
                pawnRuleset = PawnSets.WSet;
                break;
            case "X Set":
                pawnRuleset = PawnSets.XSet;
                break;
            case "Y Set":
                pawnRuleset = PawnSets.YSet;
                break;
            case "Z Set":
                pawnRuleset = PawnSets.ZSet;
                break;
        }
        Debug.Log(pawnRuleset.ToString());
    }
    public bool ColumnErasingCheck()
    {
        UnityEngine.UI.Toggle check = columnErasingEnabled.GetComponent<UnityEngine.UI.Toggle>();
        return check.isOn;
    }
    public void ChangeCamera()
    {
        if (gameStarted)
        {
            bool active = activePlayer == 1;

            cameras[0].SetActive(active);
            cameras[1].SetActive(!active);
        }
    }
    public void OnCameraToggle(InputValue value)
    {
        if (value.isPressed)
        {
            freeCamActive = !freeCamActive;
            if (freeCamActive)
            {
                cameras[2].SetActive(true);
                cameras[0].SetActive(false);
                cameras[1].SetActive(false);
            }
            else
            {
                ChangeCamera();
                cameras[2].SetActive(false);
            }
        }
    }
    public void AddPoints(int player, int points)
    {
        playerPoints[player - 1] += points;
        UpdateTierTexts();

        switch (playerTier[player - 1])
        {
            case 1:
                if (playerPoints[player - 1] >= pointsToNextTier[0])
                {
                    playerTier[player - 1] = 2;
                    playerPoints[player - 1] -= pointsToNextTier[0];
                    StartCoroutine(BoardManager.instance.PushBenchedPawns(player));
                }
                break;
            case 2:
                if (playerPoints[player - 1] >= pointsToNextTier[1])
                {
                    playerTier[player - 1] = 3;
                    playerPoints[player - 1] -= pointsToNextTier[1];
                    StartCoroutine(BoardManager.instance.PushBenchedPawns(player));
                }
                break;
            default:
                break;
        }
    }
    public bool CheckWinCondition(int player)
    {
        List<PawnBehavior> pawnsToCheck = player == 1 ? BoardManager.instance.whitePawns : BoardManager.instance.blackPawns;

        if (pawnsToCheck.Count == 0)
            return true;

        foreach (PawnBehavior pawn in pawnsToCheck)
        {
            pawn.possiblePaths.Clear();
            pawn.TrackAllPaths(false);
            pawn.TrackDiagonals(false);

            if (pawn.possiblePaths.Count > 0)
                return false;
        }
        return true;
    }

    public void GoToPawnCreation()
    {
        selectionCanvas.SetActive(false);
        pawnConfigCanvas.SetActive(true);
        SetCustomValuesInMenu();
    }
    public void SetCustomValuesInMenu()
    {
        if (customRuleset.assigned)
        {
            customRulesetSettings[0].text = customRuleset.possibleMovesTier1.Count.ToString();
            customRulesetSettings[1].text = customRuleset.possibleMovesTier2.Count.ToString();
            customRulesetSettings[2].text = customRuleset.possibleMovesTier3.Count.ToString();


        }
        else
        {
            //Will be the defaults
            customRulesetSettings[0].text = "1";
            customRulesetSettings[1].text = "1";
            customRulesetSettings[2].text = "1";
        }
        SetKillRangeInCanvas(customRulesetKillRangeSettings[0], customRuleset.killRangeTier1);
        SetKillRangeInCanvas(customRulesetKillRangeSettings[1], customRuleset.killRangeTier2);
        SetKillRangeInCanvas(customRulesetKillRangeSettings[2], customRuleset.killRangeTier3);
    }
    public void SetCustomRuleset()
    {
        customRuleset.assigned = true;

        customRuleset.possibleMovesTier1 = new List<int>();
        customRuleset.possibleMovesTier2 = new List<int>();
        customRuleset.possibleMovesTier3 = new List<int>();

        customRuleset.killRangeTier1 = new List<int>();
        customRuleset.killRangeTier2 = new List<int>();
        customRuleset.killRangeTier3 = new List<int>();

        int value;

        if (int.TryParse(customRulesetSettings[0].text, out value))
            for (int i = 0; i < value; i++)
                customRuleset.possibleMovesTier1.Add(i + 1);

        if (int.TryParse(customRulesetSettings[1].text, out value))
            for (int i = 0; i < value; i++)
                customRuleset.possibleMovesTier2.Add(i + 1);

        if (int.TryParse(customRulesetSettings[2].text, out value))
            for (int i = 0; i < value; i++)
                customRuleset.possibleMovesTier3.Add(i + 1);

        SetKillRange(customRulesetKillRangeSettings[0], customRuleset.killRangeTier1);
        SetKillRange(customRulesetKillRangeSettings[1], customRuleset.killRangeTier2);
        SetKillRange(customRulesetKillRangeSettings[2], customRuleset.killRangeTier3);

        selectionCanvas.SetActive(true);
        CheckIfCustomRulesetExists();
        pawnConfigCanvas.SetActive(false);
    }
    public void ClearCustomRuleset()
    {
        customRuleset.assigned = false;

        customRuleset.possibleMovesTier1 = null;
        customRuleset.possibleMovesTier2 = null;
        customRuleset.possibleMovesTier3 = null;

        customRuleset.killRangeTier1 = null;
        customRuleset.killRangeTier2 = null;
        customRuleset.killRangeTier3 = null;

        selectionCanvas.SetActive(true);
        SetCustomValuesInMenu();
        pawnConfigCanvas.SetActive(false);
    }
    public void CheckIfCustomRulesetExists()
    {
        if (customRuleset.assigned)
        {
            TMP_Dropdown.OptionData customSet = new TMP_Dropdown.OptionData("Custom set");
            pawnSettings.transform.parent.gameObject.GetComponent<TMP_Dropdown>().AddOptions(new List<TMP_Dropdown.OptionData> { customSet });
        }
    }
    private void SetKillRange(GameObject tierObject, List<int> targetList)
    {
        for (int i = 0; i < tierObject.transform.childCount; i++)
        {
            Transform child = tierObject.transform.GetChild(i);
            UnityEngine.UI.Toggle toggle = child.GetComponent<UnityEngine.UI.Toggle>();

            if (toggle == null || !toggle.isOn)
                continue;

            if (int.TryParse(child.name, out int value))
            {
                targetList.Add(value);
            }
        }
    }
    private void SetKillRangeInCanvas(GameObject killRange, List<int> targetList)
    {
        for (int i = 0; i < killRange.transform.childCount; i++)
        {
            Transform child = killRange.transform.GetChild(i);
            UnityEngine.UI.Toggle toggle = child.GetComponent<UnityEngine.UI.Toggle>();

            if (toggle == null)
                continue;

            if (!customRuleset.assigned)
            {
                toggle.isOn = (i == 0);
                continue;
            }

            toggle.isOn = false;

            if (targetList == null)
                continue;

            if (int.TryParse(child.name, out int value))
            {
                if (targetList.Contains(value))
                {
                    toggle.isOn = true;
                }
            }
        }
    }
    public void CheckIfAtLeastOneIsSelected(GameObject killRangeContainer)
    {
        bool anyOn = false;
        UnityEngine.UI.Toggle firstToggle = null;

        for (int i = 0; i < killRangeContainer.transform.childCount; i++)
        {
            Transform child = killRangeContainer.transform.GetChild(i);
            UnityEngine.UI.Toggle toggle = child.GetComponent<UnityEngine.UI.Toggle>();

            if (toggle == null)
                continue;

            if (firstToggle == null)
                firstToggle = toggle;

            if (toggle.isOn)
            {
                anyOn = true;
                break;
            }
        }
        if (!anyOn && firstToggle != null)
        {
            firstToggle.isOn = true;
        }
    }
}