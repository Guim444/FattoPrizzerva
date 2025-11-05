using UnityEngine;

public abstract class EnemyController : MonoBehaviour, IDamageable
{
    public int endurance;
    public TypeOfDamage enduranceDistance = 0;
    public Vector3 knockbackDirection;
    public CharacterController controller;
    public PlayerController player;

    public Vector3 knockbackSpeed;
    // Update is called once per frame
    protected virtual void Update()
    {
        if (knockbackSpeed.magnitude > 0.1f)
        {
            controller.Move(knockbackSpeed * Time.deltaTime);
            // Gradually reduce knockback speed over time
            knockbackSpeed = Vector3.Lerp(knockbackSpeed, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            FollowPlayerLogic(); // Call the method to follow the player when not being knocked back. I put it here to avoid having to put it in every enemy script if they have different movement logic.
        }
    }
    public abstract void TakeDamage(); // Implement specific damage logic in derived classes
    public abstract void FollowPlayerLogic(); // Implement specific player following logic in derived classes
    public void PushForce(Vector3 direction, int playerEndurance)
    {
        enduranceDistance = (TypeOfDamage)(endurance - playerEndurance + 2); //we add 2 to align the enum values with endurance distance values.
        Debug.Log(enduranceDistance);
        float pushMultiplier = 0;
        switch (enduranceDistance)
        {
            case TypeOfDamage.PushOnlySelf:
                //push the enemy at the knockback direction
                pushMultiplier = 10f;
                break;
            case TypeOfDamage.PushMostlySelf:
                //75% push enemy, 25% push player
                pushMultiplier = 7.5f;
                break;
            case TypeOfDamage.PushBoth:
                //50% push enemy, 50% push player
                pushMultiplier = 5f;
                break;
            case TypeOfDamage.PushMostlyOther:
                //25% push enemy, 75% push player
                pushMultiplier = 2.5f;
                break;
            case TypeOfDamage.PushOnlyOther:
                //do not push enemy
                pushMultiplier = 0f;
                break;
        }
        knockbackSpeed = direction.normalized * pushMultiplier * 2f;
    }
}
