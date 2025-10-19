using UnityEngine;

public class Pulse : MonoBehaviour
{
    [SerializeField] private float speed = 4f;
    [SerializeField] private float scaleAmount = 0.1f;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f; // 0..1
        float s = 1f + t * scaleAmount;                       // 1..1+scaleAmount
        transform.localScale = baseScale * s;
    }
}
