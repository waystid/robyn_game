using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Achievements;

namespace CozyGame.UI
{
    /// <summary>
    /// Achievement panel UI.
    /// Shows all achievements organized by category.
    /// </summary>
    public class AchievementUI : MonoBehaviour
    {
        [System.Serializable]
        public class AchievementSlotUI
        {
            public GameObject slotObject;
            public Image iconImage;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI descriptionText;
            public TextMeshProUGUI progressText;
            public Image rarityBorder;
            public GameObject lockedOverlay;
        }

        [Header("UI Panels")]
        [Tooltip("Achievement panel")]
        public GameObject achievementPanel;

        [Header("Achievement List")]
        [Tooltip("Achievement container (Vertical Layout Group)")]
        public Transform achievementContainer;

        [Tooltip("Achievement slot prefab")]
        public GameObject achievementSlotPrefab;

        [Header("Category Filter")]
        [Tooltip("Category dropdown")]
        public TMP_Dropdown categoryDropdown;

        [Header("Stats Display")]
        [Tooltip("Total unlocked text")]
        public TextMeshProUGUI totalUnlockedText;

        [Tooltip("Unlock percentage text")]
        public TextMeshProUGUI unlockPercentageText;

        [Tooltip("Total points text")]
        public TextMeshProUGUI totalPointsText;

        [Header("Buttons")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Header("Audio")]
        [Tooltip("Button click sound")]
        public string buttonClickSound = "button_click";

        // State
        private List<AchievementSlotUI> achievementSlots = new List<AchievementSlotUI>();
        private string selectedCategory = "All";

        private void Start()
        {
            // Setup button listeners
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (categoryDropdown != null)
            {
                categoryDropdown.onValueChanged.AddListener(OnCategoryChanged);
            }

            // Subscribe to achievement events
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementUnlocked.AddListener(OnAchievementUnlocked);
                AchievementSystem.Instance.OnAchievementProgress.AddListener(OnAchievementProgress);
            }

            // Initialize categories
            InitializeCategories();

            // Hide panel initially
            Hide();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (AchievementSystem.Instance != null)
            {
                AchievementSystem.Instance.OnAchievementUnlocked.RemoveListener(OnAchievementUnlocked);
                AchievementSystem.Instance.OnAchievementProgress.RemoveListener(OnAchievementProgress);
            }
        }

        /// <summary>
        /// Initialize category dropdown
        /// </summary>
        private void InitializeCategories()
        {
            if (categoryDropdown == null || AchievementSystem.Instance == null)
                return;

            categoryDropdown.ClearOptions();

            List<string> categories = new List<string> { "All" };
            categories.AddRange(AchievementSystem.Instance.GetAllCategories());

            categoryDropdown.AddOptions(categories);
        }

        /// <summary>
        /// Category changed callback
        /// </summary>
        private void OnCategoryChanged(int index)
        {
            if (categoryDropdown == null)
                return;

            selectedCategory = categoryDropdown.options[index].text;
            RefreshAchievementList();
        }

        /// <summary>
        /// Refresh achievement list display
        /// </summary>
        public void RefreshAchievementList()
        {
            if (AchievementSystem.Instance == null || achievementContainer == null || achievementSlotPrefab == null)
                return;

            // Clear existing slots
            foreach (Transform child in achievementContainer)
            {
                Destroy(child.gameObject);
            }
            achievementSlots.Clear();

            // Get achievements to display
            List<Achievement> achievements;
            if (selectedCategory == "All")
            {
                achievements = AchievementSystem.Instance.GetAllAchievements();
            }
            else
            {
                achievements = AchievementSystem.Instance.GetAchievementsByCategory(selectedCategory);
            }

            // Create slot for each achievement
            foreach (var achievement in achievements)
            {
                CreateAchievementSlot(achievement);
            }

            // Update stats
            UpdateStatsDisplay();
        }

