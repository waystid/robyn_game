using UnityEngine;
using System.Collections.Generic;
using CozyGame.Inventory;
using CozyGame.Economy;

namespace CozyGame.Building
{
    /// <summary>
    /// Building category types
    /// </summary>
    public enum BuildingCategory
    {
        Housing,        // Houses, rooms
        Farming,        // Garden plots, greenhouses
        Crafting,       // Workbenches, forges
        Decoration,     // Statues, flowers, lights
        Storage,        // Chests, warehouses
        Utility,        // Wells, fences, paths
        Furniture,      // Tables, chairs, beds
        Custom          // Custom category
    }

    /// <summary>
    /// Building size enum
    /// </summary>
    public enum BuildingSize
    {
        Small,      // 1x1
        Medium,     // 2x2 or 3x3
        Large,      // 4x4 or larger
        Custom      // Custom size
    }

    /// <summary>
    /// Building placement requirements
    /// </summary>
    [System.Serializable]
    public class BuildingRequirements
    {
        [Header("Level Requirements")]
        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Tooltip("Required quest ID (optional)")]
        public string requiredQuestID = "";

        [Header("Placement Constraints")]
        [Tooltip("Can only be placed indoors")]
        public bool indoorOnly = false;

        [Tooltip("Can only be placed outdoors")]
        public bool outdoorOnly = false;

        [Tooltip("Requires flat ground")]
        public bool requiresFlatGround = true;

        [Tooltip("Requires water nearby")]
        public bool requiresWaterNearby = false;

        [Tooltip("Water proximity radius (meters)")]
        public float waterProximityRadius = 10f;

        [Tooltip("Minimum distance from other buildings")]
        public float minDistanceFromOthers = 0f;

        [Tooltip("Can be placed on existing floors")]
        public bool canPlaceOnFloors = true;

