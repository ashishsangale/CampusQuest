using UnityEngine;
using TMPro;

public class WinScreenController : MonoBehaviour
{
    public GameObject winPanel;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI coinText;

    [Header("Effects")]
    public ParticleSystem confettiFX;
    public AudioSource victoryAudio;

    public void ShowWinScreen()
    {
        winPanel.SetActive(true);

        // Show name
        string playerName = PlayerNameManager.GetPlayerName();
        playerNameText.text = "Player: " + playerName;

        // Show coins
        int coins = CoinManager.instance.coinCount;
        coinText.text = "Coins Collected: " + coins;

        // Play celebration effects 🎉
        if (confettiFX != null)
        {
            confettiFX.gameObject.SetActive(true);
            confettiFX.Play();
        }

        if (victoryAudio != null)
        {
            victoryAudio.Play();
        }

        Time.timeScale = 0f;
    }
}