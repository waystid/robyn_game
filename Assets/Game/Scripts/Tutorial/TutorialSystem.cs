using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Tutorial
{
    /// <summary>
    /// Tutorial trigger type
    /// </summary>
    public enum TutorialTrigger
    {
        OnStart,            // Trigger when tutorial starts
        OnEvent,            // Trigger on specific event
        OnCondition,        // Trigger when condition met
        OnInput,            // Trigger on specific input
        OnLocation,         // Trigger when entering location
        OnItemObtained,     // Trigger when item obtained
        OnLevelUp,          // Trigger on level up
        Manual              // Manually triggered
    }

    /// <summary>
    /// Tutorial display mode
    /// </summary>
    public enum TutorialDisplayMode
    {
        Popup,              // Center screen popup
        Tooltip,            // Small tooltip near UI element
        Highlight,          // Highlight UI element
        Dialogue,           // NPC-style dialogue
        Fullscreen,         // Fullscreen overlay
        Message             // Simple text message
    }

    /// <summary>
    /// Tutorial step data
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        [Header("Step Info")]
        [Tooltip("Step ID (unique)")]
        public string stepID = "tutorial_001";

        [Tooltip("Step title")]
        public string title = "Tutorial Step";

        [Tooltip("Step description/instructions")]
        [TextArea(3, 6)]
        public string description = "Follow these instructions...";

        [Header("Trigger")]
        [Tooltip("How this step is triggered")]
        public TutorialTrigger triggerType = TutorialTrigger.Manual;

        [Tooltip("Event name to listen for (if OnEvent)")]
        public string triggerEvent = "";

        [Tooltip("Required condition (if OnCondition)")]
        public string conditionCheck = "";

        [Header("Display")]
        [Tooltip("How to display this step")]
        public TutorialDisplayMode displayMode = TutorialDisplayMode.Popup;

        [Tooltip("UI element to highlight (GameObject name)")]
        public string highlightTarget = "";

        [Tooltip("Arrow direction for highlight")]
        public Vector2 arrowDirection = Vector2.down;

        [Header("Requirements")]
        [Tooltip("Previous step that must be completed")]
        public string prerequisiteStepID = "";

        [Tooltip("Can this step be skipped?")]
        public bool canSkip = true;

        [Tooltip("Auto-advance after duration (0 = manual)")]
        public float autoAdvanceDuration = 0f;

        [Header("Visual")]
        [Tooltip("Icon for this step")]
        public Sprite icon;

        [Tooltip("Background color")]
        public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);

        [Header("Audio")]
        [Tooltip("Sound to play when step appears")]
        public string soundEffect = "tutorial_step";

        [Tooltip("Voice-over clip")]
        public AudioClip voiceOver;

        // Runtime state
        [System.NonSerialized]
        public bool isCompleted = false;

        [System.NonSerialized]
        public bool isActive = false;
    }

    /// <summary>
    /// Tutorial sequence
    /// </summary>
    [System.Serializable]
    public class TutorialSequence
    {
        [Header("Sequence Info")]
        public string sequenceID = "intro_tutorial";
        public string sequenceName = "Introduction";

        [TextArea(2, 4)]
        public string description = "Learn the basics...";

        [Header("Steps")]
        public List<TutorialStep> steps = new List<TutorialStep>();

        [Header("Settings")]
        [Tooltip("Show this tutorial for new players")]
        public bool showOnFirstPlay = true;

        [Tooltip("Can entire sequence be skipped?")]
        public bool canSkipSequence = true;

        [Tooltip("Required level to view")]
        public int requiredLevel = 1;

        // Runtime state
        [System.NonSerialized]
        public bool isCompleted = false;

        [System.NonSerialized]
        public int currentStepIndex = 0;

        /// <summary>
        /// Get current step
        /// </summary>
        public TutorialStep GetCurrentStep()
        {
            if (currentStepIndex >= 0 && currentStepIndex < steps.Count)
                return steps[currentStepIndex];
            return null;
        }

        /// <summary>
        /// Advance to next step
        /// </summary>
        public bool AdvanceStep()
        {
            currentStepIndex++;
            if (currentStepIndex >= steps.Count)
            {
                isCompleted = true;
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Tutorial save data
    /// </summary>
    [System.Serializable]
    public class TutorialSaveData
    {
        public bool tutorialsEnabled = true;
        public bool hasSeenTutorial = false;
        public List<string> completedSteps = new List<string>();
        public List<string> completedSequences = new List<string>();
        public List<string> skippedSequences = new List<string>();
    }

    /// <summary>
    /// Tutorial system singleton.
    /// Manages tutorials, tooltips, and first-time player guidance.
    /// </summary>
    public class TutorialSystem : MonoBehaviour
    {
        public static TutorialSystem Instance { get; private set; }

        [Header("Tutorial Sequences")]
        [Tooltip("All tutorial sequences")]
        public List<TutorialSequence> tutorialSequences = new List<TutorialSequence>();

        [Header("Settings")]
        [Tooltip("Enable tutorials")]
        public bool tutorialsEnabled = true;

        [Tooltip("Show tutorials on first play")]
        public bool showOnFirstPlay = true;

        [Tooltip("Allow skipping tutorials")]
        public bool allowSkip = true;

        [Tooltip("Auto-start first tutorial")]
        public bool autoStartFirstTutorial = true;

        [Header("Display Settings")]
        [Tooltip("Tutorial UI panel")]
        public GameObject tutorialUIPanel;

        [Tooltip("Fade overlay during tutorials")]
        public bool useFadeOverlay = true;

        [Tooltip("Pause game during tutorials")]
        public bool pauseGameDuringTutorial = false;

        [Header("Events")]
        public UnityEvent<TutorialStep> OnStepStarted;
        public UnityEvent<TutorialStep> OnStepCompleted;
        public UnityEvent<TutorialSequence> OnSequenceStarted;
        public UnityEvent<TutorialSequence> OnSequenceCompleted;
        public UnityEvent<TutorialSequence> OnSequenceSkipped;

        // State
        private TutorialSequence currentSequence;
        private TutorialStep currentStep;
        private bool hasSeenAnyTutorial = false;
        private List<string> completedSteps = new List<string>();
        private List<string> completedSequences = new List<string>();
        private Dictionary<string, System.Action> eventListeners = new Dictionary<string, System.Action>();

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

        private void Start()
        {
            // Auto-start first tutorial if enabled
            if (autoStartFirstTutorial && showOnFirstPlay && !hasSeenAnyTutorial && tutorialSequences.Count > 0)
            {
                StartTutorialSequence(tutorialSequences[0].sequenceID);
            }
        }

        /// <summary>
        /// Start tutorial sequence
        /// </summary>
        public bool StartTutorialSequence(string sequenceID)
        {
            if (!tutorialsEnabled)
                return false;

            TutorialSequence sequence = GetSequence(sequenceID);
            if (sequence == null)
            {
                Debug.LogWarning($"[TutorialSystem] Sequence '{sequenceID}' not found!");
                return false;
            }

            // Check if already completed
            if (completedSequences.Contains(sequenceID))
            {
                Debug.Log($"[TutorialSystem] Sequence '{sequenceID}' already completed");
                return false;
            }

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < sequence.requiredLevel)
            {
                Debug.Log($"[TutorialSystem] Sequence requires level {sequence.requiredLevel}");
                return false;
            }

            currentSequence = sequence;
            currentSequence.currentStepIndex = 0;

            // Pause game if enabled
            if (pauseGameDuringTutorial)
            {
                Time.timeScale = 0f;
            }

            OnSequenceStarted?.Invoke(sequence);

            // Start first step
            AdvanceToNextStep();

            hasSeenAnyTutorial = true;

            Debug.Log($"[TutorialSystem] Started sequence: {sequence.sequenceName}");

            return true;
        }

        /// <summary>
        /// Skip current sequence
        /// </summary>
        public void SkipCurrentSequence()
        {
            if (currentSequence == null)
                return;

            if (!allowSkip || !currentSequence.canSkipSequence)
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Cannot skip this tutorial!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }
                return;
            }

            string sequenceID = currentSequence.sequenceID;

            // Mark as completed (but track as skipped)
            completedSequences.Add(sequenceID);

            OnSequenceSkipped?.Invoke(currentSequence);

            // End sequence
            EndCurrentSequence();

            Debug.Log($"[TutorialSystem] Skipped sequence: {sequenceID}");
        }

        /// <summary>
        /// Complete current step
        /// </summary>
        public void CompleteCurrentStep()
        {
            if (currentStep == null)
                return;

            currentStep.isCompleted = true;
            currentStep.isActive = false;

            // Track completed step
            if (!completedSteps.Contains(currentStep.stepID))
            {
                completedSteps.Add(currentStep.stepID);
            }

            OnStepCompleted?.Invoke(currentStep);

            // Play completion sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("tutorial_complete");
            }

            // Advance to next step
            bool hasNext = currentSequence.AdvanceStep();

            if (hasNext)
            {
                AdvanceToNextStep();
            }
            else
            {
                // Sequence completed
                CompleteCurrentSequence();
            }
        }

        /// <summary>
        /// Advance to next step
        /// </summary>
        private void AdvanceToNextStep()
        {
            if (currentSequence == null)
                return;

            TutorialStep step = currentSequence.GetCurrentStep();
            if (step == null)
            {
                CompleteCurrentSequence();
                return;
            }

            currentStep = step;
            currentStep.isActive = true;

            // Show step
            if (UI.TutorialUI.Instance != null)
            {
                UI.TutorialUI.Instance.ShowStep(step);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(step.soundEffect))
            {
                AudioManager.Instance.PlaySound(step.soundEffect);
            }

            // Play voice-over
            if (step.voiceOver != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayVoiceOver(step.voiceOver);
            }

            OnStepStarted?.Invoke(step);

            // Auto-advance if duration set
            if (step.autoAdvanceDuration > 0f)
            {
                Invoke(nameof(CompleteCurrentStep), step.autoAdvanceDuration);
            }

            Debug.Log($"[TutorialSystem] Step: {step.title}");
        }

        /// <summary>
        /// Complete current sequence
        /// </summary>
        private void CompleteCurrentSequence()
        {
            if (currentSequence == null)
                return;

            currentSequence.isCompleted = true;

            // Track completed sequence
            if (!completedSequences.Contains(currentSequence.sequenceID))
            {
                completedSequences.Add(currentSequence.sequenceID);
            }

            OnSequenceCompleted?.Invoke(currentSequence);

            // Show completion message
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Tutorial Complete: {currentSequence.sequenceName}!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.cyan
                );
            }

            EndCurrentSequence();

            Debug.Log($"[TutorialSystem] Completed sequence: {currentSequence.sequenceName}");
        }

        /// <summary>
        /// End current sequence
        /// </summary>
        private void EndCurrentSequence()
        {
            // Hide tutorial UI
            if (UI.TutorialUI.Instance != null)
            {
                UI.TutorialUI.Instance.HideTutorial();
            }

            // Resume game if paused
            if (pauseGameDuringTutorial)
            {
                Time.timeScale = 1f;
            }

            currentSequence = null;
            currentStep = null;
        }

        /// <summary>
        /// Trigger tutorial by event
        /// </summary>
        public void TriggerTutorialEvent(string eventName)
        {
            if (!tutorialsEnabled)
                return;

            // Find steps that match this event
            foreach (var sequence in tutorialSequences)
            {
                if (completedSequences.Contains(sequence.sequenceID))
                    continue;

                foreach (var step in sequence.steps)
                {
                    if (step.triggerType == TutorialTrigger.OnEvent && step.triggerEvent == eventName)
                    {
                        if (!completedSteps.Contains(step.stepID))
                        {
                            StartTutorialSequence(sequence.sequenceID);
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get tutorial sequence
        /// </summary>
        public TutorialSequence GetSequence(string sequenceID)
        {
            return tutorialSequences.Find(s => s.sequenceID == sequenceID);
        }

        /// <summary>
        /// Check if step is completed
        /// </summary>
        public bool IsStepCompleted(string stepID)
        {
            return completedSteps.Contains(stepID);
        }

        /// <summary>
        /// Check if sequence is completed
        /// </summary>
        public bool IsSequenceCompleted(string sequenceID)
        {
            return completedSequences.Contains(sequenceID);
        }

        /// <summary>
        /// Enable/disable tutorials
        /// </summary>
        public void SetTutorialsEnabled(bool enabled)
        {
            tutorialsEnabled = enabled;

            if (!enabled && currentSequence != null)
            {
                SkipCurrentSequence();
            }
        }

        /// <summary>
        /// Reset all tutorials
        /// </summary>
        public void ResetAllTutorials()
        {
            completedSteps.Clear();
            completedSequences.Clear();
            hasSeenAnyTutorial = false;

            foreach (var sequence in tutorialSequences)
            {
                sequence.isCompleted = false;
                sequence.currentStepIndex = 0;

                foreach (var step in sequence.steps)
                {
                    step.isCompleted = false;
                    step.isActive = false;
                }
            }

            Debug.Log("[TutorialSystem] Reset all tutorials");
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public TutorialSaveData GetSaveData()
        {
            return new TutorialSaveData
            {
                tutorialsEnabled = tutorialsEnabled,
                hasSeenTutorial = hasSeenAnyTutorial,
                completedSteps = new List<string>(completedSteps),
                completedSequences = new List<string>(completedSequences)
            };
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(TutorialSaveData data)
        {
            if (data == null)
                return;

            tutorialsEnabled = data.tutorialsEnabled;
            hasSeenAnyTutorial = data.hasSeenTutorial;
            completedSteps = new List<string>(data.completedSteps);
            completedSequences = new List<string>(data.completedSequences);

            // Mark steps/sequences as completed
            foreach (var sequence in tutorialSequences)
            {
                if (completedSequences.Contains(sequence.sequenceID))
                {
                    sequence.isCompleted = true;
                }

                foreach (var step in sequence.steps)
                {
                    if (completedSteps.Contains(step.stepID))
                    {
                        step.isCompleted = true;
                    }
                }
            }

            Debug.Log($"[TutorialSystem] Loaded {completedSteps.Count} completed steps, {completedSequences.Count} completed sequences");
        }

        /// <summary>
        /// Get tutorial completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (tutorialSequences.Count == 0)
                return 1f;

            int totalSteps = 0;
            int completedStepCount = 0;

            foreach (var sequence in tutorialSequences)
            {
                totalSteps += sequence.steps.Count;
                foreach (var step in sequence.steps)
                {
                    if (step.isCompleted)
                        completedStepCount++;
                }
            }

            if (totalSteps == 0)
                return 1f;

            return (float)completedStepCount / totalSteps;
        }
    }
}
