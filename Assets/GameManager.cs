using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progress")]
    public Slider progressSlider;
    public int totalCoinsNeeded = 10;   // can be changed later
    private int coinsCollected = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = totalCoinsNeeded;
            progressSlider.value = 0;
        }
    }

    public void RegisterCoinCollected()
    {
        coinsCollected = Mathf.Min(coinsCollected + 1, totalCoinsNeeded);
        if (progressSlider != null)
            progressSlider.value = coinsCollected;

    }
}