        [Tooltip("Can be placed on terrain")]
        public bool canPlaceOnTerrain = true;
    }

    /// <summary>
    /// Resource cost entry
    /// </summary>
    [System.Serializable]
    public class ResourceCost
    {
        [Tooltip("Required item")]
        public Item item;

        [Tooltip("Quantity required")]
        public int quantity = 1;

        [Tooltip("Return on demolish")]
        public bool returnOnDemolish = true;

        [Tooltip("Return percentage (0-1)")]
        [Range(0f, 1f)]
        public float returnPercentage = 0.5f;
    }

    /// <summary>
    /// Currency cost entry
    /// </summary>
    [System.Serializable]
    public class CurrencyCost
    {
        [Tooltip("Currency type")]
        public CurrencyType currencyType = CurrencyType.Gold;

        [Tooltip("Amount required")]
        public int amount = 100;
    }

    /// <summary>
    /// Building upgrade tier
    /// </summary>
    [System.Serializable]
    public class BuildingUpgradeTier
    {
        [Header("Upgrade Info")]
        [Tooltip("Tier name")]
        public string tierName = "Tier 1";

        [Tooltip("Tier description")]
        [TextArea(2, 3)]
        public string description = "Basic upgrade";

        [Tooltip("Visual prefab for this tier")]
        public GameObject prefab;

        [Header("Costs")]
        [Tooltip("Resource costs")]
        public ResourceCost[] resourceCosts;

        [Tooltip("Currency costs")]
        public CurrencyCost[] currencyCosts;

        [Tooltip("Build time (seconds)")]
        public float buildTime = 5f;

        [Header("Bonuses")]
        [Tooltip("Storage capacity bonus")]
        public int storageBonusSlots = 0;

        [Tooltip("Production speed multiplier")]
        [Range(1f, 5f)]
        public float productionSpeedMultiplier = 1f;

        [Tooltip("Quality bonus")]
        [Range(0f, 1f)]
        public float qualityBonus = 0f;

        [Tooltip("Unlock new functionality")]
        public bool unlocksNewFeature = false;

        [Tooltip("Feature description")]
        public string featureDescription = "";
    }

    /// <summary>
    /// Building functionality
    /// </summary>
    [System.Serializable]
    public class BuildingFunctionality
    {
        [Header("Storage")]
        [Tooltip("Has storage functionality")]
        public bool hasStorage = false;

        [Tooltip("Storage slots")]
        public int storageSlots = 10;

        [Header("Production")]
        [Tooltip("Produces items over time")]
        public bool producesItems = false;

        [Tooltip("Production item")]
        public Item productionItem;

        [Tooltip("Production interval (seconds)")]
        public float productionInterval = 60f;

        [Tooltip("Production quantity per interval")]
        public int productionQuantity = 1;

        [Header("Crafting")]
        [Tooltip("Functions as crafting station")]
        public bool isCraftingStation = false;

        [Tooltip("Crafting station type")]
        public string craftingStationType = "";

        [Header("Farming")]
        [Tooltip("Functions as plant plot")]
        public bool isPlantPlot = false;

        [Tooltip("Number of plant slots")]
        public int plantSlots = 1;

        [Header("Interaction")]
        [Tooltip("Can be interacted with")]
        public bool canInteract = false;

        [Tooltip("Interaction prompt")]
        public string interactionPrompt = "Use";

        [Header("Living Space")]
        [Tooltip("Provides sleeping/resting")]
        public bool providesSleep = false;

        [Tooltip("Rest quality (0-1)")]
        [Range(0f, 1f)]
        public float restQuality = 0.5f;
    }

    /// <summary>
    /// Building data ScriptableObject.
    /// Defines building properties, costs, and upgrade paths.
    /// Create instances via: Right-click → Create → Cozy Game → Building → Building Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Building", menuName = "Cozy Game/Building/Building Data", order = 10)]
    public class BuildingData : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Building name")]
        public string buildingName = "New Building";

        [Tooltip("Unique building ID")]
        public string buildingID;

        [Tooltip("Building description")]
        [TextArea(3, 5)]
        public string description = "A structure that can be placed in the world...";

        [Tooltip("Building icon")]
        public Sprite icon;

        [Tooltip("Building category")]
        public BuildingCategory category = BuildingCategory.Housing;

        [Header("Size")]
        [Tooltip("Building size category")]
        public BuildingSize sizeCategory = BuildingSize.Medium;

        [Tooltip("Grid size (width x depth)")]
        public Vector2Int gridSize = new Vector2Int(2, 2);

        [Tooltip("Building height (for clearance checks)")]
        public float buildingHeight = 3f;

        [Header("Visual")]
        [Tooltip("Base building prefab (Tier 0)")]
        public GameObject basePrefab;

        [Tooltip("Preview material (semi-transparent)")]
        public Material previewMaterial;

        [Tooltip("Valid placement color")]
        public Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);

        [Tooltip("Invalid placement color")]
        public Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);

        [Header("Costs")]
        [Tooltip("Resource costs to build")]
        public ResourceCost[] buildCosts;

        [Tooltip("Currency costs to build")]
        public CurrencyCost[] currencyCosts;

        [Tooltip("Build time (seconds)")]
        public float buildTime = 10f;

        [Header("Requirements")]
        [Tooltip("Placement requirements")]
        public BuildingRequirements requirements;

        [Header("Upgrades")]
        [Tooltip("Can be upgraded")]
        public bool canUpgrade = true;

        [Tooltip("Upgrade tiers")]
        public BuildingUpgradeTier[] upgradeTiers;

        [Header("Functionality")]
        [Tooltip("Building functionality")]
        public BuildingFunctionality functionality;

        [Header("Demolition")]
        [Tooltip("Can be demolished")]
        public bool canDemolish = true;

        [Tooltip("Demolish time (seconds)")]
        public float demolishTime = 2f;

        [Tooltip("Refund percentage of costs")]
        [Range(0f, 1f)]
        public float demolishRefundPercentage = 0.5f;

        [Header("Advanced")]
        [Tooltip("Rotation snap angle (degrees)")]
        public float rotationSnapAngle = 90f;

        [Tooltip("Allow rotation")]
        public bool allowRotation = true;

        [Tooltip("Placement offset (for precise positioning)")]
        public Vector3 placementOffset = Vector3.zero;

        [Tooltip("Snaps to grid")]
        public bool snapToGrid = true;

        [Tooltip("Grid cell size (meters)")]
        public float gridCellSize = 1f;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(buildingID))
            {
                buildingID = "building_" + name.ToLower().Replace(" ", "_");
            }

            // Initialize requirements if null
            if (requirements == null)
            {
                requirements = new BuildingRequirements();
            }

            // Initialize functionality if null
            if (functionality == null)
            {
                functionality = new BuildingFunctionality();
            }
        }

        /// <summary>
        /// Check if player meets requirements to build
        /// </summary>
        public bool MeetsRequirements(out string reason)
        {
            reason = "";

            if (requirements == null)
                return true;

            // Check level
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requirements.requiredLevel)
            {
                reason = $"Requires level {requirements.requiredLevel}";
                return false;
            }

            // Check quest (placeholder)
            if (!string.IsNullOrEmpty(requirements.requiredQuestID))
            {
                // TODO: Check if quest is completed
                // For now, assume accessible
            }

            return true;
        }

        /// <summary>
        /// Check if player has enough resources
        /// </summary>
        public bool HasResources(out string missingResource)
        {
            missingResource = "";

            // Check resources
            if (buildCosts != null && buildCosts.Length > 0)
            {
                foreach (var cost in buildCosts)
                {
                    if (cost.item == null)
                        continue;

                    int playerAmount = Inventory.InventorySystem.Instance.GetItemCount(cost.item.itemID);
                    if (playerAmount < cost.quantity)
                    {
                        missingResource = $"{cost.item.itemName} ({playerAmount}/{cost.quantity})";
                        return false;
                    }
                }
            }

            // Check currency
            if (currencyCosts != null && currencyCosts.Length > 0)
            {
                foreach (var cost in currencyCosts)
                {
                    if (!CurrencyManager.Instance.HasCurrency(cost.currencyType, cost.amount))
                    {
                        missingResource = $"{cost.amount} {cost.currencyType}";
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Consume resources and currency for building
        /// </summary>
        public bool ConsumeResources()
        {
            // Check first
            string missing;
            if (!HasResources(out missing))
            {
                Debug.LogWarning($"[BuildingData] Cannot consume resources: missing {missing}");
                return false;
            }

            // Consume resources
            if (buildCosts != null)
            {
                foreach (var cost in buildCosts)
                {
                    if (cost.item != null)
                    {
                        Inventory.InventorySystem.Instance.RemoveItem(cost.item.itemID, cost.quantity);
                    }
                }
            }

            // Consume currency
            if (currencyCosts != null)
            {
                foreach (var cost in currencyCosts)
                {
                    CurrencyManager.Instance.RemoveCurrency(cost.currencyType, cost.amount);
                }
            }

            return true;
        }

        /// <summary>
        /// Refund resources on demolish
        /// </summary>
        public void RefundResources(int currentTier = 0)
        {
            if (!canDemolish)
                return;

            // Refund base costs
            if (buildCosts != null)
            {
                foreach (var cost in buildCosts)
                {
                    if (cost.item != null && cost.returnOnDemolish)
                    {
                        int refundAmount = Mathf.RoundToInt(cost.quantity * cost.returnPercentage);
                        if (refundAmount > 0)
                        {
                            Inventory.InventorySystem.Instance.AddItem(cost.item.itemID, refundAmount);
                        }
                    }
                }
            }

            // Refund currency
            if (currencyCosts != null)
            {
                foreach (var cost in currencyCosts)
                {
                    int refundAmount = Mathf.RoundToInt(cost.amount * demolishRefundPercentage);
                    if (refundAmount > 0)
                    {
                        CurrencyManager.Instance.AddCurrency(cost.currencyType, refundAmount);
                    }
                }
            }

            // Refund upgrade tier costs
            if (canUpgrade && upgradeTiers != null && currentTier > 0)
            {
                for (int i = 0; i < Mathf.Min(currentTier, upgradeTiers.Length); i++)
                {
                    var tier = upgradeTiers[i];

                    // Refund resources
                    if (tier.resourceCosts != null)
                    {
                        foreach (var cost in tier.resourceCosts)
                        {
                            if (cost.item != null && cost.returnOnDemolish)
                            {
                                int refundAmount = Mathf.RoundToInt(cost.quantity * cost.returnPercentage);
                                if (refundAmount > 0)
                                {
                                    Inventory.InventorySystem.Instance.AddItem(cost.item.itemID, refundAmount);
                                }
                            }
                        }
                    }

                    // Refund currency
                    if (tier.currencyCosts != null)
                    {
                        foreach (var cost in tier.currencyCosts)
                        {
                            int refundAmount = Mathf.RoundToInt(cost.amount * demolishRefundPercentage);
                            if (refundAmount > 0)
                            {
                                CurrencyManager.Instance.AddCurrency(cost.currencyType, refundAmount);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get prefab for specific tier
        /// </summary>
        public GameObject GetPrefabForTier(int tier)
        {
            if (tier == 0)
                return basePrefab;

            if (upgradeTiers != null && tier > 0 && tier <= upgradeTiers.Length)
            {
                return upgradeTiers[tier - 1].prefab;
            }

            return basePrefab;
        }

        /// <summary>
        /// Get upgrade tier
        /// </summary>
        public BuildingUpgradeTier GetUpgradeTier(int tier)
        {
            if (upgradeTiers == null || tier < 1 || tier > upgradeTiers.Length)
                return null;

            return upgradeTiers[tier - 1];
        }

        /// <summary>
        /// Get max tier
        /// </summary>
        public int GetMaxTier()
        {
            return canUpgrade && upgradeTiers != null ? upgradeTiers.Length : 0;
        }

        /// <summary>
        /// Get total storage slots (base + upgrades)
        /// </summary>
        public int GetTotalStorageSlots(int currentTier)
        {
            if (!functionality.hasStorage)
                return 0;

            int slots = functionality.storageSlots;

            // Add upgrade bonuses
            if (canUpgrade && upgradeTiers != null)
            {
                for (int i = 0; i < Mathf.Min(currentTier, upgradeTiers.Length); i++)
                {
                    slots += upgradeTiers[i].storageBonusSlots;
                }
            }

            return slots;
        }

        /// <summary>
        /// Get production speed multiplier (base + upgrades)
        /// </summary>
        public float GetProductionSpeedMultiplier(int currentTier)
        {
            if (!functionality.producesItems)
                return 1f;

            float multiplier = 1f;

            // Add upgrade bonuses
            if (canUpgrade && upgradeTiers != null)
            {
                for (int i = 0; i < Mathf.Min(currentTier, upgradeTiers.Length); i++)
                {
                    multiplier *= upgradeTiers[i].productionSpeedMultiplier;
                }
            }

            return multiplier;
        }
    }
}
