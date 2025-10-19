using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private bool _taken = false;

    private void OnTriggerEnter(Collider other)
    {
        if (_taken) return;

        if (!other.CompareTag("Player")) return;

        _taken = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterCoinCollected();

        Destroy(gameObject);
    }
}
