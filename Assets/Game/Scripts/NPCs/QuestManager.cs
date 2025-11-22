using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame
{
    /// <summary>
    /// Manages all active quests in the game
    /// Singleton pattern - only one instance exists
    /// Attach to a GameObject in your main scene
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Active Quests")]
        [Tooltip("Quests currently in progress")]
        public List<QuestData> activeQuests = new List<QuestData>();

        [Header("Completed Quests")]
        [Tooltip("Quests that have been finished")]
        public List<QuestData> completedQuests = new List<QuestData>();

        [Header("Events")]
        public delegate void QuestEvent(QuestData quest);
        public event QuestEvent OnQuestStarted;
        public event QuestEvent OnQuestCompleted;
        public event QuestEvent OnQuestFailed;
        public event QuestEvent OnQuestProgressUpdated;

        [Header("Debug")]
        public bool showDebugLogs = true;

        private void Awake()
        {
            // Singleton setup
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
        }

        /// <summary>
        /// Start a new quest
        /// </summary>
        public bool StartQuest(QuestData quest)
        {
            if (quest == null)
            {
                LogWarning("Cannot start null quest!");
                return false;
            }

            if (activeQuests.Contains(quest))
            {
                LogWarning($"Quest '{quest.questName}' is already active!");
                return false;
            }

            if (!quest.IsAvailable())
            {
                LogWarning($"Quest '{quest.questName}' is not available (cooldown or already completed)!");
                return false;
            }

            // Add to active quests
            activeQuests.Add(quest);
            quest.currentState = QuestState.Active;

            // Reset requirements progress
            foreach (var req in quest.requirements)
            {
                req.currentQuantity = 0;
            }

            // Trigger event
            OnQuestStarted?.Invoke(quest);

            Log($"Quest started: {quest.questName}");

            // Show UI notification (if you have a notification system)
            ShowQuestNotification($"Quest Started: {quest.questName}");

            return true;
        }

        /// <summary>
        /// Update quest progress (call when player gains items)
        /// </summary>
        public void UpdateQuestProgress(string itemName, int quantity)
        {
            bool anyUpdated = false;

            foreach (var quest in activeQuests)
            {
                foreach (var requirement in quest.requirements)
                {
                    if (requirement.itemName == itemName)
                    {
                        requirement.currentQuantity = Mathf.Min(
                            requirement.currentQuantity + quantity,
                            requirement.requiredQuantity
                        );
                        anyUpdated = true;

                        Log($"Quest '{quest.questName}' progress: {itemName} {requirement.currentQuantity}/{requirement.requiredQuantity}");
                    }
                }

                if (anyUpdated)
                {
                    OnQuestProgressUpdated?.Invoke(quest);

                    // Check if quest is complete
                    if (CheckQuestCompletion(quest))
                    {
                        // Auto-complete might not be desired for all quests
                        // You may want to require player to return to NPC
                        Log($"Quest '{quest.questName}' objectives complete! Return to quest giver.");
                    }
                }
            }
        }

        /// <summary>
        /// Check if all quest requirements are met
        /// </summary>
        public bool CheckQuestCompletion(QuestData quest)
        {
            if (quest == null || quest.requirements == null || quest.requirements.Count == 0)
                return false;

            return quest.requirements.All(req => req.IsMet());
        }

        /// <summary>
        /// Complete a quest and grant rewards
        /// Call this when player turns in the quest to NPC
        /// </summary>
        public bool CompleteQuest(QuestData quest)
        {
            if (quest == null)
            {
                LogWarning("Cannot complete null quest!");
                return false;
            }

            if (!activeQuests.Contains(quest))
            {
                LogWarning($"Quest '{quest.questName}' is not active!");
                return false;
            }

            if (!CheckQuestCompletion(quest))
            {
                LogWarning($"Quest '{quest.questName}' requirements not met!");
                return false;
            }

            // Remove from active
            activeQuests.Remove(quest);

            // Add to completed (if not already there)
            if (!completedQuests.Contains(quest))
            {
                completedQuests.Add(quest);
            }

            // Update quest state
            quest.MarkAsCompleted();

            // Grant rewards
            GrantRewards(quest);

            // Trigger event
            OnQuestCompleted?.Invoke(quest);

            Log($"Quest completed: {quest.questName}");

            // Show completion notification
            ShowQuestNotification($"Quest Complete: {quest.questName}", isCompletion: true);

            return true;
        }

        /// <summary>
        /// Fail a quest (optional, depending on game design)
        /// </summary>
        public void FailQuest(QuestData quest)
        {
            if (!activeQuests.Contains(quest))
            {
                LogWarning($"Quest '{quest.questName}' is not active!");
                return;
            }

            activeQuests.Remove(quest);
            quest.currentState = QuestState.Failed;

            OnQuestFailed?.Invoke(quest);

            Log($"Quest failed: {quest.questName}");
        }

        /// <summary>
        /// Grant quest rewards to player
        /// Override this or use events to integrate with your inventory system
        /// </summary>
        protected virtual void GrantRewards(QuestData quest)
        {
            // TODO: Integrate with Survival Engine inventory
            foreach (var reward in quest.rewards)
            {
                Log($"Granted reward: {reward.quantity}x {reward.itemName}");

                // Example integration with Survival Engine:
                // var itemData = ItemData.Get(reward.itemName);
                // if (itemData != null)
                // {
                //     PlayerData.Get().inventory.AddItem(itemData, reward.quantity);
                // }

                // Play reward sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("quest_complete");
                }
            }
        }

        /// <summary>
        /// Check if a quest is active
        /// </summary>
        public bool IsQuestActive(QuestData quest)
        {
            return activeQuests.Contains(quest);
        }

        /// <summary>
        /// Check if a quest is completed
        /// </summary>
        public bool IsQuestCompleted(QuestData quest)
        {
            return completedQuests.Contains(quest);
        }

        /// <summary>
        /// Get all active quests
        /// </summary>
        public List<QuestData> GetActiveQuests()
        {
            return new List<QuestData>(activeQuests);
        }

        /// <summary>
        /// Get all completed quests
        /// </summary>
        public List<QuestData> GetCompletedQuests()
        {
            return new List<QuestData>(completedQuests);
        }

        /// <summary>
        /// Get quest by ID
        /// </summary>
        public QuestData GetQuestByID(string questID)
        {
            var quest = activeQuests.FirstOrDefault(q => q.questID == questID);
            if (quest != null) return quest;

            quest = completedQuests.FirstOrDefault(q => q.questID == questID);
            return quest;
        }

        /// <summary>
        /// Cancel/abandon a quest
        /// </summary>
        public void AbandonQuest(QuestData quest)
        {
            if (activeQuests.Contains(quest))
            {
                activeQuests.Remove(quest);
                quest.currentState = QuestState.NotStarted;
                Log($"Quest abandoned: {quest.questName}");
            }
        }

        /// <summary>
        /// Show quest notification to player
        /// Override this to connect to your UI system
        /// </summary>
        protected virtual void ShowQuestNotification(string message, bool isCompletion = false)
        {
            // TODO: Integrate with your notification UI
            Log($"[NOTIFICATION] {message}");

            // Example: You could call a NotificationManager here
            // NotificationManager.Instance?.ShowNotification(message, isCompletion ? 3f : 2f);
        }

        // Helper logging methods
        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[QuestManager] {message}");
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[QuestManager] {message}");
        }

        /// <summary>
        /// Reset all quests (useful for testing)
        /// </summary>
        [ContextMenu("Reset All Quests")]
        public void ResetAllQuests()
        {
            foreach (var quest in activeQuests)
            {
                quest.ResetQuest();
            }

            foreach (var quest in completedQuests)
            {
                quest.ResetQuest();
            }

            activeQuests.Clear();
            completedQuests.Clear();

            Log("All quests reset!");
        }
    }
}
