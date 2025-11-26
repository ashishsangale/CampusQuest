using UnityEngine;

public class TileColliderFixer : MonoBehaviour
{
    void Start()
    {
        // Apply to this object and all children
        AddCollidersRecursive(transform);
    }

    void AddCollidersRecursive(Transform obj)
    {
        foreach (Transform child in obj)
        {
            var mf = child.GetComponent<MeshFilter>();
            var mr = child.GetComponent<MeshRenderer>();

            // Only add if there's a mesh and no existing collider
            if (mf != null && mr != null && child.GetComponent<Collider>() == null)
            {
                child.gameObject.AddComponent<MeshCollider>();
            }

            // Go deeper into children
            AddCollidersRecursive(child);
        }
    }
}
