using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;
using CozyGame.Dialogue;

namespace CozyGame.Social
{
    /// <summary>
    /// Friendship tier
    /// </summary>
    public enum FriendshipTier
    {
        Stranger,       // 0
        Acquaintance,   // 1-2
        Friend,         // 3-4
        GoodFriend,     // 5-6
        BestFriend,     // 7-8
        Soulmate        // 9-10 (marriage tier)
    }

    /// <summary>
    /// Relationship status
    /// </summary>
    public enum RelationshipStatus
    {
        Single,
        Dating,
        Engaged,
        Married
    }

    /// <summary>
    /// NPC relationship data
    /// </summary>
    [System.Serializable]
    public class NPCRelationship
    {
        [Header("NPC Info")]
        public string npcName;
        public string npcID;

        [Header("Friendship")]
        [Tooltip("Friendship level (0-10)")]
        public int friendshipLevel = 0;

        [Tooltip("Friendship points (0-100 per level)")]
        public int friendshipPoints = 0;

        [Tooltip("Points needed for next level")]
        public int pointsPerLevel = 100;

        [Header("Relationship")]
        [Tooltip("Can romance this NPC")]
        public bool isRomanceable = false;

        [Tooltip("Current relationship status")]
        public RelationshipStatus relationshipStatus = RelationshipStatus.Single;

        [Tooltip("Romance level (0-100)")]
        public int romanceLevel = 0;

        [Header("Interactions")]
        [Tooltip("Total gifts given")]
        public int giftsGiven = 0;

        [Tooltip("Total dialogues had")]
        public int dialoguesCompleted = 0;

        [Tooltip("Total quests completed for this NPC")]
        public int questsCompleted = 0;

        [Tooltip("Last interaction time")]
        public float lastInteractionTime = 0f;

        [Tooltip("Last gift time")]
        public float lastGiftTime = 0f;

        [Header("Preferences")]
        [Tooltip("Items this NPC loves (+15 points)")]
        public List<Item> lovedItems = new List<Item>();

        [Tooltip("Items this NPC likes (+8 points)")]
        public List<Item> likedItems = new List<Item>();

        [Tooltip("Items this NPC dislikes (-5 points)")]
        public List<Item> dislikedItems = new List<Item>();

        [Tooltip("Items this NPC hates (-10 points)")]
        public List<Item> hatedItems = new List<Item>();

        [Header("Unlocks")]
        [Tooltip("Friendship levels that unlock special dialogue")]
        public List<int> dialogueUnlockLevels = new List<int> { 2, 4, 6, 8 };

        [Tooltip("Special dialogues at friendship milestones")]
        public List<DialogueData> specialDialogues = new List<DialogueData>();

        [Tooltip("Quests unlocked at friendship levels")]
        public List<Quest.QuestData> unlockedQuests = new List<Quest.QuestData>();

        /// <summary>
        /// Get current friendship tier
        /// </summary>
        public FriendshipTier GetFriendshipTier()
        {
            if (friendshipLevel == 0) return FriendshipTier.Stranger;
            if (friendshipLevel <= 2) return FriendshipTier.Acquaintance;
            if (friendshipLevel <= 4) return FriendshipTier.Friend;
            if (friendshipLevel <= 6) return FriendshipTier.GoodFriend;
            if (friendshipLevel <= 8) return FriendshipTier.BestFriend;
            return FriendshipTier.Soulmate;
        }

        /// <summary>
        /// Get progress to next level (0-1)
        /// </summary>
        public float GetLevelProgress()
        {
            return (float)friendshipPoints / pointsPerLevel;
        }

        /// <summary>
        /// Get gift preference for item
        /// </summary>
        public int GetGiftValue(Item item)
        {
            if (lovedItems.Contains(item)) return 15;
            if (likedItems.Contains(item)) return 8;
            if (dislikedItems.Contains(item)) return -5;
            if (hatedItems.Contains(item)) return -10;
            return 3; // Neutral
        }

        /// <summary>
        /// Get gift reaction message
        /// </summary>
        public string GetGiftReaction(Item item)
        {
            if (lovedItems.Contains(item)) return "I love this! Thank you so much!";
            if (likedItems.Contains(item)) return "Oh, thank you! I like this.";
            if (dislikedItems.Contains(item)) return "Uh... thanks, I guess.";
            if (hatedItems.Contains(item)) return "I really don't like this...";
            return "Thank you for the gift!";
        }

        /// <summary>
        /// Check if can gift today
        /// </summary>
        public bool CanGiftToday()
        {
            // Can gift once per day
            float timeSinceLastGift = Time.time - lastGiftTime;
            float dayInSeconds = 86400f; // 24 hours

            // For testing, use shorter time (5 minutes)
            float cooldown = 300f; // 5 minutes

            return timeSinceLastGift >= cooldown;
        }

