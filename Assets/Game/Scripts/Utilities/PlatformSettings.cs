using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Automatically adjusts settings based on platform
    /// Place on a GameObject in your main scene
    /// </summary>
    public class PlatformSettings : MonoBehaviour
    {
        [Header("Quality Settings")]
        public int pcQualityLevel = 4;      // High
        public int webGLQualityLevel = 2;   // Medium
        public int mobileQualityLevel = 1;  // Low

        [Header("Performance")]
        public int pcTargetFrameRate = 60;
        public int webGLTargetFrameRate = 30;
        public int mobileTargetFrameRate = 30;

        [Header("UI References")]
        [Tooltip("Mobile UI elements (joystick, etc.)")]
        public GameObject mobileUIControls;

        [Tooltip("PC UI elements (keyboard hints, etc.)")]
        public GameObject pcUIControls;

        [Header("Graphics")]
        public bool enableShadowsOnMobile = false;
        public bool enablePostProcessingOnMobile = false;

        private void Awake()
        {
            ApplyPlatformSettings();
        }

        private void ApplyPlatformSettings()
        {
#if UNITY_WEBGL
            // WebGL Settings
            QualitySettings.SetQualityLevel(webGLQualityLevel);
            Application.targetFrameRate = webGLTargetFrameRate;
            SetupUI(false, true);
            Debug.Log("Applied WebGL settings");

#elif UNITY_ANDROID || UNITY_IOS
            // Mobile Settings
            QualitySettings.SetQualityLevel(mobileQualityLevel);
            Application.targetFrameRate = mobileTargetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Disable expensive features on mobile
            if (!enableShadowsOnMobile)
                QualitySettings.shadows = ShadowQuality.Disable;

            SetupUI(true, false);
            Debug.Log("Applied Mobile settings");

#else
            // PC Settings
            QualitySettings.SetQualityLevel(pcQualityLevel);
            Application.targetFrameRate = pcTargetFrameRate;
            SetupUI(false, true);
            Debug.Log("Applied PC settings");
#endif

            // Universal settings
            QualitySettings.vSyncCount = 0; // Disable VSync (we use target framerate)
        }

        private void SetupUI(bool showMobileUI, bool showPCUI)
        {
            if (mobileUIControls != null)
                mobileUIControls.SetActive(showMobileUI);

            if (pcUIControls != null)
                pcUIControls.SetActive(showPCUI);
        }

        /// <summary>
        /// Call this to manually refresh settings (useful for testing)
        /// </summary>
        public void RefreshSettings()
        {
            ApplyPlatformSettings();
        }
    }
}
