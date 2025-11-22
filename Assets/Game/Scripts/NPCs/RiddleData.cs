using UnityEngine;
using System.Collections.Generic;

namespace CozyGame
{
    /// <summary>
    /// Defines a riddle with question, answers, and rewards
    /// Create instances via: Right-click → Create → Cozy Game → Riddle Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Riddle", menuName = "Cozy Game/Riddle Data", order = 2)]
    public class RiddleData : ScriptableObject
    {
        [Header("Riddle")]
        [Tooltip("The riddle question")]
        [TextArea(2, 4)]
        public string question = "What is the answer to everything?";

        [Tooltip("Category/difficulty for organization")]
        public RiddleDifficulty difficulty = RiddleDifficulty.Easy;

        [Header("Answers")]
        [Tooltip("All possible answers (first one should be correct, will be shuffled in game)")]
        public List<string> answers = new List<string> { "Correct Answer", "Wrong 1", "Wrong 2", "Wrong 3" };

        [Tooltip("Index of the correct answer (0 = first answer)")]
        [Range(0, 3)]
        public int correctAnswerIndex = 0;

        [Header("Feedback")]
        [Tooltip("Message shown when player answers correctly")]
        public string correctFeedback = "Correct! Well done!";

        [Tooltip("Message shown when player answers incorrectly")]
        public string wrongFeedback = "Not quite. Think about it...";

        [Tooltip("Hint given after wrong answer")]
        [TextArea(2, 3)]
        public string hintText = "Here's a hint...";

        [Header("Rewards")]
        [Tooltip("Rewards given for correct answer")]
        public List<RiddleReward> rewards = new List<RiddleReward>();

        [Header("Cooldown")]
        [Tooltip("Time before this specific riddle can be asked again (in minutes)")]
        public float cooldownMinutes = 60f;

        [Tooltip("Can this riddle be asked multiple times?")]
        public bool isRepeatable = true;

        [Header("State (Runtime)")]
        [HideInInspector]
        public bool hasBeenAnswered = false;

        [HideInInspector]
        public bool wasAnsweredCorrectly = false;

        [HideInInspector]
        public float lastAnsweredTime = 0f;

        [HideInInspector]
        public int timesAnswered = 0;

        /// <summary>
        /// Check if this riddle is available (cooldown passed)
        /// </summary>
        public bool IsAvailable()
        {
            // If never answered, it's available
            if (!hasBeenAnswered) return true;

            // If not repeatable and already answered, not available
            if (!isRepeatable && hasBeenAnswered) return false;

            // Check cooldown
            float timeSinceAnswered = Time.time - lastAnsweredTime;
            float cooldownSeconds = cooldownMinutes * 60f;
            return timeSinceAnswered >= cooldownSeconds;
        }

        /// <summary>
        /// Get remaining cooldown time in minutes
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (!hasBeenAnswered) return 0f;

            float timeSinceAnswered = Time.time - lastAnsweredTime;
            float cooldownSeconds = cooldownMinutes * 60f;
            float remaining = cooldownSeconds - timeSinceAnswered;
            return Mathf.Max(0f, remaining / 60f);
        }

        /// <summary>
        /// Mark riddle as answered
        /// </summary>
        public void MarkAsAnswered(bool correct)
        {
            hasBeenAnswered = true;
            wasAnsweredCorrectly = correct;
            lastAnsweredTime = Time.time;
            timesAnswered++;
        }

        /// <summary>
        /// Reset riddle state (for testing or new game)
        /// </summary>
        public void ResetRiddle()
        {
            hasBeenAnswered = false;
            wasAnsweredCorrectly = false;
            lastAnsweredTime = 0f;
            timesAnswered = 0;
        }

        /// <summary>
        /// Get shuffled answers (keeps correct answer tracked)
        /// Returns list of answers with correct index updated
        /// </summary>
        public (List<string> shuffledAnswers, int correctIndex) GetShuffledAnswers()
        {
            if (answers == null || answers.Count == 0)
            {
                Debug.LogWarning($"Riddle '{question}' has no answers!");
                return (new List<string>(), 0);
            }

            // Create a copy of answers
            List<string> shuffled = new List<string>(answers);
            string correctAnswer = answers[correctAnswerIndex];

            // Simple Fisher-Yates shuffle
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                string temp = shuffled[i];
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }

            // Find new index of correct answer
            int newCorrectIndex = shuffled.IndexOf(correctAnswer);

            return (shuffled, newCorrectIndex);
        }

        /// <summary>
        /// Validate riddle data
        /// </summary>
        private void OnValidate()
        {
            // Ensure we have at least 2 answers
            if (answers.Count < 2)
            {
                Debug.LogWarning($"Riddle '{question}' should have at least 2 answers!");
            }

            // Ensure correct answer index is valid
            if (correctAnswerIndex >= answers.Count)
            {
                correctAnswerIndex = 0;
                Debug.LogWarning($"Riddle '{question}' had invalid correct answer index, reset to 0");
            }

            // Ensure at least one reward
            if (rewards.Count == 0)
            {
                Debug.LogWarning($"Riddle '{question}' has no rewards!");
            }
        }
    }

    /// <summary>
    /// Reward given for solving a riddle
    /// </summary>
    [System.Serializable]
    public class RiddleReward
    {
        [Tooltip("Name/ID of the reward item (must match ItemData)")]
        public string itemName;

        [Tooltip("How many of this item to give")]
        public int quantity = 1;

        [Tooltip("Description for UI (optional)")]
        public string description;
    }

    /// <summary>
    /// Riddle difficulty levels (for organization and balancing)
    /// </summary>
    public enum RiddleDifficulty
    {
        Easy,
        Medium,
        Hard,
        VeryHard
    }
}
