using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.FastTravel;

namespace CozyGame.UI
{
    /// <summary>
    /// Fast travel UI controller.
    /// Shows waypoint list and travel confirmation dialog.
    /// </summary>
    public class FastTravelUI : MonoBehaviour
    {
        public static FastTravelUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main fast travel panel")]
        public GameObject fastTravelPanel;

        [Tooltip("Waypoint list panel")]
        public GameObject waypointListPanel;

        [Tooltip("Travel confirmation panel")]
        public GameObject confirmationPanel;

        [Header("Waypoint List")]
        [Tooltip("Waypoint list container")]
        public RectTransform waypointListContainer;

        [Tooltip("Waypoint list item prefab")]
        public GameObject waypointListItemPrefab;

        [Tooltip("No waypoints message")]
        public GameObject noWaypointsMessage;

        [Header("Confirmation Dialog")]
        [Tooltip("Waypoint name text")]
        public TextMeshProUGUI confirmWaypointNameText;

        [Tooltip("Waypoint description text")]
        public TextMeshProUGUI confirmDescriptionText;

        [Tooltip("Distance text")]
        public TextMeshProUGUI confirmDistanceText;

        [Tooltip("Cost text (currency)")]
        public TextMeshProUGUI confirmCostText;

        [Tooltip("Mana cost text")]
        public TextMeshProUGUI confirmManaCostText;

        [Tooltip("Warning text (if can't afford)")]
        public TextMeshProUGUI confirmWarningText;

        [Tooltip("Travel button")]
        public Button confirmTravelButton;

        [Tooltip("Cancel button")]
        public Button confirmCancelButton;

        [Header("Controls")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Refresh button")]
        public Button refreshButton;

        [Header("Filters")]
        [Tooltip("Show locked waypoints")]
        public bool showLockedWaypoints = false;

        [Tooltip("Sort by distance")]
        public bool sortByDistance = true;

        // State
        private List<GameObject> waypointListItems = new List<GameObject>();
        private Waypoint selectedWaypoint;

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
                closeButton.onClick.AddListener(Close);
            }

            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(RefreshWaypointList);
            }

            if (confirmTravelButton != null)
            {
                confirmTravelButton.onClick.AddListener(OnConfirmTravel);
            }

            if (confirmCancelButton != null)
            {
                confirmCancelButton.onClick.AddListener(OnCancelTravel);
            }

