using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Economy;

namespace CozyGame.FastTravel
{
    /// <summary>
    /// Waypoint data class
    /// </summary>
    [System.Serializable]
    public class Waypoint
    {
        [Header("Basic Info")]
        public string waypointID;
        public string waypointName;
        public string description;
        public Sprite icon;

        [Header("Location")]
        public Vector3 position;
        public Quaternion rotation = Quaternion.identity;
        public GameObject visualPrefab; // Portal/waypoint model

        [Header("Unlock")]
        public bool isUnlocked = false;
        public bool startUnlocked = false;
        public int requiredLevel = 1;
        public string requiredQuestID = "";

        [Header("Travel Cost")]
        public bool hasTravelCost = true;
        public CurrencyType costCurrencyType = CurrencyType.Gold;
        public int baseCost = 100;
        public bool costScalesWithDistance = true;
        public float costPerMeter = 1f;

        [Tooltip("Mana cost for travel")]
        public bool requiresMana = false;
        public float manaCost = 50f;

        [Header("Visual")]
        public Color glowColor = Color.cyan;
        public bool showBeam = true;

        public Waypoint(string id, string name, Vector3 pos)
        {
            waypointID = id;
            waypointName = name;
            position = pos;
        }

        /// <summary>
        /// Can player unlock this waypoint?
        /// </summary>
        public bool CanUnlock(out string reason)
        {
            reason = "";

            if (isUnlocked)
            {
                reason = "Already unlocked";
                return false;
            }

            // Check level
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                reason = $"Requires level {requiredLevel}";
                return false;
            }

            // Check quest
            if (!string.IsNullOrEmpty(requiredQuestID))
            {
                // TODO: Check if quest completed
                // For now, assume can unlock
            }

            return true;
        }

        /// <summary>
        /// Calculate travel cost to this waypoint from current position
        /// </summary>
        public int CalculateTravelCost(Vector3 fromPosition)
        {
            if (!hasTravelCost)
                return 0;

            int cost = baseCost;

            if (costScalesWithDistance)
            {
                float distance = Vector3.Distance(fromPosition, position);
                cost += Mathf.RoundToInt(distance * costPerMeter);
            }

            return cost;
        }

