using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;
using CozyGame.Economy;
using CozyGame.Dialogue;
using CozyGame.Mining;

namespace CozyGame.NPCs
{
    /// <summary>
    /// Upgrade tier
    /// </summary>
    public enum UpgradeTier
    {
        Tier1,
        Tier2,
        Tier3,
        Tier4,
        Tier5,
        Max
    }

    /// <summary>
    /// Upgrade recipe for items/tools
    /// </summary>
    [System.Serializable]
    public class UpgradeRecipe
    {
        [Header("Recipe Info")]
        [Tooltip("Upgrade name")]
        public string upgradeName = "Upgrade";

        [Tooltip("Description of upgrade")]
        [TextArea(2, 3)]
        public string description = "Improves item performance...";

        [Header("Input")]
        [Tooltip("Item to upgrade")]
        public Item baseItem;

        [Tooltip("Required materials")]
        public List<ItemRequirement> materials = new List<ItemRequirement>();

        [Header("Output")]
        [Tooltip("Resulting upgraded item")]
        public Item upgradedItem;

        [Tooltip("Upgrade tier")]
        public UpgradeTier tier = UpgradeTier.Tier1;

        [Header("Cost")]
        [Tooltip("Upgrade fee")]
        public int upgradeCost = 500;

        [Tooltip("Currency type")]
        public CurrencyType currencyType = CurrencyType.Gold;

        [Header("Requirements")]
        [Tooltip("Required friendship level")]
        public int requiredFriendship = 0;

        [Tooltip("Required player level")]
        public int requiredLevel = 5;

        [Tooltip("Is this upgrade available?")]
        public bool isAvailable = true;

        [Header("Upgrade Time")]
        [Tooltip("Time to complete upgrade (seconds)")]
        public float upgradeTime = 30f;

        [Tooltip("Can instant complete with premium currency")]
        public bool canInstantComplete = true;

        [Tooltip("Instant complete cost (gems)")]
        public int instantCompleteCost = 10;

        [Header("Success Chance")]
        [Tooltip("Base success chance (0-1)")]
        [Range(0f, 1f)]
        public float successChance = 0.9f;

        [Tooltip("Chance to get bonus effect")]
        [Range(0f, 0.5f)]
        public float bonusChance = 0.1f;

        [Tooltip("Bonus effect description")]
        public string bonusEffect = "+5% additional stats";

        [Header("Dialogue")]
        [Tooltip("Dialogue when upgrade succeeds")]
        public DialogueData successDialogue;

        [Tooltip("Dialogue when upgrade fails")]
        public DialogueData failureDialogue;

