using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Social;

namespace CozyGame.UI
{
    /// <summary>
    /// Relationship UI controller.
    /// Shows all NPC relationships and their status.
    /// </summary>
    public class RelationshipUI : MonoBehaviour
    {
        public static RelationshipUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main relationship panel")]
        public GameObject relationshipPanel;

        [Tooltip("Details panel")]
        public GameObject detailsPanel;

        [Header("NPC List")]
        [Tooltip("NPC list container")]
        public RectTransform npcListContainer;

        [Tooltip("NPC list item prefab")]
        public GameObject npcListItemPrefab;

        [Tooltip("Empty list message")]
        public GameObject emptyListMessage;

        [Header("Details View")]
        [Tooltip("NPC name text")]
        public TextMeshProUGUI detailsNPCNameText;

        [Tooltip("NPC portrait")]
        public Image detailsPortraitImage;

        [Tooltip("Friendship tier text")]
        public TextMeshProUGUI friendshipTierText;

        [Tooltip("Friendship level text")]
        public TextMeshProUGUI friendshipLevelText;

        [Tooltip("Friendship progress bar")]
        public Slider friendshipProgressBar;

        [Tooltip("Friendship progress text")]
        public TextMeshProUGUI friendshipProgressText;

        [Tooltip("Relationship status text")]
        public TextMeshProUGUI relationshipStatusText;

        [Tooltip("Romance level slider")]
        public Slider romanceLevelSlider;

        [Tooltip("Romance level text")]
        public TextMeshProUGUI romanceLevelText;

        [Header("Stats")]
        [Tooltip("Gifts given text")]
        public TextMeshProUGUI giftsGivenText;

        [Tooltip("Dialogues completed text")]
        public TextMeshProUGUI dialoguesCompletedText;

        [Tooltip("Quests completed text")]
        public TextMeshProUGUI questsCompletedText;

        [Header("Preferences")]
        [Tooltip("Loved items container")]
        public RectTransform lovedItemsContainer;

        [Tooltip("Liked items container")]
        public RectTransform likedItemsContainer;

        [Tooltip("Item icon prefab")]
        public GameObject itemIconPrefab;

        [Header("Actions")]
        [Tooltip("Give gift button")]
        public Button giveGiftButton;

        [Tooltip("Start dating button")]
        public Button startDatingButton;

        [Tooltip("Propose button")]
        public Button proposeButton;

