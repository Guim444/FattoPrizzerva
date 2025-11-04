using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PunchingState : IStateActions
{
    public float speed = 2f;
    public float staminaCostPunch = 5;
    public float baseDmg = 2;
    public float damageTimer = 0.5f;
    public PlayerController player;
    public SphereCollider punchCollider;
    public CharacterController controller;
    public PlayerStaminaManager stamina;

    public PunchingState(PlayerStaminaManager staminaManager)
    {
        stamina = staminaManager;
    }

    bool isMoving;
    public void Enter()
    {
        Debug.Log("Entered PunchingState State");
        player.staminaManager.ModifyStamina(-staminaCostPunch); // Example stamina cost for punching
        player.normalPunchTimer = damageTimer;
        player.GetComponent<SpriteRenderer>().color = Color.red; // Visual cue for punching state, just for testing
        punchCollider.enabled = true;
    }

    public void Update()
    {
        isMoving = player.GetDirectionalInput().magnitude > 0.1f;
        Vector3 input;
        if (isMoving)
        {
            stamina.SetWalking();
            input = player.GetDirectionalInput();
            if (controller.enabled == true)
            {
                controller.Move(input * speed * Time.deltaTime); //move the character based on input and speed, the same as moving state
            }
        }
        else
        {
            stamina.SetIdle();
            controller.Move(Vector3.zero);
        }
    }
    /*
    public void Punch()
    {
        Vector3 knockbackDirection = player.GetDirectionalInput().normalized;
        //if there's no input, we set a default knockback direction
        if (knockbackDirection == Vector3.zero)
        {
            knockbackDirection = player.transform.right;
        }
        //we get all the colliders in the punch range. Important: we set the punchCollider as a trigger collider.
        Collider[] hitColliders = Physics.OverlapSphere(punchCollider.transform.position, punchCollider.radius);
        foreach (var hitCollider in hitColliders)
        {
            //we check if the collider belongs to an object with tag "Enemy"
            if (hitCollider.CompareTag("Enemy"))
            {
                Debug.Log("Hit an enemy: " + hitCollider.name);
                //we try to get the EnemyController component from the hit object
                EnemyController enemy = hitCollider.GetComponent<EnemyController>();
                if (enemy != null)
                {
                }
            }
        }
    }*/
    

    public void Exit()
    {
        Debug.Log("Exited PunchingState State");
        player.GetComponent<SpriteRenderer>().color = Color.white; // Reset color on exit
        punchCollider.enabled = false;
    }
}