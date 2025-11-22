using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Journal;

namespace CozyGame.UI
{
    /// <summary>
    /// Codex tab type
    /// </summary>
    public enum CodexTab
    {
        Lore,
        NPCs,
        Items,
        Locations
    }

    /// <summary>
    /// Codex UI controller.
    /// Encyclopedia showing lore, NPCs, items, and locations.
    /// </summary>
    public class CodexUI : MonoBehaviour
    {
        public static CodexUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main codex panel")]
        public GameObject codexPanel;

        [Tooltip("List panel")]
        public GameObject listPanel;

        [Tooltip("Details panel")]
        public GameObject detailsPanel;

        [Header("Tabs")]
        [Tooltip("Lore tab button")]
        public Button loreTabButton;

        [Tooltip("NPCs tab button")]
        public Button npcsTabButton;

        [Tooltip("Items tab button")]
        public Button itemsTabButton;

        [Tooltip("Locations tab button")]
        public Button locationsTabButton;

        [Header("List View")]
        [Tooltip("Entry list container")]
        public RectTransform listContainer;

        [Tooltip("Entry list item prefab")]
        public GameObject listItemPrefab;

        [Tooltip("Empty list message")]
        public GameObject emptyListMessage;

        [Tooltip("Category filter dropdown (for lore)")]
        public TMP_Dropdown categoryDropdown;

        [Header("Details View")]
        [Tooltip("Detail title text")]
        public TextMeshProUGUI detailTitleText;

        [Tooltip("Detail content text")]
        public TextMeshProUGUI detailContentText;

        [Tooltip("Detail image")]
        public Image detailImage;

        [Tooltip("Detail stats container")]
        public RectTransform detailStatsContainer;

        [Tooltip("Detail stats item prefab")]
        public GameObject detailStatsItemPrefab;

        [Header("NPC Specific")]
        [Tooltip("NPC portrait image")]
        public Image npcPortraitImage;

        [Tooltip("NPC occupation text")]
        public TextMeshProUGUI npcOccupationText;

        [Tooltip("NPC location text")]
        public TextMeshProUGUI npcLocationText;

        [Tooltip("NPC favorite items container")]
        public RectTransform npcFavoriteItemsContainer;

        [Header("Item Specific")]
        [Tooltip("Item icon image")]
        public Image itemIconImage;

        [Tooltip("Item value text")]
        public TextMeshProUGUI itemValueText;

        [Tooltip("Item rarity text")]
        public TextMeshProUGUI itemRarityText;

        [Tooltip("Item notes input")]
        public TMP_InputField itemNotesInput;

        [Header("Controls")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Back button (from details)")]
        public Button backButton;

        [Tooltip("Search input")]
        public TMP_InputField searchInput;

        [Header("Progress")]
        [Tooltip("Completion percentage text")]
        public TextMeshProUGUI completionText;

        [Tooltip("Completion progress bar")]
        public Slider completionBar;

        // State
        private CodexTab currentTab = CodexTab.Lore;
        private List<GameObject> listItems = new List<GameObject>();
        private object selectedEntry; // Can be LoreEntry, NPCJournalEntry, or ItemEncyclopediaEntry
        private string searchQuery = "";

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
                closeButton.onClick.AddListener(CloseCodex);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (loreTabButton != null)
            {
                loreTabButton.onClick.AddListener(() => SwitchTab(CodexTab.Lore));
            }

            if (npcsTabButton != null)
            {
                npcsTabButton.onClick.AddListener(() => SwitchTab(CodexTab.NPCs));
            }

            if (itemsTabButton != null)
            {
                itemsTabButton.onClick.AddListener(() => SwitchTab(CodexTab.Items));
            }

            if (locationsTabButton != null)
            {
                locationsTabButton.onClick.AddListener(() => SwitchTab(CodexTab.Locations));
            }

            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }

            if (categoryDropdown != null)
            {
                categoryDropdown.onValueChanged.AddListener(OnCategoryFilterChanged);
            }

            if (itemNotesInput != null)
            {
                itemNotesInput.onEndEdit.AddListener(OnItemNotesChanged);
            }

