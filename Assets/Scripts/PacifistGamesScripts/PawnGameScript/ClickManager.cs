using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    public static ClickManager instance;
    public PawnBehavior selectedPawn;
    public ChessSquareScript selectedSquare;

    public bool isSelectingMovement = false;

    private void Awake()
    {
        instance = this;
    }

    public void OnClick(InputValue value)
    {
        if (value.isPressed)
        {
            if (ClickHitsSelectableSquare())
            {
                isSelectingMovement = true;
                return;
            }

            if (selectedPawn != null)
            {
                selectedPawn.Deselect();
                selectedPawn = null;
            }
        }
    }

    bool ClickHitsSelectableSquare()
    {
        if (Camera.main != null && Mouse.current != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                ChessSquareScript square = hit.collider.GetComponent<ChessSquareScript>();
                if (square != null && square.selectableSquare)
                {
                    selectedSquare = square;
                    return true;
                }
            }

            return false;
        }
        return false;
    }
}
