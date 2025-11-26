using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Newtonsoft.Json.Linq;

public class RandomSpawnManager : MonoBehaviour
{
    [Header("Map Settings")]
    public AbstractMap map;
    public TextAsset geoJsonFile;
    public Transform player;

    [Header("Prefabs")]
    public GameObject[] hazardPrefabs;
    public GameObject[] healthPrefabs;

    [Header("Spawn Settings")]
    public int minItems = 5;
    public int maxItems = 15;
    public float minDistanceFromPlayer = 10f;
    public float minDistanceBetweenItems = 8f;
    public float spawnInterval = 8f;

    [Header("Spawn Locations")]
    public bool spawnInParks = true;
    public bool spawnOnPathways = true;
    public bool spawnNearBuildings = true;
    public bool spawnInSportsAreas = true;
    public int itemsPerPark = 3;
    public int itemsPerPathway = 2;
    public int itemsPerBuilding = 1;
    public int itemsPerSportsArea = 2;
    public float buildingSpawnRadius = 12f;

    [Header("Spawn Ratio")]
    [Range(0f, 1f)]
    public float hazardRatio = 0.5f; // 0.5 = 50% hazards, 50% health

    private List<GameObject> activeItems = new List<GameObject>();
    private List<Vector3> availableSpawnPositions = new List<Vector3>();
    private List<Vector3> allSpawnedPositions = new List<Vector3>();

    void Start()
    {
        if (map == null)
        {
            Debug.LogError("RandomSpawnManager: Map reference is missing!");
            return;
        }

        if (geoJsonFile == null)
        {
            Debug.LogError("RandomSpawnManager: GeoJSON file is missing!");
            return;
        }

        StartCoroutine(WaitForMapAndGeneratePositions());
    }

    IEnumerator WaitForMapAndGeneratePositions()
    {
        // Wait for map to initialize
        yield return new WaitForSeconds(2.5f);

        // Generate all possible spawn positions from GeoJSON
        GenerateSpawnPositionsFromGeoJSON();

        // Start with minimum items
        for (int i = 0; i < minItems; i++)
        {
            TrySpawn();
        }

        // Start continuous spawning loop
        StartCoroutine(SpawnLoop());
    }

    void GenerateSpawnPositionsFromGeoJSON()
    {
        if (geoJsonFile == null)
        {
            Debug.LogError("RandomSpawnManager: No GeoJSON file assigned!");
            return;
        }

        JObject geoJson = JObject.Parse(geoJsonFile.text);
        JArray features = (JArray)geoJson["features"];
        Debug.Log($"RandomSpawnManager: Processing {features.Count} features from GeoJSON");

        int parksProcessed = 0;
        int pathwaysProcessed = 0;
        int buildingsProcessed = 0;
        int sportsProcessed = 0;

        foreach (JToken feature in features)
        {
            JObject properties = (JObject)feature["properties"];
            if (properties == null) continue;

            string leisure = properties["leisure"]?.ToString() ?? "";
            string highway = properties["highway"]?.ToString() ?? "";
            string building = properties["building"]?.ToString() ?? "";
            string sport = properties["sport"]?.ToString() ?? "";

            // Generate positions in parks
            if (spawnInParks && (leisure == "park" || leisure == "garden"))
            {
                GeneratePositionsInArea(feature, itemsPerPark);
                parksProcessed++;
            }

            // Generate positions on pathways
            if (spawnOnPathways && (highway == "footway" || highway == "path" || highway == "pedestrian"))
            {
                GeneratePositionsAlongPath(feature, itemsPerPathway);
                pathwaysProcessed++;
            }

            // Generate positions near buildings
            if (spawnNearBuildings && !string.IsNullOrEmpty(building))
            {
                GeneratePositionsNearBuilding(feature, itemsPerBuilding);
                buildingsProcessed++;
            }

            // Generate positions at sports areas
            if (spawnInSportsAreas && !string.IsNullOrEmpty(sport))
            {
                GeneratePositionsInArea(feature, itemsPerSportsArea);
                sportsProcessed++;
            }
        }

        Debug.Log($"RandomSpawnManager: Generated {availableSpawnPositions.Count} potential spawn positions");
        Debug.Log($"Parks: {parksProcessed}, Pathways: {pathwaysProcessed}, Buildings: {buildingsProcessed}, Sports: {sportsProcessed}");
    }

