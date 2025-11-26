using Mapbox.Unity.Map;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WasdPlayer : MonoBehaviour
{
    [Header("Movement Settings")]
    public AbstractMap map;
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float gravity = -18f;
    public bool useCameraRelativeMovement = true;

    [Header("Animation Settings")]
    public Animator animator;                // Assign your character's Animator here
    public string horizontalParam = "Hor";   // Horizontal axis parameter
    public string verticalParam = "Vert";    // Vertical axis parameter  
    public string stateParam = "State";      // State parameter (0=walk, 1=run)

    private CharacterController _controller;
    private Vector3 _velocity;
    private Camera _mainCamera;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        
        // Try to find animator if not assigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        // Calculate movement direction based on camera or world space
        Vector3 moveDir;
        if (useCameraRelativeMovement && _mainCamera != null)
        {
            // Get camera forward and right, but keep them flat (no Y component)
            Vector3 cameraForward = _mainCamera.transform.forward;
            Vector3 cameraRight = _mainCamera.transform.right;
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Calculate move direction relative to camera
            moveDir = (cameraForward * input.z + cameraRight * input.x);
        }
        else
        {
            // World-space movement (north/south/east/west)
            moveDir = input;
        }

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (_controller.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;

        Vector3 step = moveDir * speed + new Vector3(0f, _velocity.y, 0f);
        _controller.Move(step * Time.deltaTime);

        // Rotate player to face movement direction
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Update animations
        UpdateAnimations(h, v, isSprinting, input.sqrMagnitude > 0.01f);
    }

    void UpdateAnimations(float horizontal, float vertical, bool isRunning, bool isMoving)
    {
        if (animator == null) return;

        if (isMoving)
        {
            // Convert world movement to local direction for animation
            Vector3 localDirection = transform.InverseTransformDirection(new Vector3(horizontal, 0, vertical));
            
            // Set movement axes
            animator.SetFloat(horizontalParam, localDirection.x, 0.1f, Time.deltaTime);
            animator.SetFloat(verticalParam, localDirection.z, 0.1f, Time.deltaTime);
            
            // Set state: 0 = walk, 1 = run
            animator.SetFloat(stateParam, isRunning ? 1f : 0f, 0.1f, Time.deltaTime);
        }
        else
        {
            // Idle state
            animator.SetFloat(horizontalParam, 0f, 0.1f, Time.deltaTime);
            animator.SetFloat(verticalParam, 0f, 0.1f, Time.deltaTime);
            animator.SetFloat(stateParam, 0f, 0.1f, Time.deltaTime);
        }
    }

    [ContextMenu("Teleport To Map Center")]
    void TeleportToMapCenter()
    {
        var worldPos = map.GeoToWorldPosition(map.CenterLatitudeLongitude);
        transform.position = new Vector3((float)worldPos.x, 1.2f, (float)worldPos.y);
    }
}
