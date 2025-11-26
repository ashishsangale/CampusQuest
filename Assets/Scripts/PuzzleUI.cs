<<<<<<< Updated upstream:Assets/Scripts/PuzzleUI.cs
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
=======
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PuzzleUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button optionA;
    [SerializeField] private Button optionB;
    [SerializeField] private Button optionC;

    [Header("Puzzle Data")]
    [TextArea]
    [SerializeField] private string question;
    [SerializeField] private string optionAText;
    [SerializeField] private string optionBText;
    [SerializeField] private string optionCText;
    [SerializeField, Range(0, 2)] private int correctIndex = 0;

    [Header("Rewards / Flow")]
    [SerializeField] private GameObject collectZoneToEnable;
    [SerializeField] private Behaviour playerMoverToReenable;

    [Header("Chest (optional)")]
    [SerializeField] private GameObject chestToDisable;
    [SerializeField] private bool disableChestImmediately = true;

    [Header("Toasts")]
    [SerializeField] private ToastPopup toast;
    [SerializeField] private string correctToast = "Correct!";
    [SerializeField] private string wrongToast = "Incorrect answer, try again.";
    [SerializeField] private float toastDuration = 2.5f;

    [Header("Panel (optional)")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private float fadeOutTime = 0.4f;

    [Header("Events")]
    public UnityEvent onSolved;

    // ------------------- NEW: Scoring -------------------
    [Header("Scoring")]
    [SerializeField] private int awardPoints = 20;

    // Cache
    private Button[] _buttons;

    void Awake()
    {
        _buttons = new[] { optionA, optionB, optionC };

        optionA.onClick.AddListener(() => SelectAnswer(0));
        optionB.onClick.AddListener(() => SelectAnswer(1));
        optionC.onClick.AddListener(() => SelectAnswer(2));

        if (feedbackText) feedbackText.text = string.Empty;

        // Fill UI from data
        if (questionText) questionText.text = question;
        if (optionA && optionA.GetComponentInChildren<TMP_Text>())
            optionA.GetComponentInChildren<TMP_Text>().text = optionAText;
        if (optionB && optionB.GetComponentInChildren<TMP_Text>())
            optionB.GetComponentInChildren<TMP_Text>().text = optionBText;
        if (optionC && optionC.GetComponentInChildren<TMP_Text>())
            optionC.GetComponentInChildren<TMP_Text>().text = optionCText;
    }

    private void SelectAnswer(int idx)
    {
        bool correct = idx == correctIndex;

        if (correct)
        {
            // --- score award (added) ---
            TryAwardPoints(awardPoints);

            // optional chest handling
            if (chestToDisable)
            {
                if (disableChestImmediately) chestToDisable.SetActive(false);
                else StartCoroutine(DisableAfterFade(chestToDisable, fadeOutTime));
            }

            // enable collect zone
            if (collectZoneToEnable) collectZoneToEnable.SetActive(true);

            // fire event for any external listeners
            onSolved?.Invoke();

            // show correct toast (after fade)
            StartCoroutine(FadeOutThenToastAndClose());
        }
        else
        {
            if (feedbackText) feedbackText.text = "Wrong!";
            if (toast) toast.Show(wrongToast, toastDuration);
        }
    }

    private IEnumerator FadeOutThenToastAndClose()
    {
        // fade panel out first (so the toast is visible over the world)
        if (panelGroup)
        {
            float t = 0f;
            float startA = panelGroup.alpha;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                panelGroup.alpha = Mathf.Lerp(startA, 0f, t / fadeOutTime);
                yield return null;
            }
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }

        // then show the “next treasure” toast
        if (toast && !string.IsNullOrEmpty(correctToast))
            toast.Show(correctToast, toastDuration);

        // finally disable the whole UI
        gameObject.SetActive(false);

        // re-enable player movement if specified
        if (playerMoverToReenable) playerMoverToReenable.enabled = true;
    }

    private IEnumerator DisableAfterFade(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (go) go.SetActive(false);
    }

    // -------- scoring helper (mirrors your SimonSays approach) --------
    private void TryAwardPoints(int points)
    {
        // Find ScoreManager.Instance
        var instProp = typeof(ScoreManager).GetProperty("Instance",
            BindingFlags.Public | BindingFlags.Static);
        if (instProp == null) return;

        var sm = instProp.GetValue(null, null);
        if (sm == null) return;

        // Try common method names: Add / AddPoints / AddScore
        var t = sm.GetType();
        var m =
            t.GetMethod("Add",       new[] { typeof(int) }) ??
            t.GetMethod("AddPoints", new[] { typeof(int) }) ??
            t.GetMethod("AddScore",  new[] { typeof(int) });

        if (m != null) m.Invoke(sm, new object[] { points });
    }
}
>>>>>>> Stashed changes:Assets/PuzzleUI.cs
