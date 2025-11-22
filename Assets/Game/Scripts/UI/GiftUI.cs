using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Inventory;
using CozyGame.Social;

namespace CozyGame.UI
{
    /// <summary>
    /// Gift UI controller.
    /// Shows gift selection interface for NPCs.
    /// </summary>
    public class GiftUI : MonoBehaviour
    {
        public static GiftUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main gift panel")]
        public GameObject giftPanel;

        [Tooltip("Gift confirmation panel")]
        public GameObject confirmationPanel;

        [Header("NPC Info")]
        [Tooltip("NPC name text")]
        public TextMeshProUGUI npcNameText;

        [Tooltip("NPC portrait image")]
        public Image npcPortraitImage;

        [Tooltip("Friendship level text")]
        public TextMeshProUGUI friendshipLevelText;

        [Tooltip("Friendship progress bar")]
        public Slider friendshipProgressBar;

        [Header("Item Selection")]
        [Tooltip("Gift item list container")]
        public RectTransform giftItemListContainer;

        [Tooltip("Gift item button prefab")]
        public GameObject giftItemButtonPrefab;

        [Tooltip("Empty inventory message")]
        public GameObject emptyInventoryMessage;

        [Header("Item Preview")]
        [Tooltip("Selected item icon")]
        public Image selectedItemIcon;

        [Tooltip("Selected item name")]
        public TextMeshProUGUI selectedItemNameText;

        [Tooltip("Selected item description")]
        public TextMeshProUGUI selectedItemDescriptionText;

        [Tooltip("Gift value text (loved/liked/etc)")]
        public TextMeshProUGUI giftValueText;

        [Tooltip("Give button")]
        public Button giveButton;

