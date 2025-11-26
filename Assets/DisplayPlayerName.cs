using UnityEngine;
using TMPro;

public class DisplayPlayerName : MonoBehaviour
{
    public TextMeshProUGUI nameText;

    void Start()
    {
        if (nameText == null)
        {
            nameText = GetComponent<TextMeshProUGUI>();
        }

        string playerName = PlayerPrefs.GetString("PLAYER_NAME", "Player");
        nameText.text = $"Welcome, {playerName}!";
    }
}
