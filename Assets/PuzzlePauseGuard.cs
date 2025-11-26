using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if TMP_PRESENT || TEXTMESHPRO_PRESENT
using TMPro;
#endif

/// Attach this to any puzzle root (e.g., JigsawPanel, SimonPanel).
/// It pauses gameplay while the panel is visible (active + raycastable),
/// and restores when hidden. Also disables common movement scripts as backup.
public class PuzzlePauseGuard : MonoBehaviour
{
    [Header("Optional: Player tag and scripts to disable")]
    [SerializeField] string playerTag = "Player";
    [SerializeField] Behaviour[] movementToDisable;

    CanvasGroup _cg;
    bool _isLocked;
    float _prevTimeScale = 1f;
    bool[] _wasEnabled;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        if ((movementToDisable == null || movementToDisable.Length == 0))
        {
            var p = GameObject.FindWithTag(playerTag);
            if (p)
            {
                var list = new List<Behaviour>();
                foreach (var b in p.GetComponentsInChildren<Behaviour>(true))
                {
                    var n = b.GetType().Name;
                    if (n.Contains("PlayerSimpleMover") || n.Contains("CharacterController") ||
                        n.Contains("FirstPerson") || n.Contains("ThirdPerson") ||
                        n.Contains("Mover") || n.Contains("Controller"))
                    {
                        if (!list.Contains(b)) list.Add(b);
                    }
                }
                movementToDisable = list.ToArray();
            }
        }
    }

    void OnEnable()
    {
        // If the panel starts enabled and visible, lock immediately.
        TryLockIfVisible();
    }

    void OnDisable()
    {
        UnlockIfNeeded();
    }

    void Update()
    {
        TryLockIfVisible();
        if (!_isLocked) return;

        // If it’s no longer visible or interactable, unlock.
        if (!IsVisible())
            UnlockIfNeeded();
    }

    bool IsVisible()
    {
        if (!gameObject.activeInHierarchy) return false;
        if (_cg == null) return true; // no CanvasGroup => treat as visible when active
        // Visible enough: receiving input OR mostly opaque
        return _cg.blocksRaycasts || _cg.interactable || _cg.alpha > 0.01f;
    }

    void TryLockIfVisible()
    {
        if (_isLocked) return;
        if (!IsVisible()) return;

        _isLocked = true;
        _prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (movementToDisable != null && movementToDisable.Length > 0)
        {
            _wasEnabled = new bool[movementToDisable.Length];
            for (int i = 0; i < movementToDisable.Length; i++)
            {
                var b = movementToDisable[i];
                if (!b) continue;
                _wasEnabled[i] = b.enabled;
                b.enabled = false;
            }
        }
    }

    void UnlockIfNeeded()
    {
        if (!_isLocked) return;
        _isLocked = false;

        // restore mover(s)
        if (movementToDisable != null && _wasEnabled != null)
        {
            for (int i = 0; i < movementToDisable.Length; i++)
            {
                var b = movementToDisable[i];
                if (!b) continue;
                b.enabled = _wasEnabled[i];
            }
        }
        _wasEnabled = null;

        // restore time
        Time.timeScale = _prevTimeScale;
    }
}
