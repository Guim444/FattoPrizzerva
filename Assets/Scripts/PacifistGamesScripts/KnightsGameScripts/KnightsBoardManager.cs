using UnityEngine;
using System.Collections.Generic;

public class KnightsBoardManager : MonoBehaviour
{
    public int height, width;
    public static KnightsBoardManager instance;
    public Dictionary<string, KnightsSquareScript> squares = new Dictionary<string, KnightsSquareScript>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public KnightsSquareScript GetSquare(string squareName)
    {
        if (squares.TryGetValue(squareName, out KnightsSquareScript square))
            return square;
        return null;
    }
    public void SpawnKnights(int spawnedKnights)
    {
        int spawned = 0;

        // Ordenar las casillas por fila y columna
        List<KnightsSquareScript> orderedSquares = new List<KnightsSquareScript>(squares.Values);
        orderedSquares.Sort((a, b) =>
        {
            int rowCompare = a.SquareRow.CompareTo(b.SquareRow);
            if (rowCompare != 0)
                return rowCompare;

            return a.SquareColumn.CompareTo(b.SquareColumn);
        });

        foreach (KnightsSquareScript square in orderedSquares)
        {
            if (spawned >= spawnedKnights)
                break;

            if (!square.empty)
                continue;

            GameObject knightObj = Instantiate(KnightsGameManager.instance.knightPrefab);
            KnightBehavior knight = knightObj.GetComponent<KnightBehavior>();

            knight.player = 1;
            knight.currentSquare = square;
            knight.transform.position = square.knightPosition;

            square.knight = knight;
            square.empty = false;

            spawned++;
        }
    }

    public void SpawnKnightInMiddle()
    {
        if (KnightsGameManager.instance.knightPrefab == null)
        {
            return;
        }

        int midRow = (height + 1) / 2;
        int midColIndex = (width - 1) / 2;
        char midColumn = (char)('A' + midColIndex);

        string squareName = midColumn.ToString() + midRow;
        if (!squares.TryGetValue(squareName, out KnightsSquareScript targetSquare))
        {
            return;
        }

        if (!targetSquare.empty)
        {
            return;
        }

        GameObject knightObj = Instantiate(KnightsGameManager.instance.knightPrefab);
        KnightBehavior knight = knightObj.GetComponent<KnightBehavior>();

        knight.player = 1;
        knight.currentSquare = targetSquare;
        knight.transform.position = targetSquare.knightPosition;
        knight.GetComponent<MeshRenderer>().material.color = Color.cyan;

        targetSquare.knight = knight;
        targetSquare.empty = false;
        Vector2Int[] offsets = new Vector2Int[]
            {
        new Vector2Int(2, 1),
        new Vector2Int(1, 2),
        new Vector2Int(-1, 2),
        new Vector2Int(-2, 1),
        new Vector2Int(-2, -1),
        new Vector2Int(-1, -2),
        new Vector2Int(1, -2),
        new Vector2Int(2, -1)
            };

        KnightsSquareScript square2 = null;
        foreach (var offset in offsets)
        {
            char col = (char)(midColumn + offset.x);
            int row = midRow + offset.y;
            string name = col.ToString() + row;

            if (squares.TryGetValue(name, out KnightsSquareScript candidate) && candidate.empty)
            {
                square2 = candidate;
                break;
            }
        }

        if (square2 == null)
        {
            Debug.LogWarning("No hay casilla libre en L para el jugador 2 cerca del centro");
            return;
        }

        GameObject knightObj2 = Instantiate(KnightsGameManager.instance.knightPrefab);
        KnightBehavior knight2 = knightObj2.GetComponent<KnightBehavior>();

        knight2.player = 2;
        knight2.currentSquare = square2;
        knight2.transform.position = square2.knightPosition;
        knight2.GetComponent<MeshRenderer>().material.color = Color.red;

        square2.knight = knight2;
        square2.empty = false;
    }
}
