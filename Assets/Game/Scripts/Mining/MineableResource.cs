using UnityEngine;
using CozyGame.Inventory;

namespace CozyGame.Mining
{
    /// <summary>
    /// Resource rarity types
    /// </summary>
    public enum ResourceRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Resource type categories
    /// </summary>
    public enum ResourceType
    {
        Stone,      // Basic stone
        Ore,        // Metal ores (iron, copper, gold)
        Gem,        // Precious gems (ruby, emerald, diamond)
        Crystal,    // Magic crystals
        Fossil,     // Ancient fossils
        Custom      // Custom type
    }

    /// <summary>
    /// Mining difficulty
    /// </summary>
    public enum MiningDifficulty
    {
        VeryEasy,   // 1-2 hits
        Easy,       // 3-5 hits
        Medium,     // 6-10 hits
        Hard,       // 11-15 hits
        VeryHard    // 16+ hits
    }

    /// <summary>
    /// Resource drop entry
    /// </summary>
    [System.Serializable]
    public class ResourceDrop
    {
        [Tooltip("Item to drop")]
        public Item item;

        [Tooltip("Minimum quantity")]
        public int minQuantity = 1;

        [Tooltip("Maximum quantity")]
        public int maxQuantity = 3;

        [Tooltip("Drop chance (0-1)")]
        [Range(0f, 1f)]
        public float dropChance = 1f;

        [Tooltip("Bonus from higher tier tools")]
        public int bonusPerToolTier = 0;

        /// <summary>
        /// Get random quantity
        /// </summary>
        public int GetRandomQuantity(int toolTier = 0)
        {
            int base_quantity = Random.Range(minQuantity, maxQuantity + 1);
            int bonus = bonusPerToolTier * toolTier;
            return base_quantity + bonus;
        }

        /// <summary>
        /// Roll for drop
        /// </summary>
        public bool RollForDrop()
        {
            return Random.value <= dropChance;
        }
    }

    /// <summary>
    /// Mineable resource ScriptableObject.
    /// Defines resource properties, drops, and mining requirements.
    /// Create via: Right-click → Create → Cozy Game → Mining → Mineable Resource
    /// </summary>
    [CreateAssetMenu(fileName = "New Mineable Resource", menuName = "Cozy Game/Mining/Mineable Resource", order = 10)]
    public class MineableResource : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Resource name")]
        public string resourceName = "Stone";

        [Tooltip("Resource description")]
        [TextArea(2, 3)]
        public string description = "A basic stone resource";

        [Tooltip("Resource icon")]
        public Sprite icon;

        [Tooltip("Resource type")]
        public ResourceType resourceType = ResourceType.Stone;

        [Tooltip("Resource rarity")]
        public ResourceRarity rarity = ResourceRarity.Common;

        [Tooltip("Resource color (for UI/particles)")]
        public Color resourceColor = Color.gray;

        [Header("Mining Requirements")]
        [Tooltip("Minimum tool tier required (0 = any tool)")]
        public int minToolTier = 0;

        [Tooltip("Mining difficulty")]
        public MiningDifficulty difficulty = MiningDifficulty.Medium;

        [Tooltip("Base hits to mine")]
        public int baseHitsRequired = 5;

