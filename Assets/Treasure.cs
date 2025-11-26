using UnityEngine;

public class Treasure : MonoBehaviour
{
    [SerializeField] private int pointValue = 10;
    [SerializeField] private float rotateSpeed = 60f; // visual flair

    private void Update()
    {
        // Simple spin so it's easy to spot
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Award points
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddPoints(pointValue);

        // TODO: play sound/particles here if you want

        Destroy(gameObject); // remove the chest after collecting
    }
}
