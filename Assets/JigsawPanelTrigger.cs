using UnityEngine;

public class JigsawPanelTrigger : MonoBehaviour
{
    [SerializeField] private GameObject jigsawPanel;  // assign the JigsawPanel object here

    private void Start()
    {
        // make sure the panel starts hidden
        if (jigsawPanel != null)
            jigsawPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // when the player comes close (into the trigger), show the panel
        if (other.CompareTag("Player") && jigsawPanel != null)
        {
            jigsawPanel.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // when the player leaves the trigger, hide the panel
        if (other.CompareTag("Player") && jigsawPanel != null)
        {
            jigsawPanel.SetActive(false);
        }
    }
}
