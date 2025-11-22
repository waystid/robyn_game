using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.UI
{
    /// <summary>
    /// Inventory UI controller.
    /// Manages inventory panel and slot displays.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [Tooltip("Inventory panel")]
        public GameObject inventoryPanel;

        [Header("Inventory Grid")]
        [Tooltip("Slots container (Grid Layout Group)")]
        public Transform slotsContainer;

        [Tooltip("Inventory slot prefab")]
        public GameObject slotPrefab;

        [Header("Info Display")]
        [Tooltip("Weight text (current/max)")]
        public TextMeshProUGUI weightText;

        [Tooltip("Slot count text")]
        public TextMeshProUGUI slotCountText;

        [Header("Buttons")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Sort button")]
        public Button sortButton;

        [Tooltip("Drop button")]
        public Button dropButton;

        [Header("Audio")]
        [Tooltip("Button click sound")]
        public string buttonClickSound = "button_click";

        [Tooltip("Item pickup sound")]
        public string itemPickupSound = "item_pickup";

        [Tooltip("Item drop sound")]
        public string itemDropSound = "item_drop";

        // Slot management
        private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
        private int selectedSlotIndex = -1;

        private void Start()
        {
            // Setup button listeners
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (sortButton != null)
            {
                sortButton.onClick.AddListener(OnSortClicked);
            }

            if (dropButton != null)
            {
                dropButton.onClick.AddListener(OnDropClicked);
            }

            // Subscribe to inventory events
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged.AddListener(OnInventoryChanged);
                InventorySystem.Instance.OnItemAdded.AddListener(OnItemAdded);
                InventorySystem.Instance.OnItemRemoved.AddListener(OnItemRemoved);
            }

            // Initialize slots
            InitializeSlots();

            // Hide panel initially
            Hide();
        }

        private void Update()
        {
            // Toggle inventory with I key
            if (Input.GetKeyDown(KeyCode.I))
            {
                Toggle();
            }

            // Drop selected item with X key
            if (Input.GetKeyDown(KeyCode.X) && selectedSlotIndex >= 0)
            {
                OnDropClicked();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.OnInventoryChanged.RemoveListener(OnInventoryChanged);
                InventorySystem.Instance.OnItemAdded.RemoveListener(OnItemAdded);
                InventorySystem.Instance.OnItemRemoved.RemoveListener(OnItemRemoved);
            }
        }

        /// <summary>
        /// Initialize inventory slot UIs
        /// </summary>
        private void InitializeSlots()
        {
            if (InventorySystem.Instance == null || slotsContainer == null || slotPrefab == null)
            {
                Debug.LogError("[InventoryUI] Missing required components!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            slotUIs.Clear();

            // Create slot UIs
            int slotCount = InventorySystem.Instance.maxInventorySlots;
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();

                if (slotUI != null)
                {
                    slotUI.Initialize(i, OnSlotClicked, OnSlotSwapped);
                    slotUIs.Add(slotUI);
                }
            }

            // Refresh all slots
            RefreshAllSlots();
        }

        /// <summary>
        /// Refresh all slot displays
        /// </summary>
        public void RefreshAllSlots()
        {
            if (InventorySystem.Instance == null)
                return;

            List<InventorySlot> slots = InventorySystem.Instance.GetAllSlots();

            for (int i = 0; i < slotUIs.Count && i < slots.Count; i++)
            {
                slotUIs[i].SetSlotData(slots[i]);
            }

            UpdateInfoDisplay();
        }

        /// <summary>
        /// Refresh specific slot
        /// </summary>
        private void OnInventoryChanged(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < slotUIs.Count && InventorySystem.Instance != null)
            {
                InventorySlot slot = InventorySystem.Instance.GetSlot(slotIndex);
                slotUIs[slotIndex].SetSlotData(slot);
                UpdateInfoDisplay();
            }
        }

        /// <summary>
        /// Update info display (weight, slot count)
        /// </summary>
        private void UpdateInfoDisplay()
        {
            if (InventorySystem.Instance == null)
                return;

            // Update weight
            if (weightText != null && InventorySystem.Instance.enableWeightLimit)
            {
                float current = InventorySystem.Instance.GetCurrentWeight();
                float max = InventorySystem.Instance.maxWeight;
                weightText.text = $"Weight: {current:F1}/{max:F1}";

                // Color code based on weight percentage
                float percentage = InventorySystem.Instance.GetWeightPercentage();
                if (percentage >= 0.9f)
                {
                    weightText.color = Color.red;
                }
                else if (percentage >= 0.7f)
                {
                    weightText.color = Color.yellow;
                }
                else
                {
                    weightText.color = Color.white;
                }
            }
            else if (weightText != null)
            {
                weightText.text = "";
            }

            // Update slot count
            if (slotCountText != null)
            {
                int empty = InventorySystem.Instance.GetEmptySlotCount();
                int total = InventorySystem.Instance.maxInventorySlots;
                int occupied = total - empty;

                slotCountText.text = $"Slots: {occupied}/{total}";
            }
        }

        /// <summary>
        /// Item added callback
        /// </summary>
        private void OnItemAdded(string itemID, int quantity)
        {
            PlaySound(itemPickupSound);
        }

        /// <summary>
        /// Item removed callback
        /// </summary>
        private void OnItemRemoved(string itemID, int quantity)
        {
            PlaySound(itemDropSound);
        }

        /// <summary>
        /// Slot clicked callback
        /// </summary>
        private void OnSlotClicked(int slotIndex)
        {
            PlayButtonSound();

            // Deselect if clicking same slot
            if (selectedSlotIndex == slotIndex)
            {
                selectedSlotIndex = -1;
                UpdateSelection();
                return;
            }

            // Select new slot
            selectedSlotIndex = slotIndex;
            UpdateSelection();
        }

        /// <summary>
        /// Slot swapped callback (drag & drop)
        /// </summary>
        private void OnSlotSwapped(int fromSlot, int toSlot)
        {
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.MoveItem(fromSlot, toSlot);
                PlayButtonSound();
            }
        }

        /// <summary>
        /// Update slot selection visuals
        /// </summary>
        private void UpdateSelection()
        {
            for (int i = 0; i < slotUIs.Count; i++)
            {
                slotUIs[i].SetSelected(i == selectedSlotIndex);
            }
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseClicked()
        {
            PlayButtonSound();
            Hide();
        }

        /// <summary>
        /// Sort button clicked
        /// </summary>
        private void OnSortClicked()
        {
            PlayButtonSound();
            // TODO: Implement inventory sorting
            Debug.Log("[InventoryUI] Sort not yet implemented");
        }

        /// <summary>
        /// Drop button clicked
        /// </summary>
        private void OnDropClicked()
        {
            if (selectedSlotIndex < 0)
            {
                Debug.LogWarning("[InventoryUI] No item selected to drop");
                return;
            }

            PlayButtonSound();

            if (InventorySystem.Instance != null)
            {
                bool dropped = InventorySystem.Instance.DropItem(selectedSlotIndex, 1);
                if (dropped)
                {
                    selectedSlotIndex = -1;
                    UpdateSelection();
                }
            }
        }

        /// <summary>
        /// Show inventory panel
        /// </summary>
        public void Show()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(true);
                RefreshAllSlots();

                // Show cursor
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetCursorState(true, CursorLockMode.None);
                }
            }
        }

        /// <summary>
        /// Hide inventory panel
        /// </summary>
        public void Hide()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);

                // Hide cursor if in gameplay
                if (GameManager.Instance != null && GameManager.Instance.IsPlaying())
                {
                    GameManager.Instance.SetCursorState(false, CursorLockMode.Locked);
                }
            }
        }

        /// <summary>
        /// Toggle inventory panel
        /// </summary>
        public void Toggle()
        {
            if (inventoryPanel.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonSound()
        {
            PlaySound(buttonClickSound);
        }

        /// <summary>
        /// Play sound
        /// </summary>
        private void PlaySound(string soundName)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioManager.Instance.PlaySound(soundName);
            }
        }
    }
}
