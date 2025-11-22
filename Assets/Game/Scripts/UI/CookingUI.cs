using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Cooking;

namespace CozyGame.UI
{
    /// <summary>
    /// Cooking UI controller.
    /// Shows recipes and cooking progress.
    /// </summary>
    public class CookingUI : MonoBehaviour
    {
        public static CookingUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main cooking panel")]
        public GameObject cookingPanel;

        [Tooltip("Recipe book panel")]
        public GameObject recipeBookPanel;

        [Header("Recipe List")]
        [Tooltip("Recipe list container")]
        public Transform recipeListContainer;

        [Tooltip("Recipe button prefab")]
        public GameObject recipeButtonPrefab;

        [Tooltip("Filter: Show all or only cookable")]
        public Toggle showAllToggle;

        [Header("Recipe Details")]
        [Tooltip("Recipe name text")]
        public TextMeshProUGUI recipeNameText;

        [Tooltip("Recipe description text")]
        public TextMeshProUGUI recipeDescriptionText;

        [Tooltip("Recipe icon")]
        public Image recipeIconImage;

        [Tooltip("Ingredients container")]
        public Transform ingredientsContainer;

        [Tooltip("Ingredient entry prefab")]
        public GameObject ingredientEntryPrefab;

        [Tooltip("Result text")]
        public TextMeshProUGUI resultText;

        [Tooltip("Time text")]
        public TextMeshProUGUI timeText;

        [Tooltip("Difficulty text")]
        public TextMeshProUGUI difficultyText;

        [Header("Cooking Progress")]
        [Tooltip("Progress bar")]
        public Image progressBar;

        [Tooltip("Progress text")]
        public TextMeshProUGUI progressText;

        [Tooltip("Quality indicator")]
        public TextMeshProUGUI qualityText;

        [Header("Bulk Cooking")]
        [Tooltip("Quantity slider")]
        public Slider quantitySlider;

        [Tooltip("Quantity text")]
        public TextMeshProUGUI quantityText;

        [Header("Buttons")]
        [Tooltip("Cook button")]
        public Button cookButton;

        [Tooltip("Cancel button")]
        public Button cancelButton;

        [Tooltip("Recipe book button")]
        public Button recipeBookButton;

        [Tooltip("Close button")]
        public Button closeButton;

        // State
        private CookingStation currentStation;
        private CookingRecipe selectedRecipe;
        private List<CookingRecipe> discoveredRecipes = new List<CookingRecipe>();

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
            if (cookButton != null)
            {
                cookButton.onClick.AddListener(OnCookClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelClicked);
            }

