using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Mining;

namespace CozyGame.UI
{
    /// <summary>
    /// Smelting UI controller.
    /// Shows smelting recipes and progress.
    /// </summary>
    public class SmeltingUI : MonoBehaviour
    {
        public static SmeltingUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main smelting panel")]
        public GameObject smeltingPanel;

        [Header("Recipe List")]
        [Tooltip("Recipe list container")]
        public Transform recipeListContainer;

        [Tooltip("Recipe button prefab")]
        public GameObject recipeButtonPrefab;

        [Header("Details")]
        [Tooltip("Recipe name text")]
        public TextMeshProUGUI recipeNameText;

        [Tooltip("Input item text")]
        public TextMeshProUGUI inputText;

        [Tooltip("Output item text")]
        public TextMeshProUGUI outputText;

        [Tooltip("Time text")]
        public TextMeshProUGUI timeText;

        [Tooltip("Fuel cost text")]
        public TextMeshProUGUI fuelCostText;

        [Header("Progress")]
        [Tooltip("Progress bar")]
        public Image progressBar;

        [Tooltip("Progress text")]
        public TextMeshProUGUI progressText;

        [Tooltip("Fuel bar")]
        public Image fuelBar;

        [Tooltip("Fuel text")]
        public TextMeshProUGUI fuelText;

        [Header("Buttons")]
        [Tooltip("Smelt button")]
        public Button smeltButton;

        [Tooltip("Add fuel button")]
        public Button addFuelButton;

        [Tooltip("Cancel button")]
        public Button cancelButton;

        [Tooltip("Close button")]
        public Button closeButton;

        // State
        private SmeltingStation currentStation;
        private SmeltingRecipe selectedRecipe;

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
            if (smeltButton != null)
            {
                smeltButton.onClick.AddListener(OnSmeltClicked);
            }

            if (addFuelButton != null)
            {
                addFuelButton.onClick.AddListener(OnAddFuelClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            // Hide panel initially
            if (smeltingPanel != null)
            {
                smeltingPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (currentStation != null && smeltingPanel != null && smeltingPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Open smelting UI
        /// </summary>
        public void OpenSmeltingUI(SmeltingStation station)
        {
            currentStation = station;

            if (smeltingPanel != null)
            {
                smeltingPanel.SetActive(true);
            }

            RefreshRecipeList();
            UpdateUI();
        }

        /// <summary>
        /// Close smelting UI
        /// </summary>
        public void CloseSmeltingUI()
        {
            currentStation = null;
            selectedRecipe = null;

            if (smeltingPanel != null)
            {
                smeltingPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Refresh recipe list
        /// </summary>
        private void RefreshRecipeList()
        {
            if (recipeListContainer == null || recipeButtonPrefab == null || currentStation == null)
                return;

            // Clear existing
            foreach (Transform child in recipeListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create buttons for each recipe
            foreach (var recipe in currentStation.recipes)
            {
                GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = recipe.recipeName;
                }

                if (button != null)
                {
                    SmeltingRecipe r = recipe; // Capture for lambda
                    button.onClick.AddListener(() => OnRecipeSelected(r));

                    // Check if can craft
                    string reason;
                    button.interactable = recipe.CanCraft(out reason);
                }
            }
        }

        /// <summary>
        /// Recipe selected callback
        /// </summary>
        private void OnRecipeSelected(SmeltingRecipe recipe)
        {
            selectedRecipe = recipe;
            UpdateRecipeDetails();
        }

        /// <summary>
        /// Update recipe details
        /// </summary>
        private void UpdateRecipeDetails()
        {
            if (selectedRecipe == null)
                return;

            // Update name
            if (recipeNameText != null)
            {
                recipeNameText.text = selectedRecipe.recipeName;
            }

            // Update input
            if (inputText != null)
            {
                int playerAmount = Inventory.InventorySystem.Instance.GetItemCount(selectedRecipe.inputItem.itemID);
                inputText.text = $"Input: {selectedRecipe.inputQuantity}x {selectedRecipe.inputItem.itemName} ({playerAmount})";
                inputText.color = playerAmount >= selectedRecipe.inputQuantity ? Color.white : Color.red;
            }

            // Update output
            if (outputText != null)
            {
                outputText.text = $"Output: {selectedRecipe.outputQuantity}x {selectedRecipe.outputItem.itemName}";
            }

            // Update time
            if (timeText != null)
            {
                timeText.text = $"Time: {selectedRecipe.smeltingTime:F1}s";
            }

            // Update fuel cost
            if (fuelCostText != null)
            {
                fuelCostText.text = $"Fuel: {selectedRecipe.fuelCost}";
                fuelCostText.color = currentStation.currentFuel >= selectedRecipe.fuelCost ? Color.white : Color.red;
            }

            // Update smelt button
            if (smeltButton != null)
            {
                string reason;
                bool canCraft = selectedRecipe.CanCraft(out reason) && currentStation.currentFuel >= selectedRecipe.fuelCost;
                smeltButton.interactable = canCraft && !currentStation.isSmelting;
            }
        }

        /// <summary>
        /// Update UI (progress, fuel, etc.)
        /// </summary>
        private void UpdateUI()
        {
            if (currentStation == null)
                return;

            // Update progress
            if (progressBar != null)
            {
                progressBar.fillAmount = currentStation.smeltingProgress;
            }

            if (progressText != null)
            {
                if (currentStation.isSmelting && currentStation.currentRecipe != null)
                {
                    progressText.text = $"Smelting {currentStation.currentRecipe.recipeName}... {currentStation.smeltingProgress * 100f:F0}%";
                }
                else
                {
                    progressText.text = "Idle";
                }
            }

            // Update fuel
            if (fuelBar != null)
            {
                fuelBar.fillAmount = currentStation.GetFuelPercent();
            }

            if (fuelText != null)
            {
                fuelText.text = $"Fuel: {currentStation.currentFuel}/{currentStation.maxFuelCapacity}";
            }

            // Update buttons
            if (cancelButton != null)
            {
                cancelButton.interactable = currentStation.isSmelting;
            }

            if (selectedRecipe != null)
            {
                UpdateRecipeDetails();
            }
        }

        /// <summary>
        /// Smelt button clicked
        /// </summary>
        private void OnSmeltClicked()
        {
            if (selectedRecipe == null || currentStation == null)
                return;

            currentStation.StartSmelting(selectedRecipe);
            RefreshRecipeList();
        }

        /// <summary>
        /// Add fuel button clicked
        /// </summary>
        private void OnAddFuelClicked()
        {
            if (currentStation == null)
                return;

            // Add 10 fuel
            currentStation.AddFuel(10);
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void OnCancelClicked()
        {
            if (currentStation == null)
                return;

            currentStation.CancelSmelting();
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseClicked()
        {
            CloseSmeltingUI();
        }
    }
}