        /// <summary>
        /// Add friendship points
        /// </summary>
        public bool AddPoints(int points, out bool leveledUp)
        {
            leveledUp = false;

            if (friendshipLevel >= 10)
                return false; // Max level

            friendshipPoints += points;

            // Check for level up
            while (friendshipPoints >= pointsPerLevel && friendshipLevel < 10)
            {
                friendshipPoints -= pointsPerLevel;
                friendshipLevel++;
                leveledUp = true;
            }

            // Cap points at max level
            if (friendshipLevel >= 10)
            {
                friendshipPoints = 0;
            }

            return true;
        }
    }

    /// <summary>
    /// Save data for relationship system
    /// </summary>
    [System.Serializable]
    public class RelationshipSaveData
    {
        public List<NPCRelationship> relationships = new List<NPCRelationship>();
        public string marriedNPCID = "";
    }

    /// <summary>
    /// Relationship system singleton.
    /// Manages friendships, romance, and marriage with NPCs.
    /// </summary>
    public class RelationshipSystem : MonoBehaviour
    {
        public static RelationshipSystem Instance { get; private set; }

        [Header("Relationships")]
        [Tooltip("All NPC relationships")]
        public List<NPCRelationship> relationships = new List<NPCRelationship>();

        [Header("Marriage")]
        [Tooltip("Enable marriage system")]
        public bool marriageEnabled = true;

        [Tooltip("Minimum friendship level for marriage")]
        public int marriageFriendshipLevel = 10;

        [Tooltip("Minimum romance level for marriage")]
        public int marriageRomanceLevel = 75;

        [Tooltip("Currently married NPC")]
        public NPCRelationship marriedNPC;

        [Header("Settings")]
        [Tooltip("Daily gift limit per NPC")]
        public int giftsPerDay = 1;

        [Tooltip("Friendship decay enabled")]
        public bool friendshipDecayEnabled = false;

        [Tooltip("Days without interaction before decay")]
        public int daysBeforeDecay = 7;

        [Tooltip("Decay amount per day")]
        public int decayPerDay = 1;

        [Header("Events")]
        public UnityEvent<NPCRelationship> OnFriendshipLevelUp;
        public UnityEvent<NPCRelationship> OnRelationshipStatusChanged;
        public UnityEvent<NPCRelationship> OnMarriage;
        public UnityEvent<NPCRelationship, Item> OnGiftGiven;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Get relationship with NPC
        /// </summary>
        public NPCRelationship GetRelationship(string npcName)
        {
            return relationships.Find(r => r.npcName == npcName || r.npcID == npcName);
        }

        /// <summary>
        /// Get or create relationship
        /// </summary>
        public NPCRelationship GetOrCreateRelationship(string npcName, string npcID = "")
        {
            NPCRelationship relationship = GetRelationship(npcName);

            if (relationship == null)
            {
                relationship = new NPCRelationship
                {
                    npcName = npcName,
                    npcID = string.IsNullOrEmpty(npcID) ? npcName : npcID
                };
                relationships.Add(relationship);
            }

            return relationship;
        }

        /// <summary>
        /// Modify friendship
        /// </summary>
        public void ModifyFriendship(string npcName, int points)
        {
            NPCRelationship relationship = GetOrCreateRelationship(npcName);

            bool leveledUp;
            bool success = relationship.AddPoints(points, out leveledUp);

            if (success)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    string sign = points > 0 ? "+" : "";
                    Color color = points > 0 ? Color.green : Color.red;

                    FloatingTextManager.Instance.Show(
                        $"{npcName}: {sign}{points} friendship",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        color
                    );
                }

                if (leveledUp)
                {
                    OnFriendshipLevelUp?.Invoke(relationship);

                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Friendship with {npcName} increased to level {relationship.friendshipLevel}!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.yellow
                        );
                    }

