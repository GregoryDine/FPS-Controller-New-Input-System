using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerLook))]
public class WallRun : MonoBehaviour
{
    PlayerInputActions playerInputActions;

    PlayerMovement playerMovement;

    [SerializeField] bool canWallRun = true;

    [Header("Components")]
    [SerializeField] Transform orientation;

    [Header("Detection")]
    [SerializeField] float wallDistance = 0.7f;
    [SerializeField] float minimumJumpHeight = 1.5f;

    [Header("Wall Running")]
    [SerializeField] float wallRunGravity = 1f;
    [SerializeField] float wallRunJumpForce = 5f;
    [SerializeField] float verticalJumpMultiplier = 0.7f;
    [SerializeField] float horizontalJumpMultiplier = 1f;

    [Header("Camera")]
    [SerializeField] public float wallRunFov = 90f;
    [SerializeField] public float wallRunFovTime = 20f;
    [SerializeField] float camTilt = 10f;
    [SerializeField] float camTiltTime = 20f;

    public float tilt { get; private set; }

    bool wallLeft = false;
    bool wallRight = false;

    RaycastHit leftWallHit;
    RaycastHit rightWallHit;

    Rigidbody rb;

    [HideInInspector] public bool isWallRunning = false;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Jump.performed += Jump;

        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }

    bool CanWallRun()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minimumJumpHeight);
    }

    void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallDistance);
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallDistance);
    }

    private void FixedUpdate()
    {
        if (canWallRun)
        {
            CheckWall();

            if (CanWallRun())
            {
                if (wallLeft)
                {
                    StartWallRun();
                }
                else if (wallRight)
                {
                    StartWallRun();
                }
                else
                {
                    StopWallRun();
                }
            }
            else
            {
                StopWallRun();
            }
        }
    }

    void StartWallRun()
    {
        isWallRunning = true;

        rb.useGravity = false;

        if (rb.velocity.y > wallRunGravity)
        {
            rb.AddForce(Vector3.down * wallRunGravity * playerMovement.jumpForce, ForceMode.Force);
        }
        else if (rb.velocity.y < wallRunGravity)
        {
            rb.velocity = new Vector3(rb.velocity.x, -wallRunGravity, rb.velocity.z);
        }

        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        if (wallLeft)
        {
            tilt = Mathf.Lerp(tilt, -camTilt, camTiltTime * Time.deltaTime);
        }
        else if (wallRight)
        {
            tilt = Mathf.Lerp(tilt, camTilt, camTiltTime * Time.deltaTime);
        }
    }

    void StopWallRun()
    {
        isWallRunning = false;

        rb.useGravity = true;        

        tilt = Mathf.Lerp(tilt, 0, camTiltTime * Time.deltaTime);
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isWallRunning)
        {
            if (wallLeft)
            {
                Vector3 wallRunJumpDirection = transform.up * verticalJumpMultiplier + leftWallHit.normal * horizontalJumpMultiplier;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
            }
            else if (wallRight)
            {
                Vector3 wallRunJumpDirection = transform.up * verticalJumpMultiplier + rightWallHit.normal * horizontalJumpMultiplier;
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(wallRunJumpDirection * wallRunJumpForce * 100, ForceMode.Force);
            }
        }
    }
}
