using UnityEngine;

public abstract class EnemyController : MonoBehaviour, IDamageable
{
    public int endurance;
    public TypeOfDamage enduranceDistance = 0;
    public Vector3 knockbackDirection;
    public CharacterController controller;

    public Vector3 knockbackSpeed;
    // Update is called once per frame
    void Update()
    {
        if (knockbackSpeed.magnitude > 0.1f)
        {
            controller.Move(knockbackSpeed * Time.deltaTime);
            // Gradually reduce knockback speed over time
            knockbackSpeed = Vector3.Lerp(knockbackSpeed, Vector3.zero, 5f * Time.deltaTime);
        }
    }
    public abstract void TakeDamage();
    public void PushForce(Vector3 direction, int playerEndurance)
    {
        enduranceDistance = (TypeOfDamage)(endurance - playerEndurance + 2); //we add 2 to align the enum values with endurance distance values.
        Debug.Log(enduranceDistance);
        float pushMultiplier = 0;
        switch (enduranceDistance)
        {
            case TypeOfDamage.PushOnlySelf:
                //push the enemy at the knockback direction
                pushMultiplier = 1f;
                break;
            case TypeOfDamage.PushMostlySelf:
                //75% push enemy, 25% push player
                pushMultiplier = 0.75f;
                break;
            case TypeOfDamage.PushBoth:
                //50% push enemy, 50% push player
                pushMultiplier = 0.5f;
                break;
            case TypeOfDamage.PushMostlyOther:
                //25% push enemy, 75% push player
                pushMultiplier = 0.25f;
                break;
            case TypeOfDamage.PushOnlyOther:
                //do not push enemy
                pushMultiplier = 0f;
                break;
        }
        knockbackSpeed = direction.normalized * pushMultiplier * 10f;
    }
}
