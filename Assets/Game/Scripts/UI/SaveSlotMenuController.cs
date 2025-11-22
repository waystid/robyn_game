using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.SaveSystem;

namespace CozyGame.UI
{
    /// <summary>
    /// Save/Load slot menu controller.
    /// Manages multiple save slots and handles save/load/delete operations.
    /// </summary>
    public class SaveSlotMenuController : MonoBehaviour
    {
        [Header("Mode")]
        [Tooltip("Menu mode: Save or Load")]
        public SaveSlotMenuMode menuMode = SaveSlotMenuMode.Load;

        [Header("UI References")]
        [Tooltip("Save slots container")]
        public Transform slotsContainer;

        [Tooltip("Save slot prefab")]
        public GameObject saveSlotPrefab;

        [Tooltip("Number of save slots")]
        [Range(1, 10)]
        public int numberOfSlots = 5;

        [Header("Panels")]
        [Tooltip("Save slot menu panel")]
        public GameObject saveSlotPanel;

        [Tooltip("Delete confirmation dialog")]
        public GameObject deleteConfirmDialog;

        [Tooltip("Overwrite confirmation dialog (Save mode only)")]
        public GameObject overwriteConfirmDialog;

        [Header("Confirmation Dialog References")]
        [Tooltip("Delete confirmation message text")]
        public TextMeshProUGUI deleteConfirmMessage;

        [Tooltip("Overwrite confirmation message text")]
        public TextMeshProUGUI overwriteConfirmMessage;

        [Header("Buttons")]
        [Tooltip("Back button")]
        public Button backButton;

        [Tooltip("Confirm button (for save/load)")]
        public Button confirmButton;

        [Header("Audio")]
        [Tooltip("Button click sound")]
        public string buttonClickSound = "button_click";

        [Tooltip("Save success sound")]
        public string saveSuccessSound = "save";

        [Tooltip("Load success sound")]
        public string loadSuccessSound = "load";

        [Tooltip("Delete sound")]
        public string deleteSound = "delete";

        // State
        private List<SaveSlotUI> saveSlotUIs = new List<SaveSlotUI>();
        private int selectedSlotIndex = -1;
        private int pendingDeleteSlotIndex = -1;
        private System.Action backCallback;

        private void Start()
        {
            InitializeSlots();

            // Setup button listeners
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(OnConfirmClicked);
            }

            // Hide dialogs initially
            if (deleteConfirmDialog != null)
                deleteConfirmDialog.SetActive(false);

            if (overwriteConfirmDialog != null)
                overwriteConfirmDialog.SetActive(false);

            RefreshSlots();
            UpdateConfirmButton();
        }

