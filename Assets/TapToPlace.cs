using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TapToPlace : MonoBehaviour
{
    public GameObject prefabToPlace;
    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> hits = new();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
{
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        Vector2 touchPos = Input.GetTouch(0).position;

        if (raycastManager.Raycast(touchPos, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Instantiate(prefabToPlace, hitPose.position, hitPose.rotation);
            Debug.Log("Placed object at: " + hitPose.position);
        }
        else
        {
            #if UNITY_EDITOR
            // Fallback: Place 2 meters in front of camera
            Camera cam = Camera.main;
            Vector3 spawnPos = cam.transform.position + cam.transform.forward * 2f;
            Instantiate(prefabToPlace, spawnPos, Quaternion.identity);
            Debug.Log("Editor fallback spawn at: " + spawnPos);
            #endif
        }
    }
}

}

