using UnityEngine;
using CozyGame;

namespace CozyGame.SurvivalEngine
{
    /// <summary>
    /// Integrates QuestManager with Survival Engine's inventory system
    /// Attach this to the QuestManager GameObject
    /// </summary>
    [RequireComponent(typeof(QuestManager))]
    public class QuestIntegration : MonoBehaviour
    {
        [Header("Integration Settings")]
        [Tooltip("Use Survival Engine inventory for rewards?")]
        public bool useSurvivalEngineInventory = true;

        [Tooltip("Show UI notifications for quest events?")]
        public bool showNotifications = true;

        private QuestManager questManager;

        private void Awake()
        {
            questManager = GetComponent<QuestManager>();
        }

        private void OnEnable()
        {
            // Subscribe to quest events
            if (questManager != null)
            {
                questManager.OnQuestStarted += HandleQuestStarted;
                questManager.OnQuestCompleted += HandleQuestCompleted;
                questManager.OnQuestProgressUpdated += HandleQuestProgress;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (questManager != null)
            {
                questManager.OnQuestStarted -= HandleQuestStarted;
                questManager.OnQuestCompleted -= HandleQuestCompleted;
                questManager.OnQuestProgressUpdated -= HandleQuestProgress;
            }
        }

        private void HandleQuestStarted(QuestData quest)
        {
            Debug.Log($"[QuestIntegration] Quest started: {quest.questName}");

            if (showNotifications)
            {
                SurvivalEngineHelper.ShowNotification($"New Quest: {quest.questName}", 3f);
            }

            // Play quest accept sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("quest_accept");
            }
        }

        private void HandleQuestCompleted(QuestData quest)
        {
            Debug.Log($"[QuestIntegration] Quest completed: {quest.questName}");

            // Grant rewards using Survival Engine inventory
            if (useSurvivalEngineInventory)
            {
                GrantSurvivalEngineRewards(quest);
            }

            if (showNotifications)
            {
                SurvivalEngineHelper.ShowNotification($"Quest Complete: {quest.questName}!", 4f);
            }

            // Play quest complete sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("quest_complete");
            }

            // Show reward particles
            ShowRewardEffects();
        }

        private void HandleQuestProgress(QuestData quest)
        {
            Debug.Log($"[QuestIntegration] Quest progress updated: {quest.questName}");

            // Optional: Show progress notification
            // (Might be too spammy, so disabled by default)
        }

        /// <summary>
        /// Grant quest rewards using Survival Engine inventory
        /// </summary>
        private void GrantSurvivalEngineRewards(QuestData quest)
        {
            foreach (var reward in quest.rewards)
            {
                // Use Survival Engine helper to add items
                bool success = SurvivalEngineHelper.AddItemToInventory(reward.itemName, reward.quantity);

                if (success)
                {
                    Debug.Log($"Granted {reward.quantity}x {reward.itemName}");

                    // Show floating text for reward
                    if (FloatingTextManager.Instance != null)
                    {
                        Vector3 playerPos = SurvivalEngineHelper.GetPlayerPosition();
                        if (playerPos != Vector3.zero)
                        {
                            string itemName = SurvivalEngineHelper.GetItemName(reward.itemName);
                            FloatingTextManager.Instance.ShowItemPickup(
                                itemName,
                                reward.quantity,
                                playerPos + Vector3.up * 2f,
                                isRare: true
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if player has required items using Survival Engine inventory
        /// </summary>
        public bool CheckRequirements(QuestData quest)
        {
            if (!useSurvivalEngineInventory)
                return false;

            foreach (var requirement in quest.requirements)
            {
                if (!SurvivalEngineHelper.HasItem(requirement.itemName, requirement.requiredQuantity))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Remove required items from inventory when completing quest
        /// </summary>
        public bool ConsumeRequirements(QuestData quest)
        {
            if (!useSurvivalEngineInventory)
                return true;

            // First check if player has all items
            if (!CheckRequirements(quest))
            {
                Debug.LogWarning("Player doesn't have required items!");
                return false;
            }

            // Remove items
            foreach (var requirement in quest.requirements)
            {
                SurvivalEngineHelper.RemoveItemFromInventory(
                    requirement.itemName,
                    requirement.requiredQuantity
                );
            }

            return true;
        }

        /// <summary>
        /// Show visual effects when receiving rewards
        /// </summary>
        private void ShowRewardEffects()
        {
            Vector3 playerPos = SurvivalEngineHelper.GetPlayerPosition();
            if (playerPos == Vector3.zero) return;

            // TODO: Spawn reward particle effect
            // You can add a reward particle prefab here
        }

        /// <summary>
        /// Complete quest with Survival Engine integration
        /// Call this from NPC dialogue or interaction
        /// </summary>
        public bool CompleteQuestWithIntegration(QuestData quest)
        {
            // Check if quest is active
            if (!questManager.IsQuestActive(quest))
            {
                Debug.LogWarning("Quest is not active!");
                return false;
            }

            // Check if requirements are met
            if (!CheckRequirements(quest))
            {
                Debug.LogWarning("Quest requirements not met!");
                if (showNotifications)
                {
                    SurvivalEngineHelper.ShowNotification("You don't have the required items!");
                }
                return false;
            }

            // Consume required items
            if (!ConsumeRequirements(quest))
            {
                return false;
            }

            // Complete the quest (this will trigger HandleQuestCompleted which grants rewards)
            return questManager.CompleteQuest(quest);
        }
    }
}
