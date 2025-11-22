using UnityEngine;
using System.Collections;

namespace CozyGame.CameraSystem
{
    /// <summary>
    /// Screen shake preset
    /// </summary>
    [System.Serializable]
    public class ShakePreset
    {
        public string presetName = "Default";
        public float intensity = 0.3f;
        public float duration = 0.3f;
        public float frequency = 20f;
        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }

    /// <summary>
    /// Screen shake controller.
    /// Applies camera shake effects for impacts and events.
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Enable screen shake")]
        public bool shakeEnabled = true;

        [Tooltip("Shake intensity multiplier")]
        [Range(0f, 2f)]
        public float intensityMultiplier = 1f;

        [Tooltip("Max shake intensity")]
        public float maxIntensity = 1f;

        [Header("Shake Presets")]
        [Tooltip("Predefined shake presets")]
        public ShakePreset[] shakePresets = new ShakePreset[]
        {
            new ShakePreset { presetName = "Light", intensity = 0.1f, duration = 0.2f, frequency = 15f },
            new ShakePreset { presetName = "Medium", intensity = 0.3f, duration = 0.3f, frequency = 20f },
            new ShakePreset { presetName = "Heavy", intensity = 0.5f, duration = 0.5f, frequency = 25f },
            new ShakePreset { presetName = "Explosion", intensity = 0.8f, duration = 0.7f, frequency = 30f }
        };

        [Header("Rotation Shake")]
        [Tooltip("Enable rotation shake")]
        public bool rotationShake = true;

        [Tooltip("Rotation intensity multiplier")]
        [Range(0f, 1f)]
        public float rotationIntensity = 0.5f;

        // State
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private bool isShaking = false;
        private float shakeTimer = 0f;

