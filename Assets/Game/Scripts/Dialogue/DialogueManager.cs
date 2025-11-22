using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CozyGame.Dialogue
{
    /// <summary>
    /// Singleton manager for dialogue system.
    /// Handles dialogue flow, node transitions, and integration with UI.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Typing speed for dialogue text (characters per second)")]
        public float typingSpeed = 30f;

        [Tooltip("Delay between voice blip sounds (seconds)")]
        public float voiceBlipInterval = 0.1f;

        [Tooltip("Volume for voice blip sounds")]
        [Range(0f, 1f)]
        public float voiceBlipVolume = 0.5f;

        [Tooltip("Volume for voice line audio")]
        [Range(0f, 1f)]
        public float voiceLineVolume = 0.8f;

        [Header("References")]
        [Tooltip("Reference to the DialogueUI component")]
        public DialogueUI dialogueUI;

        // Events
        public static UnityAction<DialogueData> OnDialogueStarted;
        public static UnityAction<DialogueData> OnDialogueEnded;
        public static UnityAction<DialogueNode> OnNodeDisplayed;
        public static UnityAction<DialogueChoice> OnChoiceSelected;
        public static UnityAction<string> OnCustomEvent;

        // Current dialogue state
        private DialogueData currentDialogue;
        private DialogueNode currentNode;
        private bool isDialogueActive = false;
        private Coroutine typingCoroutine;
        private AudioSource voiceLineSource;
        private AudioSource voiceBlipSource;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Find DialogueUI if not assigned
            if (dialogueUI == null)
            {
                dialogueUI = FindObjectOfType<DialogueUI>();
                if (dialogueUI == null)
                {
                    Debug.LogWarning("[DialogueManager] No DialogueUI found in scene. Dialogue visuals will not work.");
                }
            }
        }

        private void InitializeAudioSources()
        {
            // Create audio sources for voice lines and blips
            voiceLineSource = gameObject.AddComponent<AudioSource>();
            voiceLineSource.playOnAwake = false;
            voiceLineSource.loop = false;

            voiceBlipSource = gameObject.AddComponent<AudioSource>();
            voiceBlipSource.playOnAwake = false;
            voiceBlipSource.loop = false;
        }

        /// <summary>
        /// Start a dialogue conversation
        /// </summary>
        public bool StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogWarning("[DialogueManager] Cannot start null dialogue");
                return false;
            }

            if (!dialogue.IsAvailable())
            {
                Debug.Log($"[DialogueManager] Dialogue '{dialogue.dialogueName}' is not available (cooldown or already played)");
                return false;
            }

            if (isDialogueActive)
            {
                Debug.Log("[DialogueManager] Already in dialogue, ending current dialogue first");
                EndDialogue();
            }

            currentDialogue = dialogue;
            currentNode = dialogue.GetStartNode();

            if (currentNode == null)
            {
                Debug.LogError($"[DialogueManager] Dialogue '{dialogue.dialogueName}' has no valid start node!");
                return false;
            }

            isDialogueActive = true;
            currentDialogue.MarkAsPlayed();

            // Play background music if specified
            if (dialogue.backgroundMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(dialogue.backgroundMusic);
            }

            OnDialogueStarted?.Invoke(dialogue);
            DisplayCurrentNode();

            return true;
        }

        /// <summary>
        /// End the current dialogue
        /// </summary>
        public void EndDialogue()
        {
            if (!isDialogueActive)
                return;

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            StopVoiceLine();

            DialogueData endedDialogue = currentDialogue;

            isDialogueActive = false;
            currentDialogue = null;
            currentNode = null;

            if (dialogueUI != null)
            {
                dialogueUI.HideDialogue();
            }

            OnDialogueEnded?.Invoke(endedDialogue);
        }

        /// <summary>
        /// Display the current dialogue node
        /// </summary>
        private void DisplayCurrentNode()
        {
            if (currentNode == null)
            {
                EndDialogue();
                return;
            }

            // Check if node conditions are met
            if (!currentNode.AreConditionsMet())
            {
                Debug.Log($"[DialogueManager] Node '{currentNode.nodeID}' conditions not met, skipping");
                AdvanceToNextNode();
                return;
            }

            // Execute node actions (quest start, item give, etc.)
            currentNode.ExecuteActions();

            // Trigger animation if specified
            if (!string.IsNullOrEmpty(currentNode.animationTrigger))
            {
                // TODO: Trigger animation on NPC
                Debug.Log($"[DialogueManager] Playing animation: {currentNode.animationTrigger}");
            }

            // Play voice line if specified
            if (currentNode.voiceLine != null)
            {
                PlayVoiceLine(currentNode.voiceLine);
            }

            // Start typing effect
            if (dialogueUI != null)
            {
                if (typingCoroutine != null)
                    StopCoroutine(typingCoroutine);

                typingCoroutine = StartCoroutine(TypeDialogueText(currentNode));
            }

            // Auto-advance if specified
            if (currentNode.autoAdvanceDelay > 0f)
            {
                StartCoroutine(AutoAdvanceCoroutine(currentNode.autoAdvanceDelay));
            }

            OnNodeDisplayed?.Invoke(currentNode);
        }

        /// <summary>
        /// Type out dialogue text with typing effect
        /// </summary>
        private IEnumerator TypeDialogueText(DialogueNode node)
        {
            if (dialogueUI == null)
                yield break;

            // Wait for delay if specified
            if (node.delayBeforeShow > 0f)
            {
                yield return new WaitForSeconds(node.delayBeforeShow);
            }

            // Show dialogue panel
            dialogueUI.ShowDialogue(node.speakerName, node.speakerPortrait);

            // Type out text character by character
            string fullText = node.dialogueText;
            float charDelay = 1f / typingSpeed;
            float blipTimer = 0f;

            for (int i = 0; i <= fullText.Length; i++)
            {
                string currentText = fullText.Substring(0, i);
                dialogueUI.SetDialogueText(currentText);

                // Play voice blip
                if (node.voiceBlip != null)
                {
                    blipTimer += charDelay;
                    if (blipTimer >= voiceBlipInterval)
                    {
                        PlayVoiceBlip(node.voiceBlip);
                        blipTimer = 0f;
                    }
                }

                yield return new WaitForSeconds(charDelay);
            }

            typingCoroutine = null;

            // Show choices or continue button
            ShowNodeOptions(node);
        }

        /// <summary>
        /// Show choices or continue button for current node
        /// </summary>
        private void ShowNodeOptions(DialogueNode node)
        {
            if (dialogueUI == null)
                return;

            List<DialogueChoice> validChoices = node.GetValidChoices();

            if (validChoices.Count > 0)
            {
                // Show multiple choice buttons
                dialogueUI.ShowChoices(validChoices, OnChoiceButtonClicked);
            }
            else if (!node.IsEndNode())
            {
                // Show single "Continue" button
                dialogueUI.ShowContinueButton(OnContinueButtonClicked);
            }
            else
            {
                // End node - show "End Conversation" button
                dialogueUI.ShowEndButton(OnEndButtonClicked);
            }
        }

        /// <summary>
        /// Called when player clicks a choice button
        /// </summary>
        private void OnChoiceButtonClicked(DialogueChoice choice)
        {
            OnChoiceSelected?.Invoke(choice);

            if (dialogueUI != null)
            {
                dialogueUI.HideChoices();
            }

            // Move to target node
            currentNode = currentDialogue.GetNode(choice.targetNodeID);
            DisplayCurrentNode();
        }

        /// <summary>
        /// Called when player clicks continue button
        /// </summary>
        private void OnContinueButtonClicked()
        {
            AdvanceToNextNode();
        }

        /// <summary>
        /// Called when player clicks end conversation button
        /// </summary>
        private void OnEndButtonClicked()
        {
            EndDialogue();
        }

        /// <summary>
        /// Advance to the next node in linear dialogue
        /// </summary>
        private void AdvanceToNextNode()
        {
            if (currentNode == null || string.IsNullOrEmpty(currentNode.nextNodeID))
            {
                EndDialogue();
                return;
            }

            currentNode = currentDialogue.GetNode(currentNode.nextNodeID);
            DisplayCurrentNode();
        }

        /// <summary>
        /// Auto-advance coroutine
        /// </summary>
        private IEnumerator AutoAdvanceCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (isDialogueActive && currentNode != null)
            {
                AdvanceToNextNode();
            }
        }

        /// <summary>
        /// Play a voice line audio clip
        /// </summary>
        private void PlayVoiceLine(AudioClip clip)
        {
            if (voiceLineSource != null && clip != null)
            {
                voiceLineSource.clip = clip;
                voiceLineSource.volume = voiceLineVolume;
                voiceLineSource.Play();
            }
        }

        /// <summary>
        /// Stop currently playing voice line
        /// </summary>
        private void StopVoiceLine()
        {
            if (voiceLineSource != null && voiceLineSource.isPlaying)
            {
                voiceLineSource.Stop();
            }
        }

        /// <summary>
        /// Play a voice blip sound
        /// </summary>
        private void PlayVoiceBlip(AudioClip clip)
        {
            if (voiceBlipSource != null && clip != null)
            {
                voiceBlipSource.PlayOneShot(clip, voiceBlipVolume);
            }
        }

        /// <summary>
        /// Skip typing animation and show full text immediately
        /// </summary>
        public void SkipTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;

                if (dialogueUI != null && currentNode != null)
                {
                    dialogueUI.SetDialogueText(currentNode.dialogueText);
                    ShowNodeOptions(currentNode);
                }
            }
        }

        /// <summary>
        /// Get current dialogue state
        /// </summary>
        public bool IsDialogueActive()
        {
            return isDialogueActive;
        }

        /// <summary>
        /// Get current dialogue data
        /// </summary>
        public DialogueData GetCurrentDialogue()
        {
            return currentDialogue;
        }

        /// <summary>
        /// Get current node
        /// </summary>
        public DialogueNode GetCurrentNode()
        {
            return currentNode;
        }
    }
}
