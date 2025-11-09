using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;

public class BuildingDetector : MonoBehaviour
{
    [Header("References")]
    public AbstractMap map;
    public TextAsset geoJsonFile;
    
    [Header("Detection Settings")]
    public float detectionRadius = 15f; // Radius of the trigger zone around buildings
    public LayerMask buildingLayer; // Layer for building triggers
    
    [Header("Prefab")]
    public GameObject buildingTriggerPrefab; // Optional: custom prefab for triggers
    
    private List<BuildingTrigger> buildingTriggers = new List<BuildingTrigger>();

    void Start()
    {
        if (map == null)
        {
            Debug.LogError("BuildingDetector: Map reference is missing!");
            return;
        }

        if (geoJsonFile == null)
        {
            Debug.LogError("BuildingDetector: GeoJSON file is missing!");
            return;
        }

        // Wait for map to initialize
        if (map.IsAccessTokenValid)
        {
            StartCoroutine(WaitForMapAndSpawnTriggers());
        }
        else
        {
            Debug.LogError("BuildingDetector: Invalid Mapbox access token!");
        }
    }

    IEnumerator WaitForMapAndSpawnTriggers()
    {
        // Wait until map is initialized
        while (!map.IsAccessTokenValid || map.CenterLatitudeLongitude == null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Give extra time for map to fully load
        yield return new WaitForSeconds(1f);

        SpawnBuildingTriggers();
    }

    void SpawnBuildingTriggers()
    {
        try
        {
            JObject geoJson = JObject.Parse(geoJsonFile.text);
            JArray features = (JArray)geoJson["features"];

            int count = 0;

            foreach (JToken feature in features)
            {
                JObject properties = (JObject)feature["properties"];
                
                // Check if this feature is a building with a name
                if (properties["building"] != null && properties["name"] != null)
                {
                    string buildingName = properties["name"].ToString();
                    JObject geometry = (JObject)feature["geometry"];
                    
                    if (geometry["type"].ToString() == "Polygon")
                    {
                        // Get the building's coordinates
                        JArray coordinates = (JArray)geometry["coordinates"];
                        if (coordinates != null && coordinates.Count > 0)
                        {
                            JArray outerRing = (JArray)coordinates[0];
                            
                            // Calculate center of the building polygon
                            Vector2d center = CalculatePolygonCenter(outerRing);
                            
                            // Convert to world position
                            Vector3 worldPos = map.GeoToWorldPosition(center, false);
                            
                            // Create trigger zone at building location
                            CreateBuildingTrigger(buildingName, worldPos);
                            count++;
                        }
                    }
                }
            }

            Debug.Log($"BuildingDetector: Spawned {count} building triggers");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"BuildingDetector: Error parsing GeoJSON: {e.Message}");
        }
    }

    Vector2d CalculatePolygonCenter(JArray coordinates)
    {
        double sumLat = 0;
        double sumLon = 0;
        int count = 0;

        foreach (JArray coord in coordinates)
        {
            if (coord.Count >= 2)
            {
                double lon = coord[0].Value<double>();
                double lat = coord[1].Value<double>();
                sumLon += lon;
                sumLat += lat;
                count++;
            }
        }

        if (count > 0)
        {
            return new Vector2d(sumLat / count, sumLon / count);
        }

        return new Vector2d(0, 0);
    }

    void CreateBuildingTrigger(string buildingName, Vector3 position)
    {
        GameObject triggerObj;
        
        if (buildingTriggerPrefab != null)
        {
            triggerObj = Instantiate(buildingTriggerPrefab, position, Quaternion.identity, transform);
        }
        else
        {
            // Create a simple sphere trigger
            triggerObj = new GameObject($"Trigger_{buildingName}");
            triggerObj.transform.SetParent(transform);
            triggerObj.transform.position = position;
            
            // Add sphere collider as trigger
            SphereCollider collider = triggerObj.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = detectionRadius;
        }

        // Add the BuildingTrigger component
        BuildingTrigger trigger = triggerObj.GetComponent<BuildingTrigger>();
        if (trigger == null)
        {
            trigger = triggerObj.AddComponent<BuildingTrigger>();
        }
        
        trigger.buildingName = buildingName;
        buildingTriggers.Add(trigger);
    }

    void OnDrawGizmos()
    {
        // Visualize building trigger zones in the editor
        if (buildingTriggers != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            foreach (BuildingTrigger trigger in buildingTriggers)
            {
                if (trigger != null)
                {
                    Gizmos.DrawWireSphere(trigger.transform.position, detectionRadius);
                }
            }
        }
    }
}