                    // Play level up sound
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySound("friendship_levelup");
                    }

                    // Check for unlocks
                    CheckFriendshipUnlocks(relationship);
                }
            }
        }

        /// <summary>
        /// Give gift to NPC
        /// </summary>
        public bool GiveGift(string npcName, Item item)
        {
            if (item == null)
                return false;

            NPCRelationship relationship = GetOrCreateRelationship(npcName);

            // Check if can gift today
            if (!relationship.CanGiftToday())
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Already gave {npcName} a gift today!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            // Check if player has the item
            if (InventoryManager.Instance != null)
            {
                if (!InventoryManager.Instance.HasItem(item, 1))
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            "You don't have that item!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }

                // Remove item from inventory
                InventoryManager.Instance.RemoveItem(item, 1);
            }

            // Get gift value
            int points = relationship.GetGiftValue(item);
            string reaction = relationship.GetGiftReaction(item);

            // Apply points
            bool leveledUp;
            relationship.AddPoints(points, out leveledUp);

            // Update stats
            relationship.giftsGiven++;
            relationship.lastGiftTime = Time.time;

            // Show reaction
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    reaction,
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    points > 0 ? Color.green : Color.red
                );
            }

            // Play gift sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(points > 0 ? "gift_positive" : "gift_negative");
            }

            OnGiftGiven?.Invoke(relationship, item);

            if (leveledUp)
            {
                OnFriendshipLevelUp?.Invoke(relationship);
                CheckFriendshipUnlocks(relationship);
            }

            return true;
        }

        /// <summary>
        /// Start dating
        /// </summary>
        public bool StartDating(string npcName)
        {
            NPCRelationship relationship = GetRelationship(npcName);

            if (relationship == null || !relationship.isRomanceable)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Cannot date this NPC!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Check friendship requirement
            if (relationship.friendshipLevel < 5)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Need higher friendship to start dating!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            // Check if already in relationship
            if (relationship.relationshipStatus != RelationshipStatus.Single)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Already in a relationship!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            relationship.relationshipStatus = RelationshipStatus.Dating;

            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Now dating {npcName}!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.magenta
                );
            }

            OnRelationshipStatusChanged?.Invoke(relationship);

            return true;
        }

        /// <summary>
        /// Propose marriage
        /// </summary>
        public bool ProposeMarriage(string npcName)
        {
            if (!marriageEnabled)
                return false;

            NPCRelationship relationship = GetRelationship(npcName);

            if (relationship == null || !relationship.isRomanceable)
                return false;

            // Check if already married
            if (marriedNPC != null)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Already married!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Check requirements
            if (relationship.friendshipLevel < marriageFriendshipLevel)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need friendship level {marriageFriendshipLevel}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            if (relationship.romanceLevel < marriageRomanceLevel)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need romance level {marriageRomanceLevel}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            // Must be dating
            if (relationship.relationshipStatus != RelationshipStatus.Dating &&
                relationship.relationshipStatus != RelationshipStatus.Engaged)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Must be dating first!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            relationship.relationshipStatus = RelationshipStatus.Engaged;

            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"{npcName} said yes!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.magenta
                );
            }

            OnRelationshipStatusChanged?.Invoke(relationship);

            return true;
        }

        /// <summary>
        /// Get married
        /// </summary>
        public bool GetMarried(string npcName)
        {
            if (!marriageEnabled)
                return false;

            NPCRelationship relationship = GetRelationship(npcName);

            if (relationship == null || relationship.relationshipStatus != RelationshipStatus.Engaged)
                return false;

            relationship.relationshipStatus = RelationshipStatus.Married;
            marriedNPC = relationship;

            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Married to {npcName}!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.magenta
                );
            }

            // Play wedding bells
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("wedding_bells");
            }

            OnMarriage?.Invoke(relationship);
            OnRelationshipStatusChanged?.Invoke(relationship);

            return true;
        }

        /// <summary>
        /// Check for friendship unlocks
        /// </summary>
        private void CheckFriendshipUnlocks(NPCRelationship relationship)
        {
            // Unlock special dialogues
            if (relationship.dialogueUnlockLevels.Contains(relationship.friendshipLevel))
            {
                int index = relationship.dialogueUnlockLevels.IndexOf(relationship.friendshipLevel);
                if (index >= 0 && index < relationship.specialDialogues.Count)
                {
                    DialogueData dialogue = relationship.specialDialogues[index];
                    if (dialogue != null && DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.StartDialogue(dialogue);
                    }
                }
            }

            // Unlock quests
            foreach (var quest in relationship.unlockedQuests)
            {
                if (quest != null && Quest.QuestManager.Instance != null)
                {
                    // Check if quest has friendship requirement matching current level
                    Quest.QuestManager.Instance.UnlockQuest(quest);
                }
            }
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public RelationshipSaveData GetSaveData()
        {
            RelationshipSaveData data = new RelationshipSaveData
            {
                relationships = new List<NPCRelationship>(relationships),
                marriedNPCID = marriedNPC != null ? marriedNPC.npcID : ""
            };

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(RelationshipSaveData data)
        {
            if (data == null)
                return;

            relationships = new List<NPCRelationship>(data.relationships);

            if (!string.IsNullOrEmpty(data.marriedNPCID))
            {
                marriedNPC = relationships.Find(r => r.npcID == data.marriedNPCID);
            }

            Debug.Log($"[RelationshipSystem] Loaded {relationships.Count} relationships");
        }

        /// <summary>
        /// Get all relationships at or above friendship level
        /// </summary>
        public List<NPCRelationship> GetRelationshipsAboveLevel(int level)
        {
            return relationships.FindAll(r => r.friendshipLevel >= level);
        }

        /// <summary>
        /// Get all romanceable NPCs
        /// </summary>
        public List<NPCRelationship> GetRomanceableNPCs()
        {
            return relationships.FindAll(r => r.isRomanceable);
        }
    }
}
