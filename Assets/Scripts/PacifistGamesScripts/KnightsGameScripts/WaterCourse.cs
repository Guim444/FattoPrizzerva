using UnityEngine;
using System.Collections.Generic;

public class WaterCourse : MonoBehaviour
{
    public bool intermitent = false;
    public Vector2Int courseDirection;
    public List<KnightsSquareScript> waterCourseSquares = new List<KnightsSquareScript>();
    public bool dryCourse = false;

    private void OnEnable()
    {
        foreach (KnightsSquareScript sq in waterCourseSquares)
        {
            if (sq.waterCourseDirection == Vector2Int.zero)
                sq.waterCourseDirection = courseDirection;
            else
            {
                sq.isWaterCourseCrossing = true;
                sq.waterCourseDirection = Vector2Int.zero;
            }
        }
    }
}
