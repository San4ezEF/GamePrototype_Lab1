using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType
    {
        Coin,
        SpeedBoost
    }

    [Header("Type")]
    [SerializeField] private CollectibleType type;

    [Header("Coin Settings")]
    [SerializeField] private int scoreValue = 1;

    [Header("Speed Boost Settings")]
    [SerializeField] private float speedMultiplier = 1.5f;
    [SerializeField] private float boostDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        HandleCollect(other);

        Destroy(gameObject);
    }

    private void HandleCollect(Collider playerCollider)
    {
        switch (type)
        {
            case CollectibleType.Coin:
                HandleCoin();
                break;

            case CollectibleType.SpeedBoost:
                HandleSpeedBoost(playerCollider);
                break;
        }
    }

    private void HandleCoin()
    {
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
            ui.AddScore(scoreValue);
    }

    private void HandleSpeedBoost(Collider playerCollider)
    {
        PlayerMovement controller = playerCollider.GetComponent<PlayerMovement>();

        if (controller != null)
            controller.ApplySpeedBoost(speedMultiplier, boostDuration);
    }
}
    