using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Heart System Integration")]
    public bool useHeartSystem = true;
    public PlayerStats playerStats; // Assign this in Inspector
    private const int HEALTH_PER_HEART = 20; // Each heart represents 20 health points

    [Header("UI - Alternative Health Bar")]
    public Slider healthBarSlider;      // For slider-based health bars
    public Image healthBarFill;         // For image-based health bars (with fill amount)
    public TextMeshProUGUI healthText;  // Optional: displays "75/100"

    [Header("UI - Popup Messages")]
    public TextMeshProUGUI popupText;

    private void Start()
    {
        currentHealth = maxHealth;

        // Try to find PlayerStats if not assigned
        if (useHeartSystem && playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                playerStats = PlayerStats.Instance;
            }
        }

        // Initialize heart system if enabled
        if (useHeartSystem && playerStats != null)
        {
            Debug.Log("Initializing Heart System...");
            // The heart system will automatically show based on PlayerStats values
            // Make sure PlayerStats has correct initial values in Inspector:
            // - health: 5 (for 5 hearts)
            // - maxHealth: 5
            // - maxTotalHealth: 10 (maximum possible hearts)
        }
        else if (useHeartSystem)
        {
            Debug.LogWarning("PlayerHealth: Heart System enabled but PlayerStats not found! Add PlayerStats component to player.");
        }
        
        UpdateHealthBar();
        
        if (popupText != null)
            popupText.text = "";
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        // Update heart system
        if (useHeartSystem && playerStats != null)
        {
            float damageInHearts = amount / (float)HEALTH_PER_HEART;
            playerStats.TakeDamage(damageInHearts);
            Debug.Log($"Took {amount} damage ({damageInHearts} hearts). Current health: {playerStats.Health} hearts");
        }

        UpdateHealthBar();
        ShowPopup("⚠ Health declining!", Color.red);

        // Check if player died
        if (currentHealth <= 0)
        {
            OnPlayerDeath();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // Update heart system
        if (useHeartSystem && playerStats != null)
        {
            float healInHearts = amount / (float)HEALTH_PER_HEART;
            playerStats.Heal(healInHearts);
            Debug.Log($"Healed {amount} HP ({healInHearts} hearts). Current health: {playerStats.Health} hearts");
        }

        UpdateHealthBar();
        ShowPopup("💚 Player regained strength!", Color.green);
    }

    private void UpdateHealthBar()
    {
        // Calculate health percentage (0 to 1)
        float healthPercent = (float)currentHealth / maxHealth;

        // Update slider-based health bar
        if (healthBarSlider != null)
        {
            healthBarSlider.value = healthPercent;
        }

        // Update image-based health bar (fill amount)
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = healthPercent;

            // Optional: Change color based on health level
            if (healthPercent > 0.6f)
                healthBarFill.color = Color.green;
            else if (healthPercent > 0.3f)
                healthBarFill.color = Color.yellow;
            else
                healthBarFill.color = Color.red;
        }

        // Update health text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void ShowPopup(string msg, Color color)
    {
        if (popupText == null) return;

        popupText.color = color;
        popupText.text = msg;

        CancelInvoke(nameof(ClearPopup));
        Invoke(nameof(ClearPopup), 2f); // show 2 seconds
    }

    private void ClearPopup()
    {
        if (popupText != null)
            popupText.text = "";
    }

    private void OnPlayerDeath()
    {
        Debug.Log("Player has died!");
        // Add death logic here:
        // - Show death screen
        // - Respawn player
        // - Load game over scene
        // - etc.
    }

    // Public method to get current health percentage (useful for other systems)
    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    // Public method to check if player is alive
    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}

