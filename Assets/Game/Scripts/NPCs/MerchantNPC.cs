using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;
using CozyGame.Economy;
using CozyGame.Dialogue;

namespace CozyGame.NPCs
{
    /// <summary>
    /// Merchant data for trade items
    /// </summary>
    [System.Serializable]
    public class MerchantItem
    {
        [Tooltip("Item to sell")]
        public Item item;

        [Tooltip("Buy price (what player pays)")]
        public int buyPrice = 100;

        [Tooltip("Sell price (what player receives)")]
        public int sellPrice = 50;

        [Tooltip("Currency type")]
        public CurrencyType currencyType = CurrencyType.Gold;

        [Tooltip("Stock amount (-1 = infinite)")]
        public int stock = -1;

        [Tooltip("Restock time in minutes (0 = never restocks)")]
        public float restockTime = 0f;

        [Tooltip("Required friendship level to unlock")]
        public int requiredFriendship = 0;

        [Tooltip("Is this item available for purchase")]
        public bool isAvailable = true;

        // Runtime state
        [System.NonSerialized]
        public int currentStock;

        [System.NonSerialized]
        public float lastRestockTime = -999999f;

        public MerchantItem()
        {
            currentStock = stock;
        }

        /// <summary>
        /// Check if item is in stock
        /// </summary>
        public bool IsInStock()
        {
            if (stock == -1) return true; // Infinite stock
            return currentStock > 0;
        }

        /// <summary>
        /// Purchase item (reduce stock)
        /// </summary>
        public bool Purchase(int quantity = 1)
        {
            if (stock == -1) return true; // Infinite stock

            if (currentStock >= quantity)
            {
                currentStock -= quantity;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sell item (increase stock)
        /// </summary>
        public void Sell(int quantity = 1)
        {
            if (stock == -1) return; // Infinite stock doesn't need tracking

            currentStock += quantity;
        }

        /// <summary>
        /// Update restock
        /// </summary>
        public void UpdateRestock()
        {
            if (restockTime <= 0 || stock == -1)
                return;

            if (currentStock >= stock)
                return; // Already at max

            float timeSinceRestock = Time.time - lastRestockTime;
            float restockSeconds = restockTime * 60f;

            if (timeSinceRestock >= restockSeconds)
            {
                currentStock = stock;
                lastRestockTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Merchant NPC that can buy and sell items.
    /// Extends NPCInteractable with shop functionality.
    /// </summary>
    public class MerchantNPC : NPCInteractable
    {
        [Header("Merchant Settings")]
        [Tooltip("Items this merchant sells")]
        public List<MerchantItem> shopInventory = new List<MerchantItem>();

        [Tooltip("Items this merchant will buy from player")]
        public List<Item> buyList = new List<Item>();

        [Tooltip("Buy price multiplier (what merchant pays for items)")]
        [Range(0.1f, 1f)]
        public float buyPriceMultiplier = 0.5f;

        [Tooltip("Sell price multiplier (what player pays)")]
        [Range(1f, 5f)]
        public float sellPriceMultiplier = 1.5f;

        [Tooltip("Greeting dialogue when opening shop")]
        public DialogueData shopGreeting;

        [Tooltip("Dialogue when player can't afford item")]
        public DialogueData cantAffordDialogue;

        [Tooltip("Dialogue when shop is closing")]
        public DialogueData farewellDialogue;

        [Header("Shop Hours")]
        [Tooltip("Is shop always open?")]
        public bool alwaysOpen = true;

        [Tooltip("Opening hour (24-hour format)")]
        [Range(0, 23)]
        public int openHour = 8;

        [Tooltip("Closing hour (24-hour format)")]
        [Range(0, 23)]
        public int closeHour = 20;

        [Header("Relationship Bonuses")]
        [Tooltip("Discount percentage per friendship level")]
        [Range(0f, 0.1f)]
        public float discountPerLevel = 0.02f; // 2% per level

        [Tooltip("Max discount percentage")]
        [Range(0f, 0.5f)]
        public float maxDiscount = 0.3f; // 30% max

        // Events
        public UnityEvent<MerchantItem> OnItemPurchased;
        public UnityEvent<Item, int> OnItemSold;
        public UnityEvent OnShopOpened;
        public UnityEvent OnShopClosed;

        protected override void Start()
        {
            base.Start();

            // Initialize stock
            foreach (var item in shopInventory)
            {
                item.currentStock = item.stock;
            }
        }

        protected virtual void Update()
        {
            base.Update();

            // Update restocking
            foreach (var item in shopInventory)
            {
                item.UpdateRestock();
            }
        }

        /// <summary>
        /// Override interact to open shop
        /// </summary>
        public override void Interact()
        {
            // Check shop hours
            if (!IsShopOpen())
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Shop is closed. Open from {openHour}:00 to {closeHour}:00",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return;
            }

            // Play greeting dialogue if available
            if (shopGreeting != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(shopGreeting);
            }

            // Open shop UI
            OpenShop();
        }

        /// <summary>
        /// Open shop UI
        /// </summary>
        public virtual void OpenShop()
        {
            if (UI.ShopUI.Instance != null)
            {
                UI.ShopUI.Instance.OpenShop(this);
                OnShopOpened?.Invoke();
            }
            else
            {
                Debug.LogWarning("[MerchantNPC] ShopUI not found!");
            }
        }

        /// <summary>
        /// Close shop UI
        /// </summary>
        public virtual void CloseShop()
        {
            if (UI.ShopUI.Instance != null)
            {
                UI.ShopUI.Instance.CloseShop();
                OnShopClosed?.Invoke();
            }

            // Play farewell dialogue
            if (farewellDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(farewellDialogue);
            }
        }

        /// <summary>
        /// Purchase item from merchant
        /// </summary>
        public bool PurchaseItem(MerchantItem merchantItem, int quantity = 1)
        {
            if (merchantItem == null || !merchantItem.IsInStock())
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Item out of stock!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Calculate final price with discounts
            int totalPrice = CalculateBuyPrice(merchantItem, quantity);

            // Check if player can afford
            if (!CurrencyManager.Instance.HasCurrency(merchantItem.currencyType, totalPrice))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need {totalPrice} {merchantItem.currencyType}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }

                // Play can't afford dialogue
                if (cantAffordDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(cantAffordDialogue);
                }

                return false;
            }

            // Check inventory space
            if (InventoryManager.Instance != null)
            {
                if (!InventoryManager.Instance.CanAddItem(merchantItem.item, quantity))
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            "Inventory full!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }
            }

            // Complete purchase
            CurrencyManager.Instance.RemoveCurrency(merchantItem.currencyType, totalPrice);
            merchantItem.Purchase(quantity);

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(merchantItem.item, quantity);
            }

            // Show success message
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Purchased {quantity}x {merchantItem.item.itemName}",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.green
                );
            }

