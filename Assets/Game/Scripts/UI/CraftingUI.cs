using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.UI
{
    /// <summary>
    /// Crafting UI controller.
    /// Shows available recipes and handles crafting.
    /// </summary>
    public class CraftingUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Crafting panel")]
        public GameObject craftingPanel;

        [Tooltip("Recipe list container")]
        public Transform recipeListContainer;

        [Tooltip("Recipe button prefab")]
        public GameObject recipeButtonPrefab;

        [Tooltip("Recipe details text")]
        public TextMeshProUGUI recipeDetailsText;

        [Tooltip("Craft button")]
        public Button craftButton;

        [Tooltip("Close button")]
        public Button closeButton;

        [Header("Recipes")]
        [Tooltip("Available recipes")]
        public CraftingRecipe[] availableRecipes;

        [Tooltip("Discovered recipes (auto-load from Resources)")]
        public List<CraftingRecipe> discoveredRecipes = new List<CraftingRecipe>();

        private CraftingRecipe selectedRecipe;

        private void Start()
        {
            // Setup buttons
            if (craftButton != null)
            {
                craftButton.onClick.AddListener(OnCraftClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            // Load recipes
            LoadRecipes();

            // Hide panel
            Hide();
        }

        /// <summary>
        /// Load available recipes
        /// </summary>
        private void LoadRecipes()
        {
            discoveredRecipes.Clear();

            // Add assigned recipes
            if (availableRecipes != null)
            {
                discoveredRecipes.AddRange(availableRecipes);
            }

            // Also load from Resources
            CraftingRecipe[] resourceRecipes = Resources.LoadAll<CraftingRecipe>("Recipes");
            foreach (var recipe in resourceRecipes)
            {
                if (recipe != null && !discoveredRecipes.Contains(recipe))
                {
                    // Only add if discovered by default or already discovered
                    if (recipe.isDiscoveredByDefault)
                    {
                        discoveredRecipes.Add(recipe);
                    }
                }
            }

            RefreshRecipeList();
        }

        /// <summary>
        /// Refresh recipe list display
        /// </summary>
        private void RefreshRecipeList()
        {
            if (recipeListContainer == null || recipeButtonPrefab == null)
                return;

            // Clear existing buttons
            foreach (Transform child in recipeListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create button for each recipe
            foreach (var recipe in discoveredRecipes)
            {
                GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = recipe.recipeName;

                    // Color code based on can craft
                    string reason;
                    bool canCraft = recipe.CanCraft(out reason);
                    buttonText.color = canCraft ? Color.white : Color.gray;
                }

                if (button != null)
                {
                    CraftingRecipe r = recipe; // Capture for lambda
                    button.onClick.AddListener(() => OnRecipeSelected(r));
                }
            }
        }

        /// <summary>
        /// Recipe selected callback
        /// </summary>
        private void OnRecipeSelected(CraftingRecipe recipe)
        {
            selectedRecipe = recipe;

            // Update details
            if (recipeDetailsText != null)
            {
                recipeDetailsText.text = recipe.GetRecipeText();
            }

            // Update craft button
            if (craftButton != null)
            {
                string reason;
                bool canCraft = recipe.CanCraft(out reason);
                craftButton.interactable = canCraft;
            }
        }

        /// <summary>
        /// Craft button clicked
        /// </summary>
        private void OnCraftClicked()
        {
            if (selectedRecipe == null)
                return;

            bool success = selectedRecipe.Craft();

            if (success)
            {
                // Refresh UI
                RefreshRecipeList();

                // Re-select to update details
                OnRecipeSelected(selectedRecipe);
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
        /// Show crafting panel
        /// </summary>
        public void Show()
        {
            if (craftingPanel != null)
            {
                craftingPanel.SetActive(true);
                RefreshRecipeList();
            }
        }

        /// <summary>
        /// Hide crafting panel
        /// </summary>
        public void Hide()
        {
            if (craftingPanel != null)
            {
                craftingPanel.SetActive(false);
            }
        }
    }
}
