using UnityEngine;

/// <summary>
/// Орбитальная камера с коллизией.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Distance")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private float heightOffset = 1.5f;

    [Header("Collision")]
    [SerializeField] private float cameraRadius = 0.3f;
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float positionSmoothTime = 0.05f;

    private float yaw;
    private float pitch;

    private Vector3 currentVelocity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        HandleRotation();
        UpdatePosition();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void UpdatePosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = rotation * Vector3.back;

        Vector3 focusPoint = target.position + Vector3.up * heightOffset;

        Vector3 desiredPosition = focusPoint + direction * distance;
        Vector3 finalPosition = ResolveCollision(focusPoint, desiredPosition);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        transform.LookAt(focusPoint);
    }

    private Vector3 ResolveCollision(Vector3 origin, Vector3 desiredPosition)
    {
        Vector3 direction = (desiredPosition - origin).normalized;
        float maxDistance = Vector3.Distance(origin, desiredPosition);

        if (Physics.SphereCast(
                origin,
                cameraRadius,
                direction,
                out RaycastHit hit,
                maxDistance,
                collisionMask,
                QueryTriggerInteraction.Ignore))
        {
            return hit.point - direction * collisionOffset;
        }

        return desiredPosition;
    }

    public float CurrentYaw => yaw;
}
