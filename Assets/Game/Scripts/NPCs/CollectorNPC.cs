using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;
using CozyGame.Economy;
using CozyGame.Dialogue;

namespace CozyGame.NPCs
{
    /// <summary>
    /// Collection request (trade items for reward)
    /// </summary>
    [System.Serializable]
    public class CollectionRequest
    {
        [Header("Request Info")]
        [Tooltip("Request name/title")]
        public string requestName = "Collection Request";

        [Tooltip("Description of what's being collected")]
        [TextArea(2, 4)]
        public string description = "Bring me these items...";

        [Tooltip("Is this a one-time request?")]
        public bool isOneTime = true;

        [Tooltip("Cooldown before can repeat (minutes)")]
        public float cooldownMinutes = 1440f; // 24 hours default

        [Header("Required Items")]
        [Tooltip("Items player must provide")]
        public List<ItemRequirement> requiredItems = new List<ItemRequirement>();

        [Header("Rewards")]
        [Tooltip("Currency rewards")]
        public List<CurrencyReward> currencyRewards = new List<CurrencyReward>();

        [Tooltip("Item rewards")]
        public List<ItemReward> itemRewards = new List<ItemReward>();

        [Tooltip("Experience reward")]
        public int experienceReward = 0;

        [Header("Requirements")]
        [Tooltip("Required friendship level")]
        public int requiredFriendship = 0;

        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Tooltip("Is this request available?")]
        public bool isAvailable = true;

        [Header("Dialogue")]
        [Tooltip("Dialogue when request is completed")]
        public DialogueData completionDialogue;

        // Runtime state
        [System.NonSerialized]
        public bool isCompleted = false;

        [System.NonSerialized]
        public float lastCompletionTime = -999999f;

