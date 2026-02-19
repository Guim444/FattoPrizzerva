using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockObstacleScript : MonoBehaviour
{
    public bool isTall; //this will define if it can be jumped.
    public bool isBreakable;
    public KnightsSquareScript currentSquare;
    public List<GameObject> spikes;
    public List<KnightsSquareScript> dangerousSquares;

    bool isMoving;

    private void OnEnable()
    {
        dangerousSquares ??= new List<KnightsSquareScript>();
    }
    private void OnMouseDown()
    {
        if (MapEditorData.instance.editMode && MapEditorData.instance.selectedObject == null)
        {
            dangerousSquares.Clear();

            currentSquare.rock = null;
            currentSquare = null;
            GetComponent<BoxCollider>().enabled = false;

            MapEditorData.instance.selectedObject = gameObject;

            ActivateGlow(true);
        }
    }
    public void SetDangerousSquares()
    {
        if (dangerousSquares.Count > 0)
            return;
        foreach (var s in spikes)
        {
            Vector2 dir = new Vector2(transform.position.x - s.transform.position.x, transform.position.z - s.transform.position.z);
            //transform to -1, 0 , or 1
            dir.x = dir.x > 0 ? 1 : dir.x < 0 ? -1 : 0;
            dir.y = dir.y > 0 ? -1 : dir.y < 0 ? 1 : 0;
            KnightsSquareScript sq = GetNearestSquareInDirection(dir);

            if (sq == null)
                continue;
            else
                dangerousSquares.Add(sq);
        }
    }

    KnightsSquareScript GetNearestSquareInDirection(Vector2 dir)
    {
        int stepX = dir.x > 0 ? 1 : dir.x < 0 ? -1 : 0;
        int stepY = dir.y > 0 ? 1 : dir.y < 0 ? -1 : 0;

        char col = currentSquare.SquareColumn;
        int row = currentSquare.SquareRow;

        col += (char)stepY;
        row += stepX;

        return KnightsBoardManager.instance.GetSquare(col.ToString() + row);
    }

    public void ActivateGlow(bool glow)
    {
        ToggleGlow(gameObject, glow, 0.3f);

        if (spikes.Count > 0)
        {
            foreach (var spike in spikes)
            {
                ToggleGlow(spike, glow, 0.3f);
            }
        }
    }

    public void ToggleGlow(GameObject gameObj, bool glow, float intensity)
    {
        Material mat = gameObj.GetComponent<Renderer>().material;

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

    public IEnumerator MoveRock(Vector3 targetPos)
    {
        if (!isMoving)
        {
            isMoving = true;
            float elapsed = 0f;

            Vector3 startPos = transform.position;

            while (elapsed < 1f)
            {

                elapsed += Time.deltaTime * 2;
                transform.position = Vector3.Lerp(startPos, new Vector3(targetPos.x, targetPos.y, targetPos.z), elapsed);
                yield return null;
            }

            transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
            isMoving = false;
        }
        else
        {
            yield return null;
        }
    }
}
