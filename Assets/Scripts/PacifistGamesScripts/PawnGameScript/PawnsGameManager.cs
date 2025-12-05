using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PawnsGameManager : MonoBehaviour
{
    public static PawnsGameManager instance;
    public List<GameObject> cameras = new List<GameObject>();
    public int activePlayer = 1; // 1 = Player 1, -1 = Player 2

    public List<int> playerTier = new List<int>() { 1, 1 };
    public List<int> playerPoints = new List<int>() { 0, 0 };
    public List<bool> waitingRowIsReady = new List<bool>() { false, false };

    public List<int > pointsToNextTier = new List<int>() { 5, 8 };

    public TextMeshProUGUI playerTurn;

    private void Awake()
    {
        instance = this;
        cameras[0].SetActive(true);
        cameras[1].SetActive(false);
    }

    public void NextPlayerTurn()
    {
        activePlayer = activePlayer == 1 ? 2 : 1;
        playerTurn.text = "Player " + activePlayer + " Turn";
        if (waitingRowIsReady[activePlayer - 1])
        {
            StartCoroutine(BoardManager.instance.PushWaitingRowToBoard(activePlayer));
            waitingRowIsReady[activePlayer - 1] = false;
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
            ChangeCamera();
        }
    }
    public void ChangeCamera()
    {
        bool active = activePlayer == 1;

        cameras[0].SetActive(active);
        cameras[1].SetActive(!active);
    }
    public void AddPoints(int player, int points)
    {
        playerPoints[player - 1] += points;

        switch (playerTier[player - 1])
        {
            case 1:
                if (playerPoints[player - 1] >= 1)
                {
                    playerTier[player - 1] = 2;
                    playerPoints[player - 1] -= pointsToNextTier[0];
                    StartCoroutine(BoardManager.instance.PushBenchedPawns(player));
                    waitingRowIsReady[player - 1] = true;
                }
                break;
            case 2:
                if (playerPoints[player - 1] >= 8)
                {
                    playerTier[player - 1] = 3;
                    playerPoints[player - 1] -= pointsToNextTier[1];
                    StartCoroutine(BoardManager.instance.PushBenchedPawns(player));
                    waitingRowIsReady[player - 1] = true;
                }
                break;
            default:
                break;
        }
    }
}
