using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using CozyGame.Environment;
using CozyGame.Dialogue;

namespace CozyGame.NPCs
{
    /// <summary>
    /// NPC schedule controller component.
    /// Controls NPC behavior based on time of day and weather.
    /// Integrates with TimeManager and WeatherManager.
    /// </summary>
    [RequireComponent(typeof(NPCInteractable))]
    public class NPCScheduleController : MonoBehaviour
    {
        [Header("Schedule")]
        [Tooltip("NPC schedule")]
        public NPCSchedule schedule;

        [Tooltip("Follow schedule automatically")]
        public bool followSchedule = true;

        [Header("Movement")]
        [Tooltip("NavMeshAgent for pathfinding")]
        public NavMeshAgent navAgent;

        [Tooltip("Default movement speed")]
        public float defaultSpeed = 3.5f;

        [Tooltip("Arrival threshold")]
        public float arrivalThreshold = 1f;

        [Header("Wandering")]
        [Tooltip("Wander interval (seconds)")]
        public float wanderInterval = 5f;

        [Tooltip("Wander radius")]
        public float wanderRadius = 10f;

        [Header("Rotation")]
        [Tooltip("Look at player when talking")]
        public bool lookAtPlayerWhenIdle = true;

        [Tooltip("Rotation speed")]
        public float rotationSpeed = 5f;

        [Header("Animation")]
        [Tooltip("Animator")]
        public Animator animator;

        [Tooltip("Speed parameter name")]
        public string speedParameterName = "Speed";

        [Tooltip("Activity parameter name")]
        public string activityParameterName = "Activity";

        [Header("Debug")]
        [Tooltip("Show debug info")]
        public bool showDebug = false;

        [Header("Events")]
        public UnityEvent<NPCScheduleEntry> OnScheduleEntryChanged;
        public UnityEvent<NPCActivityType> OnActivityChanged;
        public UnityEvent OnArrivedAtDestination;

        // State
        private NPCInteractable npcInteractable;
        private NPCScheduleEntry currentEntry;
        private NPCActivityType currentActivity = NPCActivityType.Idle;
        private Transform currentTargetLocation;
        private Vector3 currentWanderCenter;
        private float wanderTimer = 0f;
        private bool hasArrivedAtDestination = false;
        private Transform playerTransform;

        // Weather override
        private bool isWeatherOverride = false;

        private void Start()
        {
            // Get components
            npcInteractable = GetComponent<NPCInteractable>();

            if (navAgent == null)
            {
                navAgent = GetComponent<NavMeshAgent>();
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            // Setup NavMeshAgent
            if (navAgent != null)
            {
                navAgent.speed = defaultSpeed;
                navAgent.stoppingDistance = arrivalThreshold;
            }

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Subscribe to time/weather events
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnHourChanged.AddListener(OnHourChanged);
            }

            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.OnWeatherChanged.AddListener(OnWeatherChanged);
            }

