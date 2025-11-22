using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Fishing
{
    /// <summary>
    /// Fishing spot marker.
    /// Defines an area where fish can be caught with specific fish pools.
    /// </summary>
    public class FishingSpot : MonoBehaviour
    {
        [Header("Spot Info")]
        [Tooltip("Spot name")]
        public string spotName = "Fishing Spot";

        [Tooltip("Spot type/tag (lake, river, ocean, pond)")]
        public string spotType = "lake";

        [Tooltip("Fishing spot radius")]
        public float spotRadius = 5f;

        [Header("Fish Pool")]
        [Tooltip("Fish that can be caught here")]
        public Fish[] availableFish;

        [Tooltip("Use global fish pool as well")]
        public bool useGlobalPool = true;

        [Header("Spawn Modifiers")]
        [Tooltip("Overall spawn rate multiplier")]
        [Range(0f, 3f)]
        public float spawnRateMultiplier = 1f;

        [Tooltip("Water depth (affects fish types)")]
        public float waterDepth = 5f;

        [Header("Time & Weather")]
        [Tooltip("Current time of day")]
        public FishTimePreference currentTime = FishTimePreference.Day;

        [Tooltip("Current weather")]
        public FishWeatherPreference currentWeather = FishWeatherPreference.Clear;

        [Header("Visual")]
        [Tooltip("Spot marker visual (ripples, etc.)")]
        public GameObject spotMarkerPrefab;

        [Tooltip("Show spot marker always")]
        public bool alwaysShowMarker = true;

        [Header("Audio")]
        [Tooltip("Ambient water sound")]
        public string ambientSoundName = "water_ambient";

        [Tooltip("Play ambient sound")]
        public bool playAmbientSound = true;

        private GameObject markerInstance;
        private Dictionary<Fish, float> fishWeights = new Dictionary<Fish, float>();

        private void Start()
        {
            // Calculate fish weights
            CalculateFishWeights();

            // Spawn marker
            if (alwaysShowMarker && spotMarkerPrefab != null)
            {
                markerInstance = Instantiate(spotMarkerPrefab, transform.position, Quaternion.identity, transform);
            }

            // Play ambient sound
            if (playAmbientSound && AudioManager.Instance != null && !string.IsNullOrEmpty(ambientSoundName))
            {
                AudioManager.Instance.PlayLoopingSoundAtPosition(ambientSoundName, transform.position);
            }
        }

        private void Update()
        {
            // Update time and weather from game manager
            // TODO: Connect to actual time/weather system
            UpdateTimeAndWeather();
        }

        /// <summary>
        /// Calculate weighted fish spawn chances
        /// </summary>
        private void CalculateFishWeights()
        {
            fishWeights.Clear();

            // Get all fish (local + global)
            List<Fish> allFish = new List<Fish>(availableFish);

            if (useGlobalPool && FishingManager.Instance != null)
            {
                allFish.AddRange(FishingManager.Instance.globalFishPool);
            }

            // Calculate weights for each fish
            foreach (Fish fish in allFish)
            {
                if (fish == null)
                    continue;

                // Check depth requirement
                if (fish.minDepth > 0 && waterDepth < fish.minDepth)
                    continue;

                // Check location tags
                if (fish.locationTags != null && fish.locationTags.Length > 0)
                {
                    bool hasMatchingTag = false;
                    foreach (string tag in fish.locationTags)
                    {
                        if (tag.Equals(spotType, System.StringComparison.OrdinalIgnoreCase))
                        {
                            hasMatchingTag = true;
                            break;
                        }
                    }

                    if (!hasMatchingTag)
                        continue;
                }

                // Get modified weight based on conditions
                float weight = fish.GetModifiedSpawnWeight(currentTime, currentWeather);
                weight *= spawnRateMultiplier;

                if (weight > 0f)
                {
                    fishWeights[fish] = weight;
                }
            }

            Debug.Log($"[FishingSpot] {spotName} has {fishWeights.Count} available fish");
        }

        /// <summary>
        /// Get a random fish from this spot's pool
        /// </summary>
        public Fish GetRandomFish()
        {
            if (fishWeights.Count == 0)
            {
                Debug.LogWarning($"[FishingSpot] {spotName} has no available fish!");
                return null;
            }

            // Calculate total weight
            float totalWeight = 0f;
            foreach (float weight in fishWeights.Values)
            {
                totalWeight += weight;
            }

            // Roll for fish
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var kvp in fishWeights)
            {
                cumulative += kvp.Value;

                if (roll <= cumulative)
                {
                    return kvp.Key;
                }
            }

            // Fallback: return random fish
            List<Fish> fishList = new List<Fish>(fishWeights.Keys);
            return fishList[Random.Range(0, fishList.Count)];
        }

        /// <summary>
        /// Update time and weather conditions
        /// </summary>
        private void UpdateTimeAndWeather()
        {
            // TODO: Get actual time/weather from game systems
            // For now, use placeholder values

            // Example time detection
            float hour = Time.time % 24f;
            if (hour >= 6f && hour < 18f)
            {
                currentTime = FishTimePreference.Day;
            }
            else if ((hour >= 5f && hour < 7f) || (hour >= 17f && hour < 19f))
            {
                currentTime = FishTimePreference.DawnDusk;
            }
            else
            {
                currentTime = FishTimePreference.Night;
            }

            // Example weather detection
            // currentWeather = WeatherSystem.Instance?.GetCurrentWeather() ?? FishWeatherPreference.Clear;
        }

        /// <summary>
        /// Check if position is within this fishing spot
        /// </summary>
        public bool IsPositionInSpot(Vector3 position)
        {
            float distance = Vector3.Distance(transform.position, position);
            return distance <= spotRadius;
        }

        /// <summary>
        /// Get available fish list
        /// </summary>
        public List<Fish> GetAvailableFish()
        {
            return new List<Fish>(fishWeights.Keys);
        }

        /// <summary>
        /// Refresh fish weights (call when conditions change)
        /// </summary>
        public void RefreshFishPool()
        {
            CalculateFishWeights();
        }

        private void OnDrawGizmos()
        {
            // Draw spot radius
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawSphere(transform.position, spotRadius);

            // Draw wire sphere
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spotRadius);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw spot info
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, spotRadius);

            #if UNITY_EDITOR
            // Draw label
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, spotName);
            #endif
        }
    }

    /// <summary>
    /// Fishing manager singleton.
    /// Manages global fish pool and fishing statistics.
    /// </summary>
    public class FishingManager : MonoBehaviour
    {
        public static FishingManager Instance { get; private set; }

        [Header("Global Fish Pool")]
        [Tooltip("Fish available globally")]
        public Fish[] globalFishPool;

        [Header("Statistics")]
        [Tooltip("Total fish caught")]
        public int totalFishCaught = 0;

        [Tooltip("Fish caught by species")]
        public Dictionary<string, int> fishCaughtBySpecies = new Dictionary<string, int>();

        [Header("Settings")]
        [Tooltip("Enable fishing")]
        public bool fishingEnabled = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Record a caught fish
        /// </summary>
        public void RecordCatch(Fish fish)
        {
            if (fish == null)
                return;

            totalFishCaught++;

            if (fishCaughtBySpecies.ContainsKey(fish.fishID))
            {
                fishCaughtBySpecies[fish.fishID]++;
            }
            else
            {
                fishCaughtBySpecies[fish.fishID] = 1;
            }

            Debug.Log($"[FishingManager] Caught {fish.fishName}! Total: {totalFishCaught}");
        }

        /// <summary>
        /// Get number of times a fish species has been caught
        /// </summary>
        public int GetCatchCount(string fishID)
        {
            if (fishCaughtBySpecies.ContainsKey(fishID))
            {
                return fishCaughtBySpecies[fishID];
            }

            return 0;
        }

        /// <summary>
        /// Get total fish caught
        /// </summary>
        public int GetTotalFishCaught()
        {
            return totalFishCaught;
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics()
        {
            totalFishCaught = 0;
            fishCaughtBySpecies.Clear();
        }
    }
}
