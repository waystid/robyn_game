using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame.Achievements
{
    /// <summary>
    /// Achievement progress data
    /// </summary>
    [System.Serializable]
    public class AchievementProgress
    {
        public string achievementID;
        public bool isUnlocked;
        public int currentProgress;
        public System.DateTime unlockTime;

        public AchievementProgress()
        {
        }

        public AchievementProgress(string id)
        {
            achievementID = id;
            isUnlocked = false;
            currentProgress = 0;
        }
    }

    /// <summary>
    /// Achievement system manager.
    /// Manages achievement unlocking and progress tracking.
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        public static AchievementSystem Instance { get; private set; }

        [Header("Achievement Database")]
        [Tooltip("All achievements in game (auto-load from Resources)")]
        public Achievement[] achievementDatabase;

        [Header("Settings")]
        [Tooltip("Enable debug logs")]
        public bool enableDebugLogs = true;

        [Header("Events")]
        public UnityEvent<Achievement> OnAchievementUnlocked;
        public UnityEvent<Achievement, int> OnAchievementProgress; // achievement, progress

        // Achievement data
        private Dictionary<string, Achievement> achievementLookup = new Dictionary<string, Achievement>();
        private Dictionary<string, AchievementProgress> achievementProgress = new Dictionary<string, AchievementProgress>();

        // Statistics
        private int totalAchievements = 0;
        private int unlockedAchievements = 0;
        private int totalPoints = 0;

        private void Awake()
        {
            // Singleton setup with DontDestroyOnLoad
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
        /// Initialize achievement system
        /// </summary>
        private void Initialize()
        {
            // Load all achievements from Resources
            LoadAchievementDatabase();

            // Subscribe to statistics changes
            if (StatisticsTracker.Instance != null)
            {
                StatisticsTracker.Instance.OnStatisticChanged.AddListener(OnStatisticChanged);
            }

            Log("AchievementSystem initialized");
        }

        /// <summary>
        /// Load all achievements from Resources folder
        /// </summary>
        private void LoadAchievementDatabase()
        {
            achievementLookup.Clear();

            // Load from assigned database
            if (achievementDatabase != null && achievementDatabase.Length > 0)
            {
                foreach (var achievement in achievementDatabase)
                {
                    if (achievement != null && !string.IsNullOrEmpty(achievement.achievementID))
                    {
                        achievementLookup[achievement.achievementID] = achievement;
                    }
                }
            }

            // Also try to load from Resources
            Achievement[] resourceAchievements = Resources.LoadAll<Achievement>("Achievements");
            foreach (var achievement in resourceAchievements)
            {
                if (achievement != null && !string.IsNullOrEmpty(achievement.achievementID))
                {
                    if (!achievementLookup.ContainsKey(achievement.achievementID))
                    {
                        achievementLookup[achievement.achievementID] = achievement;
                    }
                }
            }

            totalAchievements = achievementLookup.Count;

            // Initialize progress for all achievements
            foreach (var achievement in achievementLookup.Values)
            {
                if (!achievementProgress.ContainsKey(achievement.achievementID))
                {
                    achievementProgress[achievement.achievementID] = new AchievementProgress(achievement.achievementID);
                }
            }

            Log($"Loaded {achievementLookup.Count} achievements");
        }

        /// <summary>
        /// Statistic changed callback
        /// </summary>
        private void OnStatisticChanged(string statKey, float newValue)
        {
            // Check all achievements that track this statistic
            foreach (var achievement in achievementLookup.Values)
            {
                if (achievement.statisticKey == statKey)
                {
                    CheckAchievementProgress(achievement, newValue);
                }
            }
        }

        /// <summary>
        /// Check achievement progress
        /// </summary>
        private void CheckAchievementProgress(Achievement achievement, float currentValue)
        {
            if (IsAchievementUnlocked(achievement.achievementID))
                return;

            AchievementProgress progress = GetProgress(achievement.achievementID);
            if (progress == null)
                return;

            // Check prerequisites
            if (!CheckPrerequisites(achievement))
                return;

            // Update progress
            int newProgress = Mathf.FloorToInt(currentValue);
            bool progressChanged = (newProgress != progress.currentProgress);

            progress.currentProgress = newProgress;

            if (progressChanged)
            {
                OnAchievementProgress?.Invoke(achievement, newProgress);
            }

            // Check if achievement should unlock
            if (currentValue >= achievement.requiredValue)
            {
                UnlockAchievement(achievement.achievementID);
            }
        }

        /// <summary>
        /// Check if prerequisites are met
        /// </summary>
        private bool CheckPrerequisites(Achievement achievement)
        {
            if (achievement.prerequisiteAchievements == null || achievement.prerequisiteAchievements.Length == 0)
                return true;

            foreach (var prerequisite in achievement.prerequisiteAchievements)
            {
                if (prerequisite != null && !IsAchievementUnlocked(prerequisite.achievementID))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Unlock achievement
        /// </summary>
        public bool UnlockAchievement(string achievementID)
        {
            if (string.IsNullOrEmpty(achievementID))
                return false;

            if (IsAchievementUnlocked(achievementID))
                return false; // Already unlocked

            Achievement achievement = GetAchievement(achievementID);
            if (achievement == null)
            {
                LogWarning($"Achievement not found: {achievementID}");
                return false;
            }

            AchievementProgress progress = GetProgress(achievementID);
            if (progress == null)
                return false;

            // Mark as unlocked
            progress.isUnlocked = true;
            progress.unlockTime = System.DateTime.Now;
            progress.currentProgress = achievement.requiredProgress;

            unlockedAchievements++;

            // Grant rewards
            GrantRewards(achievement);

            // Fire event
            OnAchievementUnlocked?.Invoke(achievement);

            Log($"Achievement unlocked: {achievement.achievementName}");
            return true;
        }

        /// <summary>
        /// Grant achievement rewards
        /// </summary>
        private void GrantRewards(Achievement achievement)
        {
            // Grant points
            if (achievement.rewardPoints > 0)
            {
                totalPoints += achievement.rewardPoints;
            }

            // Unlock spell
            if (!string.IsNullOrEmpty(achievement.unlockSpellID) && PlayerStats.Instance != null)
            {
                PlayerStats.Instance.UnlockSpell(achievement.unlockSpellID);
                Log($"Unlocked spell: {achievement.unlockSpellID}");
            }

            // Grant item
            if (!string.IsNullOrEmpty(achievement.grantItemID) && Inventory.InventorySystem.Instance != null)
            {
                Inventory.InventorySystem.Instance.AddItem(achievement.grantItemID, achievement.grantItemQuantity);
                Log($"Granted item: {achievement.grantItemQuantity}x {achievement.grantItemID}");
            }
        }

        /// <summary>
        /// Manually trigger achievement check
        /// </summary>
        public void TriggerAchievement(string achievementID)
        {
            Achievement achievement = GetAchievement(achievementID);
            if (achievement == null)
                return;

            if (!string.IsNullOrEmpty(achievement.statisticKey) && StatisticsTracker.Instance != null)
            {
                float currentValue = StatisticsTracker.Instance.GetStatistic(achievement.statisticKey);
                CheckAchievementProgress(achievement, currentValue);
            }
            else
            {
                // OneTime achievement, unlock directly
                UnlockAchievement(achievementID);
            }
        }

        /// <summary>
        /// Get achievement by ID
        /// </summary>
        public Achievement GetAchievement(string achievementID)
        {
            if (achievementLookup.TryGetValue(achievementID, out Achievement achievement))
            {
                return achievement;
            }

            return null;
        }

        /// <summary>
        /// Get achievement progress
        /// </summary>
        public AchievementProgress GetProgress(string achievementID)
        {
            if (achievementProgress.TryGetValue(achievementID, out AchievementProgress progress))
            {
                return progress;
            }

            return null;
        }

        /// <summary>
        /// Check if achievement is unlocked
        /// </summary>
        public bool IsAchievementUnlocked(string achievementID)
        {
            AchievementProgress progress = GetProgress(achievementID);
            return progress != null && progress.isUnlocked;
        }

        /// <summary>
        /// Get all achievements
        /// </summary>
        public List<Achievement> GetAllAchievements()
        {
            return achievementLookup.Values.ToList();
        }

        /// <summary>
        /// Get achievements by category
        /// </summary>
        public List<Achievement> GetAchievementsByCategory(string category)
        {
            return achievementLookup.Values
                .Where(a => a.category == category)
                .OrderBy(a => a.sortOrder)
                .ToList();
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        public List<string> GetAllCategories()
        {
            return achievementLookup.Values
                .Select(a => a.category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

        /// <summary>
        /// Get unlocked achievements
        /// </summary>
        public List<Achievement> GetUnlockedAchievements()
        {
            return achievementLookup.Values
                .Where(a => IsAchievementUnlocked(a.achievementID))
                .ToList();
        }

        /// <summary>
        /// Get unlock percentage (0-100)
        /// </summary>
        public float GetUnlockPercentage()
        {
            if (totalAchievements == 0)
                return 0f;

            return (unlockedAchievements / (float)totalAchievements) * 100f;
        }

        /// <summary>
        /// Get total achievement points
        /// </summary>
        public int GetTotalPoints()
        {
            return totalPoints;
        }

        /// <summary>
        /// Get achievement counts
        /// </summary>
        public (int unlocked, int total) GetAchievementCounts()
        {
            return (unlockedAchievements, totalAchievements);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public List<AchievementProgress> GetSaveData()
        {
            return achievementProgress.Values.ToList();
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(List<AchievementProgress> saveData)
        {
            if (saveData == null)
                return;

            unlockedAchievements = 0;
            totalPoints = 0;

            foreach (var progress in saveData)
            {
                if (achievementProgress.ContainsKey(progress.achievementID))
                {
                    achievementProgress[progress.achievementID] = progress;

                    if (progress.isUnlocked)
                    {
                        unlockedAchievements++;

                        Achievement achievement = GetAchievement(progress.achievementID);
                        if (achievement != null)
                        {
                            totalPoints += achievement.rewardPoints;
                        }
                    }
                }
            }

            Log($"Loaded {saveData.Count} achievement progress entries");
        }

        /// <summary>
        /// Debug log
        /// </summary>
        private void Log(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AchievementSystem] {message}");
            }
        }

        /// <summary>
        /// Debug warning
        /// </summary>
        private void LogWarning(string message)
        {
            if (enableDebugLogs)
            {
                Debug.LogWarning($"[AchievementSystem] {message}");
            }
        }
    }
}
