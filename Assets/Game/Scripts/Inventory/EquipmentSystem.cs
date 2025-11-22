using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Equipment system manager.
    /// Manages equipped items and stat bonuses.
    /// </summary>
    public class EquipmentSystem : MonoBehaviour
    {
        public static EquipmentSystem Instance { get; private set; }

        [Header("Equipment Slots")]
        [Tooltip("Currently equipped items")]
        public Dictionary<EquipmentSlot, Item> equippedItems = new Dictionary<EquipmentSlot, Item>();

        [Header("Events")]
        public UnityEvent<EquipmentSlot, Item> OnItemEquipped;
        public UnityEvent<EquipmentSlot, Item> OnItemUnequipped;
        public UnityEvent OnEquipmentChanged;

        // Cached stat bonuses
        private int totalArmor = 0;
        private int totalDamage = 0;

        private void Awake()
        {
            // Singleton setup with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize equipment system
        /// </summary>
        private void Initialize()
        {
            // Initialize all equipment slots as empty
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot != EquipmentSlot.None)
                {
                    equippedItems[slot] = null;
                }
            }

            Debug.Log("[EquipmentSystem] Initialized");
        }

        /// <summary>
        /// Equip an item
        /// </summary>
        public bool EquipItem(Item item)
        {
            if (item == null || item.itemType != ItemType.Equipment)
            {
                Debug.LogWarning("[EquipmentSystem] Cannot equip non-equipment item!");
                return false;
            }

            if (item.equipmentSlot == EquipmentSlot.None)
            {
                Debug.LogWarning($"[EquipmentSystem] Item {item.itemName} has no equipment slot!");
                return false;
            }

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < item.requiredLevel)
            {
                Debug.LogWarning($"[EquipmentSystem] Requires level {item.requiredLevel} to equip {item.itemName}");
                return false;
            }

            // Unequip current item in slot if any
            if (equippedItems[item.equipmentSlot] != null)
            {
                UnequipItem(item.equipmentSlot);
            }

            // Equip new item
            equippedItems[item.equipmentSlot] = item;

            // Remove from inventory
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.RemoveItem(item.itemID, 1);
            }

            // Apply stat bonuses
            ApplyItemStats(item, true);

            OnItemEquipped?.Invoke(item.equipmentSlot, item);
            OnEquipmentChanged?.Invoke();

            Debug.Log($"[EquipmentSystem] Equipped {item.itemName} to {item.equipmentSlot}");
            return true;
        }

        /// <summary>
        /// Unequip an item
        /// </summary>
        public bool UnequipItem(EquipmentSlot slot)
        {
            if (!equippedItems.ContainsKey(slot) || equippedItems[slot] == null)
            {
                Debug.LogWarning($"[EquipmentSystem] No item equipped in {slot}");
                return false;
            }

            Item item = equippedItems[slot];

            // Return to inventory
            if (InventorySystem.Instance != null)
            {
                bool added = InventorySystem.Instance.AddItem(item.itemID, 1);
                if (!added)
                {
                    Debug.LogWarning("[EquipmentSystem] Inventory full, cannot unequip!");
                    return false;
                }
            }

            // Remove stat bonuses
            ApplyItemStats(item, false);

            // Clear slot
            equippedItems[slot] = null;

            OnItemUnequipped?.Invoke(slot, item);
            OnEquipmentChanged?.Invoke();

            Debug.Log($"[EquipmentSystem] Unequipped {item.itemName} from {slot}");
            return true;
        }

        /// <summary>
        /// Get equipped item in slot
        /// </summary>
        public Item GetEquippedItem(EquipmentSlot slot)
        {
            if (equippedItems.ContainsKey(slot))
            {
                return equippedItems[slot];
            }

            return null;
        }

        /// <summary>
        /// Check if slot has item equipped
        /// </summary>
        public bool IsSlotEquipped(EquipmentSlot slot)
        {
            return equippedItems.ContainsKey(slot) && equippedItems[slot] != null;
        }

        /// <summary>
        /// Apply/remove item stat bonuses
        /// </summary>
        private void ApplyItemStats(Item item, bool add)
        {
            int multiplier = add ? 1 : -1;

            // Armor
            if (item.armor > 0)
            {
                totalArmor += item.armor * multiplier;

                // TODO: Apply armor to PlayerStats
                // if (PlayerStats.Instance != null)
                //     PlayerStats.Instance.ModifyArmor(item.armor * multiplier);
            }

            // Damage
            if (item.damage > 0)
            {
                totalDamage += item.damage * multiplier;

                // TODO: Apply damage to PlayerStats
                // if (PlayerStats.Instance != null)
                //     PlayerStats.Instance.ModifyDamage(item.damage * multiplier);
            }

            RecalculateTotalStats();
        }

        /// <summary>
        /// Recalculate total stat bonuses
        /// </summary>
        private void RecalculateTotalStats()
        {
            totalArmor = 0;
            totalDamage = 0;

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    totalArmor += kvp.Value.armor;
                    totalDamage += kvp.Value.damage;
                }
            }

            Debug.Log($"[EquipmentSystem] Total stats - Armor: {totalArmor}, Damage: {totalDamage}");
        }

        /// <summary>
        /// Get total armor from equipment
        /// </summary>
        public int GetTotalArmor()
        {
            return totalArmor;
        }

        /// <summary>
        /// Get total damage from equipment
        /// </summary>
        public int GetTotalDamage()
        {
            return totalDamage;
        }

        /// <summary>
        /// Unequip all items
        /// </summary>
        public void UnequipAll()
        {
            List<EquipmentSlot> slotsToUnequip = new List<EquipmentSlot>();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    slotsToUnequip.Add(kvp.Key);
                }
            }

            foreach (var slot in slotsToUnequip)
            {
                UnequipItem(slot);
            }

            Debug.Log("[EquipmentSystem] Unequipped all items");
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public Dictionary<string, string> GetSaveData()
        {
            Dictionary<string, string> saveData = new Dictionary<string, string>();

            foreach (var kvp in equippedItems)
            {
                if (kvp.Value != null)
                {
                    saveData[kvp.Key.ToString()] = kvp.Value.itemID;
                }
            }

            return saveData;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(Dictionary<string, string> saveData)
        {
            // Unequip all current items
            UnequipAll();

            if (saveData == null)
                return;

            foreach (var kvp in saveData)
            {
                // Parse equipment slot
                if (System.Enum.TryParse(kvp.Key, out EquipmentSlot slot))
                {
                    // Get item data
                    if (InventorySystem.Instance != null)
                    {
                        Item item = InventorySystem.Instance.GetItemData(kvp.Value);
                        if (item != null)
                        {
                            // Add to inventory first
                            InventorySystem.Instance.AddItem(item.itemID, 1);

                            // Then equip
                            EquipItem(item);
                        }
                    }
                }
            }

            Debug.Log($"[EquipmentSystem] Loaded {saveData.Count} equipped items");
        }

        /// <summary>
        /// Get all equipped items
        /// </summary>
        public Dictionary<EquipmentSlot, Item> GetAllEquippedItems()
        {
            return new Dictionary<EquipmentSlot, Item>(equippedItems);
        }
    }
}
