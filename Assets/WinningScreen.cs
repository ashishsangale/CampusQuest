using UnityEngine;
using TMPro;

public class WinningScreen : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;              // The winning screen panel
    public TextMeshProUGUI playerNameText;   // Text to display player name
    public TextMeshProUGUI scoreText;        // Text to display score
    public TextMeshProUGUI totalCoinsText;   // Text to display total coins collected
    
    [Header("Optional")]
    public TextMeshProUGUI congratsMessage;  // Optional congratulations message
    
    void Start()
    {
        // Make sure win panel is hidden at start
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show the winning screen with player stats
    /// </summary>
    public void ShowWinScreen()
    {
        if (winPanel == null)
        {
            Debug.LogError("WinningScreen: Win Panel is not assigned!");
            return;
        }

        // Get player name
        string playerName = MainMenuController.GetPlayerName();
        
        // Get score and coins from GameManager
        int score = 0;
        int coinsCollected = 0;
        
        if (GameManager.Instance != null)
        {
            score = GameManager.Instance.Score;
            coinsCollected = GameManager.Instance.CoinsCollected;
        }
        
        // Update UI
        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }
        
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
        
        if (totalCoinsText != null)
        {
            totalCoinsText.text = $"Coins: {coinsCollected}";
        }
        
        if (congratsMessage != null)
        {
            congratsMessage.text = $"Congratulations, {playerName}!";
        }
        
        // Show the panel
        winPanel.SetActive(true);
        
        // Pause the game (optional)
        Time.timeScale = 0f;
        
        Debug.Log($"Victory! Player: {playerName}, Score: {score}, Coins: {coinsCollected}");
    }
    
    /// <summary>
    /// Close the win screen and continue
    /// </summary>
    public void CloseWinScreen()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Make sure to resume time before loading scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// Restart the game
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
