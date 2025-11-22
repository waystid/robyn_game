using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Quest;

namespace CozyGame.UI
{
    /// <summary>
    /// Quest log UI controller.
    /// Shows active and completed quests with details.
    /// </summary>
    public class QuestLogUI : MonoBehaviour
    {
        public static QuestLogUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main quest log panel")]
        public GameObject questLogPanel;

        [Tooltip("Quest list panel")]
        public GameObject questListPanel;

        [Tooltip("Quest details panel")]
        public GameObject questDetailsPanel;

        [Header("Quest List")]
        [Tooltip("Active quests container")]
        public RectTransform activeQuestsContainer;

        [Tooltip("Completed quests container")]
        public RectTransform completedQuestsContainer;

        [Tooltip("Quest list item prefab")]
        public GameObject questListItemPrefab;

        [Tooltip("Empty active message")]
        public GameObject noActiveQuestsMessage;

        [Tooltip("Empty completed message")]
        public GameObject noCompletedQuestsMessage;

        [Header("Quest Details")]
        [Tooltip("Quest title text")]
        public TextMeshProUGUI questTitleText;

        [Tooltip("Quest description text")]
        public TextMeshProUGUI questDescriptionText;

        [Tooltip("Quest type text")]
        public TextMeshProUGUI questTypeText;

        [Tooltip("Quest level text")]
        public TextMeshProUGUI questLevelText;

        [Tooltip("Quest giver text")]
        public TextMeshProUGUI questGiverText;

        [Header("Objectives")]
        [Tooltip("Objectives container")]
        public RectTransform objectivesContainer;

        [Tooltip("Objective item prefab")]
        public GameObject objectiveItemPrefab;

        [Header("Rewards")]
        [Tooltip("Rewards container")]
        public RectTransform rewardsContainer;

        [Tooltip("Reward item prefab")]
        public GameObject rewardItemPrefab;

        [Tooltip("Experience reward text")]
        public TextMeshProUGUI experienceRewardText;

        [Tooltip("Currency reward text")]
        public TextMeshProUGUI currencyRewardText;

