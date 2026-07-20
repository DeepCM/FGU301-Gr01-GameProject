using UnityEngine;
using Game.Events;
using DG.Tweening;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private Color flashColor = Color.red;

    private Color originalColor;
    private Tween flashTween;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        // Raise initial health event so UI slider updates on frame one
        GameEvents.RaiseHealthChanged(currentHealth, maxHealth);

        // Reset low health sound state on start
        if (Game.Audio.AudioManager.Instance != null)
        {
            Game.Audio.AudioManager.Instance.SetLowHealthActive(false);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Update health bar UI
        GameEvents.RaiseHealthChanged(currentHealth, maxHealth);

        Debug.Log($"Player took damage! Current health: {currentHealth}/{maxHealth}");

        // Play hurt sound from centralized audio engine
        if (Game.Audio.AudioManager.Instance != null)
        {
            Game.Audio.AudioManager.Instance.PlayHurt();
        }

        // Check low health threshold (30% of maxHealth)
        if (currentHealth > 0 && currentHealth <= maxHealth * 0.3f)
        {
            if (Game.Audio.AudioManager.Instance != null)
            {
                Game.Audio.AudioManager.Instance.SetLowHealthActive(true);
            }
        }
        else
        {
            if (Game.Audio.AudioManager.Instance != null)
            {
                Game.Audio.AudioManager.Instance.SetLowHealthActive(false);
            }
        }

        // Flash red visual juice
        FlashRed();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void FlashRed()
    {
        if (spriteRenderer == null) return;

        // Kill any existing flash tween
        if (flashTween != null && flashTween.IsActive())
        {
            flashTween.Kill();
        }

        spriteRenderer.color = flashColor;
        flashTween = spriteRenderer.DOColor(originalColor, flashDuration);
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        if (Game.Audio.AudioManager.Instance != null)
        {
            Game.Audio.AudioManager.Instance.SetLowHealthActive(false);
        }
        GameEvents.RaisePlayerDied();
    }
}
