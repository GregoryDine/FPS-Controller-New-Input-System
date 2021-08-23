using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(WallRun))]
[RequireComponent(typeof(PlayerLook))]
public class PlayerMovement : MonoBehaviour
{
    PlayerInputActions playerInputActions;

    WallRun wallRun;

    Rigidbody rb;

    [Header("Components")]
    [SerializeField] Transform orientation;
    [SerializeField] Camera cam;

    [Header("Speed")]
    [SerializeField] float walkSpeed = 7f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float crouchSpeed = 5f;
    [SerializeField] float wallRunSpeed = 13f;
    [SerializeField] float acceleration = 10f;
    float speed = 10.0f;
    bool isSprinting;

    [Header("Jump")]
    [SerializeField] public float jumpForce = 5f;

    [Header("Crouching & Sliding")]
    [SerializeField] bool canSlide = true;
    [SerializeField] float crouchSize = 0.5f;
    [SerializeField] float slideForce = 400f;
    [SerializeField] float lateralSpeedMultiplier = 1f;
    [SerializeField] float slideStopSpeed = 2f;
    bool isSliding = false;
    bool isCrouching = false;   

    [Header("Fov")]
    [SerializeField] float defaultFov = 80f;
    [SerializeField] float sprintFov = 82.5f;
    [SerializeField] float slideFov = 85f;
    [SerializeField] float fovTime = 20f;    

    [Header("Velocity")]
    [SerializeField] float groundVelocityChange = 1.5f;
    [SerializeField] float airVelocityChange = 0.3f;
    [SerializeField] float airDrag = 0.99f;
    [SerializeField] float slideDarg = 0.99f;

    [Header("Ground Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDitance = 0.2f;
    bool isGrounded;    

    Vector3 targetVelocity;

    Vector3 playerScale;
    Vector3 crouchScale;

    RaycastHit slopeHit;

    //detect if player is on a slope
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerScale.y + 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    private void Awake()
    {
        //initialize inputs
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Crouch.started += StartCrouch;
        playerInputActions.Player.Crouch.canceled += StopCrouch;

        //initialize other variables
        wallRun = GetComponent<WallRun>();

        rb = GetComponent<Rigidbody>();

        playerScale = transform.localScale;
        crouchScale = new Vector3(transform.localScale.x, transform.localScale.y * crouchSize, transform.localScale.z);
    }

    //enable inputs
    private void OnEnable()
    {
        playerInputActions.Enable();
    }

    //disable inputs
    private void OnDisable()
    {
        playerInputActions.Disable();
    }

    private void Update()
    {
        //detect if player is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDitance, groundMask);

        //stop crouching if not grounded
        if (!isGrounded && isCrouching)
        {
            isSliding = false;
            isCrouching = false;
            transform.localScale = playerScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        }
        //stop sliding if too slow
        if (isSliding && rb.velocity.magnitude < slideStopSpeed)
        {
            isSliding = false;
        }

        Inputs();
        ChangeSpeed();
    }

    private void Inputs()
    {
        //calculate velocity
        targetVelocity = (orientation.forward * playerInputActions.Player.Movement.ReadValue<Vector2>().y + orientation.right * playerInputActions.Player.Movement.ReadValue<Vector2>().x) * speed;
    }

    private void ChangeSpeed()
    {
        //change speed && fov
        if (isCrouching)
        {
            //crouching
            speed = crouchSpeed;
            if (isSliding)
            {
                //sliding
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, slideFov, fovTime * Time.deltaTime);
            }
            else
            {
                //crouching
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defaultFov, fovTime * Time.deltaTime);
            }
        }
        else if (playerInputActions.Player.Sprint.ReadValue<float>() == 1 && targetVelocity != Vector3.zero && isGrounded)
        {
            //sprinting
            speed = Mathf.Lerp(speed, sprintSpeed, acceleration * Time.deltaTime);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFov, fovTime * Time.deltaTime);
            isSprinting = true;
        }
        else if (playerInputActions.Player.Sprint.ReadValue<float>() == 1 && targetVelocity != Vector3.zero && wallRun.isWallRunning)
        {
            //wallrunning
            speed = Mathf.Lerp(speed, wallRunSpeed, acceleration * Time.deltaTime);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, wallRun.wallRunFov, wallRun.wallRunFovTime * Time.deltaTime);
            isSprinting = true;
        }
        else
        {
            //walking
            speed = Mathf.Lerp(speed, walkSpeed, acceleration * Time.deltaTime);
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, defaultFov, fovTime * Time.deltaTime);
            isSprinting = false;
        }
    }

    private void FixedUpdate()
    {
        Move();
        Drag();
    }

    private void Move()
    {
        //calculate force
        Vector3 velocity = rb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.y = 0f;
        if (OnSlope())
        {
            //change velocity to match slope angle
            velocityChange = Vector3.ProjectOnPlane(velocityChange, slopeHit.normal);
        }
        //clamp velocity
        velocityChange = Vector3.ClampMagnitude(velocityChange, isGrounded ? groundVelocityChange : airVelocityChange);

        //apply force
        //lateral control when sliding
        if (isSliding)
        {
            //get forward speed
            float moveSpeed = Vector3.Dot(rb.velocity, orientation.forward);            
            if (moveSpeed > 1f)
            {
                rb.AddForce(orientation.right * playerInputActions.Player.Movement.ReadValue<Vector2>().x * moveSpeed * lateralSpeedMultiplier, ForceMode.Force);
            }            
        }
        //total control on ground
        else if (isGrounded)
        {
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        //keep partial air control
        else if (!isGrounded && targetVelocity != Vector3.zero)
        {
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }        
    }

    private void Drag()
    {
        //slowdown velocity when not moving in air
        if (!isGrounded && targetVelocity == Vector3.zero)
        {
            rb.velocity = new Vector3(rb.velocity.x * airDrag, rb.velocity.y, rb.velocity.z * airDrag);
        }
        //apply slide drag
        else if (isSliding)
        {
            rb.velocity = new Vector3(rb.velocity.x * slideDarg, rb.velocity.y, rb.velocity.z * slideDarg);
        }
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            //reset & apply vertical force
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void StartCrouch(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            //start crouching
            isCrouching = true;
            transform.localScale = crouchScale;
            transform.position = new Vector3(transform.position.x, transform.position.y - (transform.position.y - crouchSize), transform.position.z);
            //start sliding if runnning while crouching
            if (rb.velocity.magnitude > 0.5f && isSprinting && canSlide)
            {
                isSliding = true;
                rb.AddForce(Vector3.ProjectOnPlane(orientation.forward, slopeHit.normal) * slideForce);                
            }
        }        
    }

    private void StopCrouch(InputAction.CallbackContext context)
    {
        if (context.canceled && isCrouching)
        {
            //stop crouching if input is released
            isSliding = false;
            isCrouching = false;
            transform.localScale = playerScale;
            transform.position = new Vector3(transform.position.x, transform.position.y + (transform.position.y - crouchSize), transform.position.z);
        }        
    }
}
