using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Achievements
{
    /// <summary>
    /// Statistics tracker singleton.
    /// Tracks player statistics for achievements and analytics.
    /// </summary>
    public class StatisticsTracker : MonoBehaviour
    {
        public static StatisticsTracker Instance { get; private set; }

        [Header("Events")]
        public UnityEvent<string, float> OnStatisticChanged; // statKey, newValue

        // Statistics storage
        private Dictionary<string, float> statistics = new Dictionary<string, float>();

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
        /// Initialize statistics tracker
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[StatisticsTracker] Initialized");
        }

        /// <summary>
        /// Set statistic value
        /// </summary>
        public void SetStatistic(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            statistics[key] = value;
            OnStatisticChanged?.Invoke(key, value);
        }

        /// <summary>
        /// Increment statistic
        /// </summary>
        public void IncrementStatistic(string key, float amount = 1f)
        {
            if (string.IsNullOrEmpty(key))
                return;

            float currentValue = GetStatistic(key);
            SetStatistic(key, currentValue + amount);
        }

        /// <summary>
        /// Get statistic value
        /// </summary>
        public float GetStatistic(string key)
        {
            if (string.IsNullOrEmpty(key))
                return 0f;

            if (statistics.TryGetValue(key, out float value))
            {
                return value;
            }

            return 0f;
        }

        /// <summary>
        /// Check if statistic exists
        /// </summary>
        public bool HasStatistic(string key)
        {
            return statistics.ContainsKey(key);
        }

        /// <summary>
        /// Get all statistics
        /// </summary>
        public Dictionary<string, float> GetAllStatistics()
        {
            return new Dictionary<string, float>(statistics);
        }

        /// <summary>
        /// Clear all statistics
        /// </summary>
        public void ClearAllStatistics()
        {
            statistics.Clear();
            Debug.Log("[StatisticsTracker] All statistics cleared");
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public Dictionary<string, float> GetSaveData()
        {
            return new Dictionary<string, float>(statistics);
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(Dictionary<string, float> saveData)
        {
            statistics.Clear();

            if (saveData != null)
            {
                foreach (var kvp in saveData)
                {
                    statistics[kvp.Key] = kvp.Value;
                }
            }

            Debug.Log($"[StatisticsTracker] Loaded {statistics.Count} statistics");
        }

        // ========== COMMON STATISTICS HELPERS ==========

        /// <summary>
        /// Track enemy killed
        /// </summary>
        public void TrackEnemyKilled(string enemyType = "")
        {
            IncrementStatistic("enemies_killed");

            if (!string.IsNullOrEmpty(enemyType))
            {
                IncrementStatistic($"enemies_killed_{enemyType}");
            }
        }

        /// <summary>
        /// Track damage dealt
        /// </summary>
        public void TrackDamageDealt(float damage)
        {
            IncrementStatistic("total_damage_dealt", damage);
        }

        /// <summary>
        /// Track damage taken
        /// </summary>
        public void TrackDamageTaken(float damage)
        {
            IncrementStatistic("total_damage_taken", damage);
        }

        /// <summary>
        /// Track item collected
        /// </summary>
        public void TrackItemCollected(string itemID)
        {
            IncrementStatistic("items_collected");
            IncrementStatistic($"items_collected_{itemID}");
        }

        /// <summary>
        /// Track quest completed
        /// </summary>
        public void TrackQuestCompleted(string questID)
        {
            IncrementStatistic("quests_completed");
            IncrementStatistic($"quest_completed_{questID}");
        }

        /// <summary>
        /// Track riddle solved
        /// </summary>
        public void TrackRiddleSolved(string riddleID)
        {
            IncrementStatistic("riddles_solved");
            IncrementStatistic($"riddle_solved_{riddleID}");
        }

        /// <summary>
        /// Track distance traveled
        /// </summary>
        public void TrackDistanceTraveled(float distance)
        {
            IncrementStatistic("distance_traveled", distance);
        }

        /// <summary>
        /// Track playtime
        /// </summary>
        public void TrackPlaytime(float seconds)
        {
            IncrementStatistic("total_playtime", seconds);
        }

        /// <summary>
        /// Track death
        /// </summary>
        public void TrackDeath()
        {
            IncrementStatistic("deaths");
        }

        /// <summary>
        /// Track level up
        /// </summary>
        public void TrackLevelUp(int newLevel)
        {
            SetStatistic("current_level", newLevel);

            if (newLevel > GetStatistic("max_level_reached"))
            {
                SetStatistic("max_level_reached", newLevel);
            }
        }

        /// <summary>
        /// Track spell cast
        /// </summary>
        public void TrackSpellCast(string spellID)
        {
            IncrementStatistic("spells_cast");
            IncrementStatistic($"spell_cast_{spellID}");
        }

        /// <summary>
        /// Track plant grown
        /// </summary>
        public void TrackPlantGrown(string plantSpecies)
        {
            IncrementStatistic("plants_grown");
            IncrementStatistic($"plant_grown_{plantSpecies}");
        }
    }
}
