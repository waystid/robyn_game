using UnityEngine;
using System.Collections;

namespace CozyGame.CameraSystem
{
    /// <summary>
    /// Camera zoom preset
    /// </summary>
    [System.Serializable]
    public class ZoomPreset
    {
        public string presetName = "Default";
        public float targetSize = 5f;
        public float zoomDuration = 0.5f;
        public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    /// <summary>
    /// Camera effects controller.
    /// Manages camera zoom, focus, and coordinates all camera effects.
    /// </summary>
    public class CameraEffectsController : MonoBehaviour
    {
        public static CameraEffectsController Instance { get; private set; }

        [Header("Camera Reference")]
        [Tooltip("Main camera")]
        public Camera mainCamera;

        [Header("Zoom Settings")]
        [Tooltip("Enable zoom")]
        public bool zoomEnabled = true;

        [Tooltip("Default camera size")]
        public float defaultSize = 5f;

        [Tooltip("Min camera size (max zoom in)")]
        public float minSize = 2f;

        [Tooltip("Max camera size (max zoom out)")]
        public float maxSize = 10f;

        [Tooltip("Zoom presets")]
        public ZoomPreset[] zoomPresets = new ZoomPreset[]
        {
            new ZoomPreset { presetName = "Normal", targetSize = 5f, zoomDuration = 0.5f },
            new ZoomPreset { presetName = "Close", targetSize = 3f, zoomDuration = 0.3f },
            new ZoomPreset { presetName = "Far", targetSize = 8f, zoomDuration = 0.5f },
            new ZoomPreset { presetName = "Cinematic", targetSize = 4f, zoomDuration = 1f }
        };

        [Header("Follow Settings")]
        [Tooltip("Enable camera follow")]
        public bool followEnabled = true;

        [Tooltip("Follow target")]
        public Transform followTarget;

        [Tooltip("Follow smoothing")]
        [Range(0f, 1f)]
        public float followSmoothing = 0.1f;

        [Tooltip("Follow offset")]
        public Vector3 followOffset = new Vector3(0f, 0f, -10f);

        [Header("Focus Settings")]
        [Tooltip("Focus duration")]
        public float focusDuration = 0.5f;

        [Tooltip("Focus zoom amount")]
        public float focusZoomAmount = 0.8f;

        // State
        private float currentSize;
        private float targetSize;
        private bool isZooming = false;
        private Vector3 focusPosition;
        private bool isFocusing = false;
        private Vector3 originalPosition;

        // Components
        private ScreenShake screenShake;
        private SlowMotionController slowMotion;
        private PostProcessingController postProcessing;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Get or find camera
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
            }

            // Get components
            screenShake = GetComponent<ScreenShake>();
            slowMotion = GetComponent<SlowMotionController>();
            postProcessing = GetComponent<PostProcessingController>();
        }

        private void Start()
        {
            // Initialize
            if (mainCamera != null)
            {
                if (mainCamera.orthographic)
                {
                    currentSize = mainCamera.orthographicSize;
                    targetSize = currentSize;
                    defaultSize = currentSize;
                }
            }

            originalPosition = transform.position;
        }

        private void LateUpdate()
        {
            // Camera follow
            if (followEnabled && followTarget != null && !isFocusing)
            {
                UpdateFollow();
            }

            // Smooth zoom
            if (isZooming && mainCamera != null && mainCamera.orthographic)
            {
                currentSize = Mathf.Lerp(currentSize, targetSize, followSmoothing * 2f);
                mainCamera.orthographicSize = currentSize;

                if (Mathf.Abs(currentSize - targetSize) < 0.01f)
                {
                    currentSize = targetSize;
                    isZooming = false;
                }
            }
        }

        /// <summary>
        /// Update camera follow
        /// </summary>
        private void UpdateFollow()
        {
            Vector3 targetPosition = followTarget.position + followOffset;
            Vector3 smoothPosition = Vector3.Lerp(transform.position, targetPosition, followSmoothing);
            transform.position = smoothPosition;
        }

        /// <summary>
        /// Zoom to size
        /// </summary>
        public void ZoomTo(float size, float duration = 0.5f)
        {
            if (!zoomEnabled || mainCamera == null || !mainCamera.orthographic)
                return;

            size = Mathf.Clamp(size, minSize, maxSize);
            targetSize = size;
            isZooming = true;

            // Use coroutine for smooth zoom with curve
            StopAllCoroutines();
            StartCoroutine(SmoothZoom(size, duration));
        }

        /// <summary>
        /// Smooth zoom coroutine
        /// </summary>
        private IEnumerator SmoothZoom(float targetSize, float duration)
        {
            float startSize = currentSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                currentSize = Mathf.Lerp(startSize, targetSize, t);

                if (mainCamera != null && mainCamera.orthographic)
                {
                    mainCamera.orthographicSize = currentSize;
                }

                yield return null;
            }

