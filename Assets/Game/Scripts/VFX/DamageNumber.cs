using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyGame.VFX
{
    /// <summary>
    /// Damage number type
    /// </summary>
    public enum DamageNumberType
    {
        Damage,
        CriticalDamage,
        Heal,
        CriticalHeal,
        Mana,
        Experience,
        Currency,
        Miss,
        Block,
        Custom
    }

    /// <summary>
    /// Floating damage/heal number display.
    /// Shows color-coded numbers that float upward.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Text component")]
        public TextMeshProUGUI text;

        [Header("Movement")]
        [Tooltip("Float speed")]
        public float floatSpeed = 2f;

        [Tooltip("Float direction")]
        public Vector3 floatDirection = Vector3.up;

        [Tooltip("Random spread")]
        public float randomSpread = 0.5f;

        [Tooltip("Lifetime")]
        public float lifetime = 1.5f;

        [Header("Animation")]
        [Tooltip("Scale animation")]
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Max scale")]
        public float maxScale = 1.5f;

        [Tooltip("Fade out duration")]
        public float fadeOutDuration = 0.5f;

        // State
        private Vector3 velocity;
        private float age;
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private Camera mainCamera;
        private Vector3 worldPosition;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            mainCamera = Camera.main;

            // Random spread
            Vector3 spread = new Vector3(
                Random.Range(-randomSpread, randomSpread),
                Random.Range(-randomSpread * 0.5f, randomSpread),
                0f
            );

            velocity = floatDirection + spread;

            // Destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            age += Time.deltaTime;

            // Float upward
            worldPosition += velocity * floatSpeed * Time.deltaTime;

            // Update position (world to screen)
            UpdatePosition();

            // Update scale
            float scaleT = Mathf.Clamp01(age / (lifetime * 0.3f));
            float scale = scaleCurve.Evaluate(scaleT) * maxScale;
            transform.localScale = Vector3.one * scale;

            // Fade out
            if (age > lifetime - fadeOutDuration)
            {
                float fadeT = 1f - ((age - (lifetime - fadeOutDuration)) / fadeOutDuration);
                canvasGroup.alpha = fadeT;
            }

            // Slow down over time
            velocity *= 0.98f;
        }

        /// <summary>
        /// Initialize damage number
        /// </summary>
        public void Initialize(float value, DamageNumberType type, Vector3 worldPos)
        {
            worldPosition = worldPos;

            // Set text
            string displayText = GetFormattedText(value, type);
            if (text != null)
            {
                text.text = displayText;
            }

            // Set color
            Color color = GetColorForType(type);
            if (text != null)
            {
                text.color = color;
            }

            // Set font size for critical
            if (type == DamageNumberType.CriticalDamage || type == DamageNumberType.CriticalHeal)
            {
                if (text != null)
                {
                    text.fontSize *= 1.5f;
                }
                maxScale = 2f;
            }

            // Update initial position
            UpdatePosition();
        }

        /// <summary>
        /// Update screen position from world position
        /// </summary>
        private void UpdatePosition()
        {
            if (mainCamera == null || rectTransform == null)
                return;

            // Convert world to screen position
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);

            // Set position
            rectTransform.position = screenPos;
        }

        /// <summary>
        /// Get formatted text for value
        /// </summary>
        private string GetFormattedText(float value, DamageNumberType type)
        {
            switch (type)
            {
                case DamageNumberType.Damage:
                    return $"-{Mathf.RoundToInt(value)}";

                case DamageNumberType.CriticalDamage:
                    return $"-{Mathf.RoundToInt(value)}!";

                case DamageNumberType.Heal:
                    return $"+{Mathf.RoundToInt(value)}";

                case DamageNumberType.CriticalHeal:
                    return $"+{Mathf.RoundToInt(value)}!";

                case DamageNumberType.Mana:
                    return $"+{Mathf.RoundToInt(value)} MP";

                case DamageNumberType.Experience:
                    return $"+{Mathf.RoundToInt(value)} XP";

                case DamageNumberType.Currency:
                    return $"+{Mathf.RoundToInt(value)}g";

                case DamageNumberType.Miss:
                    return "MISS";

                case DamageNumberType.Block:
                    return "BLOCK";

                default:
                    return Mathf.RoundToInt(value).ToString();
            }
        }

        /// <summary>
        /// Get color for damage number type
        /// </summary>
        private Color GetColorForType(DamageNumberType type)
        {
            switch (type)
            {
                case DamageNumberType.Damage:
                    return new Color(1f, 0.3f, 0.3f); // Red

                case DamageNumberType.CriticalDamage:
                    return new Color(1f, 0.5f, 0f); // Orange-red

                case DamageNumberType.Heal:
                    return new Color(0.3f, 1f, 0.3f); // Green

                case DamageNumberType.CriticalHeal:
                    return new Color(0.3f, 1f, 0.8f); // Bright green

                case DamageNumberType.Mana:
                    return new Color(0.3f, 0.5f, 1f); // Blue

                case DamageNumberType.Experience:
                    return new Color(1f, 0.9f, 0.3f); // Yellow

                case DamageNumberType.Currency:
                    return new Color(1f, 0.84f, 0f); // Gold

                case DamageNumberType.Miss:
                    return new Color(0.7f, 0.7f, 0.7f); // Gray

                case DamageNumberType.Block:
                    return new Color(0.5f, 0.7f, 1f); // Light blue

                default:
                    return Color.white;
            }
        }
    }

    /// <summary>
    /// Damage number spawner singleton.
    /// Spawns floating damage/heal numbers in the world.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [Header("Prefab")]
        [Tooltip("Damage number prefab")]
        public GameObject damageNumberPrefab;

        [Header("Settings")]
        [Tooltip("Enable damage numbers")]
        public bool damageNumbersEnabled = true;

        [Tooltip("Offset above target")]
        public Vector3 spawnOffset = Vector3.up * 2f;

        [Header("Canvas")]
        [Tooltip("World space canvas for damage numbers")]
        public Canvas worldCanvas;

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

            // Create world canvas if not assigned
            if (worldCanvas == null)
            {
                CreateWorldCanvas();
            }
        }

        /// <summary>
        /// Create world canvas for damage numbers
        /// </summary>
        private void CreateWorldCanvas()
        {
            GameObject canvasObj = new GameObject("DamageNumberCanvas");
            canvasObj.transform.SetParent(transform);

            worldCanvas = canvasObj.AddComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            worldCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        /// <summary>
        /// Spawn damage number
        /// </summary>
        public void SpawnDamageNumber(float value, DamageNumberType type, Vector3 worldPosition)
        {
            if (!damageNumbersEnabled || damageNumberPrefab == null || worldCanvas == null)
                return;

            // Instantiate
            GameObject numberObj = Instantiate(damageNumberPrefab, worldCanvas.transform);

            // Initialize
            DamageNumber damageNumber = numberObj.GetComponent<DamageNumber>();
            if (damageNumber != null)
            {
                damageNumber.Initialize(value, type, worldPosition + spawnOffset);
            }
        }

        /// <summary>
        /// Spawn damage number on target
        /// </summary>
        public void SpawnDamageNumber(float value, DamageNumberType type, Transform target)
        {
            if (target != null)
            {
                SpawnDamageNumber(value, type, target.position);
            }
        }

        /// <summary>
        /// Spawn damage
        /// </summary>
        public void SpawnDamage(float damage, Vector3 worldPosition, bool isCritical = false)
        {
            DamageNumberType type = isCritical ? DamageNumberType.CriticalDamage : DamageNumberType.Damage;
            SpawnDamageNumber(damage, type, worldPosition);
        }

        /// <summary>
        /// Spawn heal
        /// </summary>
        public void SpawnHeal(float heal, Vector3 worldPosition, bool isCritical = false)
        {
            DamageNumberType type = isCritical ? DamageNumberType.CriticalHeal : DamageNumberType.Heal;
            SpawnDamageNumber(heal, type, worldPosition);
        }

        /// <summary>
        /// Spawn experience
        /// </summary>
        public void SpawnExperience(float exp, Vector3 worldPosition)
        {
            SpawnDamageNumber(exp, DamageNumberType.Experience, worldPosition);
        }

        /// <summary>
        /// Spawn currency
        /// </summary>
        public void SpawnCurrency(float amount, Vector3 worldPosition)
        {
            SpawnDamageNumber(amount, DamageNumberType.Currency, worldPosition);
        }

        /// <summary>
        /// Spawn miss
        /// </summary>
        public void SpawnMiss(Vector3 worldPosition)
        {
            SpawnDamageNumber(0, DamageNumberType.Miss, worldPosition);
        }

        /// <summary>
        /// Spawn block
        /// </summary>
        public void SpawnBlock(Vector3 worldPosition)
        {
            SpawnDamageNumber(0, DamageNumberType.Block, worldPosition);
        }

        /// <summary>
        /// Set damage numbers enabled
        /// </summary>
        public void SetDamageNumbersEnabled(bool enabled)
        {
            damageNumbersEnabled = enabled;
        }
    }
}
