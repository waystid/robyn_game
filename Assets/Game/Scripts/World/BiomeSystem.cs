using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.World
{
    /// <summary>
    /// Biome type
    /// </summary>
    public enum BiomeType
    {
        Plains,         // Grasslands
        Forest,         // Woodland areas
        Desert,         // Sandy dunes
        Tundra,         // Snowy wasteland
        Swamp,          // Wetlands
        Mountains,      // Rocky peaks
        Beach,          // Coastal areas
        Lake,           // Water bodies
        Cave,           // Underground
        Volcano,        // Lava areas
        Custom          // Custom biome
    }

    /// <summary>
    /// Biome rarity
    /// </summary>
    public enum BiomeRarity
    {
        Common,         // Found everywhere
        Uncommon,       // Less common
        Rare,           // Hard to find
        VeryRare,       // Very rare
        Unique          // One-of-a-kind
    }

    /// <summary>
    /// Biome resource spawn
    /// </summary>
    [System.Serializable]
    public class BiomeResourceSpawn
    {
        public GameObject resourcePrefab;
        public float spawnChance = 0.5f;
        public int minCount = 1;
        public int maxCount = 5;
        public float minDistance = 5f;
        public float maxDistance = 50f;
    }

    /// <summary>
    /// Biome creature spawn
    /// </summary>
    [System.Serializable]
    public class BiomeCreatureSpawn
    {
        public GameObject creaturePrefab;
        public float spawnChance = 0.3f;
        public int maxPopulation = 10;
        public bool spawnOnlyAtNight = false;
        public bool spawnOnlyInWeather = false;
        public TimeWeather.WeatherType requiredWeather;
    }

    /// <summary>
    /// Biome data
    /// </summary>
    [System.Serializable]
    public class BiomeData
    {
        [Header("Basic Info")]
        public string biomeID = "biome_001";
        public string biomeName = "Plains";
        public BiomeType biomeType = BiomeType.Plains;
        public BiomeRarity rarity = BiomeRarity.Common;

        [TextArea(2, 4)]
        public string description = "A peaceful grassland...";

        [Header("Visual")]
        public Color skyColor = new Color(0.5f, 0.7f, 1f);
        public Color fogColor = new Color(0.8f, 0.8f, 0.9f);
        public float fogDensity = 0.01f;
        public Material groundMaterial;
        public Color ambientLight = Color.white;

        [Header("Weather")]
        public List<TimeWeather.WeatherType> possibleWeather = new List<TimeWeather.WeatherType>();
        public float weatherChangeChance = 0.1f;
        public bool allowRain = true;
        public bool allowSnow = false;
        public bool allowStorm = false;

        [Header("Temperature")]
        [Range(-50f, 50f)]
        public float baseTemperature = 20f;

        [Range(0f, 100f)]
        public float baseHumidity = 50f;

        [Header("Audio")]
        public AudioClip ambienceSound;
        public AudioClip musicTrack;
        public float ambienceVolume = 0.5f;

        [Header("Resources")]
        public List<BiomeResourceSpawn> resourceSpawns = new List<BiomeResourceSpawn>();

        [Header("Creatures")]
        public List<BiomeCreatureSpawn> creatureSpawns = new List<BiomeCreatureSpawn>();

        [Header("Plants")]
        public List<GameObject> plantPrefabs = new List<GameObject>();
        public float plantDensity = 0.3f;

        [Header("Boundaries")]
        public Vector3 centerPosition;
        public float radius = 100f;
        public bool useBoxBounds = false;
        public Vector3 boxSize = new Vector3(100f, 50f, 100f);

        [Header("Difficulty")]
        [Range(1, 10)]
        public int difficultyLevel = 1;
        public int recommendedLevel = 1;

        [Header("Discovery")]
        public bool isDiscovered = false;
        public bool startDiscovered = false;
        public string discoveryMessage = "Discovered: {biomeName}!";

        /// <summary>
        /// Check if position is inside biome
        /// </summary>
        public bool ContainsPosition(Vector3 position)
        {
            if (useBoxBounds)
            {
                Vector3 offset = position - centerPosition;
                return Mathf.Abs(offset.x) <= boxSize.x / 2f &&
                       Mathf.Abs(offset.y) <= boxSize.y / 2f &&
                       Mathf.Abs(offset.z) <= boxSize.z / 2f;
            }
            else
            {
                float distance = Vector3.Distance(position, centerPosition);
                return distance <= radius;
            }
        }

        /// <summary>
        /// Get distance to biome center
        /// </summary>
        public float GetDistanceToCenter(Vector3 position)
        {
            return Vector3.Distance(position, centerPosition);
        }

        /// <summary>
        /// Get temperature at time of day
        /// </summary>
        public float GetTemperatureAtTime(float timeOfDay)
        {
            // Temperature varies throughout day
            // Warmest at 14:00 (0.58), coldest at 4:00 (0.17)
            float dayFactor = Mathf.Sin((timeOfDay - 0.25f) * Mathf.PI * 2f);
            float variation = dayFactor * 10f; // ±10°C variation

            return baseTemperature + variation;
        }
    }

    /// <summary>
    /// Biome transition data
    /// </summary>
    [System.Serializable]
    public class BiomeTransition
    {
        public BiomeData fromBiome;
        public BiomeData toBiome;
        public float transitionProgress = 0f;
        public float transitionDuration = 2f;
        public bool isTransitioning = false;
    }

    /// <summary>
    /// Biome save data
    /// </summary>
    [System.Serializable]
    public class BiomeSaveData
    {
        public List<string> discoveredBiomeIDs = new List<string>();
        public string currentBiomeID = "";
    }

    /// <summary>
    /// Biome system singleton.
    /// Manages biomes, transitions, and environmental effects.
    /// </summary>
    public class BiomeSystem : MonoBehaviour
    {
        public static BiomeSystem Instance { get; private set; }

        [Header("Biomes")]
        [Tooltip("All biomes in the world")]
        public List<BiomeData> biomes = new List<BiomeData>();

        [Header("Current State")]
        [Tooltip("Current biome")]
        public BiomeData currentBiome;

        [Tooltip("Previous biome")]
        public BiomeData previousBiome;

        [Header("Player")]
        [Tooltip("Player transform")]
        public Transform playerTransform;

        [Tooltip("Check interval (seconds)")]
        public float checkInterval = 1f;

        [Header("Transition")]
        [Tooltip("Enable smooth transitions")]
        public bool enableTransitions = true;

        [Tooltip("Transition duration")]
        public float transitionDuration = 2f;

        [Tooltip("Current transition")]
        public BiomeTransition currentTransition;

        [Header("Visual Effects")]
        [Tooltip("Update sky color")]
        public bool updateSkyColor = true;

        [Tooltip("Update fog")]
        public bool updateFog = true;

        [Tooltip("Update ambient light")]
        public bool updateAmbientLight = true;

        [Header("Audio")]
        [Tooltip("Update ambience")]
        public bool updateAmbience = true;

        [Tooltip("Update music")]
        public bool updateMusic = true;

        [Tooltip("Fade duration for audio")]
        public float audioFadeDuration = 1f;

        [Header("Events")]
        public UnityEvent<BiomeData> OnBiomeEntered;
        public UnityEvent<BiomeData> OnBiomeExited;
        public UnityEvent<BiomeData> OnBiomeDiscovered;
        public UnityEvent<BiomeData, BiomeData> OnBiomeTransitionStarted;
        public UnityEvent<BiomeData> OnBiomeTransitionCompleted;

        // State
        private float checkTimer = 0f;
        private bool isTransitioning = false;

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
        /// Initialize biome system
        /// </summary>
        private void Initialize()
        {
            // Find player if not set
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            // Mark starting biomes as discovered
            foreach (var biome in biomes)
            {
                if (biome.startDiscovered)
                {
                    biome.isDiscovered = true;
                }
            }

            // Set initial biome
            if (playerTransform != null)
            {
                BiomeData startBiome = GetBiomeAtPosition(playerTransform.position);
                if (startBiome != null)
                {
                    SetCurrentBiome(startBiome, false);
                }
            }

            Debug.Log($"[BiomeSystem] Initialized with {biomes.Count} biomes");
        }

        private void Update()
        {
            if (playerTransform == null)
                return;

            // Check for biome changes
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                CheckBiomeChange();
            }

            // Update transition
            if (isTransitioning && currentTransition != null)
            {
                UpdateTransition();
            }
        }

        /// <summary>
        /// Check for biome change
        /// </summary>
        private void CheckBiomeChange()
        {
            BiomeData newBiome = GetBiomeAtPosition(playerTransform.position);

            if (newBiome != currentBiome && newBiome != null)
            {
                ChangeBiome(newBiome);
            }
        }

        /// <summary>
        /// Change to new biome
        /// </summary>
        private void ChangeBiome(BiomeData newBiome)
        {
            previousBiome = currentBiome;

            if (enableTransitions && previousBiome != null)
            {
                StartBiomeTransition(previousBiome, newBiome);
            }
            else
            {
                SetCurrentBiome(newBiome, true);
            }
        }

        /// <summary>
        /// Start biome transition
        /// </summary>
        private void StartBiomeTransition(BiomeData from, BiomeData to)
        {
            currentTransition = new BiomeTransition
            {
                fromBiome = from,
                toBiome = to,
                transitionProgress = 0f,
                transitionDuration = transitionDuration,
                isTransitioning = true
            };

            isTransitioning = true;

            OnBiomeTransitionStarted?.Invoke(from, to);

            Debug.Log($"[BiomeSystem] Transitioning from {from.biomeName} to {to.biomeName}");
        }

        /// <summary>
        /// Update transition
        /// </summary>
        private void UpdateTransition()
        {
            if (currentTransition == null)
                return;

            currentTransition.transitionProgress += Time.deltaTime / currentTransition.transitionDuration;

            if (currentTransition.transitionProgress >= 1f)
            {
                // Transition complete
                CompleteBiomeTransition();
            }
            else
            {
                // Apply transition effects
                ApplyTransitionEffects(currentTransition.transitionProgress);
            }
        }

        /// <summary>
        /// Complete biome transition
        /// </summary>
        private void CompleteBiomeTransition()
        {
            if (currentTransition == null)
                return;

            SetCurrentBiome(currentTransition.toBiome, true);

            OnBiomeTransitionCompleted?.Invoke(currentTransition.toBiome);

            currentTransition = null;
            isTransitioning = false;

            Debug.Log($"[BiomeSystem] Transition completed");
        }

        /// <summary>
        /// Apply transition effects
        /// </summary>
        private void ApplyTransitionEffects(float progress)
        {
            if (currentTransition == null)
                return;

            BiomeData from = currentTransition.fromBiome;
            BiomeData to = currentTransition.toBiome;

            // Lerp visual effects
            if (updateSkyColor)
            {
                Color skyColor = Color.Lerp(from.skyColor, to.skyColor, progress);
                RenderSettings.skybox.SetColor("_Tint", skyColor);
            }

            if (updateFog)
            {
                RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, progress);
                RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, progress);
            }

            if (updateAmbientLight)
            {
                RenderSettings.ambientLight = Color.Lerp(from.ambientLight, to.ambientLight, progress);
            }
        }

        /// <summary>
        /// Set current biome
        /// </summary>
        private void SetCurrentBiome(BiomeData biome, bool triggerEvents)
        {
            if (biome == null)
                return;

            // Exit previous biome
            if (triggerEvents && currentBiome != null)
            {
                OnBiomeExited?.Invoke(currentBiome);
            }

            currentBiome = biome;

            // Apply biome effects
            ApplyBiomeEffects(biome);

            // Discover biome
            if (!biome.isDiscovered)
            {
                DiscoverBiome(biome);
            }

            // Enter new biome
            if (triggerEvents)
            {
                OnBiomeEntered?.Invoke(biome);

                // Show notification
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    string message = biome.discoveryMessage.Replace("{biomeName}", biome.biomeName);
                    FloatingTextManager.Instance.Show(
                        message,
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.green
                    );
                }
            }

            Debug.Log($"[BiomeSystem] Entered biome: {biome.biomeName}");
        }

        /// <summary>
        /// Apply biome effects
        /// </summary>
        private void ApplyBiomeEffects(BiomeData biome)
        {
            // Visual effects
            if (updateSkyColor && RenderSettings.skybox != null)
            {
                RenderSettings.skybox.SetColor("_Tint", biome.skyColor);
            }

            if (updateFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogColor = biome.fogColor;
                RenderSettings.fogDensity = biome.fogDensity;
            }

            if (updateAmbientLight)
            {
                RenderSettings.ambientLight = biome.ambientLight;
            }

            // Audio effects
            if (updateAmbience && biome.ambienceSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAmbience(biome.ambienceSound, biome.ambienceVolume);
            }

            if (updateMusic && biome.musicTrack != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(biome.musicTrack);
            }

            // Update weather system
            if (TimeWeather.WeatherSystem.Instance != null)
            {
                TimeWeather.WeatherSystem.Instance.SetBiomeWeatherRestrictions(
                    biome.allowRain,
                    biome.allowSnow,
                    biome.allowStorm
                );
            }

            // Update map marker
            if (Map.MapSystem.Instance != null)
            {
                Map.MapSystem.Instance.UpdatePlayerBiome(biome.biomeName);
            }
        }

        /// <summary>
        /// Discover biome
        /// </summary>
        private void DiscoverBiome(BiomeData biome)
        {
            if (biome.isDiscovered)
                return;

            biome.isDiscovered = true;

            // Add to journal
            if (Journal.JournalSystem.Instance != null)
            {
                Journal.JournalSystem.Instance.DiscoverLocation(biome.biomeID);
            }

            OnBiomeDiscovered?.Invoke(biome);

            // Play discovery sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("location_discovered");
            }

            Debug.Log($"[BiomeSystem] Discovered biome: {biome.biomeName}");
        }

        /// <summary>
        /// Get biome at position
        /// </summary>
        public BiomeData GetBiomeAtPosition(Vector3 position)
        {
            // Check all biomes
            foreach (var biome in biomes)
            {
                if (biome.ContainsPosition(position))
                {
                    return biome;
                }
            }

            return null;
        }

        /// <summary>
        /// Get biome by ID
        /// </summary>
        public BiomeData GetBiome(string biomeID)
        {
            return biomes.Find(b => b.biomeID == biomeID);
        }

        /// <summary>
        /// Get discovered biomes
        /// </summary>
        public List<BiomeData> GetDiscoveredBiomes()
        {
            return biomes.FindAll(b => b.isDiscovered);
        }

        /// <summary>
        /// Get current temperature
        /// </summary>
        public float GetCurrentTemperature()
        {
            if (currentBiome == null || TimeWeather.TimeSystem.Instance == null)
                return 20f;

            float timeOfDay = TimeWeather.TimeSystem.Instance.GetTimeOfDay();
            return currentBiome.GetTemperatureAtTime(timeOfDay);
        }

        /// <summary>
        /// Get current humidity
        /// </summary>
        public float GetCurrentHumidity()
        {
            if (currentBiome == null)
                return 50f;

            return currentBiome.baseHumidity;
        }

        /// <summary>
        /// Spawn biome resources
        /// </summary>
        public void SpawnBiomeResources(BiomeData biome)
        {
            if (biome == null)
                return;

            foreach (var resourceSpawn in biome.resourceSpawns)
            {
                if (Random.value <= resourceSpawn.spawnChance)
                {
                    int count = Random.Range(resourceSpawn.minCount, resourceSpawn.maxCount + 1);

                    for (int i = 0; i < count; i++)
                    {
                        Vector3 spawnPos = GetRandomPositionInBiome(biome, resourceSpawn.minDistance, resourceSpawn.maxDistance);
                        Instantiate(resourceSpawn.resourcePrefab, spawnPos, Quaternion.identity);
                    }
                }
            }
        }

        /// <summary>
        /// Get random position in biome
        /// </summary>
        public Vector3 GetRandomPositionInBiome(BiomeData biome, float minDistance, float maxDistance)
        {
            Vector3 randomOffset = Random.insideUnitSphere * maxDistance;
            randomOffset.y = 0f; // Keep on ground level

            if (randomOffset.magnitude < minDistance)
            {
                randomOffset = randomOffset.normalized * minDistance;
            }

            return biome.centerPosition + randomOffset;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public BiomeSaveData GetSaveData()
        {
            BiomeSaveData data = new BiomeSaveData();

            foreach (var biome in biomes)
            {
                if (biome.isDiscovered)
                {
                    data.discoveredBiomeIDs.Add(biome.biomeID);
                }
            }

            if (currentBiome != null)
            {
                data.currentBiomeID = currentBiome.biomeID;
            }

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(BiomeSaveData data)
        {
            if (data == null)
                return;

            foreach (var biomeID in data.discoveredBiomeIDs)
            {
                BiomeData biome = GetBiome(biomeID);
                if (biome != null)
                {
                    biome.isDiscovered = true;
                }
            }

            Debug.Log($"[BiomeSystem] Loaded {data.discoveredBiomeIDs.Count} discovered biomes");
        }

        /// <summary>
        /// Get biome completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (biomes.Count == 0)
                return 1f;

            int discoveredCount = biomes.FindAll(b => b.isDiscovered).Count;
            return (float)discoveredCount / biomes.Count;
        }
    }
}
