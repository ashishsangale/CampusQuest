using UnityEngine;
using UnityEngine.UI;
using TMPro;   
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progress")]
    public Slider progressSlider;
    public int totalCoinsNeeded = 10;
    private int coinsCollected = 0;

    [Header("UI")]
    public TMP_Text coinsLabel; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = totalCoinsNeeded;
            progressSlider.wholeNumbers = true;
        }

        UpdateProgressUI();
    }

    public void AddCoin(int amount = 1)
    {
        coinsCollected = Mathf.Clamp(coinsCollected + amount, 0, totalCoinsNeeded);
        UpdateProgressUI();

    }

    public void UpdateProgressUI()
    {
        if (progressSlider != null)
            progressSlider.value = coinsCollected;

        if (coinsLabel != null)
            coinsLabel.text = $"Coins Collected\n{coinsCollected} / {totalCoinsNeeded}";
    }
}
