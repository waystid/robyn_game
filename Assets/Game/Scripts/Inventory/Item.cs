using UnityEngine;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Item type categories
    /// </summary>
    public enum ItemType
    {
        Consumable,     // Food, potions, etc.
        Equipment,      // Weapons, armor
        Material,       // Crafting materials
        QuestItem,      // Quest-specific items
        Spell,          // Spell scrolls/books
        Seed,           // Plant seeds
        Key,            // Keys for unlocking
        Miscellaneous   // Other items
    }

    /// <summary>
    /// Item rarity levels
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// Equipment slot types
    /// </summary>
    public enum EquipmentSlot
    {
        None,
        Head,
        Chest,
        Legs,
        Feet,
        Hands,
        MainHand,       // Weapon
        OffHand,        // Shield
        Accessory1,
        Accessory2
    }

    /// <summary>
    /// Item definition ScriptableObject.
    /// Defines all properties of an item type.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "Cozy Game/Inventory/Item")]
    public class Item : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique item ID")]
        public string itemID;

        [Tooltip("Display name")]
        public string itemName;

        [Tooltip("Item description")]
        [TextArea(3, 6)]
        public string description;

        [Tooltip("Item icon")]
        public Sprite icon;

        [Tooltip("3D model prefab (for world drops)")]
        public GameObject worldPrefab;

        [Header("Item Properties")]
        [Tooltip("Item type/category")]
        public ItemType itemType = ItemType.Miscellaneous;

        [Tooltip("Item rarity")]
        public ItemRarity rarity = ItemRarity.Common;

        [Tooltip("Maximum stack size (1 = non-stackable)")]
        [Range(1, 999)]
        public int maxStackSize = 1;

        [Tooltip("Item value (gold)")]
        public int value = 0;

        [Tooltip("Item weight")]
        public float weight = 1f;

        [Tooltip("Can be sold to vendors")]
        public bool isSellable = true;

        [Tooltip("Can be dropped from inventory")]
        public bool isDroppable = true;

        [Tooltip("Is this a quest item?")]
        public bool isQuestItem = false;

        [Header("Equipment Properties")]
        [Tooltip("Equipment slot (if equipment type)")]
        public EquipmentSlot equipmentSlot = EquipmentSlot.None;

        [Tooltip("Armor value")]
        public int armor = 0;

        [Tooltip("Damage value")]
        public int damage = 0;

        [Tooltip("Required level to use")]
        public int requiredLevel = 1;

        [Header("Consumable Properties")]
        [Tooltip("Heal amount (HP)")]
        public float healAmount = 0f;

        [Tooltip("Mana restore amount")]
        public float manaAmount = 0f;

        [Tooltip("Stamina restore amount")]
        public float staminaAmount = 0f;

        [Tooltip("Buff duration (seconds, 0 = instant)")]
        public float buffDuration = 0f;

        [Tooltip("Consumable effect description")]
        public string effectDescription = "";

        [Header("Crafting")]
        [Tooltip("Can be used in crafting")]
        public bool isCraftingMaterial = false;

        [Tooltip("Recipes that use this item")]
        public CraftingRecipe[] usedInRecipes;

        [Header("Spell Properties")]
        [Tooltip("Spell to unlock (if spell item)")]
        public string unlockSpellID = "";

        [Header("Seed Properties")]
        [Tooltip("Plant to grow (if seed item)")]
        public string plantSpeciesID = "";

        /// <summary>
        /// Get rarity color
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // Gray
                case ItemRarity.Uncommon:
                    return new Color(0.2f, 1f, 0.2f); // Green
                case ItemRarity.Rare:
                    return new Color(0.3f, 0.5f, 1f); // Blue
                case ItemRarity.Epic:
                    return new Color(0.8f, 0.3f, 1f); // Purple
                case ItemRarity.Legendary:
                    return new Color(1f, 0.6f, 0f); // Orange
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Get formatted tooltip text
        /// </summary>
        public string GetTooltipText()
        {
            string tooltip = $"<b>{itemName}</b>\n";
            tooltip += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{rarity}</color>\n\n";
            tooltip += $"{description}\n\n";

            // Add type-specific info
            if (itemType == ItemType.Equipment)
            {
                if (damage > 0)
                    tooltip += $"Damage: {damage}\n";
                if (armor > 0)
                    tooltip += $"Armor: {armor}\n";
                if (requiredLevel > 1)
                    tooltip += $"Required Level: {requiredLevel}\n";
            }
            else if (itemType == ItemType.Consumable)
            {
                if (healAmount > 0)
                    tooltip += $"Heals: {healAmount} HP\n";
                if (manaAmount > 0)
                    tooltip += $"Restores: {manaAmount} Mana\n";
                if (staminaAmount > 0)
                    tooltip += $"Restores: {staminaAmount} Stamina\n";
                if (!string.IsNullOrEmpty(effectDescription))
                    tooltip += $"{effectDescription}\n";
            }

            tooltip += $"\nValue: {value} gold";
            if (weight > 0)
                tooltip += $"\nWeight: {weight}";

            return tooltip;
        }

        /// <summary>
        /// Use/consume this item
        /// </summary>
        public bool Use()
        {
            switch (itemType)
            {
                case ItemType.Consumable:
                    return UseConsumable();

                case ItemType.Equipment:
                    return UseEquipment();

                case ItemType.Spell:
                    return UseSpellItem();

                case ItemType.Seed:
                    return UseSeed();

                default:
                    Debug.LogWarning($"[Item] Cannot use item type: {itemType}");
                    return false;
            }
        }

        /// <summary>
        /// Use consumable item
        /// </summary>
        private bool UseConsumable()
        {
            if (PlayerStats.Instance == null)
                return false;

            bool wasUsed = false;

            // Apply healing
            if (healAmount > 0)
            {
                PlayerStats.Instance.Heal(healAmount);
                wasUsed = true;
            }

            // Restore mana
            if (manaAmount > 0)
            {
                PlayerStats.Instance.RestoreMana(manaAmount);
                wasUsed = true;
            }

            // Restore stamina
            if (staminaAmount > 0)
            {
                PlayerStats.Instance.RestoreStamina(staminaAmount);
                wasUsed = true;
            }

            if (wasUsed)
            {
                Debug.Log($"[Item] Used consumable: {itemName}");
            }

            return wasUsed;
        }

        /// <summary>
        /// Use equipment item (equip it)
        /// </summary>
        private bool UseEquipment()
        {
            if (EquipmentSystem.Instance == null)
                return false;

            EquipmentSystem.Instance.EquipItem(this);
            Debug.Log($"[Item] Equipped: {itemName}");
            return true;
        }

        /// <summary>
        /// Use spell item (learn spell)
        /// </summary>
        private bool UseSpellItem()
        {
            if (PlayerStats.Instance == null || string.IsNullOrEmpty(unlockSpellID))
                return false;

            PlayerStats.Instance.UnlockSpell(unlockSpellID);
            Debug.Log($"[Item] Learned spell: {unlockSpellID}");
            return true;
        }

        /// <summary>
        /// Use seed item (plant it)
        /// </summary>
        private bool UseSeed()
        {
            // TODO: Implement planting when PlantManager integration is ready
            Debug.Log($"[Item] Would plant: {plantSpeciesID}");
            return false;
        }

        /// <summary>
        /// Validate item data
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate ID from name if empty
            if (string.IsNullOrEmpty(itemID) && !string.IsNullOrEmpty(itemName))
            {
                itemID = itemName.ToLower().Replace(" ", "_");
            }

            // Ensure quest items are not droppable/sellable
            if (isQuestItem)
            {
                isDroppable = false;
                isSellable = false;
            }

            // Equipment items shouldn't stack
            if (itemType == ItemType.Equipment)
            {
                maxStackSize = 1;
            }
        }
    }
}
