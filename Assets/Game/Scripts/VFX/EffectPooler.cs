using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.VFX
{
    /// <summary>
    /// Object pool for a specific prefab
    /// </summary>
    [System.Serializable]
    public class EffectPool
    {
        public string poolName;
        public GameObject prefab;
        public int poolSize = 10;
        public bool expandable = true;
        public int maxSize = 50;

        [System.NonSerialized]
        public Queue<GameObject> availableObjects = new Queue<GameObject>();

        [System.NonSerialized]
        public List<GameObject> allObjects = new List<GameObject>();

        [System.NonSerialized]
        public Transform poolParent;
    }

    /// <summary>
    /// Effect pooler for particle systems and VFX.
    /// Manages object pools to improve performance.
    /// </summary>
    public class EffectPooler : MonoBehaviour
    {
        [Header("Pools")]
        [Tooltip("Effect pools")]
        public List<EffectPool> pools = new List<EffectPool>();

        [Header("Settings")]
        [Tooltip("Warm up pools on start")]
        public bool warmUpOnStart = true;

        [Tooltip("Pool container parent")]
        public Transform poolContainer;

        // Lookup
        private Dictionary<string, EffectPool> poolLookup = new Dictionary<string, EffectPool>();

        private void Start()
        {
            // Create pool container
            if (poolContainer == null)
            {
                GameObject container = new GameObject("EffectPools");
                container.transform.SetParent(transform);
                poolContainer = container.transform;
            }

            // Initialize pools
            foreach (var pool in pools)
            {
                CreatePool(pool.poolName, pool.prefab, pool.poolSize);
            }

            // Warm up
            if (warmUpOnStart)
            {
                WarmUpPools();
            }
        }

        /// <summary>
        /// Create a new pool
        /// </summary>
        public void CreatePool(string poolName, GameObject prefab, int size)
        {
            if (string.IsNullOrEmpty(poolName) || prefab == null)
            {
                Debug.LogWarning("[EffectPooler] Invalid pool name or prefab");
                return;
            }

            // Check if pool already exists
            if (poolLookup.ContainsKey(poolName))
            {
                Debug.LogWarning($"[EffectPooler] Pool '{poolName}' already exists");
                return;
            }

            // Create pool
            EffectPool pool = pools.Find(p => p.poolName == poolName);
            if (pool == null)
            {
                pool = new EffectPool
                {
                    poolName = poolName,
                    prefab = prefab,
                    poolSize = size
                };
                pools.Add(pool);
            }

            // Create pool parent
            GameObject poolParentObj = new GameObject($"Pool_{poolName}");
            poolParentObj.transform.SetParent(poolContainer);
            pool.poolParent = poolParentObj.transform;

            // Pre-instantiate objects
            for (int i = 0; i < size; i++)
            {
                GameObject obj = CreatePooledObject(pool);
                obj.SetActive(false);
                pool.availableObjects.Enqueue(obj);
            }

            // Add to lookup
            poolLookup[poolName] = pool;
        }

        /// <summary>
        /// Get object from pool
        /// </summary>
        public GameObject GetFromPool(string poolName)
        {
            if (!poolLookup.ContainsKey(poolName))
            {
                Debug.LogWarning($"[EffectPooler] Pool '{poolName}' not found");
                return null;
            }

            EffectPool pool = poolLookup[poolName];

            // Get from available objects
            if (pool.availableObjects.Count > 0)
            {
                GameObject obj = pool.availableObjects.Dequeue();
                obj.SetActive(true);

                // Reset particle system
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Clear();
                    ps.time = 0f;
                }

                return obj;
            }

            // Expand pool if allowed
            if (pool.expandable && pool.allObjects.Count < pool.maxSize)
            {
                GameObject obj = CreatePooledObject(pool);
                obj.SetActive(true);
                return obj;
            }

            // Reuse oldest object
            if (pool.allObjects.Count > 0)
            {
                GameObject obj = pool.allObjects[0];
                obj.SetActive(false);
                obj.SetActive(true);

                // Reset particle system
                ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Clear();
                    ps.time = 0f;
                }

                return obj;
            }

            return null;
        }

        /// <summary>
        /// Return object to pool
        /// </summary>
        public void ReturnToPool(string poolName, GameObject obj)
        {
            if (obj == null)
                return;

            if (!poolLookup.ContainsKey(poolName))
            {
                Debug.LogWarning($"[EffectPooler] Pool '{poolName}' not found");
                Destroy(obj);
                return;
            }

            EffectPool pool = poolLookup[poolName];

            // Stop particle system
            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop();
                ps.Clear();
            }

            // Deactivate and return to pool
            obj.SetActive(false);
            obj.transform.SetParent(pool.poolParent);
            obj.transform.localPosition = Vector3.zero;

            // Add to available queue if not already there
            if (!pool.availableObjects.Contains(obj))
            {
                pool.availableObjects.Enqueue(obj);
            }
        }

        /// <summary>
        /// Create pooled object
        /// </summary>
        private GameObject CreatePooledObject(EffectPool pool)
        {
            GameObject obj = Instantiate(pool.prefab, pool.poolParent);
            obj.name = $"{pool.poolName}_{pool.allObjects.Count}";
            pool.allObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// Warm up pools (pre-instantiate and prepare)
        /// </summary>
        public void WarmUpPools()
        {
            foreach (var pool in pools)
            {
                // Get and return each object to initialize particle systems
                for (int i = 0; i < pool.poolSize; i++)
                {
                    if (pool.availableObjects.Count > 0)
                    {
                        GameObject obj = pool.availableObjects.Dequeue();
                        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
                        if (ps != null)
                        {
                            ps.Simulate(0.1f);
                            ps.Stop();
                            ps.Clear();
                        }
                        pool.availableObjects.Enqueue(obj);
                    }
                }
            }
        }

        /// <summary>
        /// Clear pool
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (!poolLookup.ContainsKey(poolName))
                return;

            EffectPool pool = poolLookup[poolName];

            // Destroy all objects
            foreach (var obj in pool.allObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            pool.allObjects.Clear();
            pool.availableObjects.Clear();

            // Destroy parent
            if (pool.poolParent != null)
            {
                Destroy(pool.poolParent.gameObject);
            }

            // Remove from lookup
            poolLookup.Remove(poolName);
            pools.Remove(pool);
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in new List<EffectPool>(pools))
            {
                ClearPool(pool.poolName);
            }
        }

        /// <summary>
        /// Get pool stats
        /// </summary>
        public void GetPoolStats(string poolName, out int total, out int available, out int active)
        {
            total = 0;
            available = 0;
            active = 0;

            if (poolLookup.ContainsKey(poolName))
            {
                EffectPool pool = poolLookup[poolName];
                total = pool.allObjects.Count;
                available = pool.availableObjects.Count;
                active = total - available;
            }
        }

        /// <summary>
        /// Log pool statistics
        /// </summary>
        public void LogPoolStats()
        {
            Debug.Log("=== Effect Pool Statistics ===");

            foreach (var pool in pools)
            {
                int total = pool.allObjects.Count;
                int available = pool.availableObjects.Count;
                int active = total - available;

                Debug.Log($"Pool '{pool.poolName}': Total={total}, Active={active}, Available={available}");
            }
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }
}
