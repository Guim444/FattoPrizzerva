using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PawnsGameManager : MonoBehaviour
{
    public static PawnsGameManager instance;
    public List<GameObject> cameras = new List<GameObject>();
    public int activePlayer = 1; // 1 = Player 1, -1 = Player 2

    private void Awake()
    {
        instance = this;
        cameras[0].SetActive(true);
        cameras[1].SetActive(false);
    }

    public void NextPlayerTurn()
    {
        if (activePlayer == 1)
        {
            foreach (PawnBehavior pawn in BoardManager.instance.whitePawns)
            {
                pawn.gameObject.GetComponent<BoxCollider>().enabled = false;
            }
            foreach (PawnBehavior pawn in BoardManager.instance.blackPawns)
            {
                pawn.gameObject.GetComponent<BoxCollider>().enabled = true;
            }
            activePlayer = 2;
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
            activePlayer = 1;
        }
        ChangeCamera();
    }
    public void ChangeCamera()
    {
        bool active = activePlayer == 1;

        cameras[0].SetActive(active);
        cameras[1].SetActive(!active);
    }
}