            // Hide panels initially
            if (codexPanel != null)
            {
                codexPanel.SetActive(false);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Open codex
        /// </summary>
        public void OpenCodex(CodexTab tab = CodexTab.Lore)
        {
            if (codexPanel != null)
            {
                codexPanel.SetActive(true);
            }

            if (listPanel != null)
            {
                listPanel.SetActive(true);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            SwitchTab(tab);
        }

        /// <summary>
        /// Close codex
        /// </summary>
        public void CloseCodex()
        {
            if (codexPanel != null)
            {
                codexPanel.SetActive(false);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            selectedEntry = null;
        }

        /// <summary>
        /// Switch tab
        /// </summary>
        private void SwitchTab(CodexTab tab)
        {
            currentTab = tab;

            // Update tab button states
            if (loreTabButton != null)
            {
                loreTabButton.interactable = tab != CodexTab.Lore;
            }

            if (npcsTabButton != null)
            {
                npcsTabButton.interactable = tab != CodexTab.NPCs;
            }

            if (itemsTabButton != null)
            {
                itemsTabButton.interactable = tab != CodexTab.Items;
            }

            if (locationsTabButton != null)
            {
                locationsTabButton.interactable = tab != CodexTab.Locations;
            }

            // Show/hide category filter (only for lore)
            if (categoryDropdown != null)
            {
                categoryDropdown.gameObject.SetActive(tab == CodexTab.Lore);
            }

            RefreshList();
            UpdateCompletionDisplay();
        }

        /// <summary>
        /// Refresh list
        /// </summary>
        private void RefreshList()
        {
            // Clear existing items
            foreach (var item in listItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            listItems.Clear();

            if (JournalSystem.Instance == null || listContainer == null)
                return;

            // Get entries based on current tab
            int entryCount = 0;

            switch (currentTab)
            {
                case CodexTab.Lore:
                    entryCount = RefreshLoreList();
                    break;
                case CodexTab.NPCs:
                    entryCount = RefreshNPCList();
                    break;
                case CodexTab.Items:
                    entryCount = RefreshItemList();
                    break;
                case CodexTab.Locations:
                    entryCount = RefreshLocationList();
                    break;
            }

            // Show/hide empty message
            if (emptyListMessage != null)
            {
                emptyListMessage.SetActive(entryCount == 0);
            }
        }

        /// <summary>
        /// Refresh lore list
        /// </summary>
        private int RefreshLoreList()
        {
            List<LoreEntry> entries = JournalSystem.Instance.GetUnlockedLore();

            // Apply category filter
            if (categoryDropdown != null && categoryDropdown.value > 0)
            {
                LoreCategory category = (LoreCategory)(categoryDropdown.value - 1);
                entries = entries.FindAll(e => e.category == category);
            }

            // Apply search
            entries = ApplySearch(entries, e => e.title);

            foreach (var entry in entries)
            {
                CreateLoreListItem(entry);
            }

            return entries.Count;
        }

        /// <summary>
        /// Refresh NPC list
        /// </summary>
        private int RefreshNPCList()
        {
            List<NPCJournalEntry> entries = JournalSystem.Instance.GetDiscoveredNPCs();

            // Apply search
            entries = ApplySearch(entries, e => e.npcName);

            foreach (var entry in entries)
            {
                CreateNPCListItem(entry);
            }

            return entries.Count;
        }

        /// <summary>
        /// Refresh item list
        /// </summary>
        private int RefreshItemList()
        {
            List<ItemEncyclopediaEntry> entries = JournalSystem.Instance.GetDiscoveredItems();

            // Apply search
            entries = ApplySearch(entries, e => e.item.itemName);

            foreach (var entry in entries)
            {
                CreateItemListItem(entry);
            }

            return entries.Count;
        }

        /// <summary>
        /// Refresh location list
        /// </summary>
        private int RefreshLocationList()
        {
            List<LocationEntry> entries = JournalSystem.Instance.GetDiscoveredLocations();

            // Apply search
            entries = ApplySearch(entries, e => e.locationName);

            foreach (var entry in entries)
            {
                CreateLocationListItem(entry);
            }

            return entries.Count;
        }

        /// <summary>
        /// Apply search filter
        /// </summary>
        private List<T> ApplySearch<T>(List<T> entries, System.Func<T, string> getName)
        {
            if (string.IsNullOrEmpty(searchQuery))
                return entries;

            return entries.FindAll(e =>
            {
                string name = getName(e);
                return name.ToLower().Contains(searchQuery.ToLower());
            });
        }

        /// <summary>
        /// Create lore list item
        /// </summary>
        private void CreateLoreListItem(LoreEntry entry)
        {
            if (listItemPrefab == null || listContainer == null)
                return;

            GameObject itemObj = Instantiate(listItemPrefab, listContainer);
            listItems.Add(itemObj);

            SetupListItem(itemObj, entry.title, entry.category.ToString(), entry.categoryColor, () => ShowLoreDetails(entry));
        }

        /// <summary>
        /// Create NPC list item
        /// </summary>
        private void CreateNPCListItem(NPCJournalEntry entry)
        {
            if (listItemPrefab == null || listContainer == null)
                return;

            GameObject itemObj = Instantiate(listItemPrefab, listContainer);
            listItems.Add(itemObj);

            SetupListItem(itemObj, entry.npcName, entry.occupation, Color.cyan, () => ShowNPCDetails(entry));
        }

        /// <summary>
        /// Create item list item
        /// </summary>
        private void CreateItemListItem(ItemEncyclopediaEntry entry)
        {
            if (listItemPrefab == null || listContainer == null || entry.item == null)
                return;

            GameObject itemObj = Instantiate(listItemPrefab, listContainer);
            listItems.Add(itemObj);

            string subtitle = $"Seen {entry.timesSeen} times";
            SetupListItem(itemObj, entry.item.itemName, subtitle, Color.white, () => ShowItemDetails(entry));
        }

        /// <summary>
        /// Create location list item
        /// </summary>
        private void CreateLocationListItem(LocationEntry entry)
        {
            if (listItemPrefab == null || listContainer == null)
                return;

            GameObject itemObj = Instantiate(listItemPrefab, listContainer);
            listItems.Add(itemObj);

            SetupListItem(itemObj, entry.locationName, "", Color.green, () => ShowLocationDetails(entry));
        }

        /// <summary>
        /// Setup list item components
        /// </summary>
        private void SetupListItem(GameObject itemObj, string title, string subtitle, Color color, System.Action onClick)
        {
            TextMeshProUGUI titleText = itemObj.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI subtitleText = itemObj.transform.Find("SubtitleText")?.GetComponent<TextMeshProUGUI>();
            Image colorBar = itemObj.transform.Find("ColorBar")?.GetComponent<Image>();
            Button button = itemObj.GetComponent<Button>();

            if (titleText != null)
            {
                titleText.text = title;
            }

            if (subtitleText != null)
            {
                subtitleText.text = subtitle;
                subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
            }

            if (colorBar != null)
            {
                colorBar.color = color;
            }

            if (button != null)
            {
                button.onClick.AddListener(() => onClick());
            }
        }

        /// <summary>
        /// Show lore details
        /// </summary>
        private void ShowLoreDetails(LoreEntry entry)
        {
            selectedEntry = entry;
            ShowDetailsPanel();

            if (detailTitleText != null)
            {
                detailTitleText.text = entry.title;
            }

            if (detailContentText != null)
            {
                detailContentText.text = entry.content;
            }

            if (detailImage != null)
            {
                if (entry.illustration != null)
                {
                    detailImage.sprite = entry.illustration;
                    detailImage.gameObject.SetActive(true);
                }
                else
                {
                    detailImage.gameObject.SetActive(false);
                }
            }

            HideNPCSpecificElements();
            HideItemSpecificElements();
        }

        /// <summary>
        /// Show NPC details
        /// </summary>
        private void ShowNPCDetails(NPCJournalEntry entry)
        {
            selectedEntry = entry;
            ShowDetailsPanel();

            if (detailTitleText != null)
            {
                detailTitleText.text = entry.npcName;
            }

            if (detailContentText != null)
            {
                string content = entry.description;
                if (!string.IsNullOrEmpty(entry.backgroundStory))
                {
                    content += "\n\n" + entry.backgroundStory;
                }
                detailContentText.text = content;
            }

            // NPC portrait
            if (npcPortraitImage != null)
            {
                if (entry.portrait != null)
                {
                    npcPortraitImage.sprite = entry.portrait;
                    npcPortraitImage.gameObject.SetActive(true);
                }
                else
                {
                    npcPortraitImage.gameObject.SetActive(false);
                }
            }

            // Occupation
            if (npcOccupationText != null)
            {
                npcOccupationText.text = $"Occupation: {entry.occupation}";
                npcOccupationText.gameObject.SetActive(!string.IsNullOrEmpty(entry.occupation));
            }

            // Location
            if (npcLocationText != null)
            {
                npcLocationText.text = $"Location: {entry.location}";
                npcLocationText.gameObject.SetActive(!string.IsNullOrEmpty(entry.location));
            }

            // Favorite items
            if (npcFavoriteItemsContainer != null)
            {
                ClearContainer(npcFavoriteItemsContainer);
                foreach (var item in entry.favoriteItems)
                {
                    if (item != null && item.icon != null)
                    {
                        CreateItemIcon(item.icon, npcFavoriteItemsContainer);
                    }
                }
                npcFavoriteItemsContainer.gameObject.SetActive(entry.favoriteItems.Count > 0);
            }

            HideItemSpecificElements();
        }

        /// <summary>
        /// Show item details
        /// </summary>
        private void ShowItemDetails(ItemEncyclopediaEntry entry)
        {
            if (entry.item == null)
                return;

            selectedEntry = entry;
            ShowDetailsPanel();

            if (detailTitleText != null)
            {
                detailTitleText.text = entry.item.itemName;
            }

            if (detailContentText != null)
            {
                detailContentText.text = entry.item.description;
            }

            // Item icon
            if (itemIconImage != null && entry.item.icon != null)
            {
                itemIconImage.sprite = entry.item.icon;
                itemIconImage.gameObject.SetActive(true);
            }
            else if (itemIconImage != null)
            {
                itemIconImage.gameObject.SetActive(false);
            }

            // Value
            if (itemValueText != null)
            {
                itemValueText.text = $"Value: {entry.item.value} Gold";
                itemValueText.gameObject.SetActive(true);
            }

            // Rarity
            if (itemRarityText != null)
            {
                itemRarityText.text = $"Rarity: {entry.item.rarity}";
                itemRarityText.gameObject.SetActive(true);
            }

            // Notes
            if (itemNotesInput != null)
            {
                itemNotesInput.text = entry.notes;
                itemNotesInput.gameObject.SetActive(true);
            }

            HideNPCSpecificElements();
        }

        /// <summary>
        /// Show location details
        /// </summary>
        private void ShowLocationDetails(LocationEntry entry)
        {
            selectedEntry = entry;
            ShowDetailsPanel();

            if (detailTitleText != null)
            {
                detailTitleText.text = entry.locationName;
            }

            if (detailContentText != null)
            {
                detailContentText.text = entry.description;
            }

            if (detailImage != null)
            {
                if (entry.mapImage != null)
                {
                    detailImage.sprite = entry.mapImage;
                    detailImage.gameObject.SetActive(true);
                }
                else
                {
                    detailImage.gameObject.SetActive(false);
                }
            }

            HideNPCSpecificElements();
            HideItemSpecificElements();
        }

        /// <summary>
        /// Show details panel
        /// </summary>
        private void ShowDetailsPanel()
        {
            if (listPanel != null)
            {
                listPanel.SetActive(false);
            }

            if (detailsPanel != null)
            {
                detailsPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide NPC specific elements
        /// </summary>
        private void HideNPCSpecificElements()
        {
            if (npcPortraitImage != null) npcPortraitImage.gameObject.SetActive(false);
            if (npcOccupationText != null) npcOccupationText.gameObject.SetActive(false);
            if (npcLocationText != null) npcLocationText.gameObject.SetActive(false);
            if (npcFavoriteItemsContainer != null) npcFavoriteItemsContainer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hide item specific elements
        /// </summary>
        private void HideItemSpecificElements()
        {
            if (itemIconImage != null) itemIconImage.gameObject.SetActive(false);
            if (itemValueText != null) itemValueText.gameObject.SetActive(false);
            if (itemRarityText != null) itemRarityText.gameObject.SetActive(false);
            if (itemNotesInput != null) itemNotesInput.gameObject.SetActive(false);
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
        private void CreateItemIcon(Sprite icon, RectTransform container)
        {
            GameObject iconObj = new GameObject("ItemIcon");
            iconObj.transform.SetParent(container);

            Image image = iconObj.AddComponent<Image>();
            image.sprite = icon;
        }

        /// <summary>
        /// Update completion display
        /// </summary>
        private void UpdateCompletionDisplay()
        {
            if (JournalSystem.Instance == null)
                return;

            float completion = JournalSystem.Instance.GetCompletionPercentage();

            if (completionBar != null)
            {
                completionBar.value = completion;
            }

            if (completionText != null)
            {
                completionText.text = $"Codex: {(completion * 100f):F0}% Complete";
            }
        }

        /// <summary>
        /// Search input changed
        /// </summary>
        private void OnSearchChanged(string query)
        {
            searchQuery = query;
            RefreshList();
        }

        /// <summary>
        /// Category filter changed
        /// </summary>
        private void OnCategoryFilterChanged(int value)
        {
            RefreshList();
        }

        /// <summary>
        /// Item notes changed
        /// </summary>
        private void OnItemNotesChanged(string notes)
        {
            if (selectedEntry is ItemEncyclopediaEntry itemEntry)
            {
                itemEntry.notes = notes;

                if (JournalSystem.Instance != null)
                {
                    JournalSystem.Instance.AddItemNote(itemEntry.item, notes);
                }
            }
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClicked()
        {
            if (detailsPanel != null)
            {
                detailsPanel.SetActive(false);
            }

            if (listPanel != null)
            {
                listPanel.SetActive(true);
            }

            selectedEntry = null;
        }
    }
}
