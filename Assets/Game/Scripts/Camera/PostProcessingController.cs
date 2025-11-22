using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using System.Collections;

namespace CozyGame.CameraSystem
{
    /// <summary>
    /// Post-processing effect type
    /// </summary>
    public enum PostProcessingEffectType
    {
        Bloom,
        Vignette,
        ColorGrading,
        ChromaticAberration,
        DepthOfField,
        MotionBlur,
        AmbientOcclusion,
        All
    }

    /// <summary>
    /// Post-processing preset
    /// </summary>
    [System.Serializable]
    public class PostProcessingPreset
    {
        public string presetName = "Default";
        public bool bloomEnabled = true;
        public float bloomIntensity = 1f;
        public bool vignetteEnabled = true;
        public float vignetteIntensity = 0.3f;
        public Color vignetteColor = Color.black;
        public bool colorGradingEnabled = true;
        public float saturation = 0f;
        public float contrast = 0f;
        public Color colorFilter = Color.white;
    }

    /// <summary>
    /// Post-processing controller.
    /// Manages post-processing effects for visual enhancement.
    /// Note: Requires Unity Post Processing Stack v2 package.
    /// </summary>
    public class PostProcessingController : MonoBehaviour
    {
        public static PostProcessingController Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Enable post-processing")]
        public bool postProcessingEnabled = true;

        [Header("Presets")]
        [Tooltip("Post-processing presets")]
        public PostProcessingPreset[] presets = new PostProcessingPreset[]
        {
            new PostProcessingPreset
            {
                presetName = "Normal",
                bloomIntensity = 1f,
                vignetteIntensity = 0.3f,
                saturation = 0f,
                contrast = 0f
            },
            new PostProcessingPreset
            {
                presetName = "Dramatic",
                bloomIntensity = 2f,
                vignetteIntensity = 0.5f,
                saturation = 10f,
                contrast = 15f
            },
            new PostProcessingPreset
            {
                presetName = "DarkMode",
                bloomIntensity = 0.5f,
                vignetteIntensity = 0.7f,
                vignetteColor = Color.black,
                saturation = -20f,
                contrast = 10f
            },
            new PostProcessingPreset
            {
                presetName = "Cinematic",
                bloomIntensity = 1.5f,
                vignetteIntensity = 0.4f,
                saturation = 5f,
                contrast = 10f
            }
        };

        [Header("Dynamic Effects")]
        [Tooltip("Vignette intensity when damaged")]
        public float damageVignetteIntensity = 0.7f;

        [Tooltip("Damage vignette color")]
        public Color damageVignetteColor = new Color(0.8f, 0f, 0f);

        [Tooltip("Damage vignette duration")]
        public float damageVignetteDuration = 0.5f;

        [Tooltip("Focus vignette intensity")]
        public float focusVignetteIntensity = 0.5f;

        [Tooltip("Focus transition duration")]
        public float focusTransitionDuration = 0.3f;

        // State
        private PostProcessingPreset currentPreset;
        private bool isDamageEffect = false;
        private bool isFocusEffect = false;

#if UNITY_POST_PROCESSING_STACK_V2
        private PostProcessVolume volume;
        private Bloom bloom;
        private Vignette vignette;
        private ColorGrading colorGrading;
        private ChromaticAberration chromaticAberration;
        private DepthOfField depthOfField;
#endif

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
        }

        private void Start()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            // Get or create post-processing volume
            volume = GetComponent<PostProcessVolume>();
            if (volume == null)
            {
                volume = gameObject.AddComponent<PostProcessVolume>();
                volume.isGlobal = true;
                volume.priority = 1;

                // Create profile
                volume.profile = ScriptableObject.CreateInstance<PostProcessProfile>();
            }

