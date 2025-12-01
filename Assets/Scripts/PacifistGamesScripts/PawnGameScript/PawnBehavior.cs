using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class PawnBehavior : MonoBehaviour
{
    public ChessSquareScript currentSquare;
    void Awake()
    {
        if (currentSquare != null)
        {
            transform.position = currentSquare.pawnPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}