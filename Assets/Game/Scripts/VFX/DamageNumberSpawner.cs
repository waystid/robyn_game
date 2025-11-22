using UnityEngine;
using TMPro;
using System.Collections.Generic;
using CozyGame.Combat;

namespace CozyGame.VFX
{
    /// <summary>
    /// Floating damage number
    /// </summary>
    public class FloatingDamageNumber : MonoBehaviour
    {
        public TextMeshPro textMesh;
        public float lifetime = 1.5f;
        public float riseSpeed = 2f;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private float timer = 0f;
        private Vector3 startPosition;

        private void OnEnable()
        {
            timer = 0f;
            startPosition = transform.position;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            // Rise upward
            transform.position = startPosition + Vector3.up * (riseSpeed * timer);

            // Fade out
            if (textMesh != null)
            {
                float alpha = fadeCurve.Evaluate(timer / lifetime);
                Color color = textMesh.color;
                color.a = alpha;
                textMesh.color = color;
            }

            // Destroy after lifetime
            if (timer >= lifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Damage number spawner singleton.
    /// Spawns floating damage numbers above damaged entities.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [Header("Prefab")]
        [Tooltip("Damage number prefab")]
        public GameObject damageNumberPrefab;

        [Header("Settings")]
        [Tooltip("Enable damage numbers")]
        public bool enableDamageNumbers = true;

        [Tooltip("Height offset above hit point")]
        public float heightOffset = 1.5f;

        [Tooltip("Random horizontal spread")]
        public float horizontalSpread = 0.5f;

        [Header("Colors")]
        [Tooltip("Normal damage color")]
        public Color normalDamageColor = Color.white;

        [Tooltip("Critical damage color")]
        public Color criticalDamageColor = Color.yellow;

        [Tooltip("Heal color")]
        public Color healColor = Color.green;

        [Tooltip("Player damage color")]
        public Color playerDamageColor = new Color(1f, 0.5f, 0f); // Orange

        [Header("Font Size")]
        [Tooltip("Normal damage font size")]
        public float normalFontSize = 4f;

        [Tooltip("Critical damage font size")]
        public float criticalFontSize = 6f;

        [Header("Pooling")]
        [Tooltip("Enable pooling")]
        public bool enablePooling = true;

        [Tooltip("Pool size")]
        public int poolSize = 20;

        private Queue<GameObject> damageNumberPool = new Queue<GameObject>();

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
        /// Initialize damage number spawner
        /// </summary>
        private void Initialize()
        {
            if (enablePooling && damageNumberPrefab != null)
            {
                for (int i = 0; i < poolSize; i++)
                {
                    GameObject obj = Instantiate(damageNumberPrefab, transform);
                    obj.SetActive(false);
                    damageNumberPool.Enqueue(obj);
                }
            }

            Debug.Log("[DamageNumberSpawner] Initialized");
        }

        /// <summary>
        /// Spawn damage number from DamageInfo
        /// </summary>
        public void SpawnDamageNumber(DamageInfo damageInfo)
        {
            if (!enableDamageNumbers || damageInfo == null)
                return;

            Vector3 position = damageInfo.hitPoint + Vector3.up * heightOffset;
            float damage = damageInfo.amount;
            bool isCritical = damageInfo.isCritical;

            // Determine color based on damage type and target
            Color color = normalDamageColor;

            if (isCritical)
            {
                color = criticalDamageColor;
            }
            else if (damageInfo.victim != null && damageInfo.victim.CompareTag("Player"))
            {
                color = playerDamageColor;
            }

            SpawnNumber(damage, position, color, isCritical);
        }

        /// <summary>
        /// Spawn healing number
        /// </summary>
        public void SpawnHealNumber(float healAmount, Vector3 position)
        {
            if (!enableDamageNumbers)
                return;

            position += Vector3.up * heightOffset;
            SpawnNumber(healAmount, position, healColor, false, "+");
        }

        /// <summary>
        /// Spawn generic number
        /// </summary>
        public void SpawnNumber(float value, Vector3 position, Color color, bool isCritical = false, string prefix = "")
        {
            if (!enableDamageNumbers || damageNumberPrefab == null)
                return;

            GameObject numberObj = null;

            // Get from pool or instantiate
            if (enablePooling && damageNumberPool.Count > 0)
            {
                numberObj = damageNumberPool.Dequeue();
            }
            else
            {
                numberObj = Instantiate(damageNumberPrefab);
            }

            if (numberObj == null)
                return;

            // Add random horizontal offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-horizontalSpread, horizontalSpread),
                0f,
                Random.Range(-horizontalSpread, horizontalSpread)
            );

            numberObj.transform.position = position + randomOffset;
            numberObj.SetActive(true);

            // Setup text
            FloatingDamageNumber floatingNumber = numberObj.GetComponent<FloatingDamageNumber>();
            if (floatingNumber != null && floatingNumber.textMesh != null)
            {
                string text = prefix + Mathf.RoundToInt(value).ToString();
                floatingNumber.textMesh.text = text;
                floatingNumber.textMesh.color = color;

                // Set font size
                float fontSize = isCritical ? criticalFontSize : normalFontSize;
                floatingNumber.textMesh.fontSize = fontSize;

                // Make critical numbers bigger and bold
                if (isCritical)
                {
                    floatingNumber.textMesh.fontStyle = FontStyles.Bold;
                }
                else
                {
                    floatingNumber.textMesh.fontStyle = FontStyles.Normal;
                }
            }

            // Return to pool after lifetime
            if (enablePooling && floatingNumber != null)
            {
                StartCoroutine(ReturnToPoolAfterDelay(numberObj, floatingNumber.lifetime));
            }
        }

        /// <summary>
        /// Return to pool after delay
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (obj != null && enablePooling)
            {
                obj.transform.SetParent(transform);
                obj.SetActive(false);
                damageNumberPool.Enqueue(obj);
            }
        }
    }
}
