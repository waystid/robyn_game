using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Pets;

namespace CozyGame.UI
{
    /// <summary>
    /// Pet UI controller.
    /// Manages pet display and interaction.
    /// </summary>
    public class PetUI : MonoBehaviour
    {
        public static PetUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Pet panel")]
        public GameObject petPanel;

        [Header("Pet List")]
        [Tooltip("Pet container")]
        public Transform petContainer;

        [Tooltip("Pet slot prefab")]
        public GameObject petSlotPrefab;

        [Header("Pet Details")]
        [Tooltip("Pet name text")]
        public TextMeshProUGUI petNameText;

        [Tooltip("Pet level text")]
        public TextMeshProUGUI petLevelText;

        [Tooltip("Pet icon")]
        public Image petIcon;

        [Tooltip("Hunger bar")]
        public Slider hungerBar;

        [Tooltip("Happiness bar")]
        public Slider happinessBar;

        [Tooltip("Loyalty bar")]
        public Slider loyaltyBar;

        [Tooltip("Experience bar")]
        public Slider expBar;

        [Header("Buttons")]
        [Tooltip("Summon button")]
        public Button summonButton;

        [Tooltip("Dismiss button")]
        public Button dismissButton;

        [Tooltip("Feed button")]
        public Button feedButton;

        [Tooltip("Pet button")]
        public Button petButton;

        [Tooltip("Rename button")]
        public Button renameButton;

        [Tooltip("Close button")]
        public Button closeButton;

        [Header("Rename Dialog")]
        [Tooltip("Rename dialog panel")]
        public GameObject renameDialog;

        [Tooltip("Rename input field")]
        public TMP_InputField renameInputField;

        [Tooltip("Confirm rename button")]
        public Button confirmRenameButton;

        [Tooltip("Cancel rename button")]
        public Button cancelRenameButton;

        // State
        private List<GameObject> petSlots = new List<GameObject>();
        private PetController selectedPet;

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
            // Setup button listeners
            if (summonButton != null)
            {
                summonButton.onClick.AddListener(OnSummonClicked);
            }

            if (dismissButton != null)
            {
                dismissButton.onClick.AddListener(OnDismissClicked);
            }

            if (feedButton != null)
            {
                feedButton.onClick.AddListener(OnFeedClicked);
            }

            if (petButton != null)
            {
                petButton.onClick.AddListener(OnPetClicked);
            }