        private void Start()
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }

        /// <summary>
        /// Trigger screen shake
        /// </summary>
        public void TriggerShake(float intensity, float duration, float frequency = 20f)
        {
            if (!shakeEnabled)
                return;

            // Clamp intensity
            intensity = Mathf.Clamp(intensity * intensityMultiplier, 0f, maxIntensity);

            // Stop existing shake
            if (isShaking)
            {
                StopAllCoroutines();
            }

            // Start new shake
            StartCoroutine(ShakeCoroutine(intensity, duration, frequency));
        }

        /// <summary>
        /// Trigger shake with preset
        /// </summary>
        public void TriggerShakePreset(string presetName)
        {
            ShakePreset preset = System.Array.Find(shakePresets, p => p.presetName == presetName);
            if (preset != null)
            {
                TriggerShake(preset.intensity, preset.duration, preset.frequency);
            }
            else
            {
                Debug.LogWarning($"[ScreenShake] Preset '{presetName}' not found");
            }
        }

        /// <summary>
        /// Shake coroutine
        /// </summary>
        private IEnumerator ShakeCoroutine(float intensity, float duration, float frequency)
        {
            isShaking = true;
            shakeTimer = 0f;

            Vector3 originalLocalPos = transform.localPosition;
            Quaternion originalLocalRot = transform.localRotation;

            while (shakeTimer < duration)
            {
                shakeTimer += Time.unscaledDeltaTime;
                float progress = shakeTimer / duration;

                // Intensity falloff over time
                float currentIntensity = intensity * (1f - progress);

                // Perlin noise for smooth shake
                float noiseX = (Mathf.PerlinNoise(shakeTimer * frequency, 0f) - 0.5f) * 2f;
                float noiseY = (Mathf.PerlinNoise(0f, shakeTimer * frequency) - 0.5f) * 2f;

                // Apply position shake
                Vector3 shakeOffset = new Vector3(noiseX, noiseY, 0f) * currentIntensity;
                transform.localPosition = originalLocalPos + shakeOffset;

                // Apply rotation shake
                if (rotationShake)
                {
                    float noiseZ = (Mathf.PerlinNoise(shakeTimer * frequency, shakeTimer * frequency) - 0.5f) * 2f;
                    float rotationAmount = noiseZ * currentIntensity * rotationIntensity * 5f;
                    transform.localRotation = originalLocalRot * Quaternion.Euler(0f, 0f, rotationAmount);
                }

                yield return null;
            }

            // Reset to original
            transform.localPosition = originalLocalPos;
            transform.localRotation = originalLocalRot;
            isShaking = false;
        }

        /// <summary>
        /// Stop shake
        /// </summary>
        public void StopShake()
        {
            if (isShaking)
            {
                StopAllCoroutines();
                transform.localPosition = originalPosition;
                transform.localRotation = originalRotation;
                isShaking = false;
            }
        }

        /// <summary>
        /// Set shake enabled
        /// </summary>
        public void SetShakeEnabled(bool enabled)
        {
            shakeEnabled = enabled;

            if (!enabled)
            {
                StopShake();
            }
        }

        /// <summary>
        /// Is currently shaking
        /// </summary>
        public bool IsShaking()
        {
            return isShaking;
        }
    }

    /// <summary>
    /// Slow motion controller.
    /// Manages time scale effects for dramatic moments.
    /// </summary>
    public class SlowMotionController : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Enable slow motion")]
        public bool slowMotionEnabled = true;

        [Tooltip("Default time scale")]
        public float defaultTimeScale = 1f;

        [Tooltip("Min time scale (max slow motion)")]
        public float minTimeScale = 0.1f;

        [Header("Presets")]
        [Tooltip("Slow motion presets")]
        public SlowMotionPreset[] presets = new SlowMotionPreset[]
        {
            new SlowMotionPreset { presetName = "Slight", slowAmount = 0.7f, duration = 0.5f },
            new SlowMotionPreset { presetName = "Medium", slowAmount = 0.5f, duration = 1f },
            new SlowMotionPreset { presetName = "Heavy", slowAmount = 0.3f, duration = 1.5f },
            new SlowMotionPreset { presetName = "BulletTime", slowAmount = 0.1f, duration = 2f }
        };

        // State
        private bool isSlowMotion = false;
        private float slowMotionTimer = 0f;

        private void Update()
        {
            // Ensure time scale doesn't get stuck
            if (!isSlowMotion && Time.timeScale != defaultTimeScale)
            {
                Time.timeScale = defaultTimeScale;
            }
        }

        /// <summary>
        /// Apply slow motion
        /// </summary>
        public void ApplySlowMotion(float slowAmount, float duration)
        {
            if (!slowMotionEnabled)
                return;

            // Clamp slow amount
            slowAmount = Mathf.Clamp(slowAmount, minTimeScale, 1f);

            // Stop existing slow motion
            if (isSlowMotion)
            {
                StopAllCoroutines();
            }

            // Start new slow motion
            StartCoroutine(SlowMotionCoroutine(slowAmount, duration));
        }

        /// <summary>
        /// Apply slow motion with preset
        /// </summary>
        public void ApplySlowMotionPreset(string presetName)
        {
            SlowMotionPreset preset = System.Array.Find(presets, p => p.presetName == presetName);
            if (preset != null)
            {
                ApplySlowMotion(preset.slowAmount, preset.duration);
            }
            else
            {
                Debug.LogWarning($"[SlowMotion] Preset '{presetName}' not found");
            }
        }

        /// <summary>
        /// Slow motion coroutine
        /// </summary>
        private IEnumerator SlowMotionCoroutine(float slowAmount, float duration)
        {
            isSlowMotion = true;
            slowMotionTimer = 0f;

            float startTimeScale = Time.timeScale;

            // Slow down
            float slowDownDuration = 0.1f;
            float elapsed = 0f;

            while (elapsed < slowDownDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / slowDownDuration;

                Time.timeScale = Mathf.Lerp(startTimeScale, slowAmount, t);

                yield return null;
            }

            Time.timeScale = slowAmount;

            // Hold slow motion
            yield return new WaitForSecondsRealtime(duration);

            // Speed up
            float speedUpDuration = 0.2f;
            elapsed = 0f;

            while (elapsed < speedUpDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / speedUpDuration;

                Time.timeScale = Mathf.Lerp(slowAmount, defaultTimeScale, t);

                yield return null;
            }

            Time.timeScale = defaultTimeScale;
            isSlowMotion = false;
        }

        /// <summary>
        /// Stop slow motion
        /// </summary>
        public void StopSlowMotion()
        {
            if (isSlowMotion)
            {
                StopAllCoroutines();
                Time.timeScale = defaultTimeScale;
                isSlowMotion = false;
            }
        }

        /// <summary>
        /// Set slow motion enabled
        /// </summary>
        public void SetSlowMotionEnabled(bool enabled)
        {
            slowMotionEnabled = enabled;

            if (!enabled)
            {
                StopSlowMotion();
            }
        }

        /// <summary>
        /// Is currently in slow motion
        /// </summary>
        public bool IsSlowMotion()
        {
            return isSlowMotion;
        }

        /// <summary>
        /// Get current time scale
        /// </summary>
        public float GetCurrentTimeScale()
        {
            return Time.timeScale;
        }

        private void OnDestroy()
        {
            // Reset time scale when destroyed
            Time.timeScale = defaultTimeScale;
        }
    }

    /// <summary>
    /// Slow motion preset
    /// </summary>
    [System.Serializable]
    public class SlowMotionPreset
    {
        public string presetName = "Default";
        public float slowAmount = 0.5f;
        public float duration = 1f;
    }
}
