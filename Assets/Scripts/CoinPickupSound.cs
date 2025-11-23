using UnityEngine;

public class CoinPickupSound : MonoBehaviour
{
    public AudioClip coinSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // play sound at coin position
            AudioSource.PlayClipAtPoint(coinSound, transform.position,10f);

            // optionally increase score here

            // destroy coin
            Destroy(gameObject);
        }
    }
}