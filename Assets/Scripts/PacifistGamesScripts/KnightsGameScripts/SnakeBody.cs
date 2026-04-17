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
            Vector2Int pushDir = new Vector2Int((int)direction.x, (int)direction.z);
            knight.StartCoroutine(knight.PushForce(knight, pushDir, 1, allowIce: true));
        }
        else if (effectType == SnakeEffectType.Eat)
        {
            //TO DO
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