            // Play purchase sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("purchase");
            }

            OnItemPurchased?.Invoke(merchantItem);

            return true;
        }

        /// <summary>
        /// Sell item to merchant
        /// </summary>
        public bool SellItem(Item item, int quantity = 1)
        {
            if (item == null)
                return false;

            // Check if merchant buys this item
            if (!buyList.Contains(item))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "I don't buy that item.",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return false;
            }

            // Check if player has the item
            if (InventoryManager.Instance != null)
            {
                if (!InventoryManager.Instance.HasItem(item, quantity))
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            "You don't have enough to sell!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }
            }

            // Calculate sell price
            int sellPrice = CalculateSellPrice(item, quantity);

            // Remove from player inventory
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(item, quantity);
            }

            // Give currency to player
            CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, sellPrice);

            // Update merchant stock if item is in shop inventory
            MerchantItem merchantItem = shopInventory.Find(mi => mi.item == item);
            if (merchantItem != null)
            {
                merchantItem.Sell(quantity);
            }

            // Show success message
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Sold {quantity}x {item.itemName} for {sellPrice} Gold",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.green
                );
            }

            // Play sell sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("sell");
            }

            OnItemSold?.Invoke(item, quantity);

            return true;
        }

        /// <summary>
        /// Calculate buy price with discounts
        /// </summary>
        public int CalculateBuyPrice(MerchantItem merchantItem, int quantity = 1)
        {
            float basePrice = merchantItem.buyPrice * quantity;

            // Apply relationship discount
            float discount = GetRelationshipDiscount();
            float finalPrice = basePrice * (1f - discount);

            return Mathf.RoundToInt(finalPrice);
        }

        /// <summary>
        /// Calculate sell price (what player receives)
        /// </summary>
        public int CalculateSellPrice(Item item, int quantity = 1)
        {
            // Base sell price is typically lower than buy price
            float basePrice = item.value * buyPriceMultiplier * quantity;

            // Relationship can slightly increase sell prices
            float discount = GetRelationshipDiscount();
            float bonus = discount * 0.5f; // Half the discount as a bonus
            float finalPrice = basePrice * (1f + bonus);

            return Mathf.RoundToInt(finalPrice);
        }

        /// <summary>
        /// Get relationship discount percentage
        /// </summary>
        public float GetRelationshipDiscount()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            float discount = relationship.friendshipLevel * discountPerLevel;
            return Mathf.Min(discount, maxDiscount);
        }

        /// <summary>
        /// Check if shop is open
        /// </summary>
        public bool IsShopOpen()
        {
            if (alwaysOpen)
                return true;

            if (TimeWeather.TimeSystem.Instance != null)
            {
                int currentHour = TimeWeather.TimeSystem.Instance.GetCurrentHour();

                if (closeHour > openHour)
                {
                    return currentHour >= openHour && currentHour < closeHour;
                }
                else
                {
                    // Handle overnight hours (e.g., 20:00 to 2:00)
                    return currentHour >= openHour || currentHour < closeHour;
                }
            }

            return true; // Default to open if no time system
        }

        /// <summary>
        /// Get available items for current friendship level
        /// </summary>
        public List<MerchantItem> GetAvailableItems()
        {
            List<MerchantItem> available = new List<MerchantItem>();

            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }
            }

            foreach (var item in shopInventory)
            {
                if (item.isAvailable && friendshipLevel >= item.requiredFriendship)
                {
                    available.Add(item);
                }
            }

            return available;
        }
    }
}
