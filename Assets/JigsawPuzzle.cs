using System.Collections.Generic;
using System.Reflection;   // for optional ScoreManager method call
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if TMP_PRESENT || TEXTMESHPRO_PRESENT
using TMPro;
#endif

public class JigsawPuzzle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GridLayoutGroup grid;
    [SerializeField] private Button shuffleButton;
#if TMP_PRESENT || TEXTMESHPRO_PRESENT
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
#else
    [SerializeField] private Text titleText;
    [SerializeField] private Text statusText;
#endif
    [SerializeField] private CanvasGroup panelGroup;

    [Header("Tiles (auto-wired if empty)")]
    [SerializeField] private List<TileButton> tiles = new List<TileButton>(9);

    [Header("Image Slices (3x3)")]
    [SerializeField] private List<Sprite> tileSprites = new List<Sprite>(9);

    [Header("Behaviour")]
    [SerializeField] private float fadeOutTime = 0.25f;
    [SerializeField] private string winMessage = "Completed!";
    [SerializeField] private int awardPoints = 20;

    [Header("Events")]
    public UnityEvent OnSolved;

    [Header("Toast (shown AFTER the panel fades)")]
    [SerializeField] private ToastPopup toast;                 // drag your Toast (TextMeshProUGUI + ToastPopup) here
    [SerializeField] private string toastMessage = "Your next treasure is at Academic Center!";
    [SerializeField] private float toastDuration = 2.5f;
    [SerializeField] private float postWinPause = 0.40f;       // tiny beat before fading out

    private TileButton firstSelected;
    private readonly int[] logicalOrder = new int[9];  // visual idx -> logical idx

    private void Awake()
    {
        // Auto-wire tiles from Grid if list empty
        if (tiles.Count == 0 && grid)
        {
            tiles.Clear();
            for (int i = 0; i < grid.transform.childCount; i++)
            {
                var tb = grid.transform.GetChild(i).GetComponent<TileButton>();
                if (tb) tiles.Add(tb);
            }
        }

        // Auto-wire shuffle if not set
        if (!shuffleButton) shuffleButton = GetComponentInChildren<Button>(true);
        if (shuffleButton)
        {
            shuffleButton.onClick.RemoveListener(Shuffle);
            shuffleButton.onClick.AddListener(Shuffle);
        }

        // Identity mapping initially
        for (int i = 0; i < 9; i++) logicalOrder[i] = i;

        ApplyLayout();
        SetStatus("Arrange the tiles to complete the image");
    }

    private void OnDestroy()
    {
        foreach (var t in tiles)
            if (t) t.Unbind();
    }

    private void SetStatus(string msg)
    {
        if (statusText) statusText.text = msg;
    }

    public void TileClicked(TileButton tile)
    {
        if (!tile) return;

        if (firstSelected == null)
        {
            firstSelected = tile;
            firstSelected.Highlight(true);
            return;
        }

        if (firstSelected == tile)
        {
            firstSelected.Highlight(false);
            firstSelected = null;
            return;
        }

        Swap(firstSelected, tile);
        firstSelected.Highlight(false);
        firstSelected = null;

        if (IsSolved())
        {
            SetStatus(winMessage);

            // Fire your existing hooks first (e.g., disable chest) and award points
            OnSolved?.Invoke();
            TryAwardPoints(awardPoints);

            // Then run the same “fade then toast” rhythm you use in Simon
            StartCoroutine(WinSequence());
        }
    }

    private System.Collections.IEnumerator WinSequence()
    {
        // small beat to let the win label be read
        yield return new WaitForSeconds(postWinPause);

        // fade panel out
        if (panelGroup)
        {
            float t = 0f;
            float start = panelGroup.alpha;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                panelGroup.alpha = Mathf.Lerp(start, 0f, t / fadeOutTime);
                yield return null;
            }
            panelGroup.alpha = 0f;
            panelGroup.interactable = false;
            panelGroup.blocksRaycasts = false;
        }

        // now show the clue toast
        if (toast) toast.Show(toastMessage, toastDuration);
    }

    private void Swap(TileButton a, TileButton b)
    {
        int viA = a.VisualIndex;
        int viB = b.VisualIndex;

        int tmp = logicalOrder[viA];
        logicalOrder[viA] = logicalOrder[viB];
        logicalOrder[viB] = tmp;

        a.SetSprite(tileSprites[logicalOrder[viA]], logicalOrder[viA]);
        b.SetSprite(tileSprites[logicalOrder[viB]], logicalOrder[viB]);
    }

    private bool IsSolved()
    {
        for (int i = 0; i < 9; i++)
            if (logicalOrder[i] != i) return false;
        return true;
    }

    public void Shuffle()
    {
        firstSelected = null;
        foreach (var t in tiles) t.Highlight(false);

        // Fisher–Yates
        for (int i = 8; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (logicalOrder[i], logicalOrder[j]) = (logicalOrder[j], logicalOrder[i]);
        }

        ApplyLayout();
        SetStatus("Mix & match the tiles!");
    }

    private void ApplyLayout()
    {
        if (tiles.Count != 9 || tileSprites.Count != 9)
        {
            Debug.LogError("JigsawPuzzle: Need exactly 9 tiles and 9 sprites.");
            return;
        }

        for (int visual = 0; visual < 9; visual++)
        {
            var t = tiles[visual];
            if (!t) continue;

            int logical = logicalOrder[visual];
            t.Init(this, visual, tileSprites[logical], logical);
        }
    }

    /// Try to award points on ScoreManager.Instance using any common method name.
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