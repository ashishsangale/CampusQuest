using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private void Awake()
    {
        // If an AudioManager already exists, destroy this duplicate
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Make this AudioManager persistent across scenes
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}