        [Header("Controls")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Back button (from details)")]
        public Button backButton;

        [Header("Filters")]
        [Tooltip("Sort by friendship level")]
        public bool sortByFriendship = true;

        [Tooltip("Show only romanceable NPCs")]
        public Toggle romanceableFilterToggle;

        // State
        private List<GameObject> npcListItems = new List<GameObject>();
        private NPCRelationship selectedRelationship;

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
                closeButton.onClick.AddListener(CloseRelationshipUI);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (giveGiftButton != null)
            {
                giveGiftButton.onClick.AddListener(OnGiveGiftClicked);
            }

            if (startDatingButton != null)
            {
                startDatingButton.onClick.AddListener(OnStartDatingClicked);
            }

            if (proposeButton != null)
            {
                proposeButton.onClick.AddListener(OnProposeClicked);
            }

            if (romanceableFilterToggle != null)
            {
                romanceableFilterToggle.onValueChanged.AddListener(OnFilterChanged);
            }

            // Hide panels initially
            if (relationshipPanel != null)
            {
                relationshipPanel.SetActive(false);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Open relationship UI
        /// </summary>
        public void OpenRelationshipUI()
        {
            if (relationshipPanel != null)
            {
                relationshipPanel.SetActive(true);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            RefreshNPCList();
        }

        /// <summary>
        /// Close relationship UI
        /// </summary>
        public void CloseRelationshipUI()
        {
            if (relationshipPanel != null)
            {
                relationshipPanel.SetActive(false);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            selectedRelationship = null;
        }

        /// <summary>
        /// Refresh NPC list
        /// </summary>
        private void RefreshNPCList()
        {
            // Clear existing items
            foreach (var item in npcListItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            npcListItems.Clear();

            if (RelationshipSystem.Instance == null || npcListContainer == null)
                return;

            // Get relationships
            List<NPCRelationship> relationships = RelationshipSystem.Instance.relationships;

            // Apply filters
            if (romanceableFilterToggle != null && romanceableFilterToggle.isOn)
            {
                relationships = relationships.FindAll(r => r.isRomanceable);
            }

            // Sort
            if (sortByFriendship)
            {
                relationships.Sort((a, b) => b.friendshipLevel.CompareTo(a.friendshipLevel));
            }

            // Show/hide empty message
            if (emptyListMessage != null)
            {
                emptyListMessage.SetActive(relationships.Count == 0);
            }

            // Create list items
            foreach (var relationship in relationships)
            {
                CreateNPCListItem(relationship);
            }
        }

        /// <summary>
        /// Create NPC list item
        /// </summary>
        private void CreateNPCListItem(NPCRelationship relationship)
        {
            if (npcListItemPrefab == null || npcListContainer == null)
                return;

            GameObject itemObj = Instantiate(npcListItemPrefab, npcListContainer);
            npcListItems.Add(itemObj);

            // Find components
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI tierText = itemObj.transform.Find("TierText")?.GetComponent<TextMeshProUGUI>();
            Slider progressBar = itemObj.transform.Find("ProgressBar")?.GetComponent<Slider>();
            Image statusIcon = itemObj.transform.Find("StatusIcon")?.GetComponent<Image>();
            Button button = itemObj.GetComponent<Button>();

            // Set name
            if (nameText != null)
            {
                nameText.text = relationship.npcName;
            }

            // Set tier
            if (tierText != null)
            {
                FriendshipTier tier = relationship.GetFriendshipTier();
                tierText.text = $"Lv{relationship.friendshipLevel} - {tier}";

                // Color by tier
                switch (tier)
                {
                    case FriendshipTier.Stranger:
                        tierText.color = Color.gray;
                        break;
                    case FriendshipTier.Acquaintance:
                        tierText.color = Color.white;
                        break;
                    case FriendshipTier.Friend:
                        tierText.color = Color.green;
                        break;
                    case FriendshipTier.GoodFriend:
                        tierText.color = Color.cyan;
                        break;
                    case FriendshipTier.BestFriend:
                        tierText.color = Color.yellow;
                        break;
                    case FriendshipTier.Soulmate:
                        tierText.color = Color.magenta;
                        break;
                }
            }

            // Set progress
            if (progressBar != null)
            {
                progressBar.value = relationship.GetLevelProgress();
            }

            // Set status icon
            if (statusIcon != null && relationship.isRomanceable)
            {
                switch (relationship.relationshipStatus)
                {
                    case RelationshipStatus.Dating:
                        statusIcon.color = Color.magenta;
                        statusIcon.gameObject.SetActive(true);
                        break;
                    case RelationshipStatus.Engaged:
                        statusIcon.color = Color.yellow;
                        statusIcon.gameObject.SetActive(true);
                        break;
                    case RelationshipStatus.Married:
                        statusIcon.color = Color.red;
                        statusIcon.gameObject.SetActive(true);
                        break;
                    default:
                        statusIcon.gameObject.SetActive(false);
                        break;
                }
            }

            // Setup button
            if (button != null)
            {
                NPCRelationship r = relationship; // Capture for lambda
                button.onClick.AddListener(() => OnNPCSelected(r));
            }
        }

        /// <summary>
        /// NPC selected callback
        /// </summary>
        private void OnNPCSelected(NPCRelationship relationship)
        {
            selectedRelationship = relationship;
            ShowDetails();
        }

        /// <summary>
        /// Show details panel
        /// </summary>
        private void ShowDetails()
        {
            if (selectedRelationship == null || detailsPanel == null)
                return;

            // Hide list, show details
            if (relationshipPanel != null)
            {
                relationshipPanel.SetActive(false);
            }

            detailsPanel.SetActive(true);

            UpdateDetailsPanel();
        }

        /// <summary>
        /// Update details panel
        /// </summary>
        private void UpdateDetailsPanel()
        {
            if (selectedRelationship == null)
                return;

            // NPC name
            if (detailsNPCNameText != null)
            {
                detailsNPCNameText.text = selectedRelationship.npcName;
            }

            // Friendship tier
            FriendshipTier tier = selectedRelationship.GetFriendshipTier();
            if (friendshipTierText != null)
            {
                friendshipTierText.text = tier.ToString();

                // Color by tier
                switch (tier)
                {
                    case FriendshipTier.Stranger:
                        friendshipTierText.color = Color.gray;
                        break;
                    case FriendshipTier.Acquaintance:
                        friendshipTierText.color = Color.white;
                        break;
                    case FriendshipTier.Friend:
                        friendshipTierText.color = Color.green;
                        break;
                    case FriendshipTier.GoodFriend:
                        friendshipTierText.color = Color.cyan;
                        break;
                    case FriendshipTier.BestFriend:
                        friendshipTierText.color = Color.yellow;
                        break;
                    case FriendshipTier.Soulmate:
                        friendshipTierText.color = Color.magenta;
                        break;
                }
            }

            // Friendship level
            if (friendshipLevelText != null)
            {
                friendshipLevelText.text = $"Level {selectedRelationship.friendshipLevel}/10";
            }

            // Friendship progress
            float progress = selectedRelationship.GetLevelProgress();
            if (friendshipProgressBar != null)
            {
                friendshipProgressBar.value = progress;
            }

            if (friendshipProgressText != null)
            {
                friendshipProgressText.text = $"{selectedRelationship.friendshipPoints}/{selectedRelationship.pointsPerLevel}";
            }

            // Relationship status
            if (relationshipStatusText != null)
            {
                if (selectedRelationship.isRomanceable)
                {
                    relationshipStatusText.text = $"Status: {selectedRelationship.relationshipStatus}";
                }
                else
                {
                    relationshipStatusText.text = "Not Romanceable";
                }
            }

            // Romance level
            if (romanceLevelSlider != null)
            {
                romanceLevelSlider.value = selectedRelationship.romanceLevel / 100f;
                romanceLevelSlider.gameObject.SetActive(selectedRelationship.isRomanceable);
            }

            if (romanceLevelText != null)
            {
                romanceLevelText.text = $"Romance: {selectedRelationship.romanceLevel}/100";
                romanceLevelText.gameObject.SetActive(selectedRelationship.isRomanceable);
            }

            // Stats
            if (giftsGivenText != null)
            {
                giftsGivenText.text = $"Gifts Given: {selectedRelationship.giftsGiven}";
            }

            if (dialoguesCompletedText != null)
            {
                dialoguesCompletedText.text = $"Conversations: {selectedRelationship.dialoguesCompleted}";
            }

            if (questsCompletedText != null)
            {
                questsCompletedText.text = $"Quests Completed: {selectedRelationship.questsCompleted}";
            }

            // Preferences
            UpdatePreferencesDisplay();

            // Update action buttons
            UpdateActionButtons();
        }

        /// <summary>
        /// Update preferences display
        /// </summary>
        private void UpdatePreferencesDisplay()
        {
            if (selectedRelationship == null)
                return;

            // Loved items
            if (lovedItemsContainer != null)
            {
                ClearContainer(lovedItemsContainer);

                foreach (var item in selectedRelationship.lovedItems)
                {
                    CreateItemIcon(item, lovedItemsContainer);
                }
            }

            // Liked items
            if (likedItemsContainer != null)
            {
                ClearContainer(likedItemsContainer);

                foreach (var item in selectedRelationship.likedItems)
                {
                    CreateItemIcon(item, likedItemsContainer);
                }
            }
        }

        /// <summary>
        /// Clear container
        /// </summary>
        private void ClearContainer(RectTransform container)
        {
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Create item icon
        /// </summary>
        private void CreateItemIcon(Inventory.Item item, RectTransform container)
        {
            if (itemIconPrefab == null || container == null || item == null)
                return;

            GameObject iconObj = Instantiate(itemIconPrefab, container);

            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null && item.icon != null)
            {
                iconImage.sprite = item.icon;
            }
        }

        /// <summary>
        /// Update action buttons
        /// </summary>
        private void UpdateActionButtons()
        {
            if (selectedRelationship == null)
                return;

            // Give gift button
            if (giveGiftButton != null)
            {
                giveGiftButton.interactable = selectedRelationship.CanGiftToday();
            }

            // Start dating button
            if (startDatingButton != null)
            {
                bool canStartDating = selectedRelationship.isRomanceable &&
                                      selectedRelationship.relationshipStatus == RelationshipStatus.Single &&
                                      selectedRelationship.friendshipLevel >= 5;

                startDatingButton.interactable = canStartDating;
                startDatingButton.gameObject.SetActive(selectedRelationship.isRomanceable);
            }

            // Propose button
            if (proposeButton != null)
            {
                bool canPropose = selectedRelationship.isRomanceable &&
                                 selectedRelationship.relationshipStatus == RelationshipStatus.Dating &&
                                 selectedRelationship.friendshipLevel >= 10 &&
                                 selectedRelationship.romanceLevel >= 75;

                proposeButton.interactable = canPropose;
                proposeButton.gameObject.SetActive(selectedRelationship.isRomanceable &&
                                                   (selectedRelationship.relationshipStatus == RelationshipStatus.Dating ||
                                                    selectedRelationship.relationshipStatus == RelationshipStatus.Engaged));
            }
        }

        /// <summary>
        /// Give gift button clicked
        /// </summary>
        private void OnGiveGiftClicked()
        {
            if (selectedRelationship == null)
                return;

            // Open gift UI
            if (GiftUI.Instance != null)
            {
                GiftUI.Instance.OpenGiftUI(selectedRelationship.npcName);

                // Close this UI
                CloseRelationshipUI();
            }
        }

        /// <summary>
        /// Start dating button clicked
        /// </summary>
        private void OnStartDatingClicked()
        {
            if (selectedRelationship == null || RelationshipSystem.Instance == null)
                return;

            bool success = RelationshipSystem.Instance.StartDating(selectedRelationship.npcName);

            if (success)
            {
                UpdateDetailsPanel();
            }
        }

        /// <summary>
        /// Propose button clicked
        /// </summary>
        private void OnProposeClicked()
        {
            if (selectedRelationship == null || RelationshipSystem.Instance == null)
                return;

            bool success = RelationshipSystem.Instance.ProposeMarriage(selectedRelationship.npcName);

            if (success)
            {
                UpdateDetailsPanel();
            }
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClicked()
        {
            // Hide details, show list
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            if (relationshipPanel != null)
            {
                relationshipPanel.SetActive(true);
            }

            selectedRelationship = null;
        }

        /// <summary>
        /// Filter changed callback
        /// </summary>
        private void OnFilterChanged(bool value)
        {
            RefreshNPCList();
        }
    }
}
