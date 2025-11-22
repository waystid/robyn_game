using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Economy;
using CozyGame.Inventory;

namespace CozyGame.UI
{
    /// <summary>
    /// Shop UI controller.
    /// Handles buying and selling items with currency management.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        public static ShopUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Shop panel")]
        public GameObject shopPanel;

        [Tooltip("Buy tab")]
        public GameObject buyTab;

        [Tooltip("Sell tab")]
        public GameObject sellTab;

        [Header("Shop Info")]
        [Tooltip("Shop name text")]
        public TextMeshProUGUI shopNameText;

        [Tooltip("Shop description text")]
        public TextMeshProUGUI shopDescriptionText;

        [Tooltip("Shopkeeper greeting text")]
        public TextMeshProUGUI greetingText;

        [Header("Buy Panel")]
        [Tooltip("Shop item list container")]
        public Transform shopItemListContainer;

        [Tooltip("Shop item button prefab")]
        public GameObject shopItemButtonPrefab;

        [Header("Sell Panel")]
        [Tooltip("Player inventory list container")]
        public Transform sellItemListContainer;

        [Tooltip("Sell item button prefab")]
        public GameObject sellItemButtonPrefab;

        [Header("Item Details")]
        [Tooltip("Selected item name")]
        public TextMeshProUGUI itemNameText;

        [Tooltip("Selected item description")]
        public TextMeshProUGUI itemDescriptionText;

        [Tooltip("Selected item icon")]
        public Image itemIcon;

        [Tooltip("Item price text")]
        public TextMeshProUGUI itemPriceText;

        [Tooltip("Item stock text")]
        public TextMeshProUGUI itemStockText;

        [Header("Buttons")]
        [Tooltip("Buy button")]
        public Button buyButton;

        [Tooltip("Sell button")]
        public Button sellButton;

        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Buy tab button")]
        public Button buyTabButton;

        [Tooltip("Sell tab button")]
        public Button sellTabButton;

        [Header("Currency Display")]
        [Tooltip("Player gold text")]
        public TextMeshProUGUI playerGoldText;

        [Tooltip("Player gems text")]
        public TextMeshProUGUI playerGemsText;

        [Header("Quantity")]
        [Tooltip("Quantity input")]
        public TMP_InputField quantityInput;

        [Tooltip("Quantity slider")]
        public Slider quantitySlider;

        [Tooltip("Max quantity button")]
        public Button maxQuantityButton;

