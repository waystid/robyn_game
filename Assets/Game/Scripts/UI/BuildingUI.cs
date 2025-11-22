using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Building;
using CozyGame.Economy;

namespace CozyGame.UI
{
    /// <summary>
    /// Building UI controller.
    /// Shows available buildings and handles build mode UI.
    /// </summary>
    public class BuildingUI : MonoBehaviour
    {
        public static BuildingUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main building menu panel")]
        public GameObject buildingMenuPanel;

        [Tooltip("Build mode HUD panel")]
        public GameObject buildModeHUDPanel;

        [Tooltip("Building details panel")]
        public GameObject buildingDetailsPanel;

        [Tooltip("Upgrade panel")]
        public GameObject upgradePanel;

        [Header("Building Menu")]
        [Tooltip("Building list container")]
        public Transform buildingListContainer;

        [Tooltip("Building button prefab")]
        public GameObject buildingButtonPrefab;

        [Tooltip("Category filter dropdown")]
        public TMP_Dropdown categoryFilter;

        [Tooltip("Search input field")]
        public TMP_InputField searchInput;

        [Header("Building Details")]
        [Tooltip("Building name text")]
        public TextMeshProUGUI buildingNameText;

        [Tooltip("Building description text")]
        public TextMeshProUGUI buildingDescriptionText;

        [Tooltip("Building icon")]
        public Image buildingIconImage;

        [Tooltip("Cost container")]
        public Transform costContainer;

        [Tooltip("Cost entry prefab")]
        public GameObject costEntryPrefab;

        [Tooltip("Build button")]
        public Button buildButton;

        [Header("Build Mode HUD")]
        [Tooltip("Current building name")]
        public TextMeshProUGUI currentBuildingNameText;

        [Tooltip("Placement status text")]
        public TextMeshProUGUI placementStatusText;

        [Tooltip("Rotation angle text")]
        public TextMeshProUGUI rotationText;

        [Tooltip("Controls help text")]
        public TextMeshProUGUI controlsHelpText;

        [Header("Upgrade")]
        [Tooltip("Upgrade button")]
        public Button upgradeButton;

        [Tooltip("Upgrade info text")]
        public TextMeshProUGUI upgradeInfoText;

        [Tooltip("Upgrade cost container")]
        public Transform upgradeCostContainer;

        [Header("Available Buildings")]
        [Tooltip("Available building data")]
        public BuildingData[] availableBuildings;

        [Header("Buttons")]
        [Tooltip("Close menu button")]
        public Button closeMenuButton;

        [Tooltip("Demolish mode button")]
        public Button demolishModeButton;

        [Tooltip("Exit build mode button")]
        public Button exitBuildModeButton;

        // State
        private BuildingData selectedBuildingData;
        private PlacedBuilding selectedPlacedBuilding;
        private BuildingCategory currentFilter = (BuildingCategory)(-1); // All
        private List<BuildingData> filteredBuildings = new List<BuildingData>();

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
            if (buildButton != null)
            {
                buildButton.onClick.AddListener(OnBuildButtonClicked);
            }

            if (closeMenuButton != null)
            {
                closeMenuButton.onClick.AddListener(OnCloseMenuClicked);
            }

            if (demolishModeButton != null)
            {
                demolishModeButton.onClick.AddListener(OnDemolishModeClicked);
            }

            if (exitBuildModeButton != null)
            {
                exitBuildModeButton.onClick.AddListener(OnExitBuildModeClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }

            // Setup category filter
            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                List<string> options = new List<string> { "All" };
                foreach (BuildingCategory cat in System.Enum.GetValues(typeof(BuildingCategory)))
                {
                    options.Add(cat.ToString());
                }
                categoryFilter.AddOptions(options);
                categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
            }

            // Setup search
            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }

