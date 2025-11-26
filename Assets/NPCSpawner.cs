using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [Header("Map Settings")]
    public AbstractMap map;               // Assign your AbstractMap
    public TextAsset geojsonFile;         // Drag your GeoJSON file into the inspector
    
    [Header("NPC Settings")]
    public GameObject[] npcPrefabs;       // Array of different NPC prefabs
    public int npcsPerBuilding = 2;       // NPCs to spawn near each building
    public float buildingSpawnRadius = 15f; // How far from building center to spawn NPCs
    
    [Header("Pathway Settings")]
    public int npcsOnPathways = 10;       // NPCs to spawn on pathways/roads
    
    [Header("Special Location Settings")]
    public int npcsAtParks = 3;           // NPCs at parks/leisure areas
    public int npcsAtSports = 2;          // NPCs at sports facilities
    
    [Header("Spawn Settings")]
    public float minDistanceBetweenNPCs = 3f; // Minimum distance between NPCs
    public bool spawnAtBuildings = true;
    public bool spawnAtParks = true;
    public bool spawnAtSports = true;
    
    [Header("NPC Behavior")]
    public bool randomRotation = true;    // Random facing direction
    public bool addWandering = false;     // Add wandering behavior (requires additional script)
    
    private List<Vector3> allSpawnedPositions = new List<Vector3>();

    void Start()
    {
        // Wait for the map to initialize before spawning NPCs
        if (map != null)
        {
            StartCoroutine(WaitForMapAndSpawnNPCs());
        }
        else
        {
            Debug.LogError("Map is not assigned! Please assign the AbstractMap in the inspector.");
        }
    }
    
    IEnumerator WaitForMapAndSpawnNPCs()
    {
        // Wait for the map to initialize
        yield return new WaitForSeconds(2.5f);
        
        // Check if map is properly initialized by testing a known coordinate
        Vector2d testCoord = new Vector2d(33.3074569702148, -111.680725097656);
        Vector3 testWorldPos = map.GeoToWorldPosition(testCoord, true);
        
        Debug.Log($"NPC Spawner - Map initialization test: {testCoord} converts to {testWorldPos}");
        
        if (testWorldPos == Vector3.zero)
        {
            Debug.LogError("Map is not properly initialized for NPC spawning.");
            yield break;
        }

        // Validate prefab arrays
        if (npcPrefabs == null || npcPrefabs.Length == 0)
        {
            Debug.LogError("No NPC prefabs assigned! Please assign at least one NPC prefab.");
            yield break;
        }
        
        SpawnNPCsFromGeoJSON();
    }

    void SpawnNPCsFromGeoJSON()
    {
        if (geojsonFile == null)
        {
            Debug.LogError("No GeoJSON file assigned!");
            return;
        }

        JObject geojson = JObject.Parse(geojsonFile.text);
        var features = geojson["features"] as JArray;
        Debug.Log($"NPC Spawner: Found {features?.Count ?? 0} features in GeoJSON");

        int buildingsProcessed = 0;
        int parksProcessed = 0;
        int sportsProcessed = 0;

        foreach (var feature in features)
        {
            var properties = feature["properties"];
            if (properties == null) continue;

            string building = properties["building"]?.ToString() ?? "";
            string leisure = properties["leisure"]?.ToString() ?? "";
            string sport = properties["sport"]?.ToString() ?? "";
            string amenity = properties["amenity"]?.ToString() ?? "";

            // Spawn NPCs near buildings (university buildings, libraries, cafes, etc.)
            if (spawnAtBuildings && !string.IsNullOrEmpty(building))
            {
                string buildingName = properties["name"]?.ToString() ?? "Unknown Building";
                SpawnNPCsNearBuilding(feature, buildingName);
                buildingsProcessed++;
            }

            // Spawn NPCs at parks and leisure areas
            if (spawnAtParks && leisure == "park")
            {
                string parkName = properties["name"]?.ToString() ?? "Park";
                SpawnNPCsInArea(feature, parkName, npcsAtParks, "Park");
                parksProcessed++;
            }

            // Spawn NPCs at sports facilities
            if (spawnAtSports && !string.IsNullOrEmpty(sport))
            {
                string sportName = properties["name"]?.ToString() ?? $"{sport} facility";
                SpawnNPCsInArea(feature, sportName, npcsAtSports, "Sports");
                sportsProcessed++;
            }
        }

        Debug.Log($"NPC Spawning complete: {buildingsProcessed} buildings, {parksProcessed} parks, {sportsProcessed} sports areas");
        Debug.Log($"Total NPCs spawned: {allSpawnedPositions.Count}");
    }

    void SpawnNPCsNearBuilding(JToken feature, string buildingName)
    {
        var geometry = feature["geometry"];
        if (geometry == null || geometry["type"]?.ToString() != "Polygon") return;

        var coordinates = geometry["coordinates"] as JArray;
        if (coordinates == null || coordinates.Count == 0) return;

        var coords = coordinates[0] as JArray;
        if (coords == null || coords.Count == 0) return;

        // Calculate the center of the building
        Vector2d buildingCenter = CalculatePolygonCenter(coords);
        Vector3 buildingWorldPos = map.GeoToWorldPosition(buildingCenter, true);

        if (buildingWorldPos == Vector3.zero) return;

        Debug.Log($"Spawning NPCs near building: {buildingName}");

        // Spawn NPCs around the building
        for (int i = 0; i < npcsPerBuilding; i++)
        {
            // Random position around the building
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(5f, buildingSpawnRadius);
            
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            Vector3 spawnPos = buildingWorldPos + offset;

            // Check minimum distance from other NPCs
            if (IsTooCloseToExisting(spawnPos)) continue;

            SpawnNPC(spawnPos, $"NPC_Building_{buildingName}_{i}");
        }
    }

    void SpawnNPCsInArea(JToken feature, string areaName, int npcCount, string areaType)
    {
        var geometry = feature["geometry"];
        if (geometry == null || geometry["type"]?.ToString() != "Polygon") return;

        var coordinates = geometry["coordinates"] as JArray;
        if (coordinates == null || coordinates.Count == 0) return;

        var coords = coordinates[0] as JArray;
        if (coords == null || coords.Count == 0) return;

        // Convert to polygon
        List<Vector2d> polygon = new List<Vector2d>();
        foreach (var coord in coords)
        {
            var coordArray = coord as JArray;
            if (coordArray != null && coordArray.Count >= 2)
            {
                double lon = coordArray[0].ToObject<double>();
                double lat = coordArray[1].ToObject<double>();
                polygon.Add(new Vector2d(lat, lon));
            }
        }

        if (polygon.Count < 3) return;

        // Get polygon bounds
        double minLat = double.MaxValue, maxLat = double.MinValue;
        double minLon = double.MaxValue, maxLon = double.MinValue;
        foreach (var p in polygon)
        {
            minLat = Mathf.Min((float)minLat, (float)p.x);
            maxLat = Mathf.Max((float)maxLat, (float)p.x);
            minLon = Mathf.Min((float)minLon, (float)p.y);
            maxLon = Mathf.Max((float)maxLon, (float)p.y);
        }

        Vector2d centerPoint = new Vector2d((minLat + maxLat) / 2, (minLon + maxLon) / 2);
        Vector3 centerWorldPos = map.GeoToWorldPosition(centerPoint, true);

        Debug.Log($"Spawning NPCs in {areaType}: {areaName}");

        int spawned = 0;
        int attempts = 0;
        int maxAttempts = npcCount * 10;

        while (spawned < npcCount && attempts < maxAttempts)
        {
            attempts++;

            // Random point in bounding box
            double lat = Random.Range((float)minLat, (float)maxLat);
            double lon = Random.Range((float)minLon, (float)maxLon);
            Vector2d randomPoint = new Vector2d(lat, lon);

            // Check if point is inside polygon
            if (IsPointInPolygon(randomPoint, polygon))
            {
                Vector3 worldPos = map.GeoToWorldPosition(randomPoint, true);

                if (worldPos == Vector3.zero)
                {
                    worldPos = CalculateFallbackPosition(randomPoint, centerPoint, centerWorldPos);
                }

                if (worldPos == Vector3.zero || IsTooCloseToExisting(worldPos)) continue;

                SpawnNPC(worldPos, $"NPC_{areaType}_{areaName}_{spawned}");
                spawned++;
            }
        }

        Debug.Log($"Spawned {spawned} NPCs in {areaName}");
    }

    void SpawnNPC(Vector3 position, string npcName)
    {
        // Randomly select an NPC prefab
        GameObject selectedPrefab = npcPrefabs[Random.Range(0, npcPrefabs.Length)];

        // Random rotation
        Quaternion rotation = randomRotation 
            ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) 
            : Quaternion.identity;

        // Spawn the NPC
        GameObject npc = Instantiate(selectedPrefab, position, rotation);
        npc.name = npcName;

        // Add slight random scale variation (90% - 110%)
        float scaleVariation = Random.Range(0.9f, 1.1f);
        npc.transform.localScale *= scaleVariation;

        // Track spawned position
        allSpawnedPositions.Add(position);

        // Optional: Add wandering behavior if you have a wandering script
        if (addWandering)
        {
            // You can add your wandering component here
            var wanderer = npc.AddComponent<NPCWanderer>();
            wanderer.wanderRadius = 5f;
        }
    }

    bool IsTooCloseToExisting(Vector3 position)
    {
        foreach (Vector3 existingPos in allSpawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < minDistanceBetweenNPCs)
            {
                return true;
            }
        }
        return false;
    }

    Vector2d CalculatePolygonCenter(JArray coords)
    {
        double sumLat = 0;
        double sumLon = 0;
        int count = 0;

        foreach (var coord in coords)
        {
            var coordArray = coord as JArray;
            if (coordArray != null && coordArray.Count >= 2)
            {
                sumLon += coordArray[0].ToObject<double>();
                sumLat += coordArray[1].ToObject<double>();
                count++;
            }
        }

        return new Vector2d(sumLat / count, sumLon / count);
    }

    bool IsPointInPolygon(Vector2d point, List<Vector2d> polygon)
    {
        bool inside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; j = i++)
        {
            if (((polygon[i].x > point.x) != (polygon[j].x > point.x)) &&
                (point.y < (polygon[j].y - polygon[i].y) * (point.x - polygon[i].x) / (polygon[j].x - polygon[i].x) + polygon[i].y))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    Vector3 CalculateFallbackPosition(Vector2d targetPoint, Vector2d centerPoint, Vector3 centerWorldPos)
    {
        if (centerWorldPos == Vector3.zero) return Vector3.zero;

        double latOffset = targetPoint.x - centerPoint.x;
        double lonOffset = targetPoint.y - centerPoint.y;

        float metersPerDegreeLat = 111000f;
        float metersPerDegreeLon = 111000f * Mathf.Cos(Mathf.Deg2Rad * (float)centerPoint.x);

        float worldOffsetX = (float)lonOffset * metersPerDegreeLon;
        float worldOffsetZ = (float)latOffset * metersPerDegreeLat;

        return centerWorldPos + new Vector3(worldOffsetX, 0, worldOffsetZ);
    }

    // Optional: Public method to get all spawned NPC positions (useful for other systems)
    public List<Vector3> GetAllNPCPositions()
    {
        return new List<Vector3>(allSpawnedPositions);
    }

    // Optional: Public method to spawn additional NPCs at runtime
    public void SpawnAdditionalNPC(Vector3 position)
    {
        if (!IsTooCloseToExisting(position))
        {
            SpawnNPC(position, $"NPC_Additional_{allSpawnedPositions.Count}");
        }
    }
}
