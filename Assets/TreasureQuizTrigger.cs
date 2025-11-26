using UnityEngine;

public class TreasureQuizTrigger : MonoBehaviour
{
    [Header("Chest + Reward")]
    public GameObject chestRoot;        // Assign the parent chest object
    public int rewardPoints = 10;        // Points to add on quiz completion

    [Header("Detection")]
    public string playerTag = "Player";

    [Header("References")]
    public QuizUIController quizUI;      // Assign your QuizCanvas object

    [Header("Completion Message (optional)")]
    public GameObject messagePrefab;

    private bool used = false;

    private void Reset()
    {
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;   // Mark as used so it doesn�t fire twice

        if (quizUI != null)
        {
            quizUI.onQuizCompleted.AddListener(OnQuizCompleted);
            quizUI.ShowQuiz();
        }
        else
        {
            Debug.LogWarning("QuizUI not assigned!");
        }

        // Disable further collisions
        GetComponent<Collider>().enabled = false;
    }

    private void OnQuizCompleted()
    {
        // Unsubscribe
        if (quizUI != null)
            quizUI.onQuizCompleted.RemoveListener(OnQuizCompleted);

        // Add score
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddPoints(rewardPoints);

        // Show floating message (optional)
        if (messagePrefab != null)
        {
            GameObject msg = Instantiate(messagePrefab, transform.position, Quaternion.identity);
            Destroy(msg, 6f);
        }

        // Destroy chest
        if (chestRoot != null)
            Destroy(chestRoot);
        else
            Destroy(gameObject);
    }
}