        [Header("Controls")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Back button (from confirmation)")]
        public Button backButton;

        [Header("Confirmation")]
        [Tooltip("Confirmation item icon")]
        public Image confirmItemIcon;

        [Tooltip("Confirmation item name")]
        public TextMeshProUGUI confirmItemNameText;

        [Tooltip("Confirmation message text")]
        public TextMeshProUGUI confirmMessageText;

        [Tooltip("Confirm button")]
        public Button confirmButton;

        [Tooltip("Cancel button")]
        public Button cancelButton;

        // State
        private NPCRelationship currentNPC;
        private Item selectedItem;
        private List<GameObject> itemButtons = new List<GameObject>();

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
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseGiftUI);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (giveButton != null)
            {
                giveButton.onClick.AddListener(OnGiveClicked);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmGift);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelGift);
            }

            // Hide panels initially
            if (giftPanel != null)
            {
                giftPanel.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Open gift UI for NPC
        /// </summary>
        public void OpenGiftUI(string npcName)
        {
            if (RelationshipSystem.Instance == null)
            {
                Debug.LogWarning("[GiftUI] RelationshipSystem not found!");
                return;
            }

            currentNPC = RelationshipSystem.Instance.GetOrCreateRelationship(npcName);

            if (currentNPC == null)
            {
                Debug.LogWarning($"[GiftUI] Could not find relationship for {npcName}!");
                return;
            }

            // Check if can gift today
            if (!currentNPC.CanGiftToday())
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Already gave {npcName} a gift today!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return;
            }

            if (giftPanel != null)
            {
                giftPanel.SetActive(true);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            UpdateNPCInfo();
            RefreshItemList();
        }

        /// <summary>
        /// Close gift UI
        /// </summary>
        public void CloseGiftUI()
        {
            if (giftPanel != null)
            {
                giftPanel.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            currentNPC = null;
            selectedItem = null;
        }

        /// <summary>
        /// Update NPC info display
        /// </summary>
        private void UpdateNPCInfo()
        {
            if (currentNPC == null)
                return;

            // NPC name
            if (npcNameText != null)
            {
                npcNameText.text = currentNPC.npcName;
            }

            // Friendship level
            if (friendshipLevelText != null)
            {
                FriendshipTier tier = currentNPC.GetFriendshipTier();
                friendshipLevelText.text = $"Level {currentNPC.friendshipLevel} - {tier}";
            }

            // Friendship progress
            if (friendshipProgressBar != null)
            {
                friendshipProgressBar.value = currentNPC.GetLevelProgress();
            }

            // TODO: Set portrait image if available
        }

        /// <summary>
        /// Refresh item list
        /// </summary>
        private void RefreshItemList()
        {
            // Clear existing items
            foreach (var button in itemButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            itemButtons.Clear();

            if (InventoryManager.Instance == null || giftItemListContainer == null)
                return;

            // Get all items from inventory
            List<Item> items = InventoryManager.Instance.GetAllUniqueItems();

            if (items.Count == 0)
            {
                if (emptyInventoryMessage != null)
                {
                    emptyInventoryMessage.SetActive(true);
                }
                return;
            }

            if (emptyInventoryMessage != null)
            {
                emptyInventoryMessage.SetActive(false);
            }

            // Create item buttons
            foreach (var item in items)
            {
                CreateItemButton(item);
            }
        }

        /// <summary>
        /// Create item button
        /// </summary>
        private void CreateItemButton(Item item)
        {
            if (giftItemButtonPrefab == null || giftItemListContainer == null)
                return;

            GameObject buttonObj = Instantiate(giftItemButtonPrefab, giftItemListContainer);
            itemButtons.Add(buttonObj);

            // Find components
            Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = buttonObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI valueText = buttonObj.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
            Button button = buttonObj.GetComponent<Button>();

            // Set icon
            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = item.itemName;
            }

            // Set value indicator
            if (valueText != null && currentNPC != null)
            {
                int value = currentNPC.GetGiftValue(item);
                if (value >= 15)
                {
                    valueText.text = "❤️ Loves";
                    valueText.color = Color.magenta;
                }
                else if (value >= 8)
                {
                    valueText.text = "👍 Likes";
                    valueText.color = Color.green;
                }
                else if (value <= -10)
                {
                    valueText.text = "💔 Hates";
                    valueText.color = Color.red;
                }
                else if (value < 0)
                {
                    valueText.text = "👎 Dislikes";
                    valueText.color = new Color(1f, 0.5f, 0f); // Orange
                }
                else
                {
                    valueText.text = "🎁 Neutral";
                    valueText.color = Color.gray;
                }
            }

            // Setup button
            if (button != null)
            {
                Item i = item; // Capture for lambda
                button.onClick.AddListener(() => OnItemSelected(i));
            }
        }

        /// <summary>
        /// Item selected callback
        /// </summary>
        private void OnItemSelected(Item item)
        {
            selectedItem = item;
            UpdateItemPreview();
        }

        /// <summary>
        /// Update item preview
        /// </summary>
        private void UpdateItemPreview()
        {
            if (selectedItem == null)
            {
                if (giveButton != null)
                {
                    giveButton.interactable = false;
                }
                return;
            }

            // Item icon
            if (selectedItemIcon != null && selectedItem.icon != null)
            {
                selectedItemIcon.sprite = selectedItem.icon;
                selectedItemIcon.gameObject.SetActive(true);
            }

            // Item name
            if (selectedItemNameText != null)
            {
                selectedItemNameText.text = selectedItem.itemName;
            }

            // Item description
            if (selectedItemDescriptionText != null)
            {
                selectedItemDescriptionText.text = selectedItem.description;
            }

            // Gift value
            if (giftValueText != null && currentNPC != null)
            {
                int value = currentNPC.GetGiftValue(item);
                string valueStr = "";

                if (value >= 15)
                {
                    valueStr = "❤️ They will LOVE this! (+15 points)";
                    giftValueText.color = Color.magenta;
                }
                else if (value >= 8)
                {
                    valueStr = "👍 They will like this. (+8 points)";
                    giftValueText.color = Color.green;
                }
                else if (value <= -10)
                {
                    valueStr = "💔 They will HATE this! (-10 points)";
                    giftValueText.color = Color.red;
                }
                else if (value < 0)
                {
                    valueStr = "👎 They won't like this. (-5 points)";
                    giftValueText.color = new Color(1f, 0.5f, 0f);
                }
                else
                {
                    valueStr = "🎁 Neutral gift. (+3 points)";
                    giftValueText.color = Color.gray;
                }

                giftValueText.text = valueStr;
            }

            // Enable give button
            if (giveButton != null)
            {
                giveButton.interactable = true;
            }
        }

        /// <summary>
        /// Give button clicked
        /// </summary>
        private void OnGiveClicked()
        {
            if (selectedItem == null || currentNPC == null)
                return;

            ShowConfirmation();
        }

        /// <summary>
        /// Show confirmation panel
        /// </summary>
        private void ShowConfirmation()
        {
            if (confirmationPanel == null)
                return;

            // Hide gift panel, show confirmation
            if (giftPanel != null)
            {
                giftPanel.SetActive(false);
            }

            confirmationPanel.SetActive(true);

            // Update confirmation UI
            if (confirmItemIcon != null && selectedItem.icon != null)
            {
                confirmItemIcon.sprite = selectedItem.icon;
            }

            if (confirmItemNameText != null)
            {
                confirmItemNameText.text = selectedItem.itemName;
            }

            if (confirmMessageText != null)
            {
                confirmMessageText.text = $"Give {selectedItem.itemName} to {currentNPC.npcName}?";
            }
        }

        /// <summary>
        /// Confirm gift
        /// </summary>
        private void OnConfirmGift()
        {
            if (selectedItem == null || currentNPC == null || RelationshipSystem.Instance == null)
                return;

            // Give gift through relationship system
            bool success = RelationshipSystem.Instance.GiveGift(currentNPC.npcName, selectedItem);

            if (success)
            {
                // Close UI
                CloseGiftUI();

                // Update NPC info if still open
                UpdateNPCInfo();
            }
            else
            {
                // Show error (already shown by RelationshipSystem)
                // Return to gift selection
                OnBackClicked();
            }
        }

        /// <summary>
        /// Cancel gift
        /// </summary>
        private void OnCancelGift()
        {
            OnBackClicked();
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClicked()
        {
            // Hide confirmation, show gift panel
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            if (giftPanel != null)
            {
                giftPanel.SetActive(true);
            }
        }
    }
}
