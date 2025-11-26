using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Newtonsoft.Json.Linq;

public class CoinSpawner : MonoBehaviour
{
    [Header("Map Settings")]
    public AbstractMap map;
    public TextAsset geoJsonFile;

    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public float coinHeight = 0.5f; // Height above ground

    [Header("Spawn Settings")]
    public int totalCoinsToSpawn = 50;
    public float spawnInterval = 5f;
    public int maxActiveCoins = 20;
    public float minDistanceBetweenCoins = 5f;

    [Header("Spawn Locations")]
    public bool spawnInParks = true;
    public bool spawnOnPathways = true;
    public bool spawnNearBuildings = true;
    public int coinsPerPark = 5;
    public int coinsPerPathway = 3;
    public int coinsPerBuilding = 2;
    public float buildingSpawnRadius = 10f;

    [Header("Animation")]
    public bool rotateCoins = true;
    public float rotationSpeed = 50f;
    public bool bobCoins = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    private int coinsSpawned = 0;
    private List<GameObject> activeCoins = new List<GameObject>();
    private List<Vector3> allSpawnedPositions = new List<Vector3>();
    private List<Vector3> availableSpawnPositions = new List<Vector3>();

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.totalCoinsNeeded = totalCoinsToSpawn;

        if (map == null)
        {
            Debug.LogError("CoinSpawner: Map reference is missing!");
            return;
        }

        if (geoJsonFile == null)
        {
            Debug.LogError("CoinSpawner: GeoJSON file is missing!");
            return;
        }

        StartCoroutine(WaitForMapAndSpawnCoins());
    }

    IEnumerator WaitForMapAndSpawnCoins()
    {
        // Wait for map to initialize
        yield return new WaitForSeconds(2.5f);

        // Generate all possible spawn positions from GeoJSON
        GenerateSpawnPositionsFromGeoJSON();

        // Start spawning coins gradually
        StartCoroutine(SpawnCoinsOverTime());
    }

    void GenerateSpawnPositionsFromGeoJSON()
    {
        if (geoJsonFile == null)
        {
            Debug.LogError("CoinSpawner: No GeoJSON file assigned!");
            return;
        }

        JObject geoJson = JObject.Parse(geoJsonFile.text);
        JArray features = (JArray)geoJson["features"];
        Debug.Log($"CoinSpawner: Processing {features.Count} features from GeoJSON");

        int parksProcessed = 0;
        int pathwaysProcessed = 0;
        int buildingsProcessed = 0;

        foreach (JToken feature in features)
        {
            JObject properties = (JObject)feature["properties"];
            if (properties == null) continue;

            string leisure = properties["leisure"]?.ToString() ?? "";
            string highway = properties["highway"]?.ToString() ?? "";
            string building = properties["building"]?.ToString() ?? "";

            // Generate spawn positions in parks
            if (spawnInParks && (leisure == "park" || leisure == "garden"))
            {
                GeneratePositionsInArea(feature, coinsPerPark);
                parksProcessed++;
            }

            // Generate spawn positions on pathways
            if (spawnOnPathways && (highway == "footway" || highway == "path" || highway == "pedestrian"))
            {
                GeneratePositionsAlongPath(feature, coinsPerPathway);
                pathwaysProcessed++;
            }

            // Generate spawn positions near buildings
            if (spawnNearBuildings && !string.IsNullOrEmpty(building))
            {
                GeneratePositionsNearBuilding(feature, coinsPerBuilding);
                buildingsProcessed++;
            }
        }

        Debug.Log($"CoinSpawner: Generated {availableSpawnPositions.Count} potential coin positions");
        Debug.Log($"Parks: {parksProcessed}, Pathways: {pathwaysProcessed}, Buildings: {buildingsProcessed}");
    }

    void GeneratePositionsInArea(JToken feature, int coinCount)
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
        int maxAttempts = coinCount * 20;

        while (generated < coinCount && attempts < maxAttempts)
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
                    worldPos.y = coinHeight;
                    availableSpawnPositions.Add(worldPos);
                    generated++;
                }
            }
        }
    }

    void GeneratePositionsAlongPath(JToken feature, int coinCount)
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
                    worldPos.y = coinHeight;
                    pathPoints.Add(worldPos);
                }
            }
        }

        if (pathPoints.Count < 2) return;

        // Spawn coins along the path
        for (int i = 0; i < coinCount && i < pathPoints.Count; i++)
        {
            int index = Random.Range(0, pathPoints.Count);
            availableSpawnPositions.Add(pathPoints[index]);
        }
    }

    void GeneratePositionsNearBuilding(JToken feature, int coinCount)
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

        for (int i = 0; i < coinCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(3f, buildingSpawnRadius);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                coinHeight,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );

            availableSpawnPositions.Add(buildingWorldPos + offset);
        }
    }

    IEnumerator SpawnCoinsOverTime()
    {
        if (availableSpawnPositions.Count == 0)
        {
            Debug.LogWarning("CoinSpawner: No spawn positions available!");
            yield break;
        }

        while (coinsSpawned < totalCoinsToSpawn)
        {
            // Remove collected coins from active list
            activeCoins.RemoveAll(c => c == null);

            if (activeCoins.Count < maxActiveCoins && availableSpawnPositions.Count > 0)
            {
                SpawnOneCoin();
                coinsSpawned++;
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log($"CoinSpawner: Finished spawning {coinsSpawned} coins");
    }

    void SpawnOneCoin()
    {
        if (coinPrefab == null || availableSpawnPositions.Count == 0) return;

        // Pick a random position from available positions
        int randomIndex = Random.Range(0, availableSpawnPositions.Count);
        Vector3 spawnPos = availableSpawnPositions[randomIndex];

        // Check if too close to existing coins
        if (IsTooCloseToExisting(spawnPos))
        {
            availableSpawnPositions.RemoveAt(randomIndex);
            return;
        }

        // Spawn the coin
        GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        coin.name = $"Coin_{coinsSpawned}";

        // Add animation component if needed
        if (rotateCoins || bobCoins)
        {
            CoinAnimation anim = coin.GetComponent<CoinAnimation>();
            if (anim == null)
            {
                anim = coin.AddComponent<CoinAnimation>();
            }
            anim.shouldRotate = rotateCoins;
            anim.rotationSpeed = rotationSpeed;
            anim.shouldBob = bobCoins;
            anim.bobSpeed = bobSpeed;
            anim.bobHeight = bobHeight;
        }

        activeCoins.Add(coin);
        allSpawnedPositions.Add(spawnPos);
        availableSpawnPositions.RemoveAt(randomIndex);
    }

    bool IsTooCloseToExisting(Vector3 position)
    {
        foreach (Vector3 existingPos in allSpawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < minDistanceBetweenCoins)
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

        return centerWorldPos + new Vector3(worldOffsetX, coinHeight, worldOffsetZ);
    }

    public int GetActiveCoinsCount()
    {
        activeCoins.RemoveAll(c => c == null);
        return activeCoins.Count;
    }

    public List<Vector3> GetAllCoinPositions()
    {
        return new List<Vector3>(allSpawnedPositions);
    }
}

// Simple coin animation component
public class CoinAnimation : MonoBehaviour
{
    public bool shouldRotate = true;
    public float rotationSpeed = 50f;
    public bool shouldBob = true;
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (shouldRotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        if (shouldBob)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