        /// <summary>
        /// Check if player has all materials
        /// </summary>
        public bool HasAllMaterials(out string missingMaterial)
        {
            missingMaterial = "";

            if (InventoryManager.Instance == null)
                return false;

            // Check base item
            if (!InventoryManager.Instance.HasItem(baseItem, 1))
            {
                missingMaterial = baseItem.itemName;
                return false;
            }

            // Check materials
            foreach (var mat in materials)
            {
                if (!InventoryManager.Instance.HasItem(mat.item, mat.quantity))
                {
                    missingMaterial = $"{mat.item.itemName} ({mat.quantity})";
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Active upgrade in progress
    /// </summary>
    [System.Serializable]
    public class ActiveUpgrade
    {
        public UpgradeRecipe recipe;
        public float startTime;
        public float completionTime;
        public bool hasBonus;

        public float GetProgress()
        {
            float elapsed = Time.time - startTime;
            float duration = completionTime - startTime;
            return Mathf.Clamp01(elapsed / duration);
        }

        public bool IsComplete()
        {
            return Time.time >= completionTime;
        }

        public float GetRemainingTime()
        {
            return Mathf.Max(0f, completionTime - Time.time);
        }
    }

    /// <summary>
    /// Blacksmith NPC that upgrades tools and equipment.
    /// Extends NPCInteractable with upgrade/repair functionality.
    /// </summary>
    public class BlacksmithNPC : NPCInteractable
    {
        [Header("Blacksmith Settings")]
        [Tooltip("Upgrade recipes this blacksmith knows")]
        public List<UpgradeRecipe> upgradeRecipes = new List<UpgradeRecipe>();

        [Tooltip("Blacksmith specialization")]
        public string specialization = "Master Smith";

        [Tooltip("Greeting dialogue when opening forge")]
        public DialogueData forgeGreeting;

        [Tooltip("Dialogue when upgrade is ready")]
        public DialogueData upgradeReadyDialogue;

        [Tooltip("Dialogue when materials missing")]
        public DialogueData missingMaterialsDialogue;

        [Header("Repair Services")]
        [Tooltip("Can repair damaged tools")]
        public bool offersRepair = true;

        [Tooltip("Repair cost per durability point")]
        public float repairCostPerPoint = 5f;

        [Tooltip("Repair discount per friendship level")]
        [Range(0f, 0.1f)]
        public float repairDiscountPerLevel = 0.02f;

        [Header("Upgrade Queue")]
        [Tooltip("Max simultaneous upgrades")]
        public int maxUpgradeSlots = 3;

        [Tooltip("Current active upgrades")]
        public List<ActiveUpgrade> activeUpgrades = new List<ActiveUpgrade>();

        [Header("Relationship Bonuses")]
        [Tooltip("Upgrade cost discount per friendship level")]
        [Range(0f, 0.1f)]
        public float discountPerLevel = 0.03f;

        [Tooltip("Success chance bonus per friendship level")]
        [Range(0f, 0.02f)]
        public float successBonusPerLevel = 0.01f; // 1% per level

        [Tooltip("Upgrade time reduction per friendship level")]
        [Range(0f, 0.05f)]
        public float timeReductionPerLevel = 0.02f; // 2% per level

        // Events
        public UnityEvent<UpgradeRecipe> OnUpgradeStarted;
        public UnityEvent<UpgradeRecipe, bool> OnUpgradeCompleted; // recipe, hasBonus
        public UnityEvent<Item> OnItemRepaired;
        public UnityEvent OnForgeOpened;
        public UnityEvent OnForgeClosed;

        protected virtual void Update()
        {
            base.Update();

            // Check for completed upgrades
            CheckCompletedUpgrades();
        }

        /// <summary>
        /// Override interact to open forge UI
        /// </summary>
        public override void Interact()
        {
            // Play greeting dialogue if available
            if (forgeGreeting != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(forgeGreeting);
            }

            // Open forge UI
            OpenForge();
        }

        /// <summary>
        /// Open forge UI
        /// </summary>
        public virtual void OpenForge()
        {
            if (UI.ForgeUI.Instance != null)
            {
                UI.ForgeUI.Instance.OpenForge(this);
                OnForgeOpened?.Invoke();
            }
            else
            {
                Debug.LogWarning("[BlacksmithNPC] ForgeUI not found!");
            }
        }

        /// <summary>
        /// Close forge UI
        /// </summary>
        public virtual void CloseForge()
        {
            if (UI.ForgeUI.Instance != null)
            {
                UI.ForgeUI.Instance.CloseForge();
                OnForgeClosed?.Invoke();
            }
        }

        /// <summary>
        /// Start upgrade
        /// </summary>
        public bool StartUpgrade(UpgradeRecipe recipe)
        {
            if (recipe == null || !recipe.isAvailable)
            {
                Debug.LogWarning("[BlacksmithNPC] Recipe not available!");
                return false;
            }

            // Check upgrade slots
            if (activeUpgrades.Count >= maxUpgradeSlots)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Forge is full! Wait for current upgrades to finish.",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
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

                if (friendshipLevel < recipe.requiredFriendship)
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Need friendship level {recipe.requiredFriendship}!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }
            }

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < recipe.requiredLevel)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Requires level {recipe.requiredLevel}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Check materials
            string missingMaterial;
            if (!recipe.HasAllMaterials(out missingMaterial))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Missing: {missingMaterial}",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }

                if (missingMaterialsDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(missingMaterialsDialogue);
                }

                return false;
            }

            // Calculate cost with discount
            int finalCost = CalculateUpgradeCost(recipe);

            // Check if can afford
            if (!CurrencyManager.Instance.HasCurrency(recipe.currencyType, finalCost))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need {finalCost} {recipe.currencyType}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Remove materials and cost
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(recipe.baseItem, 1);

                foreach (var mat in recipe.materials)
                {
                    InventoryManager.Instance.RemoveItem(mat.item, mat.quantity);
                }
            }

