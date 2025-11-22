using UnityEngine;
using CozyGame.Inventory;

namespace CozyGame.Mining
{
    /// <summary>
    /// Mining tool tier
    /// </summary>
    public enum ToolTier
    {
        Wooden = 0,     // Basic
        Stone = 1,      // Can mine copper/tin
        Bronze = 2,     // Can mine iron
        Iron = 3,       // Can mine silver/gold
        Steel = 4,      // Can mine mithril/titanium
        Mithril = 5,    // Can mine adamant
        Adamant = 6,    // Can mine runite
        Legendary = 7   // Can mine everything
    }

    /// <summary>
    /// Mining tool data (ScriptableObject for tool definitions).
    /// Create via: Right-click → Create → Cozy Game → Mining → Mining Tool
    /// </summary>
    [CreateAssetMenu(fileName = "New Mining Tool", menuName = "Cozy Game/Mining/Mining Tool", order = 11)]
    public class MiningToolData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Tool name")]
        public string toolName = "Pickaxe";

        [Tooltip("Tool description")]
        [TextArea(2, 3)]
        public string description = "A basic mining tool";

        [Tooltip("Tool icon")]
        public Sprite icon;

        [Tooltip("Tool tier (higher = better)")]
        public ToolTier tier = ToolTier.Wooden;

        [Header("Durability")]
        [Tooltip("Maximum durability")]
        public int maxDurability = 100;

        [Tooltip("Can be repaired")]
        public bool canRepair = true;

        [Tooltip("Repair cost per durability point")]
        public int repairCostPerPoint = 1;

        [Tooltip("Repair material item")]
        public Item repairMaterial;

        [Header("Mining Stats")]
        [Tooltip("Mining speed multiplier")]
        [Range(0.5f, 3f)]
        public float miningSpeedMultiplier = 1f;

        [Tooltip("Critical hit chance (0-1)")]
        [Range(0f, 1f)]
        public float criticalChance = 0.1f;

        [Tooltip("Critical hit damage multiplier")]
        [Range(1f, 5f)]
        public float criticalMultiplier = 2f;

        [Tooltip("Bonus drops per hit")]
        [Range(0f, 2f)]
        public float bonusDropMultiplier = 1f;

        [Header("Visual")]
        [Tooltip("Tool prefab (for world/hand)")]
        public GameObject toolPrefab;

        [Tooltip("Swing animation name")]
        public string swingAnimationName = "Mining_Swing";

        [Header("Audio")]
        [Tooltip("Equip sound")]
        public string equipSound = "tool_equip";

        [Tooltip("Swing sound")]
        public string swingSound = "pickaxe_swing";

        [Tooltip("Break sound")]
        public string breakSound = "tool_break";

        [Header("Crafting")]
        [Tooltip("Crafting recipe (optional)")]
        public Inventory.CraftingRecipe craftingRecipe;

        /// <summary>
        /// Get tier as integer
        /// </summary>
        public int GetTierLevel()
        {
            return (int)tier;
        }

        /// <summary>
        /// Calculate repair cost for given durability
        /// </summary>
        public int GetRepairCost(int durabilityToRestore)
        {
            if (!canRepair)
                return -1;

            return durabilityToRestore * repairCostPerPoint;
        }

