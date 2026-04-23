using UnityEngine;

public class SnakePart : MonoBehaviour
{
    public KnightsSquareScript currentSquare;
    public SnakeBody body;
    public bool isHead;
    Material mat;

    public void OnEnable()
    {
        body = GetComponentInParent<SnakeBody>();
        mat = GetComponent<Renderer>().material;
    }

    private void OnMouseDown()
    {
        if (MapEditorData.instance.editMode && MapEditorData.instance.selectedObject == null)
        {
            MapEditorData.instance.selectedObject = body.gameObject;

            foreach (SnakePart part in body.bodyParts)
            {
                part.ToggleGlow(true, 1f);
            }
        }
    }

    public bool CheckSquareUnder()
    {
        if (body != null)
        {
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 5f))
            {
                KnightsSquareScript square = hit.collider.GetComponent<KnightsSquareScript>();
                if (square != null && !(square.heavenStartZone || square.hellStartZone))
                {
                    return true;
                }
            }
        }
        return false;
    }
    public void AssignSquare()
    {
        RaycastHit hit;
        if (body != null)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f))
            {
                KnightsSquareScript square = hit.collider.GetComponent<KnightsSquareScript>();
                if (square != null)
                {
                    currentSquare = square;
                    currentSquare.snake = body;
                }
            }
        }
    }

    public void ToggleGlow(bool glow, float intensity)
    {
        if (glow)
        {
            Color glowColor = mat.color;

            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glowColor * intensity);

            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white);
            mat.DisableKeyword("_EMISSION");
        }
    }
}
