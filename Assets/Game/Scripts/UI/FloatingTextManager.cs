using UnityEngine;
using TMPro;

namespace CozyGame.UI
{
    /// <summary>
    /// Shows floating text when picking up items, completing quests, etc.
    /// Example: "+5 Wood" floats up and fades away
    /// Singleton pattern for easy access
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [Header("Prefab Reference")]
        [Tooltip("Prefab with TextMeshProUGUI component for floating text")]
        public GameObject floatingTextPrefab;

        [Header("Animation Settings")]
        [Tooltip("Speed at which text floats upward")]
        public float floatSpeed = 1f;

        [Tooltip("Speed at which text fades out")]
        public float fadeSpeed = 1f;

        [Tooltip("How long text stays visible (seconds)")]
        public float lifetime = 2f;

        [Tooltip("Random horizontal spread")]
        public float horizontalSpread = 0.5f;

        [Header("Default Colors")]
        public Color defaultColor = Color.white;
        public Color positiveColor = Color.green; // For gains
        public Color negativeColor = Color.red;   // For losses
        public Color rareColor = new Color(1f, 0.5f, 0f); // Orange for rare items

        [Header("Object Pool")]
        [Tooltip("Pre-create this many floating texts for performance")]
        public int poolSize = 10;

        private Transform poolParent;
        private System.Collections.Generic.Queue<GameObject> textPool = new System.Collections.Generic.Queue<GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePool()
        {
            // Create pool parent
            poolParent = new GameObject("FloatingText_Pool").transform;
            poolParent.SetParent(transform);

            // Create initial pool if prefab is assigned
            if (floatingTextPrefab != null)
            {
                for (int i = 0; i < poolSize; i++)
                {
                    CreatePooledText();
                }
            }
        }

        private GameObject CreatePooledText()
        {
            GameObject textObj = Instantiate(floatingTextPrefab, poolParent);
            textObj.SetActive(false);
            textPool.Enqueue(textObj);
            return textObj;
        }

        private GameObject GetPooledText()
        {
            if (textPool.Count > 0)
            {
                GameObject textObj = textPool.Dequeue();
                textObj.SetActive(true);
                return textObj;
            }
            else
            {
                // Pool exhausted, create new one
                return CreatePooledText();
            }
        }

        private void ReturnToPool(GameObject textObj)
        {
            textObj.SetActive(false);
            textObj.transform.SetParent(poolParent);
            textPool.Enqueue(textObj);
        }

        /// <summary>
        /// Show floating text at world position
        /// </summary>
        public void Show(string text, Vector3 worldPosition, Color? color = null)
        {
            if (floatingTextPrefab == null)
            {
                Debug.LogWarning("[FloatingTextManager] Floating text prefab not assigned!");
                return;
            }

            // Get text from pool
            GameObject textObj = GetPooledText();

            // Set world position with slight random offset
            Vector3 randomOffset = new Vector3(
                Random.Range(-horizontalSpread, horizontalSpread),
                0f,
                Random.Range(-horizontalSpread, horizontalSpread)
            );
            textObj.transform.position = worldPosition + randomOffset;

            // Set text and color
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = color ?? defaultColor;
            }

            // Animate
            StartCoroutine(AnimateFloatingText(textObj, textComponent));
        }

        /// <summary>
        /// Show floating text for item pickup (automatically colored)
        /// </summary>
        public void ShowItemPickup(string itemName, int quantity, Vector3 worldPosition, bool isRare = false)
        {
            string text = quantity > 1 ? $"+{quantity} {itemName}" : $"+{itemName}";
            Color color = isRare ? rareColor : positiveColor;
            Show(text, worldPosition, color);
        }

        /// <summary>
        /// Show floating text for resource loss
        /// </summary>
        public void ShowItemLoss(string itemName, int quantity, Vector3 worldPosition)
        {
            string text = quantity > 1 ? $"-{quantity} {itemName}" : $"-{itemName}";
            Show(text, worldPosition, negativeColor);
        }

        /// <summary>
        /// Show floating text for quest/riddle completion
        /// </summary>
        public void ShowCompletion(string message, Vector3 worldPosition)
        {
            Show(message, worldPosition, rareColor);
        }

        private System.Collections.IEnumerator AnimateFloatingText(GameObject textObj, TextMeshProUGUI textComponent)
        {
            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Color startColor = textComponent != null ? textComponent.color : defaultColor;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / lifetime;

                // Float upward
                if (textObj != null)
                {
                    textObj.transform.position = startPos + Vector3.up * (floatSpeed * elapsed);
                }

                // Fade out
                if (textComponent != null)
                {
                    Color color = startColor;
                    color.a = 1f - progress;
                    textComponent.color = color;
                }

                yield return null;
            }

            // Return to pool
            if (textObj != null)
            {
                ReturnToPool(textObj);
            }
        }

        /// <summary>
        /// Show floating text at screen position (UI space)
        /// </summary>
        public void ShowAtScreenPosition(string text, Vector2 screenPosition, Color? color = null)
        {
            if (floatingTextPrefab == null || Camera.main == null)
                return;

            // Convert screen position to world position
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            Show(text, worldPosition, color);
        }

        /// <summary>
        /// Clear all active floating texts
        /// </summary>
        public void ClearAll()
        {
            StopAllCoroutines();

            // Return all active texts to pool
            foreach (Transform child in poolParent)
            {
                if (child.gameObject.activeSelf)
                {
                    ReturnToPool(child.gameObject);
                }
            }
        }
    }
}
