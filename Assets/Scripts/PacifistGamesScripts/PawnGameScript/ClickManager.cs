using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PawnClickManager : MonoBehaviour
{
    public static PawnClickManager instance;
    public PawnBehavior selectedPawn;
    public PawnSquareScript selectedSquare;

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
                PawnSquareScript square = hit.collider.GetComponent<PawnSquareScript>();
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
    public void OnExit(InputValue value)
    {
        if (PawnsGameManager.instance != null)
        {
            if (value.isPressed && PawnsGameManager.instance.gameStarted)
            {
                PawnsGameManager.instance.gameStarted = false;
                PawnsGameManager.instance.dataCanvas.SetActive(false);
                SceneManager.LoadScene("Main menu");
            }
        }
    }
}
