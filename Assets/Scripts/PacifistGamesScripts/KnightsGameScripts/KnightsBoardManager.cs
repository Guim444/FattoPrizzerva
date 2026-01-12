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

            GameObject knightObj = Instantiate(KnightsGameManager.instance.agileKnightPrefab);
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
        int midRow = (height + 1) / 2;
        int midColIndex = (width - 1) / 2;
        char midColumn = (char)('A' + midColIndex);

        Vector2Int[][] LPaths = new Vector2Int[][]
        {
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(2,-1) },

        new[] { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-2,0), new Vector2Int(-2,1) },
        new[] { new Vector2Int(0,0), new Vector2Int(-1,0), new Vector2Int(-2,0), new Vector2Int(-2,-1) },

        new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(1,2) },
        new[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2), new Vector2Int(-1,2) },

        new[] { new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2), new Vector2Int(1,-2) },
        new[] { new Vector2Int(0,0), new Vector2Int(0,-1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        };

        Vector2Int[] chosenPath = null;

        foreach (var path in LPaths)
        {
            bool valid = true;

            foreach (var offset in path)
            {
                char c = (char)(midColumn + offset.x);
                int r = midRow + offset.y;

                if (!squares.TryGetValue(c.ToString() + r, out var sq) || !sq.empty)
                {
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                chosenPath = path;
                break;
            }
        }

        if (chosenPath == null)
        {
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            char c = (char)(midColumn + chosenPath[i].x);
            int r = midRow + chosenPath[i].y;

            KnightsSquareScript sq = squares[c.ToString() + r];

            GameObject obj = Instantiate(KnightsGameManager.instance.agileKnightPrefab);
            KnightBehavior knight = obj.GetComponent<KnightBehavior>();

            knight.player = 1;
            knight.currentSquare = sq;
            knight.transform.position = sq.knightPosition;
            knight.GetComponent<MeshRenderer>().material.color = Color.cyan;

            sq.knight = knight;
            sq.empty = false;
        }

        {
            char c = (char)(midColumn + chosenPath[3].x);
            int r = midRow + chosenPath[3].y;

            KnightsSquareScript sq = squares[c.ToString() + r];

            GameObject obj = Instantiate(KnightsGameManager.instance.tucutuKnightPrefab);
            KnightBehavior knight = obj.GetComponent<KnightBehavior>();

            knight.player = 2;
            knight.currentSquare = sq;
            knight.transform.position = sq.knightPosition;
            knight.GetComponent<MeshRenderer>().material.color = Color.red;

            sq.knight = knight;
            sq.empty = false;
        }
    }
}
