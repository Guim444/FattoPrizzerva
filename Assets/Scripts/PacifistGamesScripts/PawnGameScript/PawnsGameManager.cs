using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

public class PawnsGameManager : MonoBehaviour
{
    public static PawnsGameManager instance;
    public List<GameObject> cameras = new List<GameObject>();
    public int activePlayer = 1; // 1 = Player 1, -1 = Player 2

    public List<int> playerTier = new List<int>() { 1, 1 };
    public List<int> playerPoints = new List<int>() { 0, 0 };
    public List<bool> waitingRowIsReady = new List<bool>() { false, false };

    public List<int> pointsToNextTier = new List<int>();

    public TextMeshProUGUI playerTurn;

    public List<GameObject> playerInfo;

    public bool freeCamActive = false;

    private void Awake()
    {
        instance = this;
        cameras[0].SetActive(true);
        cameras[1].SetActive(false);
        cameras[2].SetActive(false);
    }

    public void NextPlayerTurn()
    {
        activePlayer = activePlayer == 1 ? 2 : 1;

        if (!CheckWinCondition(activePlayer))
        {
            playerTurn.text = "Player " + activePlayer + " Turn";
            if (waitingRowIsReady[activePlayer - 1])
            {
                StartCoroutine(BoardManager.instance.PushWaitingRowToBoard(activePlayer));
                waitingRowIsReady[activePlayer - 1] = false;
            }
            else if (!freeCamActive)
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
                ChangeCamera();
            }
        }
        else
        {
            playerTurn.text = "Player " + (activePlayer == 1 ? 2 : 1) + " Wins!";
        }
    }
    public void UpdateTierTexts()
    {
        foreach (GameObject info in playerInfo)
        {
            info.GetComponent<TextMeshProUGUI>().text = "PLAYER " + (playerInfo.IndexOf(info)+1) + "\nNext tier: " + (playerTier[playerInfo.IndexOf(info)] + 1) + "\nPoints: " + playerPoints[playerInfo.IndexOf(info)] + "/" + pointsToNextTier[playerTier[playerInfo.IndexOf(info)] - 1];
        }
    }
    public void ChangeCamera()
    {
        bool active = activePlayer == 1;

        cameras[0].SetActive(active);
        cameras[1].SetActive(!active);
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
}