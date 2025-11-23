using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class SimonSaysPlacer : MonoBehaviour
{
    [SerializeField] private AbstractMap map;              // drag Map object here
    [SerializeField] private GameObject simonPanelPrefab;  // drag SimonPanel prefab here

    [SerializeField] private float forwardOffset = 10f;    // meters in front of building
    [SerializeField] private float heightOffset = 1f;      // meters above ground

    private void Start()
    {
        // Centroid of "Arizona State University Student Union" from asu_poly.json
        Vector2d studentUnionLatLon = new Vector2d(33.3071620, -111.6770916);

        // Convert geo position to Unity world position
        Vector3 basePos = map.GeoToWorldPosition(studentUnionLatLon, true);

        // Offset so puzzle is slightly in front of building
        Vector3 spawnPos = basePos + new Vector3(0f, heightOffset, forwardOffset);

        // Spawn SimonSays panel in world at that position
        Instantiate(simonPanelPrefab, spawnPos, Quaternion.identity, map.transform);
    }
}
