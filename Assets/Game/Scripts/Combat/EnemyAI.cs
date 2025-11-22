using UnityEngine;
using UnityEngine.AI;

namespace CozyGame.Combat
{
    /// <summary>
    /// Enemy AI states
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Retreat,
        Dead
    }

    /// <summary>
    /// Enemy AI controller.
    /// Handles enemy behavior, pathfinding, and combat.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        [Tooltip("Current AI state")]
        public EnemyState currentState = EnemyState.Idle;

        [Tooltip("Detection range")]
        public float detectionRange = 10f;

        [Tooltip("Attack range")]
        public float attackRange = 2f;

        [Tooltip("Retreat health threshold (0-1)")]
        [Range(0f, 1f)]
        public float retreatHealthThreshold = 0.2f;

        [Header("Movement")]
        [Tooltip("Move speed")]
        public float moveSpeed = 3.5f;

        [Tooltip("Chase speed")]
        public float chaseSpeed = 5f;

        [Tooltip("Rotation speed")]
        public float rotationSpeed = 5f;

        [Header("Patrol")]
        [Tooltip("Enable patrol behavior")]
        public bool enablePatrol = false;

        [Tooltip("Patrol waypoints")]
        public Transform[] patrolWaypoints;

        [Tooltip("Wait time at waypoint")]
        public float waypointWaitTime = 2f;

        private int currentWaypointIndex = 0;
        private float waypointTimer = 0f;

        [Header("Combat")]
        [Tooltip("Damage per attack")]
        public float attackDamage = 10f;

        [Tooltip("Damage type")]
        public DamageType attackDamageType = DamageType.Physical;

        [Tooltip("Attack cooldown (seconds)")]
        public float attackCooldown = 1.5f;

        private float attackTimer = 0f;

        [Tooltip("Attack animation trigger")]
        public string attackAnimationTrigger = "Attack";

        [Header("References")]
        [Tooltip("Target (usually player)")]
        public Transform target;

        [Tooltip("Animator")]
        public Animator animator;

        // Components
        private Health health;
        private NavMeshAgent navAgent;

        // State
        private Vector3 startPosition;
        private Quaternion startRotation;

        private void Awake()
        {
            health = GetComponent<Health>();
            navAgent = GetComponent<NavMeshAgent>();

            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        private void Start()
        {
            // Setup NavMeshAgent
            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
                navAgent.angularSpeed = rotationSpeed * 50f;
            }

            // Subscribe to health events
            if (health != null)
            {
                health.OnDeath.AddListener(OnDeath);
            }

            // Find player as target
            if (target == null && PlayerController.Instance != null)
            {
                target = PlayerController.Instance.transform;
            }
        }

        private void Update()
        {
            if (health != null && health.isDead)
            {
                currentState = EnemyState.Dead;
                return;
            }

            // Update attack timer
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            // AI state machine
            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdle();
                    break;

                case EnemyState.Patrol:
                    UpdatePatrol();
                    break;

                case EnemyState.Chase:
                    UpdateChase();
                    break;

                case EnemyState.Attack:
                    UpdateAttack();
                    break;

                case EnemyState.Retreat:
                    UpdateRetreat();
                    break;
            }

            // Update animator
            UpdateAnimator();
        }

        /// <summary>
        /// Idle state
        /// </summary>
        private void UpdateIdle()
        {
            // Check for target
            if (CanSeeTarget())
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // Start patrol if enabled
            if (enablePatrol && patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        /// <summary>
        /// Patrol state
        /// </summary>
        private void UpdatePatrol()
        {
            // Check for target
            if (CanSeeTarget())
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            if (patrolWaypoints == null || patrolWaypoints.Length == 0)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            Transform waypoint = patrolWaypoints[currentWaypointIndex];
            if (waypoint == null)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                return;
            }

            // Move to waypoint
            if (navAgent != null)
            {
                navAgent.SetDestination(waypoint.position);
                navAgent.speed = moveSpeed;
            }

            // Check if reached waypoint
            if (navAgent != null && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                waypointTimer += Time.deltaTime;

                if (waypointTimer >= waypointWaitTime)
                {
                    waypointTimer = 0f;
                    currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
                }
            }
        }

        /// <summary>
        /// Chase state
        /// </summary>
        private void UpdateChase()
        {
            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            // Check retreat
            if (health != null && health.GetHealthPercentage() <= retreatHealthThreshold)
            {
                ChangeState(EnemyState.Retreat);
                return;
            }

            // Check attack range
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (distanceToTarget <= attackRange)
            {
                ChangeState(EnemyState.Attack);
                return;
            }

            // Check if lost target
            if (distanceToTarget > detectionRange * 1.5f)
            {
                ChangeState(enablePatrol ? EnemyState.Patrol : EnemyState.Idle);
                return;
            }

            // Chase target
            if (navAgent != null)
            {
                navAgent.SetDestination(target.position);
                navAgent.speed = chaseSpeed;
            }
        }

        /// <summary>
        /// Attack state
        /// </summary>
        private void UpdateAttack()
        {
            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            // Check if target moved away
            if (distanceToTarget > attackRange * 1.2f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // Stop movement
            if (navAgent != null)
            {
                navAgent.ResetPath();
            }

            // Face target
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            directionToTarget.y = 0f;

            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Attack
            if (attackTimer <= 0f)
            {
                PerformAttack();
                attackTimer = attackCooldown;
            }
        }

        /// <summary>
        /// Retreat state
        /// </summary>
        private void UpdateRetreat()
        {
            if (target == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            // If health recovered, return to chase
            if (health != null && health.GetHealthPercentage() > retreatHealthThreshold + 0.1f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // Run away from target
            Vector3 retreatDirection = (transform.position - target.position).normalized;
            Vector3 retreatPosition = transform.position + retreatDirection * 5f;

            if (navAgent != null)
            {
                navAgent.SetDestination(retreatPosition);
                navAgent.speed = chaseSpeed;
            }
        }

        /// <summary>
        /// Perform attack
        /// </summary>
        private void PerformAttack()
        {
            // Trigger animation
            if (animator != null && !string.IsNullOrEmpty(attackAnimationTrigger))
            {
                animator.SetTrigger(attackAnimationTrigger);
            }

            // Deal damage to target
            if (target != null)
            {
                Health targetHealth = target.GetComponent<Health>();
                if (targetHealth != null)
                {
                    DamageInfo damageInfo = new DamageInfo(
                        attackDamage,
                        attackDamageType,
                        gameObject,
                        target.gameObject,
                        target.position,
                        (target.position - transform.position).normalized
                    );

                    targetHealth.TakeDamage(damageInfo);
                }
            }
        }

        /// <summary>
        /// Check if can see target
        /// </summary>
        private bool CanSeeTarget()
        {
            if (target == null)
                return false;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            return distanceToTarget <= detectionRange;
        }

        /// <summary>
        /// Change AI state
        /// </summary>
        private void ChangeState(EnemyState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;

            // Reset timers on state change
            waypointTimer = 0f;
        }

        /// <summary>
        /// Update animator
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null)
                return;

            // Speed parameter
            float speed = (navAgent != null) ? navAgent.velocity.magnitude : 0f;
            animator.SetFloat("Speed", speed);

            // State parameters
            animator.SetBool("IsChasing", currentState == EnemyState.Chase);
            animator.SetBool("IsAttacking", currentState == EnemyState.Attack);
            animator.SetBool("IsDead", currentState == EnemyState.Dead);
        }

        /// <summary>
        /// Death callback
        /// </summary>
        private void OnDeath()
        {
            currentState = EnemyState.Dead;

            // Stop NavMeshAgent
            if (navAgent != null)
            {
                navAgent.ResetPath();
                navAgent.enabled = false;
            }

            // Disable this script
            enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Draw patrol waypoints
            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < patrolWaypoints.Length; i++)
                {
                    if (patrolWaypoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(patrolWaypoints[i].position, 0.5f);

                        // Draw line to next waypoint
                        int nextIndex = (i + 1) % patrolWaypoints.Length;
                        if (patrolWaypoints[nextIndex] != null)
                        {
                            Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[nextIndex].position);
                        }
                    }
                }
            }
        }
    }
}
