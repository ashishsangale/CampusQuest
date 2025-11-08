using UnityEngine;
using TMPro;

public class PlayerNameManager : MonoBehaviour
{
    private const string KEY = "PLAYER_NAME";

    [Header("UI")]
    public TMP_InputField inputField;
    public TextMeshProUGUI warningText; // optional

    void OnEnable() {
        if (inputField) inputField.text = PlayerPrefs.GetString(KEY, "");
        if (warningText) warningText.gameObject.SetActive(false);
        // Put keyboard focus in the box
        if (inputField) { inputField.Select(); inputField.ActivateInputField(); }
    }

    public bool TrySaveFromInput() {
        if (!inputField) return false;
        string name = inputField.text.Trim();
        if (string.IsNullOrEmpty(name)) {
            if (warningText) { warningText.text = "Please enter a name."; warningText.gameObject.SetActive(true); }
            return false;
        }
        PlayerPrefs.SetString(KEY, name);
        PlayerPrefs.Save();
        return true;
    }

    public static string GetPlayerName() =>
        PlayerPrefs.GetString(KEY, "Player");

    public static bool HasName() {
        string n = PlayerPrefs.GetString(KEY, "");
        return !string.IsNullOrWhiteSpace(n);
    }

    public void FocusInputIfAvailable()
    {
    if (inputField != null)
    {
        inputField.Select();
        inputField.ActivateInputField();
    }
    }
}