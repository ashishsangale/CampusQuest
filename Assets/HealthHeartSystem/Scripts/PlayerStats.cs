/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 *  Modified to add SetHealth method
 */
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public delegate void OnHealthChangedDelegate();
    public OnHealthChangedDelegate onHealthChangedCallback;

    #region Singleton
    private static PlayerStats instance;
    public static PlayerStats Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PlayerStats>();
            return instance;
        }
    }
    #endregion

    [SerializeField]
    private float health = 5;
    [SerializeField]
    private float maxHealth = 5;
    [SerializeField]
    private float maxTotalHealth = 10;

    public float Health { get { return health; } }
    public float MaxHealth { get { return maxHealth; } }
    public float MaxTotalHealth { get { return maxTotalHealth; } }

    // NEW METHOD: Set health directly (for syncing with PlayerHealth)
    public void SetHealth(float newHealth, float newMaxHealth)
    {
        health = newHealth;
        maxHealth = newMaxHealth;
        ClampHealth();
    }

    public void Heal(float health)
    {
        this.health += health;
        ClampHealth();
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;
        ClampHealth();
    }

    public void AddHealth()
    {
        if (maxHealth < maxTotalHealth)
        {
            maxHealth += 1;
            health = maxHealth;
            if (onHealthChangedCallback != null)
                onHealthChangedCallback.Invoke();
        }   
    }

    void ClampHealth()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        if (onHealthChangedCallback != null)
            onHealthChangedCallback.Invoke();
    }
}