    void GeneratePositionsInArea(JToken feature, int itemCount)
    {
        JObject geometry = (JObject)feature["geometry"];
        if (geometry == null || geometry["type"]?.ToString() != "Polygon") return;

        JArray coordinates = (JArray)geometry["coordinates"];
        if (coordinates == null || coordinates.Count == 0) return;

        JArray coords = (JArray)coordinates[0];
        if (coords == null || coords.Count == 0) return;

        // Convert to polygon
        List<Vector2d> polygon = new List<Vector2d>();
        foreach (JToken coord in coords)
        {
            JArray coordArray = (JArray)coord;
            if (coordArray != null && coordArray.Count >= 2)
            {
                double lon = coordArray[0].Value<double>();
                double lat = coordArray[1].Value<double>();
                polygon.Add(new Vector2d(lat, lon));
            }
        }

        if (polygon.Count < 3) return;

        // Get bounds
        double minLat = double.MaxValue, maxLat = double.MinValue;
        double minLon = double.MaxValue, maxLon = double.MinValue;
        foreach (Vector2d p in polygon)
        {
            minLat = Mathf.Min((float)minLat, (float)p.x);
            maxLat = Mathf.Max((float)maxLat, (float)p.x);
            minLon = Mathf.Min((float)minLon, (float)p.y);
            maxLon = Mathf.Max((float)maxLon, (float)p.y);
        }

        Vector2d centerPoint = new Vector2d((minLat + maxLat) / 2, (minLon + maxLon) / 2);
        Vector3 centerWorldPos = map.GeoToWorldPosition(centerPoint, true);

        int generated = 0;
        int attempts = 0;
        int maxAttempts = itemCount * 20;

        while (generated < itemCount && attempts < maxAttempts)
        {
            attempts++;

            double lat = Random.Range((float)minLat, (float)maxLat);
            double lon = Random.Range((float)minLon, (float)maxLon);
            Vector2d randomPoint = new Vector2d(lat, lon);

            if (IsPointInPolygon(randomPoint, polygon))
            {
                Vector3 worldPos = map.GeoToWorldPosition(randomPoint, true);

                if (worldPos == Vector3.zero)
                {
                    worldPos = CalculateFallbackPosition(randomPoint, centerPoint, centerWorldPos);
                }

                if (worldPos != Vector3.zero)
                {
                    worldPos.y = 0.5f; // Height above ground
                    availableSpawnPositions.Add(worldPos);
                    generated++;
                }
            }
        }
    }

    void GeneratePositionsAlongPath(JToken feature, int itemCount)
    {
        JObject geometry = (JObject)feature["geometry"];
        if (geometry == null || geometry["type"]?.ToString() != "LineString") return;

        JArray coordinates = (JArray)geometry["coordinates"];
        if (coordinates == null || coordinates.Count < 2) return;

        // Get path points
        List<Vector3> pathPoints = new List<Vector3>();
        foreach (JToken coord in coordinates)
        {
            JArray coordArray = (JArray)coord;
            if (coordArray != null && coordArray.Count >= 2)
            {
                double lon = coordArray[0].Value<double>();
                double lat = coordArray[1].Value<double>();
                Vector2d geoPos = new Vector2d(lat, lon);
                Vector3 worldPos = map.GeoToWorldPosition(geoPos, true);

                if (worldPos != Vector3.zero)
                {
                    worldPos.y = 0.5f;
                    pathPoints.Add(worldPos);
                }
            }
        }

        if (pathPoints.Count < 2) return;

        // Add random points along the path
        for (int i = 0; i < itemCount && i < pathPoints.Count; i++)
        {
            int index = Random.Range(0, pathPoints.Count);
            availableSpawnPositions.Add(pathPoints[index]);
        }
    }

