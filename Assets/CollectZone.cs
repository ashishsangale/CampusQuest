using UnityEngine;

public class CollectZone : MonoBehaviour
{
    [SerializeField] private int pointValue = 10;
    [SerializeField] private GameObject treasureToRemove; // assign the chest here
    [SerializeField] private bool destroyTreasure = true; // or SetActive(false)

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddPoints(pointValue);

        if (treasureToRemove != null)
        {
            if (destroyTreasure) Destroy(treasureToRemove);
            else treasureToRemove.SetActive(false);
        }

        // Optionally remove the zone too
        Destroy(gameObject);
    }
}
