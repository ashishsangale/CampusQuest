using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BuildingUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buildingPanel;
    public TextMeshProUGUI buildingNameText;
    
    [Header("Animation Settings")]
    public float fadeSpeed = 3f;
    
    private CanvasGroup canvasGroup;
    private string currentBuilding = "";
    private HashSet<string> nearbyBuildings = new HashSet<string>();

    void Awake()
    {
        // Get or add CanvasGroup for fading
        if (buildingPanel != null)
        {
            canvasGroup = buildingPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = buildingPanel.AddComponent<CanvasGroup>();
            }
            
            // Start hidden
            canvasGroup.alpha = 0f;
            buildingPanel.SetActive(false);
        }
    }

    public void ShowBuildingName(string buildingName)
    {
        nearbyBuildings.Add(buildingName);
        
        // Update to show the newly entered building
        currentBuilding = buildingName;
        
        if (buildingPanel != null && buildingNameText != null)
        {
            buildingPanel.SetActive(true);
            buildingNameText.text = buildingName;
            
            // Fade in
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
    }

    public void HideBuildingName(string buildingName)
    {
        nearbyBuildings.Remove(buildingName);
        
        // If we just left the currently displayed building
        if (currentBuilding == buildingName)
        {
            // Check if there are other buildings nearby
            if (nearbyBuildings.Count > 0)
            {
                // Show another nearby building
                foreach (string otherBuilding in nearbyBuildings)
                {
                    currentBuilding = otherBuilding;
                    if (buildingNameText != null)
                    {
                        buildingNameText.text = otherBuilding;
                    }
                    break;
                }
            }
            else
            {
                // No more buildings nearby, hide the UI
                currentBuilding = "";
                StopAllCoroutines();
                StartCoroutine(FadeOut());
            }
        }
    }

    System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        while (canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }

    System.Collections.IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        while (canvasGroup.alpha > 0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        if (buildingPanel != null)
        {
            buildingPanel.SetActive(false);
        }
    }
}
