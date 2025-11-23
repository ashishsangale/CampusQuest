using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public TextMeshProUGUI popupText;

    private void Start()
    {
        currentHealth = maxHealth;
        if (popupText != null)
            popupText.text = "";
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        ShowPopup("⚠ Health declining!", Color.red);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        ShowPopup("💚 Player regained strength!", Color.green);
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
}

