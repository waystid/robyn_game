using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.VFX
{
    /// <summary>
    /// Particle effect type categories
    /// </summary>
    public enum EffectType
    {
        SpellCast,
        SpellHit,
        Heal,
        Damage,
        LevelUp,
        PlantGrowth,
        PlantHarvest,
        QuestComplete,
        ItemPickup,
        Death,
        Explosion,
        Sparkle,
        Smoke,
        Fire,
        Water,
        Custom
    }

    /// <summary>
    /// Particle effect definition
    /// </summary>
    [System.Serializable]
    public class ParticleEffectDefinition
    {
        public EffectType effectType;
        public GameObject prefab;
        public float lifetime = 2f;
        public bool pooled = true;
        public int poolSize = 10;
    }

    /// <summary>
    /// Particle effect manager singleton.
    /// Centralized system for spawning and pooling particle effects.
    /// </summary>
    public class ParticleEffectManager : MonoBehaviour
    {
        public static ParticleEffectManager Instance { get; private set; }

        [Header("Effect Definitions")]
        [Tooltip("Particle effect prefabs")]
        public ParticleEffectDefinition[] effectDefinitions;

        [Header("Pooling Settings")]
        [Tooltip("Enable object pooling")]
        public bool enablePooling = true;

        [Tooltip("Auto-expand pools")]
        public bool autoExpandPools = true;

        [Header("Performance")]
        [Tooltip("Max active particles")]
        public int maxActiveParticles = 100;

        [Tooltip("Cull distance (0 = no culling)")]
        public float cullDistance = 50f;

        // Pooling
        private Dictionary<EffectType, Queue<GameObject>> effectPools = new Dictionary<EffectType, Queue<GameObject>>();
        private Dictionary<EffectType, GameObject> effectPrefabs = new Dictionary<EffectType, GameObject>();
        private Dictionary<EffectType, float> effectLifetimes = new Dictionary<EffectType, float>();

        // Active effects
        private List<GameObject> activeEffects = new List<GameObject>();

        // Camera reference
        private Camera mainCamera;

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
        /// Initialize particle effect manager
        /// </summary>
        private void Initialize()
        {
            mainCamera = Camera.main;

            // Build lookup dictionaries and create pools
            foreach (var def in effectDefinitions)
            {
                if (def.prefab != null)
                {
                    effectPrefabs[def.effectType] = def.prefab;
                    effectLifetimes[def.effectType] = def.lifetime;

                    if (enablePooling && def.pooled)
                    {
                        CreatePool(def.effectType, def.poolSize);
                    }
                }
            }

            Debug.Log($"[ParticleEffectManager] Initialized with {effectDefinitions.Length} effect types");
        }

        /// <summary>
        /// Create object pool for effect type
        /// </summary>
        private void CreatePool(EffectType effectType, int poolSize)
        {
            if (effectPools.ContainsKey(effectType))
                return;

            Queue<GameObject> pool = new Queue<GameObject>();

            if (!effectPrefabs.TryGetValue(effectType, out GameObject prefab))
                return;

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            effectPools[effectType] = pool;
        }

        /// <summary>
        /// Spawn particle effect
        /// </summary>
        public GameObject SpawnEffect(EffectType effectType, Vector3 position, Quaternion rotation)
        {
            // Check active particle limit
            if (activeEffects.Count >= maxActiveParticles)
            {
                Debug.LogWarning("[ParticleEffectManager] Max active particles reached!");
                return null;
            }

            // Check distance culling
            if (cullDistance > 0 && mainCamera != null)
            {
                float distance = Vector3.Distance(position, mainCamera.transform.position);
                if (distance > cullDistance)
                    return null;
            }

            GameObject effect = null;

            // Try to get from pool
            if (enablePooling && effectPools.ContainsKey(effectType))
            {
                Queue<GameObject> pool = effectPools[effectType];

                if (pool.Count > 0)
                {
                    effect = pool.Dequeue();
                }
                else if (autoExpandPools && effectPrefabs.TryGetValue(effectType, out GameObject prefab))
                {
                    effect = Instantiate(prefab, transform);
                }
            }
            else if (effectPrefabs.TryGetValue(effectType, out GameObject prefab))
            {
                effect = Instantiate(prefab);
            }

            if (effect == null)
            {
                Debug.LogWarning($"[ParticleEffectManager] Failed to spawn effect: {effectType}");
                return null;
            }

            // Setup effect
            effect.transform.position = position;
            effect.transform.rotation = rotation;
            effect.SetActive(true);

            // Play particle system
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }

            // Track active effect
            activeEffects.Add(effect);

            // Auto-return to pool after lifetime
            if (effectLifetimes.TryGetValue(effectType, out float lifetime))
            {
                StartCoroutine(ReturnToPoolAfterDelay(effect, effectType, lifetime));
            }

            return effect;
        }

        /// <summary>
        /// Spawn effect at position
        /// </summary>
        public GameObject SpawnEffect(EffectType effectType, Vector3 position)
        {
            return SpawnEffect(effectType, position, Quaternion.identity);
        }

        /// <summary>
        /// Spawn effect attached to transform
        /// </summary>
        public GameObject SpawnEffectAttached(EffectType effectType, Transform parent, Vector3 localPosition)
        {
            GameObject effect = SpawnEffect(effectType, parent.position + localPosition, parent.rotation);

            if (effect != null)
            {
                effect.transform.SetParent(parent);
                effect.transform.localPosition = localPosition;
            }

            return effect;
        }

        /// <summary>
        /// Return effect to pool after delay
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject effect, EffectType effectType, float delay)
        {
            yield return new WaitForSeconds(delay);

            ReturnToPool(effect, effectType);
        }

        /// <summary>
        /// Return effect to pool
        /// </summary>
        private void ReturnToPool(GameObject effect, EffectType effectType)
        {
            if (effect == null)
                return;

            // Remove from active list
            activeEffects.Remove(effect);

            // Return to pool or destroy
            if (enablePooling && effectPools.ContainsKey(effectType))
            {
                effect.transform.SetParent(transform);
                effect.SetActive(false);
                effectPools[effectType].Enqueue(effect);
            }
            else
            {
                Destroy(effect);
            }
        }

        /// <summary>
        /// Stop and return effect immediately
        /// </summary>
        public void StopEffect(GameObject effect, EffectType effectType)
        {
            if (effect == null)
                return;

            // Stop particle system
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
            }

            ReturnToPool(effect, effectType);
        }

        /// <summary>
        /// Clear all active effects
        /// </summary>
        public void ClearAllEffects()
        {
            foreach (var effect in activeEffects.ToArray())
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }

            activeEffects.Clear();
        }

        /// <summary>
        /// Get active effect count
        /// </summary>
        public int GetActiveEffectCount()
        {
            return activeEffects.Count;
        }

        private void Update()
        {
            // Update camera reference
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
    }
}
