using UnityEngine;
using UnityEngine.SceneManagement;

public class EndSceneController : MonoBehaviour
{
    // Change this to match your main menu scene name exactly
    public string mainMenuSceneName = "00_startmenu";

    public void OnClick_MainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}