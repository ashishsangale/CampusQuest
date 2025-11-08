using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearPlayerNameOnExit : MonoBehaviour
{
    // Optional: clear when returning to the main menu
    public void ReturnToMainMenu()
    {
        // Delete saved name
        PlayerPrefs.DeleteKey("PLAYER_NAME");
        PlayerPrefs.Save();

        // Load your start menu scene
        SceneManager.LoadScene("00_startmenu");
    }

    // Also clear when the app closes
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("PLAYER_NAME");
        PlayerPrefs.Save();
    }
}