            // Initialize
            currentWanderCenter = transform.position;
            UpdateSchedule();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnHourChanged.RemoveListener(OnHourChanged);
            }

            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.OnWeatherChanged.RemoveListener(OnWeatherChanged);
            }
        }

        private void Update()
        {
            if (!followSchedule)
                return;

            // Update current activity
            UpdateActivity();

            // Update animation
            UpdateAnimation();

            // Look at player if idle
            if (lookAtPlayerWhenIdle && currentActivity == NPCActivityType.Idle && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                if (distance <= npcInteractable.interactionRange)
                {
                    LookAtTarget(playerTransform.position);
                }
            }
        }

        /// <summary>
        /// Update current activity based on schedule entry
        /// </summary>
        private void UpdateActivity()
        {
            if (currentEntry == null && !isWeatherOverride)
                return;

            switch (currentActivity)
            {
                case NPCActivityType.Idle:
                    // Do nothing
                    break;

                case NPCActivityType.Wander:
                    UpdateWandering();
                    break;

                case NPCActivityType.Work:
                case NPCActivityType.Sleep:
                case NPCActivityType.Eat:
                case NPCActivityType.Socialize:
                    // Move to target location
                    if (currentTargetLocation != null && !hasArrivedAtDestination)
                    {
                        MoveToLocation(currentTargetLocation.position);
                    }
                    break;

                case NPCActivityType.Custom:
                    // Custom behavior handled externally
                    break;
            }
        }

        /// <summary>
        /// Update wandering behavior
        /// </summary>
        private void UpdateWandering()
        {
            wanderTimer += Time.deltaTime;

            if (wanderTimer >= wanderInterval)
            {
                wanderTimer = 0f;

                // Pick random point within radius
                Vector3 randomPoint = currentWanderCenter + Random.insideUnitSphere * wanderRadius;
                randomPoint.y = currentWanderCenter.y;

                // Find valid NavMesh position
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    MoveToLocation(hit.position);
                }
            }
        }

        /// <summary>
        /// Move to location using NavMeshAgent
        /// </summary>
        private void MoveToLocation(Vector3 position)
        {
            if (navAgent == null)
                return;

            navAgent.SetDestination(position);

            // Check if arrived
            if (!navAgent.pathPending)
            {
                if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    if (!hasArrivedAtDestination)
                    {
                        hasArrivedAtDestination = true;
                        OnArrivedAtDestination?.Invoke();

                        if (showDebug)
                        {
                            Debug.Log($"[NPCScheduleController] {npcInteractable.npcName} arrived at destination");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Look at target position
        /// </summary>
        private void LookAtTarget(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0f; // Keep rotation on horizontal plane

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Update animation based on movement
        /// </summary>
        private void UpdateAnimation()
        {
            if (animator == null)
                return;

            // Set speed parameter
            if (!string.IsNullOrEmpty(speedParameterName) && navAgent != null)
            {
                float speed = navAgent.velocity.magnitude;
                animator.SetFloat(speedParameterName, speed);
            }

            // Set activity parameter
            if (!string.IsNullOrEmpty(activityParameterName))
            {
                animator.SetInteger(activityParameterName, (int)currentActivity);
            }
        }

        /// <summary>
        /// Hour changed callback
        /// </summary>
        private void OnHourChanged(float currentHour)
        {
            UpdateSchedule();
        }

        /// <summary>
        /// Weather changed callback
        /// </summary>
        private void OnWeatherChanged(WeatherState newWeather)
        {
            UpdateSchedule();
        }

        /// <summary>
        /// Update schedule based on current time and weather
        /// </summary>
        private void UpdateSchedule()
        {
            if (schedule == null || !followSchedule)
                return;

            float currentHour = 12f; // Default
            if (TimeManager.Instance != null)
            {
                currentHour = TimeManager.Instance.GetCurrentHour();
            }

            WeatherState currentWeather = WeatherState.Clear;
            if (WeatherManager.Instance != null)
            {
                currentWeather = WeatherManager.Instance.GetCurrentWeather();
            }

            // Check weather override
            NPCActivityType weatherActivity;
            Transform weatherLocation;
            if (schedule.ShouldOverrideForWeather(currentWeather, out weatherActivity, out weatherLocation))
            {
                isWeatherOverride = true;
                SetActivity(weatherActivity, weatherLocation);

                if (showDebug)
                {
                    Debug.Log($"[NPCScheduleController] {npcInteractable.npcName} weather override: {weatherActivity}");
                }

                return;
            }

            isWeatherOverride = false;

            // Get current schedule entry
            NPCScheduleEntry newEntry = schedule.GetCurrentEntry(currentHour);

            if (newEntry != currentEntry)
            {
                // Schedule changed
                currentEntry = newEntry;
                OnScheduleEntryChanged?.Invoke(currentEntry);

                if (currentEntry != null)
                {
                    SetActivity(currentEntry.activityType, currentEntry.targetLocation);

                    // Update wander center
                    if (currentEntry.targetLocation != null)
                    {
                        currentWanderCenter = currentEntry.targetLocation.position;
                    }

                    // Update speed
                    if (navAgent != null)
                    {
                        navAgent.speed = defaultSpeed * currentEntry.speedMultiplier;
                    }

                    // Update dialogue
                    if (currentEntry.customDialogue != null && npcInteractable != null)
                    {
                        npcInteractable.defaultDialogue = currentEntry.customDialogue;
                    }

                    // Update animation
                    if (animator != null && !string.IsNullOrEmpty(currentEntry.animationStateName))
                    {
                        animator.Play(currentEntry.animationStateName);
                    }

                    if (showDebug)
                    {
                        Debug.Log($"[NPCScheduleController] {npcInteractable.npcName} schedule changed: {currentEntry.activityDescription} ({currentHour:F1}h)");
                    }
                }
                else
                {
                    // No entry, use default
                    SetActivity(schedule.defaultActivity, schedule.defaultLocation);
                }
            }
        }

        /// <summary>
        /// Set current activity
        /// </summary>
        private void SetActivity(NPCActivityType activity, Transform targetLocation)
        {
            if (activity != currentActivity)
            {
                currentActivity = activity;
                OnActivityChanged?.Invoke(currentActivity);
            }

            currentTargetLocation = targetLocation;
            hasArrivedAtDestination = false;

            // Reset wander timer
            if (activity == NPCActivityType.Wander)
            {
                wanderTimer = wanderInterval; // Trigger immediate wander
            }

            // Move to target location if specified
            if (targetLocation != null && activity != NPCActivityType.Wander)
            {
                MoveToLocation(targetLocation.position);
            }
            else if (navAgent != null)
            {
                // Stop moving if no target
                navAgent.ResetPath();
            }
        }

        /// <summary>
        /// Get current activity
        /// </summary>
        public NPCActivityType GetCurrentActivity()
        {
            return currentActivity;
        }

        /// <summary>
        /// Get current schedule entry
        /// </summary>
        public NPCScheduleEntry GetCurrentEntry()
        {
            return currentEntry;
        }

        /// <summary>
        /// Get next schedule entry
        /// </summary>
        public NPCScheduleEntry GetNextEntry()
        {
            if (schedule == null)
                return null;

            float currentHour = 12f;
            if (TimeManager.Instance != null)
            {
                currentHour = TimeManager.Instance.GetCurrentHour();
            }

            return schedule.GetNextEntry(currentHour);
        }

        /// <summary>
        /// Force update schedule (useful after changing schedule at runtime)
        /// </summary>
        public void ForceUpdateSchedule()
        {
            UpdateSchedule();
        }

        /// <summary>
        /// Override current activity (disables schedule following)
        /// </summary>
        public void OverrideActivity(NPCActivityType activity, Transform location = null, float duration = 0f)
        {
            followSchedule = false;
            SetActivity(activity, location);

            if (duration > 0f)
            {
                Invoke(nameof(ResumeSchedule), duration);
            }
        }

        /// <summary>
        /// Resume schedule following
        /// </summary>
        public void ResumeSchedule()
        {
            followSchedule = true;
            UpdateSchedule();
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebug)
                return;

            // Draw current target
            if (currentTargetLocation != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTargetLocation.position);
                Gizmos.DrawWireSphere(currentTargetLocation.position, 0.5f);
            }

            // Draw wander radius
            if (currentActivity == NPCActivityType.Wander)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentWanderCenter, wanderRadius);
            }

            // Draw path
            if (navAgent != null && navAgent.hasPath)
            {
                Gizmos.color = Color.cyan;
                Vector3[] path = navAgent.path.corners;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }

            #if UNITY_EDITOR
            // Draw current activity label
            string label = $"{currentActivity}";
            if (currentEntry != null)
            {
                label += $"\n{currentEntry.activityDescription}";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, label);
            #endif
        }
    }
}
