using System.Collections.Generic;
using UnityEngine;
using CozyGame.Dialogue;

namespace CozyGame
{
    /// <summary>
    /// Riddle-giving Sphinx NPC.
    /// Presents riddles to the player and rewards correct answers.
    /// </summary>
    public class SphinxNPC : NPCInteractable
    {
        [Header("Sphinx Riddle Settings")]
        [Tooltip("All riddles this sphinx can ask")]
        public List<RiddleData> availableRiddles = new List<RiddleData>();

        [Tooltip("Dialogue when sphinx has new riddle available")]
        public DialogueData newRiddleDialogue;

        [Tooltip("Dialogue when player has already answered all riddles")]
        public DialogueData allRiddlesCompleteDialogue;

        [Tooltip("Dialogue when riddle is on cooldown")]
        public DialogueData riddleOnCooldownDialogue;

        [Header("Sphinx Personality")]
        [Tooltip("Correct answer sound")]
        public AudioClip correctAnswerSound;

        [Tooltip("Wrong answer sound")]
        public AudioClip wrongAnswerSound;

        [Tooltip("Mysterious greeting sound")]
        public AudioClip greetingSound;

        [Header("Visual Effects")]
        [Tooltip("Particle effect when answer is correct")]
        public ParticleSystem correctParticles;

        [Tooltip("Particle effect when answer is wrong")]
        public ParticleSystem wrongParticles;

        [Tooltip("Mysterious glow effect")]
        public GameObject riddleGlow;

        [Header("Riddle UI")]
        [Tooltip("Show riddle UI panel instead of dialogue")]
        public bool useRiddleUI = true;

        // Current riddle state
        private RiddleData currentRiddle;

        protected override void Start()
        {
            base.Start();

            // Subscribe to riddle events
            RiddleManager.OnRiddleAnswered += HandleRiddleAnswered;

            UpdateRiddleIndicator();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unsubscribe from riddle events
            RiddleManager.OnRiddleAnswered -= HandleRiddleAnswered;
        }

        public override void Interact()
        {
            if (isInteracting)
                return;

            // Play greeting sound
            if (greetingSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(greetingSound.name);
            }

            // Determine which dialogue to show based on riddle availability
            DialogueData dialogueToShow = GetAppropriateDialogue();

            if (dialogueToShow != null)
            {
                OnInteracted?.Invoke();
                bool started = StartDialogue(dialogueToShow);

                if (!started)
                {
                    Debug.LogWarning($"[SphinxNPC] Could not start dialogue for {npcName}");
                }
            }
            else
            {
                // Try to present a riddle directly
                if (!PresentNextRiddle())
                {
                    // Fallback to default dialogue
                    base.Interact();
                }
            }
        }

        /// <summary>
        /// Determine which dialogue to show based on riddle availability
        /// </summary>
        private DialogueData GetAppropriateDialogue()
        {
            if (RiddleManager.Instance == null)
                return defaultDialogue;

            // Check if any riddle is available
            RiddleData availableRiddle = GetNextAvailableRiddle();

            if (availableRiddle != null)
            {
                return newRiddleDialogue;
            }

            // Check if all riddles are completed
            bool allCompleted = true;
            foreach (RiddleData riddle in availableRiddles)
            {
                if (!riddle.IsCorrectlyAnswered())
                {
                    allCompleted = false;
                    break;
                }
            }

            if (allCompleted)
            {
                return allRiddlesCompleteDialogue != null ? allRiddlesCompleteDialogue : defaultDialogue;
            }

            // Riddles exist but are on cooldown
            return riddleOnCooldownDialogue != null ? riddleOnCooldownDialogue : defaultDialogue;
        }

        /// <summary>
        /// Present the next available riddle to the player
        /// </summary>
        public bool PresentNextRiddle()
        {
            RiddleData riddle = GetNextAvailableRiddle();

            if (riddle == null)
            {
                Debug.Log($"[SphinxNPC] {npcName} has no available riddles");
                return false;
            }

            return PresentRiddle(riddle);
        }

