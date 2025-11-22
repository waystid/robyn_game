using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Dialogue;

namespace CozyGame.NPCs
{
    /// <summary>
    /// Story category
    /// </summary>
    public enum StoryCategory
    {
        History,        // Historical events
        Legend,         // Myths and legends
        Personal,       // NPC personal stories
        Location,       // Stories about places
        Character,      // Stories about other NPCs
        Secret,         // Hidden lore
        Tutorial,       // Game mechanics explained through story
        Custom          // Custom category
    }

    /// <summary>
    /// Lore story data
    /// </summary>
    [System.Serializable]
    public class LoreStory
    {
        [Header("Story Info")]
        [Tooltip("Story title")]
        public string storyTitle = "Untold Story";

        [Tooltip("Story category")]
        public StoryCategory category = StoryCategory.Legend;

        [Tooltip("Short description")]
        [TextArea(2, 3)]
        public string description = "An interesting tale...";

        [Tooltip("Story dialogue tree")]
        public DialogueData storyDialogue;

        [Header("Unlock Requirements")]
        [Tooltip("Required friendship level")]
        public int requiredFriendship = 0;

        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Tooltip("Other stories that must be heard first")]
        public List<LoreStory> prerequisiteStories = new List<LoreStory>();

        [Tooltip("Is this story available?")]
        public bool isAvailable = true;

        [Tooltip("Is this a one-time story?")]
        public bool isOneTime = false;

        [Header("Rewards")]
        [Tooltip("Experience reward for hearing story")]
        public int experienceReward = 5;

        [Tooltip("Unlocks new quest after hearing")]
        public Quest.QuestData unlocksQuest;

        [Tooltip("Unlocks new location after hearing")]
        public string unlocksLocation = "";

        // Runtime state
        [System.NonSerialized]
        public bool hasBeenHeard = false;

        [System.NonSerialized]
        public int timesHeard = 0;

        /// <summary>
        /// Check if story is available
        /// </summary>
        public bool IsAvailable()
        {
            if (!isAvailable)
                return false;

            if (isOneTime && hasBeenHeard)
                return false;

            return true;
        }