        /// <summary>
        /// Can travel to this waypoint?
        /// </summary>
        public bool CanTravel(Vector3 fromPosition, out string reason)
        {
            reason = "";

            if (!isUnlocked)
            {
                reason = "Waypoint not unlocked";
                return false;
            }

            // Check currency cost
            if (hasTravelCost)
            {
                int cost = CalculateTravelCost(fromPosition);
                if (!CurrencyManager.Instance.HasCurrency(costCurrencyType, cost))
                {
                    reason = $"Need {cost} {costCurrencyType}";
                    return false;
                }
            }

            // Check mana cost
            if (requiresMana && Magic.MagicSystem.Instance != null)
            {
                if (Magic.MagicSystem.Instance.currentMana < manaCost)
                {
                    reason = $"Need {manaCost} mana";
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Waypoint save data
    /// </summary>
    [System.Serializable]
    public class WaypointSaveData
    {
        public List<string> unlockedWaypointIDs = new List<string>();
    }

    /// <summary>
    /// Waypoint system singleton.
    /// Manages fast travel waypoints and teleportation.
    /// </summary>
    public class WaypointSystem : MonoBehaviour
    {
        public static WaypointSystem Instance { get; private set; }

        [Header("Waypoints")]
        [Tooltip("All waypoints in the world")]
        public List<Waypoint> waypoints = new List<Waypoint>();

        [Header("Travel Settings")]
        [Tooltip("Enable fast travel")]
        public bool fastTravelEnabled = true;

        [Tooltip("Travel animation duration")]
        public float travelAnimationDuration = 2f;

        [Tooltip("Fade to black during travel")]
        public bool useFadeTransition = true;

        [Tooltip("Particle effect on departure")]
        public GameObject departureEffect;

        [Tooltip("Particle effect on arrival")]
        public GameObject arrivalEffect;

        [Header("Restrictions")]
        [Tooltip("Can't travel during combat")]
        public bool blockDuringCombat = true;

        [Tooltip("Can't travel indoors")]
        public bool blockIndoors = false;

        [Header("Events")]
        public UnityEvent<Waypoint> OnWaypointUnlocked;
        public UnityEvent<Waypoint, Waypoint> OnTravelStarted; // from, to
        public UnityEvent<Waypoint> OnTravelCompleted;
        public UnityEvent<string> OnTravelFailed; // reason

        // State
        private bool isTraveling = false;
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize waypoint system
        /// </summary>
        private void Initialize()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Unlock starting waypoints
            foreach (var waypoint in waypoints)
            {
                if (waypoint.startUnlocked)
                {
                    waypoint.isUnlocked = true;
                }

                // Add map marker for waypoint
                if (Map.MapSystem.Instance != null)
                {
                    Map.MapSystem.Instance.AddMarker(
                        waypoint.waypointID,
                        waypoint.waypointName,
                        Map.MapMarkerType.Waypoint,
                        waypoint.position,
                        waypoint.isUnlocked
                    );
                }
            }

            Debug.Log($"[WaypointSystem] Initialized with {waypoints.Count} waypoints");
        }

        /// <summary>
        /// Unlock waypoint
        /// </summary>
        public bool UnlockWaypoint(string waypointID)
        {
            Waypoint waypoint = GetWaypoint(waypointID);
            if (waypoint == null)
            {
                Debug.LogWarning($"[WaypointSystem] Waypoint {waypointID} not found!");
                return false;
            }

            string reason;
            if (!waypoint.CanUnlock(out reason))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(reason, Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.red);
                }
                return false;
            }

            waypoint.isUnlocked = true;

            // Update map marker
            if (Map.MapSystem.Instance != null)
            {
                Map.MapMarker marker = Map.MapSystem.Instance.GetMarker(waypointID);
                if (marker != null)
                {
                    Map.MapSystem.Instance.DiscoverMarker(marker);
                }
            }

            // Trigger event
            OnWaypointUnlocked?.Invoke(waypoint);

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show($"Waypoint Unlocked: {waypoint.waypointName}!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.cyan);
            }

            Debug.Log($"[WaypointSystem] Unlocked waypoint: {waypoint.waypointName}");

            return true;
        }

        /// <summary>
        /// Travel to waypoint
        /// </summary>
        public bool TravelToWaypoint(string waypointID)
        {
            if (!fastTravelEnabled)
            {
                OnTravelFailed?.Invoke("Fast travel is disabled");
                return false;
            }

            if (isTraveling)
            {
                OnTravelFailed?.Invoke("Already traveling");
                return false;
            }

            Waypoint waypoint = GetWaypoint(waypointID);
            if (waypoint == null)
            {
                OnTravelFailed?.Invoke("Waypoint not found");
                return false;
            }

            // Get current position
            Vector3 currentPos = playerTransform != null ? playerTransform.position : Vector3.zero;

            // Check if can travel
            string reason;
            if (!waypoint.CanTravel(currentPos, out reason))
            {
                OnTravelFailed?.Invoke(reason);

                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(reason, Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.red);
                }

                return false;
            }

            // Check combat restriction
            if (blockDuringCombat)
            {
                // TODO: Check if in combat
            }

            // Check indoor restriction
            if (blockIndoors)
            {
                // TODO: Check if indoors
            }

            // Consume costs
            if (waypoint.hasTravelCost)
            {
                int cost = waypoint.CalculateTravelCost(currentPos);
                CurrencyManager.Instance.RemoveCurrency(waypoint.costCurrencyType, cost);
            }

            if (waypoint.requiresMana && Magic.MagicSystem.Instance != null)
            {
                Magic.MagicSystem.Instance.UseMana(waypoint.manaCost);
            }

            // Get current waypoint (nearest)
            Waypoint fromWaypoint = GetNearestWaypoint(currentPos);

            // Trigger event
            OnTravelStarted?.Invoke(fromWaypoint, waypoint);

            // Start travel
            StartCoroutine(TravelCoroutine(waypoint));

            return true;
        }