        // State
        private Shop currentShop;
        private ShopNPC currentShopkeeper;
        private ShopItem selectedShopItem;
        private Item selectedPlayerItem;
        private bool isBuyMode = true;
        private int purchaseQuantity = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Setup buttons
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyClicked);
            }

            if (sellButton != null)
            {
                sellButton.onClick.AddListener(OnSellClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (buyTabButton != null)
            {
                buyTabButton.onClick.AddListener(() => SwitchTab(true));
            }

            if (sellTabButton != null)
            {
                sellTabButton.onClick.AddListener(() => SwitchTab(false));
            }

            if (maxQuantityButton != null)
            {
                maxQuantityButton.onClick.AddListener(OnMaxQuantityClicked);
            }

            // Setup quantity input
            if (quantityInput != null)
            {
                quantityInput.onValueChanged.AddListener(OnQuantityInputChanged);
            }

            if (quantitySlider != null)
            {
                quantitySlider.onValueChanged.AddListener(OnQuantitySliderChanged);
            }

            // Hide panel
            Hide();
        }

        private void Update()
        {
            // Update currency display
            UpdateCurrencyDisplay();
        }

        /// <summary>
        /// Open shop
        /// </summary>
        public void OpenShop(Shop shop, ShopNPC shopkeeper)
        {
            currentShop = shop;
            currentShopkeeper = shopkeeper;

            if (shop == null)
            {
                Debug.LogWarning("[ShopUI] Cannot open null shop!");
                return;
            }

            // Update shop info
            if (shopNameText != null)
            {
                shopNameText.text = shop.shopName;
            }

            if (shopDescriptionText != null)
            {
                shopDescriptionText.text = shop.description;
            }

            if (greetingText != null && shopkeeper != null)
            {
                greetingText.text = shopkeeper.greetingMessage;
            }

            // Reset state
            purchaseQuantity = 1;
            selectedShopItem = null;
            selectedPlayerItem = null;

            // Show buy tab by default
            SwitchTab(true);

            // Show panel
            Show();

            Debug.Log($"[ShopUI] Opened shop: {shop.shopName}");
        }

        /// <summary>
        /// Switch between buy and sell tabs
        /// </summary>
        private void SwitchTab(bool buyMode)
        {
            isBuyMode = buyMode;

            // Show/hide tabs
            if (buyTab != null)
            {
                buyTab.SetActive(buyMode);
            }

            if (sellTab != null)
            {
                sellTab.SetActive(!buyMode);
            }

            // Show/hide buttons
            if (buyButton != null)
            {
                buyButton.gameObject.SetActive(buyMode);
            }

            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(!buyMode);
            }

            // Refresh lists
            if (buyMode)
            {
                RefreshShopItems();
            }
            else
            {
                RefreshSellableItems();
            }

            // Clear selection
            selectedShopItem = null;
            selectedPlayerItem = null;
            ClearItemDetails();
        }

        /// <summary>
        /// Refresh shop items (buy tab)
        /// </summary>
        private void RefreshShopItems()
        {
            if (shopItemListContainer == null || shopItemButtonPrefab == null || currentShop == null)
                return;

            // Clear existing buttons
            foreach (Transform child in shopItemListContainer)
            {
                Destroy(child.gameObject);
            }

            // Get available items
            List<ShopItem> availableItems = currentShop.GetAvailableItems();

            // Create button for each item
            foreach (var shopItem in availableItems)
            {
                if (shopItem == null || shopItem.item == null)
                    continue;

                GameObject buttonObj = Instantiate(shopItemButtonPrefab, shopItemListContainer);
                SetupShopItemButton(buttonObj, shopItem);
            }
        }

        /// <summary>
        /// Setup shop item button
        /// </summary>
        private void SetupShopItemButton(GameObject buttonObj, ShopItem shopItem)
        {
            // Find UI components
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = buttonObj.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
            Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();

            // Set name
            if (nameText != null)
            {
                nameText.text = shopItem.item.itemName;
            }

            // Set price
            if (priceText != null)
            {
                int price = currentShop.GetBuyPrice(shopItem);
                string currencySymbol = CurrencyManager.Instance != null ?
                    CurrencyManager.Instance.GetCurrencySymbol(shopItem.currencyType) :
                    shopItem.currencyType.ToString();
                priceText.text = $"{price} {currencySymbol}";
            }

            // Set icon
            if (icon != null && shopItem.item.icon != null)
            {
                icon.sprite = shopItem.item.icon;
            }

            // Add click listener
            if (button != null)
            {
                button.onClick.AddListener(() => OnShopItemSelected(shopItem));
            }
        }

        /// <summary>
        /// Refresh sellable items (sell tab)
        /// </summary>
        private void RefreshSellableItems()
        {
            if (sellItemListContainer == null || sellItemButtonPrefab == null)
                return;

            if (currentShop == null || !currentShop.allowSelling)
                return;

            // Clear existing buttons
            foreach (Transform child in sellItemListContainer)
            {
                Destroy(child.gameObject);
            }

            // Get player inventory
            if (InventorySystem.Instance == null)
                return;

            List<InventorySlot> slots = InventorySystem.Instance.GetAllSlots();

            // Create button for each item
            foreach (var slot in slots)
            {
                if (slot.item == null || slot.quantity == 0)
                    continue;

                GameObject buttonObj = Instantiate(sellItemButtonPrefab, sellItemListContainer);
                SetupSellItemButton(buttonObj, slot.item, slot.quantity);
            }
        }

        /// <summary>
        /// Setup sell item button
        /// </summary>
        private void SetupSellItemButton(GameObject buttonObj, Item item, int quantity)
        {
            // Find UI components
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = buttonObj.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI quantityText = buttonObj.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
            Image icon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();

            // Set name
            if (nameText != null)
            {
                nameText.text = item.itemName;
            }

            // Set price
            if (priceText != null)
            {
                int sellPrice = currentShop.GetItemSellPrice(item);
                priceText.text = $"{sellPrice} G";
            }

            // Set quantity
            if (quantityText != null)
            {
                quantityText.text = $"x{quantity}";
            }

            // Set icon
            if (icon != null && item.icon != null)
            {
                icon.sprite = item.icon;
            }

            // Add click listener
            if (button != null)
            {
                button.onClick.AddListener(() => OnPlayerItemSelected(item));
            }
        }

        /// <summary>
        /// Shop item selected
        /// </summary>
        private void OnShopItemSelected(ShopItem shopItem)
        {
            selectedShopItem = shopItem;
            UpdateItemDetails();
        }

        /// <summary>
        /// Player item selected for selling
        /// </summary>
        private void OnPlayerItemSelected(Item item)
        {
            selectedPlayerItem = item;
            UpdateSellItemDetails();
        }

        /// <summary>
        /// Update item details panel (buy mode)
        /// </summary>
        private void UpdateItemDetails()
        {
            if (selectedShopItem == null || selectedShopItem.item == null)
            {
                ClearItemDetails();
                return;
            }

            if (itemNameText != null)
            {
                itemNameText.text = selectedShopItem.item.itemName;
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = selectedShopItem.item.description;
            }

            if (itemIcon != null && selectedShopItem.item.icon != null)
            {
                itemIcon.sprite = selectedShopItem.item.icon;
                itemIcon.enabled = true;
            }

            if (itemPriceText != null)
            {
                int price = currentShop.GetBuyPrice(selectedShopItem);
                string currencySymbol = CurrencyManager.Instance != null ?
                    CurrencyManager.Instance.GetCurrencySymbol(selectedShopItem.currencyType) :
                    selectedShopItem.currencyType.ToString();
                itemPriceText.text = $"{price} {currencySymbol}";
            }

            if (itemStockText != null)
            {
                if (selectedShopItem.stock < 0)
                {
                    itemStockText.text = "Stock: Unlimited";
                }
                else
                {
                    itemStockText.text = $"Stock: {selectedShopItem.currentStock}";
                }
            }

            // Update buy button
            UpdateBuyButton();
        }

        /// <summary>
        /// Update sell item details
        /// </summary>
        private void UpdateSellItemDetails()
        {
            if (selectedPlayerItem == null)
            {
                ClearItemDetails();
                return;
            }

            if (itemNameText != null)
            {
                itemNameText.text = selectedPlayerItem.itemName;
            }

            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = selectedPlayerItem.description;
            }

            if (itemIcon != null && selectedPlayerItem.icon != null)
            {
                itemIcon.sprite = selectedPlayerItem.icon;
                itemIcon.enabled = true;
            }

            if (itemPriceText != null)
            {
                int sellPrice = currentShop.GetItemSellPrice(selectedPlayerItem);
                itemPriceText.text = $"{sellPrice} G";
            }

            if (itemStockText != null)
            {
                int playerCount = InventorySystem.Instance != null ?
                    InventorySystem.Instance.GetItemCount(selectedPlayerItem.itemID) : 0;
                itemStockText.text = $"You have: {playerCount}";
            }

            // Update sell button
            UpdateSellButton();
        }

        /// <summary>
        /// Clear item details
        /// </summary>
        private void ClearItemDetails()
        {
            if (itemNameText != null) itemNameText.text = "";
            if (itemDescriptionText != null) itemDescriptionText.text = "";
            if (itemPriceText != null) itemPriceText.text = "";
            if (itemStockText != null) itemStockText.text = "";
            if (itemIcon != null) itemIcon.enabled = false;
        }

        /// <summary>
        /// Update buy button state
        /// </summary>
        private void UpdateBuyButton()
        {
            if (buyButton == null || selectedShopItem == null)
                return;

            // Check if can afford
            int price = currentShop.GetBuyPrice(selectedShopItem) * purchaseQuantity;
            bool canAfford = CurrencyManager.Instance != null &&
                CurrencyManager.Instance.HasCurrency(selectedShopItem.currencyType, price);

            // Check if in stock
            bool inStock = selectedShopItem.IsInStock() &&
                (selectedShopItem.stock < 0 || selectedShopItem.currentStock >= purchaseQuantity);

            buyButton.interactable = canAfford && inStock;
        }

        /// <summary>
        /// Update sell button state
        /// </summary>
        private void UpdateSellButton()
        {
            if (sellButton == null || selectedPlayerItem == null)
                return;

            // Check if has items to sell
            bool hasItems = InventorySystem.Instance != null &&
                InventorySystem.Instance.GetItemCount(selectedPlayerItem.itemID) >= purchaseQuantity;

            sellButton.interactable = hasItems;
        }

        /// <summary>
        /// Buy button clicked
        /// </summary>
        private void OnBuyClicked()
        {
            if (selectedShopItem == null || currentShop == null)
                return;

            // Calculate price
            int price = currentShop.GetBuyPrice(selectedShopItem) * purchaseQuantity;

            // Check currency
            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.HasCurrency(selectedShopItem.currencyType, price))
            {
                Debug.Log("[ShopUI] Not enough currency!");
                ShowMessage("Not enough currency!");
                return;
            }

            // Check stock
            if (!selectedShopItem.IsInStock() ||
                (selectedShopItem.stock >= 0 && selectedShopItem.currentStock < purchaseQuantity))
            {
                Debug.Log("[ShopUI] Out of stock!");
                ShowMessage("Out of stock!");
                return;
            }

            // Purchase
            if (CurrencyManager.Instance.RemoveCurrency(selectedShopItem.currencyType, price))
            {
                // Add item to inventory
                if (InventorySystem.Instance != null)
                {
                    InventorySystem.Instance.AddItem(selectedShopItem.item.itemID, purchaseQuantity);
                }

                // Update stock
                selectedShopItem.Purchase(purchaseQuantity);

                // Notify shopkeeper
                if (currentShopkeeper != null)
                {
                    currentShopkeeper.OnPurchase(selectedShopItem);
                }

                // Refresh UI
                RefreshShopItems();
                UpdateItemDetails();

                Debug.Log($"[ShopUI] Purchased {purchaseQuantity}x {selectedShopItem.item.itemName}");
                ShowMessage($"Purchased {purchaseQuantity}x {selectedShopItem.item.itemName}!");
            }
        }

        /// <summary>
        /// Sell button clicked
        /// </summary>
        private void OnSellClicked()
        {
            if (selectedPlayerItem == null || currentShop == null)
                return;

            // Check inventory
            if (InventorySystem.Instance == null || InventorySystem.Instance.GetItemCount(selectedPlayerItem.itemID) < purchaseQuantity)
            {
                Debug.Log("[ShopUI] Not enough items to sell!");
                ShowMessage("Not enough items!");
                return;
            }

            // Calculate sell price
            int sellPrice = currentShop.GetItemSellPrice(selectedPlayerItem) * purchaseQuantity;

            // Remove from inventory
            if (InventorySystem.Instance.RemoveItem(selectedPlayerItem.itemID, purchaseQuantity))
            {
                // Add currency
                if (CurrencyManager.Instance != null)
                {
                    CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, sellPrice);
                }

                // Notify shopkeeper
                ShopItem shopItem = currentShop.GetShopItem(selectedPlayerItem.itemID);
                if (currentShopkeeper != null)
                {
                    currentShopkeeper.OnSell(shopItem);
                }

                // Update shop stock if item is in shop
                if (shopItem != null)
                {
                    shopItem.Sell(purchaseQuantity);
                }

                // Refresh UI
                RefreshSellableItems();
                UpdateSellItemDetails();

                Debug.Log($"[ShopUI] Sold {purchaseQuantity}x {selectedPlayerItem.itemName} for {sellPrice}G");
                ShowMessage($"Sold {purchaseQuantity}x {selectedPlayerItem.itemName}!");
            }
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseClicked()
        {
            Hide();

            // Notify shopkeeper
            if (currentShopkeeper != null)
            {
                currentShopkeeper.CloseShop();
            }
        }

        /// <summary>
        /// Quantity input changed
        /// </summary>
        private void OnQuantityInputChanged(string value)
        {
            if (int.TryParse(value, out int quantity))
            {
                purchaseQuantity = Mathf.Max(1, quantity);

                if (quantitySlider != null)
                {
                    quantitySlider.value = purchaseQuantity;
                }

                UpdateBuyButton();
                UpdateSellButton();
            }
        }

        /// <summary>
        /// Quantity slider changed
        /// </summary>
        private void OnQuantitySliderChanged(float value)
        {
            purchaseQuantity = Mathf.Max(1, Mathf.RoundToInt(value));

            if (quantityInput != null)
            {
                quantityInput.text = purchaseQuantity.ToString();
            }

            UpdateBuyButton();
            UpdateSellButton();
        }

        /// <summary>
        /// Max quantity button clicked
        /// </summary>
        private void OnMaxQuantityClicked()
        {
            if (isBuyMode && selectedShopItem != null)
            {
                // Max based on currency and stock
                int maxAffordable = int.MaxValue;

                if (CurrencyManager.Instance != null)
                {
                    int price = currentShop.GetBuyPrice(selectedShopItem);
                    int currency = CurrencyManager.Instance.GetCurrency(selectedShopItem.currencyType);
                    maxAffordable = price > 0 ? currency / price : int.MaxValue;
                }

                if (selectedShopItem.stock >= 0)
                {
                    maxAffordable = Mathf.Min(maxAffordable, selectedShopItem.currentStock);
                }

                purchaseQuantity = Mathf.Max(1, maxAffordable);
            }
            else if (!isBuyMode && selectedPlayerItem != null)
            {
                // Max based on inventory
                if (InventorySystem.Instance != null)
                {
                    purchaseQuantity = InventorySystem.Instance.GetItemCount(selectedPlayerItem.itemID);
                }
            }

            if (quantityInput != null)
            {
                quantityInput.text = purchaseQuantity.ToString();
            }

            if (quantitySlider != null)
            {
                quantitySlider.value = purchaseQuantity;
            }

            UpdateBuyButton();
            UpdateSellButton();
        }

        /// <summary>
        /// Update currency display
        /// </summary>
        private void UpdateCurrencyDisplay()
        {
            if (CurrencyManager.Instance == null)
                return;

            if (playerGoldText != null)
            {
                playerGoldText.text = CurrencyManager.Instance.GetFormattedCurrency(CurrencyType.Gold);
            }

            if (playerGemsText != null)
            {
                playerGemsText.text = CurrencyManager.Instance.GetFormattedCurrency(CurrencyType.Gems);
            }
        }

        /// <summary>
        /// Show message to player
        /// </summary>
        private void ShowMessage(string message)
        {
            if (FloatingTextManager.Instance != null && PlayerController.Instance != null)
            {
                FloatingTextManager.Instance.Show(message, PlayerController.Instance.transform.position + Vector3.up * 2f, Color.white);
            }
        }

        /// <summary>
        /// Show shop panel
        /// </summary>
        public void Show()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide shop panel
        /// </summary>
        public void Hide()
        {
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            currentShop = null;
            currentShopkeeper = null;
        }
    }
}
