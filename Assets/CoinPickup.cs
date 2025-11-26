using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Add coin to GameManager if it exists
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoin(1);
            }
            
            // Add coin to CoinManager if it exists
            if (CoinManager.instance != null)
            {
                CoinManager.instance.AddCoin();
            }
            
            Destroy(gameObject);
        }
    }
}
