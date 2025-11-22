using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.VFX
{
    /// <summary>
    /// VFX effect type
    /// </summary>
    public enum VFXType
    {
        // Spell effects
        FireCast,
        IceCast,
        LightningCast,
        HealingCast,
        WindCast,
        EarthCast,
        DarkCast,
        LightCast,

        // Plant/farming effects
        PlantGrowth,
        PlantWither,
        Watering,
        Fertilizing,

        // Harvest effects
        CropHarvest,
        TreeChop,
        RockMine,
        FishCatch,

        // Player effects
        LevelUp,
        SkillUnlock,
        AchievementUnlock,
        QuestComplete,

        // Combat effects
        Hit,
        CriticalHit,
        Block,
        Dodge,
        Death,

        // Environmental effects
        Fireflies,
        FallingLeaves,
        Dust,
        Rain,
        Snow,
        Sparkle,

        // UI effects
        Collect,
        Purchase,
        Upgrade,

        Custom
    }

    /// <summary>
    /// VFX configuration data
    /// </summary>
    [System.Serializable]
    public class VFXConfig
    {
        public VFXType type;
        public GameObject prefab;
        public float lifetime = 2f;
        public bool usePooling = true;
        public int poolSize = 10;
        public AudioClip sound;
        [Range(0f, 1f)]
        public float soundVolume = 1f;
        public bool attachToTarget = false;
        public Vector3 offset = Vector3.zero;
        public bool worldSpace = true;
    }

    /// <summary>
    /// VFX Manager singleton.
    /// Manages particle effects and visual feedback throughout the game.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("VFX Library")]
        [Tooltip("VFX effect configurations")]
        public List<VFXConfig> vfxConfigs = new List<VFXConfig>();

        [Header("Settings")]
        [Tooltip("Enable VFX")]
        public bool vfxEnabled = true;

        [Tooltip("VFX quality (affects particle count)")]
        [Range(0.1f, 2f)]
        public float qualityMultiplier = 1f;

        [Tooltip("Max active effects")]
        public int maxActiveEffects = 50;

        [Header("Default Prefabs")]
        [Tooltip("Default sparkle effect")]
        public GameObject defaultSparkle;

        [Tooltip("Default explosion effect")]
        public GameObject defaultExplosion;

        [Tooltip("Default magic effect")]
        public GameObject defaultMagic;

        // State
        private Dictionary<VFXType, VFXConfig> vfxLookup = new Dictionary<VFXType, VFXConfig>();
        private List<GameObject> activeEffects = new List<GameObject>();
        private EffectPooler pooler;

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

            // Build lookup
            BuildVFXLookup();

            // Get or create pooler
            pooler = GetComponent<EffectPooler>();
            if (pooler == null)
            {
                pooler = gameObject.AddComponent<EffectPooler>();
            }
        }

        private void Start()
        {
            // Initialize pools
            if (pooler != null)
            {
                foreach (var config in vfxConfigs)
                {
                    if (config.usePooling && config.prefab != null)
                    {
                        pooler.CreatePool(config.type.ToString(), config.prefab, config.poolSize);
                    }
                }
            }
        }

        /// <summary>
        /// Build VFX lookup dictionary
        /// </summary>
        private void BuildVFXLookup()
        {
            vfxLookup.Clear();

            foreach (var config in vfxConfigs)
            {
                if (!vfxLookup.ContainsKey(config.type))
                {
                    vfxLookup.Add(config.type, config);
                }
                else
                {
                    Debug.LogWarning($"[VFXManager] Duplicate VFX type: {config.type}");
                }
            }
        }

        /// <summary>
        /// Play VFX effect at position
        /// </summary>
        public GameObject PlayVFX(VFXType type, Vector3 position, Quaternion rotation = default)
        {
            if (!vfxEnabled)
                return null;

            // Check max effects
            if (activeEffects.Count >= maxActiveEffects)
            {
                // Remove oldest
                if (activeEffects.Count > 0 && activeEffects[0] != null)
                {
                    Destroy(activeEffects[0]);
                    activeEffects.RemoveAt(0);
                }
            }

            // Get config
            VFXConfig config = GetConfig(type);
            if (config == null || config.prefab == null)
            {
                Debug.LogWarning($"[VFXManager] No config for VFX: {type}");
                return null;
            }

            // Spawn effect
            GameObject effect = null;

            if (config.usePooling && pooler != null)
            {
                effect = pooler.GetFromPool(type.ToString());
            }
            else
            {
                effect = Instantiate(config.prefab);
            }

            if (effect == null)
                return null;

            // Position
            if (rotation == default)
            {
                rotation = Quaternion.identity;
            }

            effect.transform.position = position + config.offset;
            effect.transform.rotation = rotation;

            // Play particles
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                // Apply quality multiplier
                var main = ps.main;
                main.startLifetime = main.startLifetime.constant * qualityMultiplier;

                var emission = ps.emission;
                emission.rateOverTime = emission.rateOverTime.constant * qualityMultiplier;

                ps.Play();
            }

            // Play sound
            if (config.sound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundAtPosition(config.sound, position, config.soundVolume);
            }

            // Track active effect
            activeEffects.Add(effect);

            // Auto-destroy or return to pool
            if (config.usePooling && pooler != null)
            {
                StartCoroutine(ReturnToPoolAfterDelay(effect, type.ToString(), config.lifetime));
            }
            else
            {
                Destroy(effect, config.lifetime);
            }

            return effect;
        }

        /// <summary>
        /// Play VFX effect at position with custom lifetime
        /// </summary>
        public GameObject PlayVFX(VFXType type, Vector3 position, float lifetime)
        {
            GameObject effect = PlayVFX(type, position);

            if (effect != null)
            {
                VFXConfig config = GetConfig(type);
                if (config != null)
                {
                    if (config.usePooling && pooler != null)
                    {
                        StopAllCoroutines();
                        StartCoroutine(ReturnToPoolAfterDelay(effect, type.ToString(), lifetime));
                    }
                    else
                    {
                        Destroy(effect, lifetime);
                    }
                }
            }

            return effect;
        }

        /// <summary>
        /// Play VFX attached to target
        /// </summary>
        public GameObject PlayVFXOnTarget(VFXType type, Transform target, Vector3 localOffset = default)
        {
            if (!vfxEnabled || target == null)
                return null;

            GameObject effect = PlayVFX(type, target.position + localOffset);

            if (effect != null)
            {
                effect.transform.SetParent(target);
                effect.transform.localPosition = localOffset;
            }

            return effect;
        }

        /// <summary>
        /// Play spell cast effect
        /// </summary>
        public void PlaySpellCast(string spellName, Vector3 position, Transform caster = null)
        {
            // Determine spell type from name
            VFXType vfxType = VFXType.Custom;

            if (spellName.ToLower().Contains("fire"))
                vfxType = VFXType.FireCast;
            else if (spellName.ToLower().Contains("ice") || spellName.ToLower().Contains("frost"))
                vfxType = VFXType.IceCast;
            else if (spellName.ToLower().Contains("lightning") || spellName.ToLower().Contains("shock"))
                vfxType = VFXType.LightningCast;
            else if (spellName.ToLower().Contains("heal") || spellName.ToLower().Contains("cure"))
                vfxType = VFXType.HealingCast;
            else if (spellName.ToLower().Contains("wind") || spellName.ToLower().Contains("air"))
                vfxType = VFXType.WindCast;
            else if (spellName.ToLower().Contains("earth") || spellName.ToLower().Contains("stone"))
                vfxType = VFXType.EarthCast;
            else if (spellName.ToLower().Contains("dark") || spellName.ToLower().Contains("shadow"))
                vfxType = VFXType.DarkCast;
            else if (spellName.ToLower().Contains("light") || spellName.ToLower().Contains("holy"))
                vfxType = VFXType.LightCast;
            else
                vfxType = VFXType.Custom;

            // Play effect
            if (caster != null)
            {
                PlayVFXOnTarget(vfxType, caster, Vector3.up * 1.5f);
            }
            else
            {
                PlayVFX(vfxType, position);
            }

            // Play default magic effect if no specific effect found
            if (vfxType == VFXType.Custom && defaultMagic != null)
            {
                GameObject effect = Instantiate(defaultMagic, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// Play plant growth effect
        /// </summary>
        public void PlayPlantGrowth(Vector3 position)
        {
            PlayVFX(VFXType.PlantGrowth, position);
            PlayVFX(VFXType.Sparkle, position + Vector3.up * 0.5f);
        }

        /// <summary>
        /// Play harvest effect
        /// </summary>
        public void PlayHarvest(string resourceType, Vector3 position)
        {
            VFXType vfxType = VFXType.CropHarvest;

            if (resourceType.ToLower().Contains("tree") || resourceType.ToLower().Contains("wood"))
                vfxType = VFXType.TreeChop;
            else if (resourceType.ToLower().Contains("rock") || resourceType.ToLower().Contains("ore"))
                vfxType = VFXType.RockMine;
            else if (resourceType.ToLower().Contains("fish"))
                vfxType = VFXType.FishCatch;

            PlayVFX(vfxType, position);
        }

        /// <summary>
        /// Play level up effect
        /// </summary>
        public void PlayLevelUp(Transform target)
        {
            if (target == null)
                return;

            PlayVFXOnTarget(VFXType.LevelUp, target, Vector3.zero);

            // Add sparkles around player
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 1.5f;
                PlayVFX(VFXType.Sparkle, target.position + offset + Vector3.up);
            }
        }

        /// <summary>
        /// Play hit effect
        /// </summary>
        public void PlayHit(Vector3 position, bool isCritical = false)
        {
            if (isCritical)
            {
                PlayVFX(VFXType.CriticalHit, position);
            }
            else
            {
                PlayVFX(VFXType.Hit, position);
            }
        }

        /// <summary>
        /// Play collect effect
        /// </summary>
        public void PlayCollect(Vector3 position)
        {
            PlayVFX(VFXType.Collect, position);

            if (defaultSparkle != null)
            {
                GameObject sparkle = Instantiate(defaultSparkle, position, Quaternion.identity);
                Destroy(sparkle, 1f);
            }
        }

        /// <summary>
        /// Get VFX config
        /// </summary>
        private VFXConfig GetConfig(VFXType type)
        {
            if (vfxLookup.ContainsKey(type))
            {
                return vfxLookup[type];
            }

            return null;
        }

        /// <summary>
        /// Return effect to pool after delay
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject effect, string poolName, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (effect != null && pooler != null)
            {
                pooler.ReturnToPool(poolName, effect);
                activeEffects.Remove(effect);
            }
        }

        /// <summary>
        /// Stop all effects
        /// </summary>
        public void StopAllEffects()
        {
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                {
                    ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        ps.Stop();
                    }

                    Destroy(effect);
                }
            }

            activeEffects.Clear();
        }

        /// <summary>
        /// Set VFX enabled
        /// </summary>
        public void SetVFXEnabled(bool enabled)
        {
            vfxEnabled = enabled;

            if (!enabled)
            {
                StopAllEffects();
            }
        }

        /// <summary>
        /// Set VFX quality
        /// </summary>
        public void SetQuality(float quality)
        {
            qualityMultiplier = Mathf.Clamp(quality, 0.1f, 2f);
        }

        private void OnDestroy()
        {
            StopAllEffects();
        }
    }
}
