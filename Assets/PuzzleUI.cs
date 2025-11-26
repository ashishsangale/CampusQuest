using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;   // <-- added

public class PuzzleUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;
    public Button optionA, optionB, optionC;

    [Header("Puzzle Data")]
    [TextArea] public string question = "Which direction best describes the polytechnic campus location within the phoenix area?";
    public string optionAText = "East valley near mesa/gateway";
    public string optionBText = "South phoenix downtown";
    public string optionCText = "West valley near glendale";
    [Range(0, 2)] public int correctIndex = 0; // 0=A, 1=B, 2=C

    [Header("Rewards/Flow")]
    public GameObject collectZoneToEnable;          // assign CollectZone here
    public MonoBehaviour playerMoverToReenable;     // drag PlayerSimpleMover here

    [Header("Toasts")]
    [SerializeField] private ToastPopup toast;      // drag your Toast object here
    [TextArea] public string correctToast = "Your next treasure is at !";
    [TextArea] public string wrongToast = "Try another answer.";
    public float toastDuration = 2.5f;

    [Header("Events")]
    public UnityEvent onSolved;                     // <-- added

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

        // Fallback toast lookup if not set
        if (!toast) toast = FindObjectOfType<ToastPopup>();
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
            // Fire event so you can disable/destroy the chest from the Inspector
            onSolved?.Invoke();                     // <-- added

            // Enable collect zone and close shortly after
            if (collectZoneToEnable) collectZoneToEnable.SetActive(true);
            if (toast) toast.Show(correctToast, toastDuration);
            StartCoroutine(CloseAfter(0.6f));
        }
        else
        {
            // Show gentle nudge for wrong answer
            if (toast) toast.Show(wrongToast, 1.6f);
        }
    }

    IEnumerator CloseAfter(float t)
    {
        yield return new WaitForSeconds(t);
        if (playerMoverToReenable) playerMoverToReenable.enabled = true;
        gameObject.SetActive(false);
    }
}