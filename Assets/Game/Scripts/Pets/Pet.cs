using UnityEngine;

namespace CozyGame.Pets
{
    /// <summary>
    /// Pet type categories
    /// </summary>
    public enum PetType
    {
        Dog,
        Cat,
        Bird,
        Rabbit,
        Fox,
        Dragon,
        Fairy,
        Spirit,
        Custom
    }

    /// <summary>
    /// Pet abilities
    /// </summary>
    public enum PetAbility
    {
        FindItems,      // Discovers nearby items
        FindPlants,     // Finds harvestable plants
        Fight,          // Attacks enemies
        Heal,           // Heals player
        Scout,          // Reveals map areas
        Gather,         // Auto-collects nearby items
        Luck,           // Increases loot quality
        Protection      // Shields player from damage
    }

    /// <summary>
    /// Pet rarity levels
    /// </summary>
    public enum PetRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Pet definition ScriptableObject.
    /// Defines all properties of a pet type.
    /// </summary>
    [CreateAssetMenu(fileName = "New Pet", menuName = "Cozy Game/Pets/Pet")]
    public class Pet : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique pet ID")]
        public string petID;

        [Tooltip("Pet name (default, can be customized)")]
        public string petName;

        [Tooltip("Pet type")]
        public PetType petType = PetType.Dog;

        [Tooltip("Pet rarity")]
        public PetRarity rarity = PetRarity.Common;

        [Tooltip("Pet description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Pet icon/portrait")]
        public Sprite icon;

        [Tooltip("Pet prefab (3D model)")]
        public GameObject prefab;

        [Header("Stats")]
        [Tooltip("Base health")]
        public float baseHealth = 50f;

        [Tooltip("Base damage (if fighting)")]
        public float baseDamage = 5f;

        [Tooltip("Move speed")]
        public float moveSpeed = 4f;

        [Tooltip("Follow distance")]
        public float followDistance = 3f;

        [Header("Needs")]
        [Tooltip("Max hunger value")]
        public float maxHunger = 100f;

        [Tooltip("Hunger decrease rate (per second)")]
        public float hungerDecreaseRate = 0.5f;

        [Tooltip("Max happiness value")]
        public float maxHappiness = 100f;

        [Tooltip("Happiness decrease rate (per second)")]
        public float happinessDecreaseRate = 0.2f;

        [Header("Abilities")]
        [Tooltip("Pet abilities")]
        public PetAbility[] abilities;

        [Tooltip("Ability cooldown (seconds)")]
        public float abilityCooldown = 30f;

        [Header("Leveling")]
        [Tooltip("Can level up")]
        public bool canLevelUp = true;

        [Tooltip("Max level")]
        public int maxLevel = 10;

        [Tooltip("Exp required for level 1")]
        public int baseExpRequired = 100;

        [Tooltip("Exp multiplier per level")]
        public float expMultiplier = 1.5f;

        [Header("Customization")]
        [Tooltip("Can be renamed")]
        public bool canRename = true;

        [Tooltip("Available accessories")]
        public string[] availableAccessories;

        [Header("Audio")]
        [Tooltip("Idle sound")]
        public string idleSound = "pet_idle";

        [Tooltip("Happy sound")]
        public string happySound = "pet_happy";

        [Tooltip("Hungry sound")]
        public string hungrySound = "pet_hungry";

        /// <summary>
        /// Get rarity color
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case PetRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // Gray
                case PetRarity.Uncommon:
                    return new Color(0.2f, 1f, 0.2f); // Green
                case PetRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f); // Blue
                case PetRarity.Epic:
                    return new Color(0.8f, 0.3f, 1f); // Purple
                case PetRarity.Legendary:
                    return new Color(1f, 0.6f, 0f); // Orange
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Calculate exp required for level
        /// </summary>
        public int GetExpRequiredForLevel(int level)
        {
            return Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expMultiplier, level - 1));
        }

        /// <summary>
        /// Check if has ability
        /// </summary>
        public bool HasAbility(PetAbility ability)
        {
            if (abilities == null)
                return false;

            foreach (var a in abilities)
            {
                if (a == ability)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Validate pet data
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate ID from name if empty
            if (string.IsNullOrEmpty(petID) && !string.IsNullOrEmpty(petName))
            {
                petID = petName.ToLower().Replace(" ", "_");
            }
        }
    }
}
