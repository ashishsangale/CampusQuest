using UnityEngine;

public class TreasureQuizTrigger : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";

    [Header("References")]
    public QuizUIController quizUI;

    [Header("Completion Message")]
    public GameObject messagePrefab;


    private bool used = false;       
    private bool quizOpen = false;  

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true; 
    }

    void OnTriggerEnter(Collider other)
    {
        if (used || quizOpen) return;
        if (!other.CompareTag(playerTag)) return;

        if (quizUI != null)
        {
            quizOpen = true;
            quizUI.onQuizCompleted.AddListener(OnQuizCompleted);
            quizUI.ShowQuiz();

            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
        else
        {
            Debug.LogWarning("TreasureQuizTrigger: quizUI not assigned.");
        }
    }

    private void OnQuizCompleted()
    {
        if (quizUI != null)
            quizUI.onQuizCompleted.RemoveListener(OnQuizCompleted);

        used = true;
        quizOpen = false;

        if (messagePrefab != null)
        {
            GameObject msg = Instantiate(messagePrefab, transform.position, Quaternion.identity);
            Destroy(msg, 6f); // disappear after 6 sec
        }

        Destroy(gameObject);
    }
}