        [Header("Controls")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Back button (from details)")]
        public Button backButton;

        [Tooltip("Track button")]
        public Button trackButton;

        [Tooltip("Untrack button")]
        public Button untrackButton;

        [Tooltip("Abandon button")]
        public Button abandonButton;

        [Header("Tabs")]
        [Tooltip("Active quests tab button")]
        public Button activeTab;

        [Tooltip("Completed quests tab button")]
        public Button completedTab;

        [Header("Filters")]
        [Tooltip("Sort by level")]
        public bool sortByLevel = false;

        [Tooltip("Group by type")]
        public bool groupByType = false;

        // State
        private List<GameObject> questListItems = new List<GameObject>();
        private List<GameObject> objectiveItems = new List<GameObject>();
        private List<GameObject> rewardItems = new List<GameObject>();
        private QuestData selectedQuest;
        private bool showingActive = true;

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
                closeButton.onClick.AddListener(CloseQuestLog);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (trackButton != null)
            {
                trackButton.onClick.AddListener(OnTrackClicked);
            }

            if (untrackButton != null)
            {
                untrackButton.onClick.AddListener(OnUntrackClicked);
            }

            if (abandonButton != null)
            {
                abandonButton.onClick.AddListener(OnAbandonClicked);
            }

            if (activeTab != null)
            {
                activeTab.onClick.AddListener(() => SwitchTab(true));
            }

            if (completedTab != null)
            {
                completedTab.onClick.AddListener(() => SwitchTab(false));
            }

            // Hide panels initially
            if (questLogPanel != null)
            {
                questLogPanel.SetActive(false);
            }

            if (questDetailsPanel != null)
            {
                questDetailsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Open quest log
        /// </summary>
        public void OpenQuestLog()
        {
            if (questLogPanel != null)
            {
                questLogPanel.SetActive(true);
            }

            if (questListPanel != null)
            {
                questListPanel.SetActive(true);
            }

            if (questDetailsPanel != null)
            {
                questDetailsPanel.SetActive(false);
            }

            SwitchTab(true); // Show active quests by default
        }

        /// <summary>
        /// Close quest log
        /// </summary>
        public void CloseQuestLog()
        {
            if (questLogPanel != null)
            {
                questLogPanel.SetActive(false);
            }

            if (questDetailsPanel != null)
            {
                questDetailsPanel.SetActive(false);
            }

            selectedQuest = null;
        }

        /// <summary>
        /// Switch tab
        /// </summary>
        private void SwitchTab(bool showActive)
        {
            showingActive = showActive;

            // Update tab visual state
            if (activeTab != null)
            {
                activeTab.interactable = !showActive;
            }

            if (completedTab != null)
            {
                completedTab.interactable = showActive;
            }

            RefreshQuestList();
        }

        /// <summary>
        /// Refresh quest list
        /// </summary>
        private void RefreshQuestList()
        {
            // Clear existing items
            foreach (var item in questListItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            questListItems.Clear();

            if (QuestManager.Instance == null)
                return;

            // Get quests
            List<QuestData> quests;
            RectTransform container;

            if (showingActive)
            {
                quests = QuestManager.Instance.GetActiveQuests();
                container = activeQuestsContainer;

                // Show/hide empty message
                if (noActiveQuestsMessage != null)
                {
                    noActiveQuestsMessage.SetActive(quests.Count == 0);
                }
            }
            else
            {
                quests = QuestManager.Instance.GetCompletedQuests();
                container = completedQuestsContainer;

                // Show/hide empty message
                if (noCompletedQuestsMessage != null)
                {
                    noCompletedQuestsMessage.SetActive(quests.Count == 0);
                }
            }

            // Sort quests
            if (sortByLevel)
            {
                quests.Sort((a, b) => a.requiredLevel.CompareTo(b.requiredLevel));
            }

            // Create list items
            foreach (var quest in quests)
            {
                CreateQuestListItem(quest, container);
            }
        }

        /// <summary>
        /// Create quest list item
        /// </summary>
        private void CreateQuestListItem(QuestData quest, RectTransform container)
        {
            if (questListItemPrefab == null || container == null)
                return;

            GameObject itemObj = Instantiate(questListItemPrefab, container);
            questListItems.Add(itemObj);

            // Find components
            TextMeshProUGUI titleText = itemObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI typeText = itemObj.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI levelText = itemObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            Image trackedIcon = itemObj.transform.Find("TrackedIcon")?.GetComponent<Image>();
            Slider progressBar = itemObj.transform.Find("ProgressBar")?.GetComponent<Slider>();
            Button button = itemObj.GetComponent<Button>();

            // Set title
            if (titleText != null)
            {
                titleText.text = quest.questName;
            }

            // Set type
            if (typeText != null)
            {
                typeText.text = quest.questType.ToString();

                // Color by type
                switch (quest.questType)
                {
                    case QuestType.Main:
                        typeText.color = Color.yellow;
                        break;
                    case QuestType.Side:
                        typeText.color = Color.cyan;
                        break;
                    case QuestType.Daily:
                        typeText.color = Color.green;
                        break;
                    case QuestType.Repeatable:
                        typeText.color = Color.white;
                        break;
                }
            }

            // Set level
            if (levelText != null)
            {
                levelText.text = $"Lv{quest.requiredLevel}";
            }

            // Tracked icon
            if (trackedIcon != null && QuestManager.Instance != null)
            {
                bool isTracked = QuestManager.Instance.trackedQuest == quest;
                trackedIcon.gameObject.SetActive(isTracked);
            }

            // Progress bar (for active quests)
            if (progressBar != null)
            {
                if (showingActive)
                {
                    float progress = quest.GetCompletionPercentage();
                    progressBar.value = progress;
                    progressBar.gameObject.SetActive(true);
                }
                else
                {
                    progressBar.gameObject.SetActive(false);
                }
            }

            // Setup button
            if (button != null)
            {
                QuestData q = quest; // Capture for lambda
                button.onClick.AddListener(() => OnQuestSelected(q));
            }
        }

        /// <summary>
        /// Quest selected callback
        /// </summary>
        private void OnQuestSelected(QuestData quest)
        {
            selectedQuest = quest;
            ShowQuestDetails();
        }

        /// <summary>
        /// Show quest details
        /// </summary>
        private void ShowQuestDetails()
        {
            if (selectedQuest == null || questDetailsPanel == null)
                return;

            // Hide list, show details
            if (questListPanel != null)
            {
                questListPanel.SetActive(false);
            }

            questDetailsPanel.SetActive(true);

            UpdateQuestDetailsPanel();
        }

        /// <summary>
        /// Update quest details panel
        /// </summary>
        private void UpdateQuestDetailsPanel()
        {
            if (selectedQuest == null)
                return;

            // Title
            if (questTitleText != null)
            {
                questTitleText.text = selectedQuest.questName;
            }

            // Description
            if (questDescriptionText != null)
            {
                questDescriptionText.text = selectedQuest.description;
            }

            // Type
            if (questTypeText != null)
            {
                questTypeText.text = $"Type: {selectedQuest.questType}";
            }

            // Level
            if (questLevelText != null)
            {
                questLevelText.text = $"Recommended Level: {selectedQuest.requiredLevel}";
            }

            // Quest giver
            if (questGiverText != null)
            {
                if (!string.IsNullOrEmpty(selectedQuest.questGiver))
                {
                    questGiverText.text = $"Quest Giver: {selectedQuest.questGiver}";
                    questGiverText.gameObject.SetActive(true);
                }
                else
                {
                    questGiverText.gameObject.SetActive(false);
                }
            }

            // Update objectives
            UpdateObjectivesDisplay();

            // Update rewards
            UpdateRewardsDisplay();

            // Update action buttons
            UpdateActionButtons();
        }

        /// <summary>
        /// Update objectives display
        /// </summary>
        private void UpdateObjectivesDisplay()
        {
            // Clear existing
            foreach (var item in objectiveItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            objectiveItems.Clear();

            if (selectedQuest == null || objectivesContainer == null)
                return;

            // Create objective items
            foreach (var objective in selectedQuest.objectives)
            {
                CreateObjectiveItem(objective);
            }
        }

        /// <summary>
        /// Create objective item
        /// </summary>
        private void CreateObjectiveItem(QuestObjective objective)
        {
            if (objectiveItemPrefab == null || objectivesContainer == null)
                return;

            GameObject itemObj = Instantiate(objectiveItemPrefab, objectivesContainer);
            objectiveItems.Add(itemObj);

            // Find components
            TextMeshProUGUI descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI progressText = itemObj.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            Image checkmark = itemObj.transform.Find("Checkmark")?.GetComponent<Image>();

            // Description
            if (descText != null)
            {
                descText.text = objective.description;
            }

            // Progress
            if (progressText != null)
            {
                if (objective.requiresCount > 1)
                {
                    progressText.text = $"{objective.currentCount}/{objective.requiresCount}";
                    progressText.gameObject.SetActive(true);
                }
                else
                {
                    progressText.gameObject.SetActive(false);
                }
            }

            // Checkmark
            if (checkmark != null)
            {
                checkmark.gameObject.SetActive(objective.isCompleted);
            }
        }

        /// <summary>
        /// Update rewards display
        /// </summary>
        private void UpdateRewardsDisplay()
        {
            // Clear existing
            foreach (var item in rewardItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            rewardItems.Clear();

            if (selectedQuest == null)
                return;

            // Experience
            if (experienceRewardText != null)
            {
                if (selectedQuest.experienceReward > 0)
                {
                    experienceRewardText.text = $"+{selectedQuest.experienceReward} XP";
                    experienceRewardText.gameObject.SetActive(true);
                }
                else
                {
                    experienceRewardText.gameObject.SetActive(false);
                }
            }

            // Currency
            if (currencyRewardText != null)
            {
                if (selectedQuest.goldReward > 0)
                {
                    currencyRewardText.text = $"+{selectedQuest.goldReward} Gold";
                    currencyRewardText.gameObject.SetActive(true);
                }
                else
                {
                    currencyRewardText.gameObject.SetActive(false);
                }
            }

            // Item rewards
            if (rewardsContainer != null)
            {
                foreach (var reward in selectedQuest.itemRewards)
                {
                    CreateRewardItem(reward);
                }
            }
        }

        /// <summary>
        /// Create reward item
        /// </summary>
        private void CreateRewardItem(QuestItemReward reward)
        {
            if (rewardItemPrefab == null || rewardsContainer == null)
                return;

            GameObject itemObj = Instantiate(rewardItemPrefab, rewardsContainer);
            rewardItems.Add(itemObj);

            // Find components
            Image iconImage = itemObj.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI quantityText = itemObj.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();

            // Icon
            if (iconImage != null && reward.item != null && reward.item.icon != null)
            {
                iconImage.sprite = reward.item.icon;
            }

            // Name
            if (nameText != null && reward.item != null)
            {
                nameText.text = reward.item.itemName;
            }

            // Quantity
            if (quantityText != null)
            {
                quantityText.text = $"x{reward.quantity}";
            }
        }

        /// <summary>
        /// Update action buttons
        /// </summary>
        private void UpdateActionButtons()
        {
            if (selectedQuest == null || QuestManager.Instance == null)
                return;

            bool isActive = QuestManager.Instance.GetActiveQuests().Contains(selectedQuest);
            bool isTracked = QuestManager.Instance.trackedQuest == selectedQuest;

            // Track button
            if (trackButton != null)
            {
                trackButton.gameObject.SetActive(isActive && !isTracked);
            }

            // Untrack button
            if (untrackButton != null)
            {
                untrackButton.gameObject.SetActive(isActive && isTracked);
            }

            // Abandon button
            if (abandonButton != null)
            {
                abandonButton.gameObject.SetActive(isActive && selectedQuest.canAbandon);
            }
        }

        /// <summary>
        /// Track button clicked
        /// </summary>
        private void OnTrackClicked()
        {
            if (selectedQuest == null || QuestManager.Instance == null)
                return;

            QuestManager.Instance.TrackQuest(selectedQuest);
            UpdateQuestDetailsPanel();
            RefreshQuestList(); // Refresh to update tracked icon
        }

        /// <summary>
        /// Untrack button clicked
        /// </summary>
        private void OnUntrackClicked()
        {
            if (QuestManager.Instance == null)
                return;

            QuestManager.Instance.UntrackQuest();
            UpdateQuestDetailsPanel();
            RefreshQuestList();
        }

        /// <summary>
        /// Abandon button clicked
        /// </summary>
        private void OnAbandonClicked()
        {
            if (selectedQuest == null || QuestManager.Instance == null)
                return;

            // Show confirmation
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    "Quest abandoned",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.red
                );
            }

            QuestManager.Instance.AbandonQuest(selectedQuest);

            // Return to list
            OnBackClicked();
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClicked()
        {
            // Hide details, show list
            if (questDetailsPanel != null)
            {
                questDetailsPanel.SetActive(false);
            }

            if (questListPanel != null)
            {
                questListPanel.SetActive(true);
            }

            selectedQuest = null;

            RefreshQuestList();
        }
    }
}
