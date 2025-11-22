using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace CozyGame.Mobile
{
    /// <summary>
    /// Graphics quality level
    /// </summary>
    public enum GraphicsQualityLevel
    {
        Low,
        Medium,
        High,
        Ultra,
        Auto
    }

    /// <summary>
    /// Battery mode
    /// </summary>
    public enum BatteryMode
    {
        Performance,
        Balanced,
        PowerSaver
    }

    /// <summary>
    /// Performance profile
    /// </summary>
    [System.Serializable]
    public class PerformanceProfile
    {
        public string profileName = "Default";
        public GraphicsQualityLevel qualityLevel = GraphicsQualityLevel.Medium;
        public int targetFrameRate = 60;
        public float renderScale = 1f;
        public int shadowQuality = 2;
        public bool enableAntiAliasing = true;
        public bool enablePostProcessing = true;
        public bool enableParticles = true;
        public float lodBias = 1f;
        public int maxLODLevel = 0;
        public bool enableOcclusionCulling = true;
        public float particleQuality = 1f;
    }

    /// <summary>
    /// Mobile performance manager singleton.
    /// Handles performance optimization for mobile devices.
    /// </summary>
    public class MobilePerformanceManager : MonoBehaviour
    {
        public static MobilePerformanceManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Enable performance optimization")]
        public bool performanceOptimizationEnabled = true;

        [Tooltip("Auto-detect performance profile")]
        public bool autoDetectProfile = true;

        [Header("Performance Profiles")]
        public PerformanceProfile[] profiles = new PerformanceProfile[]
        {
            new PerformanceProfile
            {
                profileName = "Low",
                qualityLevel = GraphicsQualityLevel.Low,
                targetFrameRate = 30,
                renderScale = 0.7f,
                shadowQuality = 0,
                enableAntiAliasing = false,
                enablePostProcessing = false,
                enableParticles = true,
                lodBias = 0.5f,
                maxLODLevel = 2,
                particleQuality = 0.5f
            },
            new PerformanceProfile
            {
                profileName = "Medium",
                qualityLevel = GraphicsQualityLevel.Medium,
                targetFrameRate = 30,
                renderScale = 0.85f,
                shadowQuality = 1,
                enableAntiAliasing = false,
                enablePostProcessing = true,
                enableParticles = true,
                lodBias = 0.75f,
                maxLODLevel = 1,
                particleQuality = 0.75f
            },
            new PerformanceProfile
            {
                profileName = "High",
                qualityLevel = GraphicsQualityLevel.High,
                targetFrameRate = 60,
                renderScale = 1f,
                shadowQuality = 2,
                enableAntiAliasing = true,
                enablePostProcessing = true,
                enableParticles = true,
                lodBias = 1f,
                maxLODLevel = 0,
                particleQuality = 1f
            }
        };

        [Header("Battery Settings")]
        [Tooltip("Current battery mode")]
        public BatteryMode batteryMode = BatteryMode.Balanced;

        [Tooltip("Battery threshold for power saver (0-1)")]
        [Range(0f, 1f)]
        public float powerSaverThreshold = 0.2f;

        [Tooltip("Auto-enable power saver on low battery")]
        public bool autoEnablePowerSaver = true;

        [Header("Memory Management")]
        [Tooltip("Enable automatic garbage collection")]
        public bool autoGarbageCollection = true;

        [Tooltip("GC interval in seconds")]
        public float gcInterval = 30f;

        [Tooltip("Max memory usage (MB) before GC")]
        public long maxMemoryBeforeGC = 512;

        [Header("LOD Settings")]
        [Tooltip("Enable LOD management")]
        public bool enableLODManagement = true;

        [Tooltip("LOD bias multiplier")]
        [Range(0.1f, 2f)]
        public float lodBiasMultiplier = 1f;

        [Tooltip("Max LOD level (0 = highest quality)")]
        public int maxLODLevel = 0;

        [Header("Occlusion Culling")]
        [Tooltip("Enable occlusion culling")]
        public bool enableOcclusionCulling = true;

        [Tooltip("Culling distance")]
        public float cullingDistance = 50f;

        [Header("Monitoring")]
        [Tooltip("Monitor frame rate")]
        public bool monitorFrameRate = true;

        [Tooltip("Frame rate check interval")]
        public float frameRateCheckInterval = 1f;

        [Tooltip("Target frame rate")]
        public int targetFrameRate = 60;

        [Tooltip("Min acceptable frame rate")]
        public int minAcceptableFrameRate = 30;

        // State
        private PerformanceProfile currentProfile;
        private float lastGCTime = 0f;
        private float lastFrameRateCheck = 0f;
        private float currentFPS = 60f;
        private int frameCount = 0;
        private float deltaTimeAccumulator = 0f;
        private bool isLowPerformance = false;

        // Components
        private Camera mainCamera;
        private LODGroup[] lodGroups;

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
                return;
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;

            // Auto-detect profile
            if (autoDetectProfile)
            {
                DetectOptimalProfile();
            }
            else
            {
                // Apply default profile
                ApplyProfile(profiles[0].profileName);
            }

            // Find all LOD groups
            if (enableLODManagement)
            {
                lodGroups = FindObjectsOfType<LODGroup>();
            }

            // Start monitoring
            if (monitorFrameRate)
            {
                StartCoroutine(MonitorPerformance());
            }

            // Start GC management
            if (autoGarbageCollection)
            {
                StartCoroutine(ManageGarbageCollection());
            }
        }

        private void Update()
        {
            // Monitor frame rate
            if (monitorFrameRate)
            {
                frameCount++;
                deltaTimeAccumulator += Time.unscaledDeltaTime;

                if (Time.time - lastFrameRateCheck >= frameRateCheckInterval)
                {
                    currentFPS = frameCount / deltaTimeAccumulator;
                    frameCount = 0;
                    deltaTimeAccumulator = 0f;
                    lastFrameRateCheck = Time.time;

                    // Check if performance is low
                    if (currentFPS < minAcceptableFrameRate && !isLowPerformance)
                    {
                        OnLowPerformanceDetected();
                    }
                }
            }

            // Check battery level
            if (autoEnablePowerSaver)
            {
                CheckBatteryLevel();
            }
        }

        /// <summary>
        /// Detect optimal performance profile
        /// </summary>
        private void DetectOptimalProfile()
        {
            // Get device info
            int processorCount = SystemInfo.processorCount;
            int systemMemorySize = SystemInfo.systemMemorySize;
            int graphicsMemorySize = SystemInfo.graphicsMemorySize;
            string graphicsDeviceName = SystemInfo.graphicsDeviceName.ToLower();

            // Score device
            int score = 0;

            // CPU
            if (processorCount >= 8)
                score += 3;
            else if (processorCount >= 4)
                score += 2;
            else
                score += 1;

            // RAM
            if (systemMemorySize >= 8192)
                score += 3;
            else if (systemMemorySize >= 4096)
                score += 2;
            else if (systemMemorySize >= 2048)
                score += 1;

            // GPU Memory
            if (graphicsMemorySize >= 4096)
                score += 3;
            else if (graphicsMemorySize >= 2048)
                score += 2;
            else if (graphicsMemorySize >= 1024)
                score += 1;

            // Determine profile based on score
            string profileName;
            if (score >= 7)
                profileName = "High";
            else if (score >= 4)
                profileName = "Medium";
            else
                profileName = "Low";

            Debug.Log($"[Performance] Auto-detected profile: {profileName} (Score: {score}/9)");
            ApplyProfile(profileName);
        }

        /// <summary>
        /// Apply performance profile
        /// </summary>
        public void ApplyProfile(string profileName)
        {
            PerformanceProfile profile = System.Array.Find(profiles, p => p.profileName == profileName);
            if (profile == null)
            {
                Debug.LogWarning($"[Performance] Profile '{profileName}' not found");
                return;
            }

            currentProfile = profile;

            // Apply settings
            ApplyQualitySettings(profile);
            ApplyFrameRateSettings(profile);
            ApplyRenderSettings(profile);
            ApplyLODSettings(profile);
            ApplyParticleSettings(profile);
            ApplyPostProcessingSettings(profile);

            Debug.Log($"[Performance] Applied profile: {profileName}");
        }

        /// <summary>
        /// Apply quality settings
        /// </summary>
        private void ApplyQualitySettings(PerformanceProfile profile)
        {
            switch (profile.qualityLevel)
            {
                case GraphicsQualityLevel.Low:
                    QualitySettings.SetQualityLevel(0, true);
                    break;
                case GraphicsQualityLevel.Medium:
                    QualitySettings.SetQualityLevel(1, true);
                    break;
                case GraphicsQualityLevel.High:
                    QualitySettings.SetQualityLevel(2, true);
                    break;
                case GraphicsQualityLevel.Ultra:
                    QualitySettings.SetQualityLevel(3, true);
                    break;
            }

            // Shadow settings
            if (profile.shadowQuality == 0)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
            }
            else if (profile.shadowQuality == 1)
            {
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
            }
            else
            {
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.Medium;
            }

            // Anti-aliasing
            QualitySettings.antiAliasing = profile.enableAntiAliasing ? 2 : 0;
        }

        /// <summary>
        /// Apply frame rate settings
        /// </summary>
        private void ApplyFrameRateSettings(PerformanceProfile profile)
        {
            Application.targetFrameRate = profile.targetFrameRate;
            targetFrameRate = profile.targetFrameRate;

            // VSync (disable on mobile for better battery)
            QualitySettings.vSyncCount = 0;
        }

        /// <summary>
        /// Apply render settings
        /// </summary>
        private void ApplyRenderSettings(PerformanceProfile profile)
        {
            if (mainCamera != null)
            {
                // Render scale
                UniversalRenderPipelineAsset urpAsset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
                if (urpAsset != null)
                {
                    urpAsset.renderScale = profile.renderScale;
                }
            }

            // Occlusion culling
            if (mainCamera != null)
            {
                mainCamera.useOcclusionCulling = profile.enableOcclusionCulling;
                if (profile.enableOcclusionCulling)
                {
                    mainCamera.farClipPlane = cullingDistance;
                }
            }
        }

        /// <summary>
        /// Apply LOD settings
        /// </summary>
        private void ApplyLODSettings(PerformanceProfile profile)
        {
            if (!enableLODManagement)
                return;

            QualitySettings.lodBias = profile.lodBias * lodBiasMultiplier;
            QualitySettings.maximumLODLevel = profile.maxLODLevel;

            // Update all LOD groups
            if (lodGroups != null)
            {
                foreach (var lodGroup in lodGroups)
                {
                    if (lodGroup != null)
                    {
                        // Force LOD group to recalculate
                        lodGroup.RecalculateBounds();
                    }
                }
            }
        }

        /// <summary>
        /// Apply particle settings
        /// </summary>
        private void ApplyParticleSettings(PerformanceProfile profile)
        {
            if (!profile.enableParticles)
            {
                // Disable all particles
                ParticleSystem[] particles = FindObjectsOfType<ParticleSystem>();
                foreach (var ps in particles)
                {
                    ps.Stop();
                }
                return;
            }

            // Adjust particle quality
            QualitySettings.particleRaycastBudget = Mathf.RoundToInt(4096 * profile.particleQuality);

            // Apply VFX quality if VFXManager exists
            if (VFX.VFXManager.Instance != null)
            {
                VFX.VFXManager.Instance.SetQuality(profile.particleQuality);
            }
        }

        /// <summary>
        /// Apply post-processing settings
        /// </summary>
        private void ApplyPostProcessingSettings(PerformanceProfile profile)
        {
            if (CameraSystem.PostProcessingController.Instance != null)
            {
                CameraSystem.PostProcessingController.Instance.SetPostProcessingEnabled(profile.enablePostProcessing);
            }
        }

        /// <summary>
        /// Monitor performance coroutine
        /// </summary>
        private IEnumerator MonitorPerformance()
        {
            while (true)
            {
                yield return new WaitForSeconds(frameRateCheckInterval * 5f);

                // Log performance stats
                long memoryUsage = System.GC.GetTotalMemory(false) / 1048576; // MB
                Debug.Log($"[Performance] FPS: {currentFPS:F1}, Memory: {memoryUsage}MB");
            }
        }

        /// <summary>
        /// Manage garbage collection
        /// </summary>
        private IEnumerator ManageGarbageCollection()
        {
            while (true)
            {
                yield return new WaitForSeconds(gcInterval);

                long memoryUsage = System.GC.GetTotalMemory(false) / 1048576; // MB

                if (memoryUsage >= maxMemoryBeforeGC)
                {
                    Debug.Log($"[Performance] Running GC (Memory: {memoryUsage}MB)");
                    System.GC.Collect();
                    Resources.UnloadUnusedAssets();
                }
            }
        }

        /// <summary>
        /// Check battery level
        /// </summary>
        private void CheckBatteryLevel()
        {
            float batteryLevel = SystemInfo.batteryLevel;

            if (batteryLevel >= 0f && batteryLevel <= powerSaverThreshold)
            {
                if (batteryMode != BatteryMode.PowerSaver)
                {
                    SetBatteryMode(BatteryMode.PowerSaver);
                }
            }
        }

        /// <summary>
        /// Set battery mode
        /// </summary>
        public void SetBatteryMode(BatteryMode mode)
        {
            batteryMode = mode;

            switch (mode)
            {
                case BatteryMode.Performance:
                    ApplyProfile("High");
                    break;

                case BatteryMode.Balanced:
                    ApplyProfile("Medium");
                    break;

                case BatteryMode.PowerSaver:
                    ApplyProfile("Low");
                    // Additional power saving measures
                    Application.targetFrameRate = 30;
                    break;
            }

            Debug.Log($"[Performance] Battery mode set to: {mode}");
        }

        /// <summary>
        /// On low performance detected
        /// </summary>
        private void OnLowPerformanceDetected()
        {
            isLowPerformance = true;
            Debug.LogWarning($"[Performance] Low performance detected (FPS: {currentFPS:F1})");

            // Auto-downgrade quality
            if (currentProfile != null)
            {
                if (currentProfile.profileName == "High")
                {
                    ApplyProfile("Medium");
                }
                else if (currentProfile.profileName == "Medium")
                {
                    ApplyProfile("Low");
                }
            }
        }

        /// <summary>
        /// Force garbage collection
        /// </summary>
        public void ForceGarbageCollection()
        {
            System.GC.Collect();
            Resources.UnloadUnusedAssets();
            Debug.Log("[Performance] Forced garbage collection");
        }

        /// <summary>
        /// Get current FPS
        /// </summary>
        public float GetCurrentFPS()
        {
            return currentFPS;
        }

        /// <summary>
        /// Get memory usage (MB)
        /// </summary>
        public long GetMemoryUsage()
        {
            return System.GC.GetTotalMemory(false) / 1048576;
        }

        /// <summary>
        /// Get current profile
        /// </summary>
        public PerformanceProfile GetCurrentProfile()
        {
            return currentProfile;
        }

        /// <summary>
        /// Get battery level
        /// </summary>
        public float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }

        /// <summary>
        /// Is low performance mode
        /// </summary>
        public bool IsLowPerformance()
        {
            return isLowPerformance;
        }
    }
}
