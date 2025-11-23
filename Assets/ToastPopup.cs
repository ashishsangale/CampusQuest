using System.Collections;
using UnityEngine;
using TMPro;

public class ToastPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeTime = 0.25f;

    void Reset()  // auto-wire if added on the Toast object
    {
        label = GetComponent<TextMeshProUGUI>();
        group = GetComponent<CanvasGroup>();
    }

    public void Show(string message, float hold = 1.0f)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(message, hold));
    }

    IEnumerator ShowRoutine(string message, float hold)
    {
        if (label) label.text = message;
        yield return Fade(0f, 1f, fadeTime);
        yield return new WaitForSeconds(hold);
        yield return Fade(1f, 0f, fadeTime);
    }

    IEnumerator Fade(float a, float b, float t)
    {
        if (!group) yield break;
        float e = 0f;
        while (e < t)
        {
            e += Time.unscaledDeltaTime;            // UI not affected by timescale
            group.alpha = Mathf.Lerp(a, b, e / t);
            yield return null;
        }
        group.alpha = b;
    }
}
