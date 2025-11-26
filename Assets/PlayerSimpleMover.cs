using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSimpleMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(h, 0f, v);
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Applies gravity internally and stays grounded against colliders
        controller.SimpleMove(move * moveSpeed);
    }
}