            CurrencyManager.Instance.RemoveCurrency(recipe.currencyType, finalCost);

            // Calculate upgrade time with bonuses
            float upgradeTime = CalculateUpgradeTime(recipe);

            // Roll for bonus
            float bonusChance = recipe.bonusChance;
            bool hasBonus = Random.value <= bonusChance;

            // Create active upgrade
            ActiveUpgrade upgrade = new ActiveUpgrade
            {
                recipe = recipe,
                startTime = Time.time,
                completionTime = Time.time + upgradeTime,
                hasBonus = hasBonus
            };

            activeUpgrades.Add(upgrade);

            // Show message
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Upgrading {recipe.baseItem.itemName}... ({upgradeTime:F0}s)",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.yellow
                );
            }

            // Play forge sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("forge_start");
            }

            OnUpgradeStarted?.Invoke(recipe);

            return true;
        }

        /// <summary>
        /// Instant complete upgrade
        /// </summary>
        public bool InstantCompleteUpgrade(ActiveUpgrade upgrade)
        {
            if (upgrade == null || !activeUpgrades.Contains(upgrade))
                return false;

            if (!upgrade.recipe.canInstantComplete)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "This upgrade cannot be rushed!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Check if has gems
            if (!CurrencyManager.Instance.HasCurrency(CurrencyType.Gems, upgrade.recipe.instantCompleteCost))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need {upgrade.recipe.instantCompleteCost} Gems!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Pay gems
            CurrencyManager.Instance.RemoveCurrency(CurrencyType.Gems, upgrade.recipe.instantCompleteCost);

            // Complete immediately
            upgrade.completionTime = Time.time;

            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    "Upgrade completed instantly!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.cyan
                );
            }

            return true;
        }

        /// <summary>
        /// Check for completed upgrades
        /// </summary>
        private void CheckCompletedUpgrades()
        {
            List<ActiveUpgrade> completed = new List<ActiveUpgrade>();

            foreach (var upgrade in activeUpgrades)
            {
                if (upgrade.IsComplete())
                {
                    completed.Add(upgrade);
                }
            }

            foreach (var upgrade in completed)
            {
                CompleteUpgrade(upgrade);
            }
        }

        /// <summary>
        /// Complete upgrade
        /// </summary>
        private void CompleteUpgrade(ActiveUpgrade upgrade)
        {
            if (upgrade == null)
                return;

            // Calculate success chance
            float successChance = CalculateSuccessChance(upgrade.recipe);

            bool success = Random.value <= successChance;

            if (success)
            {
                // Add upgraded item to inventory
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(upgrade.recipe.upgradedItem, 1);
                }

                // Show success message
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    string message = $"{upgrade.recipe.upgradeName} complete!";
                    if (upgrade.hasBonus)
                    {
                        message += $" (Bonus: {upgrade.recipe.bonusEffect})";
                    }

                    FloatingTextManager.Instance.Show(
                        message,
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.green
                    );
                }

                // Play success dialogue
                if (upgrade.recipe.successDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(upgrade.recipe.successDialogue);
                }
                else if (upgradeReadyDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(upgradeReadyDialogue);
                }

                // Play success sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("upgrade_success");
                }
            }
            else
            {
                // Upgrade failed - return base item (or nothing)
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Upgrade failed! Item lost...",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }

                // Play failure dialogue
                if (upgrade.recipe.failureDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(upgrade.recipe.failureDialogue);
                }

                // Play failure sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("upgrade_fail");
                }
            }

            OnUpgradeCompleted?.Invoke(upgrade.recipe, upgrade.hasBonus);

            // Remove from active upgrades
            activeUpgrades.Remove(upgrade);

            // Increase friendship
            if (Social.RelationshipSystem.Instance != null)
            {
                int friendshipGain = success ? 5 : 2; // Less if failed
                Social.RelationshipSystem.Instance.ModifyFriendship(npcName, friendshipGain);
            }
        }

        /// <summary>
        /// Repair item
        /// </summary>
        public bool RepairItem(MiningTool tool)
        {
            if (!offersRepair || tool == null)
                return false;

            if (tool.currentDurability >= tool.toolData.maxDurability)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Tool doesn't need repair!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            // Calculate repair cost
            int repairCost = CalculateRepairCost(tool);

            // Check if can afford
            if (!CurrencyManager.Instance.HasCurrency(CurrencyType.Gold, repairCost))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need {repairCost} Gold for repair!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Pay for repair
            CurrencyManager.Instance.RemoveCurrency(CurrencyType.Gold, repairCost);

            // Repair tool
            tool.currentDurability = tool.toolData.maxDurability;

            // Show message
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"{tool.toolData.toolName} repaired!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.green
                );
            }

            // Play repair sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("repair");
            }

            OnItemRepaired?.Invoke(tool.toolData);

            return true;
        }

        /// <summary>
        /// Calculate upgrade cost with discounts
        /// </summary>
        public int CalculateUpgradeCost(UpgradeRecipe recipe)
        {
            float baseCost = recipe.upgradeCost;

            // Apply friendship discount
            float discount = GetRelationshipDiscount();
            float finalCost = baseCost * (1f - discount);

            return Mathf.RoundToInt(finalCost);
        }

        /// <summary>
        /// Calculate upgrade time with bonuses
        /// </summary>
        public float CalculateUpgradeTime(UpgradeRecipe recipe)
        {
            float baseTime = recipe.upgradeTime;

            // Apply friendship time reduction
            float timeReduction = GetTimeReduction();
            float finalTime = baseTime * (1f - timeReduction);

            return finalTime;
        }

        /// <summary>
        /// Calculate success chance with bonuses
        /// </summary>
        public float CalculateSuccessChance(UpgradeRecipe recipe)
        {
            float baseChance = recipe.successChance;

            // Apply friendship success bonus
            float successBonus = GetSuccessBonus();
            float finalChance = Mathf.Min(baseChance + successBonus, 1f);

            return finalChance;
        }

        /// <summary>
        /// Calculate repair cost
        /// </summary>
        public int CalculateRepairCost(MiningTool tool)
        {
            int durabilityLost = tool.toolData.maxDurability - tool.currentDurability;
            float baseCost = durabilityLost * repairCostPerPoint;

            // Apply friendship discount
            float discount = GetRepairDiscount();
            float finalCost = baseCost * (1f - discount);

            return Mathf.RoundToInt(finalCost);
        }

        /// <summary>
        /// Get relationship discount
        /// </summary>
        public float GetRelationshipDiscount()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            return relationship.friendshipLevel * discountPerLevel;
        }

        /// <summary>
        /// Get repair discount
        /// </summary>
        public float GetRepairDiscount()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            return relationship.friendshipLevel * repairDiscountPerLevel;
        }

        /// <summary>
        /// Get time reduction
        /// </summary>
        public float GetTimeReduction()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            return relationship.friendshipLevel * timeReductionPerLevel;
        }

        /// <summary>
        /// Get success bonus
        /// </summary>
        public float GetSuccessBonus()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            return relationship.friendshipLevel * successBonusPerLevel;
        }

        /// <summary>
        /// Get available upgrades
        /// </summary>
        public List<UpgradeRecipe> GetAvailableUpgrades()
        {
            List<UpgradeRecipe> available = new List<UpgradeRecipe>();

            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }
            }

            foreach (var recipe in upgradeRecipes)
            {
                if (recipe.isAvailable && friendshipLevel >= recipe.requiredFriendship)
                {
                    available.Add(recipe);
                }
            }

            return available;
        }
    }
}
