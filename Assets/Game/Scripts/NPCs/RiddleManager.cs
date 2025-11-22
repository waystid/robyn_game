using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame
{
    /// <summary>
    /// Manages riddle system and tracks answered riddles
    /// Singleton pattern for easy access
    /// Attach to a GameObject in your main scene
    /// </summary>
    public class RiddleManager : MonoBehaviour
    {
        public static RiddleManager Instance { get; private set; }

        [Header("All Riddles")]
        [Tooltip("Assign all riddle data assets here")]
        public List<RiddleData> allRiddles = new List<RiddleData>();

        [Header("Settings")]
        [Tooltip("Should riddles be selected randomly or in order?")]
        public bool randomizeRiddles = true;

        [Tooltip("Filter by difficulty (leave empty for all difficulties)")]
        public List<RiddleDifficulty> allowedDifficulties = new List<RiddleDifficulty>();

        [Header("Events")]
        public delegate void RiddleEvent(RiddleData riddle, bool correct);
        public event RiddleEvent OnRiddleAnswered;

        public delegate void RiddleStartEvent(RiddleData riddle);
        public event RiddleStartEvent OnRiddleStarted;

        [Header("Debug")]
        public bool showDebugLogs = true;

        [Header("Current Riddle")]
        [SerializeField] private RiddleData currentRiddle;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Get a random available riddle
        /// </summary>
        public RiddleData GetRandomAvailableRiddle()
        {
            var available = GetAvailableRiddles();

            if (available.Count == 0)
            {
                LogWarning("No riddles available (all on cooldown or answered)");
                return null;
            }

            int randomIndex = Random.Range(0, available.Count);
            currentRiddle = available[randomIndex];

            OnRiddleStarted?.Invoke(currentRiddle);
            Log($"Selected riddle: {currentRiddle.question.Substring(0, Mathf.Min(30, currentRiddle.question.Length))}...");

            return currentRiddle;
        }

        /// <summary>
        /// Get next available riddle (in order)
        /// </summary>
        public RiddleData GetNextAvailableRiddle()
        {
            var available = GetAvailableRiddles();

            if (available.Count == 0)
            {
                LogWarning("No riddles available");
                return null;
            }

            currentRiddle = available[0];
            OnRiddleStarted?.Invoke(currentRiddle);

            return currentRiddle;
        }

        /// <summary>
        /// Get riddle by specific difficulty
        /// </summary>
        public RiddleData GetRiddleByDifficulty(RiddleDifficulty difficulty)
        {
            var available = GetAvailableRiddles()
                .Where(r => r.difficulty == difficulty)
                .ToList();

            if (available.Count == 0)
            {
                LogWarning($"No {difficulty} riddles available");
                return null;
            }

            int randomIndex = Random.Range(0, available.Count);
            currentRiddle = available[randomIndex];
            OnRiddleStarted?.Invoke(currentRiddle);

            return currentRiddle;
        }

        /// <summary>
        /// Get all available riddles (not on cooldown)
        /// </summary>
        public List<RiddleData> GetAvailableRiddles()
        {
            var available = allRiddles.Where(r => r.IsAvailable()).ToList();

            // Filter by allowed difficulties if specified
            if (allowedDifficulties != null && allowedDifficulties.Count > 0)
            {
                available = available.Where(r => allowedDifficulties.Contains(r.difficulty)).ToList();
            }

            return available;
        }

        /// <summary>
        /// Answer a riddle
        /// </summary>
        /// <param name="riddle">The riddle being answered</param>
        /// <param name="answerIndex">The index of the selected answer</param>
        /// <returns>True if answer was correct, false otherwise</returns>
        public bool AnswerRiddle(RiddleData riddle, int answerIndex)
        {
            if (riddle == null)
            {
                LogWarning("Cannot answer null riddle!");
                return false;
            }

            bool correct = (answerIndex == riddle.correctAnswerIndex);

            Log($"Riddle answered: {(correct ? "CORRECT" : "WRONG")}");

            if (correct)
            {
                // Mark as answered
                riddle.MarkAsAnswered(true);

                // Grant rewards
                GrantRewards(riddle);

                // Play success sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("riddle_correct");
                }
            }
            else
            {
                // Still mark as answered but don't grant rewards
                riddle.MarkAsAnswered(false);

                // Play failure sound
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("riddle_wrong");
                }
            }

            // Trigger event
            OnRiddleAnswered?.Invoke(riddle, correct);

            // Clear current riddle
            if (currentRiddle == riddle)
            {
                currentRiddle = null;
            }

            return correct;
        }

        /// <summary>
        /// Grant riddle rewards to player
        /// Override this or use events to integrate with your inventory system
        /// </summary>
        protected virtual void GrantRewards(RiddleData riddle)
        {
            // TODO: Integrate with Survival Engine
            foreach (var reward in riddle.rewards)
            {
                Log($"Granted reward: {reward.quantity}x {reward.itemName}");

                // Example integration with Survival Engine:
                // var itemData = ItemData.Get(reward.itemName);
                // if (itemData != null)
                // {
                //     PlayerData.Get().inventory.AddItem(itemData, reward.quantity);
                // }
            }

            // Show reward notification
            ShowRewardNotification(riddle);
        }

        /// <summary>
        /// Show reward notification
        /// Override to integrate with your UI system
        /// </summary>
        protected virtual void ShowRewardNotification(RiddleData riddle)
        {
            string rewardText = "Rewards: ";
            foreach (var reward in riddle.rewards)
            {
                rewardText += $"{reward.quantity}x {reward.itemName}, ";
            }
            Log($"[NOTIFICATION] {rewardText}");
        }

        /// <summary>
        /// Get count of available riddles
        /// </summary>
        public int GetAvailableRiddleCount()
        {
            return GetAvailableRiddles().Count;
        }

        /// <summary>
        /// Get count of answered riddles
        /// </summary>
        public int GetAnsweredRiddleCount()
        {
            return allRiddles.Count(r => r.hasBeenAnswered);
        }

        /// <summary>
        /// Get count of correctly answered riddles
        /// </summary>
        public int GetCorrectlyAnsweredCount()
        {
            return allRiddles.Count(r => r.wasAnsweredCorrectly);
        }

        /// <summary>
        /// Get player's accuracy percentage
        /// </summary>
        public float GetAccuracyPercentage()
        {
            int answered = GetAnsweredRiddleCount();
            if (answered == 0) return 0f;

            int correct = GetCorrectlyAnsweredCount();
            return ((float)correct / answered) * 100f;
        }

        /// <summary>
        /// Reset all riddles (useful for testing or new game)
        /// </summary>
        [ContextMenu("Reset All Riddles")]
        public void ResetAllRiddles()
        {
            foreach (var riddle in allRiddles)
            {
                riddle.ResetRiddle();
            }

            currentRiddle = null;
            Log("All riddles reset!");
        }

        /// <summary>
        /// Get current riddle
        /// </summary>
        public RiddleData GetCurrentRiddle()
        {
            return currentRiddle;
        }

        /// <summary>
        /// Check if there's a riddle in progress
        /// </summary>
        public bool HasActiveRiddle()
        {
            return currentRiddle != null;
        }

        // Helper logging methods
        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[RiddleManager] {message}");
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[RiddleManager] {message}");
        }

        /// <summary>
        /// Get statistics for UI display
        /// </summary>
        public RiddleStats GetStats()
        {
            return new RiddleStats
            {
                totalRiddles = allRiddles.Count,
                answeredRiddles = GetAnsweredRiddleCount(),
                correctAnswers = GetCorrectlyAnsweredCount(),
                availableRiddles = GetAvailableRiddleCount(),
                accuracyPercentage = GetAccuracyPercentage()
            };
        }

        // ========== SAVE SYSTEM INTEGRATION ==========

        /// <summary>
        /// Get save data for all riddles
        /// </summary>
        public List<SaveSystem.RiddleSaveData> GetSaveData()
        {
            List<SaveSystem.RiddleSaveData> saveData = new List<SaveSystem.RiddleSaveData>();

            foreach (var riddle in allRiddles)
            {
                var riddleSave = new SaveSystem.RiddleSaveData(riddle.riddleID);
                riddleSave.isAnswered = riddle.hasBeenAnswered;
                riddleSave.wasCorrect = riddle.wasAnsweredCorrectly;
                riddleSave.lastAttemptTime = riddle.lastAttemptTime;
                riddleSave.attemptCount = riddle.totalAttempts;
                saveData.Add(riddleSave);
            }

            return saveData;
        }

        /// <summary>
        /// Load riddle data from save
        /// </summary>
        public void LoadSaveData(List<SaveSystem.RiddleSaveData> saveData)
        {
            // TODO: This requires riddle ScriptableObjects to be loaded/referenced
            // For now, just log the data
            // In full implementation, you'd need a RiddleDatabase to look up riddles by ID

            foreach (var riddleSave in saveData)
            {
                Log($"Loading riddle: {riddleSave.riddleID} - Answered: {riddleSave.isAnswered}, Correct: {riddleSave.wasCorrect}");

                // Example of how you would restore state:
                // RiddleData riddle = RiddleDatabase.GetRiddleByID(riddleSave.riddleID);
                // if (riddle != null)
                // {
                //     riddle.hasBeenAnswered = riddleSave.isAnswered;
                //     riddle.wasAnsweredCorrectly = riddleSave.wasCorrect;
                //     riddle.lastAttemptTime = riddleSave.lastAttemptTime;
                //     riddle.totalAttempts = riddleSave.attemptCount;
                // }
            }

            Log($"Loaded {saveData.Count} riddle states from save");
        }
    }

    /// <summary>
    /// Statistics structure for riddle system
    /// </summary>
    [System.Serializable]
    public struct RiddleStats
    {
        public int totalRiddles;
        public int answeredRiddles;
        public int correctAnswers;
        public int availableRiddles;
        public float accuracyPercentage;
    }
}
