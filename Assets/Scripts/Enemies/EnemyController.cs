using System.Collections;
using TMPro;
using Unity.VisualScripting;
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

    public int enemyPhase = 0;
    public float attackTime; //Used to know when the enemy is ready to attack.

    public float HP { get; set; }

    //Animator controllers
    [Header ("Animator controllers")] public bool isAttacking, isMoving, hasKnockback, damageCondition;
    public Animator animator;

    // Update is called once per frame
    protected virtual void Update()
    {
        UpdateScaleBasedOnZ();

        if (player.battleIsActive)
        {
            if (knockbackSpeed.magnitude > 0.1f)
            {
                controller.Move(knockbackSpeed * Time.deltaTime);
                // Gradually reduce knockback speed over time
                knockbackSpeed = Vector3.Lerp(knockbackSpeed, Vector3.zero, 5f * Time.deltaTime);
                hasKnockback = true;
            }
            else
            {
                knockbackSpeed = Vector3.zero;
                hasKnockback = false;

                if (!isAttacking)
                {
                    isMoving = true;
                    Vector3 input = player.transform.position - transform.position;
                    lastDir = new Vector3(input.x, 0, input.z).normalized;

                    FlipCharacter(lastDir);
                    FollowPlayerLogic();
                }
            }

            UpdateAttackTimer();

            if (hitTimer > 0)
            {
                hitTimer -= Time.deltaTime;
            }
            else
            {
                hitCollider.enabled = true;
            }
        }
        else
        {
            isMoving = false;
            isAttacking = false;
            hasKnockback = false;
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
    public void FlipCharacter(Vector3 lastDir)
    {
        if (Mathf.Abs(lastDir.x) < 0.01f) return; // if there's no horizontal input, do nothing

        // Check the current facing direction and compare with desired direction
        float currentSign = Mathf.Sign(transform.localScale.x);
        float desiredSign = Mathf.Sign(lastDir.x);

        // If they differ, flip the character by inverting the x scale
        if (currentSign != desiredSign)
            transform.localScale = new Vector3(-transform.localScale.x, 0, transform.localScale.z);
    }
    public abstract void FollowPlayerLogic(); // Implement specific player following logic in derived classes
    public void PushForce(Vector3 direction, int playerEndurance, GameObject attacker)
    {
        enduranceDistance = (TypeOfDamage)(endurance - playerEndurance + 2);

        if (enduranceDistance < 0) enduranceDistance = TypeOfDamage.PushOnlyOtherPlus;
        else if ((int)enduranceDistance > 4) enduranceDistance = (TypeOfDamage)4;

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
            case TypeOfDamage.PushOnlyOtherPlus:
                pushMultiplier = 15f;
                break;
        }
        knockbackSpeed = direction.normalized * pushMultiplier * 2f;

        if ((int)enduranceDistance <= 2)
        {
            attackTime = Random.Range(3, 6);
        }

        if (attacker.CompareTag("Player"))
        {
            TakeDamage((int)player.playerDamage);
        }
    }
    public abstract void ChangeBossPhase();
    void UpdateAttackTimer()
    {
        if (attackTime <= 0)
        {
            isAttacking = true;
            Attack();
        }
        else
        {
            attackTime -= Time.deltaTime;
        }
    }
    public void Die()
    {
    }

    public void TakeDamage(int dmg)
    {
        //Here will be coded the common behaviours between enemies when damaged.
        DamagedBehaviour(dmg);
    }
    public abstract void Attack();
    public abstract void DamagedBehaviour(int dmg); //Specific enemy damage calcs
}
