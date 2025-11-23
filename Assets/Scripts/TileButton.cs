using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class TileButton : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private Button btn;

    private JigsawPuzzle owner;
    private int visualIndex = -1;   // Where this tile sits in the 3x3 grid (0..8)
    private int logicalIndex = -1;  // Which slice it is showing (0..8)

    public int VisualIndex => visualIndex;
    public int LogicalIndex => logicalIndex;

    private void Reset()
    {
        img = GetComponent<Image>();
        btn = GetComponent<Button>();
    }

    private void Awake()
    {
        if (!img) img = GetComponent<Image>();
        if (!btn) btn = GetComponent<Button>();
    }

    /// Called by JigsawPuzzle during setup.
    public void Init(JigsawPuzzle puzzleOwner, int visualIdx, Sprite sprite, int logicalIdx)
    {
        // Ensure components are assigned
        if (!img) img = GetComponent<Image>();
        if (!btn) btn = GetComponent<Button>();

        owner       = puzzleOwner;
        visualIndex = visualIdx;
        SetSprite(sprite, logicalIdx);

        if (btn != null)
        {
            btn.onClick.RemoveListener(OnClicked);
            btn.onClick.AddListener(OnClicked);
        }
        Highlight(false);
    }

    public void Unbind()
    {
        if (btn) btn.onClick.RemoveListener(OnClicked);
    }

    public void SetSprite(Sprite sprite, int newLogicalIndex)
    {
        if (img) img.sprite = sprite;
        logicalIndex = newLogicalIndex;
    }

    public void Highlight(bool on)
    {
        if (!img) return;
        img.color = on ? new Color(1f, 1f, 1f, 0.85f) : Color.white;
    }

    private void OnClicked()
    {
        if (owner != null)
            owner.TileClicked(this);
    }
}
