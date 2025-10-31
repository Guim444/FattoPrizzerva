using UnityEngine;
using UnityEngine.InputSystem;
/*
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(IEstaminable))]
[RequireComponent(typeof(IAdrenalinable))] */
  

public class PlayerController : MonoBehaviour
    {
       
        Vector3 velocity; //3D vector to store the current movement speed and direction of the character.
    [Header("Player Parameters")]
    public float normalSpeed = 7f, runningSpeed = 10f, gravity = -9.81f, jumpForce = 5f, cooldownPunch = 0.75f; 
    [Header ("Z Boundaries")] public float minZ = 0f, maxZ = 100f, minScale = 0.3f, maxScale = 1f; //Declared Floats

    bool isGrounded, isPunching, isInRun, onCooldown;
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


    void Awake()
        {
            stamina = GetComponent<IEstaminable>();
            adrenaline = GetComponent<IAdrenalinable>();
            cc = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
        
        var idle = new IdleState();
        idle.player = this;
        idle.controller = GetComponent<CharacterController>();

        var moving = new MovingState();
        moving.player = this;
        moving.controller = GetComponent<CharacterController>();

        var running = new RunningState();
        running.player = this;
        running.controller = GetComponent<CharacterController>();

    
            StateMachine.AddState(State.Idle, idle);
            StateMachine.AddState(State.Moving, moving);
            //global: :State means: “Use the enum called State that exists in the global namespace (outside of any class/namespace), not something else that also happens to be called State.”
            StateMachine.AddState(State.Running, running);
            StateMachine.AddState(State.Punching, new PunchingState());
            StateMachine.AddState(State.PunchRunning, new PunchRunningState());
            StateMachine.AddState(State.Jumping, new JumpingState());
            StateMachine.AddState(State.Falling, new FallingState());
            StateMachine.AddState(State.PunchFalling, new PunchFallingState());
            StateMachine.AddState(State.Gliding, new GlidingState());
            StateMachine.AddState(State.Knockedback, new KnockedbackState());
            StateMachine.AddState(State.Interacting, new InteractingState());

            StateMachine.SetState(State.Idle);
        }


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        void Update()
        {
            UpdateScaleBasedOnZ();
            isGrounded = cc.isGrounded; //Uses the character controller's built-in ground detection
       // Debug.Log(transform.position.z);


        if (isGrounded && velocity.y < 0) velocity.y = -2f; //When player touches the floor and falling in any speed (<0) it makes it -2 to stick the player on the floor 
           //StateMachine.Tick(Time.deltaTime);      //Updates the current state logic
           if (cc.enabled == true) cc.Move(velocity * Time.deltaTime);  //Applies accumulated velocity


        State newState;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            newState = Input.GetKey(KeyCode.LeftShift) ? State.Running : State.Moving;
        }
        else
        {
            newState = State.Idle;
        }

        // Change state only if different
        if (newState != currentState)
        {
            currentState = newState;
            StateMachine.SetState(currentState);
        }

        // Let the state run its own Update logic
        StateMachine.Update();

    }

    public Vector3 GetDirectionalInput()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += Vector3.forward;   // Z+
        if (Input.GetKey(KeyCode.S)) move += Vector3.back;      // Z-
        if (Input.GetKey(KeyCode.D)) move += Vector3.right;     // X+
        if (Input.GetKey(KeyCode.A)) move += Vector3.left;      // X-

        move.y = 0; // never touch Y except gravity/jumps
        if (move.x < 0) flip = true;
        else if (move.x > 0) flip = false;
        FlipChar(flip);

        return move.normalized;
    }
    public void FlipChar(bool flip)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (flip ? -1 : 1);
        transform.localScale = scale;
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

        transform.localScale = Vector3.one * scaleFactor;
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

      /*  IEnumerator CooldownRoutine(float t)
        {
            onCooldown = true;
            yield return new WaitForSeconds(t);
            onCooldown = false;
        }
      */
        // … public helper methods for stamina, adrenalina, etc. 

    }

