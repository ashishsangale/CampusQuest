using UnityEngine;

public class SimonPanelTrigger : MonoBehaviour
{
    [SerializeField] private GameObject simonPanel;  // assign the SimonPanel object here

    private void Start()
    {
        // make sure the panel starts hidden
        if (simonPanel != null)
            simonPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // when the player comes close (into the trigger), show the panel
        if (other.CompareTag("Player") && simonPanel != null)
        {
            simonPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // when the player leaves the trigger, hide the panel
        if (other.CompareTag("Player") && simonPanel != null)
        {
            simonPanel.SetActive(false);
        }
    }
}
