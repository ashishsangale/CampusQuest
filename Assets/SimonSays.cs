using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private GameObject collectZoneToEnable;       // drag your CollectZone (disabled at start)
    [SerializeField] private MonoBehaviour playerMoverToReenable;  // drag PlayerSimpleMover

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
            sequence.Add(Random.Range(0, 4));      // extend sequence
            yield return ShowSequence();            // play it

            if (statusText) statusText.text = $"Your turn (round {round}/{roundsToWin})";
            inputIndex = 0;
            acceptingInput = true;

            // wait until player finishes or fails
            while (acceptingInput) yield return null;

            if (statusText && statusText.text.StartsWith("Wrong")) yield break; // stop on failure
            yield return new WaitForSeconds(betweenRounds);
        }

        // success 🎉
        if (statusText) statusText.text = "You did it!";
        yield return new WaitForSeconds(0.6f);
        if (collectZoneToEnable) collectZoneToEnable.SetActive(true);
        if (playerMoverToReenable) playerMoverToReenable.enabled = true;
        gameObject.SetActive(false); // close panel
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

    Image GetImage(int idx) => idx switch { 0 => rImg, 1 => bImg, 2 => gImg, _ => yImg };
    Color GetBase(int idx)  => idx switch { 0 => rBase, 1 => bBase, 2 => gBase, _ => yBase };
}
