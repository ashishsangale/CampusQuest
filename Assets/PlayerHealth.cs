using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI popupText;

    private void Start()
    {
        currentHealth = maxHealth;
        if (popupText != null)
            popupText.text = "";

        // Sync with PlayerStats (Unity Store system)
        SyncToPlayerStats();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        ShowPopup("⚠ Health declining!", Color.red);

        // Sync with PlayerStats
        SyncToPlayerStats();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        ShowPopup("💚 Player regained strength!", Color.green);

        // Sync with PlayerStats
        SyncToPlayerStats();
    }

    // NEW METHOD: Sync PlayerHealth to PlayerStats
    private void SyncToPlayerStats()
    {
        if (PlayerStats.Instance != null)
        {
            // Convert 20 HP = 1 heart
            float healthInHearts = currentHealth / 20f;
            float maxHealthInHearts = maxHealth / 20f;

            // Update PlayerStats to match
            PlayerStats.Instance.SetHealth(healthInHearts, maxHealthInHearts);
        }
    }

    private void ShowPopup(string msg, Color color)
    {
        if (popupText == null) return;
        popupText.color = color;
        popupText.text = msg;
        CancelInvoke(nameof(ClearPopup));
        Invoke(nameof(ClearPopup), 2f);
    }

    private void ClearPopup()
    {
        if (popupText != null)
            popupText.text = "";
    }
}