using System.Collections.Generic;
using UnityEngine;
using CozyGame.Dialogue;

namespace CozyGame
{
    /// <summary>
    /// Quest-giving Dragon NPC.
    /// Manages multiple quests and changes dialogue based on quest states.
    /// </summary>
    public class DragonNPC : NPCInteractable
    {
        [Header("Dragon Quest Settings")]
        [Tooltip("All quests this dragon can give")]
        public List<QuestData> availableQuests = new List<QuestData>();

        [Tooltip("Dialogue when dragon has new quest available")]
        public DialogueData newQuestDialogue;

        [Tooltip("Dialogue when player has quest in progress")]
        public DialogueData questInProgressDialogue;

        [Tooltip("Dialogue when player can turn in completed quest")]
        public DialogueData questCompleteDialogue;

        [Tooltip("Dialogue when player has no active quests and none available")]
        public DialogueData noQuestDialogue;

        [Header("Dragon Personality")]
        [Tooltip("Dragon hoards this item type (for quest rewards)")]
        public string hoardedItemType = "Crystal";

        [Tooltip("Friendly greeting sound")]
        public AudioClip greetingSound;

        [Tooltip("Happy sound when quest is turned in")]
        public AudioClip happySound;

        [Tooltip("Disappointed sound when quest is not complete")]
        public AudioClip disappointedSound;

        [Header("Visual Effects")]
        [Tooltip("Particle effect when dragon is happy")]
        public ParticleSystem happyParticles;

        [Tooltip("Glow effect when quest is available")]
        public GameObject questAvailableGlow;

        protected override void Start()
        {
            base.Start();

            // Subscribe to quest events
            QuestManager.OnQuestCompleted += HandleQuestCompleted;
            QuestManager.OnQuestStarted += HandleQuestStarted;

            UpdateQuestIndicator();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from quest events
            QuestManager.OnQuestCompleted -= HandleQuestCompleted;
            QuestManager.OnQuestStarted -= HandleQuestStarted;
        }

        public override void Interact()
        {
            if (isInteracting)
                return;

            // Play greeting sound
            if (greetingSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(greetingSound.name);
            }

            // Determine which dialogue to show based on quest state
            DialogueData dialogueToShow = GetAppropriateDialogue();

            if (dialogueToShow != null)
            {
                OnInteracted?.Invoke();
                bool started = StartDialogue(dialogueToShow);

                if (!started)
                {
                    Debug.LogWarning($"[DragonNPC] Could not start dialogue for {npcName}");
                }
            }
            else
            {
                // Fallback to default dialogue
                base.Interact();
            }
        }

        /// <summary>
        /// Determine which dialogue to show based on current quest states
        /// </summary>
        private DialogueData GetAppropriateDialogue()
        {
            if (QuestManager.Instance == null)
                return defaultDialogue;

            // Check if any quest is ready to turn in
            foreach (QuestData quest in availableQuests)
            {
                if (QuestManager.Instance.IsQuestActive(quest))
                {
                    if (QuestManager.Instance.CheckQuestCompletion(quest))
                    {
                        // Quest complete, ready to turn in
                        return questCompleteDialogue;
                    }
                    else
                    {
                        // Quest in progress but not complete
                        return questInProgressDialogue;
                    }
                }
            }

            // Check if any new quest is available
            foreach (QuestData quest in availableQuests)
            {
                if (quest.currentState == QuestState.NotStarted && quest.IsAvailable())
                {
                    return newQuestDialogue;
                }
            }

            // No quests available or in progress
            return noQuestDialogue != null ? noQuestDialogue : defaultDialogue;
        }

        /// <summary>
        /// Give a specific quest to the player
        /// </summary>
        public bool GiveQuest(QuestData quest)
        {
            if (quest == null || QuestManager.Instance == null)
                return false;

            if (!availableQuests.Contains(quest))
            {
                Debug.LogWarning($"[DragonNPC] Quest '{quest.questName}' is not in {npcName}'s available quests");
                return false;
            }

            bool started = QuestManager.Instance.StartQuest(quest);

            if (started && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("quest_accept");
            }

            return started;
        }

        /// <summary>
        /// Complete a specific quest
        /// </summary>
        public bool CompleteQuest(QuestData quest)
        {
            if (quest == null || QuestManager.Instance == null)
                return false;

            bool completed = QuestManager.Instance.CompleteQuest(quest);

            if (completed)
            {
                // Play happy sound
                if (happySound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(happySound.name);
                }

                // Play happy particles
                if (happyParticles != null)
                {
                    happyParticles.Play();
                }

                UpdateQuestIndicator();
            }

            return completed;
        }

        /// <summary>
        /// Update quest indicator based on available quests
        /// </summary>
        private void UpdateQuestIndicator()
        {
            if (nameplate == null || QuestManager.Instance == null)
                return;

            // Check if any quest is complete
            foreach (QuestData quest in availableQuests)
            {
                if (QuestManager.Instance.IsQuestActive(quest) && QuestManager.Instance.CheckQuestCompletion(quest))
                {
                    nameplate.ShowQuestIndicator(QuestIndicatorState.Complete);
                    if (questAvailableGlow != null)
                        questAvailableGlow.SetActive(true);
                    return;
                }
            }

            // Check if any quest is in progress
            foreach (QuestData quest in availableQuests)
            {
                if (QuestManager.Instance.IsQuestActive(quest))
                {
                    nameplate.ShowQuestIndicator(QuestIndicatorState.InProgress);
                    if (questAvailableGlow != null)
                        questAvailableGlow.SetActive(false);
                    return;
                }
            }

            // Check if any new quest is available
            foreach (QuestData quest in availableQuests)
            {
                if (quest.currentState == QuestState.NotStarted && quest.IsAvailable())
                {
                    nameplate.ShowQuestIndicator(QuestIndicatorState.Available);
                    if (questAvailableGlow != null)
                        questAvailableGlow.SetActive(true);
                    return;
                }
            }

            // No quests
            nameplate.HideQuestIndicator();
            if (questAvailableGlow != null)
                questAvailableGlow.SetActive(false);
        }

        /// <summary>
        /// Called when any quest is completed
        /// </summary>
        private void HandleQuestCompleted(QuestData quest)
        {
            if (availableQuests.Contains(quest))
            {
                UpdateQuestIndicator();
            }
        }

        /// <summary>
        /// Called when any quest is started
        /// </summary>
        private void HandleQuestStarted(QuestData quest)
        {
            if (availableQuests.Contains(quest))
            {
                UpdateQuestIndicator();
            }
        }

        /// <summary>
        /// Get the next available quest
        /// </summary>
        public QuestData GetNextAvailableQuest()
        {
            if (QuestManager.Instance == null)
                return null;

            foreach (QuestData quest in availableQuests)
            {
                if (quest.currentState == QuestState.NotStarted && quest.IsAvailable())
                {
                    return quest;
                }
            }

            return null;
        }

        /// <summary>
        /// Get currently active quest from this dragon
        /// </summary>
        public QuestData GetActiveQuest()
        {
            if (QuestManager.Instance == null)
                return null;

            foreach (QuestData quest in availableQuests)
            {
                if (QuestManager.Instance.IsQuestActive(quest))
                {
                    return quest;
                }
            }

            return null;
        }
    }
}
