using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class LArrow : MonoBehaviour
{
    SpriteRenderer sr;
    float alpha;
    public KnightsSquareScript target;
    public KnightBehavior knight;

    public Dictionary<BoxCollider, Vector3> boxColliders;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        alpha = sr.color.a;

        boxColliders = new Dictionary<BoxCollider, Vector3>();
        foreach (BoxCollider bc in GetComponents<BoxCollider>())
        {
            boxColliders.Add(bc, bc.center);
        }
    }
    private void OnMouseDown()
    {
        KnightsGameManager.instance.CallMoveCoroutine(knight, target);
        target.ToggleGlow(false, 1f);
        knight.Deselect();
    }
    private void OnMouseEnter()
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        target.ToggleGlow(true, 1f);
    }
    private void OnMouseExit()
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        target.ToggleGlow(false, 1f);
    }
}
