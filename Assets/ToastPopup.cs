using System.Collections;
using UnityEngine;
using TMPro;

public class ToastPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeTime = 0.25f;

    // Optional: For convenience, auto-wire if added directly
    void Reset()
    {
        label = GetComponent<TextMeshProUGUI>();
        group = GetComponent<CanvasGroup>();
    }

    public void Show(string message, float hold = 1.0f)
    {
        StopAllCoroutines();
        if (label) label.text = message;
        if (group)
        {
            group.alpha = 1f; // Instantly appear
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        StartCoroutine(HideRoutine(hold));
    }

    private IEnumerator HideRoutine(float hold)
    {
        yield return new WaitForSeconds(hold);
        // Fade out
        float elapsed = 0f;
        float startAlpha = group ? group.alpha : 1f;
        while (group && elapsed < fadeTime)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeTime);
            yield return null;
        }
        if (group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
