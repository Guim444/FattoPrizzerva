using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public GameObject pawnPrefab;
    public GameObject whiteSquarePrefab, blackSquarePrefab, boardPrefab;
    public List<GameObject> rowCount = new List<GameObject>();
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
    }
    public void RegisterSquare(ChessSquareScript square)
    {
        if (!squares.ContainsKey(square.name))
        {
            squares.Add(square.name, square);
        }
        else
            Debug.LogWarning("Duplicate square: " + square.name);

        if (squares.Count == height * width)
        {
            SpawnPawns((int)width);
        }
    }

    public ChessSquareScript GetSquare(string name)
    {
        if (squares.TryGetValue(name, out ChessSquareScript square))
            return square;
        return null;
    }
    public void SpawnPawns(int quantity)
    {
        int row = 1;
        int spawned = 0;

        for (char col = 'A'; spawned < quantity; col++)
        {
            string squareName = col.ToString() + row.ToString();
            ChessSquareScript square = GetSquare(squareName);

            if (square != null && square.empty)
            {
                GameObject pawnObj = Instantiate(pawnPrefab, square.pawnPosition, Quaternion.identity);

                PawnBehavior pawnScript = pawnObj.GetComponent<PawnBehavior>();
                if (pawnScript != null)
                    pawnScript.currentSquare = square;

                square.empty = false;

                spawned++;
            }
        }

        if (spawned < quantity)
            Debug.LogWarning("No se pudieron instanciar todos los pawns: faltaron " + (quantity - spawned));
    }
}
