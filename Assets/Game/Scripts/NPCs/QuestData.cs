using UnityEngine;
using System.Collections.Generic;

namespace CozyGame
{
    /// <summary>
    /// Defines a quest with requirements and rewards
    /// Create instances via: Right-click → Create → Cozy Game → Quest Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest", menuName = "Cozy Game/Quest Data", order = 1)]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Info")]
        [Tooltip("Display name of the quest")]
        public string questName = "My Quest";

        [Tooltip("Description shown to the player")]
        [TextArea(3, 6)]
        public string questDescription = "Quest description here...";

        [Tooltip("NPC who gives this quest (for reference)")]
        public string questGiver = "Dragon";

        [Tooltip("Unique ID for this quest (auto-generated)")]
        public string questID;

        [Header("Requirements")]
        [Tooltip("What items/quantities the player needs to complete this quest")]
        public List<QuestRequirement> requirements = new List<QuestRequirement>();

        [Header("Rewards")]
        [Tooltip("What the player receives upon completion")]
        public List<QuestReward> rewards = new List<QuestReward>();

        [Header("Quest State")]
        [Tooltip("Current state of this quest")]
        public QuestState currentState = QuestState.NotStarted;

        [Header("Optional Settings")]
        [Tooltip("Can this quest be repeated?")]
        public bool isRepeatable = false;

        [Tooltip("If repeatable, cooldown in minutes")]
        public float cooldownMinutes = 60f;

        [HideInInspector]
        public float lastCompletedTime = 0f;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(questID))
            {
                questID = name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            }
        }

        /// <summary>
        /// Check if quest is available (not on cooldown)
        /// </summary>
        public bool IsAvailable()
        {
            if (currentState == QuestState.NotStarted)
                return true;

            if (currentState == QuestState.Completed && isRepeatable)
            {
                float timeSinceCompleted = Time.time - lastCompletedTime;
                float cooldownSeconds = cooldownMinutes * 60f;
                return timeSinceCompleted >= cooldownSeconds;
            }

            return false;
        }

        /// <summary>
        /// Get remaining cooldown time in minutes
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (currentState != QuestState.Completed || !isRepeatable)
                return 0f;

            float timeSinceCompleted = Time.time - lastCompletedTime;
            float cooldownSeconds = cooldownMinutes * 60f;
            float remaining = cooldownSeconds - timeSinceCompleted;
            return Mathf.Max(0f, remaining / 60f);
        }

        /// <summary>
        /// Mark quest as completed
        /// </summary>
        public void MarkAsCompleted()
        {
            currentState = QuestState.Completed;
            lastCompletedTime = Time.time;
        }

        /// <summary>
        /// Reset quest to not started (for testing or repeatable quests)
        /// </summary>
        public void ResetQuest()
        {
            currentState = QuestState.NotStarted;
            lastCompletedTime = 0f;
        }
    }

    /// <summary>
    /// Requirement for completing a quest
    /// </summary>
    [System.Serializable]
    public class QuestRequirement
    {
        [Tooltip("Name/ID of the required item (must match ItemData)")]
        public string itemName;

        [Tooltip("How many of this item are required")]
        public int requiredQuantity = 1;

        [Tooltip("Current progress (auto-tracked)")]
        [HideInInspector]
        public int currentQuantity = 0;

        /// <summary>
        /// Check if this requirement is met
        /// </summary>
        public bool IsMet()
        {
            return currentQuantity >= requiredQuantity;
        }

        /// <summary>
        /// Get progress as a percentage
        /// </summary>
        public float GetProgress()
        {
            if (requiredQuantity == 0) return 1f;
            return Mathf.Clamp01((float)currentQuantity / requiredQuantity);
        }
    }

    /// <summary>
    /// Reward given for completing a quest
    /// </summary>
    [System.Serializable]
    public class QuestReward
    {
        [Tooltip("Name/ID of the reward item (must match ItemData)")]
        public string itemName;

        [Tooltip("How many of this item to give")]
        public int quantity = 1;

        [Tooltip("Description of this reward (optional)")]
        public string description;
    }

    /// <summary>
    /// Possible states for a quest
    /// </summary>
    public enum QuestState
    {
        NotStarted,  // Quest hasn't been accepted yet
        Active,      // Quest is in progress
        Completed,   // Quest has been finished
        Failed       // Quest was failed (optional, depends on game design)
    }
}
