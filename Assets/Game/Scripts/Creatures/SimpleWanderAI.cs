using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Simple AI that makes creatures wander around randomly
    /// Perfect for fireflies, slimes, and ambient creatures
    /// Attach to any GameObject you want to wander
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SimpleWanderAI : MonoBehaviour
    {
        [Header("Wander Settings")]
        [Tooltip("How far from starting position can it wander?")]
        public float wanderRadius = 10f;

        [Tooltip("How fast does it move?")]
        public float moveSpeed = 1f;

        [Tooltip("Minimum wait time between moves (seconds)")]
        public float waitTimeMin = 2f;

        [Tooltip("Maximum wait time between moves (seconds)")]
        public float waitTimeMax = 5f;

        [Header("Rotation")]
        [Tooltip("Should creature rotate to face movement direction?")]
        public bool rotateTowardsTarget = true;

        [Tooltip("How fast to rotate")]
        public float rotationSpeed = 5f;

        [Header("Ground Detection")]
        [Tooltip("Raycast down to keep creature on ground?")]
        public bool stayOnGround = true;

        [Tooltip("Layers considered as ground")]
        public LayerMask groundLayer = -1; // Default: all layers

        [Tooltip("Maximum distance to raycast down")]
        public float groundCheckDistance = 10f;

        [Header("Obstacle Avoidance")]
        [Tooltip("Check for obstacles before moving?")]
        public bool avoidObstacles = true;

        [Tooltip("Layers considered as obstacles")]
        public LayerMask obstacleLayer = -1;

        [Tooltip("Distance to check for obstacles")]
        public float obstacleCheckDistance = 1f;

        [Header("Animation (Optional)")]
        [Tooltip("Animator component (if creature has animations)")]
        public Animator animator;

        [Tooltip("Name of 'Speed' parameter in animator")]
        public string speedParameter = "Speed";

        [Header("Debug")]
        public bool showDebugGizmos = true;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float waitTimer;
        private bool isWaiting;
        private bool isMoving;

        private void Start()
        {
            startPosition = transform.position;
            PickNewTarget();
        }

        private void Update()
        {
            if (isWaiting)
            {
                // Wait before moving again
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    PickNewTarget();
                }
            }
            else
            {
                // Move toward target
                isMoving = true;
                MoveTowardsTarget();

                // Check if reached target
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                if (distanceToTarget < 0.5f)
                {
                    // Reached target, wait before picking new one
                    isWaiting = true;
                    isMoving = false;
                    waitTimer = Random.Range(waitTimeMin, waitTimeMax);
                }
            }

            // Update animator if present
            UpdateAnimator();
        }

        private void MoveTowardsTarget()
        {
            Vector3 currentPos = transform.position;
            Vector3 direction = (targetPosition - currentPos).normalized;

            // Check for obstacles
            if (avoidObstacles && CheckForObstacle(direction))
            {
                // Pick new target if blocked
                PickNewTarget();
                return;
            }

            // Move
            Vector3 newPosition = currentPos + direction * moveSpeed * Time.deltaTime;

            // Keep on ground if enabled
            if (stayOnGround)
            {
                newPosition = KeepOnGround(newPosition);
            }

            transform.position = newPosition;

            // Rotate to face direction
            if (rotateTowardsTarget && direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void PickNewTarget()
        {
            int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                // Pick random position within radius of start position
                Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
                Vector3 potentialTarget = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

                // If obstacle avoidance is off, accept immediately
                if (!avoidObstacles)
                {
                    targetPosition = potentialTarget;
                    break;
                }

                // Check if target is reachable (not blocked)
                Vector3 directionToTarget = (potentialTarget - transform.position).normalized;
                if (!CheckForObstacle(directionToTarget))
                {
                    targetPosition = potentialTarget;
                    break;
                }

                attempts++;
            }

            // Make sure target is on the ground
            if (stayOnGround)
            {
                targetPosition = KeepOnGround(targetPosition);
            }
        }

        private bool CheckForObstacle(Vector3 direction)
        {
            return Physics.Raycast(
                transform.position,
                direction,
                obstacleCheckDistance,
                obstacleLayer
            );
        }

        private Vector3 KeepOnGround(Vector3 position)
        {
            // Raycast from above to find ground
            Vector3 rayStart = position + Vector3.up * groundCheckDistance;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckDistance * 2f, groundLayer))
            {
                return hit.point;
            }

            // If no ground found, return original position
            return position;
        }

        private void UpdateAnimator()
        {
            if (animator == null) return;

            // Set speed parameter based on movement
            float speed = isMoving ? moveSpeed : 0f;

            if (animator.parameters.Length > 0)
            {
                foreach (var param in animator.parameters)
                {
                    if (param.name == speedParameter && param.type == AnimatorControllerParameterType.Float)
                    {
                        animator.SetFloat(speedParameter, speed);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Set new wander center (useful for following player or changing zones)
        /// </summary>
        public void SetWanderCenter(Vector3 newCenter)
        {
            startPosition = newCenter;
            PickNewTarget();
        }

        /// <summary>
        /// Pause wandering
        /// </summary>
        public void PauseWandering()
        {
            enabled = false;
            isMoving = false;
        }

        /// <summary>
        /// Resume wandering
        /// </summary>
        public void ResumeWandering()
        {
            enabled = true;
        }

        // Visualize wander radius and target in editor
        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            Vector3 center = Application.isPlaying ? startPosition : transform.position;

            // Draw wander radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, wanderRadius);

            // Draw current target
            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(targetPosition, 0.3f);

                // Draw line to target
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, targetPosition);
            }

            // Draw forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}