        /// <summary>
        /// Present a specific riddle to the player
        /// </summary>
        public bool PresentRiddle(RiddleData riddle)
        {
            if (riddle == null || RiddleManager.Instance == null)
                return false;

            if (!availableRiddles.Contains(riddle))
            {
                Debug.LogWarning($"[SphinxNPC] Riddle '{riddle.riddleName}' is not in {npcName}'s available riddles");
                return false;
            }

            currentRiddle = riddle;

            if (useRiddleUI)
            {
                // Use RiddleManager to present riddle with UI
                bool presented = RiddleManager.Instance.PresentRiddle(riddle);

                if (presented)
                {
                    isInteracting = true;
                    PlayTalkAnimation();
                }

                return presented;
            }
            else
            {
                // Riddle will be presented through dialogue system
                Debug.Log($"[SphinxNPC] Presenting riddle: {riddle.riddleName}");
                return true;
            }
        }

        /// <summary>
        /// Check the player's answer to the current riddle
        /// </summary>
        public void CheckAnswer(int answerIndex)
        {
            if (currentRiddle == null || RiddleManager.Instance == null)
                return;

            bool correct = RiddleManager.Instance.SubmitAnswer(currentRiddle, answerIndex);

            if (correct)
            {
                OnCorrectAnswer();
            }
            else
            {
                OnWrongAnswer();
            }

            currentRiddle = null;
            isInteracting = false;
        }

        /// <summary>
        /// Called when player answers correctly
        /// </summary>
        private void OnCorrectAnswer()
        {
            // Play correct sound
            if (correctAnswerSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(correctAnswerSound.name);
            }

            // Play correct particles
            if (correctParticles != null)
            {
                correctParticles.Play();
            }

            UpdateRiddleIndicator();
        }

        /// <summary>
        /// Called when player answers incorrectly
        /// </summary>
        private void OnWrongAnswer()
        {
            // Play wrong sound
            if (wrongAnswerSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(wrongAnswerSound.name);
            }

            // Play wrong particles
            if (wrongParticles != null)
            {
                wrongParticles.Play();
            }
        }

        /// <summary>
        /// Update riddle indicator based on available riddles
        /// </summary>
        private void UpdateRiddleIndicator()
        {
            if (riddleGlow == null)
                return;

            RiddleData availableRiddle = GetNextAvailableRiddle();
            riddleGlow.SetActive(availableRiddle != null);
        }

        /// <summary>
        /// Get the next available riddle
        /// </summary>
        public RiddleData GetNextAvailableRiddle()
        {
            if (RiddleManager.Instance == null)
                return null;

            foreach (RiddleData riddle in availableRiddles)
            {
                if (riddle.IsAvailable())
                {
                    return riddle;
                }
            }

            return null;
        }

        /// <summary>
        /// Get all unanswered riddles
        /// </summary>
        public List<RiddleData> GetUnansweredRiddles()
        {
            List<RiddleData> unanswered = new List<RiddleData>();

            foreach (RiddleData riddle in availableRiddles)
            {
                if (!riddle.IsCorrectlyAnswered())
                {
                    unanswered.Add(riddle);
                }
            }

            return unanswered;
        }

        /// <summary>
        /// Get completion percentage (0-100)
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (availableRiddles.Count == 0)
                return 100f;

            int answeredCount = 0;
            foreach (RiddleData riddle in availableRiddles)
            {
                if (riddle.IsCorrectlyAnswered())
                {
                    answeredCount++;
                }
            }

            return (float)answeredCount / availableRiddles.Count * 100f;
        }

        /// <summary>
        /// Called when any riddle is answered
        /// </summary>
        private void HandleRiddleAnswered(RiddleData riddle, bool correct)
        {
            if (availableRiddles.Contains(riddle))
            {
                UpdateRiddleIndicator();
            }
        }
    }
}