            // Get or add effects
            if (volume.profile != null)
            {
                if (!volume.profile.TryGetSettings(out bloom))
                {
                    bloom = volume.profile.AddSettings<Bloom>();
                }

                if (!volume.profile.TryGetSettings(out vignette))
                {
                    vignette = volume.profile.AddSettings<Vignette>();
                }

                if (!volume.profile.TryGetSettings(out colorGrading))
                {
                    colorGrading = volume.profile.AddSettings<ColorGrading>();
                }

                if (!volume.profile.TryGetSettings(out chromaticAberration))
                {
                    chromaticAberration = volume.profile.AddSettings<ChromaticAberration>();
                }

                if (!volume.profile.TryGetSettings(out depthOfField))
                {
                    depthOfField = volume.profile.AddSettings<DepthOfField>();
                }
            }

            // Apply default preset
            if (presets.Length > 0)
            {
                ApplyPreset(presets[0].presetName);
            }
#else
            Debug.LogWarning("[PostProcessing] Unity Post Processing Stack v2 not installed!");
#endif
        }

        /// <summary>
        /// Apply post-processing preset
        /// </summary>
        public void ApplyPreset(string presetName, float transitionDuration = 0f)
        {
            PostProcessingPreset preset = System.Array.Find(presets, p => p.presetName == presetName);
            if (preset != null)
            {
                currentPreset = preset;

                if (transitionDuration > 0f)
                {
                    StartCoroutine(TransitionToPreset(preset, transitionDuration));
                }
                else
                {
                    ApplyPresetImmediate(preset);
                }
            }
            else
            {
                Debug.LogWarning($"[PostProcessing] Preset '{presetName}' not found");
            }
        }

        /// <summary>
        /// Apply preset immediately
        /// </summary>
        private void ApplyPresetImmediate(PostProcessingPreset preset)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            // Bloom
            if (bloom != null)
            {
                bloom.enabled.value = preset.bloomEnabled;
                bloom.intensity.value = preset.bloomIntensity;
            }

            // Vignette
            if (vignette != null && !isDamageEffect && !isFocusEffect)
            {
                vignette.enabled.value = preset.vignetteEnabled;
                vignette.intensity.value = preset.vignetteIntensity;
                vignette.color.value = preset.vignetteColor;
            }

            // Color Grading
            if (colorGrading != null)
            {
                colorGrading.enabled.value = preset.colorGradingEnabled;
                colorGrading.saturation.value = preset.saturation;
                colorGrading.contrast.value = preset.contrast;
                colorGrading.colorFilter.value = preset.colorFilter;
            }
#endif
        }

        /// <summary>
        /// Transition to preset over time
        /// </summary>
        private IEnumerator TransitionToPreset(PostProcessingPreset targetPreset, float duration)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            float elapsed = 0f;

            // Store start values
            float startBloomIntensity = bloom != null ? bloom.intensity.value : 1f;
            float startVignetteIntensity = vignette != null ? vignette.intensity.value : 0.3f;
            float startSaturation = colorGrading != null ? colorGrading.saturation.value : 0f;
            float startContrast = colorGrading != null ? colorGrading.contrast.value : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                // Lerp values
                if (bloom != null)
                {
                    bloom.intensity.value = Mathf.Lerp(startBloomIntensity, targetPreset.bloomIntensity, t);
                }

                if (vignette != null && !isDamageEffect)
                {
                    vignette.intensity.value = Mathf.Lerp(startVignetteIntensity, targetPreset.vignetteIntensity, t);
                }

                if (colorGrading != null)
                {
                    colorGrading.saturation.value = Mathf.Lerp(startSaturation, targetPreset.saturation, t);
                    colorGrading.contrast.value = Mathf.Lerp(startContrast, targetPreset.contrast, t);
                }

                yield return null;
            }

            // Apply final values
            ApplyPresetImmediate(targetPreset);
#else
            yield return null;
#endif
        }

        /// <summary>
        /// Set bloom intensity
        /// </summary>
        public void SetBloomIntensity(float intensity)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (bloom != null)
            {
                bloom.intensity.value = intensity;
            }
#endif
        }

        /// <summary>
        /// Set vignette intensity
        /// </summary>
        public void SetVignetteIntensity(float intensity, Color? color = null)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (vignette != null)
            {
                vignette.intensity.value = intensity;
                if (color.HasValue)
                {
                    vignette.color.value = color.Value;
                }
            }
