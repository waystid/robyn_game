using System;
using System.Collections.Generic;
using UnityEngine;

namespace CozyGame.SaveSystem
{
    /// <summary>
    /// Complete save data structure containing all game state.
    /// Serializable to JSON for easy debugging and human readability.
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Header("Save Metadata")]
        public string saveName = "Save Slot 1";
        public string saveGUID = "";
        public DateTime saveDateTime;
        public string saveVersion = "1.0.0";
        public float totalPlayTime = 0f;

        [Header("Player Data")]
        public PlayerSaveData playerData;

        [Header("Quest Data")]
        public List<QuestSaveData> questData = new List<QuestSaveData>();

        [Header("Riddle Data")]
        public List<RiddleSaveData> riddleData = new List<RiddleSaveData>();

        [Header("Plant Data")]
        public List<PlantSaveData> plantData = new List<PlantSaveData>();

        [Header("World Data")]
        public WorldSaveData worldData;

        [Header("Inventory Data (Future)")]
        public InventorySaveData inventoryData;

        /// <summary>
        /// Create new save data with default values
        /// </summary>
        public SaveData()
        {
            saveGUID = System.Guid.NewGuid().ToString();
            saveDateTime = DateTime.Now;
            playerData = new PlayerSaveData();
            worldData = new WorldSaveData();
            inventoryData = new InventorySaveData();
        }

        /// <summary>
        /// Update save metadata before saving
        /// </summary>
        public void UpdateMetadata(string slotName, float playtime)
        {
            saveName = slotName;
            saveDateTime = DateTime.Now;
            totalPlayTime = playtime;
        }
    }

    /// <summary>
    /// Player-specific save data
    /// </summary>
    [System.Serializable]
    public class PlayerSaveData
    {
        // Position and Rotation
        public Vector3Serializable position = new Vector3Serializable(0, 0, 0);
        public QuaternionSerializable rotation = new QuaternionSerializable(0, 0, 0, 1);

        // Stats
        public float currentHealth = 100f;
        public float maxHealth = 100f;
        public float currentMana = 100f;
        public float maxMana = 100f;
        public float currentStamina = 100f;
        public float maxStamina = 100f;

        // Level and Experience
        public int level = 1;
        public int currentExp = 0;
        public int expToNextLevel = 100;

        // Unlocks
        public List<string> unlockedSpells = new List<string>();
        public List<string> discoveredPlants = new List<string>();
    }

    /// <summary>
    /// Quest save data
    /// </summary>
    [System.Serializable]
    public class QuestSaveData
    {
        public string questID;
        public string questState; // NotStarted, Active, Completed, Failed
        public DateTime lastCompletedTime;
        public int timesCompleted = 0;

        public QuestSaveData(string id, string state)
        {
            questID = id;
            questState = state;
        }
    }

    /// <summary>
    /// Riddle save data
    /// </summary>
    [System.Serializable]
    public class RiddleSaveData
    {
        public string riddleID;
        public bool isAnswered = false;
        public bool wasCorrect = false;
        public DateTime lastAttemptTime;
        public int attemptCount = 0;

        public RiddleSaveData(string id)
        {
            riddleID = id;
        }
    }

    /// <summary>
    /// Plant save data
    /// </summary>
    [System.Serializable]
    public class PlantSaveData
    {
        public string plantID;
        public string plantInstanceID; // Unique ID for this plant instance
        public Vector3Serializable position;
        public int currentStage = 0;
        public float currentStageTime = 0f;
        public int timesWatered = 0;
        public DateTime plantedTime;
        public bool isGrowing = true;

        public PlantSaveData(string id, string instanceID, Vector3 pos)
        {
            plantID = id;
            plantInstanceID = instanceID;
            position = new Vector3Serializable(pos);
            plantedTime = DateTime.Now;
        }
    }

    /// <summary>
    /// World/Game state save data
    /// </summary>
    [System.Serializable]
    public class WorldSaveData
    {
        public float gameTime = 0f; // Total game time in seconds
        public float timeOfDay = 12f; // Hour of day (0-24)
        public int daysPassed = 0;
        public string currentWeather = "Clear";
        public string currentScene = "Gameplay";
    }

    /// <summary>
    /// Inventory save data (for future inventory system)
    /// </summary>
    [System.Serializable]
    public class InventorySaveData
    {
        public List<ItemSaveData> items = new List<ItemSaveData>();
        public int currency = 0;
    }

    /// <summary>
    /// Individual item save data
    /// </summary>
    [System.Serializable]
    public class ItemSaveData
    {
        public string itemID;
        public int quantity = 1;
        public int slotIndex = 0;

        public ItemSaveData(string id, int qty, int slot)
        {
            itemID = id;
            quantity = qty;
            slotIndex = slot;
        }
    }

    // ========== SERIALIZATION HELPERS ==========

    /// <summary>
    /// Serializable Vector3 (Unity's Vector3 is not directly JSON serializable)
    /// </summary>
    [System.Serializable]
    public class Vector3Serializable
    {
        public float x, y, z;

        public Vector3Serializable(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Serializable(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// Serializable Quaternion
    /// </summary>
    [System.Serializable]
    public class QuaternionSerializable
    {
        public float x, y, z, w;

        public QuaternionSerializable(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public QuaternionSerializable(Quaternion q)
        {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }
}
