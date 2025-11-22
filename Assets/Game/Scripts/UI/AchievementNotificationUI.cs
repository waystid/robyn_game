using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using CozyGame.Achievements;

namespace CozyGame.UI
{
    /// <summary>
    /// Achievement notification popup.
    /// Shows when achievements are unlocked.
    /// </summary>
    public class AchievementNotificationUI : MonoBehaviour
    {
        public static AchievementNotificationUI Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("Notification panel")]
        public GameObject notificationPanel;

        [Tooltip("Achievement icon")]
        public Image achievementIcon;

        [Tooltip("Achievement name text")]
        public TextMeshProUGUI achievementNameText;

        [Tooltip("Achievement description text")]
        public TextMeshProUGUI achievementDescriptionText;

        [Tooltip("Points text")]
        public TextMeshProUGUI pointsText;

        [Tooltip("Rarity border (optional)")]
        public Image rarityBorder;

        [Header("Animation")]
        [Tooltip("Display duration (seconds)")]
        public float displayDuration = 4f;

        [Tooltip("Fade in duration")]
        public float fadeInDuration = 0.5f;

        [Tooltip("Fade out duration")]
        public float fadeOutDuration = 0.5f;

        [Header("Audio")]
        [Tooltip("Achievement unlock sound")]
        public string unlockSound = "achievement_unlock";

        // Queue for multiple achievements
        private Queue<Achievement> notificationQueue = new Queue<Achievement>();
        private bool isShowingNotification = false;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }

            notificationPanel.SetActive(false);
        }

        private void Start()
        {
            // Subscribe to achievement events
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementUnlocked.AddListener(ShowNotification);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementUnlocked.RemoveListener(ShowNotification);
            }
        }

        /// <summary>
        /// Show notification for achievement
        /// </summary>
        public void ShowNotification(Achievement achievement)
        {
            if (achievement == null)
                return;

            // Add to queue
            notificationQueue.Enqueue(achievement);

            // Start showing if not already showing
            if (!isShowingNotification)
            {
                StartCoroutine(ShowNextNotification());
            }
        }

        /// <summary>
        /// Show next notification in queue
        /// </summary>
        private IEnumerator ShowNextNotification()
        {
            while (notificationQueue.Count > 0)
            {
                isShowingNotification = true;

                Achievement achievement = notificationQueue.Dequeue();

                // Set notification data
                if (achievementIcon != null && achievement.icon != null)
                {
                    achievementIcon.sprite = achievement.icon;
                }

                if (achievementNameText != null)
                {
                    achievementNameText.text = achievement.achievementName;
                }

                if (achievementDescriptionText != null)
                {
                    achievementDescriptionText.text = achievement.description;
                }

                if (pointsText != null)
                {
                    pointsText.text = $"+{achievement.rewardPoints} points";
                }

                if (rarityBorder != null)
                {
                    rarityBorder.color = achievement.GetRarityColor();
                }

                // Play sound
                PlaySound(unlockSound);

                // Show panel
                notificationPanel.SetActive(true);

                // Fade in
                yield return StartCoroutine(FadeIn());

                // Display
                yield return new WaitForSeconds(displayDuration);

                // Fade out
                yield return StartCoroutine(FadeOut());

                // Hide panel
                notificationPanel.SetActive(false);
            }

            isShowingNotification = false;
        }

        /// <summary>
        /// Fade in animation
        /// </summary>
        private IEnumerator FadeIn()
        {
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Fade out animation
        /// </summary>
        private IEnumerator FadeOut()
        {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Play sound
        /// </summary>
        private void PlaySound(string soundName)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioManager.Instance.PlaySound(soundName);
            }
        }
    }
}
