using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TreeSpawnerFromGeoJSON : MonoBehaviour
{
    [Header("Map Settings")]
    public AbstractMap map;               // Assign your AbstractMap
    public TextAsset geojsonFile;         // Drag your GeoJSON file into the inspector
    
    [Header("Tree Settings")]
    public GameObject[] treePrefabs;      // Array of different tree prefabs
    public int treesPerPolygon = 20;      // Number of trees to spawn per polygon
    
    [Header("Grass Settings")]
    public GameObject[] grassPrefabs;     // Array of different grass prefabs
    public int grassItemsPerPolygon = 80; // Number of grass items to spawn per polygon (more dense than trees)
    
    [Header("Spawn Settings")]
    public float minDistanceBetweenObjects = 2f; // Minimum distance between spawned objects
    public bool spawnTrees = true;        // Toggle tree spawning
    public bool spawnGrass = true;        // Toggle grass spawning

    void Start()
    {
        // Wait for the map to initialize before spawning trees
        if (map != null)
        {
            StartCoroutine(WaitForMapAndSpawnTrees());
        }
        else
        {
            Debug.LogError("Map is not assigned! Please assign the AbstractMap in the inspector.");
        }
    }
    
    System.Collections.IEnumerator WaitForMapAndSpawnTrees()
    {
        // Wait a few frames for the map to initialize
        yield return new WaitForSeconds(2f);
        
        // Check if map is properly initialized by testing a known coordinate
        Vector2d testCoord = new Vector2d(33.3074569702148, -111.680725097656); // Center of ASU Polytechnic area
        Vector3 testWorldPos = map.GeoToWorldPosition(testCoord, true);
        
        Debug.Log($"Map initialization test: Coordinate {testCoord} converts to {testWorldPos}");
        
        if (testWorldPos == Vector3.zero)
        {
            Debug.LogError("Map is not properly initialized or coordinates are outside map bounds.");
            Debug.LogError("Make sure your AbstractMap is configured with the correct center point and zoom level for ASU Polytechnic campus.");
            Debug.LogError("Recommended center: Lat=33.307, Lon=-111.681, Zoom level 15-17");
            yield break;
        }

        // Validate prefab arrays
        if (spawnTrees && (treePrefabs == null || treePrefabs.Length == 0))
        {
            Debug.LogWarning("Tree spawning is enabled but no tree prefabs are assigned!");
        }
        if (spawnGrass && (grassPrefabs == null || grassPrefabs.Length == 0))
        {
            Debug.LogWarning("Grass spawning is enabled but no grass prefabs are assigned!");
        }
        
        SpawnTreesFromGeoJSON();
    }

    void SpawnTreesFromGeoJSON()
    {
        if (geojsonFile == null)
        {
            Debug.LogError("No GeoJSON file assigned!");
            return;
        }

        JObject geojson = JObject.Parse(geojsonFile.text);
        var features = geojson["features"] as JArray;
        Debug.Log($"Found {features?.Count ?? 0} features in GeoJSON");

        foreach (var feature in features)
        {
            var properties = feature["properties"];
            if (properties == null) continue;

            string leisure = properties["leisure"]?.ToString() ?? "";
            string landuse = properties["landuse"]?.ToString() ?? "";
            Debug.Log($"Feature: leisure='{leisure}', landuse='{landuse}'");

            // Only spawn in parks or grass landuse
            if (leisure == "park" || landuse == "grass")
            {
                Debug.Log($"Found matching feature: leisure='{leisure}', landuse='{landuse}'");
                var geometry = feature["geometry"];
                if (geometry == null) 
                {
                    Debug.LogWarning("Geometry is null");
                    continue;
                }
                
                Debug.Log($"Geometry type: {geometry["type"]}");
                if (geometry["type"].ToString() != "Polygon") 
                {
                    Debug.LogWarning($"Geometry type is not Polygon: {geometry["type"]}");
                    continue;
                }

                var coordinates = geometry["coordinates"] as JArray;
                if (coordinates == null || coordinates.Count == 0)
                {
                    Debug.LogWarning("No coordinates found");
                    continue;
                }
                
                var coords = coordinates[0] as JArray; // outer ring
                if (coords == null || coords.Count == 0)
                {
                    Debug.LogWarning("No coordinate points found");
                    continue;
                }
                
                Debug.Log($"Polygon has {coords.Count} points");
                List<Vector2d> polygon = new List<Vector2d>();

                foreach (var coord in coords)
                {
                    var coordArray = coord as JArray;
                    if (coordArray != null && coordArray.Count >= 2)
                    {
                        double lon = coordArray[0].ToObject<double>();
                        double lat = coordArray[1].ToObject<double>();
                        
                        // Log first few coordinates to verify parsing
                        if (polygon.Count < 3)
                        {
                            Debug.Log($"Parsed coordinate {polygon.Count}: Lon={lon}, Lat={lat}");
                        }
                        
                        // Validate coordinates are reasonable (roughly Arizona coordinates)
                        if (lat < 30 || lat > 40 || lon < -120 || lon > -100)
                        {
                            Debug.LogWarning($"Coordinate seems outside Arizona bounds: Lat={lat}, Lon={lon}");
                        }
                        
                        polygon.Add(new Vector2d(lat, lon));
                    }
                }

                if (polygon.Count >= 3)
                {
                    Debug.Log($"Spawning objects in polygon with {polygon.Count} vertices");
                    ScatterObjectsInPolygon(polygon);
                }
                else
                {
                    Debug.LogWarning($"Polygon has insufficient vertices: {polygon.Count}");
                }
            }
        }
    }

    void ScatterObjectsInPolygon(List<Vector2d> polygon)
    {
        if (polygon.Count < 3) 
        {
            Debug.LogWarning($"Polygon has too few vertices: {polygon.Count}");
            return;
        }

        // Log some sample coordinates to verify they look correct
        Debug.Log($"Sample polygon coordinates:");
        for (int i = 0; i < Mathf.Min(3, polygon.Count); i++)
        {
            Debug.Log($"  Point {i}: Lat={polygon[i].x}, Lon={polygon[i].y}");
        }

        // Compute polygon bounds (rough bounding box)
        double minLat = double.MaxValue, maxLat = double.MinValue;
        double minLon = double.MaxValue, maxLon = double.MinValue;
        foreach (var p in polygon)
        {
            minLat = Mathf.Min((float)minLat, (float)p.x);
            maxLat = Mathf.Max((float)maxLat, (float)p.x);
            minLon = Mathf.Min((float)minLon, (float)p.y);
            maxLon = Mathf.Max((float)maxLon, (float)p.y);
        }

        Debug.Log($"Polygon bounds: Lat({minLat}, {maxLat}), Lon({minLon}, {maxLon})");
        
        // Test the map conversion with the center of the bounding box
        double centerLat = (minLat + maxLat) / 2;
        double centerLon = (minLon + maxLon) / 2;
        Vector2d centerPoint = new Vector2d(centerLat, centerLon);
        Vector3 centerWorldPos = Vector3.zero;
        
        if (map != null)
        {
            centerWorldPos = map.GeoToWorldPosition(centerPoint, true);
            Debug.Log($"Center point Lat={centerLat}, Lon={centerLon} converts to world position: {centerWorldPos}");
        }
        else
        {
            Debug.LogError("Map is null - cannot test coordinate conversion!");
            return;
        }

        // Keep track of spawned positions to maintain minimum distance
        List<Vector3> spawnedPositions = new List<Vector3>();

        // Spawn trees first
        if (spawnTrees && treePrefabs != null && treePrefabs.Length > 0)
        {
            SpawnObjectType("Trees", treePrefabs, treesPerPolygon, polygon, minLat, maxLat, minLon, maxLon, 
                           centerPoint, centerWorldPos, spawnedPositions);
        }

        // Then spawn grass
        if (spawnGrass && grassPrefabs != null && grassPrefabs.Length > 0)
        {
            SpawnObjectType("Grass", grassPrefabs, grassItemsPerPolygon, polygon, minLat, maxLat, minLon, maxLon, 
                           centerPoint, centerWorldPos, spawnedPositions);
        }
    }

    void SpawnObjectType(string objectTypeName, GameObject[] prefabs, int targetCount, List<Vector2d> polygon,
                        double minLat, double maxLat, double minLon, double maxLon,
                        Vector2d centerPoint, Vector3 centerWorldPos, List<Vector3> spawnedPositions)
    {
        int objectsSpawned = 0;
        int attempts = 0;
        int maxAttempts = targetCount * 5; // Give more attempts to find valid positions

        Debug.Log($"Starting to spawn {objectTypeName}...");

        while (objectsSpawned < targetCount && attempts < maxAttempts)
        {
            attempts++;
            
            // Random geographic point in bounding box
            double lat = Random.Range((float)minLat, (float)maxLat);
            double lon = Random.Range((float)minLon, (float)maxLon);
            Vector2d randomPoint = new Vector2d(lat, lon);

            // Check if point is actually inside polygon
            if (IsPointInPolygon(randomPoint, polygon))
            {
                Vector3 worldPos = map.GeoToWorldPosition(randomPoint, true);
                
                // Check if the conversion resulted in a valid position
                if (worldPos == Vector3.zero)
                {
                    // Fallback: Use relative positioning based on the center of the polygon
                    worldPos = CalculateFallbackPosition(randomPoint, centerPoint, centerWorldPos);
                }
                
                // Skip spawning if position is still zero
                if (worldPos == Vector3.zero)
                {
                    continue;
                }

                // Check minimum distance from other objects
                bool tooClose = false;
                foreach (Vector3 existingPos in spawnedPositions)
                {
                    if (Vector3.Distance(worldPos, existingPos) < minDistanceBetweenObjects)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose)
                {
                    continue; // Try another position
                }
                
                // Randomly select a prefab from the array
                GameObject selectedPrefab = prefabs[Random.Range(0, prefabs.Length)];
                
                // Random rotation for variety
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                
                // Spawn the object
                GameObject spawnedObject = Instantiate(selectedPrefab, worldPos, randomRotation);
                spawnedPositions.Add(worldPos);
                objectsSpawned++;
                
                // Optional: Add some random scale variation
                float scaleVariation = Random.Range(0.8f, 1.2f);
                spawnedObject.transform.localScale *= scaleVariation;
            }
        }

        Debug.Log($"Successfully spawned {objectsSpawned} {objectTypeName} out of {targetCount} requested (took {attempts} attempts)");
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
        // If center world position is also zero, we can't use fallback
        if (centerWorldPos == Vector3.zero)
        {
            return Vector3.zero;
        }
        
        // Calculate offset from center in geographic coordinates
        double latOffset = targetPoint.x - centerPoint.x;
        double lonOffset = targetPoint.y - centerPoint.y;
        
        // Convert to approximate world coordinates using a rough scale
        // This is a simplified conversion - 1 degree ≈ 111km, but longitude varies by latitude
        float metersPerDegreeLat = 111000f;
        float metersPerDegreeLon = 111000f * Mathf.Cos(Mathf.Deg2Rad * (float)centerPoint.x);
        
        // Convert to Unity world units (assuming 1 Unity unit = 1 meter, adjust if needed)
        float worldOffsetX = (float)lonOffset * metersPerDegreeLon;
        float worldOffsetZ = (float)latOffset * metersPerDegreeLat;
        
        Vector3 fallbackPosition = centerWorldPos + new Vector3(worldOffsetX, 0, worldOffsetZ);
        
        Debug.Log($"Fallback calculation: LatOffset={latOffset}, LonOffset={lonOffset}, WorldOffset=({worldOffsetX}, {worldOffsetZ})");
        
        return fallbackPosition;
    }
}