            if (recipeBookButton != null)
            {
                recipeBookButton.onClick.AddListener(OnRecipeBookClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (showAllToggle != null)
            {
                showAllToggle.onValueChanged.AddListener(OnShowAllChanged);
            }

            if (quantitySlider != null)
            {
                quantitySlider.onValueChanged.AddListener(OnQuantityChanged);
            }

            // Hide panels initially
            if (cookingPanel != null)
            {
                cookingPanel.SetActive(false);
            }

            if (recipeBookPanel != null)
            {
                recipeBookPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (currentStation != null && cookingPanel != null && cookingPanel.activeSelf)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// Open cooking UI
        /// </summary>
        public void OpenCookingUI(CookingStation station)
        {
            currentStation = station;

            if (cookingPanel != null)
            {
                cookingPanel.SetActive(true);
            }

            RefreshRecipeList();
            UpdateUI();
        }

        /// <summary>
        /// Close cooking UI
        /// </summary>
        public void CloseCookingUI()
        {
            currentStation = null;
            selectedRecipe = null;

            if (cookingPanel != null)
            {
                cookingPanel.SetActive(false);
            }

            if (recipeBookPanel != null)
            {
                recipeBookPanel.SetActive(false);
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

            // Get recipes to show
            List<CookingRecipe> recipesToShow;
            if (showAllToggle != null && showAllToggle.isOn)
            {
                recipesToShow = currentStation.GetAvailableRecipes();
            }
            else
            {
                recipesToShow = currentStation.GetCookableRecipes();
            }

            // Create buttons
            foreach (var recipe in recipesToShow)
            {
                GameObject buttonObj = Instantiate(recipeButtonPrefab, recipeListContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                Image buttonIcon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();

                if (buttonText != null)
                {
                    buttonText.text = recipe.recipeName;
                }

                if (buttonIcon != null && recipe.icon != null)
                {
                    buttonIcon.sprite = recipe.icon;
                }

                if (button != null)
                {
                    CookingRecipe r = recipe; // Capture for lambda
                    button.onClick.AddListener(() => OnRecipeSelected(r));

                    // Check if can cook
                    string reason;
                    button.interactable = recipe.CanCook(out reason);

                    if (!button.interactable && buttonText != null)
                    {
                        buttonText.color = Color.gray;
                    }
                }
            }
        }

        /// <summary>
        /// Recipe selected callback
        /// </summary>
        private void OnRecipeSelected(CookingRecipe recipe)
        {
            selectedRecipe = recipe;
            UpdateRecipeDetails();
        }

        /// <summary>
        /// Update recipe details panel
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

            // Update description
            if (recipeDescriptionText != null)
            {
                recipeDescriptionText.text = selectedRecipe.description;
            }

            // Update icon
            if (recipeIconImage != null && selectedRecipe.icon != null)
            {
                recipeIconImage.sprite = selectedRecipe.icon;
            }

            // Update ingredients
            UpdateIngredientsDisplay();

            // Update result
            if (resultText != null && selectedRecipe.resultFood != null)
            {
                resultText.text = $"Result: {selectedRecipe.resultQuantity}x {selectedRecipe.resultFood.itemName}";
            }

            // Update time
            if (timeText != null)
            {
                timeText.text = $"Cook Time: {selectedRecipe.cookingTime:F1}s";
            }

            // Update difficulty
            if (difficultyText != null)
            {
                difficultyText.text = $"Difficulty: {selectedRecipe.GetDifficultyString()}";
                difficultyText.color = selectedRecipe.GetDifficultyColor();
            }

            // Update quantity slider
            if (quantitySlider != null)
            {
                quantitySlider.minValue = 1;
                quantitySlider.maxValue = selectedRecipe.allowBulkCooking ? selectedRecipe.maxBulkQuantity : 1;
                quantitySlider.value = 1;
            }

            // Update cook button
            if (cookButton != null)
            {
                string reason;
                cookButton.interactable = selectedRecipe.CanCook(out reason) && !currentStation.isCooking;
            }
        }

        /// <summary>
        /// Update ingredients display
        /// </summary>
        private void UpdateIngredientsDisplay()
        {
            if (ingredientsContainer == null || ingredientEntryPrefab == null || selectedRecipe == null)
                return;

            // Clear existing
            foreach (Transform child in ingredientsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create entries
            if (selectedRecipe.ingredients == null)
                return;

            foreach (var ing in selectedRecipe.ingredients)
            {
                if (ing.ingredient == null)
                    continue;

                GameObject entryObj = Instantiate(ingredientEntryPrefab, ingredientsContainer);
                TextMeshProUGUI entryText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
                Image entryIcon = entryObj.transform.Find("Icon")?.GetComponent<Image>();

                if (entryText != null)
                {
                    int playerAmount = Inventory.InventorySystem.Instance.GetItemCount(ing.ingredient.itemID);
                    string optional = ing.isOptional ? " (optional)" : "";
                    string text = $"{ing.ingredient.itemName}: {playerAmount}/{ing.quantity}{optional}";
                    entryText.text = text;

                    if (!ing.isOptional && playerAmount < ing.quantity)
                    {
                        entryText.color = Color.red;
                    }
                    else
                    {
                        entryText.color = Color.white;
                    }
                }

                if (entryIcon != null && ing.ingredient.icon != null)
                {
                    entryIcon.sprite = ing.ingredient.icon;
                }
            }
        }

        /// <summary>
        /// Update UI (progress, etc.)
        /// </summary>
        private void UpdateUI()
        {
            if (currentStation == null)
                return;

            // Update progress
            if (progressBar != null)
            {
                progressBar.fillAmount = currentStation.cookingProgress;
            }

            if (progressText != null)
            {
                if (currentStation.isCooking && currentStation.currentRecipe != null)
                {
                    string text = $"Cooking {currentStation.currentRecipe.recipeName}... {currentStation.cookingProgress * 100f:F0}%";
                    if (currentStation.bulkRemaining > 1)
                    {
                        text += $" ({currentStation.bulkRemaining} remaining)";
                    }
                    progressText.text = text;
                }
                else
                {
                    progressText.text = "Idle";
                }
            }

            // Update quality indicator
            if (qualityText != null && currentStation.isCooking)
            {
                qualityText.text = $"Target: {currentStation.targetQuality}";
                qualityText.color = GetQualityColor(currentStation.targetQuality);
            }

            // Update buttons
            if (cancelButton != null)
            {
                cancelButton.interactable = currentStation.isCooking;
            }

            if (selectedRecipe != null && cookButton != null)
            {
                string reason;
                cookButton.interactable = selectedRecipe.CanCook(out reason) && !currentStation.isCooking;
            }
        }

        /// <summary>
        /// Get quality color
        /// </summary>
        private Color GetQualityColor(FoodQuality quality)
        {
            switch (quality)
            {
                case FoodQuality.Poor: return new Color(0.5f, 0.3f, 0.2f);
                case FoodQuality.Normal: return Color.white;
                case FoodQuality.Good: return Color.green;
                case FoodQuality.Excellent: return Color.cyan;
                case FoodQuality.Masterpiece: return new Color(1f, 0.5f, 0f);
                default: return Color.white;
            }
        }

        /// <summary>
        /// Cook button clicked
        /// </summary>
        private void OnCookClicked()
        {
            if (selectedRecipe == null || currentStation == null)
                return;

            int quantity = quantitySlider != null ? Mathf.RoundToInt(quantitySlider.value) : 1;
            currentStation.StartCooking(selectedRecipe, quantity);
            RefreshRecipeList();
        }

        /// <summary>
        /// Cancel button clicked
        /// </summary>
        private void OnCancelClicked()
        {
            if (currentStation == null)
                return;

            currentStation.CancelCooking();
        }

        /// <summary>
        /// Recipe book button clicked
        /// </summary>
        private void OnRecipeBookClicked()
        {
            if (recipeBookPanel != null)
            {
                bool isActive = recipeBookPanel.activeSelf;
                recipeBookPanel.SetActive(!isActive);
            }
        }

        /// <summary>
        /// Close button clicked
        /// </summary>
        private void OnCloseClicked()
        {
            CloseCookingUI();
        }

        /// <summary>
        /// Show all toggle changed
        /// </summary>
        private void OnShowAllChanged(bool value)
        {
            RefreshRecipeList();
        }

        /// <summary>
        /// Quantity slider changed
        /// </summary>
        private void OnQuantityChanged(float value)
        {
            if (quantityText != null)
            {
                quantityText.text = $"x{Mathf.RoundToInt(value)}";
            }
        }

        /// <summary>
        /// Discover recipe
        /// </summary>
        public void DiscoverRecipe(CookingRecipe recipe)
        {
            if (recipe == null || discoveredRecipes.Contains(recipe))
                return;

            discoveredRecipes.Add(recipe);

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show($"Recipe Discovered: {recipe.recipeName}!", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.yellow);
            }

            Debug.Log($"[CookingUI] Discovered recipe: {recipe.recipeName}");
        }

        /// <summary>
        /// Is recipe discovered?
        /// </summary>
        public bool IsRecipeDiscovered(CookingRecipe recipe)
        {
            return discoveredRecipes.Contains(recipe);
        }
    }
}
