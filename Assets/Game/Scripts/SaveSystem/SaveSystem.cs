using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CozyGame.SaveSystem
{
    /// <summary>
    /// Main save system manager.
    /// Handles saving and loading game state to/from disk.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Save Settings")]
        [Tooltip("Save format (JSON is human-readable, Binary is more secure)")]
        public SaveFormat saveFormat = SaveFormat.JSON;

        [Tooltip("Number of save slots available")]
        public int numberOfSaveSlots = 3;

        [Tooltip("Enable encryption for save files")]
        public bool encryptSaves = false;

        [Tooltip("Create backup of previous save before overwriting")]
        public bool createBackups = true;

        [Header("Auto-Save")]
        [Tooltip("Enable auto-save")]
        public bool enableAutoSave = true;

        [Tooltip("Auto-save interval in seconds")]
        public float autoSaveInterval = 300f; // 5 minutes

        [Header("Debug")]
        [Tooltip("Log save/load operations")]
        public bool verboseLogging = true;

        // Events
        public static UnityEvent<int> OnGameSaved = new UnityEvent<int>();
        public static UnityEvent<int> OnGameLoaded = new UnityEvent<int>();
        public static UnityEvent<int> OnSaveDeleted = new UnityEvent<int>();
        public static UnityEvent<string> OnSaveError = new UnityEvent<string>();

        // Current state
        private SaveData currentSaveData;
        private int currentSaveSlot = -1;
        private float timeSinceLastSave = 0f;
        private float sessionStartTime = 0f;

        // Save path
        private string SaveDirectory => Path.Combine(Application.persistentDataPath, "Saves");

        private void Awake()
        {
            // Singleton setup
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

        private void Start()
        {
            sessionStartTime = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            // Auto-save timer
            if (enableAutoSave && currentSaveSlot >= 0)
            {
                timeSinceLastSave += Time.deltaTime;

                if (timeSinceLastSave >= autoSaveInterval)
                {
                    AutoSave();
                }
            }
        }

        /// <summary>
        /// Initialize save system
        /// </summary>
        private void Initialize()
        {
            // Create save directory if it doesn't exist
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Log("Created save directory: " + SaveDirectory);
            }

            Log("SaveSystem initialized");
        }

        /// <summary>
        /// Save game to specific slot
        /// </summary>
        public bool SaveGame(int slotIndex, string slotName = null)
        {
            try
            {
                if (slotIndex < 0 || slotIndex >= numberOfSaveSlots)
                {
                    LogError($"Invalid save slot: {slotIndex}");
                    return false;
                }

                // Capture current game state
                SaveData saveData = CaptureGameState();

                // Update metadata
                float playtime = GetCurrentPlaytime();
                string finalName = slotName ?? $"Save Slot {slotIndex + 1}";
                saveData.UpdateMetadata(finalName, playtime);

                // Create backup if enabled
                if (createBackups && HasSaveFile(slotIndex))
                {
                    CreateBackup(slotIndex);
                }

                // Save to disk
                string filePath = GetSaveFilePath(slotIndex);
                bool success = WriteSaveFile(filePath, saveData);

                if (success)
                {
                    currentSaveData = saveData;
                    currentSaveSlot = slotIndex;
                    timeSinceLastSave = 0f;

                    Log($"Game saved to slot {slotIndex}: {finalName}");
                    OnGameSaved.Invoke(slotIndex);

                    return true;
                }
                else
                {
                    LogError($"Failed to save game to slot {slotIndex}");
                    OnSaveError.Invoke($"Failed to save to slot {slotIndex}");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogError($"Save error: {e.Message}");
                OnSaveError.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Load game from specific slot
        /// </summary>
        public bool LoadGame(int slotIndex)
        {
            try
            {
                if (slotIndex < 0 || slotIndex >= numberOfSaveSlots)
                {
                    LogError($"Invalid save slot: {slotIndex}");
                    return false;
                }

                if (!HasSaveFile(slotIndex))
                {
                    LogError($"No save file in slot {slotIndex}");
                    return false;
                }

                // Read from disk
                string filePath = GetSaveFilePath(slotIndex);
                SaveData saveData = ReadSaveFile(filePath);

                if (saveData == null)
                {
                    LogError($"Failed to load save from slot {slotIndex}");
                    OnSaveError.Invoke($"Corrupted save file in slot {slotIndex}");
                    return false;
                }

                // Store data to apply after scene loads
                currentSaveData = saveData;
                currentSaveSlot = slotIndex;
                timeSinceLastSave = 0f;
                sessionStartTime = Time.realtimeSinceStartup;

                // Load scene first, then apply game state
                string targetScene = saveData.worldData.currentScene;

                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.LoadScene(targetScene, () =>
                    {
                        // Scene loaded, now apply game state
                        ApplyGameState(saveData);

                        Log($"Game loaded from slot {slotIndex}: {saveData.saveName}");
                        OnGameLoaded.Invoke(slotIndex);
                    });
                }
                else
                {
                    LogWarning("SceneTransitionManager not found, applying state without scene transition");
                    ApplyGameState(saveData);

                    Log($"Game loaded from slot {slotIndex}: {saveData.saveName}");
                    OnGameLoaded.Invoke(slotIndex);
                }

                return true;
            }
            catch (Exception e)
            {
                LogError($"Load error: {e.Message}");
                OnSaveError.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Delete save file from slot
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            try
            {
                if (slotIndex < 0 || slotIndex >= numberOfSaveSlots)
                {
                    LogError($"Invalid save slot: {slotIndex}");
                    return false;
                }

                string filePath = GetSaveFilePath(slotIndex);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Log($"Deleted save in slot {slotIndex}");
                    OnSaveDeleted.Invoke(slotIndex);

                    if (currentSaveSlot == slotIndex)
                    {
                        currentSaveSlot = -1;
                        currentSaveData = null;
                    }

                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                LogError($"Delete error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Auto-save to current slot
        /// </summary>
        public void AutoSave()
        {
            if (currentSaveSlot >= 0)
            {
                Log("Auto-saving...");
                SaveGame(currentSaveSlot);
            }
        }

        /// <summary>
        /// Quick save to slot 0
        /// </summary>
        public void QuickSave()
        {
            SaveGame(0, "Quick Save");
        }

        /// <summary>
        /// Quick load from slot 0
        /// </summary>
        public void QuickLoad()
        {
            LoadGame(0);
        }

        /// <summary>
        /// Check if save file exists in slot
        /// </summary>
        public bool HasSaveFile(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Get save metadata without loading full save
        /// </summary>
        public SaveMetadata GetSaveMetadata(int slotIndex)
        {
            if (!HasSaveFile(slotIndex))
                return null;

            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                SaveData saveData = ReadSaveFile(filePath);

                if (saveData != null)
                {
                    return new SaveMetadata
                    {
                        slotIndex = slotIndex,
                        saveName = saveData.saveName,
                        saveDateTime = saveData.saveDateTime,
                        totalPlayTime = saveData.totalPlayTime,
                        playerLevel = saveData.playerData.level,
                        saveVersion = saveData.saveVersion
                    };
                }
            }
            catch (Exception e)
            {
                LogError($"Error reading metadata for slot {slotIndex}: {e.Message}");
            }

            return null;
        }

        /// <summary>
        /// Get all save metadata
        /// </summary>
        public List<SaveMetadata> GetAllSaveMetadata()
        {
            List<SaveMetadata> allMetadata = new List<SaveMetadata>();

            for (int i = 0; i < numberOfSaveSlots; i++)
            {
                SaveMetadata metadata = GetSaveMetadata(i);
                if (metadata != null)
                {
                    allMetadata.Add(metadata);
                }
            }

            return allMetadata;
        }

        // ========== PRIVATE METHODS ==========

        /// <summary>
        /// Capture current game state into SaveData
        /// </summary>
        private SaveData CaptureGameState()
        {
            SaveData data = new SaveData();

            // Capture player data
            if (PlayerController.Instance != null)
            {
                data.playerData.position = new Vector3Serializable(PlayerController.Instance.transform.position);
                data.playerData.rotation = new QuaternionSerializable(PlayerController.Instance.transform.rotation);
            }

            if (PlayerStats.Instance != null)
            {
                data.playerData.currentHealth = PlayerStats.Instance.currentHealth;
                data.playerData.maxHealth = PlayerStats.Instance.maxHealth;
                data.playerData.currentMana = PlayerStats.Instance.currentMana;
                data.playerData.maxMana = PlayerStats.Instance.maxMana;
                data.playerData.currentStamina = PlayerStats.Instance.currentStamina;
                data.playerData.maxStamina = PlayerStats.Instance.maxStamina;
                data.playerData.level = PlayerStats.Instance.level;
                data.playerData.currentExp = PlayerStats.Instance.currentExp;
                data.playerData.expToNextLevel = PlayerStats.Instance.expToNextLevel;
            }

            // Capture quest data
            if (QuestManager.Instance != null)
            {
                data.questData = QuestManager.Instance.GetSaveData();
            }

            // Capture riddle data
            if (RiddleManager.Instance != null)
            {
                data.riddleData = RiddleManager.Instance.GetSaveData();
            }

            // Capture plant data
            // TODO: Implement when PlantManager is created
            // data.plantData = PlantManager.Instance.GetSaveData();

            // Capture inventory data
            if (Inventory.InventorySystem.Instance != null)
            {
                data.inventoryData.items = Inventory.InventorySystem.Instance.GetSaveData();
            }

            // Capture equipment data
            if (Inventory.EquipmentSystem.Instance != null)
            {
                data.inventoryData.equippedItems = Inventory.EquipmentSystem.Instance.GetSaveData();
            }

            // Capture world data
            data.worldData.gameTime = GetCurrentPlaytime();
            if (GameManager.Instance != null)
            {
                data.worldData.currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }

            return data;
        }

        /// <summary>
        /// Apply loaded SaveData to game state
        /// </summary>
        private void ApplyGameState(SaveData data)
        {
            // Apply player data
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.Teleport(
                    data.playerData.position.ToVector3(),
                    data.playerData.rotation.ToQuaternion()
                );
            }

            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.currentHealth = data.playerData.currentHealth;
                PlayerStats.Instance.maxHealth = data.playerData.maxHealth;
                PlayerStats.Instance.currentMana = data.playerData.currentMana;
                PlayerStats.Instance.maxMana = data.playerData.maxMana;
                PlayerStats.Instance.currentStamina = data.playerData.currentStamina;
                PlayerStats.Instance.maxStamina = data.playerData.maxStamina;
                PlayerStats.Instance.level = data.playerData.level;
                PlayerStats.Instance.currentExp = data.playerData.currentExp;
                PlayerStats.Instance.expToNextLevel = data.playerData.expToNextLevel;

                // Trigger UI updates
                PlayerStats.Instance.OnHealthChanged?.Invoke(data.playerData.currentHealth, data.playerData.maxHealth);
                PlayerStats.Instance.OnManaChanged?.Invoke(data.playerData.currentMana, data.playerData.maxMana);
                PlayerStats.Instance.OnStaminaChanged?.Invoke(data.playerData.currentStamina, data.playerData.maxStamina);
                PlayerStats.Instance.OnExpChanged?.Invoke(data.playerData.currentExp, data.playerData.expToNextLevel);
            }

            // Apply quest data
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.LoadSaveData(data.questData);
            }

            // Apply riddle data
            if (RiddleManager.Instance != null)
            {
                RiddleManager.Instance.LoadSaveData(data.riddleData);
            }

            // Apply plant data
            // TODO: Implement when PlantManager is created

            // Apply inventory data
            if (Inventory.InventorySystem.Instance != null)
            {
                Inventory.InventorySystem.Instance.LoadSaveData(data.inventoryData.items);
            }

            // Apply equipment data
            if (Inventory.EquipmentSystem.Instance != null)
            {
                Inventory.EquipmentSystem.Instance.LoadSaveData(data.inventoryData.equippedItems);
            }

            Log("Game state applied from save data");
        }

        /// <summary>
        /// Write save file to disk
        /// </summary>
        private bool WriteSaveFile(string filePath, SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);

                if (encryptSaves)
                {
                    json = EncryptString(json);
                }

                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception e)
            {
                LogError($"Write error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Read save file from disk
        /// </summary>
        private SaveData ReadSaveFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);

                if (encryptSaves)
                {
                    json = DecryptString(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);
                return data;
            }
            catch (Exception e)
            {
                LogError($"Read error: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create backup of save file
        /// </summary>
        private void CreateBackup(int slotIndex)
        {
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                string backupPath = GetBackupFilePath(slotIndex);

                if (File.Exists(filePath))
                {
                    File.Copy(filePath, backupPath, true);
                    Log($"Created backup for slot {slotIndex}");
                }
            }
            catch (Exception e)
            {
                LogError($"Backup error: {e.Message}");
            }
        }

        /// <summary>
        /// Get save file path for slot
        /// </summary>
        private string GetSaveFilePath(int slotIndex)
        {
            string extension = saveFormat == SaveFormat.JSON ? ".json" : ".sav";
            return Path.Combine(SaveDirectory, $"save_{slotIndex}{extension}");
        }

        /// <summary>
        /// Get backup file path for slot
        /// </summary>
        private string GetBackupFilePath(int slotIndex)
        {
            string extension = saveFormat == SaveFormat.JSON ? ".json" : ".sav";
            return Path.Combine(SaveDirectory, $"save_{slotIndex}_backup{extension}");
        }

        /// <summary>
        /// Get current playtime including this session
        /// </summary>
        private float GetCurrentPlaytime()
        {
            float sessionTime = Time.realtimeSinceStartup - sessionStartTime;
            float previousTime = currentSaveData != null ? currentSaveData.totalPlayTime : 0f;
            return previousTime + sessionTime;
        }

        /// <summary>
        /// Simple encryption (XOR cipher - for basic obfuscation)
        /// </summary>
        private string EncryptString(string text)
        {
            // Simple XOR encryption with key
            string key = "CozyGameEncryptionKey2024";
            char[] chars = text.ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] ^ key[i % key.Length]);
            }

            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(chars));
        }

        /// <summary>
        /// Simple decryption (XOR cipher)
        /// </summary>
        private string DecryptString(string encryptedText)
        {
            string key = "CozyGameEncryptionKey2024";
            byte[] bytes = Convert.FromBase64String(encryptedText);
            char[] chars = System.Text.Encoding.UTF8.GetString(bytes).ToCharArray();

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] ^ key[i % key.Length]);
            }

            return new string(chars);
        }

        /// <summary>
        /// Log message
        /// </summary>
        private void Log(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[SaveSystem] {message}");
            }
        }

        /// <summary>
        /// Log error
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[SaveSystem] {message}");
        }

        /// <summary>
        /// Get current save slot
        /// </summary>
        public int GetCurrentSaveSlot()
        {
            return currentSaveSlot;
        }

        /// <summary>
        /// Check if a save is currently loaded
        /// </summary>
        public bool IsSaveLoaded()
        {
            return currentSaveSlot >= 0;
        }
    }

    /// <summary>
    /// Save format options
    /// </summary>
    public enum SaveFormat
    {
        JSON,   // Human-readable, easy to debug
        Binary  // More compact, harder to edit
    }

    /// <summary>
    /// Save metadata for UI display
    /// </summary>
    public class SaveMetadata
    {
        public int slotIndex;
        public string saveName;
        public DateTime saveDateTime;
        public float totalPlayTime;
        public int playerLevel;
        public string saveVersion;

        public string GetPlaytimeFormatted()
        {
            int hours = Mathf.FloorToInt(totalPlayTime / 3600f);
            int minutes = Mathf.FloorToInt((totalPlayTime % 3600f) / 60f);
            return $"{hours}h {minutes}m";
        }

        public string GetDateTimeFormatted()
        {
            return saveDateTime.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