        /// <summary>
        /// Check if prerequisites are met
        /// </summary>
        public bool MeetsPrerequisites()
        {
            foreach (var prereq in prerequisiteStories)
            {
                if (!prereq.hasBeenHeard)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Mark story as heard
        /// </summary>
        public void MarkAsHeard()
        {
            hasBeenHeard = true;
            timesHeard++;
        }
    }

    /// <summary>
    /// Storyteller NPC that shares lore and background stories.
    /// Extends NPCInteractable with storytelling mechanics.
    /// </summary>
    public class StorytellerNPC : NPCInteractable
    {
        [Header("Storyteller Settings")]
        [Tooltip("Stories this storyteller knows")]
        public List<LoreStory> knownStories = new List<LoreStory>();

        [Tooltip("Storyteller specialty")]
        public string specialty = "Tales of Old";

        [Tooltip("Greeting dialogue when opening story menu")]
        public DialogueData storyGreeting;

        [Tooltip("Dialogue when no stories available")]
        public DialogueData noStoriesDialogue;

        [Tooltip("Dialogue after finishing a story")]
        public DialogueData storyEndDialogue;

        [Header("Storytelling Settings")]
        [Tooltip("Play background music during stories")]
        public bool playBackgroundMusic = true;

        [Tooltip("Story background music")]
        public AudioClip storyMusic;

        [Tooltip("Ambient sound during stories")]
        public AudioClip storyAmbience;

        [Tooltip("Grant experience for hearing new stories")]
        public bool grantExperience = true;

        [Header("Relationship Effects")]
        [Tooltip("Friendship gain per story heard")]
        public int friendshipGainPerStory = 3;

        [Tooltip("Unlock special stories at high friendship")]
        public bool hasSecretStories = true;

        [Tooltip("Friendship level to unlock secret stories")]
        public int secretStoriesLevel = 5;

        // Events
        public UnityEvent<LoreStory> OnStoryStarted;
        public UnityEvent<LoreStory> OnStoryCompleted;
        public UnityEvent OnStoryMenuOpened;
        public UnityEvent OnStoryMenuClosed;

        // State
        private LoreStory currentStory;
        private bool isTellingStory = false;

        /// <summary>
        /// Override interact to open story menu
        /// </summary>
        public override void Interact()
        {
            // Check if any stories available
            if (GetAvailableStories().Count == 0)
            {
                if (noStoriesDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(noStoriesDialogue);
                }
                else if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "I have no new stories to share right now.",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return;
            }

            // Play greeting dialogue if available
            if (storyGreeting != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(storyGreeting);
            }

            // Open story menu UI
            OpenStoryMenu();
        }

        /// <summary>
        /// Open story menu UI
        /// </summary>
        public virtual void OpenStoryMenu()
        {
            if (UI.StorytellerUI.Instance != null)
            {
                UI.StorytellerUI.Instance.OpenStoryMenu(this);
                OnStoryMenuOpened?.Invoke();
            }
            else
            {
                Debug.LogWarning("[StorytellerNPC] StorytellerUI not found!");
            }
        }

        /// <summary>
        /// Close story menu UI
        /// </summary>
        public virtual void CloseStoryMenu()
        {
            if (UI.StorytellerUI.Instance != null)
            {
                UI.StorytellerUI.Instance.CloseStoryMenu();
                OnStoryMenuClosed?.Invoke();
            }
        }

        /// <summary>
        /// Tell a story
        /// </summary>
        public bool TellStory(LoreStory story)
        {
            if (story == null || !story.IsAvailable())
            {
                Debug.LogWarning("[StorytellerNPC] Story not available!");
                return false;
            }

            if (isTellingStory)
            {
                Debug.LogWarning("[StorytellerNPC] Already telling a story!");
                return false;
            }

            // Check friendship requirement
            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }

                if (friendshipLevel < story.requiredFriendship)
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Need friendship level {story.requiredFriendship}!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }
            }

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < story.requiredLevel)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Requires level {story.requiredLevel}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Check prerequisites
            if (!story.MeetsPrerequisites())
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "You must hear other stories first.",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            currentStory = story;
            isTellingStory = true;

            // Play background music
            if (playBackgroundMusic && AudioManager.Instance != null)
            {
                if (storyMusic != null)
                {
                    AudioManager.Instance.PlayMusic(storyMusic);
                }

                if (storyAmbience != null)
                {
                    AudioManager.Instance.PlayAmbience(storyAmbience);
                }
            }

            // Start story dialogue
            if (story.storyDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(story.storyDialogue);

                // Subscribe to dialogue end event
                DialogueManager.OnDialogueEnded += OnStoryDialogueEnded;
            }
            else
            {
                Debug.LogWarning($"[StorytellerNPC] Story '{story.storyTitle}' has no dialogue!");
                CompleteStory(story);
            }

            OnStoryStarted?.Invoke(story);

            return true;
        }

        /// <summary>
        /// Called when story dialogue ends
        /// </summary>
        private void OnStoryDialogueEnded(DialogueData dialogue)
        {
            if (currentStory != null && currentStory.storyDialogue == dialogue)
            {
                DialogueManager.OnDialogueEnded -= OnStoryDialogueEnded;
                CompleteStory(currentStory);
            }
        }

        /// <summary>
        /// Complete a story
        /// </summary>
        private void CompleteStory(LoreStory story)
        {
            if (story == null)
                return;

            // Mark as heard
            story.MarkAsHeard();

            // Grant experience
            if (grantExperience && story.experienceReward > 0 && PlayerStats.Instance != null)
            {
                PlayerStats.Instance.AddExperience(story.experienceReward);

                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"+{story.experienceReward} XP",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.cyan
                    );
                }
            }

            // Unlock quest if specified
            if (story.unlocksQuest != null && Quest.QuestManager.Instance != null)
            {
                Quest.QuestManager.Instance.UnlockQuest(story.unlocksQuest);

                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"New Quest Unlocked: {story.unlocksQuest.questName}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
            }

            // Unlock location if specified
            if (!string.IsNullOrEmpty(story.unlocksLocation))
            {
                if (Map.MapSystem.Instance != null)
                {
                    // Add location marker to map
                    Debug.Log($"[StorytellerNPC] Unlocked location: {story.unlocksLocation}");
                }

                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"New Location Discovered: {story.unlocksLocation}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.green
                    );
                }
            }

            // Increase friendship
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.RelationshipSystem.Instance.ModifyFriendship(npcName, friendshipGainPerStory);
            }

            // Play story end dialogue
            if (storyEndDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(storyEndDialogue);
            }

            OnStoryCompleted?.Invoke(story);

            currentStory = null;
            isTellingStory = false;
        }

        /// <summary>
        /// Get available stories
        /// </summary>
        public List<LoreStory> GetAvailableStories()
        {
            List<LoreStory> available = new List<LoreStory>();

            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }
            }

            foreach (var story in knownStories)
            {
                if (!story.IsAvailable())
                    continue;

                // Check if secret story
                if (hasSecretStories && story.category == StoryCategory.Secret)
                {
                    if (friendshipLevel < secretStoriesLevel)
                        continue;
                }

                if (friendshipLevel >= story.requiredFriendship)
                {
                    available.Add(story);
                }
            }

            return available;
        }

        /// <summary>
        /// Get stories player can hear right now
        /// </summary>
        public List<LoreStory> GetHearableStories()
        {
            List<LoreStory> hearable = new List<LoreStory>();
            List<LoreStory> available = GetAvailableStories();

            foreach (var story in available)
            {
                if (story.MeetsPrerequisites())
                {
                    hearable.Add(story);
                }
            }

            return hearable;
        }

        /// <summary>
        /// Get stories by category
        /// </summary>
        public List<LoreStory> GetStoriesByCategory(StoryCategory category)
        {
            List<LoreStory> stories = new List<LoreStory>();
            List<LoreStory> available = GetAvailableStories();

            foreach (var story in available)
            {
                if (story.category == category)
                {
                    stories.Add(story);
                }
            }

            return stories;
        }

        /// <summary>
        /// Get story completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (knownStories.Count == 0)
                return 0f;

            int heardCount = 0;
            foreach (var story in knownStories)
            {
                if (story.hasBeenHeard)
                    heardCount++;
            }

            return (float)heardCount / knownStories.Count;
        }
    }
}