        /// <summary>
        /// Can mine resource with this tool?
        /// </summary>
        public bool CanMineResource(MineableResource resource)
        {
            if (resource == null)
                return false;

            return GetTierLevel() >= resource.minToolTier;
        }
    }

    /// <summary>
    /// Mining tool instance component.
    /// Manages tool durability and usage.
    /// </summary>
    public class MiningTool : MonoBehaviour
    {
        [Header("Tool Data")]
        [Tooltip("Tool definition")]
        public MiningToolData toolData;

        [Header("Current State")]
        [Tooltip("Current durability")]
        public int currentDurability;

        [Tooltip("Is equipped")]
        public bool isEquipped = false;

        [Header("References")]
        [Tooltip("Tool visual model")]
        public GameObject toolModel;

        [Tooltip("Hand attachment point")]
        public Transform handAttachment;

        // State
        private bool isBroken = false;

        private void Start()
        {
            if (toolData != null)
            {
                currentDurability = toolData.maxDurability;
            }
        }

        /// <summary>
        /// Equip tool
        /// </summary>
        public void Equip()
        {
            isEquipped = true;

            // Show model
            if (toolModel != null)
            {
                toolModel.SetActive(true);
            }

            // Play equip sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(toolData.equipSound))
            {
                AudioManager.Instance.PlaySound(toolData.equipSound);
            }
        }

        /// <summary>
        /// Unequip tool
        /// </summary>
        public void Unequip()
        {
            isEquipped = false;

            // Hide model
            if (toolModel != null)
            {
                toolModel.SetActive(false);
            }
        }

        /// <summary>
        /// Use tool (swing/hit)
        /// </summary>
        public bool Use(int durabilityDamage = 1)
        {
            if (isBroken)
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Tool is broken!", transform.position, Color.red);
                }
                return false;
            }

            // Damage durability
            DamageDurability(durabilityDamage);

            // Play swing sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(toolData.swingSound))
            {
                AudioManager.Instance.PlaySound(toolData.swingSound);
            }

            return true;
        }

        /// <summary>
        /// Damage tool durability
        /// </summary>
        public void DamageDurability(int amount)
        {
            if (isBroken)
                return;

            currentDurability -= amount;

            // Check if broken
            if (currentDurability <= 0)
            {
                currentDurability = 0;
                OnToolBroken();
            }

            // Show durability warning
            if (currentDurability <= toolData.maxDurability * 0.1f && currentDurability > 0)
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Tool is almost broken!", transform.position, Color.yellow);
                }
            }
        }

        /// <summary>
        /// Called when tool breaks
        /// </summary>
        private void OnToolBroken()
        {
            isBroken = true;

            // Play break sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(toolData.breakSound))
            {
                AudioManager.Instance.PlaySound(toolData.breakSound);
            }

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show($"{toolData.toolName} broke!", transform.position, Color.red);
            }

            // Unequip
            Unequip();

            Debug.Log($"[MiningTool] {toolData.toolName} broke!");
        }

        /// <summary>
        /// Repair tool
        /// </summary>
        public bool Repair(int amount)
        {
            if (toolData == null || !toolData.canRepair)
                return false;

            // Calculate cost
            int cost = toolData.GetRepairCost(amount);

            // Check if player has repair materials
            if (toolData.repairMaterial != null)
            {
                int playerAmount = Inventory.InventorySystem.Instance.GetItemCount(toolData.repairMaterial.itemID);
                if (playerAmount < cost)
                {
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Need {cost}x {toolData.repairMaterial.itemName}!",
                            transform.position,
                            Color.red
                        );
                    }
                    return false;
                }

                // Consume materials
                Inventory.InventorySystem.Instance.RemoveItem(toolData.repairMaterial.itemID, cost);
            }

            // Restore durability
            currentDurability = Mathf.Min(currentDurability + amount, toolData.maxDurability);
            isBroken = false;

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show($"Repaired +{amount}", transform.position, Color.green);
            }

            Debug.Log($"[MiningTool] {toolData.toolName} repaired by {amount}");

            return true;
        }

        /// <summary>
        /// Fully repair tool
        /// </summary>
        public bool RepairFully()
        {
            int durabilityToRestore = toolData.maxDurability - currentDurability;
            return Repair(durabilityToRestore);
        }

        /// <summary>
        /// Get durability percentage (0-1)
        /// </summary>
        public float GetDurabilityPercent()
        {
            return toolData != null && toolData.maxDurability > 0 ? (float)currentDurability / toolData.maxDurability : 0f;
        }

        /// <summary>
        /// Is tool broken?
        /// </summary>
        public bool IsBroken()
        {
            return isBroken;
        }

        /// <summary>
        /// Get tool tier
        /// </summary>
        public int GetTier()
        {
            return toolData != null ? toolData.GetTierLevel() : 0;
        }

        /// <summary>
        /// Roll for critical hit
        /// </summary>
        public bool RollForCritical()
        {
            if (toolData == null)
                return false;

            return Random.value <= toolData.criticalChance;
        }

        /// <summary>
        /// Can mine resource?
        /// </summary>
        public bool CanMineResource(MineableResource resource)
        {
            if (toolData == null || resource == null)
                return false;

            return toolData.CanMineResource(resource);
        }
    }
}
