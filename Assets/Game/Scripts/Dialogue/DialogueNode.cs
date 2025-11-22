using System.Collections.Generic;
using UnityEngine;

namespace CozyGame.Dialogue
{
    /// <summary>
    /// Represents a single node in a dialogue tree.
    /// Can have multiple choices leading to different branches.
    /// </summary>
    [System.Serializable]
    public class DialogueNode
    {
        [Header("Speaker Info")]
        [Tooltip("Name of the character speaking this line")]
        public string speakerName = "NPC";

        [Tooltip("Optional portrait sprite for the speaker")]
        public Sprite speakerPortrait;

        [Header("Dialogue Content")]
        [TextArea(3, 10)]
        [Tooltip("The actual dialogue text to display")]
        public string dialogueText = "Hello, traveler!";

        [Tooltip("Optional voice line audio clip")]
        public AudioClip voiceLine;

        [Tooltip("Voice blip sound for typing effect (short audio clip played per character)")]
        public AudioClip voiceBlip;

        [Header("Branching")]
        [Tooltip("Player choices available at this node")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        [Tooltip("If no choices, this is the next node ID to go to (linear conversation)")]
        public string nextNodeID = "";

        [Tooltip("Unique identifier for this node")]
        public string nodeID = "node_001";

        [Header("Special Actions")]
        [Tooltip("Quest ID to start when this node is shown")]
        public string questToStart = "";

        [Tooltip("Quest ID to complete when this node is shown")]
        public string questToComplete = "";

        [Tooltip("Riddle ID to present when this node is shown")]
        public string riddleToPresent = "";

        [Tooltip("Item ID to give to player")]
        public string itemToGive = "";

        [Tooltip("Quantity of item to give")]
        public int itemQuantity = 1;

        [Tooltip("Custom event name to trigger (for game-specific actions)")]
        public string customEvent = "";

        [Header("Conditions")]
        [Tooltip("Only show this node if player has this quest active")]
        public string requiresActiveQuest = "";

        [Tooltip("Only show this node if player has completed this quest")]
        public string requiresCompletedQuest = "";

        [Tooltip("Only show this node if player has this item")]
        public string requiresItem = "";

        [Tooltip("Quantity of required item")]
        public int requiredItemQuantity = 1;

        [Header("Animation")]
        [Tooltip("Animation trigger to play on NPC")]
        public string animationTrigger = "";

        [Tooltip("Delay before showing this dialogue (seconds)")]
        public float delayBeforeShow = 0f;

        [Tooltip("Auto-advance to next node after this duration (0 = manual advance)")]
        public float autoAdvanceDelay = 0f;

        /// <summary>
        /// Check if this node's conditions are met
        /// </summary>
        public bool AreConditionsMet()
        {
            // TODO: Integrate with QuestManager and Inventory when ready
            // For now, always return true

            /*
            if (!string.IsNullOrEmpty(requiresActiveQuest))
            {
                if (!QuestManager.Instance.IsQuestActive(requiresActiveQuest))
                    return false;
            }

            if (!string.IsNullOrEmpty(requiresCompletedQuest))
            {
                if (!QuestManager.Instance.IsQuestCompleted(requiresCompletedQuest))
                    return false;
            }

            if (!string.IsNullOrEmpty(requiresItem))
            {
                if (!SurvivalEngineHelper.HasItem(requiresItem, requiredItemQuantity))
                    return false;
            }
            */

            return true;
        }

        /// <summary>
        /// Execute any special actions associated with this node
        /// </summary>
        public void ExecuteActions()
        {
            if (!string.IsNullOrEmpty(questToStart))
            {
                // TODO: Integrate with QuestManager
                Debug.Log($"[DialogueNode] Starting quest: {questToStart}");
                // QuestManager.Instance.StartQuest(questToStart);
            }

            if (!string.IsNullOrEmpty(questToComplete))
            {
                Debug.Log($"[DialogueNode] Completing quest: {questToComplete}");
                // QuestManager.Instance.CompleteQuest(questToComplete);
            }

            if (!string.IsNullOrEmpty(riddleToPresent))
            {
                Debug.Log($"[DialogueNode] Presenting riddle: {riddleToPresent}");
                // RiddleManager.Instance.PresentRiddle(riddleToPresent);
            }

            if (!string.IsNullOrEmpty(itemToGive))
            {
                Debug.Log($"[DialogueNode] Giving item: {itemQuantity}x {itemToGive}");
                // SurvivalEngineHelper.AddItemToInventory(itemToGive, itemQuantity);
            }

            if (!string.IsNullOrEmpty(customEvent))
            {
                Debug.Log($"[DialogueNode] Triggering custom event: {customEvent}");
                DialogueManager.OnCustomEvent?.Invoke(customEvent);
            }
        }

        /// <summary>
        /// Get valid choices based on current game state
        /// </summary>
        public List<DialogueChoice> GetValidChoices()
        {
            List<DialogueChoice> validChoices = new List<DialogueChoice>();

            foreach (DialogueChoice choice in choices)
            {
                if (choice.IsAvailable())
                    validChoices.Add(choice);
            }

            return validChoices;
        }

        /// <summary>
        /// Check if this is an ending node (no choices, no next node)
        /// </summary>
        public bool IsEndNode()
        {
            return choices.Count == 0 && string.IsNullOrEmpty(nextNodeID);
        }
    }

    /// <summary>
    /// Represents a player choice in dialogue
    /// </summary>
    [System.Serializable]
    public class DialogueChoice
    {
        [TextArea(1, 3)]
        [Tooltip("The choice text shown to the player")]
        public string choiceText = "What can you tell me?";

        [Tooltip("Node ID to jump to when this choice is selected")]
        public string targetNodeID = "node_002";

        [Header("Conditions")]
        [Tooltip("Only show if player has this quest active")]
        public string requiresActiveQuest = "";

        [Tooltip("Only show if player has this item")]
        public string requiresItem = "";

        [Tooltip("Required item quantity")]
        public int requiredItemQuantity = 1;

        [Tooltip("Only show if player level >= this value")]
        public int minimumLevel = 0;

        [Header("Choice Style")]
        [Tooltip("Color tint for this choice button (use for quest/shop/etc indicators)")]
        public Color choiceColor = Color.white;

        [Tooltip("Icon to show on this choice button")]
        public Sprite choiceIcon;

        /// <summary>
        /// Check if this choice should be available to the player
        /// </summary>
        public bool IsAvailable()
        {
            // TODO: Integrate with game systems when ready

            /*
            if (!string.IsNullOrEmpty(requiresActiveQuest))
            {
                if (!QuestManager.Instance.IsQuestActive(requiresActiveQuest))
                    return false;
            }

            if (!string.IsNullOrEmpty(requiresItem))
            {
                if (!SurvivalEngineHelper.HasItem(requiresItem, requiredItemQuantity))
                    return false;
            }

            if (minimumLevel > 0)
            {
                if (PlayerData.Get().level < minimumLevel)
                    return false;
            }
            */

            return true;
        }
    }
}
