using UnityEngine;

public interface IKnockbackable
{
    public void PushForce(Vector3 direction, int otherEndurance);
}
