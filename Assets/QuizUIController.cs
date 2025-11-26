using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[Serializable]
public class Question
{
    [TextArea] public string prompt;
    public string[] choices = new string[4];
    [Range(0, 3)] public int correctIndex;
}

public class QuizUIController : MonoBehaviour
{
    [Header("UI")]
    public GameObject quizPanel;               
    public TextMeshProUGUI questionText;     
    public Button[] answerButtons;          
    public TextMeshProUGUI feedbackText;      

    [Header("Quiz Content")]
    public Question[] questions;

    [Header("Events")]
    public UnityEvent onQuizCompleted;

    [Header("Input lock while quiz is open")]
    public MonoBehaviour[] componentsToDisable;

    private int currentQ = 0;
    private bool isActive = false;

    void Awake()
    {
        if (quizPanel != null) quizPanel.SetActive(false);
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int idx = i;
            answerButtons[i].onClick.AddListener(() => OnAnswerClicked(idx));
        }
    }

    public void ShowQuiz()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("QuizUIController: No questions set.");
            return;
        }

        currentQ = UnityEngine.Random.Range(0, questions.Length);

        isActive = true;
        RefreshUI();

        foreach (var comp in componentsToDisable)
            if (comp != null) comp.enabled = false;

        quizPanel.SetActive(true);
    }

    public void HideQuiz()
    {
        isActive = false;
        if (quizPanel != null) quizPanel.SetActive(false);

        foreach (var comp in componentsToDisable)
            if (comp != null) comp.enabled = true;
    }

    private void RefreshUI()
    {
        var q = questions[currentQ];
        questionText.text = q.prompt;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            var txt = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) txt.text = q.choices[i];
        }

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    private void OnAnswerClicked(int choiceIndex)
    {
        if (!isActive) return;

        var q = questions[currentQ];
        bool correct = (choiceIndex == q.correctIndex);

        if (correct)
        {
            if (feedbackText != null) feedbackText.gameObject.SetActive(false);

            HideQuiz();
            onQuizCompleted?.Invoke();
        }
        else
        {
            if (feedbackText != null)
            {
                feedbackText.text = "Try again!";
                feedbackText.gameObject.SetActive(true);
            }
        }
    }
}
