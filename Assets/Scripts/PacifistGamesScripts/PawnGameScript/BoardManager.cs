using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    public List<BenchSquare> whiteWaitingZone1, whiteWaitingZone2, whiteWaitingZone3;
    public List<BenchSquare> blackWaitingZone1, blackWaitingZone2, blackWaitingZone3;


    public List<GraveyardSquare> whiteGraveyardSquaresToTier2 = new List<GraveyardSquare>();
    public List<GraveyardSquare> whiteGraveyardSquaresToTier3 = new List<GraveyardSquare>();
    public List<GraveyardSquare> blackGraveyardSquaresToTier2 = new List<GraveyardSquare>();
    public List<GraveyardSquare> blackGraveyardSquaresToTier3 = new List<GraveyardSquare>();

    public float height, width;

    public Dictionary<string, ChessSquareScript> squares = new Dictionary<string, ChessSquareScript>();

    private Dictionary<GameObject, List<int>> deadZoneColumns = new Dictionary<GameObject, List<int>>();

    public GameObject whiteDeadZone, blackDeadZone;

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
        GenerateDeadZone();
        PawnsGameManager.instance.UpdateTierTexts();
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

                pawnScript.pawnTier = tier;
                pawnScript.SetPawnTier();

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
            bench.name = "Bench" + (i + 1);
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

        char column;

        column = 'A';
        foreach (Transform benchZone in waitingZone.transform)
        {
            foreach (Transform bench in benchZone)
            {
                BenchSquare bs = bench.GetComponent<BenchSquare>();
                bs.pawnPosition = bench.transform.position + Vector3.up * 0.75f;
                bs.player = 2;

                switch (bs.tier)
                {
                    case 2:
                        blackWaitingZone2.Add(bs);
                        GenerateBenchedPawn(bs);
                        break;
                    case 3:
                        blackWaitingZone3.Add(bs);
                        GenerateBenchedPawn(bs);
                        break;
                    default:
                        bs.column = column;
                        blackWaitingZone1.Add(bs);
                        break;
                }
            }
            column++;
        }
        column = (char)('A' + width - 1);
        foreach (Transform benchZone in waitingZone2.transform)
        {
            foreach (Transform bench in benchZone)
            {
                BenchSquare bs = bench.GetComponent<BenchSquare>();
                if (bs != null)
                {
                    bs.pawnPosition = bench.transform.position + Vector3.up * 0.75f;
                    bs.player = 1;

                    switch (bs.tier)
                    {
                        case 2:
                            whiteWaitingZone2.Add(bs);
                            GenerateBenchedPawn(bs);
                            break;
                        case 3:
                            whiteWaitingZone3.Add(bs);
                            GenerateBenchedPawn(bs);
                            break;
                        default:
                            bs.column = column;
                            whiteWaitingZone1.Add(bs);
                            break;
                    }
                }
            }
            column--;
        }
    }

    public void GenerateBenchedPawn(BenchSquare bs)
    {
        GameObject pawnObj = Instantiate((bs.player == 1) ? whitePawnPrefab : blackPawnPrefab);
        PawnBehavior pawnScript = pawnObj.GetComponent<PawnBehavior>();
        pawnScript.pawnTier = bs.tier;
        pawnScript.SetPawnTier();
        pawnObj.GetComponent<Transform>().position = bs.pawnPosition;
        pawnObj.transform.position = bs.pawnPosition;
        bs.empty = false;
        bs.storedPawn = pawnScript;
        pawnObj.GetComponent<BoxCollider>().enabled = false;
    }
    public IEnumerator PushBenchedPawns(int player)
    {
        List<PawnBehavior> pawnsTier3 = new List<PawnBehavior>();
        List<PawnBehavior> pawnsTier2 = new List<PawnBehavior>();

        if (player == 1)
        {
            foreach (BenchSquare bs in whiteWaitingZone3)
            {
                if (!bs.empty)
                {
                    bs.empty = true;
                    pawnsTier3.Add(bs.storedPawn);
                    bs.storedPawn = null;
                }
            }
            foreach (BenchSquare bs in whiteWaitingZone2)
            {
                if (!bs.empty)
                {
                    bs.empty = true;
                    pawnsTier2.Add(bs.storedPawn);
                    bs.storedPawn = null;
                }
            }

            yield return new WaitForSeconds(0.5f);

            foreach (PawnBehavior pb in pawnsTier2)
            {
                foreach (BenchSquare bs in whiteWaitingZone1)
                {
                    if (bs.empty)
                    {
                        StartCoroutine(pb.SmoothMove(bs.pawnPosition));
                        bs.storedPawn = pb;
                        bs.empty = false;
                        break;
                    }
                }
            }
            foreach (PawnBehavior pb in pawnsTier3)
            {
                foreach (BenchSquare bs in whiteWaitingZone2)
                {
                    if (bs.empty)
                    {
                        StartCoroutine(pb.SmoothMove(bs.pawnPosition));
                        bs.storedPawn = pb;
                        bs.empty = false;
                        break;
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
        else if (player == 2)
        {
            foreach (BenchSquare bs in blackWaitingZone3)
            {
                if (!bs.empty)
                {
                    bs.empty = true;
                    pawnsTier3.Add(bs.storedPawn);
                    bs.storedPawn = null;
                }
            }
            foreach (BenchSquare bs in blackWaitingZone2)
            {
                if (!bs.empty)
                {
                    bs.empty = true;
                    pawnsTier2.Add(bs.storedPawn);
                    bs.storedPawn = null;
                }
            }
            yield return new WaitForSeconds(0.5f);
            foreach (PawnBehavior pb in pawnsTier2)
            {
                foreach (BenchSquare bs in blackWaitingZone1)
                {
                    if (bs.empty)
                    {
                        StartCoroutine(pb.SmoothMove(bs.pawnPosition));
                        bs.storedPawn = pb;
                        bs.empty = false;
                        break;
                    }
                }
            }
            foreach (PawnBehavior pb in pawnsTier3)
            {
                foreach (BenchSquare bs in blackWaitingZone2)
                {
                    if (bs.empty)
                    {
                        StartCoroutine(pb.SmoothMove(bs.pawnPosition));
                        bs.storedPawn = pb;
                        bs.empty = false;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        PawnsGameManager.instance.waitingRowIsReady[player - 1] = true;
    }

    public IEnumerator PushWaitingRowToBoard(int player)
    {
        Debug.Log("Pushing waiting row to board for player " + player);
        List<BenchSquare> waitingZone = (player == 1) ? whiteWaitingZone1 : blackWaitingZone1;
        foreach (BenchSquare bs in waitingZone)
        {
            if (!bs.empty)
            {
                StartCoroutine(bs.storedPawn.MoveToBoard(player, bs));
                bs.empty = true;
                bs.storedPawn = null;
            }
        }
        yield return new WaitForSeconds(0.1f);
        PawnsGameManager.instance.NextPlayerTurn();

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
            GameObject graveyardSquare = Instantiate((i >= graveyardHeight / 2) ? graveyardPrefab : graveyardPrefab2);
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

    public void GenerateDeadZone()
    {
        whiteDeadZone = new GameObject("White Dead Zone");
        whiteDeadZone.transform.parent = transform;
        whiteDeadZone.transform.position = new Vector3(height / 2, 0, -(width / 2f) - 3.5f);

        blackDeadZone = new GameObject("Black Dead Zone");
        blackDeadZone.transform.parent = transform;
        blackDeadZone.transform.position = new Vector3(-height / 2, 0, -(width / 2f) - 3.5f);
    }

    internal Vector3 GetGraveyardPosition(int player, int playerTier)
    {
        if (player == 1)
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
            else
            {
                return GetSafeDeadZonePosition(whiteDeadZone);
            }
        }
        else
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
            else
            {
                return GetSafeDeadZonePosition(blackDeadZone);
            }
        }
        return Vector3.zero;
    }

    public ChessSquareScript GetBenchToBoardPosition(int player, BenchSquare bs)
    {
        int direction = (player == 1) ? 1 : -1;
        int targetRow = (player == 1) ? 1 : (int)height;
        char column = bs.column;

        ChessSquareScript firstSquare = null;
        List<ChessSquareScript> chain = new List<ChessSquareScript>();

        while (true)
        {
            string name = column.ToString() + targetRow.ToString();
            if (!squares.TryGetValue(name, out ChessSquareScript square))
            {
                Debug.LogWarning("Square not found: " + name);
                return null;
            }

            if (firstSquare == null)
                firstSquare = square;

            chain.Add(square);

            if (square.empty)
                break;

            int nextRow = targetRow + direction;
            if (nextRow < 1 || nextRow > height)
            {
                GameObject deadZone = (player == 1) ? whiteDeadZone : blackDeadZone;
                Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0, UnityEngine.Random.Range(-0.5f, 0.5f));

                square.pawn.transform.position = deadZone.transform.position + offset;
                square.pawn.currentSquare = null;
                square.pawn = null;
                square.empty = true;

                break;
            }

            targetRow = nextRow;
        }

        for (int i = chain.Count - 1; i > 0; i--)
        {
            ChessSquareScript from = chain[i - 1];
            ChessSquareScript to = chain[i];

            if (from.pawn != null)
            {
                to.pawn = from.pawn;
                to.empty = false;
                from.pawn.currentSquare = to;
                StartCoroutine(from.pawn.SmoothMove(to.pawnPosition));
            }

            from.pawn = null;
            from.empty = true;
        }

        return firstSquare;
    }
    private Vector3 GetSafeDeadZonePosition(GameObject zone)
    {
        float columnSpacing = 1f;
        float rowSpacing = 1f;
        int maxPerColumn = 4;

        if (!deadZoneColumns.ContainsKey(zone))
            deadZoneColumns[zone] = new List<int>() { 0 };

        List<int> columns = deadZoneColumns[zone];
        int colIndex = 0;
        int rowIndex = 0;

        for (int i = 0; i < columns.Count; i++)
        {
            if (columns[i] < maxPerColumn)
            {
                colIndex = i;
                rowIndex = maxPerColumn - 1 - columns[i];
                columns[i]++;
                break;
            }
            else if (i == columns.Count - 1)
            {
                columns.Add(0);
                colIndex = columns.Count - 1;
                rowIndex = maxPerColumn - 1 - columns[colIndex];
                columns[colIndex]++;
                break;
            }
        }

        Vector3 pos = zone.transform.position + new Vector3(colIndex * columnSpacing, 0, rowIndex * rowSpacing);
        return pos;
    }

    internal void DeselectAll()
    {
        foreach (var square in squares.Values)
        {
            square.ToggleGlow(false);
            square.selectableSquare = false;
        }
    }
}
