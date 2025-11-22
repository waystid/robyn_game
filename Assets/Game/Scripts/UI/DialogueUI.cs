using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CozyGame.Dialogue;

namespace CozyGame.UI
{
    /// <summary>
    /// UI controller for dialogue display.
    /// Manages dialogue panel, speaker info, text display, and choice buttons.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("Panel References")]
        [Tooltip("Main dialogue panel container")]
        public GameObject dialoguePanel;

        [Tooltip("Speaker name text")]
        public TextMeshProUGUI speakerNameText;

        [Tooltip("Speaker portrait image")]
        public Image speakerPortrait;

        [Tooltip("Main dialogue text")]
        public TextMeshProUGUI dialogueText;

        [Header("Button References")]
        [Tooltip("Container for choice buttons")]
        public Transform choiceButtonContainer;

        [Tooltip("Prefab for choice buttons")]
        public GameObject choiceButtonPrefab;

        [Tooltip("Continue button (for linear dialogue)")]
        public Button continueButton;

        [Tooltip("End conversation button")]
        public Button endButton;

        [Header("Settings")]
        [Tooltip("Hide speaker portrait if none is provided")]
        public bool hidePortraitWhenNull = true;

        [Tooltip("Max number of choice buttons to pool")]
        public int maxChoiceButtons = 6;

        // Button pool
        private List<GameObject> choiceButtonPool = new List<GameObject>();
        private System.Action<DialogueChoice> currentChoiceCallback;

        private void Awake()
        {
            // Initialize choice button pool
            if (choiceButtonPrefab != null && choiceButtonContainer != null)
            {
                for (int i = 0; i < maxChoiceButtons; i++)
                {
                    GameObject button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                    button.SetActive(false);
                    choiceButtonPool.Add(button);
                }
            }

            // Hide dialogue panel initially
            HideDialogue();
        }

        /// <summary>
        /// Show the dialogue panel with speaker info
        /// </summary>
        public void ShowDialogue(string speakerName, Sprite portrait = null)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // Set speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = speakerName;
            }

            // Set speaker portrait
            if (speakerPortrait != null)
            {
                if (portrait != null)
                {
                    speakerPortrait.sprite = portrait;
                    speakerPortrait.gameObject.SetActive(true);
                }
                else
                {
                    speakerPortrait.gameObject.SetActive(!hidePortraitWhenNull);
                }
            }

            // Clear text initially
            if (dialogueText != null)
            {
                dialogueText.text = "";
            }

            // Hide all buttons initially
            HideAllButtons();
        }

        /// <summary>
        /// Hide the dialogue panel
        /// </summary>
        public void HideDialogue()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            HideAllButtons();
        }

        /// <summary>
        /// Set the dialogue text content
        /// </summary>
        public void SetDialogueText(string text)
        {
            if (dialogueText != null)
            {
                dialogueText.text = text;
            }
        }

        /// <summary>
        /// Show choice buttons for player decisions
        /// </summary>
        public void ShowChoices(List<DialogueChoice> choices, System.Action<DialogueChoice> onChoiceSelected)
        {
            HideAllButtons();

            currentChoiceCallback = onChoiceSelected;

            int buttonIndex = 0;
            foreach (DialogueChoice choice in choices)
            {
                if (buttonIndex >= choiceButtonPool.Count)
                {
                    Debug.LogWarning($"[DialogueUI] Not enough choice buttons pooled. Need {choices.Count}, have {choiceButtonPool.Count}");
                    break;
                }

                GameObject buttonObj = choiceButtonPool[buttonIndex];
                buttonObj.SetActive(true);

                // Set button text
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = choice.choiceText;
                }

                // Set button color
                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = choice.choiceColor;
                }

                // Set button icon if present
                if (choice.choiceIcon != null)
                {
                    Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
                    if (iconImage != null)
                    {
                        iconImage.sprite = choice.choiceIcon;
                        iconImage.gameObject.SetActive(true);
                    }
                }

                // Set button click handler
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    DialogueChoice capturedChoice = choice; // Capture for closure
                    button.onClick.AddListener(() => OnChoiceButtonClick(capturedChoice));
                }

                buttonIndex++;
            }
        }

        /// <summary>
        /// Hide all choice buttons
        /// </summary>
        public void HideChoices()
        {
            foreach (GameObject button in choiceButtonPool)
            {
                button.SetActive(false);
            }
        }

        /// <summary>
        /// Show the continue button for linear dialogue
        /// </summary>
        public void ShowContinueButton(System.Action onContinue)
        {
            HideAllButtons();

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() => onContinue?.Invoke());
            }
        }

        /// <summary>
        /// Show the end conversation button
        /// </summary>
        public void ShowEndButton(System.Action onEnd)
        {
            HideAllButtons();

            if (endButton != null)
            {
                endButton.gameObject.SetActive(true);
                endButton.onClick.RemoveAllListeners();
                endButton.onClick.AddListener(() => onEnd?.Invoke());
            }
        }

        /// <summary>
        /// Hide all buttons (choices, continue, end)
        /// </summary>
        private void HideAllButtons()
        {
            HideChoices();

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(false);
            }

            if (endButton != null)
            {
                endButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when a choice button is clicked
        /// </summary>
        private void OnChoiceButtonClick(DialogueChoice choice)
        {
            currentChoiceCallback?.Invoke(choice);
        }

        /// <summary>
        /// Check if dialogue panel is visible
        /// </summary>
        public bool IsVisible()
        {
            return dialoguePanel != null && dialoguePanel.activeSelf;
        }
    }
}