            // Subscribe to BuildingSystem events
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.OnBuildModeChanged.AddListener(OnBuildModeChanged);
            }

            // Load buildings
            LoadBuildings();

            // Hide all panels initially
            HideAll();
        }

        private void Update()
        {
            // Update build mode HUD
            if (BuildingSystem.Instance != null && BuildingSystem.Instance.currentMode == BuildMode.Placing)
            {
                UpdateBuildModeHUD();
            }
        }

        /// <summary>
        /// Load available buildings
        /// </summary>
        private void LoadBuildings()
        {
            filteredBuildings.Clear();

            if (availableBuildings == null)
            {
                // Load from Resources
                availableBuildings = Resources.LoadAll<BuildingData>("BuildingData");
            }

            filteredBuildings.AddRange(availableBuildings);
            RefreshBuildingList();
        }

        /// <summary>
        /// Refresh building list display
        /// </summary>
        private void RefreshBuildingList()
        {
            if (buildingListContainer == null || buildingButtonPrefab == null)
                return;

            // Clear existing buttons
            foreach (Transform child in buildingListContainer)
            {
                Destroy(child.gameObject);
            }

            // Apply filters
            List<BuildingData> displayBuildings = ApplyFilters();

            // Create button for each building
            foreach (var building in displayBuildings)
            {
                GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingListContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                Image buttonIcon = buttonObj.transform.Find("Icon")?.GetComponent<Image>();

                if (buttonText != null)
                {
                    buttonText.text = building.buildingName;
                }

                if (buttonIcon != null && building.icon != null)
                {
                    buttonIcon.sprite = building.icon;
                }

                if (button != null)
                {
                    BuildingData b = building; // Capture for lambda
                    button.onClick.AddListener(() => OnBuildingSelected(b));

                    // Check if can afford
                    string missing;
                    bool canAfford = building.HasResources(out missing);
                    button.interactable = canAfford && building.MeetsRequirements(out string _);

                    if (!button.interactable && buttonText != null)
                    {
                        buttonText.color = Color.gray;
                    }
                }
            }
        }

        /// <summary>
        /// Apply category and search filters
        /// </summary>
        private List<BuildingData> ApplyFilters()
        {
            List<BuildingData> filtered = new List<BuildingData>(filteredBuildings);

            // Category filter
            if (currentFilter != (BuildingCategory)(-1))
            {
                filtered.RemoveAll(b => b.category != currentFilter);
            }

            // Search filter
            if (searchInput != null && !string.IsNullOrEmpty(searchInput.text))
            {
                string search = searchInput.text.ToLower();
                filtered.RemoveAll(b => !b.buildingName.ToLower().Contains(search));
            }

            return filtered;
        }

        /// <summary>
        /// Building selected callback
        /// </summary>
        private void OnBuildingSelected(BuildingData building)
        {
            selectedBuildingData = building;
            ShowBuildingDetails(building);
        }

        /// <summary>
        /// Show building details panel
        /// </summary>
        private void ShowBuildingDetails(BuildingData building)
        {
            if (buildingDetailsPanel != null)
            {
                buildingDetailsPanel.SetActive(true);
            }

            // Update name
            if (buildingNameText != null)
            {
                buildingNameText.text = building.buildingName;
            }

            // Update description
            if (buildingDescriptionText != null)
            {
                buildingDescriptionText.text = building.description;
            }

            // Update icon
            if (buildingIconImage != null && building.icon != null)
            {
                buildingIconImage.sprite = building.icon;
            }

            // Update costs
            UpdateCostDisplay(building);

            // Update build button
            if (buildButton != null)
            {
                string reason;
                bool canBuild = building.MeetsRequirements(out reason) && building.HasResources(out reason);
                buildButton.interactable = canBuild;
            }
        }

        /// <summary>
        /// Update cost display
        /// </summary>
        private void UpdateCostDisplay(BuildingData building)
        {
            if (costContainer == null || costEntryPrefab == null)
                return;

            // Clear existing
            foreach (Transform child in costContainer)
            {
                Destroy(child.gameObject);
            }

            // Resource costs
            if (building.buildCosts != null)
            {
                foreach (var cost in building.buildCosts)
                {
                    if (cost.item == null)
                        continue;

                    GameObject entryObj = Instantiate(costEntryPrefab, costContainer);
                    TextMeshProUGUI entryText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
                    Image entryIcon = entryObj.transform.Find("Icon")?.GetComponent<Image>();

                    if (entryText != null)
                    {
                        int playerAmount = Inventory.InventorySystem.Instance.GetItemCount(cost.item.itemID);
                        string text = $"{cost.item.itemName}: {playerAmount}/{cost.quantity}";
                        entryText.text = text;

                        if (playerAmount < cost.quantity)
                        {
                            entryText.color = Color.red;
                        }
                    }

                    if (entryIcon != null && cost.item.icon != null)
                    {
                        entryIcon.sprite = cost.item.icon;
                    }
                }
            }

            // Currency costs
            if (building.currencyCosts != null)
            {
                foreach (var cost in building.currencyCosts)
                {
                    GameObject entryObj = Instantiate(costEntryPrefab, costContainer);
                    TextMeshProUGUI entryText = entryObj.GetComponentInChildren<TextMeshProUGUI>();

                    if (entryText != null)
                    {
                        int playerAmount = CurrencyManager.Instance.GetCurrency(cost.currencyType);
                        string symbol = CurrencyManager.Instance.GetCurrencySymbol(cost.currencyType);
                        string text = $"{cost.amount} {symbol} ({playerAmount})";
                        entryText.text = text;

                        if (playerAmount < cost.amount)
                        {
                            entryText.color = Color.red;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Build button clicked
        /// </summary>
        private void OnBuildButtonClicked()
        {
            if (selectedBuildingData == null)
                return;

            // Enter build mode
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.EnterBuildMode(selectedBuildingData);
                HideBuildingMenu();
                ShowBuildModeHUD();
            }
        }

        /// <summary>
        /// Close menu clicked
        /// </summary>
        private void OnCloseMenuClicked()
        {
            HideBuildingMenu();
        }

        /// <summary>
        /// Demolish mode clicked
        /// </summary>
        private void OnDemolishModeClicked()
        {
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.EnterDemolishMode();
                HideBuildingMenu();
                ShowBuildModeHUD();
            }
        }

        /// <summary>
        /// Exit build mode clicked
        /// </summary>
        private void OnExitBuildModeClicked()
        {
            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.ExitBuildMode();
            }
        }

        /// <summary>
        /// Category filter changed
        /// </summary>
        private void OnCategoryFilterChanged(int index)
        {
            if (index == 0)
            {
                currentFilter = (BuildingCategory)(-1); // All
            }
            else
            {
                currentFilter = (BuildingCategory)(index - 1);
            }

            RefreshBuildingList();
        }

        /// <summary>
        /// Search input changed
        /// </summary>
        private void OnSearchChanged(string searchText)
        {
            RefreshBuildingList();
        }

        /// <summary>
        /// Build mode changed callback
        /// </summary>
        private void OnBuildModeChanged(BuildMode newMode)
        {
            if (newMode == BuildMode.None)
            {
                HideBuildModeHUD();
            }
            else
            {
                ShowBuildModeHUD();
            }
        }

        /// <summary>
        /// Update build mode HUD
        /// </summary>
        private void UpdateBuildModeHUD()
        {
            if (BuildingSystem.Instance == null)
                return;

            // Update building name
            if (currentBuildingNameText != null && BuildingSystem.Instance.selectedBuilding != null)
            {
                currentBuildingNameText.text = BuildingSystem.Instance.selectedBuilding.buildingName;
            }

            // Update placement status
            if (placementStatusText != null)
            {
                bool isValid = true; // TODO: Get from BuildingSystem
                placementStatusText.text = isValid ? "Valid Placement" : "Invalid Placement";
                placementStatusText.color = isValid ? Color.green : Color.red;
            }

            // Update rotation
            if (rotationText != null && BuildingSystem.Instance.selectedBuilding != null)
            {
                if (BuildingSystem.Instance.selectedBuilding.allowRotation)
                {
                    // TODO: Get actual rotation from BuildingSystem
                    rotationText.text = "Rotation: 0°";
                }
                else
                {
                    rotationText.text = "";
                }
            }

            // Update controls help
            if (controlsHelpText != null)
            {
                string controls = "Left Click: Place | Right Click: Cancel";
                if (BuildingSystem.Instance.selectedBuilding != null && BuildingSystem.Instance.selectedBuilding.allowRotation)
                {
                    controls += " | R/Scroll: Rotate";
                }
                controlsHelpText.text = controls;
            }
        }

        /// <summary>
        /// Show building menu
        /// </summary>
        public void ShowBuildingMenu()
        {
            if (buildingMenuPanel != null)
            {
                buildingMenuPanel.SetActive(true);
                RefreshBuildingList();
            }
        }

        /// <summary>
        /// Hide building menu
        /// </summary>
        public void HideBuildingMenu()
        {
            if (buildingMenuPanel != null)
            {
                buildingMenuPanel.SetActive(false);
            }

            if (buildingDetailsPanel != null)
            {
                buildingDetailsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show build mode HUD
        /// </summary>
        public void ShowBuildModeHUD()
        {
            if (buildModeHUDPanel != null)
            {
                buildModeHUDPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide build mode HUD
        /// </summary>
        public void HideBuildModeHUD()
        {
            if (buildModeHUDPanel != null)
            {
                buildModeHUDPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Show upgrade panel for selected building
        /// </summary>
        public void ShowUpgradePanel(PlacedBuilding building)
        {
            selectedPlacedBuilding = building;

            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
            }

            if (building == null || building.buildingData == null)
                return;

            // Check if can upgrade
            bool canUpgrade = building.CanUpgrade();

            if (upgradeButton != null)
            {
                upgradeButton.interactable = canUpgrade;
            }

            // Update info text
            if (upgradeInfoText != null)
            {
                if (canUpgrade)
                {
                    int nextTier = building.currentTier + 1;
                    BuildingUpgradeTier tier = building.buildingData.GetUpgradeTier(nextTier);
                    if (tier != null)
                    {
                        upgradeInfoText.text = $"Upgrade to {tier.tierName}\n{tier.description}";
                    }
                }
                else
                {
                    if (building.currentTier >= building.buildingData.GetMaxTier())
                    {
                        upgradeInfoText.text = "Max tier reached";
                    }
                    else
                    {
                        upgradeInfoText.text = "Cannot upgrade (missing resources)";
                    }
                }
            }

            // Update upgrade costs
            UpdateUpgradeCostDisplay(building);
        }

        /// <summary>
        /// Hide upgrade panel
        /// </summary>
        public void HideUpgradePanel()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }

            selectedPlacedBuilding = null;
        }

        /// <summary>
        /// Update upgrade cost display
        /// </summary>
        private void UpdateUpgradeCostDisplay(PlacedBuilding building)
        {
            if (upgradeCostContainer == null || costEntryPrefab == null)
                return;

            // Clear existing
            foreach (Transform child in upgradeCostContainer)
            {
                Destroy(child.gameObject);
            }

            if (building == null || !building.CanUpgrade())
                return;

            int nextTier = building.currentTier + 1;
            BuildingUpgradeTier tier = building.buildingData.GetUpgradeTier(nextTier);
            if (tier == null)
                return;

            // Resource costs
            if (tier.resourceCosts != null)
            {
                foreach (var cost in tier.resourceCosts)
                {
                    if (cost.item == null)
                        continue;

                    GameObject entryObj = Instantiate(costEntryPrefab, upgradeCostContainer);
                    TextMeshProUGUI entryText = entryObj.GetComponentInChildren<TextMeshProUGUI>();

                    if (entryText != null)
                    {
                        int playerAmount = Inventory.InventorySystem.Instance.GetItemCount(cost.item.itemID);
                        entryText.text = $"{cost.item.itemName}: {playerAmount}/{cost.quantity}";

                        if (playerAmount < cost.quantity)
                        {
                            entryText.color = Color.red;
                        }
                    }
                }
            }

            // Currency costs
            if (tier.currencyCosts != null)
            {
                foreach (var cost in tier.currencyCosts)
                {
                    GameObject entryObj = Instantiate(costEntryPrefab, upgradeCostContainer);
                    TextMeshProUGUI entryText = entryObj.GetComponentInChildren<TextMeshProUGUI>();

                    if (entryText != null)
                    {
                        int playerAmount = CurrencyManager.Instance.GetCurrency(cost.currencyType);
                        entryText.text = $"{cost.currencyType}: {playerAmount}/{cost.amount}";

                        if (playerAmount < cost.amount)
                        {
                            entryText.color = Color.red;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Upgrade button clicked
        /// </summary>
        private void OnUpgradeButtonClicked()
        {
            if (selectedPlacedBuilding == null)
                return;

            if (BuildingSystem.Instance != null)
            {
                BuildingSystem.Instance.UpgradeBuilding(selectedPlacedBuilding);
            }

            HideUpgradePanel();
        }

        /// <summary>
        /// Hide all panels
        /// </summary>
        public void HideAll()
        {
            HideBuildingMenu();
            HideBuildModeHUD();
            HideUpgradePanel();
        }
    }
}