            if (renameButton != null)
            {
                renameButton.onClick.AddListener(OnRenameClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (confirmRenameButton != null)
            {
                confirmRenameButton.onClick.AddListener(OnConfirmRename);
            }

            if (cancelRenameButton != null)
            {
                cancelRenameButton.onClick.AddListener(OnCancelRename);
            }

            // Hide rename dialog
            if (renameDialog != null)
            {
                renameDialog.SetActive(false);
            }

            // Hide panel initially
            Hide();
        }

        private void Update()
        {
            // Update selected pet details
            if (selectedPet != null && petPanel.activeSelf)
            {
                UpdatePetDetails();
            }
        }

        /// <summary>
        /// Refresh pet list
        /// </summary>
        public void RefreshPetList()
        {
            if (PetManager.Instance == null || petContainer == null || petSlotPrefab == null)
                return;

            // Clear existing slots
            foreach (var slot in petSlots)
            {
                Destroy(slot);
            }
            petSlots.Clear();

            // Get owned pets
            List<PetController> pets = PetManager.Instance.GetOwnedPets();

            // Create slot for each pet
            foreach (var pet in pets)
            {
                CreatePetSlot(pet);
            }
        }

        /// <summary>
        /// Create pet slot
        /// </summary>
        private void CreatePetSlot(PetController pet)
        {
            GameObject slotObj = Instantiate(petSlotPrefab, petContainer);

            // Setup slot UI (simplified, assumes prefab has these components)
            Image icon = slotObj.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = slotObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI levelText = slotObj.transform.Find("Level")?.GetComponent<TextMeshProUGUI>();
            Button button = slotObj.GetComponent<Button>();

            if (icon != null && pet.petData != null)
            {
                icon.sprite = pet.petData.icon;
            }

            if (nameText != null)
            {
                nameText.text = pet.GetDisplayName();
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{pet.level}";
            }

            if (button != null)
            {
                button.onClick.AddListener(() => OnPetSelected(pet));
            }

            petSlots.Add(slotObj);
        }

        /// <summary>
        /// Pet selected callback
        /// </summary>
        private void OnPetSelected(PetController pet)
        {
            selectedPet = pet;
            UpdatePetDetails();
            UpdateButtons();
        }

        /// <summary>
        /// Update pet details display
        /// </summary>
        private void UpdatePetDetails()
        {
            if (selectedPet == null)
                return;

            if (petNameText != null)
            {
                petNameText.text = selectedPet.GetDisplayName();
            }

            if (petLevelText != null)
            {
                petLevelText.text = $"Level {selectedPet.level}";
            }

            if (petIcon != null && selectedPet.petData != null)
            {
                petIcon.sprite = selectedPet.petData.icon;
            }

            if (hungerBar != null)
            {
                hungerBar.value = selectedPet.currentHunger / selectedPet.petData.maxHunger;
            }

            if (happinessBar != null)
            {
                happinessBar.value = selectedPet.currentHappiness / selectedPet.petData.maxHappiness;
            }

            if (loyaltyBar != null)
            {
                loyaltyBar.value = selectedPet.loyalty / 100f;
            }

            if (expBar != null && selectedPet.petData != null)
            {
                int expRequired = selectedPet.petData.GetExpRequiredForLevel(selectedPet.level + 1);
                expBar.value = (float)selectedPet.currentExp / expRequired;
            }
        }

        /// <summary>
        /// Update button states
        /// </summary>
        private void UpdateButtons()
        {
            if (selectedPet == null)
            {
                if (summonButton != null) summonButton.interactable = false;
                if (dismissButton != null) dismissButton.interactable = false;
                if (feedButton != null) feedButton.interactable = false;
                if (petButton != null) petButton.interactable = false;
                if (renameButton != null) renameButton.interactable = false;
                return;
            }

            if (summonButton != null)
            {
                summonButton.interactable = !selectedPet.isActive;
            }

            if (dismissButton != null)
            {
                dismissButton.interactable = selectedPet.isActive;
            }

            if (feedButton != null)
            {
                feedButton.interactable = true;
            }

            if (petButton != null)
            {
                petButton.interactable = true;
            }

            if (renameButton != null && selectedPet.petData != null)
            {
                renameButton.interactable = selectedPet.petData.canRename;
            }
        }

        /// <summary>
        /// Summon button clicked
        /// </summary>
        private void OnSummonClicked()
        {
            if (selectedPet == null || PetManager.Instance == null)
                return;

            PetManager.Instance.SummonPet(selectedPet.instanceID);
            UpdateButtons();
        }

        /// <summary>
        /// Dismiss button clicked
        /// </summary>
        private void OnDismissClicked()
        {
            if (selectedPet == null || PetManager.Instance == null)
                return;

            PetManager.Instance.DismissPet(selectedPet.instanceID);
            UpdateButtons();
        }

        /// <summary>
        /// Feed button clicked
        /// </summary>
        private void OnFeedClicked()
        {
            if (selectedPet == null)
                return;

            // Feed pet (assumes player has food)
            selectedPet.Feed(30f);
        }

        /// <summary>
        /// Pet button clicked
        /// </summary>
        private void OnPetClicked()
        {
            if (selectedPet == null)
                return;

            selectedPet.PetPet();
        }

        /// <summary>
        /// Rename button clicked
        /// </summary>
        private void OnRenameClicked()
        {
            if (selectedPet == null || renameDialog == null)
                return;

            renameDialog.SetActive(true);

            if (renameInputField != null)
            {
                renameInputField.text = selectedPet.customName;
            }
        }

        /// <summary>
        /// Confirm rename
        /// </summary>
        private void OnConfirmRename()
        {
            if (selectedPet == null || renameInputField == null)
                return;

            string newName = renameInputField.text.Trim();

            if (!string.IsNullOrEmpty(newName))
            {
                selectedPet.Rename(newName);
                UpdatePetDetails();
                RefreshPetList();
            }

            if (renameDialog != null)
            {
                renameDialog.SetActive(false);
            }
        }

        /// <summary>
        /// Cancel rename
        /// </summary>
        private void OnCancelRename()
        {
            if (renameDialog != null)
            {
                renameDialog.SetActive(false);
            }
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseClicked()
        {
            Hide();
        }

        /// <summary>
        /// Show pet panel
        /// </summary>
        public void Show()
        {
            if (petPanel != null)
            {
                petPanel.SetActive(true);
                RefreshPetList();
            }
        }

        /// <summary>
        /// Hide pet panel
        /// </summary>
        public void Hide()
        {
            if (petPanel != null)
            {
                petPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle pet panel
        /// </summary>
        public void Toggle()
        {
            if (petPanel.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}
