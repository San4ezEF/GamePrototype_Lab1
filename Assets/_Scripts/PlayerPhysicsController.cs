using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPhysicsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Movement")]
    [SerializeField] private float baseMaxSpeed = 8f;
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float airAcceleration = 10f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Drag")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 0f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float extraGravityMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private float groundCheckRadius = 0.25f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;

    private Vector3 moveInput;
    private bool jumpRequested;
    private bool isGrounded;

    private float speedMultiplier = 1f;
    private float speedBoostTimer = 0f;

    private float CurrentMaxSpeed => baseMaxSpeed * speedMultiplier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Update()
    {
        ReadInput();
    }

    private void FixedUpdate()
    {
        UpdateSpeedBoostTimer();
        CheckGround();
        ApplyDrag();
        HandleMovement();
        HandleRotation();
        HandleJump();
        ApplyExtraGravity();

        jumpRequested = false;
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f)
            return;

        speedMultiplier = multiplier;
        speedBoostTimer = duration;
    }

    private void UpdateSpeedBoostTimer()
    {
        if (speedBoostTimer <= 0f)
            return;

        speedBoostTimer -= Time.fixedDeltaTime;

        if (speedBoostTimer <= 0f)
        {
            speedMultiplier = 1f;
            speedBoostTimer = 0f;
        }
    }

    private void ReadInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        moveInput = new Vector3(h, 0f, v).normalized;

        if (Input.GetButtonDown("Jump"))
            jumpRequested = true;
    }

    private void HandleMovement()
    {
        Vector3 desiredDirection = GetCameraRelativeDirection();
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float accel = isGrounded ? acceleration : airAcceleration;

        // Активное торможение только на земле
        if (isGrounded && desiredDirection.sqrMagnitude < 0.01f)
            accel = deceleration;

        Vector3 targetVelocity = desiredDirection * CurrentMaxSpeed;
        Vector3 velocityChange = targetVelocity - horizontalVelocity;

        // Ограничение прироста скорости
        Vector3 accelerationForce = Vector3.ClampMagnitude(
            velocityChange / Time.fixedDeltaTime,
            accel
        );

        rb.AddForce(accelerationForce, ForceMode.Acceleration);

        ClampHorizontalSpeed();
    }

    private void ClampHorizontalSpeed()
    {
        Vector3 horizontal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (horizontal.magnitude > CurrentMaxSpeed)
        {
            Vector3 limited = horizontal.normalized * CurrentMaxSpeed;
            rb.velocity = new Vector3(limited.x, rb.velocity.y, limited.z);
        }
    }

    private void HandleRotation()
    {
        Vector3 direction = GetCameraRelativeDirection();

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothRotation = Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );

        rb.MoveRotation(smoothRotation);
    }

    private void HandleJump()
    {
        if (!jumpRequested || !isGrounded)
            return;

        Vector3 velocity = rb.velocity;
        velocity.y = 0f;
        rb.velocity = velocity;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ApplyExtraGravity()
    {
        if (isGrounded)
            return;

        if (rb.velocity.y < 0f)
        {
            rb.AddForce(
                Vector3.up * Physics.gravity.y * (extraGravityMultiplier - 1f),
                ForceMode.Acceleration
            );
        }
    }

    private void ApplyDrag()
    {
        rb.drag = isGrounded ? groundDrag : airDrag;
    }

    private Vector3 GetCameraRelativeDirection()
    {
        if (cameraTransform == null)
            return Vector3.zero;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        return (camForward * moveInput.z + camRight * moveInput.x).normalized;
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        isGrounded = Physics.SphereCast(
            origin,
            groundCheckRadius,
            Vector3.down,
            out _,
            groundCheckDistance,
            groundLayer
        );
    }
}