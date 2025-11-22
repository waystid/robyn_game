using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Inventory slot data
    /// </summary>
    [System.Serializable]
    public class InventorySlot
    {
        public string itemID;
        public int quantity;

        [System.NonSerialized]
        public Item itemData;

        public InventorySlot()
        {
            itemID = "";
            quantity = 0;
            itemData = null;
        }

        public InventorySlot(string id, int qty, Item data)
        {
            itemID = id;
            quantity = qty;
            itemData = data;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(itemID) || quantity <= 0;
        }

        public void Clear()
        {
            itemID = "";
            quantity = 0;
            itemData = null;
        }
    }

    /// <summary>
    /// Inventory system manager.
    /// Manages player inventory, items, and equipment.
    /// </summary>
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem Instance { get; private set; }

        [Header("Inventory Settings")]
        [Tooltip("Maximum inventory slots")]
        [Range(10, 100)]
        public int maxInventorySlots = 40;

        [Tooltip("Enable weight limit")]
        public bool enableWeightLimit = false;

        [Tooltip("Maximum carry weight")]
        public float maxWeight = 100f;

        [Header("Item Database")]
        [Tooltip("All items in game (auto-loaded from Resources)")]
        public Item[] itemDatabase;

        [Header("Starting Items")]
        [Tooltip("Items to give player on new game")]
        public ItemStack[] startingItems;

        [Header("Events")]
        public UnityEvent<string, int> OnItemAdded;      // itemID, quantity
        public UnityEvent<string, int> OnItemRemoved;    // itemID, quantity
        public UnityEvent<int> OnInventoryChanged;       // slotIndex
        public UnityEvent OnInventoryFull;

        // Inventory data
        private List<InventorySlot> inventory = new List<InventorySlot>();
        private Dictionary<string, Item> itemLookup = new Dictionary<string, Item>();

        // State
        private float currentWeight = 0f;

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
        /// Initialize inventory system
        /// </summary>
        private void Initialize()
        {
            // Load all items from Resources
            LoadItemDatabase();

            // Initialize inventory slots
            for (int i = 0; i < maxInventorySlots; i++)
            {
                inventory.Add(new InventorySlot());
            }

            // Give starting items if new game
            // TODO: Check if this is a new game (not loading save)
            // GiveStartingItems();

            Debug.Log($"[InventorySystem] Initialized with {maxInventorySlots} slots");
        }

        /// <summary>
        /// Load all items from Resources folder
        /// </summary>
        private void LoadItemDatabase()
        {
            itemLookup.Clear();

            // Load from assigned database
            if (itemDatabase != null && itemDatabase.Length > 0)
            {
                foreach (var item in itemDatabase)
                {
                    if (item != null && !string.IsNullOrEmpty(item.itemID))
                    {
                        itemLookup[item.itemID] = item;
                    }
                }
            }

            // Also try to load from Resources
            Item[] resourceItems = Resources.LoadAll<Item>("Items");
            foreach (var item in resourceItems)
            {
                if (item != null && !string.IsNullOrEmpty(item.itemID))
                {
                    if (!itemLookup.ContainsKey(item.itemID))
                    {
                        itemLookup[item.itemID] = item;
                    }
                }
            }

            Debug.Log($"[InventorySystem] Loaded {itemLookup.Count} items");
        }

        /// <summary>
        /// Give starting items to player
        /// </summary>
        public void GiveStartingItems()
        {
            if (startingItems == null || startingItems.Length == 0)
                return;

            foreach (var stack in startingItems)
            {
                if (stack.item != null)
                {
                    AddItem(stack.item.itemID, stack.quantity);
                }
            }

            Debug.Log($"[InventorySystem] Gave {startingItems.Length} starting items");
        }

        /// <summary>
        /// Get item data by ID
        /// </summary>
        public Item GetItemData(string itemID)
        {
            if (itemLookup.TryGetValue(itemID, out Item item))
            {
                return item;
            }

            Debug.LogWarning($"[InventorySystem] Item not found: {itemID}");
            return null;
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        public bool AddItem(string itemID, int quantity = 1)
        {
            Item itemData = GetItemData(itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventorySystem] Cannot add unknown item: {itemID}");
                return false;
            }

            // Check weight limit
            if (enableWeightLimit)
            {
                float addedWeight = itemData.weight * quantity;
                if (currentWeight + addedWeight > maxWeight)
                {
                    Debug.LogWarning($"[InventorySystem] Cannot add item, would exceed weight limit!");
                    OnInventoryFull?.Invoke();
                    return false;
                }
            }

            int remainingQuantity = quantity;

            // Try to stack with existing items
            if (itemData.maxStackSize > 1)
            {
                for (int i = 0; i < inventory.Count && remainingQuantity > 0; i++)
                {
                    InventorySlot slot = inventory[i];

                    if (slot.itemID == itemID && slot.quantity < itemData.maxStackSize)
                    {
                        int spaceInSlot = itemData.maxStackSize - slot.quantity;
                        int amountToAdd = Mathf.Min(spaceInSlot, remainingQuantity);

                        slot.quantity += amountToAdd;
                        remainingQuantity -= amountToAdd;

                        OnInventoryChanged?.Invoke(i);
                    }
                }
            }

            // Add to empty slots
            while (remainingQuantity > 0)
            {
                int emptySlotIndex = GetFirstEmptySlot();
                if (emptySlotIndex < 0)
                {
                    Debug.LogWarning("[InventorySystem] Inventory full!");
                    OnInventoryFull?.Invoke();
                    return false;
                }

                InventorySlot slot = inventory[emptySlotIndex];
                int amountToAdd = Mathf.Min(itemData.maxStackSize, remainingQuantity);

                slot.itemID = itemID;
                slot.quantity = amountToAdd;
                slot.itemData = itemData;
                remainingQuantity -= amountToAdd;

                OnInventoryChanged?.Invoke(emptySlotIndex);
            }

            // Update weight
            if (enableWeightLimit)
            {
                currentWeight += itemData.weight * quantity;
            }

            OnItemAdded?.Invoke(itemID, quantity);
            Debug.Log($"[InventorySystem] Added {quantity}x {itemData.itemName}");
            return true;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        public bool RemoveItem(string itemID, int quantity = 1)
        {
            Item itemData = GetItemData(itemID);
            if (itemData == null)
                return false;

            int remainingToRemove = quantity;

            // Remove from slots
            for (int i = 0; i < inventory.Count && remainingToRemove > 0; i++)
            {
                InventorySlot slot = inventory[i];

                if (slot.itemID == itemID)
                {
                    int amountToRemove = Mathf.Min(slot.quantity, remainingToRemove);
                    slot.quantity -= amountToRemove;
                    remainingToRemove -= amountToRemove;

                    if (slot.quantity <= 0)
                    {
                        slot.Clear();
                    }

                    OnInventoryChanged?.Invoke(i);
                }
            }

            if (remainingToRemove > 0)
            {
                Debug.LogWarning($"[InventorySystem] Could not remove full quantity of {itemID}");
                return false;
            }

            // Update weight
            if (enableWeightLimit)
            {
                currentWeight -= itemData.weight * quantity;
                currentWeight = Mathf.Max(0f, currentWeight);
            }

            OnItemRemoved?.Invoke(itemID, quantity);
            Debug.Log($"[InventorySystem] Removed {quantity}x {itemData.itemName}");
            return true;
        }

        /// <summary>
        /// Get count of specific item
        /// </summary>
        public int GetItemCount(string itemID)
        {
            int count = 0;

            foreach (var slot in inventory)
            {
                if (slot.itemID == itemID)
                {
                    count += slot.quantity;
                }
            }

            return count;
        }

        /// <summary>
        /// Check if inventory has item
        /// </summary>
        public bool HasItem(string itemID, int quantity = 1)
        {
            return GetItemCount(itemID) >= quantity;
        }

        /// <summary>
        /// Use item from inventory
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= inventory.Count)
                return false;

            InventorySlot slot = inventory[slotIndex];
            if (slot.IsEmpty() || slot.itemData == null)
                return false;

            // Use the item
            bool wasUsed = slot.itemData.Use();

            if (wasUsed)
            {
                // Remove one from stack
                RemoveItem(slot.itemID, 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Drop item from inventory
        /// </summary>
        public bool DropItem(int slotIndex, int quantity = 1)
        {
            if (slotIndex < 0 || slotIndex >= inventory.Count)
                return false;

            InventorySlot slot = inventory[slotIndex];
            if (slot.IsEmpty() || slot.itemData == null)
                return false;

            if (!slot.itemData.isDroppable)
            {
                Debug.LogWarning($"[InventorySystem] Item cannot be dropped: {slot.itemData.itemName}");
                return false;
            }

            // Spawn item in world
            SpawnItemInWorld(slot.itemData, quantity);

            // Remove from inventory
            RemoveItem(slot.itemID, quantity);

            return true;
        }

        /// <summary>
        /// Spawn item in world
        /// </summary>
        private void SpawnItemInWorld(Item item, int quantity)
        {
            if (PlayerController.Instance == null)
                return;

            Vector3 spawnPosition = PlayerController.Instance.transform.position +
                                   PlayerController.Instance.transform.forward * 2f;

            if (item.worldPrefab != null)
            {
                GameObject dropped = Instantiate(item.worldPrefab, spawnPosition, Quaternion.identity);

                // TODO: Add ItemPickup component with item data and quantity
                Debug.Log($"[InventorySystem] Dropped {quantity}x {item.itemName} at {spawnPosition}");
            }
            else
            {
                Debug.LogWarning($"[InventorySystem] No world prefab for {item.itemName}");
            }
        }

        /// <summary>
        /// Move item between slots
        /// </summary>
        public bool MoveItem(int fromSlot, int toSlot)
        {
            if (fromSlot < 0 || fromSlot >= inventory.Count ||
                toSlot < 0 || toSlot >= inventory.Count)
                return false;

            InventorySlot from = inventory[fromSlot];
            InventorySlot to = inventory[toSlot];

            // Swap slots
            string tempID = to.itemID;
            int tempQty = to.quantity;
            Item tempData = to.itemData;

            to.itemID = from.itemID;
            to.quantity = from.quantity;
            to.itemData = from.itemData;

            from.itemID = tempID;
            from.quantity = tempQty;
            from.itemData = tempData;

            OnInventoryChanged?.Invoke(fromSlot);
            OnInventoryChanged?.Invoke(toSlot);

            return true;
        }

        /// <summary>
        /// Get inventory slot
        /// </summary>
        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= inventory.Count)
                return null;

            return inventory[index];
        }

        /// <summary>
        /// Get all inventory slots
        /// </summary>
        public List<InventorySlot> GetAllSlots()
        {
            return new List<InventorySlot>(inventory);
        }

        /// <summary>
        /// Get first empty slot index
        /// </summary>
        public int GetFirstEmptySlot()
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i].IsEmpty())
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Get number of empty slots
        /// </summary>
        public int GetEmptySlotCount()
        {
            int count = 0;
            foreach (var slot in inventory)
            {
                if (slot.IsEmpty())
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Get current weight
        /// </summary>
        public float GetCurrentWeight()
        {
            return currentWeight;
        }

        /// <summary>
        /// Get weight percentage (0-1)
        /// </summary>
        public float GetWeightPercentage()
        {
            if (!enableWeightLimit || maxWeight <= 0)
                return 0f;

            return currentWeight / maxWeight;
        }

        /// <summary>
        /// Clear entire inventory
        /// </summary>
        public void ClearInventory()
        {
            foreach (var slot in inventory)
            {
                slot.Clear();
            }

            currentWeight = 0f;

            Debug.Log("[InventorySystem] Inventory cleared");
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public List<SaveSystem.ItemSaveData> GetSaveData()
        {
            List<SaveSystem.ItemSaveData> saveData = new List<SaveSystem.ItemSaveData>();

            for (int i = 0; i < inventory.Count; i++)
            {
                InventorySlot slot = inventory[i];
                if (!slot.IsEmpty())
                {
                    saveData.Add(new SaveSystem.ItemSaveData
                    {
                        itemID = slot.itemID,
                        quantity = slot.quantity,
                        slotIndex = i
                    });
                }
            }

            return saveData;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(List<SaveSystem.ItemSaveData> saveData)
        {
            ClearInventory();

            if (saveData == null)
                return;

            foreach (var itemData in saveData)
            {
                if (itemData.slotIndex >= 0 && itemData.slotIndex < inventory.Count)
                {
                    Item item = GetItemData(itemData.itemID);
                    if (item != null)
                    {
                        InventorySlot slot = inventory[itemData.slotIndex];
                        slot.itemID = itemData.itemID;
                        slot.quantity = itemData.quantity;
                        slot.itemData = item;

                        OnInventoryChanged?.Invoke(itemData.slotIndex);
                    }
                }
            }

            // Recalculate weight
            RecalculateWeight();

            Debug.Log($"[InventorySystem] Loaded {saveData.Count} items from save");
        }

        /// <summary>
        /// Recalculate current weight
        /// </summary>
        private void RecalculateWeight()
        {
            currentWeight = 0f;

            if (!enableWeightLimit)
                return;

            foreach (var slot in inventory)
            {
                if (!slot.IsEmpty() && slot.itemData != null)
                {
                    currentWeight += slot.itemData.weight * slot.quantity;
                }
            }
        }
    }

    /// <summary>
    /// Item stack for starting items
    /// </summary>
    [System.Serializable]
    public class ItemStack
    {
        public Item item;
        [Range(1, 99)]
        public int quantity = 1;
    }
}
