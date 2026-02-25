using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
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
    private PlayerInput playerInput;

    private bool isGrounded;
    private float speedMultiplier = 1f;
    private float speedBoostTimer = 0f;

    private float CurrentMaxSpeed => baseMaxSpeed * speedMultiplier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();

        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        UpdateSpeedBoost();
        CheckGround();
        ApplyDrag();
        HandleMovement();
        HandleRotation();
        HandleJump();
        ApplyExtraGravity();
    }

    // Публичный метод для активации ускорителя (вызывается из Collectible)
    public void ApplySpeedBoost(float multiplier, float duration)
    {
        if (multiplier <= 0f || duration <= 0f)
            return;

        speedMultiplier = multiplier;
        speedBoostTimer = duration;
    }

    private void UpdateSpeedBoost()
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

    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out _, groundCheckDistance, groundLayer);
    }

    private void ApplyDrag()
    {
        rb.drag = isGrounded ? groundDrag : airDrag;
    }

    private void HandleMovement()
    {
        Vector3 desiredDirection = playerInput.MoveDirection;
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float accel = isGrounded ? acceleration : airAcceleration;

        // Активное торможение только на земле
        if (isGrounded && desiredDirection.sqrMagnitude < 0.01f)
            accel = deceleration;

        Vector3 targetVelocity = desiredDirection * CurrentMaxSpeed;
        Vector3 velocityChange = targetVelocity - horizontalVelocity;

        Vector3 accelerationForce = Vector3.ClampMagnitude(velocityChange / Time.fixedDeltaTime, accel);
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
        Vector3 direction = playerInput.MoveDirection;
        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);
    }

    private void HandleJump()
    {
        if (playerInput.ConsumeJump() && isGrounded)
        {
            Vector3 velocity = rb.velocity;
            velocity.y = 0f;
            rb.velocity = velocity;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void ApplyExtraGravity()
    {
        if (isGrounded)
            return;

        if (rb.velocity.y < 0f)
        {
            rb.AddForce(Vector3.up * Physics.gravity.y * (extraGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }
}