            currentSize = targetSize;
            if (mainCamera != null && mainCamera.orthographic)
            {
                mainCamera.orthographicSize = currentSize;
            }

            isZooming = false;
        }

        /// <summary>
        /// Zoom to preset
        /// </summary>
        public void ZoomToPreset(string presetName)
        {
            ZoomPreset preset = System.Array.Find(zoomPresets, p => p.presetName == presetName);
            if (preset != null)
            {
                ZoomTo(preset.targetSize, preset.zoomDuration);
            }
            else
            {
                Debug.LogWarning($"[CameraEffects] Zoom preset '{presetName}' not found");
            }
        }

        /// <summary>
        /// Reset zoom to default
        /// </summary>
        public void ResetZoom(float duration = 0.5f)
        {
            ZoomTo(defaultSize, duration);
        }

        /// <summary>
        /// Zoom in relative
        /// </summary>
        public void ZoomIn(float amount = 1f, float duration = 0.3f)
        {
            float newSize = currentSize - amount;
            ZoomTo(newSize, duration);
        }

        /// <summary>
        /// Zoom out relative
        /// </summary>
        public void ZoomOut(float amount = 1f, float duration = 0.3f)
        {
            float newSize = currentSize + amount;
            ZoomTo(newSize, duration);
        }

        /// <summary>
        /// Focus on position
        /// </summary>
        public void FocusOn(Vector3 worldPosition, float duration = -1f)
        {
            if (duration < 0f)
                duration = focusDuration;

            focusPosition = worldPosition;
            focusPosition.z = transform.position.z;
            isFocusing = true;

            StartCoroutine(FocusCoroutine(duration));
        }

        /// <summary>
        /// Focus on target
        /// </summary>
        public void FocusOn(Transform target, float duration = -1f)
        {
            if (target != null)
            {
                FocusOn(target.position, duration);
            }
        }

        /// <summary>
        /// Focus coroutine
        /// </summary>
        private IEnumerator FocusCoroutine(float duration)
        {
            Vector3 startPosition = transform.position;
            float startSize = currentSize;
            float targetFocusSize = currentSize * focusZoomAmount;

            float elapsed = 0f;

            // Focus in
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration * 0.5f);

                transform.position = Vector3.Lerp(startPosition, focusPosition, t);

                if (mainCamera != null && mainCamera.orthographic)
                {
                    currentSize = Mathf.Lerp(startSize, targetFocusSize, t);
                    mainCamera.orthographicSize = currentSize;
                }

                yield return null;
            }

            // Hold
            yield return new WaitForSecondsRealtime(duration * 0.3f);

            // Focus out
            elapsed = 0f;
            while (elapsed < duration * 0.2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (duration * 0.2f);

                transform.position = Vector3.Lerp(focusPosition, startPosition, t);

                if (mainCamera != null && mainCamera.orthographic)
                {
                    currentSize = Mathf.Lerp(targetFocusSize, startSize, t);
                    mainCamera.orthographicSize = currentSize;
                }

                yield return null;
            }

            transform.position = startPosition;
            if (mainCamera != null && mainCamera.orthographic)
            {
                mainCamera.orthographicSize = startSize;
            }

            isFocusing = false;
        }

        /// <summary>
        /// Stop focus
        /// </summary>
        public void StopFocus()
        {
            StopAllCoroutines();
            isFocusing = false;
        }

        /// <summary>
        /// Shake camera
        /// </summary>
        public void Shake(float intensity = 0.3f, float duration = 0.3f, float frequency = 20f)
        {
            if (screenShake != null)
            {
                screenShake.TriggerShake(intensity, duration, frequency);
            }
        }

        /// <summary>
        /// Apply slow motion
        /// </summary>
        public void SlowMotion(float slowAmount = 0.3f, float duration = 1f)
        {
            if (slowMotion != null)
            {
                slowMotion.ApplySlowMotion(slowAmount, duration);
            }
        }

        /// <summary>
        /// Set follow target
        /// </summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        /// <summary>
        /// Set follow enabled
        /// </summary>
        public void SetFollowEnabled(bool enabled)
        {
            followEnabled = enabled;
        }

        /// <summary>
        /// Get current zoom level (0-1, where 0 is max zoom out, 1 is max zoom in)
        /// </summary>
        public float GetZoomLevel()
        {
            return 1f - ((currentSize - minSize) / (maxSize - minSize));
        }

        /// <summary>
        /// Is currently focusing
        /// </summary>
        public bool IsFocusing()
        {
            return isFocusing;
        }

        /// <summary>
        /// Is currently zooming
        /// </summary>
        public bool IsZooming()
        {
            return isZooming;
        }
    }
}
