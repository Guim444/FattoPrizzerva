using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance;
    public GameObject pawnPrefab;

    public Dictionary<string, ChessSquareScript> squares = new Dictionary<string, ChessSquareScript>();

    void Awake()
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

    public void RegisterSquare(ChessSquareScript square)
    {
        if (!squares.ContainsKey(square.name))
        {
            squares.Add(square.name, square);
        }
        else
            Debug.LogWarning("Duplicate square: " + square.name);
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

        for (char col = 'A'; col <= 'H' && spawned < quantity; col++)
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
