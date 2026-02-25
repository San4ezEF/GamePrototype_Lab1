using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI positionText;
    [SerializeField] private TextMeshProUGUI speedText;

    [Header("Player References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRigidbody;

    private int currentScore = 0;

    private void Start()
    {
        UpdateScoreUI();
    }

    private void Update()
    {
        UpdatePositionUI();
        UpdateSpeedUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }

    private void UpdatePositionUI()
    {
        if (player == null || positionText == null)
            return;

        Vector3 pos = player.position;

        positionText.text =
            $"Position  X:{pos.x:F1}  Y:{pos.y:F1}  Z:{pos.z:F1}";
    }

    private void UpdateSpeedUI()
    {
        if (playerRigidbody == null || speedText == null)
            return;

        float speed = playerRigidbody.velocity.magnitude;

        speedText.text = $"Speed: {speed:F2}";
    }
}