        /// <summary>
        /// Travel coroutine
        /// </summary>
        private System.Collections.IEnumerator TravelCoroutine(Waypoint waypoint)
        {
            isTraveling = true;

            // Spawn departure effect
            if (departureEffect != null && playerTransform != null)
            {
                GameObject effect = Instantiate(departureEffect, playerTransform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("fast_travel");
            }

            // Fade out
            if (useFadeTransition && UI.FadeUI.Instance != null)
            {
                UI.FadeUI.Instance.FadeOut(travelAnimationDuration * 0.3f);
                yield return new WaitForSeconds(travelAnimationDuration * 0.3f);
            }

            // Teleport player
            if (playerTransform != null)
            {
                playerTransform.position = waypoint.position;
                playerTransform.rotation = waypoint.rotation;

                // Disable character controller temporarily
                CharacterController controller = playerTransform.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                    yield return null;
                    controller.enabled = true;
                }
            }

            // Wait
            yield return new WaitForSeconds(travelAnimationDuration * 0.4f);

            // Spawn arrival effect
            if (arrivalEffect != null)
            {
                GameObject effect = Instantiate(arrivalEffect, waypoint.position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            // Fade in
            if (useFadeTransition && UI.FadeUI.Instance != null)
            {
                UI.FadeUI.Instance.FadeIn(travelAnimationDuration * 0.3f);
                yield return new WaitForSeconds(travelAnimationDuration * 0.3f);
            }

            // Complete
            isTraveling = false;
            OnTravelCompleted?.Invoke(waypoint);

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show($"Arrived at {waypoint.waypointName}", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.green);
            }

            Debug.Log($"[WaypointSystem] Traveled to {waypoint.waypointName}");
        }

        /// <summary>
        /// Get waypoint by ID
        /// </summary>
        public Waypoint GetWaypoint(string waypointID)
        {
            return waypoints.Find(w => w.waypointID == waypointID);
        }

        /// <summary>
        /// Get all unlocked waypoints
        /// </summary>
        public List<Waypoint> GetUnlockedWaypoints()
        {
            return waypoints.FindAll(w => w.isUnlocked);
        }

        /// <summary>
        /// Get nearest waypoint to position
        /// </summary>
        public Waypoint GetNearestWaypoint(Vector3 position)
        {
            Waypoint nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var waypoint in waypoints)
            {
                if (!waypoint.isUnlocked)
                    continue;

                float distance = Vector3.Distance(position, waypoint.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = waypoint;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Is traveling?
        /// </summary>
        public bool IsTraveling()
        {
            return isTraveling;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public WaypointSaveData GetSaveData()
        {
            WaypointSaveData data = new WaypointSaveData();

            foreach (var waypoint in waypoints)
            {
                if (waypoint.isUnlocked)
                {
                    data.unlockedWaypointIDs.Add(waypoint.waypointID);
                }
            }

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(WaypointSaveData data)
        {
            if (data == null)
                return;

            foreach (string waypointID in data.unlockedWaypointIDs)
            {
                Waypoint waypoint = GetWaypoint(waypointID);
                if (waypoint != null)
                {
                    waypoint.isUnlocked = true;

                    // Update map marker
                    if (Map.MapSystem.Instance != null)
                    {
                        Map.MapMarker marker = Map.MapSystem.Instance.GetMarker(waypointID);
                        if (marker != null)
                        {
                            marker.isDiscovered = true;
                        }
                    }
                }
            }

            Debug.Log($"[WaypointSystem] Loaded {data.unlockedWaypointIDs.Count} unlocked waypoints");
        }
    }
}
