using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using CozyGame.Inventory;

namespace CozyGame.UI
{
    /// <summary>
    /// Individual inventory slot UI component.
    /// Handles display, drag & drop, and interaction.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [Tooltip("Item icon image")]
        public Image itemIcon;

        [Tooltip("Quantity text")]
        public TextMeshProUGUI quantityText;

        [Tooltip("Empty slot background")]
        public Image slotBackground;

        [Tooltip("Selected highlight")]
        public GameObject selectedHighlight;

        [Tooltip("Rarity border (optional)")]
        public Image rarityBorder;

        [Header("Colors")]
        [Tooltip("Empty slot color")]
        public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);

        [Tooltip("Occupied slot color")]
        public Color occupiedSlotColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);

        // State
        private int slotIndex = -1;
        private InventorySlot slotData;
        private bool isSelected = false;

        // Drag & drop
        private GameObject dragIcon;
        private Canvas canvas;

        // Callbacks
        private System.Action<int> onSlotClicked;
        private System.Action<int, int> onSlotSwapped; // from, to

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// Initialize slot
        /// </summary>
        public void Initialize(int index, System.Action<int> clickCallback, System.Action<int, int> swapCallback)
        {
            slotIndex = index;
            onSlotClicked = clickCallback;
            onSlotSwapped = swapCallback;

            RefreshDisplay();
        }

        /// <summary>
        /// Set slot data
        /// </summary>
        public void SetSlotData(InventorySlot slot)
        {
            slotData = slot;
            RefreshDisplay();
        }

        /// <summary>
        /// Refresh visual display
        /// </summary>
        public void RefreshDisplay()
        {
            bool isEmpty = (slotData == null || slotData.IsEmpty());

            // Update icon
            if (itemIcon != null)
            {
                if (isEmpty || slotData.itemData == null || slotData.itemData.icon == null)
                {
                    itemIcon.enabled = false;
                }
                else
                {
                    itemIcon.enabled = true;
                    itemIcon.sprite = slotData.itemData.icon;
                }
            }

            // Update quantity
            if (quantityText != null)
            {
                if (isEmpty || slotData.quantity <= 1)
                {
                    quantityText.text = "";
                }
                else
                {
                    quantityText.text = slotData.quantity.ToString();
                }
            }

            // Update background color
            if (slotBackground != null)
            {
                slotBackground.color = isEmpty ? emptySlotColor : occupiedSlotColor;
            }

            // Update rarity border
            if (rarityBorder != null)
            {
                if (isEmpty || slotData.itemData == null)
                {
                    rarityBorder.enabled = false;
                }
                else
                {
                    rarityBorder.enabled = true;
                    rarityBorder.color = slotData.itemData.GetRarityColor();
                }
            }

            // Update selection
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(isSelected);
            }
        }

        /// <summary>
        /// Set selected state
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(selected);
            }
        }

        /// <summary>
        /// Pointer click handler
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Left click - select/use
                onSlotClicked?.Invoke(slotIndex);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Right click - use item
                UseItem();
            }
        }

        /// <summary>
        /// Use item in this slot
        /// </summary>
        private void UseItem()
        {
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.UseItem(slotIndex);
            }
        }

        /// <summary>
        /// Begin drag handler
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slotData == null || slotData.IsEmpty())
                return;

            // Create drag icon
            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(canvas.transform, false);
            dragIcon.transform.SetAsLastSibling();

            Image dragImage = dragIcon.AddComponent<Image>();
            dragImage.sprite = slotData.itemData.icon;
            dragImage.raycastTarget = false;
            dragImage.SetNativeSize();

            // Set alpha
            Color color = dragImage.color;
            color.a = 0.6f;
            dragImage.color = color;

            // Set position
            dragIcon.transform.position = eventData.position;
        }

        /// <summary>
        /// Drag handler
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                dragIcon.transform.position = eventData.position;
            }
        }

        /// <summary>
        /// End drag handler
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            // Destroy drag icon
            if (dragIcon != null)
            {
                Destroy(dragIcon);
            }

            // Check if dropped on another slot
            GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
            if (dropTarget != null)
            {
                InventorySlotUI targetSlot = dropTarget.GetComponent<InventorySlotUI>();
                if (targetSlot != null && targetSlot.slotIndex != slotIndex)
                {
                    // Swap slots
                    onSlotSwapped?.Invoke(slotIndex, targetSlot.slotIndex);
                }
            }
        }

        /// <summary>
        /// Pointer enter handler (show tooltip)
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (slotData != null && !slotData.IsEmpty() && slotData.itemData != null)
            {
                if (ItemTooltipUI.Instance != null)
                {
                    ItemTooltipUI.Instance.ShowTooltip(slotData.itemData, transform.position);
                }
            }
        }

        /// <summary>
        /// Pointer exit handler (hide tooltip)
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.HideTooltip();
            }
        }

        /// <summary>
        /// Get slot index
        /// </summary>
        public int GetSlotIndex()
        {
            return slotIndex;
        }

        /// <summary>
        /// Check if slot is empty
        /// </summary>
        public bool IsEmpty()
        {
            return slotData == null || slotData.IsEmpty();
        }
    }
}
