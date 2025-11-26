using UnityEngine;
using System.Collections;

public class NPCWanderer : MonoBehaviour
{
    [Header("Wandering Settings")]
    public float wanderRadius = 10f;         // How far the NPC can wander from spawn point
    public float moveSpeed = 1.5f;           // Movement speed (matches walk speed)
    public float rotationSpeed = 3f;         // How fast NPC rotates to face direction
    
    [Header("Timing")]
    public float minWaitTime = 2f;           // Minimum time to wait at destination
    public float maxWaitTime = 5f;           // Maximum time to wait at destination
    public float minWalkTime = 3f;           // Minimum time before choosing new destination
    public float maxWalkTime = 8f;           // Maximum time before choosing new destination
    
    [Header("Animation System")]
    public AnimationSystemType animationSystem = AnimationSystemType.CreativeCharacters;
    public Animator animator;                // Animator component
    
    [Header("Animation Parameters - Simple System")]
    public string walkAnimationParam = "IsWalking"; // For simple bool-based animations
    
    [Header("Animation Parameters - Creative Characters")]
    public string horizontalParam = "Hor";   // Horizontal axis parameter
    public string verticalParam = "Vert";    // Vertical axis parameter  
    public string stateParam = "State";      // State parameter (0=idle, 1=run)
    
    private Vector3 spawnPoint;              // Original spawn position
    private Vector3 targetPosition;          // Current target position
    private bool isWalking = false;
    private float waitTimer = 0f;
    private float walkTimer = 0f;
    private CharacterController characterController;
    
    public enum AnimationSystemType
    {
        Simple,              // Uses boolean IsWalking parameter
        CreativeCharacters,  // Uses Hor, Vert, State parameters (your current system)
        None                 // No animation
    }
    
    void Start()
    {
        // Store the spawn point
        spawnPoint = transform.position;
        
        // Try to get animator if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Try to get CharacterController if present
        characterController = GetComponent<CharacterController>();
        
        // Start wandering after a random delay
        StartCoroutine(WanderRoutine());
    }
    
    IEnumerator WanderRoutine()
    {
        // Initial random delay before starting
        yield return new WaitForSeconds(Random.Range(0f, 3f));
        
        while (true)
        {
            // Choose a new random destination within wander radius
            ChooseNewDestination();
            
            // Walk to destination
            isWalking = true;
            
            walkTimer = Random.Range(minWalkTime, maxWalkTime);
            
            while (walkTimer > 0)
            {
                MoveTowardsTarget();
                walkTimer -= Time.deltaTime;
                
                // Check if we've reached the destination
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                if (distanceToTarget < 0.5f)
                {
                    break; // Reached destination
                }
                
                yield return null;
            }
            
            // Stop and wait
            isWalking = false;
            SetAnimationState(Vector2.zero, false);
            
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTimer);
        }
    }
    
    void ChooseNewDestination()
    {
        // Random point within wander radius
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        targetPosition = spawnPoint + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Make sure the target is on the same Y level as spawn point (keep on ground)
        targetPosition.y = spawnPoint.y;
    }
    
    void MoveTowardsTarget()
    {
        // Calculate direction to target
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep movement horizontal
        
        // Calculate movement
        Vector3 movement = direction * moveSpeed * Time.deltaTime;
        
        // Move using CharacterController if available, otherwise use transform
        if (characterController != null && characterController.enabled)
        {
            // Add gravity
            movement.y = -2f * Time.deltaTime;
            characterController.Move(movement);
        }
        else
        {
            transform.position += movement;
        }
        
        // Rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Update animation based on local movement direction
        Vector3 localDirection = transform.InverseTransformDirection(direction);
        Vector2 animAxis = new Vector2(localDirection.x, localDirection.z);
        SetAnimationState(animAxis, isWalking);
    }
    
    void SetAnimationState(Vector2 movementAxis, bool walking)
    {
        if (animator == null) return;
        
        switch (animationSystem)
        {
            case AnimationSystemType.Simple:
                // Simple boolean-based animation
                if (!string.IsNullOrEmpty(walkAnimationParam))
                {
                    animator.SetBool(walkAnimationParam, walking);
                }
                break;
                
            case AnimationSystemType.CreativeCharacters:
                // Creative Characters system (Hor, Vert, State)
                if (walking)
                {
                    // Set movement axes (normalized -1 to 1)
                    animator.SetFloat(horizontalParam, movementAxis.x, 0.1f, Time.deltaTime);
                    animator.SetFloat(verticalParam, movementAxis.y, 0.1f, Time.deltaTime);
                    animator.SetFloat(stateParam, 0f); // 0 = walk, 1 = run
                }
                else
                {
                    // Idle state
                    animator.SetFloat(horizontalParam, 0f, 0.1f, Time.deltaTime);
                    animator.SetFloat(verticalParam, 0f, 0.1f, Time.deltaTime);
                    animator.SetFloat(stateParam, 0f);
                }
                break;
                
            case AnimationSystemType.None:
                // No animation
                break;
        }
    }
    
    // Visualize wander radius in editor
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? spawnPoint : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(center, wanderRadius);
        
        if (Application.isPlaying && isWalking)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.3f);
        }
    }
    
    // Public method to make NPC walk to a specific position
    public void WalkToPosition(Vector3 position)
    {
        targetPosition = position;
        isWalking = true;
    }
    
    // Public method to stop wandering temporarily
    public void StopWandering()
    {
        StopAllCoroutines();
        isWalking = false;
        SetAnimationState(Vector2.zero, false);
    }
    
    // Public method to resume wandering
    public void ResumeWandering()
    {
        StartCoroutine(WanderRoutine());
    }
}
