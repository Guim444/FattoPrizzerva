using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
/*
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(IEstaminable))]
[RequireComponent(typeof(IAdrenalinable))] */
  

public class PlayerController : MonoBehaviour, IDamageable, IKnockbackable
    {
       
        Vector3 velocity; //3D vector to store the current movement speed and direction of the character.
    [Header("Player Parameters")]
    public float normalSpeed = 7f, runningSpeed = 10f, gravity = -9.81f, jumpForce = 5f;
    [Header ("Z Boundaries")] public float minZ = 0f, maxZ = 100f, minScale = 0.3f, maxScale = 1f; //Declared Floats

    public bool isGrounded, isPunching, isInRun;
    bool flip; //To know if the player is facing left or right. Left is true, right is false.
    CharacterController cc;  //Built-in component called for handling character movements & collisions withour Rigidbody physics
        public Animator animator; //Built-in component called for playing animations from code
        public LayerMask interactMask;   //A filter that will tell the raycast which layers of objects it should detect when the player tries to interact to avoid hitting everything
        public Camera myCamera;
        
        IEstaminable stamina;  //Interface representing everything related to Stamina usage (CurrentStamina, Consume(), Recover(), etc.)
        IAdrenalinable adrenaline;   //Same with adrenaline
    /*public UnityEvent OnEnterPlantZone, OnExitPlantZone; // Event to assign inside the Unity Editor for when the player gets inside a plant */

    // StateMachine<State> sm; //A custom state machine object where <State> will be one of the different enums stated below 
    internal State currentState = State.Idle; //A variable that stores the current state

    public PlayerStaminaManager staminaManager; // Reference to the PlayerStaminaManager component

    public Vector3 currentSpeed, lastDirection = Vector3.zero; // currentSpeed is the normalized direction of movement, lastDirection is used for saving the last movement direction for inertia calculations or animations.

    public bool isInsideRing = false; // To check if the player is inside the ring area

    public bool hasKnockback = false;
    public Vector3 knockbackVelocity; // Velocity applied during knockback

    public float normalPunchTimer = 0; // Timer to control when the punch can deal damage again
    public float normalPunchCooldown = 0.5f; // Cooldown duration between punches

    public int damageBoost = 0; // This value is used to increase the damage dealt by the player when is punching while running. Zero by default.
    public int endurance = 0; // Player's endurance level, affects knockback resistance

    public TypeOfDamage enduranceDistance = 0; // Variable to hold the type of damage based on endurance

    void Awake()
        {
            stamina = GetComponent<IEstaminable>();
            adrenaline = GetComponent<IAdrenalinable>();
            cc = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            staminaManager = GetComponent<PlayerStaminaManager>();

        var idle = new IdleState(staminaManager);
        idle.player = this;
        idle.controller = GetComponent<CharacterController>();

        var moving = new MovingState(staminaManager);
        moving.player = this;
        moving.controller = GetComponent<CharacterController>();

        var running = new RunningState(staminaManager);
        running.player = this;
        running.controller = GetComponent<CharacterController>();

        var tired = new TiredState(staminaManager);
        tired.player = this;
        tired.controller = GetComponent<CharacterController>();

        var punching = new PunchingState(staminaManager);
        punching.player = this;
        punching.controller = GetComponent<CharacterController>();
        punching.punchCollider = GetComponent<SphereCollider>();

        var punchRunning = new PunchRunningState(staminaManager);
        punchRunning.player = this;
        punchRunning.controller = GetComponent<CharacterController>();
        punching.punchCollider = GetComponent<SphereCollider>();

        var knockedback = new KnockedbackState();
        knockedback.player = this;
        knockedback.controller = GetComponent<CharacterController>();


        StateMachine.AddState(State.Idle, idle);
            StateMachine.AddState(State.Moving, moving);
            //global: :State means: “Use the enum called State that exists in the global namespace (outside of any class/namespace), not something else that also happens to be called State.”
            StateMachine.AddState(State.Running, running);
            StateMachine.AddState(State.Tired, tired);
            StateMachine.AddState(State.Punching, punching);
            StateMachine.AddState(State.PunchRunning, punchRunning);
            StateMachine.AddState(State.Jumping, new JumpingState());
            StateMachine.AddState(State.Falling, new FallingState());
            StateMachine.AddState(State.PunchFalling, new PunchFallingState());
            StateMachine.AddState(State.Gliding, new GlidingState());
            StateMachine.AddState(State.Knockedback, new KnockedbackState());
            StateMachine.AddState(State.Interacting, new InteractingState());

        StateMachine.SetState(State.Idle);
        }

        void Update()
        {
            UpdateScaleBasedOnZ();
            isGrounded = cc.isGrounded; //Uses the character controller's built-in ground detection

       // Debug.Log(transform.position.z);


        if (!hasKnockback)
        {
            if (isGrounded && velocity.y < 0) velocity.y = -2f; //When player touches the floor and falling in any speed (<0) it makes it -2 to stick the player on the floor 
                                                                //StateMachine.Tick(Time.deltaTime);      //Updates the current state logic
            if (cc.enabled == true) cc.Move(velocity * Time.deltaTime);  //Applies accumulated velocity
        }
        else
        {
            // Apply knockback movement
            if (cc.enabled == true) cc.Move(knockbackVelocity * Time.deltaTime); knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 5f * Time.deltaTime);
            //when lerp ends, we stop the knockback
            if (knockbackVelocity.magnitude < 0.1f)
            {
                //end knockback
                hasKnockback = false;
            }
        }


        State newState;


        if (normalPunchTimer > 0) // During attacks, we don't change state until the punch timer ends
        {
            normalPunchTimer -= Time.deltaTime;
        }
        else
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) // Movement input detected
            {
                if (staminaManager.isTired)
                {
                    newState = State.Tired;
                }
                else if (Input.GetKey(KeyCode.LeftShift) && staminaManager.currentStamina > 0) // Running input detected and enough stamina
                {
                    newState = State.Running;
                    if (Input.GetMouseButton(0))
                    {
                        newState = State.PunchRunning; // Punching while running
                    }
                }
                else
                {
                    newState = State.Moving;
                    if (Input.GetMouseButtonDown(0))
                    {
                        newState = State.Punching; // Punching while moving
                    }
                }
            }
            else
            {
                newState = State.Idle;
                if (Input.GetMouseButton(0))
                {
                    newState = State.Punching; // Punching while idle
                }
            }

            // Change state only if different
            if (newState != currentState)
            {
                currentState = newState;
                StateMachine.SetState(currentState);
            }
        }
        // Let the state run its own Update logic
        StateMachine.Update();

        if (normalPunchCooldown > 0)
        {
            normalPunchCooldown -= Time.deltaTime; // Reduces the cooldown timer each frame
        }
    }

    public Vector3 GetDirectionalInput()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;   // Z+
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;      // Z-
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;     // X+
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;      // X-

        if (move.magnitude > 0)
        {
            lastDirection = move.normalized; // Update lastDirection only when there's movement input
        }
        currentSpeed = move.normalized;
        if (!hasKnockback)
        {
            FlipCharacter(lastDirection);
        }

        return currentSpeed;
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

    // Formula for scaling based on Z
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

    public void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 dir = ctx.ReadValue<Vector2>(); //Reads the movement joystick/ keys(Vector2).
         // _stateMachine.SetContext("move", dir);  //Sends this direction as a “context” to the state machine so the MovingState knows how to move.
        }

        public void OnJump(InputAction.CallbackContext ctx)
        {
            //  if (ctx.started && isGrounded)   //If the jump button is pressed and you’re grounded → triggers the Jumping state.
                 // _stateMachine.SetTrigger(State.Jumping);
        }

        public void OnPunch(InputAction.CallbackContext ctx) 
        {
            // if (ctx.started && !onCooldown)    //If you press punch and you’re not on cooldown → triggers Punching state.
            //     sm.SetTrigger(State.Punching);
        }

        public void OnRun(InputAction.CallbackContext ctx)
        {
            isInRun = ctx.ReadValue<float>() > 0.5f; //Reads an analog value (trigger or shift key).
          //  sm.ShowContext("run", isInRun); //Updates a boolean context “run” so the state machine can decide between walking and running.
        }

        public void OnAction(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
                TryInteract();
        }

        void TryInteract()
        {
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, transform.forward, out var hit, 3f, interactMask)) // Shoots a ray 1.5m above your feet, 3m forward only hitting objects in the interactMask
            {
                /*     - If it hits an object on the interactMask, checks if it’s a PlantaCatapulta.      If yes: Enters the “InsidePlant” state - Calls the plant’s EnterPlant() method with this player.
            
                var plant = hit.collider.GetComponent<PlantaCatapulta>();
                     if (plant != null)
                     {
                         sm.SetTrigger(State.InsidePlant);
                         plant.EnterPlant(this);
                     } */
            }
        }

        public void ApplyGravity(float dt)
        {
            velocity.y += gravity * dt; //Adds gravity to the Y velocity each frame.
        }

        public void BeginCooldown(float duration)  // When a punch starts, BeginCooldown() is called with a duration.
        {
          //  StartCoroutine(CooldownRoutine(duration));
        }

    public Vector3 ApplyInertia(Vector3 inputDir, float deltaTime, float turnSpeed)
    {
        if (inputDir.magnitude > 0.01f)
            inputDir.Normalize();
        //turnSpeed varies depending on the player's speed.

        lastDirection = Vector3.MoveTowards(lastDirection, inputDir, turnSpeed * deltaTime);
        return lastDirection;
    }

    public void TakeDamage()
    {
        Debug.Log("Player took damage. Will be implemented later.");
    }

    public void PushForce(Vector3 direction, int enemyEndurance)
    {
        enduranceDistance = (TypeOfDamage)(endurance + damageBoost - enemyEndurance + 2); //we add 2 to align the enum values with endurance distance values.
        float pushMultiplier = 0;
        switch (enduranceDistance)
        {
            case TypeOfDamage.PushOnlySelf:
                pushMultiplier = 10f;
                break;
            case TypeOfDamage.PushMostlySelf:
                pushMultiplier = 7.5f;
                break;
            case TypeOfDamage.PushBoth:
                pushMultiplier = 5f;
                break;
            case TypeOfDamage.PushMostlyOther:
                pushMultiplier = 2.5f;
                break;
            case TypeOfDamage.PushOnlyOther:
                break;
        }
        Debug.Log("Player Push Force with boost: " + damageBoost);
        damageBoost = 0; // Reset damage boost after being used
        if (pushMultiplier > 0)
        {
            //do a little knockback
            knockbackVelocity = direction.normalized * pushMultiplier * 2f;
            hasKnockback = true;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (normalPunchCooldown <= 0 && normalPunchTimer > 0) //only during punch and if not on cooldown
        {
            Vector3 knockbackDirection = GetDirectionalInput().normalized;
            //if there's no input, we set a default knockback direction
            if (knockbackDirection == Vector3.zero)
            {
                knockbackDirection = lastDirection;
                knockbackDirection.y = 0;
            }
            //we check if the collider belongs to an object with tag "Enemy"
            if (other.CompareTag("Enemy") && !other.isTrigger)
            {
                Debug.Log("Hit an enemy: " + other.name);
                //we try to get the EnemyController component from the hit object
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.PushForce(knockbackDirection, endurance);
                    PushForce(-knockbackDirection, enemy.endurance);
                    normalPunchTimer = 0;
                }
            }
        }
    }
    /*  IEnumerator CooldownRoutine(float t)
      {
          onCooldown = true;
          yield return new WaitForSeconds(t);
          onCooldown = false;
      }
    */
    // … public helper methods for stamina, adrenalina, etc. 

}

