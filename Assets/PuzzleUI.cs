using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PuzzleUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;
    public Button optionA, optionB, optionC;

    [Header("Puzzle Data")]
    [TextArea] public string question = "What is 2 + 2?";
    public string optionAText = "3";
    public string optionBText = "4";
    public string optionCText = "5";
    [Range(0,2)] public int correctIndex = 1; // 0=A, 1=B, 2=C

    [Header("Rewards/Flow")]
    public GameObject collectZoneToEnable;          // assign CollectZone here
    public MonoBehaviour playerMoverToReenable;     // drag PlayerSimpleMover here

    void OnEnable()
    {
        // Populate labels when the panel opens
        if (questionText) questionText.text = question;
        SetButtonLabel(optionA, optionAText);
        SetButtonLabel(optionB, optionBText);
        SetButtonLabel(optionC, optionCText);
        if (feedbackText) feedbackText.text = "";

        // Wire buttons
        optionA.onClick.RemoveAllListeners();
        optionB.onClick.RemoveAllListeners();
        optionC.onClick.RemoveAllListeners();
        optionA.onClick.AddListener(() => SelectAnswer(0));
        optionB.onClick.AddListener(() => SelectAnswer(1));
        optionC.onClick.AddListener(() => SelectAnswer(2));
    }

    void SetButtonLabel(Button btn, string text)
    {
        if (!btn) return;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp) tmp.text = text;
    }

    public void SelectAnswer(int idx)
    {
        bool correct = idx == correctIndex;
        if (feedbackText)
        {
            feedbackText.text = correct ? "Correct!" : "Try again…";
            feedbackText.color = correct ? Color.green : Color.red;
        }

        if (correct)
        {
            if (collectZoneToEnable) collectZoneToEnable.SetActive(true);
            StartCoroutine(CloseAfter(0.6f));
        }
    }

    IEnumerator CloseAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (playerMoverToReenable) playerMoverToReenable.enabled = true;
        gameObject.SetActive(false);
    }
}
