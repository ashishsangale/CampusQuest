using UnityEngine;
using UnityEngine.SceneManagement;   // lets us load scenes by name
using UnityEngine.UI;               // for Button, Slider, Toggle types

public class MainMenuController : MonoBehaviour
{
    // ----- 1) Scene names you will load -----
    [Header("Scene Names")]
    public string sceneMap = "SampleScene";
    public string sceneAR  = "02_ARChallenge";

    // ----- 2) UI references you will drag in the Inspector -----
    [Header("UI References")]
    public Button continueButton;
    public GameObject panelHowToPlay;
    public GameObject panelSettings;
    public Slider sliderMasterVolume;
    public Toggle toggleVibration;

    // ----- 3) PlayerPrefs keys (like simple tiny save slots) -----
    const string SAVE_KEY = "HasSave";
    const string VOL_KEY  = "MasterVolume";
    const string VIB_KEY  = "Vibration";

    // ----- 4) Runs before the first frame -----
    void Awake()
    {
        // Load saved settings (with defaults if not set yet)
        float vol = PlayerPrefs.GetFloat(VOL_KEY, 1f);
        AudioListener.volume = vol;                       // applies globally
        if (sliderMasterVolume) sliderMasterVolume.value = vol;

        bool vib = PlayerPrefs.GetInt(VIB_KEY, 1) == 1;  // 1=true, 0=false
        if (toggleVibration) toggleVibration.isOn = vib;

        // Enable/disable Continue based on SAVE_KEY
        bool hasSave = PlayerPrefs.GetInt(SAVE_KEY, 0) == 1;
        if (continueButton) continueButton.interactable = hasSave;

        // Make sure pop-up panels start hidden
        if (panelHowToPlay) panelHowToPlay.SetActive(false);
        if (panelSettings)  panelSettings.SetActive(false);
    }

    // ----- 5) Button handlers you’ll wire from the Inspector -----
    public void OnStartNewQuest()
{
    if (string.IsNullOrEmpty(sceneMap))
    {
        Debug.LogError("sceneMap is empty on MainMenuController.");
        return;
    }

    if (!Application.CanStreamedLevelBeLoaded(sceneMap))
    {
        Debug.LogError($"Scene '{sceneMap}' is not in Build Settings / Build Profile. " +
                       "Open File→Build Settings (or Build Profiles) and add it to Scenes In Build.");
        return;
    }

    SceneManager.LoadScene(sceneMap);
}
    public void OnContinue()
    {
        bool hasSave = PlayerPrefs.GetInt(SAVE_KEY, 0) == 1;
        if (hasSave) SceneManager.LoadScene(sceneMap);
        else Debug.Log("No save found. Start a new quest first.");
    }

    public void OnOpenMap() => SceneManager.LoadScene(sceneMap);
    public void OnOpenAR()  => SceneManager.LoadScene(sceneAR);

    public void OnOpenHowToPlay() { if (panelHowToPlay) panelHowToPlay.SetActive(true); }
    public void OnCloseHowToPlay(){ if (panelHowToPlay) panelHowToPlay.SetActive(false); }

    public void OnOpenSettings()  { if (panelSettings)  panelSettings.SetActive(true); }
    public void OnCloseSettings() { if (panelSettings)  panelSettings.SetActive(false); }

    public void OnQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // stops play mode
#else
        Application.Quit(); // closes the app on device/PC build
#endif
    }

    // ----- 6) Settings change callbacks (wired from UI controls) -----
    public void OnVolumeChanged(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat(VOL_KEY, v);   // save immediately
    }

    public void OnVibrationChanged(bool on)
    {
        PlayerPrefs.SetInt(VIB_KEY, on ? 1 : 0);
        // During gameplay you can read this to decide whether to vibrate
    }

    // ----- 7) Called by gameplay when the player makes progress -----
    // You won’t press this from the menu; gameplay calls it once.
    public static void MarkHasSave()
    {
        PlayerPrefs.SetInt(SAVE_KEY, 1);
        PlayerPrefs.Save();
    }
}