using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    public static ClickManager instance;
    public PawnBehavior selectedPawn;
    public ChessSquareScript selectedSquare;

    bool clickPerformed = false;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick(InputValue value)
    {
        if (value.isPressed && !clickPerformed)
        {
            clickPerformed = true;
            Debug.Log("Click");
            if (selectedPawn != null)
            {
                selectedPawn.Deselect();
                selectedPawn = null;
            }
        }
        else if (!value.isPressed)
        {
            clickPerformed = false;
        }
    }
}
