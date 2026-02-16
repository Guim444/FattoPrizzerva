using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCourse : MonoBehaviour
{
    public Vector3 offset;
    public Vector2Int courseDirection;
    public List<KnightsSquareScript> waterCourseSquares = new List<KnightsSquareScript>();
    public bool dryCourse = false;
    public int dryTurns = 0, activeTurns;
    public int length;

    bool isMoving;

    private void OnEnable()
    {
        offset = transform.position;
    }

    private void OnMouseDown()
    {
        if (MapEditorData.instance.editMode && MapEditorData.instance.selectedObject == null)
        {
            MapEditorData.instance.selectedObject = gameObject;
        }
    }
    public IEnumerator MoveCourse(Vector3 targetPos)
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
