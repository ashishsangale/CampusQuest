using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SimonSays : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button padRed;
    [SerializeField] private Button padBlue;
    [SerializeField] private Button padGreen;
    [SerializeField] private Button padYellow;
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Game Settings")]
    [SerializeField, Range(1, 20)] private int roundsToWin = 3;
    [SerializeField] private float flashTime = 0.5f;
    [SerializeField] private float betweenFlashes = 0.25f;
    [SerializeField] private float betweenRounds = 0.6f;

    [Header("Reward / Flow")]
    [SerializeField] private GameObject collectZoneToEnable;       // disabled at start
    [SerializeField] private MonoBehaviour playerMoverToReenable;  // PlayerSimpleMover

    [Header("Scoring")]
    [SerializeField] private int awardPoints = 20;

    [Header("Events")]
    public UnityEvent onSolved;   // fired to let the scene hide/destroy the chest

    [Header("Panel & Toast")]
    [SerializeField] private CanvasGroup panelGroup;   // SimonPanel CanvasGroup
    [SerializeField] private ToastPopup toast;

    [Header("HUD Masking")]
    [Tooltip("CanvasGroups to hide while the Simon panel is open (ScoreText, PlayerNameText, BuildingInfoPanel, etc.).")]
    [SerializeField] private CanvasGroup[] hudToHide;
    [SerializeField, Range(0f,1f)] private float hudHiddenAlpha = 0f;

    private readonly Dictionary<CanvasGroup, (float a, bool i, bool b)> hudSaved = new();

    private List<int> sequence = new List<int>();
    private int inputIndex = 0;
    private bool acceptingInput = false;

    private Image rImg, bImg, gImg, yImg;
    private Color rBase, bBase, gBase, yBase;

    void Awake()
    {
        rImg = padRed.GetComponent<Image>();
        bImg = padBlue.GetComponent<Image>();
        gImg = padGreen.GetComponent<Image>();
        yImg = padYellow.GetComponent<Image>();
        rBase = rImg.color; bBase = bImg.color; gBase = gImg.color; yBase = yImg.color;

        padRed.onClick.RemoveAllListeners();
        padBlue.onClick.RemoveAllListeners();
        padGreen.onClick.RemoveAllListeners();
        padYellow.onClick.RemoveAllListeners();
        padRed.onClick.AddListener(() => ClickPad(0));
        padBlue.onClick.AddListener(() => ClickPad(1));
        padGreen.onClick.AddListener(() => ClickPad(2));
        padYellow.onClick.AddListener(() => ClickPad(3));

        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        StopAllCoroutines();
        sequence.Clear();
        inputIndex = 0;
        acceptingInput = false;
        if (statusText) statusText.text = "Watch the sequence…";
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        for (int round = 1; round <= roundsToWin; round++)
        {
            sequence.Add(Random.Range(0, 4));
            yield return ShowSequence();

            if (statusText) statusText.text = $"Your turn (round {round}/{roundsToWin})";
            inputIndex = 0;
            acceptingInput = true;

            while (acceptingInput) yield return null;

            if (statusText && statusText.text.StartsWith("Wrong")) yield break;
            yield return new WaitForSeconds(betweenRounds);
        }

        // Won
        if (statusText) statusText.text = "You did it!";

        // award score like Jigsaw
        TryAwardPoints(awardPoints);

        // notify the scene to hide/destroy the chest
        onSolved?.Invoke();

        yield return new WaitForSeconds(0.4f);

        // fade out panel
        if (panelGroup)
        {
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
            yield return Fade(panelGroup, 1f, 0f, 0.25f);
        }

        // show clue toast after panel is gone
        if (toast) toast.Show("Your next treasure is at Academic Center!", 2.5f);

        // unlock collect zone and restore movement
        if (collectZoneToEnable) collectZoneToEnable.SetActive(true);
        if (playerMoverToReenable) playerMoverToReenable.enabled = true;

        gameObject.SetActive(false);
    }

    IEnumerator ShowSequence()
    {
        startButton.interactable = false;
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < sequence.Count; i++)
        {
            yield return Flash(sequence[i]);
            yield return new WaitForSeconds(betweenFlashes);
        }
        startButton.interactable = true;
    }

    IEnumerator Fade(CanvasGroup g, float from, float to, float t)
    {
        float e = 0f;
        g.alpha = from;
        while (e < t)
        {
            e += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(from, to, e / t);
            yield return null;
        }
        g.alpha = to;
    }

    IEnumerator Flash(int idx)
    {
        Image img = GetImage(idx);
        Color baseCol = GetBase(idx);
        img.color = Color.white;
        yield return new WaitForSeconds(flashTime);
        img.color = baseCol;
    }

    void ClickPad(int idx)
    {
        if (!acceptingInput) return;

        StartCoroutine(FlashTap(idx));

        if (idx == sequence[inputIndex])
        {
            inputIndex++;
            if (inputIndex >= sequence.Count)
            {
                acceptingInput = false;
                if (statusText) statusText.text = "Correct!";
            }
        }
        else
        {
            acceptingInput = false;
            if (statusText) statusText.text = "Wrong! Press Start to retry.";
        }
    }

    IEnumerator FlashTap(int idx)
    {
        Image img = GetImage(idx);
        Color baseCol = GetBase(idx);
        img.color = Color.white;
        yield return new WaitForSeconds(0.15f);
        img.color = baseCol;
    }

    void OnEnable()
    {
        // pause player movement
        if (playerMoverToReenable) playerMoverToReenable.enabled = false;

        // hide HUD
        hudSaved.Clear();
        if (hudToHide != null)
        {
            foreach (var cg in hudToHide)
            {
                if (!cg) continue;
                hudSaved[cg] = (cg.alpha, cg.interactable, cg.blocksRaycasts);
                cg.alpha = hudHiddenAlpha;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }

    void OnDisable()
    {
        // resume player movement
        if (playerMoverToReenable) playerMoverToReenable.enabled = true;

        // restore HUD
        foreach (var kv in hudSaved)
        {
            var cg = kv.Key;
            var snap = kv.Value;
            if (!cg) continue;
            cg.alpha = snap.a;
            cg.interactable = snap.i;
            cg.blocksRaycasts = snap.b;
        }
        hudSaved.Clear();
    }

    Image GetImage(int idx) => idx switch { 0 => rImg, 1 => bImg, 2 => gImg, _ => yImg };
    Color GetBase(int idx)  => idx switch { 0 => rBase, 1 => bBase, 2 => gBase, _ => yBase };

    // reflection-based score award (works with ScoreManager.Add/AddPoints/AddScore)
    private void TryAwardPoints(int points)
    {
        var smInstanceProp = typeof(ScoreManager).GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
        if (smInstanceProp == null) return;
        var sm = smInstanceProp.GetValue(null, null);
        if (sm == null) return;

        var t = sm.GetType();
        var m =
            t.GetMethod("Add", new[] { typeof(int) }) ??
            t.GetMethod("AddPoints", new[] { typeof(int) }) ??
            t.GetMethod("AddScore", new[] { typeof(int) });

        if (m != null) m.Invoke(sm, new object[] { points });
    }
}
