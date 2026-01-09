using UnityEngine;

public class TucutuKnight : KnightBehavior
{
    protected override void Awake()
    {
        base.Awake();
        movementType = false;
        grounded = true;
    }
    protected override void OnArrive(KnightsSquareScript square)
    {
    }
}
