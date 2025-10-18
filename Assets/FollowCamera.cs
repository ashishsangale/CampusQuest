using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    // Position camera behind and slightly above the player
    public Vector3 offset = new Vector3(0f, 2.0f, -5.0f);
    // Lower values make camera movement more stable but less responsive
    public float positionDamping = 0.15f;
    public float rotationDamping = 0.1f;
    // Offset for where the camera looks at (slightly above the player's pivot)
    public float targetHeightOffset = 1.0f;
    // Camera collision detection (optional)
    public bool enableCollisionDetection = false;
    public float collisionRadius = 0.2f;
    public LayerMask collisionLayers = -1;
    
    // Stability parameters
    [Header("Stability Settings")]
    public bool useSmoothing = true;
    public float minFollowDistance = 0.5f;  // Don't follow tiny movements
    public float maxFollowDistance = 10f;   // Follow faster if player is far away
    
    // Internal state
    private Vector3 currentVelocity;

    // Use FixedUpdate instead of LateUpdate for more stable camera movement
    void FixedUpdate()
    {
        if (!target) return;
        
        // Calculate rotation based on target's forward direction
        Quaternion targetRotation = Quaternion.Euler(0, target.eulerAngles.y, 0);
        
        // Calculate desired position based on rotation and offset
        Vector3 desiredPosition = target.position + targetRotation * offset;
        
        // Smoothly move the camera with improved stability
        Vector3 smoothedPosition;
        
        if (useSmoothing)
        {
            // Get distance from current to desired position
            float distanceToTarget = Vector3.Distance(transform.position, desiredPosition);
            
            // Adjust smoothing based on distance (faster when far away)
            float currentDamping = positionDamping;
            if (distanceToTarget > maxFollowDistance)
            {
                // Reduce damping when far away to catch up faster
                currentDamping *= 0.5f;
            }
            else if (distanceToTarget < minFollowDistance)
            {
                // Ignore very small movements
                smoothedPosition = transform.position;
                goto SkipSmoothing;
            }
            
            // Use SmoothDamp for more natural following motion
            smoothedPosition = Vector3.SmoothDamp(
                transform.position,
                desiredPosition, 
                ref currentVelocity, 
                currentDamping);
        }
        else
        {
            smoothedPosition = desiredPosition;
        }
        
    SkipSmoothing:
        // Handle collision detection if enabled
        if (enableCollisionDetection)
        {
            HandleCollisions(ref smoothedPosition);
        }
        
        // Apply position
        transform.position = smoothedPosition;
        
        // Calculate look target with height offset
        Vector3 lookTarget = target.position + new Vector3(0, targetHeightOffset, 0);
        
        // Smoothly rotate to look at target with improved stability
        Quaternion lookRotation = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            lookRotation, 
            rotationDamping);
    }
    
    // Called from LateUpdate to handle any additional updates that should happen after FixedUpdate
    void LateUpdate()
    {
        // This ensures camera updates that need to happen after physics calculations
        if (!target) return;
        
        // Any additional camera adjustments can go here
    }
    
    private void HandleCollisions(ref Vector3 desiredPosition)
    {
        // Check for collisions between target and camera
        RaycastHit hit;
        Vector3 directionToCamera = desiredPosition - target.position;
        
        if (Physics.SphereCast(target.position, collisionRadius, directionToCamera.normalized, 
            out hit, directionToCamera.magnitude, collisionLayers))
        {
            // Move camera to collision point
            desiredPosition = target.position + directionToCamera.normalized * (hit.distance - collisionRadius);
        }
    }
}
