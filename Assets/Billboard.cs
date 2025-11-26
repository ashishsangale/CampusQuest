using UnityEngine;
public class Billboard : MonoBehaviour
{
    Camera cam;
    void Start() => cam = Camera.main;
    void LateUpdate()
    {
        if (!cam) return;
        var dir = transform.position - cam.transform.position;
        dir.y = 0f; // keep level
        if (dir.sqrMagnitude > 1e-4)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}
