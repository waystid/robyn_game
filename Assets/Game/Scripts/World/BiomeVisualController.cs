using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.World
{
    /// <summary>
    /// Biome visual effects controller.
    /// Manages particles, lighting, and visual atmosphere per biome.
    /// </summary>
    public class BiomeVisualController : MonoBehaviour
    {
        public static BiomeVisualController Instance { get; private set; }

        [Header("Particle Effects")]
        [Tooltip("Rain particle system")]
        public ParticleSystem rainParticles;

        [Tooltip("Snow particle system")]
        public ParticleSystem snowParticles;

        [Tooltip("Fog particle system")]
        public ParticleSystem fogParticles;

        [Tooltip("Fireflies particle system")]
        public ParticleSystem firefliesParticles;

        [Tooltip("Dust particle system")]
        public ParticleSystem dustParticles;

        [Tooltip("Leaves particle system")]
        public ParticleSystem leavesParticles;

        [Header("Lighting")]
        [Tooltip("Directional light (sun)")]
        public Light directionalLight;

        [Tooltip("Biome-specific light intensity multiplier")]
        [Range(0f, 2f)]
        public float lightIntensityMultiplier = 1f;

        [Header("Post Processing")]
        [Tooltip("Enable color grading per biome")]
        public bool enableColorGrading = true;

        [Tooltip("Color grading strength")]
        [Range(0f, 1f)]
        public float colorGradingStrength = 0.5f;

        [Header("Ground Effects")]
        [Tooltip("Ground plane for material swapping")]
        public Renderer groundRenderer;

        [Tooltip("Fade duration for ground material")]
        public float materialFadeDuration = 1f;

        // State
        private BiomeData currentBiome;
        private Dictionary<string, ParticleSystem> activeParticleSystems = new Dictionary<string, ParticleSystem>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to biome events
            if (BiomeSystem.Instance != null)
            {
                BiomeSystem.Instance.OnBiomeEntered.AddListener(OnBiomeEntered);
                BiomeSystem.Instance.OnBiomeTransitionStarted.AddListener(OnBiomeTransitionStarted);
            }

            // Initialize all particle systems as inactive
            StopAllParticles();
        }

        /// <summary>
        /// Biome entered callback
        /// </summary>
        private void OnBiomeEntered(BiomeData biome)
        {
            currentBiome = biome;
            ApplyBiomeVisuals(biome);
        }

        /// <summary>
        /// Biome transition started callback
        /// </summary>
        private void OnBiomeTransitionStarted(BiomeData from, BiomeData to)
        {
            // Gradually transition visuals
            StartCoroutine(TransitionVisualsCoroutine(from, to));
        }

        /// <summary>
        /// Apply biome visuals
        /// </summary>
        private void ApplyBiomeVisuals(BiomeData biome)
        {
            if (biome == null)
                return;

            // Stop all current particles
            StopAllParticles();

            // Apply biome-specific effects
            switch (biome.biomeType)
            {
                case BiomeType.Plains:
                    ApplyPlainsEffects();
                    break;
                case BiomeType.Forest:
                    ApplyForestEffects();
                    break;
                case BiomeType.Desert:
                    ApplyDesertEffects();
                    break;
                case BiomeType.Tundra:
                    ApplyTundraEffects();
                    break;
                case BiomeType.Swamp:
                    ApplySwampEffects();
                    break;
                case BiomeType.Mountains:
                    ApplyMountainsEffects();
                    break;
                case BiomeType.Beach:
                    ApplyBeachEffects();
                    break;
                case BiomeType.Cave:
                    ApplyCaveEffects();
                    break;
                case BiomeType.Volcano:
                    ApplyVolcanoEffects();
                    break;
            }

            // Apply ground material
            if (groundRenderer != null && biome.groundMaterial != null)
            {
                groundRenderer.material = biome.groundMaterial;
            }

            // Apply lighting
            ApplyLighting(biome);
        }

        /// <summary>
        /// Apply plains effects
        /// </summary>
        private void ApplyPlainsEffects()
        {
            // Gentle breeze with occasional leaves
            if (leavesParticles != null)
            {
                leavesParticles.Play();
                var emission = leavesParticles.emission;
                emission.rateOverTime = 2f;
            }

            lightIntensityMultiplier = 1f;
        }

        /// <summary>
        /// Apply forest effects
        /// </summary>
        private void ApplyForestEffects()
        {
            // Fireflies and fog
            if (firefliesParticles != null)
            {
                firefliesParticles.Play();
            }

            if (fogParticles != null)
            {
                fogParticles.Play();
                var emission = fogParticles.emission;
                emission.rateOverTime = 10f;
            }

            lightIntensityMultiplier = 0.8f; // Dimmer under trees
        }

        /// <summary>
        /// Apply desert effects
        /// </summary>
        private void ApplyDesertEffects()
        {
            // Dust and heat waves
            if (dustParticles != null)
            {
                dustParticles.Play();
                var emission = dustParticles.emission;
                emission.rateOverTime = 15f;
            }

            lightIntensityMultiplier = 1.2f; // Bright sun
        }

        /// <summary>
        /// Apply tundra effects
        /// </summary>
        private void ApplyTundraEffects()
        {
            // Snow and wind
            if (snowParticles != null)
            {
                snowParticles.Play();
                var emission = snowParticles.emission;
                emission.rateOverTime = 20f;
            }

            lightIntensityMultiplier = 0.9f; // Overcast
        }

        /// <summary>
        /// Apply swamp effects
        /// </summary>
        private void ApplySwampEffects()
        {
            // Heavy fog and fireflies
            if (fogParticles != null)
            {
                fogParticles.Play();
                var emission = fogParticles.emission;
                emission.rateOverTime = 20f;
            }

            if (firefliesParticles != null)
            {
                firefliesParticles.Play();
            }

            lightIntensityMultiplier = 0.7f; // Dark and gloomy
        }

        /// <summary>
        /// Apply mountains effects
        /// </summary>
        private void ApplyMountainsEffects()
        {
            // Light snow at peaks
            if (snowParticles != null)
            {
                snowParticles.Play();
                var emission = snowParticles.emission;
                emission.rateOverTime = 5f;
            }

            lightIntensityMultiplier = 1.1f; // Clear mountain air
        }

        /// <summary>
        /// Apply beach effects
        /// </summary>
        private void ApplyBeachEffects()
        {
            // Seagulls and ocean mist (using fog particles)
            if (fogParticles != null)
            {
                fogParticles.Play();
                var emission = fogParticles.emission;
                emission.rateOverTime = 3f;
            }

            lightIntensityMultiplier = 1.1f; // Sunny beach
        }

        /// <summary>
        /// Apply cave effects
        /// </summary>
        private void ApplyCaveEffects()
        {
            // Very dark with dust
            if (dustParticles != null)
            {
                dustParticles.Play();
                var emission = dustParticles.emission;
                emission.rateOverTime = 5f;
            }

            lightIntensityMultiplier = 0.3f; // Very dark
        }

        /// <summary>
        /// Apply volcano effects
        /// </summary>
        private void ApplyVolcanoEffects()
        {
            // Heavy ash/dust
            if (dustParticles != null)
            {
                dustParticles.Play();
                var emission = dustParticles.emission;
                emission.rateOverTime = 30f;

                var main = dustParticles.main;
                main.startColor = new Color(0.3f, 0.3f, 0.3f); // Dark ash
            }

            lightIntensityMultiplier = 0.8f; // Dim from ash
        }

        /// <summary>
        /// Apply lighting
        /// </summary>
        private void ApplyLighting(BiomeData biome)
        {
            if (directionalLight == null)
                return;

            // Base intensity modified by time of day
            float baseIntensity = 1f;
            if (TimeWeather.TimeSystem.Instance != null)
            {
                float timeOfDay = TimeWeather.TimeSystem.Instance.GetTimeOfDay();
                baseIntensity = TimeWeather.TimeSystem.Instance.GetSunIntensity();
            }

            directionalLight.intensity = baseIntensity * lightIntensityMultiplier;
            directionalLight.color = biome.ambientLight;
        }

        /// <summary>
        /// Stop all particle systems
        /// </summary>
        private void StopAllParticles()
        {
            if (rainParticles != null) rainParticles.Stop();
            if (snowParticles != null) snowParticles.Stop();
            if (fogParticles != null) fogParticles.Stop();
            if (firefliesParticles != null) firefliesParticles.Stop();
            if (dustParticles != null) dustParticles.Stop();
            if (leavesParticles != null) leavesParticles.Stop();
        }

        /// <summary>
        /// Transition visuals coroutine
        /// </summary>
        private System.Collections.IEnumerator TransitionVisualsCoroutine(BiomeData from, BiomeData to)
        {
            float duration = BiomeSystem.Instance != null ? BiomeSystem.Instance.transitionDuration : 2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Lerp lighting
                if (directionalLight != null)
                {
                    float fromIntensity = 1f * GetBiomeLightMultiplier(from);
                    float toIntensity = 1f * GetBiomeLightMultiplier(to);
                    directionalLight.intensity = Mathf.Lerp(fromIntensity, toIntensity, progress);

                    directionalLight.color = Color.Lerp(from.ambientLight, to.ambientLight, progress);
                }

                yield return null;
            }

            // Apply final visuals
            ApplyBiomeVisuals(to);
        }

        /// <summary>
        /// Get biome light multiplier
        /// </summary>
        private float GetBiomeLightMultiplier(BiomeData biome)
        {
            switch (biome.biomeType)
            {
                case BiomeType.Cave: return 0.3f;
                case BiomeType.Swamp: return 0.7f;
                case BiomeType.Forest: return 0.8f;
                case BiomeType.Volcano: return 0.8f;
                case BiomeType.Tundra: return 0.9f;
                case BiomeType.Plains: return 1f;
                case BiomeType.Mountains: return 1.1f;
                case BiomeType.Beach: return 1.1f;
                case BiomeType.Desert: return 1.2f;
                default: return 1f;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (BiomeSystem.Instance != null)
            {
                BiomeSystem.Instance.OnBiomeEntered.RemoveListener(OnBiomeEntered);
                BiomeSystem.Instance.OnBiomeTransitionStarted.RemoveListener(OnBiomeTransitionStarted);
            }
        }
    }

    /// <summary>
    /// Biome spawner.
    /// Spawns resources and creatures in biomes.
    /// </summary>
    public class BiomeSpawner : MonoBehaviour
    {
        public static BiomeSpawner Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Auto-spawn resources on biome enter")]
        public bool autoSpawnResources = true;

        [Tooltip("Auto-spawn creatures on biome enter")]
        public bool autoSpawnCreatures = true;

        [Tooltip("Resource spawn interval (seconds)")]
        public float resourceSpawnInterval = 60f;

        [Tooltip("Creature spawn interval (seconds)")]
        public float creatureSpawnInterval = 30f;

        [Header("Limits")]
        [Tooltip("Max resources per biome")]
        public int maxResourcesPerBiome = 50;

        [Tooltip("Max creatures per biome")]
        public int maxCreaturesPerBiome = 20;

        // State
        private float resourceSpawnTimer = 0f;
        private float creatureSpawnTimer = 0f;
        private Dictionary<string, List<GameObject>> spawnedResources = new Dictionary<string, List<GameObject>>();
        private Dictionary<string, List<GameObject>> spawnedCreatures = new Dictionary<string, List<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to biome events
            if (BiomeSystem.Instance != null)
            {
                BiomeSystem.Instance.OnBiomeEntered.AddListener(OnBiomeEntered);
            }
        }

        private void Update()
        {
            // Spawn timers
            if (autoSpawnResources)
            {
                resourceSpawnTimer += Time.deltaTime;
                if (resourceSpawnTimer >= resourceSpawnInterval)
                {
                    resourceSpawnTimer = 0f;
                    SpawnResourcesInCurrentBiome();
                }
            }

            if (autoSpawnCreatures)
            {
                creatureSpawnTimer += Time.deltaTime;
                if (creatureSpawnTimer >= creatureSpawnInterval)
                {
                    creatureSpawnTimer = 0f;
                    SpawnCreaturesInCurrentBiome();
                }
            }
        }

        /// <summary>
        /// Biome entered callback
        /// </summary>
        private void OnBiomeEntered(BiomeData biome)
        {
            if (autoSpawnResources)
            {
                SpawnResourcesInBiome(biome);
            }

            if (autoSpawnCreatures)
            {
                SpawnCreaturesInBiome(biome);
            }
        }

        /// <summary>
        /// Spawn resources in current biome
        /// </summary>
        private void SpawnResourcesInCurrentBiome()
        {
            if (BiomeSystem.Instance == null || BiomeSystem.Instance.currentBiome == null)
                return;

            SpawnResourcesInBiome(BiomeSystem.Instance.currentBiome);
        }

        /// <summary>
        /// Spawn resources in biome
        /// </summary>
        public void SpawnResourcesInBiome(BiomeData biome)
        {
            if (biome == null)
                return;

            // Check current resource count
            if (!spawnedResources.ContainsKey(biome.biomeID))
            {
                spawnedResources[biome.biomeID] = new List<GameObject>();
            }

            // Remove destroyed resources
            spawnedResources[biome.biomeID].RemoveAll(r => r == null);

            if (spawnedResources[biome.biomeID].Count >= maxResourcesPerBiome)
                return;

            // Spawn resources
            foreach (var resourceSpawn in biome.resourceSpawns)
            {
                if (Random.value <= resourceSpawn.spawnChance)
                {
                    int count = Random.Range(resourceSpawn.minCount, resourceSpawn.maxCount + 1);

                    for (int i = 0; i < count; i++)
                    {
                        if (spawnedResources[biome.biomeID].Count >= maxResourcesPerBiome)
                            break;

                        Vector3 spawnPos = BiomeSystem.Instance.GetRandomPositionInBiome(
                            biome,
                            resourceSpawn.minDistance,
                            resourceSpawn.maxDistance
                        );

                        GameObject resource = Instantiate(resourceSpawn.resourcePrefab, spawnPos, Quaternion.identity);
                        spawnedResources[biome.biomeID].Add(resource);
                    }
                }
            }
        }

        /// <summary>
        /// Spawn creatures in current biome
        /// </summary>
        private void SpawnCreaturesInCurrentBiome()
        {
            if (BiomeSystem.Instance == null || BiomeSystem.Instance.currentBiome == null)
                return;

            SpawnCreaturesInBiome(BiomeSystem.Instance.currentBiome);
        }

        /// <summary>
        /// Spawn creatures in biome
        /// </summary>
        public void SpawnCreaturesInBiome(BiomeData biome)
        {
            if (biome == null)
                return;

            // Check current creature count
            if (!spawnedCreatures.ContainsKey(biome.biomeID))
            {
                spawnedCreatures[biome.biomeID] = new List<GameObject>();
            }

            // Remove destroyed creatures
            spawnedCreatures[biome.biomeID].RemoveAll(c => c == null);

            if (spawnedCreatures[biome.biomeID].Count >= maxCreaturesPerBiome)
                return;

            // Check time/weather conditions
            bool isNight = false;
            if (TimeWeather.TimeSystem.Instance != null)
            {
                float time = TimeWeather.TimeSystem.Instance.GetTimeOfDay();
                isNight = time < 0.25f || time > 0.75f; // Night is 6pm-6am
            }

            // Spawn creatures
            foreach (var creatureSpawn in biome.creatureSpawns)
            {
                // Check spawn conditions
                if (creatureSpawn.spawnOnlyAtNight && !isNight)
                    continue;

                if (Random.value <= creatureSpawn.spawnChance)
                {
                    if (spawnedCreatures[biome.biomeID].Count >= creatureSpawn.maxPopulation)
                        continue;

                    Vector3 spawnPos = BiomeSystem.Instance.GetRandomPositionInBiome(biome, 10f, 50f);

                    GameObject creature = Instantiate(creatureSpawn.creaturePrefab, spawnPos, Quaternion.identity);
                    spawnedCreatures[biome.biomeID].Add(creature);
                }
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (BiomeSystem.Instance != null)
            {
                BiomeSystem.Instance.OnBiomeEntered.RemoveListener(OnBiomeEntered);
            }
        }
    }
}
