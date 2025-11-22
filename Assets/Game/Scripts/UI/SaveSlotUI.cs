using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CozyGame.SaveSystem;

namespace CozyGame.UI
{
    /// <summary>
    /// Individual save slot UI element.
    /// Displays save metadata and handles slot interaction.
    /// </summary>
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Slot background button")]
        public Button slotButton;

        [Tooltip("Save name text")]
        public TextMeshProUGUI saveNameText;

        [Tooltip("Save date/time text")]
        public TextMeshProUGUI dateTimeText;

        [Tooltip("Playtime text")]
        public TextMeshProUGUI playtimeText;

        [Tooltip("Player level text")]
        public TextMeshProUGUI levelText;

        [Tooltip("Empty slot text")]
        public GameObject emptySlotIndicator;

        [Tooltip("Delete button")]
        public Button deleteButton;

        [Header("Visual States")]
        [Tooltip("Empty slot color")]
        public Color emptySlotColor = Color.gray;

        [Tooltip("Occupied slot color")]
        public Color occupiedSlotColor = Color.white;

        [Tooltip("Selected slot color")]
        public Color selectedSlotColor = Color.yellow;

        // State
        private int slotIndex = -1;
        private SaveMetadata saveMetadata;
        private bool isEmpty = true;
        private bool isSelected = false;

        // Callbacks
        private System.Action<int> onSlotSelected;
        private System.Action<int> onSlotDeleted;

        /// <summary>
        /// Initialize slot with metadata
        /// </summary>
        public void Initialize(int index, SaveMetadata metadata, System.Action<int> selectCallback, System.Action<int> deleteCallback)
        {
            slotIndex = index;
            saveMetadata = metadata;
            onSlotSelected = selectCallback;
            onSlotDeleted = deleteCallback;

            isEmpty = (metadata == null);

            // Setup button listeners
            if (slotButton != null)
            {
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(OnSlotClicked);
            }

            if (deleteButton != null)
            {
                deleteButton.onClick.RemoveAllListeners();
                deleteButton.onClick.AddListener(OnDeleteClicked);
                deleteButton.gameObject.SetActive(!isEmpty);
            }

            RefreshDisplay();
        }

        /// <summary>
        /// Refresh display with current data
        /// </summary>
        public void RefreshDisplay()
        {
            if (isEmpty)
            {
                // Empty slot
                if (emptySlotIndicator != null)
                    emptySlotIndicator.SetActive(true);

                if (saveNameText != null)
                {
                    saveNameText.text = "Empty Slot";
                    saveNameText.gameObject.SetActive(false);
                }

                if (dateTimeText != null)
                    dateTimeText.gameObject.SetActive(false);

                if (playtimeText != null)
                    playtimeText.gameObject.SetActive(false);

                if (levelText != null)
                    levelText.gameObject.SetActive(false);

                if (deleteButton != null)
                    deleteButton.gameObject.SetActive(false);

                SetSlotColor(emptySlotColor);
            }
            else
            {
                // Occupied slot
                if (emptySlotIndicator != null)
                    emptySlotIndicator.SetActive(false);

                if (saveNameText != null)
                {
                    saveNameText.text = saveMetadata.saveName;
                    saveNameText.gameObject.SetActive(true);
                }

                if (dateTimeText != null)
                {
                    dateTimeText.text = saveMetadata.GetDateTimeFormatted();
                    dateTimeText.gameObject.SetActive(true);
                }

                if (playtimeText != null)
                {
                    playtimeText.text = $"Playtime: {saveMetadata.GetPlaytimeFormatted()}";
                    playtimeText.gameObject.SetActive(true);
                }

                if (levelText != null)
                {
                    levelText.text = $"Level {saveMetadata.playerLevel}";
                    levelText.gameObject.SetActive(true);
                }

                if (deleteButton != null)
                    deleteButton.gameObject.SetActive(true);

                SetSlotColor(isSelected ? selectedSlotColor : occupiedSlotColor);
            }
        }

        /// <summary>
        /// Set selected state
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            RefreshDisplay();
        }

        /// <summary>
        /// Set slot color
        /// </summary>
        private void SetSlotColor(Color color)
        {
            if (slotButton != null)
            {
                ColorBlock colors = slotButton.colors;
                colors.normalColor = color;
                slotButton.colors = colors;
            }
        }

        /// <summary>
        /// Slot clicked callback
        /// </summary>
        private void OnSlotClicked()
        {
            onSlotSelected?.Invoke(slotIndex);
        }

        /// <summary>
        /// Delete button clicked
        /// </summary>
        private void OnDeleteClicked()
        {
            onSlotDeleted?.Invoke(slotIndex);
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
            return isEmpty;
        }
    }
}
