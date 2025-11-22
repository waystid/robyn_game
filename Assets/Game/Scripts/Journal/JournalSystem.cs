using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.Journal
{
    /// <summary>
    /// Lore entry category
    /// </summary>
    public enum LoreCategory
    {
        History,        // World history
        Geography,      // Locations and places
        Culture,        // Customs and traditions
        Magic,          // Magic system lore
        Creatures,      // Bestiary entries
        Characters,     // Character backgrounds
        Events,         // Significant events
        Legends,        // Myths and legends
        Custom          // Custom category
    }

    /// <summary>
    /// Lore entry
    /// </summary>
    [System.Serializable]
    public class LoreEntry
    {
        [Header("Entry Info")]
        public string entryID = "lore_001";
        public string title = "Lore Entry";
        public LoreCategory category = LoreCategory.History;

        [TextArea(5, 15)]
        public string content = "Lore content...";

        [Header("Visual")]
        public Sprite illustration;
        public Color categoryColor = Color.white;

        [Header("Unlock")]
        public bool isUnlocked = false;
        public bool startUnlocked = false;
        public string unlockHint = "???";

        [Header("Related")]
        public List<string> relatedEntryIDs = new List<string>();
        public List<string> relatedNPCIDs = new List<string>();

        // Runtime
        [System.NonSerialized]
        public float unlockTime = 0f;
    }

    /// <summary>
    /// NPC journal entry
    /// </summary>
    [System.Serializable]
    public class NPCJournalEntry
    {
        [Header("NPC Info")]
        public string npcID;
        public string npcName;

        [TextArea(3, 8)]
        public string description = "NPC description...";

        public Sprite portrait;

        [Header("Details")]
        public string occupation = "";
        public string location = "";
        public string personality = "";

        [TextArea(2, 5)]
        public string backgroundStory = "";

        [Header("Relationships")]
        public List<string> relatedNPCIDs = new List<string>();
        public string familyInfo = "";

        [Header("Preferences")]
        public List<Item> favoriteItems = new List<Item>();
        public string schedule = "";

        [Header("Unlock")]
        public bool isDiscovered = false;
        public bool autoDiscoverOnMeet = true;

        // Runtime
        [System.NonSerialized]
        public int timesInteracted = 0;

        [System.NonSerialized]
        public float firstMetTime = 0f;
    }

    /// <summary>
    /// Item encyclopedia entry
    /// </summary>
    [System.Serializable]
    public class ItemEncyclopediaEntry
    {
        public Item item;
        public bool isDiscovered = false;

        [TextArea(3, 6)]
        public string notes = "";

        public int timesSeen = 0;
        public int timesObtained = 0;
        public int timesUsed = 0;

        [Header("Locations")]
        public List<string> foundLocations = new List<string>();
        public List<string> vendors = new List<string>();

        // Runtime
        [System.NonSerialized]
        public float firstSeenTime = 0f;
    }

    /// <summary>
    /// Location entry
    /// </summary>
    [System.Serializable]
    public class LocationEntry
    {
        public string locationID;
        public string locationName;

        [TextArea(3, 6)]
        public string description = "";

        public Sprite mapImage;
        public Vector3 worldPosition;

        public bool isDiscovered = false;
        public float discoveryTime = 0f;

        [Header("Info")]
        public List<string> landmarksText = new List<string>();
        public List<string> npcsFound = new List<string>();
        public List<string> resourcesAvailable = new List<string>();
    }

    /// <summary>
    /// Journal save data
    /// </summary>
    [System.Serializable]
    public class JournalSaveData
    {
        public List<string> unlockedLoreIDs = new List<string>();
        public List<string> discoveredNPCIDs = new List<string>();
        public List<string> discoveredItemIDs = new List<string>();
        public List<string> discoveredLocationIDs = new List<string>();
        public Dictionary<string, int> itemSeenCounts = new Dictionary<string, int>();
        public Dictionary<string, string> itemNotes = new Dictionary<string, string>();
    }

    /// <summary>
    /// Journal system singleton.
    /// Manages codex, lore entries, NPC information, and item encyclopedia.
    /// </summary>
    public class JournalSystem : MonoBehaviour
    {
        public static JournalSystem Instance { get; private set; }

        [Header("Lore Entries")]
        [Tooltip("All lore entries")]
        public List<LoreEntry> loreEntries = new List<LoreEntry>();

        [Header("NPC Entries")]
        [Tooltip("All NPC journal entries")]
        public List<NPCJournalEntry> npcEntries = new List<NPCJournalEntry>();

        [Header("Item Encyclopedia")]
        [Tooltip("All item encyclopedia entries")]
        public List<ItemEncyclopediaEntry> itemEntries = new List<ItemEncyclopediaEntry>();

        [Header("Locations")]
        [Tooltip("All location entries")]
        public List<LocationEntry> locationEntries = new List<LocationEntry>();

        [Header("Settings")]
        [Tooltip("Auto-discover items when first seen")]
        public bool autoDiscoverItems = true;

        [Tooltip("Auto-discover NPCs when first met")]
        public bool autoDiscoverNPCs = true;

        [Tooltip("Auto-discover locations when entered")]
        public bool autoDiscoverLocations = true;

        [Header("Events")]
        public UnityEvent<LoreEntry> OnLoreUnlocked;
        public UnityEvent<NPCJournalEntry> OnNPCDiscovered;
        public UnityEvent<ItemEncyclopediaEntry> OnItemDiscovered;
        public UnityEvent<LocationEntry> OnLocationDiscovered;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize journal
        /// </summary>
        private void Initialize()
        {
            // Unlock starting lore entries
            foreach (var entry in loreEntries)
            {
                if (entry.startUnlocked)
                {
                    entry.isUnlocked = true;
                }
            }

            Debug.Log($"[JournalSystem] Initialized with {loreEntries.Count} lore entries, {npcEntries.Count} NPCs, {itemEntries.Count} items");
        }

        /// <summary>
        /// Unlock lore entry
        /// </summary>
        public bool UnlockLoreEntry(string entryID)
        {
            LoreEntry entry = GetLoreEntry(entryID);
            if (entry == null)
            {
                Debug.LogWarning($"[JournalSystem] Lore entry '{entryID}' not found!");
                return false;
            }

            if (entry.isUnlocked)
            {
                Debug.Log($"[JournalSystem] Lore entry '{entry.title}' already unlocked");
                return false;
            }

            entry.isUnlocked = true;
            entry.unlockTime = Time.time;

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"New Lore: {entry.title}",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    entry.categoryColor
                );
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("lore_unlocked");
            }

            OnLoreUnlocked?.Invoke(entry);

            Debug.Log($"[JournalSystem] Unlocked lore: {entry.title}");

            return true;
        }

        /// <summary>
        /// Discover NPC
        /// </summary>
        public bool DiscoverNPC(string npcID)
        {
            NPCJournalEntry entry = GetNPCEntry(npcID);
            if (entry == null)
            {
                Debug.LogWarning($"[JournalSystem] NPC entry '{npcID}' not found!");
                return false;
            }

            if (entry.isDiscovered)
            {
                entry.timesInteracted++;
                return false;
            }

            entry.isDiscovered = true;
            entry.firstMetTime = Time.time;
            entry.timesInteracted = 1;

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Met: {entry.npcName}",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.cyan
                );
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("npc_discovered");
            }

            OnNPCDiscovered?.Invoke(entry);

            Debug.Log($"[JournalSystem] Discovered NPC: {entry.npcName}");

            return true;
        }

        /// <summary>
        /// Discover item
        /// </summary>
        public bool DiscoverItem(Item item)
        {
            if (item == null)
                return false;

            ItemEncyclopediaEntry entry = GetItemEntry(item);
            if (entry == null)
            {
                // Create new entry
                entry = new ItemEncyclopediaEntry
                {
                    item = item,
                    isDiscovered = true,
                    firstSeenTime = Time.time,
                    timesSeen = 1
                };
                itemEntries.Add(entry);

                OnItemDiscovered?.Invoke(entry);

                Debug.Log($"[JournalSystem] Discovered item: {item.itemName}");

                return true;
            }
            else if (!entry.isDiscovered)
            {
                entry.isDiscovered = true;
                entry.firstSeenTime = Time.time;
                entry.timesSeen++;

                OnItemDiscovered?.Invoke(entry);

                Debug.Log($"[JournalSystem] Discovered item: {item.itemName}");

                return true;
            }
            else
            {
                entry.timesSeen++;
                return false;
            }
        }

        /// <summary>
        /// Track item obtained
        /// </summary>
        public void TrackItemObtained(Item item)
        {
            if (item == null)
                return;

            ItemEncyclopediaEntry entry = GetItemEntry(item);
            if (entry != null)
            {
                entry.timesObtained++;

                if (autoDiscoverItems)
                {
                    DiscoverItem(item);
                }
            }
        }

        /// <summary>
        /// Track item used
        /// </summary>
        public void TrackItemUsed(Item item)
        {
            if (item == null)
                return;

            ItemEncyclopediaEntry entry = GetItemEntry(item);
            if (entry != null)
            {
                entry.timesUsed++;
            }
        }

        /// <summary>
        /// Add item note
        /// </summary>
        public void AddItemNote(Item item, string note)
        {
            if (item == null)
                return;

            ItemEncyclopediaEntry entry = GetItemEntry(item);
            if (entry != null)
            {
                entry.notes = note;
            }
        }

        /// <summary>
        /// Discover location
        /// </summary>
        public bool DiscoverLocation(string locationID)
        {
            LocationEntry entry = GetLocationEntry(locationID);
            if (entry == null)
            {
                Debug.LogWarning($"[JournalSystem] Location '{locationID}' not found!");
                return false;
            }

            if (entry.isDiscovered)
                return false;

            entry.isDiscovered = true;
            entry.discoveryTime = Time.time;

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Location Discovered: {entry.locationName}",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.green
                );
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("location_discovered");
            }

            OnLocationDiscovered?.Invoke(entry);

            Debug.Log($"[JournalSystem] Discovered location: {entry.locationName}");

            return true;
        }

        /// <summary>
        /// Get lore entry
        /// </summary>
        public LoreEntry GetLoreEntry(string entryID)
        {
            return loreEntries.Find(e => e.entryID == entryID);
        }

        /// <summary>
        /// Get NPC entry
        /// </summary>
        public NPCJournalEntry GetNPCEntry(string npcID)
        {
            return npcEntries.Find(e => e.npcID == npcID || e.npcName == npcID);
        }

        /// <summary>
        /// Get item entry
        /// </summary>
        public ItemEncyclopediaEntry GetItemEntry(Item item)
        {
            return itemEntries.Find(e => e.item == item);
        }

        /// <summary>
        /// Get location entry
        /// </summary>
        public LocationEntry GetLocationEntry(string locationID)
        {
            return locationEntries.Find(e => e.locationID == locationID);
        }

        /// <summary>
        /// Get unlocked lore entries
        /// </summary>
        public List<LoreEntry> GetUnlockedLore()
        {
            return loreEntries.FindAll(e => e.isUnlocked);
        }

        /// <summary>
        /// Get lore by category
        /// </summary>
        public List<LoreEntry> GetLoreByCategory(LoreCategory category)
        {
            return loreEntries.FindAll(e => e.isUnlocked && e.category == category);
        }

        /// <summary>
        /// Get discovered NPCs
        /// </summary>
        public List<NPCJournalEntry> GetDiscoveredNPCs()
        {
            return npcEntries.FindAll(e => e.isDiscovered);
        }

        /// <summary>
        /// Get discovered items
        /// </summary>
        public List<ItemEncyclopediaEntry> GetDiscoveredItems()
        {
            return itemEntries.FindAll(e => e.isDiscovered);
        }

        /// <summary>
        /// Get discovered locations
        /// </summary>
        public List<LocationEntry> GetDiscoveredLocations()
        {
            return locationEntries.FindAll(e => e.isDiscovered);
        }

        /// <summary>
        /// Get completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            int totalEntries = loreEntries.Count + npcEntries.Count + itemEntries.Count + locationEntries.Count;
            if (totalEntries == 0)
                return 1f;

            int discoveredCount = 0;
            discoveredCount += loreEntries.FindAll(e => e.isUnlocked).Count;
            discoveredCount += npcEntries.FindAll(e => e.isDiscovered).Count;
            discoveredCount += itemEntries.FindAll(e => e.isDiscovered).Count;
            discoveredCount += locationEntries.FindAll(e => e.isDiscovered).Count;

            return (float)discoveredCount / totalEntries;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public JournalSaveData GetSaveData()
        {
            JournalSaveData data = new JournalSaveData();

            // Lore
            foreach (var entry in loreEntries)
            {
                if (entry.isUnlocked)
                    data.unlockedLoreIDs.Add(entry.entryID);
            }

            // NPCs
            foreach (var entry in npcEntries)
            {
                if (entry.isDiscovered)
                    data.discoveredNPCIDs.Add(entry.npcID);
            }

            // Items
            foreach (var entry in itemEntries)
            {
                if (entry.isDiscovered)
                {
                    data.discoveredItemIDs.Add(entry.item.itemID);
                    data.itemSeenCounts[entry.item.itemID] = entry.timesSeen;
                    if (!string.IsNullOrEmpty(entry.notes))
                    {
                        data.itemNotes[entry.item.itemID] = entry.notes;
                    }
                }
            }

            // Locations
            foreach (var entry in locationEntries)
            {
                if (entry.isDiscovered)
                    data.discoveredLocationIDs.Add(entry.locationID);
            }

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(JournalSaveData data)
        {
            if (data == null)
                return;

            // Lore
            foreach (var loreID in data.unlockedLoreIDs)
            {
                LoreEntry entry = GetLoreEntry(loreID);
                if (entry != null)
                    entry.isUnlocked = true;
            }

            // NPCs
            foreach (var npcID in data.discoveredNPCIDs)
            {
                NPCJournalEntry entry = GetNPCEntry(npcID);
                if (entry != null)
                    entry.isDiscovered = true;
            }

            // Items
            foreach (var itemID in data.discoveredItemIDs)
            {
                Item item = InventoryManager.Instance?.GetItemByID(itemID);
                if (item != null)
                {
                    ItemEncyclopediaEntry entry = GetItemEntry(item);
                    if (entry != null)
                    {
                        entry.isDiscovered = true;

                        if (data.itemSeenCounts.ContainsKey(itemID))
                            entry.timesSeen = data.itemSeenCounts[itemID];

                        if (data.itemNotes.ContainsKey(itemID))
                            entry.notes = data.itemNotes[itemID];
                    }
                }
            }

            // Locations
            foreach (var locationID in data.discoveredLocationIDs)
            {
                LocationEntry entry = GetLocationEntry(locationID);
                if (entry != null)
                    entry.isDiscovered = true;
            }

            Debug.Log($"[JournalSystem] Loaded journal data: {data.unlockedLoreIDs.Count} lore, {data.discoveredNPCIDs.Count} NPCs, {data.discoveredItemIDs.Count} items");
        }
    }
}