        /// <summary>
        /// Create achievement slot UI
        /// </summary>
        private void CreateAchievementSlot(Achievement achievement)
        {
            GameObject slotObj = Instantiate(achievementSlotPrefab, achievementContainer);

            AchievementSlotUI slot = new AchievementSlotUI
            {
                slotObject = slotObj,
                iconImage = slotObj.transform.Find("Icon")?.GetComponent<Image>(),
                nameText = slotObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>(),
                descriptionText = slotObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>(),
                progressText = slotObj.transform.Find("Progress")?.GetComponent<TextMeshProUGUI>(),
                rarityBorder = slotObj.transform.Find("RarityBorder")?.GetComponent<Image>(),
                lockedOverlay = slotObj.transform.Find("LockedOverlay")?.gameObject
            };

            achievementSlots.Add(slot);

            // Update slot display
            UpdateAchievementSlot(slot, achievement);
        }

        /// <summary>
        /// Update achievement slot display
        /// </summary>
        private void UpdateAchievementSlot(AchievementSlotUI slot, Achievement achievement)
        {
            bool isUnlocked = AchievementSystem.Instance.IsAchievementUnlocked(achievement.achievementID);
            AchievementProgress progress = AchievementSystem.Instance.GetProgress(achievement.achievementID);

            // Icon
            if (slot.iconImage != null)
            {
                slot.iconImage.sprite = achievement.GetDisplayIcon(isUnlocked);
            }

            // Name
            if (slot.nameText != null)
            {
                slot.nameText.text = achievement.GetDisplayName(isUnlocked);
            }

            // Description
            if (slot.descriptionText != null)
            {
                slot.descriptionText.text = achievement.GetDisplayDescription(isUnlocked);
            }

            // Progress
            if (slot.progressText != null)
            {
                if (achievement.achievementType == AchievementType.Incremental && progress != null)
                {
                    slot.progressText.text = $"{progress.currentProgress}/{achievement.requiredProgress}";
                }
                else if (isUnlocked)
                {
                    slot.progressText.text = "Unlocked";
                }
                else
                {
                    slot.progressText.text = "Locked";
                }
            }

            // Rarity border
            if (slot.rarityBorder != null)
            {
                slot.rarityBorder.color = achievement.GetRarityColor();
            }

            // Locked overlay
            if (slot.lockedOverlay != null)
            {
                slot.lockedOverlay.SetActive(!isUnlocked);
            }
        }

        /// <summary>
        /// Update stats display
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (AchievementSystem.Instance == null)
                return;

            var (unlocked, total) = AchievementSystem.Instance.GetAchievementCounts();

            if (totalUnlockedText != null)
            {
                totalUnlockedText.text = $"{unlocked}/{total} Achievements";
            }

            if (unlockPercentageText != null)
            {
                float percentage = AchievementSystem.Instance.GetUnlockPercentage();
                unlockPercentageText.text = $"{percentage:F1}% Complete";
            }

            if (totalPointsText != null)
            {
                int points = AchievementSystem.Instance.GetTotalPoints();
                totalPointsText.text = $"{points} Points";
            }
        }

        /// <summary>
        /// Achievement unlocked callback
        /// </summary>
        private void OnAchievementUnlocked(Achievement achievement)
        {
            RefreshAchievementList();
        }

        /// <summary>
        /// Achievement progress callback
        /// </summary>
        private void OnAchievementProgress(Achievement achievement, int progress)
        {
            // Update just the progress for this achievement
            RefreshAchievementList();
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseClicked()
        {
            PlayButtonSound();
            Hide();
        }

        /// <summary>
        /// Show achievement panel
        /// </summary>
        public void Show()
        {
            if (achievementPanel != null)
            {
                achievementPanel.SetActive(true);
                RefreshAchievementList();
            }
        }

        /// <summary>
        /// Hide achievement panel
        /// </summary>
        public void Hide()
        {
            if (achievementPanel != null)
            {
                achievementPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle achievement panel
        /// </summary>
        public void Toggle()
        {
            if (achievementPanel.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonSound()
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(buttonClickSound))
            {
                AudioManager.Instance.PlaySound(buttonClickSound);
            }
        }
    }
}
