using UnityEngine;
using TMPro;

public class ClueManager : MonoBehaviour
{
    [Header("Show with ToastPopup (recommended)")]
    public ToastPopup toast;          // drag your existing Toast object here
    public float toastSeconds = 2.5f;

    [Header("Fallback (if no ToastPopup)")]
    public TextMeshProUGUI clueText;  // optional: a TMP text on your Canvas
    public CanvasGroup clueGroup;     // optional: CanvasGroup on the same object
    public float fadeSeconds = 0.6f;
    public float holdSeconds = 2.0f;

    /// <summary>Call this from puzzle OnSolved() to announce the next spot.</summary>
    public void AnnounceNextAt(string buildingName)
    {
        if (!string.IsNullOrEmpty(buildingName))
        {
            string msg = $"Your next treasure is waiting at {buildingName}.";
            Show(msg);
        }
    }

    // Convenience if you hard-wire the next place in the Inspector
    [SerializeField] private string fixedBuilding;
    public void AnnounceFixedNext() => AnnounceNextAt(fixedBuilding);

    void Show(string message)
    {
        if (toast != null)
        {
            // Most ToastPopup scripts expose Show(msg, seconds). If yours is Show(msg) only,
            // just remove the second argument.
            toast.Show(message, toastSeconds);
            return;
        }

        // Fallback simple fade text
        if (clueText != null && clueGroup != null)
        {
            StopAllCoroutines();
            clueText.text = message;
            gameObject.SetActive(true);
            StartCoroutine(FadeRoutine());
        }
        else
        {
            Debug.Log($"Clue: {message}");
        }
    }

    System.Collections.IEnumerator FadeRoutine()
    {
        // fade in
        for (float t = 0; t < fadeSeconds; t += Time.deltaTime)
        {
            clueGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeSeconds);
            yield return null;
        }
        clueGroup.alpha = 1f;

        yield return new WaitForSeconds(holdSeconds);

        // fade out
        for (float t = 0; t < fadeSeconds; t += Time.deltaTime)
        {
            clueGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeSeconds);
            yield return null;
        }
        clueGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
