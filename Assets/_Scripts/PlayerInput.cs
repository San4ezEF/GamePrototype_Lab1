using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // камера, относительно которой считается направление

    public Vector3 MoveDirection { get; private set; } // мировое направление движения (нормализованное)
    private bool jumpRequested; // флаг запроса прыжка в текущем кадре

    private void Update()
    {
        // Чтение ввода 
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Локальное направление относительно игрока
        Vector3 localMove = new Vector3(h, 0f, v).normalized;

        // Преобразование в мировые координаты с учётом камеры
        MoveDirection = GetCameraRelativeDirection(localMove);

        // Запрос прыжка
        if (Input.GetButtonDown("Jump"))
            jumpRequested = true;
    }

   
    public bool ConsumeJump()
    {
        bool result = jumpRequested;
        jumpRequested = false;
        return result;
    }

    private Vector3 GetCameraRelativeDirection(Vector3 localDir)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("CameraTransform not assigned. Using world axes.");
            return localDir;
        }

        // Берём направления камеры
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Проецируем на горизонтальную плоскость
        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Собираем итоговый вектор
        return (camForward * localDir.z + camRight * localDir.x).normalized;
    }
}