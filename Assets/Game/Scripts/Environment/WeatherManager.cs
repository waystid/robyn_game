using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace CozyGame.Environment
{
    /// <summary>
    /// Weather state types
    /// </summary>
    public enum WeatherState
    {
        Clear,
        Cloudy,
        Rain,
        HeavyRain,
        Snow,
        Fog,
        Storm,
        Custom
    }

    /// <summary>
    /// Weather preset configuration
    /// </summary>
    [System.Serializable]
    public class WeatherPreset
    {
        [Header("Identification")]
        [Tooltip("Weather state type")]
        public WeatherState weatherState = WeatherState.Clear;

        [Tooltip("Weather display name")]
        public string weatherName = "Clear";

        [Tooltip("Weather description")]
        [TextArea(2, 3)]
        public string description = "Clear skies";

        [Header("Fog Settings")]
        [Tooltip("Fog density (0-1)")]
        [Range(0f, 1f)]
        public float fogDensity = 0f;

        [Tooltip("Fog color")]
        public Color fogColor = Color.gray;

        [Header("Lighting")]
        [Tooltip("Light intensity multiplier")]
        [Range(0f, 2f)]
        public float lightIntensityMultiplier = 1f;

        [Tooltip("Ambient light color multiplier")]
        public Color ambientColorMultiplier = Color.white;

        [Header("Particle Effects")]
        [Tooltip("Weather particle system prefab")]
        public GameObject particleEffectPrefab;

        [Tooltip("Particle emission rate")]
        public float particleEmissionRate = 100f;

        [Header("Audio")]
        [Tooltip("Weather ambient sound name")]
        public string ambientSoundName = "";

        [Tooltip("Ambient sound volume")]
        [Range(0f, 1f)]
        public float ambientSoundVolume = 0.5f;

        [Header("Wind")]
        [Tooltip("Wind strength")]
        [Range(0f, 10f)]
        public float windStrength = 0f;

        [Tooltip("Wind direction")]
        public Vector3 windDirection = Vector3.right;

        [Header("Probability")]
        [Tooltip("Chance to occur (0-100)")]
        [Range(0f, 100f)]
        public float spawnProbability = 10f;

        [Tooltip("Minimum duration in seconds")]
        public float minDuration = 60f;

        [Tooltip("Maximum duration in seconds")]
        public float maxDuration = 300f;

        [Header("Time Preferences")]
        [Tooltip("Can occur during day")]
        public bool canOccurDuringDay = true;

        [Tooltip("Can occur during night")]
        public bool canOccurDuringNight = true;

        [Tooltip("Time of day spawn weight bonus")]
        [Range(0f, 5f)]
        public float timeOfDayWeightBonus = 1f;
    }

    /// <summary>
    /// Weather save data
    /// </summary>
    [System.Serializable]
    public class WeatherSaveData
    {
        public string currentWeatherState;
        public float weatherDurationRemaining;
    }

    /// <summary>
    /// Weather manager singleton.
    /// Manages weather states, transitions, and effects.
    /// Integrates with TimeManager for time-based weather patterns.
    /// </summary>
    public class WeatherManager : MonoBehaviour
    {
        public static WeatherManager Instance { get; private set; }

        [Header("Weather Presets")]
        [Tooltip("Available weather presets")]
        public WeatherPreset[] weatherPresets;

        [Header("Transition Settings")]
        [Tooltip("Weather transition duration (seconds)")]
        public float transitionDuration = 5f;

        [Tooltip("Use smooth fog transitions")]
        public bool useSmoothFogTransitions = true;

        [Tooltip("Use smooth lighting transitions")]
        public bool useSmoothLightingTransitions = true;

        [Header("Auto Weather Changes")]
        [Tooltip("Enable automatic weather changes")]
        public bool enableAutoWeatherChanges = true;

        [Tooltip("Check for weather change every X seconds")]
        public float weatherCheckInterval = 120f; // 2 minutes

        [Tooltip("Chance to change weather per check (0-100)")]
        [Range(0f, 100f)]
        public float weatherChangeChance = 30f;

        [Header("References")]
        [Tooltip("Sun/moon lights (will be affected by weather)")]
        public Light[] affectedLights;

        [Tooltip("Weather particle spawn point (usually camera position)")]
        public Transform particleSpawnPoint;

        [Tooltip("Weather particle container")]
        public Transform particleContainer;

        [Header("Events")]
        public UnityEvent<WeatherState> OnWeatherChanged;
        public UnityEvent<WeatherState, WeatherState> OnWeatherTransitionStarted; // from, to
        public UnityEvent<WeatherState> OnWeatherTransitionCompleted;

        // State
        private WeatherPreset currentWeather;
        private WeatherPreset targetWeather;
        private WeatherState currentWeatherState = WeatherState.Clear;
        private bool isTransitioning = false;
        private float transitionProgress = 0f;

        // Weather duration
        private float weatherDuration = 0f;
        private float weatherTimer = 0f;

        // Effects
        private GameObject currentParticleEffect;
        private AudioSource weatherAudioSource;

        // Original lighting values (to restore after weather)
        private Dictionary<Light, float> originalLightIntensities = new Dictionary<Light, float>();
        private Color originalAmbientColor;
        private Color originalFogColor;
        private float originalFogDensity;

        // Weather check timer
        private float weatherCheckTimer = 0f;

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
        /// Initialize weather manager
        /// </summary>
        private void Initialize()
        {
            // Setup audio source for weather sounds
            weatherAudioSource = gameObject.AddComponent<AudioSource>();
            weatherAudioSource.loop = true;
            weatherAudioSource.playOnAwake = false;
            weatherAudioSource.spatialBlend = 0f; // 2D sound

            // Store original rendering settings
            originalAmbientColor = RenderSettings.ambientLight;
            originalFogColor = RenderSettings.fogColor;
            originalFogDensity = RenderSettings.fogDensity;

            // Store original light intensities
            if (affectedLights != null)
            {
                foreach (var light in affectedLights)
                {
                    if (light != null)
                    {
                        originalLightIntensities[light] = light.intensity;
                    }
                }
            }

            // Setup particle spawn point
            if (particleSpawnPoint == null && Camera.main != null)
            {
                particleSpawnPoint = Camera.main.transform;
            }

            // Create particle container
            if (particleContainer == null)
            {
                GameObject container = new GameObject("WeatherParticleContainer");
                particleContainer = container.transform;
                particleContainer.SetParent(transform);
            }

            // Create default weather presets if none assigned
            if (weatherPresets == null || weatherPresets.Length == 0)
            {
                CreateDefaultWeatherPresets();
            }

            // Start with clear weather
            WeatherPreset clearWeather = GetWeatherPreset(WeatherState.Clear);
            if (clearWeather != null)
            {
                SetWeatherImmediate(clearWeather);
            }

            Debug.Log("[WeatherManager] Initialized");
        }

        private void Update()
        {
            // Update transitions
            if (isTransitioning)
            {
                UpdateWeatherTransition();
            }

            // Update weather duration
            if (!isTransitioning && weatherDuration > 0f)
            {
                weatherTimer += Time.deltaTime;

                if (weatherTimer >= weatherDuration)
                {
                    // Weather duration expired, change to new weather
                    if (enableAutoWeatherChanges)
                    {
                        ChangeToRandomWeather();
                    }
                }
            }

            // Auto weather change check
            if (enableAutoWeatherChanges && !isTransitioning)
            {
                weatherCheckTimer += Time.deltaTime;

                if (weatherCheckTimer >= weatherCheckInterval)
                {
                    weatherCheckTimer = 0f;

                    // Roll for weather change
                    if (Random.Range(0f, 100f) <= weatherChangeChance)
                    {
                        ChangeToRandomWeather();
                    }
                }
            }

            // Update particle effect position (follow camera)
            if (currentParticleEffect != null && particleSpawnPoint != null)
            {
                currentParticleEffect.transform.position = particleSpawnPoint.position;
            }
        }

        /// <summary>
        /// Update weather transition
        /// </summary>
        private void UpdateWeatherTransition()
        {
            transitionProgress += Time.deltaTime / transitionDuration;

            if (transitionProgress >= 1f)
            {
                // Transition complete
                transitionProgress = 1f;
                isTransitioning = false;
                OnWeatherTransitionCompleted?.Invoke(currentWeatherState);

                Debug.Log($"[WeatherManager] Transition to {currentWeatherState} completed");
            }

            // Lerp fog
            if (useSmoothFogTransitions && currentWeather != null && targetWeather != null)
            {
                RenderSettings.fogDensity = Mathf.Lerp(currentWeather.fogDensity, targetWeather.fogDensity, transitionProgress);
                RenderSettings.fogColor = Color.Lerp(currentWeather.fogColor, targetWeather.fogColor, transitionProgress);
            }

            // Lerp lighting
            if (useSmoothLightingTransitions && currentWeather != null && targetWeather != null)
            {
                float fromIntensity = currentWeather.lightIntensityMultiplier;
                float toIntensity = targetWeather.lightIntensityMultiplier;
                float intensityMultiplier = Mathf.Lerp(fromIntensity, toIntensity, transitionProgress);

                foreach (var kvp in originalLightIntensities)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.intensity = kvp.Value * intensityMultiplier;
                    }
                }
            }

            // When transition completes, update to target weather
            if (transitionProgress >= 1f)
            {
                currentWeather = targetWeather;
            }
        }

        /// <summary>
        /// Set weather immediately (no transition)
        /// </summary>
        public void SetWeatherImmediate(WeatherState weatherState)
        {
            WeatherPreset preset = GetWeatherPreset(weatherState);
            if (preset != null)
            {
                SetWeatherImmediate(preset);
            }
        }

        /// <summary>
        /// Set weather immediately (no transition)
        /// </summary>
        private void SetWeatherImmediate(WeatherPreset preset)
        {
            if (preset == null)
                return;

            currentWeather = preset;
            targetWeather = preset;
            currentWeatherState = preset.weatherState;
            isTransitioning = false;
            transitionProgress = 1f;

            // Set duration
            weatherDuration = Random.Range(preset.minDuration, preset.maxDuration);
            weatherTimer = 0f;

            // Apply weather effects immediately
            ApplyWeatherEffects(preset);

            Debug.Log($"[WeatherManager] Weather set to {preset.weatherName}");
        }

        /// <summary>
        /// Change weather with transition
        /// </summary>
        public void ChangeWeather(WeatherState weatherState)
        {
            WeatherPreset preset = GetWeatherPreset(weatherState);
            if (preset != null)
            {
                ChangeWeather(preset);
            }
        }

        /// <summary>
        /// Change weather with transition
        /// </summary>
        private void ChangeWeather(WeatherPreset preset)
        {
            if (preset == null || preset == currentWeather)
                return;

            WeatherState fromState = currentWeatherState;
            targetWeather = preset;
            currentWeatherState = preset.weatherState;
            isTransitioning = true;
            transitionProgress = 0f;

            // Set duration
            weatherDuration = Random.Range(preset.minDuration, preset.maxDuration);
            weatherTimer = 0f;

            // Start transition
            OnWeatherTransitionStarted?.Invoke(fromState, currentWeatherState);
            OnWeatherChanged?.Invoke(currentWeatherState);

            // Apply target weather effects (particles, audio)
            ApplyWeatherEffects(preset);

            Debug.Log($"[WeatherManager] Transitioning from {fromState} to {currentWeatherState}");
        }

        /// <summary>
        /// Apply weather effects (particles, audio, fog, lighting)
        /// </summary>
        private void ApplyWeatherEffects(WeatherPreset preset)
        {
            if (preset == null)
                return;

            // Apply fog
            RenderSettings.fog = preset.fogDensity > 0f;
            if (!isTransitioning)
            {
                RenderSettings.fogDensity = preset.fogDensity;
                RenderSettings.fogColor = preset.fogColor;
            }

            // Apply lighting
            if (!isTransitioning)
            {
                foreach (var kvp in originalLightIntensities)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.intensity = kvp.Value * preset.lightIntensityMultiplier;
                    }
                }
            }

            // Spawn particle effects
            if (currentParticleEffect != null)
            {
                Destroy(currentParticleEffect);
            }

            if (preset.particleEffectPrefab != null && particleSpawnPoint != null)
            {
                currentParticleEffect = Instantiate(preset.particleEffectPrefab, particleSpawnPoint.position, Quaternion.identity, particleContainer);

                // Set emission rate
                ParticleSystem ps = currentParticleEffect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    var emission = ps.emission;
                    emission.rateOverTime = preset.particleEmissionRate;
                }
            }

            // Play ambient sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(preset.ambientSoundName))
            {
                // Use AudioManager for weather sounds
                AudioManager.Instance.PlayAmbient(preset.ambientSoundName);
            }
            else if (weatherAudioSource != null)
            {
                // Fallback: use local audio source
                weatherAudioSource.Stop();

                if (!string.IsNullOrEmpty(preset.ambientSoundName))
                {
                    // Load audio clip from Resources
                    AudioClip clip = Resources.Load<AudioClip>($"Audio/Weather/{preset.ambientSoundName}");
                    if (clip != null)
                    {
                        weatherAudioSource.clip = clip;
                        weatherAudioSource.volume = preset.ambientSoundVolume;
                        weatherAudioSource.Play();
                    }
                }
            }
        }

        /// <summary>
        /// Change to random weather based on probabilities
        /// </summary>
        public void ChangeToRandomWeather()
        {
            if (weatherPresets == null || weatherPresets.Length == 0)
                return;

            // Get current time of day
            bool isDaytime = true;
            if (TimeManager.Instance != null)
            {
                isDaytime = TimeManager.Instance.IsDaytime();
            }

            // Calculate total weight
            float totalWeight = 0f;
            List<WeatherPreset> validPresets = new List<WeatherPreset>();

            foreach (var preset in weatherPresets)
            {
                // Check time of day
                bool validTime = (isDaytime && preset.canOccurDuringDay) || (!isDaytime && preset.canOccurDuringNight);
                if (!validTime)
                    continue;

                // Don't pick current weather
                if (preset == currentWeather)
                    continue;

                validPresets.Add(preset);
                totalWeight += preset.spawnProbability;
            }

            if (validPresets.Count == 0)
                return;

            // Roll for weather
            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var preset in validPresets)
            {
                cumulative += preset.spawnProbability;
                if (roll <= cumulative)
                {
                    ChangeWeather(preset);
                    return;
                }
            }

            // Fallback: use first valid preset
            ChangeWeather(validPresets[0]);
        }

        /// <summary>
        /// Get weather preset by state
        /// </summary>
        public WeatherPreset GetWeatherPreset(WeatherState state)
        {
            if (weatherPresets == null)
                return null;

            foreach (var preset in weatherPresets)
            {
                if (preset.weatherState == state)
                {
                    return preset;
                }
            }

            return null;
        }

        /// <summary>
        /// Get current weather state
        /// </summary>
        public WeatherState GetCurrentWeather()
        {
            return currentWeatherState;
        }

        /// <summary>
        /// Get current weather preset
        /// </summary>
        public WeatherPreset GetCurrentWeatherPreset()
        {
            return currentWeather;
        }

        /// <summary>
        /// Is weather transitioning?
        /// </summary>
        public bool IsTransitioning()
        {
            return isTransitioning;
        }

        /// <summary>
        /// Get weather transition progress (0-1)
        /// </summary>
        public float GetTransitionProgress()
        {
            return transitionProgress;
        }

        /// <summary>
        /// Get current wind direction
        /// </summary>
        public Vector3 GetWindDirection()
        {
            return currentWeather != null ? currentWeather.windDirection.normalized : Vector3.zero;
        }

        /// <summary>
        /// Get current wind strength
        /// </summary>
        public float GetWindStrength()
        {
            return currentWeather != null ? currentWeather.windStrength : 0f;
        }

        /// <summary>
        /// Create default weather presets
        /// </summary>
        private void CreateDefaultWeatherPresets()
        {
            weatherPresets = new WeatherPreset[5];

            // Clear
            weatherPresets[0] = new WeatherPreset
            {
                weatherState = WeatherState.Clear,
                weatherName = "Clear",
                description = "Clear skies with bright sunshine",
                fogDensity = 0f,
                fogColor = Color.white,
                lightIntensityMultiplier = 1f,
                ambientColorMultiplier = Color.white,
                windStrength = 0.5f,
                spawnProbability = 40f,
                minDuration = 300f,
                maxDuration = 600f
            };

            // Cloudy
            weatherPresets[1] = new WeatherPreset
            {
                weatherState = WeatherState.Cloudy,
                weatherName = "Cloudy",
                description = "Overcast skies",
                fogDensity = 0.01f,
                fogColor = new Color(0.8f, 0.8f, 0.8f),
                lightIntensityMultiplier = 0.7f,
                ambientColorMultiplier = new Color(0.9f, 0.9f, 0.9f),
                windStrength = 1f,
                spawnProbability = 30f,
                minDuration = 180f,
                maxDuration = 400f
            };

            // Rain
            weatherPresets[2] = new WeatherPreset
            {
                weatherState = WeatherState.Rain,
                weatherName = "Rain",
                description = "Light rainfall",
                fogDensity = 0.02f,
                fogColor = new Color(0.7f, 0.7f, 0.8f),
                lightIntensityMultiplier = 0.6f,
                ambientColorMultiplier = new Color(0.8f, 0.8f, 0.9f),
                windStrength = 2f,
                spawnProbability = 20f,
                minDuration = 120f,
                maxDuration = 300f
            };

            // Fog
            weatherPresets[3] = new WeatherPreset
            {
                weatherState = WeatherState.Fog,
                weatherName = "Fog",
                description = "Dense fog reduces visibility",
                fogDensity = 0.05f,
                fogColor = new Color(0.8f, 0.8f, 0.85f),
                lightIntensityMultiplier = 0.5f,
                ambientColorMultiplier = new Color(0.85f, 0.85f, 0.9f),
                windStrength = 0.2f,
                spawnProbability = 10f,
                minDuration = 60f,
                maxDuration = 180f,
                canOccurDuringDay = true,
                canOccurDuringNight = true
            };

            // Storm
            weatherPresets[4] = new WeatherPreset
            {
                weatherState = WeatherState.Storm,
                weatherName = "Storm",
                description = "Heavy rain and strong winds",
                fogDensity = 0.03f,
                fogColor = new Color(0.5f, 0.5f, 0.6f),
                lightIntensityMultiplier = 0.4f,
                ambientColorMultiplier = new Color(0.7f, 0.7f, 0.8f),
                windStrength = 5f,
                spawnProbability = 5f,
                minDuration = 60f,
                maxDuration = 180f
            };
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public WeatherSaveData GetSaveData()
        {
            WeatherSaveData data = new WeatherSaveData
            {
                currentWeatherState = currentWeatherState.ToString(),
                weatherDurationRemaining = weatherDuration - weatherTimer
            };

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(WeatherSaveData data)
        {
            if (data == null)
                return;

            // Parse weather state
            if (System.Enum.TryParse<WeatherState>(data.currentWeatherState, out WeatherState state))
            {
                SetWeatherImmediate(state);
                weatherTimer = 0f;
                weatherDuration = Mathf.Max(data.weatherDurationRemaining, 60f);
            }

            Debug.Log($"[WeatherManager] Loaded weather: {currentWeatherState}");
        }

        private void OnDestroy()
        {
            // Clean up particle effects
            if (currentParticleEffect != null)
            {
                Destroy(currentParticleEffect);
            }

            // Restore original settings
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogColor = originalFogColor;
        }
    }
}
