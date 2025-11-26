using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progress")]
    public int totalCoinsNeeded = 10;
    private int coinsCollected = 0;

    [Header("Win Condition")]
    public int winningScore = 60; // Score needed to win
    public string winningSceneName = "WinningScene"; // Name of the winning scene

    [Header("UI")]
    public TMP_Text coinsLabel;

    // Public properties
    public int CoinsCollected => coinsCollected;
    public int Score => ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        PositionCoinsLabel();
        UpdateProgressUI();
    }

    public void AddCoin(int amount = 1)
    {
        coinsCollected = Mathf.Clamp(coinsCollected + amount, 0, totalCoinsNeeded);
        UpdateProgressUI();
        
        // NOTE: Coins don't affect score anymore
        // Score is tracked separately by ScoreManager
    }
    
    public void AddScore(int points)
    {
        // Use ScoreManager to add score
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(points);
        }
        
        UpdateProgressUI();
        
        // CheckWinCondition is now called by ScoreManager.AddPoints()
    }
    
    public void CheckWinCondition()
    {
        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        
        if (currentScore >= winningScore)
        {
            // Player reached winning score!
            OnGameWon();
        }
    }
    
    private void OnGameWon()
    {
        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.Score : 0;
        Debug.Log($"Game Won! Score: {currentScore} (Goal: {winningScore})");
        
        // Load the winning scene
        if (!string.IsNullOrEmpty(winningSceneName))
        {
            SceneManager.LoadScene(winningSceneName);
        }
        else
        {
            Debug.LogWarning("GameManager: Winning scene name is not set!");
        }
    }

    private void UpdateProgressUI()
    {
        if (coinsLabel != null)
            coinsLabel.text = $"Coins: {coinsCollected}";
    }

    private void PositionCoinsLabel()
    {
        if (coinsLabel == null) return;

        RectTransform rt = coinsLabel.GetComponent<RectTransform>();

        // Set position to (926, 387, 0)
        rt.anchoredPosition3D = new Vector3(926f, 387f, 0f);
    }
}
