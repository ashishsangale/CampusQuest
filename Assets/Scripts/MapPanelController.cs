using UnityEngine;

public class MapPanelController : MonoBehaviour
{
    public GameObject mapPanel;

    public void ShowMap()
    {
        mapPanel.SetActive(true);
    }

    public void HideMap()
    {
        mapPanel.SetActive(false);
    }
}
