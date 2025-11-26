using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    [HideInInspector]
    public string buildingName;

    void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            BuildingUI buildingUI = FindObjectOfType<BuildingUI>();
            if (buildingUI != null)
            {
                buildingUI.ShowBuildingName(buildingName);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Check if the player left the trigger
        if (other.CompareTag("Player"))
        {
            BuildingUI buildingUI = FindObjectOfType<BuildingUI>();
            if (buildingUI != null)
            {
                buildingUI.HideBuildingName(buildingName);
            }
        }
    }
}
