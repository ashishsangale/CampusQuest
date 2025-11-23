using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

public class PlaceChestAtStudentUnion : MonoBehaviour
{
    [SerializeField] private AbstractMap map;      // Mapbox map
    [SerializeField] private Transform chest;      // Treasure (chest) transform

    [SerializeField] private float forwardOffsetMeters = 8f; // how far IN FRONT of the building
    [SerializeField] private float heightOffset = 0.5f;      // small lift above ground

    private void Start()
    {
        // Centroid of "Arizona State University Student Union" from asu_poly.json
        Vector2d studentUnionLatLon = new Vector2d(33.30717878, -111.67709498); // lat, lon

        // World position of building center
        Vector3 centerWorld = map.GeoToWorldPosition(studentUnionLatLon, true);

        // "Front" = a bit in front of the map’s forward direction (you can flip sign if needed)
        Vector3 frontDir = -map.transform.forward.normalized;

        Vector3 targetPos = centerWorld
                            + frontDir * forwardOffsetMeters
                            + Vector3.up * heightOffset;

        chest.position = targetPos;
    }
}