            // Hide panels initially
            if (fastTravelPanel != null)
            {
                fastTravelPanel.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Open fast travel UI
        /// </summary>
        public void Open()
        {
            if (fastTravelPanel != null)
            {
                fastTravelPanel.SetActive(true);
            }

            if (waypointListPanel != null)
            {
                waypointListPanel.SetActive(true);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            RefreshWaypointList();
        }

        /// <summary>
        /// Close fast travel UI
        /// </summary>
        public void Close()
        {
            if (fastTravelPanel != null)
            {
                fastTravelPanel.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            selectedWaypoint = null;
        }

        /// <summary>
        /// Refresh waypoint list
        /// </summary>
        public void RefreshWaypointList()
        {
            if (WaypointSystem.Instance == null || waypointListContainer == null)
                return;

            // Clear existing items
            foreach (var item in waypointListItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            waypointListItems.Clear();

            // Get waypoints
            List<Waypoint> waypoints = new List<Waypoint>();

            if (showLockedWaypoints)
            {
                waypoints.AddRange(WaypointSystem.Instance.waypoints);
            }
            else
            {
                waypoints.AddRange(WaypointSystem.Instance.GetUnlockedWaypoints());
            }

            // Sort by distance if enabled
            if (sortByDistance)
            {
                Vector3 playerPos = GetPlayerPosition();
                waypoints.Sort((a, b) =>
                {
                    float distA = Vector3.Distance(playerPos, a.position);
                    float distB = Vector3.Distance(playerPos, b.position);
                    return distA.CompareTo(distB);
                });
            }

            // Show/hide no waypoints message
            if (noWaypointsMessage != null)
            {
                noWaypointsMessage.SetActive(waypoints.Count == 0);
            }

            // Create list items
            foreach (var waypoint in waypoints)
            {
                CreateWaypointListItem(waypoint);
            }
        }

        /// <summary>
        /// Create waypoint list item
        /// </summary>
        private void CreateWaypointListItem(Waypoint waypoint)
        {
            if (waypointListItemPrefab == null || waypointListContainer == null)
                return;

            GameObject itemObj = Instantiate(waypointListItemPrefab, waypointListContainer);
            waypointListItems.Add(itemObj);

            // Find components
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI distanceText = itemObj.transform.Find("DistanceText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI costText = itemObj.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            Image iconImage = itemObj.transform.Find("Icon")?.GetComponent<Image>();
            GameObject lockedOverlay = itemObj.transform.Find("LockedOverlay")?.gameObject;
            Button button = itemObj.GetComponent<Button>();

            // Set name
            if (nameText != null)
            {
                nameText.text = waypoint.waypointName;
            }

            // Set distance
            Vector3 playerPos = GetPlayerPosition();
            float distance = Vector3.Distance(playerPos, waypoint.position);

            if (distanceText != null)
            {
                if (distance < 1000f)
                {
                    distanceText.text = $"{distance:F0}m";
                }
                else
                {
                    distanceText.text = $"{(distance / 1000f):F1}km";
                }
            }

            // Set cost
            if (costText != null)
            {
                if (waypoint.isUnlocked)
                {
                    int cost = waypoint.CalculateTravelCost(playerPos);
                    costText.text = $"{cost} {waypoint.costCurrencyType}";

                    if (waypoint.requiresMana)
                    {
                        costText.text += $" + {waypoint.manaCost} Mana";
                    }
                }
                else
                {
                    costText.text = "Locked";
                }
            }

            // Set icon
            if (iconImage != null && waypoint.icon != null)
            {
                iconImage.sprite = waypoint.icon;
                iconImage.color = waypoint.glowColor;
            }

            // Show locked overlay
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(!waypoint.isUnlocked);
            }

            // Setup button
            if (button != null)
            {
                Waypoint w = waypoint; // Capture for lambda
                button.onClick.AddListener(() => OnWaypointSelected(w));

                // Disable button if locked
                button.interactable = waypoint.isUnlocked;
            }
        }

        /// <summary>
        /// Waypoint selected callback
        /// </summary>
        private void OnWaypointSelected(Waypoint waypoint)
        {
            if (waypoint == null || !waypoint.isUnlocked)
                return;

            ShowTravelConfirmation(waypoint);
        }

        /// <summary>
        /// Show travel confirmation dialog
        /// </summary>
        public void ShowTravelConfirmation(Waypoint waypoint)
        {
            if (waypoint == null)
                return;

            selectedWaypoint = waypoint;

            // Hide list, show confirmation
            if (waypointListPanel != null)
            {
                waypointListPanel.SetActive(false);
            }

            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(true);
            }

            // Update confirmation UI
            UpdateConfirmationUI();
        }

        /// <summary>
        /// Update confirmation UI
        /// </summary>
        private void UpdateConfirmationUI()
        {
            if (selectedWaypoint == null)
                return;

            Vector3 playerPos = GetPlayerPosition();

            // Waypoint name
            if (confirmWaypointNameText != null)
            {
                confirmWaypointNameText.text = selectedWaypoint.waypointName;
            }

            // Description
            if (confirmDescriptionText != null)
            {
                confirmDescriptionText.text = selectedWaypoint.description;
            }

            // Distance
            float distance = Vector3.Distance(playerPos, selectedWaypoint.position);
            if (confirmDistanceText != null)
            {
                if (distance < 1000f)
                {
                    confirmDistanceText.text = $"Distance: {distance:F0}m";
                }
                else
                {
                    confirmDistanceText.text = $"Distance: {(distance / 1000f):F1}km";
                }
            }

            // Cost
            int cost = selectedWaypoint.CalculateTravelCost(playerPos);
            bool canAffordCurrency = true;
            bool canAffordMana = true;

            if (confirmCostText != null)
            {
                if (selectedWaypoint.hasTravelCost)
                {
                    confirmCostText.text = $"Cost: {cost} {selectedWaypoint.costCurrencyType}";

                    // Check if can afford
                    if (Economy.CurrencyManager.Instance != null)
                    {
                        canAffordCurrency = Economy.CurrencyManager.Instance.HasCurrency(selectedWaypoint.costCurrencyType, cost);
                    }
                }
                else
                {
                    confirmCostText.text = "Cost: Free";
                }
            }

            // Mana cost
            if (confirmManaCostText != null)
            {
                if (selectedWaypoint.requiresMana)
                {
                    confirmManaCostText.gameObject.SetActive(true);
                    confirmManaCostText.text = $"Mana: {selectedWaypoint.manaCost}";

                    // Check if can afford
                    if (Magic.MagicSystem.Instance != null)
                    {
                        canAffordMana = Magic.MagicSystem.Instance.currentMana >= selectedWaypoint.manaCost;
                    }
                }
                else
                {
                    confirmManaCostText.gameObject.SetActive(false);
                }
            }

            // Warning text
            string reason;
            bool canTravel = selectedWaypoint.CanTravel(playerPos, out reason);

            if (confirmWarningText != null)
            {
                if (!canTravel)
                {
                    confirmWarningText.gameObject.SetActive(true);
                    confirmWarningText.text = reason;
                    confirmWarningText.color = Color.red;
                }
                else
                {
                    confirmWarningText.gameObject.SetActive(false);
                }
            }

            // Enable/disable travel button
            if (confirmTravelButton != null)
            {
                confirmTravelButton.interactable = canTravel;
            }
        }

        /// <summary>
        /// Confirm travel button clicked
        /// </summary>
        private void OnConfirmTravel()
        {
            if (selectedWaypoint == null || WaypointSystem.Instance == null)
                return;

            // Attempt travel
            bool success = WaypointSystem.Instance.TravelToWaypoint(selectedWaypoint.waypointID);

            if (success)
            {
                // Close UI
                Close();
            }
            else
            {
                // Show error (already shown by WaypointSystem)
                Debug.LogWarning($"[FastTravelUI] Failed to travel to {selectedWaypoint.waypointName}");
            }
        }

        /// <summary>
        /// Cancel travel button clicked
        /// </summary>
        private void OnCancelTravel()
        {
            // Hide confirmation, show list
            if (confirmationPanel != null)
            {
                confirmationPanel.SetActive(false);
            }

            if (waypointListPanel != null)
            {
                waypointListPanel.SetActive(true);
            }

            selectedWaypoint = null;
        }

        /// <summary>
        /// Get player position
        /// </summary>
        private Vector3 GetPlayerPosition()
        {
            if (Map.MapSystem.Instance != null)
            {
                return Map.MapSystem.Instance.GetPlayerPosition();
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                return player.transform.position;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Toggle show locked waypoints
        /// </summary>
        public void ToggleShowLocked(bool show)
        {
            showLockedWaypoints = show;
            RefreshWaypointList();
        }

        /// <summary>
        /// Toggle sort by distance
        /// </summary>
        public void ToggleSortByDistance(bool sort)
        {
            sortByDistance = sort;
            RefreshWaypointList();
        }
    }
}