        [Tooltip("Mining hardness (0-1, affects mini-game difficulty)")]
        [Range(0f, 1f)]
        public float hardness = 0.5f;

        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Header("Drops")]
        [Tooltip("Resource drops when mined")]
        public ResourceDrop[] drops;

        [Tooltip("Experience gained when mined")]
        public int experienceReward = 10;

        [Header("Visual")]
        [Tooltip("Node prefab (visual representation)")]
        public GameObject nodePrefab;

        [Tooltip("Damaged node prefabs (optional, per damage stage)")]
        public GameObject[] damagedPrefabs;

        [Tooltip("Mining particle effect")]
        public GameObject miningParticlePrefab;

        [Tooltip("Destruction particle effect")]
        public GameObject destructionParticlePrefab;

        [Header("Audio")]
        [Tooltip("Mining hit sound")]
        public string miningHitSound = "mining_hit";

        [Tooltip("Mining success sound")]
        public string miningSuccessSound = "mining_success";

        [Tooltip("Destruction sound")]
        public string destructionSound = "rock_break";

        [Header("Respawn")]
        [Tooltip("Can respawn after being mined")]
        public bool canRespawn = true;

        [Tooltip("Respawn time (seconds)")]
        public float respawnTime = 300f; // 5 minutes

        [Tooltip("Random respawn variance (±seconds)")]
        public float respawnVariance = 60f;

        [Header("Advanced")]
        [Tooltip("Tool durability damage per hit")]
        public int toolDurabilityDamage = 1;

        [Tooltip("Bonus drops on critical hit")]
        public bool hasCriticalDrops = false;

        [Tooltip("Critical drop multiplier")]
        [Range(1f, 5f)]
        public float criticalDropMultiplier = 2f;

        /// <summary>
        /// Get hits required based on tool tier
        /// </summary>
        public int GetHitsRequired(int toolTier)
        {
            // Higher tier tools reduce hits required
            int reduction = Mathf.Max(0, toolTier - minToolTier);
            int hits = baseHitsRequired - reduction;
            return Mathf.Max(1, hits); // At least 1 hit
        }

        /// <summary>
        /// Check if tool can mine this resource
        /// </summary>
        public bool CanMineWithTool(int toolTier)
        {
            return toolTier >= minToolTier;
        }

        /// <summary>
        /// Get all drops for successful mining
        /// </summary>
        public void GenerateDrops(int toolTier, bool isCritical, out Item[] items, out int[] quantities)
        {
            System.Collections.Generic.List<Item> itemList = new System.Collections.Generic.List<Item>();
            System.Collections.Generic.List<int> quantityList = new System.Collections.Generic.List<int>();

            if (drops == null || drops.Length == 0)
            {
                items = new Item[0];
                quantities = new int[0];
                return;
            }

            foreach (var drop in drops)
            {
                if (drop.item == null)
                    continue;

                // Roll for drop
                if (!drop.RollForDrop())
                    continue;

                // Get quantity
                int quantity = drop.GetRandomQuantity(toolTier);

                // Apply critical multiplier
                if (isCritical && hasCriticalDrops)
                {
                    quantity = Mathf.RoundToInt(quantity * criticalDropMultiplier);
                }

                itemList.Add(drop.item);
                quantityList.Add(quantity);
            }

            items = itemList.ToArray();
            quantities = quantityList.ToArray();
        }

        /// <summary>
        /// Get respawn time with variance
        /// </summary>
        public float GetRandomRespawnTime()
        {
            return respawnTime + Random.Range(-respawnVariance, respawnVariance);
        }

        /// <summary>
        /// Get damaged prefab for current health percentage
        /// </summary>
        public GameObject GetDamagedPrefab(float healthPercent)
        {
            if (damagedPrefabs == null || damagedPrefabs.Length == 0)
                return null;

            // Map health to damage stage
            int index = Mathf.FloorToInt((1f - healthPercent) * damagedPrefabs.Length);
            index = Mathf.Clamp(index, 0, damagedPrefabs.Length - 1);

            return damagedPrefabs[index];
        }

        /// <summary>
        /// Get rarity color
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ResourceRarity.Common: return Color.white;
                case ResourceRarity.Uncommon: return Color.green;
                case ResourceRarity.Rare: return Color.blue;
                case ResourceRarity.Epic: return new Color(0.6f, 0f, 1f); // Purple
                case ResourceRarity.Legendary: return new Color(1f, 0.5f, 0f); // Orange
                default: return Color.white;
            }
        }

        /// <summary>
        /// Get difficulty display string
        /// </summary>
        public string GetDifficultyString()
        {
            switch (difficulty)
            {
                case MiningDifficulty.VeryEasy: return "Very Easy";
                case MiningDifficulty.Easy: return "Easy";
                case MiningDifficulty.Medium: return "Medium";
                case MiningDifficulty.Hard: return "Hard";
                case MiningDifficulty.VeryHard: return "Very Hard";
                default: return "Unknown";
            }
        }
    }
}
