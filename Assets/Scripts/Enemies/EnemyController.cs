using UnityEngine;

public abstract class EnemyController : MonoBehaviour, IDamageable
{

    public float minZ, maxZ; // Z boundaries for scaling
    public float minScale, maxScale; // Scale values

    public Vector3 lastDir = Vector3.zero; // Last movement direction. Zero by default, but will be set when moving. Used for knockback direction and facing.

    public int endurance; // Enemy's endurance stat. It can change with phase changes of the boss.
    public TypeOfDamage enduranceDistance = 0; // Calculated based on endurance comparison. Zero by default, but will be set when taking damage.
    public Vector3 knockbackDirection; // Direction of the knockback applied to the enemy
    public CharacterController controller; // Reference to the CharacterController component
    public PlayerController player; // Reference to the player

    public CapsuleCollider hitCollider; // Collider to damage the player on contact
    public float hitTimer = 0, maxHitTimer; // Time between hits to the player

    public Vector3 knockbackSpeed;
    // Update is called once per frame
    protected virtual void Update()
    {

        UpdateScaleBasedOnZ();

        if (knockbackSpeed.magnitude > 0.1f)
        {
            controller.Move(knockbackSpeed * Time.deltaTime);
            // Gradually reduce knockback speed over time
            knockbackSpeed = Vector3.Lerp(knockbackSpeed, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            knockbackSpeed = Vector3.zero;

            Vector3 input = player.transform.position - transform.position;
            lastDir = new Vector3(input.x, 0, input.z).normalized;

            FollowPlayerLogic(); // Call the method to follow the player when not being knocked back. I put it here to avoid having to put it in every enemy script if they have different movement logic.
            FlipCharacter(lastDir);
        }

        if (hitTimer > 0)
        {
            hitTimer -= Time.deltaTime;
        }
        else
        {
            hitCollider.enabled = true;
        }
    }

    public void UpdateScaleBasedOnZ()
    {
        float z = transform.position.z;

        // Convert z into a 0–1 range between minZ and maxZ
        float t = Mathf.InverseLerp(minZ, maxZ, z);
        t = Mathf.Clamp01(t);

        // Lerp between maxScale (near) and minScale (far)
        float scaleFactor = Mathf.Lerp(maxScale, minScale, t);

        // Preserve the original sign of the x scale to maintain facing direction
        float signX = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(scaleFactor * signX, scaleFactor, scaleFactor);
    }
    void FlipCharacter(Vector3 lastDir)
    {
        if (Mathf.Abs(lastDir.x) < 0.01f) return; // if there's no horizontal input, do nothing

        // Check the current facing direction and compare with desired direction
        float currentSign = Mathf.Sign(transform.localScale.x);
        float desiredSign = Mathf.Sign(lastDir.x);

        // If they differ, flip the character by inverting the x scale
        if (currentSign != desiredSign)
            transform.localScale = new Vector3(-transform.localScale.x, 0, transform.localScale.z);
    }
    public abstract void TakeDamage(); // Implement specific damage logic in derived classes
    public abstract void FollowPlayerLogic(); // Implement specific player following logic in derived classes
    public void PushForce(Vector3 direction, int playerEndurance)
    {
        enduranceDistance = (TypeOfDamage)Mathf.Clamp(endurance - playerEndurance + 2, 0, 4); //we add 2 to align the enum values with endurance distance values.
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
                pushMultiplier = 5f;
                break;
            case TypeOfDamage.PushBoth:
                //50% push enemy, 50% push player
                pushMultiplier = 3.5f;
                break;
            case TypeOfDamage.PushMostlyOther:
                //25% push enemy, 75% push player
                pushMultiplier = 3f;
                break;
            case TypeOfDamage.PushOnlyOther:
                //do not push enemy
                pushMultiplier = 0f;
                break;
        }
        knockbackSpeed = direction.normalized * pushMultiplier * 2f;
    }
}
