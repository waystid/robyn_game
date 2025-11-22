using UnityEngine;
using System;

namespace CozyGame.Achievements
{
    /// <summary>
    /// Achievement type categories
    /// </summary>
    public enum AchievementType
    {
        OneTime,        // Unlock once (e.g., "Beat first boss")
        Incremental,    // Progress over time (e.g., "Collect 100 items")
        累計            // Cumulative (Japanese for cumulative, tracks total)
    }

    /// <summary>
    /// Achievement rarity levels
    /// </summary>
    public enum AchievementRarity
    {
        Common,
        Rare,
        Epic,
        Legendary,
        Secret
    }

    /// <summary>
    /// Achievement definition ScriptableObject.
    /// Defines all properties of an achievement.
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement", menuName = "Cozy Game/Achievements/Achievement")]
    public class Achievement : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique achievement ID")]
        public string achievementID;

        [Tooltip("Achievement name")]
        public string achievementName;

        [Tooltip("Achievement description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Achievement icon")]
        public Sprite icon;

        [Tooltip("Locked icon (shown when not unlocked)")]
        public Sprite lockedIcon;

        [Header("Properties")]
        [Tooltip("Achievement type")]
        public AchievementType achievementType = AchievementType.OneTime;

        [Tooltip("Achievement rarity")]
        public AchievementRarity rarity = AchievementRarity.Common;

        [Tooltip("Is this a secret achievement?")]
        public bool isSecret = false;

        [Tooltip("Required progress (for incremental achievements)")]
        public int requiredProgress = 1;

        [Tooltip("Reward points (for player profile)")]
        public int rewardPoints = 10;

        [Header("Unlock Criteria")]
        [Tooltip("Statistic to track (e.g., 'enemies_killed')")]
        public string statisticKey = "";

        [Tooltip("Required statistic value")]
        public float requiredValue = 1f;

        [Tooltip("Required achievements (must unlock these first)")]
        public Achievement[] prerequisiteAchievements;

        [Header("Rewards")]
        [Tooltip("Unlock a spell on achievement")]
        public string unlockSpellID = "";

        [Tooltip("Grant item on achievement")]
        public string grantItemID = "";

        [Tooltip("Grant item quantity")]
        public int grantItemQuantity = 1;

        [Header("Display")]
        [Tooltip("Category for grouping (e.g., 'Combat', 'Exploration')")]
        public string category = "General";

        [Tooltip("Sort order within category")]
        public int sortOrder = 0;

        /// <summary>
        /// Get rarity color
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case AchievementRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // Gray
                case AchievementRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f); // Blue
                case AchievementRarity.Epic:
                    return new Color(0.8f, 0.3f, 1f); // Purple
                case AchievementRarity.Legendary:
                    return new Color(1f, 0.6f, 0f); // Orange
                case AchievementRarity.Secret:
                    return new Color(1f, 0.8f, 0.2f); // Gold
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Get display name (hides secret achievements)
        /// </summary>
        public string GetDisplayName(bool isUnlocked)
        {
            if (isSecret && !isUnlocked)
            {
                return "???";
            }

            return achievementName;
        }

        /// <summary>
        /// Get display description (hides secret achievements)
        /// </summary>
        public string GetDisplayDescription(bool isUnlocked)
        {
            if (isSecret && !isUnlocked)
            {
                return "This is a secret achievement.";
            }

            return description;
        }

        /// <summary>
        /// Get display icon
        /// </summary>
        public Sprite GetDisplayIcon(bool isUnlocked)
        {
            if (isUnlocked)
            {
                return icon;
            }
            else
            {
                return lockedIcon != null ? lockedIcon : icon;
            }
        }

        /// <summary>
        /// Get formatted tooltip text
        /// </summary>
        public string GetTooltipText(bool isUnlocked, int currentProgress)
        {
            string tooltip = $"<b>{GetDisplayName(isUnlocked)}</b>\n";
            tooltip += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{rarity}</color>\n\n";
            tooltip += $"{GetDisplayDescription(isUnlocked)}\n\n";

            if (achievementType == AchievementType.Incremental && isUnlocked)
            {
                tooltip += $"Progress: {currentProgress}/{requiredProgress}\n";
            }

            if (rewardPoints > 0)
            {
                tooltip += $"\nReward: {rewardPoints} points";
            }

            if (!string.IsNullOrEmpty(category))
            {
                tooltip += $"\nCategory: {category}";
            }

            return tooltip;
        }

        /// <summary>
        /// Validate achievement data
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate ID from name if empty
            if (string.IsNullOrEmpty(achievementID) && !string.IsNullOrEmpty(achievementName))
            {
                achievementID = achievementName.ToLower().Replace(" ", "_");
            }

            // Ensure progress is at least 1
            if (requiredProgress < 1)
            {
                requiredProgress = 1;
            }

            // OneTime achievements should have progress of 1
            if (achievementType == AchievementType.OneTime)
            {
                requiredProgress = 1;
            }
        }
    }
}
