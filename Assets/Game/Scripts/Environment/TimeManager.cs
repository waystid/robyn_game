using UnityEngine;
using UnityEngine.Events;

namespace CozyGame.Environment
{
    /// <summary>
    /// Time of day periods
    /// </summary>
    public enum TimeOfDay
    {
        Dawn,       // 5-7 AM
        Morning,    // 7-12 AM
        Afternoon,  // 12-5 PM
        Dusk,       // 5-7 PM
        Evening,    // 7-10 PM
        Night       // 10 PM - 5 AM
    }

    /// <summary>
    /// Time manager singleton.
    /// Manages 24-hour day/night cycle with directional light rotation.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [Tooltip("Current hour of day (0-24)")]
        [Range(0f, 24f)]
        public float currentHour = 12f;

        [Tooltip("Time scale (1 = real-time, 60 = 1 minute = 1 hour)")]
        [Range(0f, 1000f)]
        public float timeScale = 60f;

        [Tooltip("Pause time progression")]
        public bool pauseTime = false;

        [Header("Day Length")]
        [Tooltip("Length of a full day in real seconds")]
        public float dayLengthInSeconds = 1440f; // 24 minutes

        [Header("Sun & Moon")]
        [Tooltip("Directional light (sun)")]
        public Light sunLight;

        [Tooltip("Directional light (moon)")]
        public Light moonLight;

        [Tooltip("Sun rotation curve (0-24 hours)")]
        public AnimationCurve sunRotationCurve = AnimationCurve.Linear(0, 0, 24, 360);

        [Tooltip("Moon rotation curve (0-24 hours)")]
        public AnimationCurve moonRotationCurve = AnimationCurve.Linear(0, 180, 24, 540);

        [Header("Light Intensity")]
        [Tooltip("Sun intensity by hour")]
        public AnimationCurve sunIntensityCurve = AnimationCurve.EaseInOut(0, 0, 12, 1);

        [Tooltip("Moon intensity by hour")]
        public AnimationCurve moonIntensityCurve = AnimationCurve.EaseInOut(0, 0.3f, 12, 0);

        [Header("Ambient Light")]
        [Tooltip("Use ambient light color gradient")]
        public bool useAmbientGradient = true;

        [Tooltip("Ambient light color by time of day")]
        public Gradient ambientColorGradient;

        [Header("Skybox")]
        [Tooltip("Use skybox blend")]
        public bool useSkyboxBlend = true;

        [Tooltip("Day skybox material")]
        public Material daySkybox;

        [Tooltip("Night skybox material")]
        public Material nightSkybox;

        [Tooltip("Skybox blend by hour")]
        public AnimationCurve skyboxBlendCurve = AnimationCurve.EaseInOut(0, 1, 24, 1);

        [Header("Events")]
        public UnityEvent<float> OnHourChanged; // Current hour
        public UnityEvent<TimeOfDay> OnTimeOfDayChanged;
        public UnityEvent OnDayStart; // 6 AM
        public UnityEvent OnNightStart; // 6 PM

        // State
        private int currentDay = 0;
        private TimeOfDay currentTimeOfDay;
        private TimeOfDay previousTimeOfDay;

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
        /// Initialize time manager
        /// </summary>
        private void Initialize()
        {
            // Calculate initial time scale from day length
            if (dayLengthInSeconds > 0)
            {
                timeScale = 86400f / dayLengthInSeconds; // 86400 seconds in a day
            }

            // Setup default gradients if null
            if (ambientColorGradient == null)
            {
                ambientColorGradient = new Gradient();
                GradientColorKey[] colorKeys = new GradientColorKey[4];
                colorKeys[0] = new GradientColorKey(new Color(0.3f, 0.3f, 0.4f), 0f); // Night
                colorKeys[1] = new GradientColorKey(new Color(1f, 0.6f, 0.4f), 0.25f); // Dawn
                colorKeys[2] = new GradientColorKey(new Color(1f, 1f, 1f), 0.5f); // Day
                colorKeys[3] = new GradientColorKey(new Color(0.3f, 0.3f, 0.4f), 1f); // Night
                ambientColorGradient.colorKeys = colorKeys;
            }

            currentTimeOfDay = GetTimeOfDay();
            previousTimeOfDay = currentTimeOfDay;

            Debug.Log("[TimeManager] Initialized");
        }

        private void Update()
        {
            if (pauseTime)
                return;

            // Update time
            float deltaHours = (Time.deltaTime * timeScale) / 3600f;
            currentHour += deltaHours;

            // Wrap around 24 hours
            if (currentHour >= 24f)
            {
                currentHour -= 24f;
                currentDay++;
                Debug.Log($"[TimeManager] Day {currentDay} started");
            }

            // Update lighting
            UpdateSunMoon();
            UpdateAmbientLight();
            UpdateSkybox();

            // Check time of day changes
            currentTimeOfDay = GetTimeOfDay();
            if (currentTimeOfDay != previousTimeOfDay)
            {
                OnTimeOfDayChanged?.Invoke(currentTimeOfDay);
                previousTimeOfDay = currentTimeOfDay;

                // Check for day/night start
                if (currentTimeOfDay == TimeOfDay.Morning && previousTimeOfDay == TimeOfDay.Dawn)
                {
                    OnDayStart?.Invoke();
                }
                else if (currentTimeOfDay == TimeOfDay.Evening && previousTimeOfDay == TimeOfDay.Dusk)
                {
                    OnNightStart?.Invoke();
                }
            }

            // Trigger hourly event
            OnHourChanged?.Invoke(currentHour);
        }

