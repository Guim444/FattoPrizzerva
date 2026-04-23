using UnityEngine;
using System.Collections.Generic;
using System;

public class SnakeBody : MonoBehaviour
{
    public List<SnakePart> bodyParts = new List<SnakePart>();
    public Vector3 direction;
    public SnakeEffectType effectType;

    private void OnEnable()
    {
        bodyParts.AddRange(GetComponentsInChildren<SnakePart>());
    }
    public void RecalcDirection()
    {
        direction = bodyParts[0].transform.position - bodyParts[1].transform.position;
    }
    public bool CheckAll()
    {
        foreach (SnakePart part in bodyParts)
        {
            bool ok = part.CheckSquareUnder();

            if (!ok)
            {
                return false;
            }
        }
        return true;
    }

    public void SnakeEffect(KnightBehavior knight)
    {
        if (effectType == SnakeEffectType.Push)
        {
            Vector2Int pushDir = -knight.lookingDirection;
            var origin = knight.currentSquare;

            char col = (char)(origin.SquareColumn + pushDir.x);
            int row = origin.SquareRow + pushDir.y;

            var target = KnightsBoardManager.instance.GetSquare(col.ToString() + row);

            if (target != null && target.snake != null)
            {
                Vector2Int[] perpendiculars =
                {
                new Vector2Int(pushDir.y, -pushDir.x),
                new Vector2Int(-pushDir.y, pushDir.x)
            };

                foreach (var dir in perpendiculars)
                {
                    char c = (char)(origin.SquareColumn + dir.x);
                    int r = origin.SquareRow + dir.y;

                    var alt = KnightsBoardManager.instance.GetSquare(c.ToString() + r);

                    if (alt != null && alt.snake == null)
                    {
                        pushDir = dir;
                        break;
                    }
                }
            }

            knight.StartCoroutine(knight.PushForce(knight, pushDir, 1, allowIce: true));
        }
        else if (effectType == SnakeEffectType.Eat)
        {
            //TO DO
        }
    }

    public void SetAllSquares()
    {
        for (int i = 1; i < bodyParts.Count; i++)
        {
            Vector3 relativeDir = bodyParts[i].transform.position - bodyParts[i - 1].transform.position;
            Vector2Int dir2D = new Vector2Int(Mathf.RoundToInt(relativeDir.x), Mathf.RoundToInt(relativeDir.z));

            char col = (char)(bodyParts[i - 1].currentSquare.SquareColumn + dir2D.y);
            int row = bodyParts[i - 1].currentSquare.SquareRow - dir2D.x;
            string sqName = col.ToString() + row.ToString();
            Debug.Log(sqName);

            bodyParts[i].currentSquare = KnightsBoardManager.instance.GetSquare(sqName);
        }

        foreach (SnakePart part in bodyParts)
        {
            part.currentSquare.snake = this;
        }
    }
    public void FlipSnake()
    {
        Vector3 pivot = bodyParts[0].transform.position;
        Vector3 forward = direction.normalized;

        Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3[] originalPositions = new Vector3[bodyParts.Count];
        for (int i = 0; i < bodyParts.Count; i++)
        {
            originalPositions[i] = bodyParts[i].transform.position;
        }

        for (int i = 1; i < bodyParts.Count; i++)
        {
            Vector3 offset = bodyParts[i].transform.position - pivot;

            float forwardDist = Vector3.Dot(offset, forward);
            float sideDist = Vector3.Dot(offset, side);

            sideDist *= -1;

            Vector3 mirroredOffset = forward * forwardDist + side * sideDist;
            bodyParts[i].transform.position = pivot + mirroredOffset;
        }

        RecalcDirection();
    }
    public void RecalcPositions()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].AssignSquare();
        }
    }

    internal void ToggleGlow(bool glow, int intensity)
    {
        foreach (SnakePart part in bodyParts)
        {
            part.ToggleGlow(glow, intensity);
        }
    }
}
public enum SnakeEffectType
{
    Push,
    Eat
}