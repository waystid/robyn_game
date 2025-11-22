using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CozyGame.Mobile
{
    /// <summary>
    /// Screen orientation type
    /// </summary>
    public enum ScreenOrientationType
    {
        Portrait,
        Landscape,
        Auto
    }

    /// <summary>
    /// UI scale mode for mobile
    /// </summary>
    public enum MobileUIScaleMode
    {
        Phone,
        Tablet,
        Desktop,
        Auto
    }

    /// <summary>
    /// Mobile UI layout preset
    /// </summary>
    [System.Serializable]
    public class MobileUILayout
    {
        public string layoutName = "Default";
        public MobileUIScaleMode scaleMode = MobileUIScaleMode.Auto;
        public float buttonSizeMultiplier = 1f;
        public float fontSize = 16f;
        public float spacing = 10f;
        public bool simplifiedUI = false;
        public bool showAdvancedOptions = true;
    }

    /// <summary>
    /// Mobile UI adapter singleton.
    /// Adapts UI elements for mobile devices with larger tap targets and simplified layouts.
    /// </summary>
    public class MobileUIAdapter : MonoBehaviour
    {
        public static MobileUIAdapter Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Enable mobile UI adaptation")]
        public bool mobileUIEnabled = true;

        [Tooltip("Auto-detect mobile device")]
        public bool autoDetectMobile = true;

        [Tooltip("Force mobile mode (for testing)")]
        public bool forceMobileMode = false;

        [Header("Screen Settings")]
        [Tooltip("Target screen orientation")]
        public ScreenOrientationType screenOrientation = ScreenOrientationType.Auto;

        [Tooltip("Target frame rate on mobile")]
        public int targetFrameRate = 60;

        [Header("Touch Target Settings")]
        [Tooltip("Minimum tap target size (Apple HIG: 44pt, Android: 48dp)")]
        public float minTapTargetSize = 44f;

        [Tooltip("Button size multiplier for mobile")]
        [Range(1f, 3f)]
        public float buttonSizeMultiplier = 1.5f;

        [Tooltip("Auto-resize buttons to meet minimum size")]
        public bool autoResizeButtons = true;

        [Header("UI Scaling")]
        [Tooltip("UI scale for different device types")]
        public float phoneScale = 1.0f;

        [Tooltip("Tablet UI scale")]
        public float tabletScale = 0.8f;

        [Tooltip("Desktop UI scale")]
        public float desktopScale = 0.7f;

        [Header("Font Settings")]
        [Tooltip("Font size multiplier for mobile")]
        [Range(1f, 2f)]
        public float fontSizeMultiplier = 1.2f;

        [Tooltip("Min font size")]
        public int minFontSize = 14;

        [Header("Simplified UI")]
        [Tooltip("Enable simplified UI for mobile")]
        public bool useSimplifiedUI = true;

        [Tooltip("Hide advanced options on mobile")]
        public bool hideAdvancedOptions = true;

        [Tooltip("Reduce animation complexity")]
        public bool reduceAnimations = true;

        [Header("Layout Presets")]
        public MobileUILayout[] layoutPresets = new MobileUILayout[]
        {
            new MobileUILayout
            {
                layoutName = "Phone",
                scaleMode = MobileUIScaleMode.Phone,
                buttonSizeMultiplier = 1.5f,
                fontSize = 16f,
                spacing = 12f,
                simplifiedUI = true,
                showAdvancedOptions = false
            },
            new MobileUILayout
            {
                layoutName = "Tablet",
                scaleMode = MobileUIScaleMode.Tablet,
                buttonSizeMultiplier = 1.2f,
                fontSize = 14f,
                spacing = 10f,
                simplifiedUI = false,
                showAdvancedOptions = true
            }
        };

        // State
        private bool isMobileDevice = false;
        private MobileUIScaleMode currentScaleMode = MobileUIScaleMode.Auto;
        private Canvas[] allCanvases;
        private CanvasScaler[] allCanvasScalers;
        private List<Button> adaptedButtons = new List<Button>();

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

            // Detect mobile device
            DetectMobileDevice();
        }

        private void Start()
        {
            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;

            // Set screen orientation
            ApplyScreenOrientation();

            // Apply mobile UI adaptations
            if (mobileUIEnabled && isMobileDevice)
            {
                ApplyMobileAdaptations();
            }
        }

        /// <summary>
        /// Detect if running on mobile device
        /// </summary>
        private void DetectMobileDevice()
        {
            if (forceMobileMode)
            {
                isMobileDevice = true;
                return;
            }

            if (autoDetectMobile)
            {
                isMobileDevice = Application.isMobilePlatform ||
                                 SystemInfo.deviceType == DeviceType.Handheld;

                // Additional checks
                if (!isMobileDevice)
                {
                    // Check DPI (mobile devices typically have higher DPI)
                    float dpi = Screen.dpi;
                    if (dpi > 200f)
                    {
                        isMobileDevice = true;
                    }
                }
            }

            // Determine scale mode
            DetermineScaleMode();
        }

        /// <summary>
        /// Determine UI scale mode based on device
        /// </summary>
        private void DetermineScaleMode()
        {
            if (!isMobileDevice)
            {
                currentScaleMode = MobileUIScaleMode.Desktop;
                return;
            }

            // Determine if tablet or phone based on screen size
            float diagonalInches = GetScreenDiagonalInches();

            if (diagonalInches >= 6.5f)
            {
                currentScaleMode = MobileUIScaleMode.Tablet;
            }
            else
            {
                currentScaleMode = MobileUIScaleMode.Phone;
            }
        }

        /// <summary>
        /// Get screen diagonal size in inches
        /// </summary>
        private float GetScreenDiagonalInches()
        {
            float dpi = Screen.dpi > 0 ? Screen.dpi : 160f; // Default to 160 if not available
            float width = Screen.width / dpi;
            float height = Screen.height / dpi;
            return Mathf.Sqrt(width * width + height * height);
        }

        /// <summary>
        /// Apply screen orientation
        /// </summary>
        private void ApplyScreenOrientation()
        {
            switch (screenOrientation)
            {
                case ScreenOrientationType.Portrait:
                    Screen.orientation = UnityEngine.ScreenOrientation.Portrait;
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = false;
                    Screen.autorotateToLandscapeLeft = false;
                    Screen.autorotateToLandscapeRight = false;
                    break;

                case ScreenOrientationType.Landscape:
                    Screen.orientation = UnityEngine.ScreenOrientation.LandscapeLeft;
                    Screen.autorotateToPortrait = false;
                    Screen.autorotateToPortraitUpsideDown = false;
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    break;

                case ScreenOrientationType.Auto:
                    Screen.orientation = UnityEngine.ScreenOrientation.AutoRotation;
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    break;
            }
        }

        /// <summary>
        /// Apply mobile UI adaptations
        /// </summary>
        public void ApplyMobileAdaptations()
        {
            // Find all canvases
            allCanvases = FindObjectsOfType<Canvas>();
            allCanvasScalers = FindObjectsOfType<CanvasScaler>();

            // Apply canvas scaler settings
            ApplyCanvasScaling();

            // Adapt buttons
            AdaptButtons();

            // Adapt text
            AdaptText();

            // Apply simplified UI
            if (useSimplifiedUI)
            {
                ApplySimplifiedUI();
            }
        }

        /// <summary>
        /// Apply canvas scaling
        /// </summary>
        private void ApplyCanvasScaling()
        {
            float scale = GetScaleForMode(currentScaleMode);

            foreach (var scaler in allCanvasScalers)
            {
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    // Adjust reference resolution for mobile
                    scaler.matchWidthOrHeight = Screen.width > Screen.height ? 0f : 1f;
                }
            }
        }

        /// <summary>
        /// Get scale for UI scale mode
        /// </summary>
        private float GetScaleForMode(MobileUIScaleMode mode)
        {
            switch (mode)
            {
                case MobileUIScaleMode.Phone:
                    return phoneScale;
                case MobileUIScaleMode.Tablet:
                    return tabletScale;
                case MobileUIScaleMode.Desktop:
                    return desktopScale;
                default:
                    return 1f;
            }
        }

        /// <summary>
        /// Adapt buttons for touch
        /// </summary>
        private void AdaptButtons()
        {
            Button[] buttons = FindObjectsOfType<Button>(true);

            foreach (var button in buttons)
            {
                AdaptButton(button);
            }
        }

        /// <summary>
        /// Adapt single button
        /// </summary>
        public void AdaptButton(Button button)
        {
            if (button == null || adaptedButtons.Contains(button))
                return;

            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null)
                return;

            // Apply size multiplier
            if (autoResizeButtons)
            {
                Vector2 currentSize = rectTransform.sizeDelta;
                Vector2 newSize = currentSize * buttonSizeMultiplier;

                // Ensure minimum tap target size
                newSize.x = Mathf.Max(newSize.x, minTapTargetSize);
                newSize.y = Mathf.Max(newSize.y, minTapTargetSize);

                rectTransform.sizeDelta = newSize;
            }

            // Increase touch area without changing visuals
            // Add padding to make tap target larger
            var padding = button.gameObject.GetComponent<LayoutElement>();
            if (padding == null && autoResizeButtons)
            {
                padding = button.gameObject.AddComponent<LayoutElement>();
                padding.minWidth = minTapTargetSize;
                padding.minHeight = minTapTargetSize;
            }

            adaptedButtons.Add(button);
        }

        /// <summary>
        /// Adapt text for mobile
        /// </summary>
        private void AdaptText()
        {
            Text[] texts = FindObjectsOfType<Text>(true);

            foreach (var text in texts)
            {
                int currentSize = text.fontSize;
                int newSize = Mathf.RoundToInt(currentSize * fontSizeMultiplier);
                newSize = Mathf.Max(newSize, minFontSize);
                text.fontSize = newSize;
            }

            // Also adapt TextMeshPro if available
            var tmpTexts = FindObjectsOfType<TMPro.TextMeshProUGUI>(true);
            foreach (var tmp in tmpTexts)
            {
                float currentSize = tmp.fontSize;
                float newSize = currentSize * fontSizeMultiplier;
                newSize = Mathf.Max(newSize, minFontSize);
                tmp.fontSize = newSize;
            }
        }

        /// <summary>
        /// Apply simplified UI for mobile
        /// </summary>
        private void ApplySimplifiedUI()
        {
            if (hideAdvancedOptions)
            {
                // Hide advanced UI elements (tagged with "AdvancedUI")
                GameObject[] advancedUI = GameObject.FindGameObjectsWithTag("AdvancedUI");
                foreach (var obj in advancedUI)
                {
                    obj.SetActive(false);
                }
            }

            if (reduceAnimations)
            {
                // Reduce animation complexity
                Animator[] animators = FindObjectsOfType<Animator>();
                foreach (var animator in animators)
                {
                    animator.speed = 1.5f; // Speed up animations
                }
            }
        }

        /// <summary>
        /// Apply layout preset
        /// </summary>
        public void ApplyLayoutPreset(string presetName)
        {
            MobileUILayout preset = System.Array.Find(layoutPresets, p => p.layoutName == presetName);
            if (preset != null)
            {
                buttonSizeMultiplier = preset.buttonSizeMultiplier;
                useSimplifiedUI = preset.simplifiedUI;
                hideAdvancedOptions = !preset.showAdvancedOptions;

                ApplyMobileAdaptations();
            }
            else
            {
                Debug.LogWarning($"[MobileUI] Layout preset '{presetName}' not found");
            }
        }

        /// <summary>
        /// Is mobile device
        /// </summary>
        public bool IsMobileDevice()
        {
            return isMobileDevice;
        }

        /// <summary>
        /// Get current scale mode
        /// </summary>
        public MobileUIScaleMode GetScaleMode()
        {
            return currentScaleMode;
        }

        /// <summary>
        /// Set mobile UI enabled
        /// </summary>
        public void SetMobileUIEnabled(bool enabled)
        {
            mobileUIEnabled = enabled;

            if (enabled && isMobileDevice)
            {
                ApplyMobileAdaptations();
            }
        }

        /// <summary>
        /// Get safe area (for notches, etc.)
        /// </summary>
        public Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        /// <summary>
        /// Apply safe area to rect transform
        /// </summary>
        public void ApplySafeArea(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }

        /// <summary>
        /// Get screen DPI
        /// </summary>
        public float GetScreenDPI()
        {
            return Screen.dpi > 0 ? Screen.dpi : 160f;
        }

        /// <summary>
        /// Is tablet device
        /// </summary>
        public bool IsTablet()
        {
            return currentScaleMode == MobileUIScaleMode.Tablet;
        }

        /// <summary>
        /// Is phone device
        /// </summary>
        public bool IsPhone()
        {
            return currentScaleMode == MobileUIScaleMode.Phone;
        }
    }
}
