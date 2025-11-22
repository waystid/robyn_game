using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CozyGame.Combat;

namespace CozyGame.UI
{
    /// <summary>
    /// Health bar UI component.
    /// Displays health bar for entities.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Health component to track")]
        public Health healthComponent;

        [Tooltip("Health bar fill image")]
        public Image healthBarFill;

        [Tooltip("Health text (optional)")]
        public TextMeshProUGUI healthText;

        [Header("Settings")]
        [Tooltip("Show health text")]
        public bool showHealthText = true;

        [Tooltip("Smooth transition")]
        public bool smoothTransition = true;

        [Tooltip("Transition speed")]
        public float transitionSpeed = 5f;

        [Header("Colors")]
        [Tooltip("Healthy color (> 50%)")]
        public Color healthyColor = Color.green;

        [Tooltip("Warning color (25-50%)")]
        public Color warningColor = Color.yellow;

        [Tooltip("Critical color (< 25%)")]
        public Color criticalColor = Color.red;

        [Header("World Space")]
        [Tooltip("Is world space health bar")]
        public bool isWorldSpace = false;

        [Tooltip("Camera to face (world space only)")]
        public Camera targetCamera;

        [Tooltip("Height offset above entity")]
        public float heightOffset = 2f;

        private float currentFillAmount = 1f;

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged.AddListener(OnHealthChanged);
                OnHealthChanged(healthComponent.currentHealth, healthComponent.maxHealth);
            }
        }

        private void Update()
        {
            // World space: face camera
            if (isWorldSpace && targetCamera != null)
            {
                transform.LookAt(transform.position + targetCamera.transform.rotation * Vector3.forward,
                                targetCamera.transform.rotation * Vector3.up);
            }

            // Smooth transition
            if (smoothTransition && healthBarFill != null)
            {
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, currentFillAmount, transitionSpeed * Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (healthComponent != null)
            {
                healthComponent.OnHealthChanged.RemoveListener(OnHealthChanged);
            }
        }

        /// <summary>
        /// Health changed callback
        /// </summary>
        private void OnHealthChanged(float currentHealth, float maxHealth)
        {
            UpdateHealthBar(currentHealth, maxHealth);
        }

        /// <summary>
        /// Update health bar display
        /// </summary>
        public void UpdateHealthBar(float currentHealth, float maxHealth)
        {
            float healthPercent = maxHealth > 0 ? (currentHealth / maxHealth) : 0f;
            currentFillAmount = Mathf.Clamp01(healthPercent);

            // Update fill
            if (healthBarFill != null)
            {
                if (!smoothTransition)
                {
                    healthBarFill.fillAmount = currentFillAmount;
                }

                // Update color
                if (healthPercent > 0.5f)
                {
                    healthBarFill.color = healthyColor;
                }
                else if (healthPercent > 0.25f)
                {
                    healthBarFill.color = warningColor;
                }
                else
                {
                    healthBarFill.color = criticalColor;
                }
            }

            // Update text
            if (healthText != null && showHealthText)
            {
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
        }
    }
}
