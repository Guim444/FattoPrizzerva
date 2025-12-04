using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines.ExtrusionShapes;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public GameObject whitePawnPrefab, blackPawnPrefab;
    public GameObject whiteSquarePrefab, blackSquarePrefab, boardPrefab;
    public GameObject waitingZonePrefab1, waitingZonePrefab2, graveyardPrefab, graveyardPrefab2;
    public List<GameObject> rowCount = new List<GameObject>();

    public List<PawnBehavior> whitePawns = new List<PawnBehavior>();
    public List<PawnBehavior> blackPawns = new List<PawnBehavior>();

    public List<GameObject> whiteWaitingZone = new List<GameObject>();
    public List<GameObject> blackWaitingZone = new List<GameObject>();

    public List<GraveyardSquare> whiteGraveyardSquaresToTier2 = new List<GraveyardSquare>();
    public List<GraveyardSquare> whiteGraveyardSquaresToTier3 = new List<GraveyardSquare>();
    public List<GraveyardSquare> blackGraveyardSquaresToTier2 = new List<GraveyardSquare>();
    public List<GraveyardSquare> blackGraveyardSquaresToTier3 = new List<GraveyardSquare>();

    public float height, width;

    public Dictionary<string, ChessSquareScript> squares = new Dictionary<string, ChessSquareScript>();

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        GenerateBoard();
    }
    public void GenerateBoard()
    {
        List<GameObject> fullBoard = new List<GameObject>();

        GameObject board = Instantiate(boardPrefab, transform.position, Quaternion.identity);
        board.transform.parent = transform;
        board.transform.localScale = new Vector3(height + 0.5f, 1, width + 0.5f);

        for (int i = 0; i < height; i++)
        {
            GameObject rowParent = new GameObject("Row" + (i + 1));
            rowParent.transform.parent = board.transform;

            float rowX = height / 2f - 0.5f - i;
            Vector3 rowStartPos = new Vector3(rowX, 0.15f, -(width / 2f - 0.5f));
            GameObject previousSquare = null;

            for (int j = 0; j < width; j++)
            {
                GameObject prefab = ((i + j) % 2 == 0) ? whiteSquarePrefab : blackSquarePrefab;

                GameObject square = Instantiate(prefab);
                square.transform.parent = rowParent.transform;

                if (previousSquare == null)
                {
                    square.transform.position = rowStartPos;
                }
                else
                {
                    Vector3 prevPos = previousSquare.transform.position;
                    square.transform.position = new Vector3(prevPos.x, prevPos.y, prevPos.z + 1f);
                }

                previousSquare = square;

                ChessSquareScript css = square.GetComponent<ChessSquareScript>();
                if (css != null)
                {
                    css.SquareColumn = (char)('A' + j);
                }
                css.CreateSquare();
            }
        }
        GenerateWaitingZone();
        GenerateGraveyard();
    }
    public void RegisterSquare(ChessSquareScript square)
    {
        if (!squares.ContainsKey(square.name))
        {
            squares.Add(square.name, square);
        }

        if (squares.Count == height * width)
        {
            SpawnPawns((int)width, 1);
        }
    }

    public ChessSquareScript GetSquare(string name)
    {
        if (squares.TryGetValue(name, out ChessSquareScript square))
            return square;
        return null;
    }
    public void SpawnPawns(int quantity, int tier)
    {
        int row = 1;
        int spawned = 0;

        for (char col = 'A'; spawned < quantity; col++)
        {
            string squareName = col.ToString() + row.ToString();
            ChessSquareScript square = GetSquare(squareName);

            if (square != null && square.empty)
            {
                GameObject pawnObj = Instantiate(whitePawnPrefab, square.pawnPosition, Quaternion.identity);

                PawnBehavior pawnScript = pawnObj.GetComponent<PawnBehavior>();

                pawnScript.pawnTier = tier;
                pawnScript.SetPawnTier();

                if (pawnScript != null)
                    pawnScript.currentSquare = square;

                square.empty = false;
                square.pawn = pawnScript;

                whitePawns.Add(pawnScript);

                spawned++;
            }
        }

        row = (int)height;
        spawned = 0;

        for (char col = (char)(quantity - 1); spawned < quantity; col --)
        {
            string squareName = col.ToString() + row.ToString();
            ChessSquareScript square = GetSquare(squareName);

            if (square != null && square.empty)
            {
                GameObject pawnObj = Instantiate(blackPawnPrefab, square.pawnPosition, Quaternion.identity);

                PawnBehavior pawnScript = pawnObj.GetComponent<PawnBehavior>();
                if (pawnScript != null)
                    pawnScript.currentSquare = square;

                square.empty = false;
                square.pawn = pawnScript;

                blackPawns.Add(pawnScript);

                spawned++;
            }
        }
    }

    public void GenerateWaitingZone()
    {
        GameObject waitingZone = Instantiate(boardPrefab, transform.position, Quaternion.identity);
        waitingZone.name = "WaitingZone";
        waitingZone.transform.parent = transform;

        waitingZone.transform.localScale = new Vector3(4.5f, 1, width + 0.5f);
        waitingZone.transform.position = new Vector3(-(height / 2f) - 2.25f, 0, 0);

        GameObject previousBench = null;
        for (int i = 0; i < width; i++)
        {
            GameObject bench = Instantiate((i % 2 == 0) ? waitingZonePrefab1 : waitingZonePrefab2);
            bench.transform.parent = waitingZone.transform;
            float zOffset = -(width / 2f) + 0.5f + i;
            if (previousBench == null)
            {
                bench.transform.position = new Vector3(waitingZone.transform.position.x - 0.375f, 0.15f, zOffset);
            }
            else
            {
                Vector3 prevPos = previousBench.transform.position;
                bench.transform.position = new Vector3(prevPos.x, prevPos.y, prevPos.z + 1f);
            }
            previousBench = bench;
        }
        //Now copy the waiting zone to the other side. Ensure it's facing the right way (-180 degrees on Y axis)

        GameObject waitingZone2 = Instantiate(waitingZone, transform.position, Quaternion.identity);
        waitingZone2.name = "WaitingZone2";
        waitingZone2.transform.parent = transform;
        waitingZone2.transform.position = new Vector3((height / 2f) + 2.25f, 0, 0);
        waitingZone2.transform.rotation = Quaternion.Euler(0, 180, 0);
    }
    public void GenerateGraveyard()
    {
        GameObject graveyard = Instantiate(boardPrefab, transform.position, Quaternion.identity);
        graveyard.name = "Graveyard";
        graveyard.transform.parent = transform;

        float graveyardHeight = height + 8f;

        graveyard.transform.localScale = new Vector3(graveyardHeight + 1, 1, 2.5f);
        graveyard.transform.position = new Vector3(0, 0, (width / 2f) + 1.5f);

        GameObject previousGraveyardSquare = null;
        for (int i = 0; i < graveyardHeight; i++)
        {
            GameObject graveyardSquare = Instantiate((i >= graveyardHeight/2) ? graveyardPrefab : graveyardPrefab2);
            graveyardSquare.name = "GraveyardSquare" + (i + 1);
            graveyardSquare.transform.parent = graveyard.transform;
            float xOffset = -(graveyardHeight / 2f) + 0.5f + i;

            if (previousGraveyardSquare == null)
            {
                graveyardSquare.transform.position = new Vector3(xOffset, 0.15f, graveyard.transform.position.z);
            }
            else
            {
                Vector3 prevPos = previousGraveyardSquare.transform.position;
                graveyardSquare.transform.position = new Vector3(prevPos.x + 1f, prevPos.y, prevPos.z);
            }
            previousGraveyardSquare = graveyardSquare;

            foreach (Transform child in graveyardSquare.transform)
            {
                GraveyardSquare gqs = child.GetComponent<GraveyardSquare>();
                gqs.pawnPosition = child.position + Vector3.up * 0.75f;
                if (gqs != null)
                {
                    if (i < graveyardHeight / 2)
                    {
                        gqs.player = 1;
                        if (gqs.tier == 2)
                        {
                            whiteGraveyardSquaresToTier2.Add(gqs);
                        }
                        else if (gqs.tier == 3)
                        {
                            whiteGraveyardSquaresToTier3.Add(gqs);
                        }
                    }
                    else
                    {
                        gqs.player = 2;
                        if (gqs.tier == 2)
                        {
                            blackGraveyardSquaresToTier2.Add(gqs);
                        }
                        else if (gqs.tier == 3)
                        {
                            blackGraveyardSquaresToTier3.Add(gqs);
                        }
                    }
                }
            }
        }
    }

    internal Vector3 GetGraveyardPosition(int player, int playerTier)
    {
        if (player == 1)
        {
            if (playerTier == 1)
            {
                foreach (GraveyardSquare gqs in whiteGraveyardSquaresToTier2)
                {
                    if (gqs.empty)
                    {
                        gqs.empty = false;
                        return gqs.pawnPosition;
                    }
                }
            }
            else if (playerTier == 2)
            {
                foreach (GraveyardSquare gqs in whiteGraveyardSquaresToTier3)
                {
                    if (gqs.empty)
                    {
                        gqs.empty = false;
                        return gqs.pawnPosition;
                    }
                }
            }
        }
        else
        {
            if (playerTier == 1)
            {
                foreach (GraveyardSquare gqs in blackGraveyardSquaresToTier2)
                {
                    if (gqs.empty)
                    {
                        gqs.empty = false;
                        return gqs.pawnPosition;
                    }
                }
            }
            else if (playerTier == 2)
            {
                foreach (GraveyardSquare gqs in blackGraveyardSquaresToTier3)
                {
                    if (gqs.empty)
                    {
                        gqs.empty = false;
                        return gqs.pawnPosition;
                    }
                }
            }
        }
        return Vector3.zero;
    }
}
