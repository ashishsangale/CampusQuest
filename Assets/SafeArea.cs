using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    RectTransform _rt;
    Rect _lastSafe;

    void OnEnable()
    {
        _rt = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != _lastSafe) ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safe = Screen.safeArea;
        _lastSafe = safe;

        // Convert safe area rectangle from absolute pixels to anchor min/max
        Vector2 anchorMin = safe.position;
        Vector2 anchorMax = safe.position + safe.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }
}
