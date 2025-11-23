using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    public WinScreenController winScreen;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            winScreen.ShowWinScreen();
        }
    }
}