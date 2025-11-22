using UnityEngine;

namespace CozyGame.UI
{
    /// <summary>
    /// Handles safe area for mobile devices (notches, rounded corners, etc.)
    /// Attach to the main Canvas or a panel that contains all UI
    /// Automatically adjusts to device safe area
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Apply safe area automatically on start?")]
        public bool applyOnStart = true;

        [Tooltip("Update safe area every frame? (needed for device rotation)")]
        public bool updateEveryFrame = true;

        [Header("Debug")]
        public bool showDebugInfo = false;

        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (applyOnStart)
            {
                ApplySafeArea();
            }
        }

        private void Update()
        {
            if (updateEveryFrame)
            {
                // Only update if safe area or screen size changed
                if (lastSafeArea != Screen.safeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
                {
                    ApplySafeArea();
                }
            }
        }

        /// <summary>
        /// Apply safe area to this RectTransform
        /// Call this manually if you have applyOnStart = false
        /// </summary>
        public void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            // Store for comparison
            lastSafeArea = safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            // Convert safe area to anchors
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Normalize to 0-1 range
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            // Apply to RectTransform
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;

            if (showDebugInfo)
            {
                Debug.Log($"Safe Area applied: {safeArea} | Anchors: Min({anchorMin}), Max({anchorMax})");
            }
        }

        /// <summary>
        /// Reset to full screen (remove safe area)
        /// </summary>
        public void ResetToFullScreen()
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }

        // Visualize safe area in editor
        private void OnValidate()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Safe Area (Editor Test)")]
        private void ApplySafeAreaEditor()
        {
            ApplySafeArea();
            Debug.Log($"Safe Area Test: Screen Size = {Screen.width}x{Screen.height}, Safe Area = {Screen.safeArea}");
        }
#endif
    }
}
