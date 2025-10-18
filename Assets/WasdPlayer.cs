using Mapbox.Unity.Map;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class WasdPlayer : MonoBehaviour
{
    public AbstractMap map;
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;
    public float gravity = -18f;
    public bool topDownNorthUp = true;

    private CharacterController _controller;
    private Vector3 _velocity;

    void Awake() => _controller = GetComponent<CharacterController>();

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0, v).normalized;

        Vector3 moveDir = topDownNorthUp
            ? input
            : transform.TransformDirection(input);

        float speed = Input.GetKey(KeyCode.LeftShift) ? moveSpeed * sprintMultiplier : moveSpeed;

        if (_controller.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;

        Vector3 step = moveDir * speed + new Vector3(0f, _velocity.y, 0f);
        _controller.Move(step * Time.deltaTime);

        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    [ContextMenu("Teleport To Map Center")]
    void TeleportToMapCenter()
    {
        var worldPos = map.GeoToWorldPosition(map.CenterLatitudeLongitude);
        transform.position = new Vector3((float)worldPos.x, 1.2f, (float)worldPos.y);
    }
}
