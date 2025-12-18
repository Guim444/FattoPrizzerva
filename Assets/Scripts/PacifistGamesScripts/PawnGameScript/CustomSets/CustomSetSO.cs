using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu (fileName = "CustomSet", menuName = "Custom Pawn Set", order = 1)]
public class CustomSetSO : ScriptableObject
{
    public bool assigned = false;
    public List<int> possibleMovesTier1, possibleMovesTier2, possibleMovesTier3, killRangeTier1, killRangeTier2, killRangeTier3;
}