        /// <summary>
        /// Check if request is available
        /// </summary>
        public bool IsAvailable()
        {
            if (!isAvailable)
                return false;

            if (isOneTime && isCompleted)
                return false;

            // Check cooldown
            if (cooldownMinutes > 0 && lastCompletionTime > 0)
            {
                float timeSinceCompletion = Time.time - lastCompletionTime;
                float cooldownSeconds = cooldownMinutes * 60f;

                if (timeSinceCompletion < cooldownSeconds)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if player has all required items
        /// </summary>
        public bool HasAllItems(out string missingItem)
        {
            missingItem = "";

            if (InventoryManager.Instance == null)
                return false;

            foreach (var req in requiredItems)
            {
                if (!InventoryManager.Instance.HasItem(req.item, req.quantity))
                {
                    missingItem = $"{req.item.itemName} ({req.quantity})";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Mark as completed
        /// </summary>
        public void Complete()
        {
            isCompleted = true;
            lastCompletionTime = Time.time;
        }
    }

    [System.Serializable]
    public class ItemRequirement
    {
        public Item item;
        public int quantity = 1;
    }

    [System.Serializable]
    public class CurrencyReward
    {
        public CurrencyType currencyType = CurrencyType.Gold;
        public int amount = 100;
    }

    [System.Serializable]
    public class ItemReward
    {
        public Item item;
        public int quantity = 1;
    }

    /// <summary>
    /// Collector NPC that trades specific items for rewards.
    /// Extends NPCInteractable with collection mechanics.
    /// </summary>
    public class CollectorNPC : NPCInteractable
    {
        [Header("Collector Settings")]
        [Tooltip("Collection requests this NPC offers")]
        public List<CollectionRequest> collectionRequests = new List<CollectionRequest>();

        [Tooltip("Collector specialization (what they collect)")]
        public string collectionType = "Rare Items";

        [Tooltip("Greeting dialogue when opening collection UI")]
        public DialogueData collectionGreeting;

        [Tooltip("Dialogue when player doesn't have required items")]
        public DialogueData missingItemsDialogue;

        [Tooltip("Dialogue when no requests available")]
        public DialogueData noRequestsDialogue;

        [Header("Relationship Bonuses")]
        [Tooltip("Bonus reward percentage per friendship level")]
        [Range(0f, 0.1f)]
        public float bonusRewardPerLevel = 0.05f; // 5% per level

        [Tooltip("Max bonus percentage")]
        [Range(0f, 1f)]
        public float maxBonus = 0.5f; // 50% max

        // Events
        public UnityEvent<CollectionRequest> OnRequestCompleted;
        public UnityEvent OnCollectionOpened;
        public UnityEvent OnCollectionClosed;

        /// <summary>
        /// Override interact to open collection UI
        /// </summary>
        public override void Interact()
        {
            // Check if any requests available
            if (GetAvailableRequests().Count == 0)
            {
                if (noRequestsDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(noRequestsDialogue);
                }
                else if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "I don't have any requests right now.",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return;
            }

            // Play greeting dialogue if available
            if (collectionGreeting != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(collectionGreeting);
            }

            // Open collection UI
            OpenCollection();
        }

        /// <summary>
        /// Open collection UI
        /// </summary>
        public virtual void OpenCollection()
        {
            if (UI.CollectionUI.Instance != null)
            {
                UI.CollectionUI.Instance.OpenCollection(this);
                OnCollectionOpened?.Invoke();
            }
            else
            {
                Debug.LogWarning("[CollectorNPC] CollectionUI not found!");
            }
        }

        /// <summary>
        /// Close collection UI
        /// </summary>
        public virtual void CloseCollection()
        {
            if (UI.CollectionUI.Instance != null)
            {
                UI.CollectionUI.Instance.CloseCollection();
                OnCollectionClosed?.Invoke();
            }
        }

        /// <summary>
        /// Complete collection request
        /// </summary>
        public bool CompleteRequest(CollectionRequest request)
        {
            if (request == null || !request.IsAvailable())
            {
                Debug.LogWarning("[CollectorNPC] Request not available!");
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

                if (friendshipLevel < request.requiredFriendship)
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Need friendship level {request.requiredFriendship}!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }
            }

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < request.requiredLevel)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Requires level {request.requiredLevel}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Check if player has all required items
            string missingItem;
            if (!request.HasAllItems(out missingItem))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Missing: {missingItem}",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }

                if (missingItemsDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(missingItemsDialogue);
                }

                return false;
            }

            // Remove required items from inventory
            if (InventoryManager.Instance != null)
            {
                foreach (var req in request.requiredItems)
                {
                    InventoryManager.Instance.RemoveItem(req.item, req.quantity);
                }
            }

            // Calculate bonus multiplier
            float bonusMultiplier = GetRewardBonus();

            // Give currency rewards
            foreach (var reward in request.currencyRewards)
            {
                int amount = Mathf.RoundToInt(reward.amount * (1f + bonusMultiplier));
                CurrencyManager.Instance.AddCurrency(reward.currencyType, amount);

                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"+{amount} {reward.currencyType}",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
            }

            // Give item rewards
            if (InventoryManager.Instance != null)
            {
                foreach (var reward in request.itemRewards)
                {
                    int quantity = Mathf.RoundToInt(reward.quantity * (1f + bonusMultiplier));
                    InventoryManager.Instance.AddItem(reward.item, quantity);

                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Received {quantity}x {reward.item.itemName}",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.green
                        );
                    }
                }
            }

            // Give experience
            if (request.experienceReward > 0 && PlayerStats.Instance != null)
            {
                int exp = Mathf.RoundToInt(request.experienceReward * (1f + bonusMultiplier));
                PlayerStats.Instance.AddExperience(exp);
            }

            // Play completion sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("quest_complete");
            }

            // Play completion dialogue
            if (request.completionDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(request.completionDialogue);
            }

            // Mark as completed
            request.Complete();

            OnRequestCompleted?.Invoke(request);

            // Increase friendship
            if (Social.RelationshipSystem.Instance != null)
            {
                int friendshipGain = 10; // Base gain
                Social.RelationshipSystem.Instance.ModifyFriendship(npcName, friendshipGain);
            }

            return true;
        }

        /// <summary>
        /// Get reward bonus from relationship
        /// </summary>
        public float GetRewardBonus()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            float bonus = relationship.friendshipLevel * bonusRewardPerLevel;
            return Mathf.Min(bonus, maxBonus);
        }

        /// <summary>
        /// Get available requests
        /// </summary>
        public List<CollectionRequest> GetAvailableRequests()
        {
            List<CollectionRequest> available = new List<CollectionRequest>();

            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }
            }

            foreach (var request in collectionRequests)
            {
                if (request.IsAvailable() && friendshipLevel >= request.requiredFriendship)
                {
                    available.Add(request);
                }
            }

            return available;
        }

        /// <summary>
        /// Get requests player can complete right now
        /// </summary>
        public List<CollectionRequest> GetCompletableRequests()
        {
            List<CollectionRequest> completable = new List<CollectionRequest>();
            List<CollectionRequest> available = GetAvailableRequests();

            foreach (var request in available)
            {
                string missingItem;
                if (request.HasAllItems(out missingItem))
                {
                    completable.Add(request);
                }
            }

            return completable;
        }
    }
}
