using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(WallRun))]
public class PlayerLook : MonoBehaviour
{
    PlayerInputActions playerInputActions;

    WallRun wallRun;

    [Header("Components")]
    [SerializeField] Transform cam;
    [SerializeField] Transform orientation;

    [Header("Sensitivity")]
    [SerializeField] float sensX = 10f;
    [SerializeField] float sensY = 10f;    

    float mouseX;
    float mouseY;

    float multiplier = 0.01f;

    float xRotation;
    float yRotation;

    private void Awake()
    {
        //initialize inputs
        playerInputActions = new PlayerInputActions();

        //initialize other variables
        wallRun = GetComponent<WallRun>();
    }

    private void Start()
    {
        //lock & hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //enable inputs
    private void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    //disable inputs
    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }

    private void Update()
    {
        Inputs();

        //update rotation
        cam.transform.localRotation = Quaternion.Euler(xRotation, yRotation, wallRun.tilt);
        orientation.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void Inputs()
    {
        //detect inputs
        mouseX = playerInputActions.Player.Look.ReadValue<Vector2>().x;
        mouseY = playerInputActions.Player.Look.ReadValue<Vector2>().y;

        //calculate rotation
        yRotation += mouseX * sensX * multiplier;
        xRotation -= mouseY * sensY * multiplier;

        //clamp vertical rotation
        xRotation = Mathf.Clamp(xRotation, -90, 90);
    }
}
