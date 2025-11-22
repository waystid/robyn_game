using UnityEngine;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.Economy
{
    /// <summary>
    /// Shop item entry
    /// </summary>
    [System.Serializable]
    public class ShopItem
    {
        [Tooltip("Item to sell")]
        public Item item;

        [Tooltip("Buy price")]
        public int buyPrice = 10;

        [Tooltip("Sell price (0 = cannot sell)")]
        public int sellPrice = 5;

        [Tooltip("Currency type")]
        public CurrencyType currencyType = CurrencyType.Gold;

        [Tooltip("Stock quantity (-1 = infinite)")]
        public int stock = -1;

        [Tooltip("Restock amount (if stock > 0)")]
        public int restockAmount = 10;

        [Tooltip("Restock time in seconds (0 = no restock)")]
        public float restockTime = 0f;

        [Tooltip("Required player level to buy")]
        public int requiredLevel = 1;

        [Tooltip("Is unlocked by default")]
        public bool unlockedByDefault = true;

        // Runtime
        [System.NonSerialized]
        public int currentStock;

        [System.NonSerialized]
        public float timeSinceRestock;

        [System.NonSerialized]
        public bool isUnlocked;

        /// <summary>
        /// Initialize shop item
        /// </summary>
        public void Initialize()
        {
            currentStock = stock;
            timeSinceRestock = 0f;
            isUnlocked = unlockedByDefault;
        }

        /// <summary>
        /// Update restock timer
        /// </summary>
        public void UpdateRestock(float deltaTime)
        {
            if (stock <= 0 || restockTime <= 0f)
                return;

            timeSinceRestock += deltaTime;

            if (timeSinceRestock >= restockTime)
            {
                currentStock = Mathf.Min(currentStock + restockAmount, stock);
                timeSinceRestock = 0f;
            }
        }

        /// <summary>
        /// Check if in stock
        /// </summary>
        public bool IsInStock()
        {
            return stock < 0 || currentStock > 0;
        }

        /// <summary>
        /// Purchase item
        /// </summary>
        public bool Purchase(int quantity = 1)
        {
            if (stock < 0)
            {
                // Infinite stock
                return true;
            }

            if (currentStock >= quantity)
            {
                currentStock -= quantity;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sell item back
        /// </summary>
        public void Sell(int quantity = 1)
        {
            if (stock < 0)
            {
                // Infinite stock, no need to add back
                return;
            }

            currentStock = Mathf.Min(currentStock + quantity, stock);
        }
    }

    /// <summary>
    /// Shop ScriptableObject definition.
    /// Defines shop inventory, prices, and settings.
    /// Create instances via: Right-click → Create → Cozy Game → Economy → Shop
    /// </summary>
    [CreateAssetMenu(fileName = "New Shop", menuName = "Cozy Game/Economy/Shop", order = 10)]
    public class Shop : ScriptableObject
    {
        [Header("Shop Info")]
        [Tooltip("Shop name")]
        public string shopName = "General Store";

        [Tooltip("Unique shop ID")]
        public string shopID;

        [Tooltip("Shop description")]
        [TextArea(2, 4)]
        public string description = "A shop selling various goods...";

        [Tooltip("Shop icon")]
        public Sprite icon;

        [Header("Items")]
        [Tooltip("Items sold in this shop")]
        public ShopItem[] shopItems;

        [Header("Buy/Sell Settings")]
        [Tooltip("Allow selling items to this shop")]
        public bool allowSelling = true;

        [Tooltip("Sell price multiplier (0-1)")]
        [Range(0f, 1f)]
        public float sellPriceMultiplier = 0.5f;

        [Tooltip("Buy price multiplier (affects all items)")]
        [Range(0.5f, 2f)]
        public float buyPriceMultiplier = 1f;

        [Header("Requirements")]
        [Tooltip("Required quest to unlock shop")]
        public string requiredQuestID = "";

        [Tooltip("Required level to access shop")]
        public int requiredLevel = 1;

        [Header("Dynamic Pricing")]
        [Tooltip("Enable dynamic pricing")]
        public bool enableDynamicPricing = false;

        [Tooltip("Price variance range (0-1)")]
        [Range(0f, 0.5f)]
        public float priceVariance = 0.1f;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(shopID))
            {
                shopID = "shop_" + name.ToLower().Replace(" ", "_");
            }

            // Initialize all shop items
            if (shopItems != null)
            {
                foreach (var shopItem in shopItems)
                {
                    if (shopItem != null)
                    {
                        shopItem.Initialize();
                    }
                }
            }
        }

        /// <summary>
        /// Get buy price for item
        /// </summary>
        public int GetBuyPrice(ShopItem shopItem)
        {
            if (shopItem == null)
                return 0;

            float price = shopItem.buyPrice * buyPriceMultiplier;

            // Apply dynamic pricing
            if (enableDynamicPricing)
            {
                float variance = Random.Range(-priceVariance, priceVariance);
                price *= (1f + variance);
            }

            return Mathf.RoundToInt(price);
        }

        /// <summary>
        /// Get sell price for item
        /// </summary>
        public int GetSellPrice(ShopItem shopItem)
        {
            if (shopItem == null || shopItem.sellPrice == 0)
                return 0;

            return Mathf.RoundToInt(shopItem.sellPrice * sellPriceMultiplier);
        }

        /// <summary>
        /// Get sell price for any item (not in shop inventory)
        /// </summary>
        public int GetItemSellPrice(Item item)
        {
            if (!allowSelling || item == null)
                return 0;

            // Check if item is in shop inventory
            foreach (var shopItem in shopItems)
            {
                if (shopItem.item == item)
                {
                    return GetSellPrice(shopItem);
                }
            }

            // Use base value with multiplier
            int baseValue = item.baseValue > 0 ? item.baseValue : 1;
            return Mathf.RoundToInt(baseValue * sellPriceMultiplier);
        }

        /// <summary>
        /// Check if shop is accessible
        /// </summary>
        public bool CanAccess(out string reason)
        {
            reason = "";

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                reason = $"Requires level {requiredLevel}";
                return false;
            }

            // Check quest requirement
            if (!string.IsNullOrEmpty(requiredQuestID))
            {
                // TODO: Check if quest is completed
                // For now, assume accessible
            }

            return true;
        }

        /// <summary>
        /// Update all shop items (restock timers)
        /// </summary>
        public void UpdateShop(float deltaTime)
        {
            if (shopItems == null)
                return;

            foreach (var shopItem in shopItems)
            {
                if (shopItem != null)
                {
                    shopItem.UpdateRestock(deltaTime);
                }
            }
        }

        /// <summary>
        /// Get shop item by item ID
        /// </summary>
        public ShopItem GetShopItem(string itemID)
        {
            if (shopItems == null)
                return null;

            foreach (var shopItem in shopItems)
            {
                if (shopItem != null && shopItem.item != null && shopItem.item.itemID == itemID)
                {
                    return shopItem;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all available shop items (unlocked & in stock)
        /// </summary>
        public List<ShopItem> GetAvailableItems()
        {
            List<ShopItem> available = new List<ShopItem>();

            if (shopItems == null)
                return available;

            foreach (var shopItem in shopItems)
            {
                if (shopItem != null && shopItem.isUnlocked && shopItem.IsInStock())
                {
                    // Check level requirement
                    if (PlayerStats.Instance == null || PlayerStats.Instance.level >= shopItem.requiredLevel)
                    {
                        available.Add(shopItem);
                    }
                }
            }

            return available;
        }

        /// <summary>
        /// Unlock shop item
        /// </summary>
        public void UnlockItem(string itemID)
        {
            ShopItem shopItem = GetShopItem(itemID);
            if (shopItem != null)
            {
                shopItem.isUnlocked = true;
                Debug.Log($"[Shop] Unlocked {shopItem.item.itemName} in {shopName}");
            }
        }
    }
}