#endif
        }

        /// <summary>
        /// Apply damage vignette effect
        /// </summary>
        public void ApplyDamageEffect()
        {
            if (!postProcessingEnabled)
                return;

            StartCoroutine(DamageEffectCoroutine());
        }

        /// <summary>
        /// Damage effect coroutine
        /// </summary>
        private IEnumerator DamageEffectCoroutine()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            isDamageEffect = true;

            if (vignette != null)
            {
                // Flash red vignette
                vignette.enabled.value = true;
                vignette.intensity.value = damageVignetteIntensity;
                vignette.color.value = damageVignetteColor;
            }

            yield return new WaitForSecondsRealtime(damageVignetteDuration);

            // Fade back to normal
            float elapsed = 0f;
            float fadeDuration = 0.3f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / fadeDuration;

                if (vignette != null && currentPreset != null)
                {
                    vignette.intensity.value = Mathf.Lerp(damageVignetteIntensity, currentPreset.vignetteIntensity, t);
                    vignette.color.value = Color.Lerp(damageVignetteColor, currentPreset.vignetteColor, t);
                }

                yield return null;
            }

            isDamageEffect = false;

            // Reset to preset values
            if (currentPreset != null && vignette != null)
            {
                vignette.intensity.value = currentPreset.vignetteIntensity;
                vignette.color.value = currentPreset.vignetteColor;
            }
#else
            yield return null;
#endif
        }

        /// <summary>
        /// Apply focus effect (vignette)
        /// </summary>
        public void ApplyFocusEffect(bool enable)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (!postProcessingEnabled || vignette == null)
                return;

            isFocusEffect = enable;

            if (enable)
            {
                StartCoroutine(FocusEffectCoroutine(true));
            }
            else
            {
                StartCoroutine(FocusEffectCoroutine(false));
            }
#endif
        }

        /// <summary>
        /// Focus effect coroutine
        /// </summary>
        private IEnumerator FocusEffectCoroutine(bool fadeIn)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (vignette == null || currentPreset == null)
                yield break;

            float elapsed = 0f;
            float startIntensity = vignette.intensity.value;
            float targetIntensity = fadeIn ? focusVignetteIntensity : currentPreset.vignetteIntensity;

            while (elapsed < focusTransitionDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / focusTransitionDuration;

                vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, t);

                yield return null;
            }

            vignette.intensity.value = targetIntensity;

            if (!fadeIn)
            {
                isFocusEffect = false;
            }
#else
            yield return null;
#endif
        }

        /// <summary>
        /// Set effect enabled
        /// </summary>
        public void SetEffectEnabled(PostProcessingEffectType effectType, bool enabled)
        {
#if UNITY_POST_PROCESSING_STACK_V2
            switch (effectType)
            {
                case PostProcessingEffectType.Bloom:
                    if (bloom != null)
                        bloom.enabled.value = enabled;
                    break;

                case PostProcessingEffectType.Vignette:
                    if (vignette != null)
                        vignette.enabled.value = enabled;
                    break;

                case PostProcessingEffectType.ColorGrading:
                    if (colorGrading != null)
                        colorGrading.enabled.value = enabled;
                    break;

                case PostProcessingEffectType.ChromaticAberration:
                    if (chromaticAberration != null)
                        chromaticAberration.enabled.value = enabled;
                    break;

                case PostProcessingEffectType.DepthOfField:
                    if (depthOfField != null)
                        depthOfField.enabled.value = enabled;
                    break;

                case PostProcessingEffectType.All:
                    if (volume != null)
                        volume.enabled = enabled;
                    break;
            }
#endif
        }

        /// <summary>
        /// Set post-processing enabled
        /// </summary>
        public void SetPostProcessingEnabled(bool enabled)
        {
            postProcessingEnabled = enabled;

#if UNITY_POST_PROCESSING_STACK_V2
            if (volume != null)
            {
                volume.enabled = enabled;
            }
#endif
        }

        /// <summary>
        /// Get current preset name
        /// </summary>
        public string GetCurrentPresetName()
        {
            return currentPreset != null ? currentPreset.presetName : "None";
        }
    }
}
