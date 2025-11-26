using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    public bool isHazard = false; // true = hazard, false = health
    public int amount = 20;

    [HideInInspector] public RandomSpawnManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth h = other.GetComponent<PlayerHealth>();
        if (h != null)
        {
            if (isHazard)
                h.TakeDamage(amount);
            else
                h.Heal(amount);
        }

        if (manager != null)
            manager.RemoveFromList(gameObject);

        Destroy(gameObject);
    }
}