        /// <summary>
        /// Initialize save slot UI elements
        /// </summary>
        private void InitializeSlots()
        {
            if (slotsContainer == null || saveSlotPrefab == null)
            {
                Debug.LogError("[SaveSlotMenuController] Slots container or prefab not assigned!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            saveSlotUIs.Clear();

            // Create slot UI elements
            for (int i = 0; i < numberOfSlots; i++)
            {
                GameObject slotObj = Instantiate(saveSlotPrefab, slotsContainer);
                SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

                if (slotUI != null)
                {
                    saveSlotUIs.Add(slotUI);
                }
                else
                {
                    Debug.LogError($"[SaveSlotMenuController] Slot prefab missing SaveSlotUI component!");
                }
            }
        }

        /// <summary>
        /// Refresh all slot displays with current save data
        /// </summary>
        public void RefreshSlots()
        {
            if (SaveSystem.SaveSystem.Instance == null)
            {
                Debug.LogWarning("[SaveSlotMenuController] SaveSystem not available!");
                return;
            }

            List<SaveMetadata> allMetadata = SaveSystem.SaveSystem.Instance.GetAllSaveMetadata();

            for (int i = 0; i < saveSlotUIs.Count; i++)
            {
                SaveMetadata metadata = (i < allMetadata.Count) ? allMetadata[i] : null;
                saveSlotUIs[i].Initialize(i, metadata, OnSlotSelected, OnSlotDeleteRequested);
            }
        }

        /// <summary>
        /// Slot selected callback
        /// </summary>
        private void OnSlotSelected(int slotIndex)
        {
            PlayButtonSound();

            selectedSlotIndex = slotIndex;

            // Update visual selection
            for (int i = 0; i < saveSlotUIs.Count; i++)
            {
                saveSlotUIs[i].SetSelected(i == slotIndex);
            }

            UpdateConfirmButton();
        }

        /// <summary>
        /// Slot delete requested callback
        /// </summary>
        private void OnSlotDeleteRequested(int slotIndex)
        {
            PlayButtonSound();

            if (saveSlotUIs[slotIndex].IsEmpty())
            {
                Debug.LogWarning($"[SaveSlotMenuController] Cannot delete empty slot {slotIndex}");
                return;
            }

            pendingDeleteSlotIndex = slotIndex;

            // Show delete confirmation dialog
            if (deleteConfirmDialog != null)
            {
                if (deleteConfirmMessage != null)
                {
                    deleteConfirmMessage.text = $"Delete save slot {slotIndex + 1}?\n\nThis action cannot be undone.";
                }

                deleteConfirmDialog.SetActive(true);
            }
            else
            {
                // No dialog, delete immediately
                ConfirmDelete();
            }
        }

        /// <summary>
        /// Confirm button clicked
        /// </summary>
        private void OnConfirmClicked()
        {
            PlayButtonSound();

            if (selectedSlotIndex < 0)
            {
                Debug.LogWarning("[SaveSlotMenuController] No slot selected!");
                return;
            }

            if (menuMode == SaveSlotMenuMode.Save)
            {
                HandleSave();
            }
            else if (menuMode == SaveSlotMenuMode.Load)
            {
                HandleLoad();
            }
        }

        /// <summary>
        /// Handle save operation
        /// </summary>
        private void HandleSave()
        {
            if (SaveSystem.SaveSystem.Instance == null)
            {
                Debug.LogError("[SaveSlotMenuController] SaveSystem not available!");
                return;
            }

            // Check if slot is occupied
            if (!saveSlotUIs[selectedSlotIndex].IsEmpty())
            {
                // Show overwrite confirmation
                if (overwriteConfirmDialog != null)
                {
                    if (overwriteConfirmMessage != null)
                    {
                        overwriteConfirmMessage.text = $"Overwrite save slot {selectedSlotIndex + 1}?";
                    }

                    overwriteConfirmDialog.SetActive(true);
                    return;
                }
            }

            // Perform save
            PerformSave();
        }

        /// <summary>
        /// Perform the actual save
        /// </summary>
        private void PerformSave()
        {
            string slotName = $"Save Slot {selectedSlotIndex + 1}";
            bool success = SaveSystem.SaveSystem.Instance.SaveGame(selectedSlotIndex, slotName);

            if (success)
            {
                Debug.Log($"[SaveSlotMenuController] Saved to slot {selectedSlotIndex}");
                PlaySound(saveSuccessSound);
                RefreshSlots();

                // Return to previous menu
                OnBackClicked();
            }
            else
            {
                Debug.LogError($"[SaveSlotMenuController] Failed to save to slot {selectedSlotIndex}");
            }
        }

        /// <summary>
        /// Confirm overwrite (Save mode)
        /// </summary>
        public void ConfirmOverwrite()
        {
            PlayButtonSound();

            if (overwriteConfirmDialog != null)
                overwriteConfirmDialog.SetActive(false);

            PerformSave();
        }

        /// <summary>
        /// Cancel overwrite
        /// </summary>
        public void CancelOverwrite()
        {
            PlayButtonSound();

            if (overwriteConfirmDialog != null)
                overwriteConfirmDialog.SetActive(false);
        }

        /// <summary>
        /// Handle load operation
        /// </summary>
        private void HandleLoad()
        {
            if (SaveSystem.SaveSystem.Instance == null)
            {
                Debug.LogError("[SaveSlotMenuController] SaveSystem not available!");
                return;
            }

            // Check if slot has save data
            if (saveSlotUIs[selectedSlotIndex].IsEmpty())
            {
                Debug.LogWarning($"[SaveSlotMenuController] Slot {selectedSlotIndex} is empty!");
                return;
            }

            // Perform load
            bool success = SaveSystem.SaveSystem.Instance.LoadGame(selectedSlotIndex);

            if (success)
            {
                Debug.Log($"[SaveSlotMenuController] Loaded from slot {selectedSlotIndex}");
                PlaySound(loadSuccessSound);

                // Loading will trigger scene transition, so we don't need to do anything else
            }
            else
            {
                Debug.LogError($"[SaveSlotMenuController] Failed to load from slot {selectedSlotIndex}");
            }
        }

        /// <summary>
        /// Confirm delete
        /// </summary>
        public void ConfirmDelete()
        {
            PlayButtonSound();

            if (deleteConfirmDialog != null)
                deleteConfirmDialog.SetActive(false);

            if (pendingDeleteSlotIndex < 0)
                return;

            if (SaveSystem.SaveSystem.Instance == null)
            {
                Debug.LogError("[SaveSlotMenuController] SaveSystem not available!");
                return;
            }

            // Perform delete
            bool success = SaveSystem.SaveSystem.Instance.DeleteSave(pendingDeleteSlotIndex);

            if (success)
            {
                Debug.Log($"[SaveSlotMenuController] Deleted slot {pendingDeleteSlotIndex}");
                PlaySound(deleteSound);

                // Deselect if deleted slot was selected
                if (selectedSlotIndex == pendingDeleteSlotIndex)
                {
                    selectedSlotIndex = -1;
                }

                RefreshSlots();
                UpdateConfirmButton();
            }
            else
            {
                Debug.LogError($"[SaveSlotMenuController] Failed to delete slot {pendingDeleteSlotIndex}");
            }

            pendingDeleteSlotIndex = -1;
        }

        /// <summary>
        /// Cancel delete
        /// </summary>
        public void CancelDelete()
        {
            PlayButtonSound();

            if (deleteConfirmDialog != null)
                deleteConfirmDialog.SetActive(false);

            pendingDeleteSlotIndex = -1;
        }

        /// <summary>
        /// Update confirm button state
        /// </summary>
        private void UpdateConfirmButton()
        {
            if (confirmButton == null)
                return;

            if (menuMode == SaveSlotMenuMode.Save)
            {
                // Save mode: always enabled if slot selected
                confirmButton.interactable = (selectedSlotIndex >= 0);
            }
            else if (menuMode == SaveSlotMenuMode.Load)
            {
                // Load mode: only enabled if slot selected and not empty
                confirmButton.interactable = (selectedSlotIndex >= 0 &&
                                              !saveSlotUIs[selectedSlotIndex].IsEmpty());
            }
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClicked()
        {
            PlayButtonSound();
            backCallback?.Invoke();

            // Hide this menu
            if (saveSlotPanel != null)
                saveSlotPanel.SetActive(false);
        }

        /// <summary>
        /// Set back callback
        /// </summary>
        public void SetBackCallback(System.Action callback)
        {
            backCallback = callback;
        }

        /// <summary>
        /// Show save slot menu
        /// </summary>
        public void Show(SaveSlotMenuMode mode)
        {
            menuMode = mode;
            selectedSlotIndex = -1;

            if (saveSlotPanel != null)
                saveSlotPanel.SetActive(true);

            RefreshSlots();
            UpdateConfirmButton();

            // Update confirm button text based on mode
            if (confirmButton != null)
            {
                TextMeshProUGUI buttonText = confirmButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = (mode == SaveSlotMenuMode.Save) ? "Save" : "Load";
                }
            }
        }

        /// <summary>
        /// Hide save slot menu
        /// </summary>
        public void Hide()
        {
            if (saveSlotPanel != null)
                saveSlotPanel.SetActive(false);
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

    /// <summary>
    /// Save slot menu mode
    /// </summary>
    public enum SaveSlotMenuMode
    {
        Save,
        Load
    }
}
