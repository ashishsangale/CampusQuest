using UnityEngine;

public class InteractZone : MonoBehaviour
{
    [SerializeField] private GameObject puzzlePanel; // assign the UI panel in Step 2
    [SerializeField] private MonoBehaviour playerMover; // drag PlayerSimpleMover here (optional)

    private bool openedOnce = false;

    private void OnTriggerEnter(Collider other)
    {
        if (openedOnce) return;
        if (!other.CompareTag("Player")) return;

        openedOnce = true;
        if (playerMover) playerMover.enabled = false; // freeze movement while puzzle is open
        if (puzzlePanel) puzzlePanel.SetActive(true);
    }
}
