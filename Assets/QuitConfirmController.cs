using UnityEngine;

public class QuitConfirmController : MonoBehaviour
{
    public GameObject quitPanel;   // drag your Panel_QuitConfirm here in Inspector

    private void Start()
    {
        // Make sure the panel starts hidden
        if (quitPanel != null)
            quitPanel.SetActive(false);
    }

    private void Update()
    {
        // This should run every frame while the scene is playing
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed!");   // check Console to see this

            if (quitPanel == null)
            {
                Debug.LogWarning("QuitConfirmController: quitPanel is NOT assigned!");
                return;
            }

            // Toggle panel on/off
            bool newState = !quitPanel.activeSelf;
            quitPanel.SetActive(newState);

            // Pause game when panel is open, resume when closed
            Time.timeScale = newState ? 0f : 1f;
        }
    }

    // Button: YES
    public void QuitGame()
    {
        Debug.Log("Quit Game called");
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Button: NO
    public void ClosePanel()
    {
        if (quitPanel != null)
            quitPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}