        /// <summary>
        /// Update sun and moon rotation/intensity
        /// </summary>
        private void UpdateSunMoon()
        {
            if (sunLight != null)
            {
                // Rotate sun
                float sunAngle = sunRotationCurve.Evaluate(currentHour);
                sunLight.transform.rotation = Quaternion.Euler(sunAngle - 90f, 0f, 0f);

                // Update sun intensity
                sunLight.intensity = sunIntensityCurve.Evaluate(currentHour);

                // Disable sun at night
                sunLight.enabled = currentHour >= 5f && currentHour <= 19f;
            }

            if (moonLight != null)
            {
                // Rotate moon
                float moonAngle = moonRotationCurve.Evaluate(currentHour);
                moonLight.transform.rotation = Quaternion.Euler(moonAngle - 90f, 0f, 0f);

                // Update moon intensity
                moonLight.intensity = moonIntensityCurve.Evaluate(currentHour);

                // Disable moon during day
                moonLight.enabled = currentHour < 5f || currentHour > 19f;
            }
        }

        /// <summary>
        /// Update ambient light color
        /// </summary>
        private void UpdateAmbientLight()
        {
            if (!useAmbientGradient)
                return;

            // Normalize hour to 0-1
            float t = currentHour / 24f;

            // Get color from gradient
            Color ambientColor = ambientColorGradient.Evaluate(t);

            // Apply to ambient light
            RenderSettings.ambientLight = ambientColor;
        }

        /// <summary>
        /// Update skybox blend
        /// </summary>
        private void UpdateSkybox()
        {
            if (!useSkyboxBlend || daySkybox == null || nightSkybox == null)
                return;

            // Get blend value
            float blend = skyboxBlendCurve.Evaluate(currentHour);

            // Lerp between day and night skybox
            // Note: This is a simplified approach. For proper skybox blending,
            // you would need a custom shader or use RenderSettings.skybox with Material.Lerp
            if (blend > 0.5f && RenderSettings.skybox != daySkybox)
            {
                RenderSettings.skybox = daySkybox;
            }
            else if (blend <= 0.5f && RenderSettings.skybox != nightSkybox)
            {
                RenderSettings.skybox = nightSkybox;
            }
        }

        /// <summary>
        /// Get current time of day
        /// </summary>
        public TimeOfDay GetTimeOfDay()
        {
            if (currentHour >= 5f && currentHour < 7f)
                return TimeOfDay.Dawn;
            else if (currentHour >= 7f && currentHour < 12f)
                return TimeOfDay.Morning;
            else if (currentHour >= 12f && currentHour < 17f)
                return TimeOfDay.Afternoon;
            else if (currentHour >= 17f && currentHour < 19f)
                return TimeOfDay.Dusk;
            else if (currentHour >= 19f && currentHour < 22f)
                return TimeOfDay.Evening;
            else
                return TimeOfDay.Night;
        }

        /// <summary>
        /// Set time of day
        /// </summary>
        public void SetTime(float hour)
        {
            currentHour = Mathf.Clamp(hour, 0f, 24f);
            Debug.Log($"[TimeManager] Time set to {currentHour:F2}");
        }

        /// <summary>
        /// Set time to specific period
        /// </summary>
        public void SetTimeOfDay(TimeOfDay timeOfDay)
        {
            switch (timeOfDay)
            {
                case TimeOfDay.Dawn:
                    SetTime(6f);
                    break;
                case TimeOfDay.Morning:
                    SetTime(9f);
                    break;
                case TimeOfDay.Afternoon:
                    SetTime(14f);
                    break;
                case TimeOfDay.Dusk:
                    SetTime(18f);
                    break;
                case TimeOfDay.Evening:
                    SetTime(20f);
                    break;
                case TimeOfDay.Night:
                    SetTime(0f);
                    break;
            }
        }

        /// <summary>
        /// Get current day
        /// </summary>
        public int GetCurrentDay()
        {
            return currentDay;
        }

        /// <summary>
        /// Get current hour
        /// </summary>
        public float GetCurrentHour()
        {
            return currentHour;
        }

        /// <summary>
        /// Get formatted time string
        /// </summary>
        public string GetFormattedTime(bool use24Hour = false)
        {
            int hour = Mathf.FloorToInt(currentHour);
            int minute = Mathf.FloorToInt((currentHour - hour) * 60f);

            if (use24Hour)
            {
                return $"{hour:D2}:{minute:D2}";
            }
            else
            {
                int hour12 = hour == 0 ? 12 : (hour > 12 ? hour - 12 : hour);
                string period = hour >= 12 ? "PM" : "AM";
                return $"{hour12}:{minute:D2} {period}";
            }
        }

        /// <summary>
        /// Is it daytime?
        /// </summary>
        public bool IsDaytime()
        {
            return currentHour >= 6f && currentHour < 18f;
        }

        /// <summary>
        /// Is it nighttime?
        /// </summary>
        public bool IsNighttime()
        {
            return !IsDaytime();
        }

        /// <summary>
        /// Pause/unpause time
        /// </summary>
        public void SetPaused(bool paused)
        {
            pauseTime = paused;
        }

        /// <summary>
        /// Set time scale
        /// </summary>
        public void SetTimeScale(float scale)
        {
            timeScale = Mathf.Max(0f, scale);
        }

        private void OnValidate()
        {
            // Clamp hour
            currentHour = Mathf.Clamp(currentHour, 0f, 24f);
        }
    }
}
