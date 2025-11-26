using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public WinningScreen winningScreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (winningScreen != null)
            {
                winningScreen.ShowWinScreen();
            }
            else
            {
                Debug.LogWarning("FinishTrigger: WinningScreen reference not set!");
            }
        }
    }
}