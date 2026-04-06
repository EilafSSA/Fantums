using UnityEngine;
using TMPro;

// displays HP and Score as plain text in the top left
public class PlayerHUD : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("=== Text Elements ===")]
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text scoreText;

    private void Update()
    {
        if (playerHealth != null && healthText != null)
        {
            healthText.text = $"HP: {playerHealth.GetCurrentHealth()} / {playerHealth.GetMaxHealth()}";
        }

        if (scoreText != null && GameManager.Instance != null)
        {
            scoreText.text = $"SCORE: {GameManager.Instance.GetScore()}";
        }
    }
}