    void GeneratePositionsNearBuilding(JToken feature, int itemCount)
    {
        JObject geometry = (JObject)feature["geometry"];
        if (geometry == null || geometry["type"]?.ToString() != "Polygon") return;

        JArray coordinates = (JArray)geometry["coordinates"];
        if (coordinates == null || coordinates.Count == 0) return;

        JArray coords = (JArray)coordinates[0];
        if (coords == null || coords.Count == 0) return;

        Vector2d buildingCenter = CalculatePolygonCenter(coords);
        Vector3 buildingWorldPos = map.GeoToWorldPosition(buildingCenter, true);

        if (buildingWorldPos == Vector3.zero) return;

        for (int i = 0; i < itemCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(5f, buildingSpawnRadius);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0.5f,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            availableSpawnPositions.Add(buildingWorldPos + offset);
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Remove collected/destroyed items
            activeItems.RemoveAll(item => item == null);

            // Spawn new items if below max
            if (activeItems.Count < maxItems)
            {
                TrySpawn();
            }

            // Ensure minimum items are always present
            while (activeItems.Count < minItems)
            {
                TrySpawn();
            }
        }
    }

    bool TrySpawn()
    {
        if (availableSpawnPositions.Count == 0)
        {
            Debug.LogWarning("RandomSpawnManager: No spawn positions available!");
            return false;
        }

        // Pick random position
        int randomIndex = Random.Range(0, availableSpawnPositions.Count);
        Vector3 spawnPos = availableSpawnPositions[randomIndex];

        // Check distance from player
        if (player != null && Vector3.Distance(player.position, spawnPos) < minDistanceFromPlayer)
        {
            return false;
        }

        // Check distance from other items
        if (IsTooCloseToExisting(spawnPos))
        {
            return false;
        }

        // Decide if it's a hazard or health based on ratio
        bool isHazard = Random.value < hazardRatio;
        GameObject[] prefabArray = isHazard ? hazardPrefabs : healthPrefabs;

        if (prefabArray == null || prefabArray.Length == 0)
        {
            Debug.LogWarning($"RandomSpawnManager: No {(isHazard ? "hazard" : "health")} prefabs assigned!");
            return false;
        }

        // Pick random prefab from array
        GameObject prefab = prefabArray[Random.Range(0, prefabArray.Length)];
        if (prefab == null) return false;

        // Spawn the item
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        obj.name = $"{(isHazard ? "Hazard" : "Health")}_{activeItems.Count}";

        // Connect to manager if it has SpawnItem component
        SpawnItem item = obj.GetComponent<SpawnItem>();
        if (item != null)
        {
            item.manager = this;
        }

        activeItems.Add(obj);
        allSpawnedPositions.Add(spawnPos);

        return true;
    }

    bool IsTooCloseToExisting(Vector3 position)
    {
        foreach (GameObject item in activeItems)
        {
            if (item != null && Vector3.Distance(position, item.transform.position) < minDistanceBetweenItems)
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

        foreach (JToken coord in coords)
        {
            JArray coordArray = (JArray)coord;
            if (coordArray != null && coordArray.Count >= 2)
            {
                sumLon += coordArray[0].Value<double>();
                sumLat += coordArray[1].Value<double>();
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

        return centerWorldPos + new Vector3(worldOffsetX, 0.5f, worldOffsetZ);
    }

    public void RemoveFromList(GameObject obj)
    {
        activeItems.Remove(obj);
    }

    public int GetActiveItemsCount()
    {
        activeItems.RemoveAll(item => item == null);
        return activeItems.Count;
    }

    public List<Vector3> GetAllSpawnedPositions()
    {
        return new List<Vector3>(allSpawnedPositions);
    }
}

