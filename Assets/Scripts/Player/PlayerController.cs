using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamageable, IKnockbackable
{
    // ============================================
    // COMPONENT REFERENCES
    // ============================================
    [Header("Component References")]
    public CharacterController cc;
    public Animator animator;
    public LayerMask interactMask;
    public Camera myCamera;
    public PlayerStaminaManager staminaManager;
    
    private IEstaminable stamina;
    private IAdrenalinable adrenaline;

    // ============================================
    // PLAYER PARAMETERS
    // ============================================
    [Header("Player Parameters")]
    public float normalSpeed = 7f;
    public float runningSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    public float HP { get; set; }
    public float basicPunchDamage, thrustDamage1, thrustDamage2, thrustDamage3;
    public float playerDamage; // The base damage that the player inflicts. It's used every time the player attacks.

    // ============================================
    // GAME DESIGN TUNING - MOVEMENT SPEEDS
    // ============================================

    [Header("Game Design: Movement Speeds")]
    [Tooltip("Walking speed (MovingState)")]
    public float walkingSpeed = 3f;
    [Tooltip("Running base speed (RunningState)")]
    public float runningBaseSpeed = 6f;
    [Tooltip("Running speed multiplier for phase 2")]
    public float runningPhase2Multiplier = 1.4f;
    [Tooltip("Running speed multiplier for phase 3")]
    public float runningPhase3Multiplier = 1.8f;
    [Tooltip("Tired state movement speed")]
    public float tiredSpeed = 1f;
    [Tooltip("Gliding state movement speed")]
    public float glidingSpeed = 3f;
    [Tooltip("Punching while moving speed")]
    public float punchingSpeed = 2f;
    [Tooltip("PunchRunning base speed")]
    public float punchRunningBaseSpeed = 6f;
    
    // ============================================
    // GAME DESIGN TUNING - STAMINA COSTS
    // ============================================
    [Header("Game Design: Stamina Costs")]
    [Tooltip("Stamina cost per second for running (phase 1)")]
    public float runningStaminaCostPhase1 = 10f;
    [Tooltip("Stamina cost per second for running (phase 2)")]
    public float runningStaminaCostPhase2 = 14f;
    [Tooltip("Stamina cost per second for running (phase 3)")]
    public float runningStaminaCostPhase3 = 18f;
    [Tooltip("Stamina cost for normal punch (phase 0)")]
    public float punchStaminaCostPhase0 = 2f;
    [Tooltip("Stamina cost for punch while running (phase 1)")]
    public float punchRunningStaminaCostPhase1 = 2f;
    [Tooltip("Stamina cost for punch while running (phase 2)")]
    public float punchRunningStaminaCostPhase2 = 4f;
    [Tooltip("Stamina cost for punch while running (phase 3)")]
    public float punchRunningStaminaCostPhase3 = 6f;
    
    // ============================================
    // GAME DESIGN TUNING - RUNNING THRUST SYSTEM
    // ============================================
    [Header("Game Design: Running Thrust System")]
    [Tooltip("Time required to reach next thrust phase (seconds)")]
    public float thrustPhaseThreshold = 1.5f;
    [Tooltip("Turn speed for inertia in phase 1-2")]
    public float runningTurnSpeedNormal = 4f;
    [Tooltip("Turn speed for inertia in phase 3 (less control)")]
    public float runningTurnSpeedPhase3 = 2f;
    [Tooltip("Turn speed for inertia when walking")]
    public float walkingTurnSpeed = 5f;
    [Tooltip("Turn speed for inertia when tired")]
    public float tiredTurnSpeed = 10f;
    [Tooltip("Turn speed for inertia when gliding")]
    public float glidingTurnSpeed = 5f;
    [Tooltip("Turn speed for inertia when punch-running")]
    public float punchRunningTurnSpeed = 4f;

    [Header("Z Boundaries")]
    public float minZ = 0f;
    public float maxZ = 100f;
    public float minScale = 0.3f;
    public float maxScale = 1f;

    // ============================================
    // STATE MANAGEMENT
    // ============================================
    internal State currentState = State.Idle;
    public bool canMove = true;
    public bool isGrounded;
    public bool isPunching;
    public bool isInRun;
    public bool canAttack = true;

    // ============================================
    // MOVEMENT & INPUT
    // ============================================
    [Header("Movement")]
    public Vector3 currentSpeed;
    public Vector3 lastDirection = Vector3.zero;
    public KeyCode lastVerticalKey = KeyCode.None;
    public KeyCode lastHorizontalKey = KeyCode.None;
    private Vector3 velocity;

    // ============================================
    // RING SYSTEM
    // ============================================
    [Header("Ring System")]
    public bool isInsideRing = false;
    [HideInInspector] public RingSlopeHandler ringSlopeHandler; // Reference to RingSlopeHandler component
    
    // Expose ring slope handler properties for compatibility
    public bool isOnSlope 
    { 
        get 
        { 
            if (ringSlopeHandler == null)
            {
                ringSlopeHandler = GetComponent<RingSlopeHandler>();
                if (ringSlopeHandler == null)
                {
                    ringSlopeHandler = gameObject.AddComponent<RingSlopeHandler>();
                }
            }
            return ringSlopeHandler.isOnSlope; 
        } 
    }
    
    public Transform ringCenter 
    { 
        get 
        { 
            if (ringSlopeHandler == null)
            {
                ringSlopeHandler = GetComponent<RingSlopeHandler>();
                if (ringSlopeHandler == null)
                {
                    ringSlopeHandler = gameObject.AddComponent<RingSlopeHandler>();
                }
            }
            return ringSlopeHandler != null ? ringSlopeHandler.ringCenter : null; 
        } 
    }

    // ============================================
    // COMBAT & DAMAGE
    // ============================================
    [Header("Combat")]
    public float normalPunchTimer = 0;
    public float normalPunchCooldown = 0.5f;
    public int damageBoost = 0;
    public int endurance = 0;
    public TypeOfDamage enduranceDistance = 0;
    
    // ============================================
    // GAME DESIGN TUNING - KNOCKBACK/PUSH FORCES
    // ============================================
    [Header("Game Design: Knockback Forces")]
    [Tooltip("Push force multiplier when player is pushed (PushOnlySelf)")]
    public float pushForceSelf = 10f;
    [Tooltip("Push force multiplier when mostly self (PushMostlySelf)")]
    public float pushForceMostlySelf = 5f;
    [Tooltip("Push force multiplier when both are pushed (PushBoth)")]
    public float pushForceBoth = 3.5f;
    [Tooltip("Push force multiplier when mostly other (PushMostlyOther)")]
    public float pushForceMostlyOther = 3f;
    [Tooltip("Push force multiplier when only other is pushed (PushOnlyOther)")]
    public float pushForceOnlyOther = 0f;
    [Tooltip("Push force multiplier for hardest push (PushOnlyOtherPlus)")]
    public float pushForceOtherPlus = 15f;
    [Tooltip("Minimum push force when sprint-punching (damageBoost >= 2)")]
    public float sprintPunchMinPush = 3f;
    [Tooltip("Base push multiplier applied to all push forces")]
    public float pushForceBaseMultiplier = 2f;

    // ============================================
    // KNOCKBACK
    // ============================================
    [Header("Knockback")]
    public bool hasKnockback = false;
    public Vector3 knockbackVelocity;
    [Tooltip("Knockback decay speed (higher = faster decay)")]
    public float knockbackDecaySpeed = 5f;
    [Tooltip("Minimum knockback velocity before stopping")]
    public float knockbackMinVelocity = 1f;

    // ============================================
    // INITIALIZATION
    // ============================================
    void Awake()
    {
        if (GenericBattleManager.instance != null)
        {
            GenericBattleManager.instance.battleIsActive = true;
        }

        HP = 2;
        
        // Get component references
        stamina = GetComponent<IEstaminable>();
        adrenaline = GetComponent<IAdrenalinable>();
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        staminaManager = GetComponent<PlayerStaminaManager>();
        ringSlopeHandler = GetComponent<RingSlopeHandler>();
        
        // If RingSlopeHandler doesn't exist, add it
        if (ringSlopeHandler == null)
        {
            ringSlopeHandler = gameObject.AddComponent<RingSlopeHandler>();
        }

        InitializeStateMachine();
    }

    void InitializeStateMachine()
    {
        var idle = new IdleState(staminaManager);
        idle.player = this;
        idle.controller = cc;

        var moving = new MovingState(staminaManager);
        moving.player = this;
        moving.controller = cc;

        var running = new RunningState(staminaManager);
        running.player = this;
        running.controller = cc;

        var tired = new TiredState(staminaManager);
        tired.player = this;
        tired.controller = cc;

        var punching = new PunchingState(staminaManager);
        punching.player = this;
        punching.controller = cc;
        punching.punchCollider = GetComponent<SphereCollider>();

        var punchRunning = new PunchRunningState(staminaManager);
        punchRunning.player = this;
        punchRunning.controller = cc;
        punchRunning.punchCollider = GetComponent<BoxCollider>();

        var knockedback = new KnockedbackState();
        knockedback.player = this;
        knockedback.controller = cc;

        var gliding = new GlidingState(staminaManager);
        gliding.player = this;
        gliding.controller = cc;

        StateMachine.AddState(State.Idle, idle);
        StateMachine.AddState(State.Moving, moving);
        StateMachine.AddState(State.Running, running);
        StateMachine.AddState(State.Tired, tired);
        StateMachine.AddState(State.Punching, punching);
        StateMachine.AddState(State.PunchRunning, punchRunning);
        StateMachine.AddState(State.Jumping, new JumpingState());
        StateMachine.AddState(State.Falling, new FallingState());
        StateMachine.AddState(State.PunchFalling, new PunchFallingState());
        StateMachine.AddState(State.Gliding, gliding);
        StateMachine.AddState(State.Knockedback, knockedback);
        StateMachine.AddState(State.Interacting, new InteractingState());

        StateMachine.SetState(State.Idle);
    }

    // ============================================
    // UPDATE LOOP
    // ============================================
    void Update()
    {
        UpdateScaleBasedOnZ();
        isGrounded = cc.isGrounded;

        HandleKnockback();
        HandleStateTransitions();
        StateMachine.Update();
        UpdatePunchCooldown();
    }

    void HandleKnockback()
    {
        if (hasKnockback)
        {
            if (cc.enabled)
            {
                cc.Move(knockbackVelocity * Time.deltaTime);
                knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecaySpeed * Time.deltaTime);

                // Reset inertia
                lastDirection = Vector3.zero;
                currentSpeed = Vector3.zero;
            }

            if (knockbackVelocity.magnitude < knockbackMinVelocity)
            {
                hasKnockback = false;
                currentState = State.Knockedback;
                StateMachine.SetState(State.Idle);
            }
            return;
        }
        else
        {
            // Apply knockback movement
            if (cc.enabled == true)
            {
                cc.Move(knockbackVelocity * Time.deltaTime);
            }
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecaySpeed * Time.deltaTime);
            
            if (knockbackVelocity.magnitude < 0.1f)
            {
                hasKnockback = false;
            }
        }
    }

    void HandleStateTransitions()
    {
        if (normalPunchTimer > 0)
        {
            normalPunchTimer -= Time.deltaTime;
            return; // Don't change state during punch
        }

        if (!canMove)
        {
            currentState = State.Idle;
            StateMachine.SetState(currentState);
            return;
        }

        // Use PlayerStateHelper to determine new state
        State newState = PlayerStateHelper.DetermineState(staminaManager, isOnSlope, currentState);

        // GLIDING SYSTEM (COMMENTED OUT - TO BE ENABLED AFTER TELEPORTATION TESTING)
        // Change state only if different and not in GlidingState (unless exiting slope)
        // if (newState != currentState && !(isOnSlope && currentState == State.Gliding))
        if (newState != currentState)
        {
            currentState = newState;
            StateMachine.SetState(currentState);

            if (!(currentState == State.Running || currentState == State.PunchRunning))
            {
                damageBoost = 0;
            }
        }
    }

    void UpdatePunchCooldown()
    {
        if (normalPunchCooldown > 0)
        {
            normalPunchCooldown -= Time.deltaTime;
        }
    }

    // ============================================
    // INPUT HANDLING
    // ============================================
    public Vector3 GetDirectionalInput()
    {
        Vector3 move = Vector3.zero;
        CalcMovePriority();

        if ((Input.GetKey(KeyCode.W)) || Input.GetKey(KeyCode.S))
        {
            if (lastVerticalKey == KeyCode.W) move += Vector3.forward;
            else if (lastVerticalKey == KeyCode.S) move += Vector3.back;
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (lastHorizontalKey == KeyCode.D) move += Vector3.right;
            else if (lastHorizontalKey == KeyCode.A) move += Vector3.left;
        }

        if (move.magnitude > 0)
        {
            currentSpeed = move.normalized;
        }
        else
        {
            currentSpeed = move.normalized;
        }
        
        if (!hasKnockback)
        {
            FlipCharacter(lastDirection);
        }

        return currentSpeed;
    }

    void CalcMovePriority()
    {
        // Calculate horizontal priority
        if (Input.GetKeyDown(KeyCode.A)) lastHorizontalKey = KeyCode.A;
        if (Input.GetKeyDown(KeyCode.D)) lastHorizontalKey = KeyCode.D;
        if (Input.GetKeyUp(KeyCode.A) && lastHorizontalKey == KeyCode.A)
            lastHorizontalKey = Input.GetKey(KeyCode.D) ? KeyCode.D : KeyCode.None;
        if (Input.GetKeyUp(KeyCode.D) && lastHorizontalKey == KeyCode.D)
            lastHorizontalKey = Input.GetKey(KeyCode.A) ? KeyCode.A : KeyCode.None;

        // Calculate vertical priority
        if (Input.GetKeyDown(KeyCode.W)) lastVerticalKey = KeyCode.W;
        if (Input.GetKeyDown(KeyCode.S)) lastVerticalKey = KeyCode.S;
        if (Input.GetKeyUp(KeyCode.W) && lastVerticalKey == KeyCode.W)
            lastVerticalKey = Input.GetKey(KeyCode.S) ? KeyCode.S : KeyCode.None;
        if (Input.GetKeyUp(KeyCode.S) && lastVerticalKey == KeyCode.S)
            lastVerticalKey = Input.GetKey(KeyCode.W) ? KeyCode.W : KeyCode.None;
    }

    // ============================================
    // MOVEMENT UTILITIES
    // ============================================
    public Vector3 ApplyInertia(Vector3 inputDir, float deltaTime, float turnSpeed)
    {
        if (inputDir.magnitude > 0.01f)
            inputDir.Normalize();

        lastDirection = Vector3.MoveTowards(lastDirection, inputDir, turnSpeed * deltaTime);

        if (lastDirection.magnitude < 0.01f)
            lastDirection = Vector3.zero;

        return lastDirection;
    }

    public void FlipCharacter(Vector3 lastDir)
    {
        if (Mathf.Abs(lastDir.x) < 0.01f) return;

        float currentSign = Mathf.Sign(transform.localScale.x);
        float desiredSign = Mathf.Sign(lastDir.x);

        if (currentSign != desiredSign)
            transform.localScale = new Vector3(-transform.localScale.x, 0, transform.localScale.z);
    }

    public void UpdateScaleBasedOnZ()
    {
        float z = transform.position.z;
        float t = Mathf.InverseLerp(minZ, maxZ, z);
        t = Mathf.Clamp01(t);
        float scaleFactor = Mathf.Lerp(maxScale, minScale, t);
        float signX = Mathf.Sign(transform.localScale.x);
        transform.localScale = new Vector3(scaleFactor * signX, scaleFactor, scaleFactor);
    }

    // ============================================
    // RING SLOPE SYSTEM (Delegates to RingSlopeHandler)
    // ============================================
    public void EnterSlopeZone(Transform center)
    {
        // Ensure RingSlopeHandler exists
        if (ringSlopeHandler == null)
        {
            ringSlopeHandler = GetComponent<RingSlopeHandler>();
            if (ringSlopeHandler == null)
            {
                ringSlopeHandler = gameObject.AddComponent<RingSlopeHandler>();
            }
        }
        
        if (ringSlopeHandler != null)
        {
            ringSlopeHandler.EnterSlopeZone(center);
            
            // Transition to GlidingState when entering slope
            if (currentState != State.Gliding && canMove)
            {
                currentState = State.Gliding;
                StateMachine.SetState(State.Gliding);
            }
        }
    }

    public void ExitSlopeZone()
    {
        // Ensure RingSlopeHandler exists
        if (ringSlopeHandler == null)
        {
            ringSlopeHandler = GetComponent<RingSlopeHandler>();
            if (ringSlopeHandler == null)
            {
                ringSlopeHandler = gameObject.AddComponent<RingSlopeHandler>();
            }
        }
        
        if (ringSlopeHandler != null)
        {
            ringSlopeHandler.ExitSlopeZone();
            
            // Exit GlidingState and return to appropriate state
            if (currentState == State.Gliding && canMove)
            {
                State newState = PlayerStateHelper.DetermineStateFromInput(staminaManager);
                currentState = newState;
                StateMachine.SetState(newState);
            }
        }
    }

    public Vector3 RestrictToSlopeDirection(Vector3 inputDirection)
    {
        // Ensure RingSlopeHandler exists
        if (ringSlopeHandler == null)
        {
            ringSlopeHandler = GetComponent<RingSlopeHandler>();
            if (ringSlopeHandler == null)
            {
                ringSlopeHandler = gameObject.AddComponent<RingSlopeHandler>();
            }
        }
        
        if (ringSlopeHandler != null)
        {
            return ringSlopeHandler.RestrictToSlopeDirection(inputDirection);
        }
        return inputDirection;
    }

    // ============================================
    // COMBAT
    // ============================================
    public void PushForce(Vector3 direction, int enemyEndurance)
    {
        enduranceDistance = (TypeOfDamage)(endurance - enemyEndurance + 2);

        if (enduranceDistance < 0) enduranceDistance = TypeOfDamage.PushOnlyOtherPlus;
        else if ((int)enduranceDistance > 4) enduranceDistance = (TypeOfDamage)4;

        float pushMultiplier = 0;
        switch (enduranceDistance)
        {
            case TypeOfDamage.PushOnlySelf:
                pushMultiplier = pushForceSelf;
                break;
            case TypeOfDamage.PushMostlySelf:
                pushMultiplier = pushForceMostlySelf;
                break;
            case TypeOfDamage.PushBoth:
                pushMultiplier = pushForceBoth;
                break;
            case TypeOfDamage.PushMostlyOther:
                pushMultiplier = pushForceMostlyOther;
                break;
            case TypeOfDamage.PushOnlyOther:
                pushMultiplier = pushForceOnlyOther;
                break;
            case TypeOfDamage.PushOnlyOtherPlus:
                pushMultiplier = pushForceOtherPlus;
                break;
        }

        if (damageBoost >= 2)
        {
            pushMultiplier = Mathf.Max(pushMultiplier, sprintPunchMinPush);
        }

        damageBoost = 0;

        if (pushMultiplier > 0)
        {
            knockbackVelocity = direction.normalized * pushMultiplier * pushForceBaseMultiplier;
            hasKnockback = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (normalPunchCooldown <= 0 && normalPunchTimer > 0)
        {
            Vector3 knockbackDirection = GetDirectionalInput().normalized;
            if (knockbackDirection == Vector3.zero)
            {
                knockbackDirection = Vector3.right;
                knockbackDirection.x = Mathf.Sign(transform.localScale.x);
                knockbackDirection.y = 0;
            }
            
            if (other.CompareTag("Enemy"))
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.PushForce(knockbackDirection, endurance, gameObject);
                    PushForce(-knockbackDirection, enemy.endurance);
                    normalPunchTimer = 0;
                }
            }
        }
    }

    // ============================================
    // DAMAGE & DEATH
    // ============================================
    public void TakeDamage(int dmg)
    {
        HP = Mathf.Max(0, HP - dmg);
        if (HP == 0)
        {
            Die();
        }
    }

    public void Die()
    {
        canMove = false;
        animator.speed = 1;
        animator.SetBool("isDead", true);
        GenericBattleManager.instance.battleIsActive = false;
    }

    // ============================================
    // INPUT SYSTEM CALLBACKS
    // ============================================
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        // TO DO: Implement jump
    }

    public void OnPunch(InputAction.CallbackContext ctx)
    {
        // TO DO: Implement punch input
    }

    public void OnRun(InputAction.CallbackContext ctx)
    {
        isInRun = ctx.ReadValue<float>() > 0.5f;
    }

    public void OnAction(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
            TryInteract();
    }

    void TryInteract()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, transform.forward, out var hit, 3f, interactMask))
        {
            // TO DO: Implement interaction logic
        }
    }

    // ============================================
    // UTILITY METHODS
    // ============================================
    public void ApplyGravity(float dt)
    {
        velocity.y += gravity * dt;
    }

    public void BeginCooldown(float duration)
    {
        // TO DO: Implement cooldown system
    }
}
