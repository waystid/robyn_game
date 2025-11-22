using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyGame.UI
{
    /// <summary>
    /// Floating nameplate and interaction prompt above NPCs.
    /// Shows NPC name and "Press E to Talk" style prompts.
    /// </summary>
    public class NPCNameplate : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Canvas for the nameplate (should be World Space)")]
        public Canvas nameplateCanvas;

        [Tooltip("NPC name text")]
        public TextMeshProUGUI nameText;

        [Tooltip("Interaction prompt text (e.g., 'Press E to Talk')")]
        public TextMeshProUGUI promptText;

        [Tooltip("Optional icon for interaction")]
        public Image interactionIcon;

        [Tooltip("Quest indicator icon (! or ?)")]
        public Image questIndicator;

        [Header("Settings")]
        [Tooltip("NPC display name")]
        public string npcName = "Friendly NPC";

        [Tooltip("Interaction key/button name")]
        public string interactionKey = "E";

        [Tooltip("Show prompt only when player is nearby")]
        public bool showPromptOnlyWhenNear = true;

        [Tooltip("Distance at which prompt becomes visible")]
        public float promptVisibilityDistance = 5f;

        [Tooltip("Always face camera")]
        public bool billboardToCamera = true;

        [Header("Quest Indicators")]
        [Tooltip("Icon for available quest")]
        public Sprite questAvailableIcon;

        [Tooltip("Icon for quest in progress")]
        public Sprite questInProgressIcon;

        [Tooltip("Icon for quest completion")]
        public Sprite questCompleteIcon;

        [Tooltip("Color for available quest indicator")]
        public Color questAvailableColor = Color.yellow;

        [Tooltip("Color for in-progress quest indicator")]
        public Color questInProgressColor = Color.gray;

        [Tooltip("Color for complete quest indicator")]
        public Color questCompleteColor = Color.green;

        private Transform playerTransform;
        private bool isPlayerNearby = false;

        private void Start()
        {
            // Find player (usually tagged "Player")
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Initialize nameplate
            UpdateNameplate();
            HidePrompt();
        }

        private void Update()
        {
            // Billboard to camera
            if (billboardToCamera && nameplateCanvas != null && Camera.main != null)
            {
                nameplateCanvas.transform.LookAt(Camera.main.transform);
                nameplateCanvas.transform.Rotate(0, 180, 0); // Face camera correctly
            }

            // Check player distance
            if (showPromptOnlyWhenNear && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                bool shouldShowPrompt = distance <= promptVisibilityDistance;

                if (shouldShowPrompt != isPlayerNearby)
                {
                    isPlayerNearby = shouldShowPrompt;

                    if (isPlayerNearby)
                        ShowPrompt();
                    else
                        HidePrompt();
                }
            }
        }

        /// <summary>
        /// Update nameplate with current NPC name
        /// </summary>
        public void UpdateNameplate()
        {
            if (nameText != null)
            {
                nameText.text = npcName;
            }
        }

        /// <summary>
        /// Show interaction prompt
        /// </summary>
        public void ShowPrompt()
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(true);

                // Update prompt text with current input method
                if (InputManager.Instance != null && InputManager.Instance.IsMobile())
                {
                    promptText.text = "Tap to Talk";
                }
                else
                {
                    promptText.text = $"Press {interactionKey} to Talk";
                }
            }
        }

        /// <summary>
        /// Hide interaction prompt
        /// </summary>
        public void HidePrompt()
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show quest indicator with specified state
        /// </summary>
        public void ShowQuestIndicator(QuestIndicatorState state)
        {
            if (questIndicator == null)
                return;

            questIndicator.gameObject.SetActive(true);

            switch (state)
            {
                case QuestIndicatorState.Available:
                    questIndicator.sprite = questAvailableIcon;
                    questIndicator.color = questAvailableColor;
                    break;

                case QuestIndicatorState.InProgress:
                    questIndicator.sprite = questInProgressIcon;
                    questIndicator.color = questInProgressColor;
                    break;

                case QuestIndicatorState.Complete:
                    questIndicator.sprite = questCompleteIcon;
                    questIndicator.color = questCompleteColor;
                    break;
            }
        }

        /// <summary>
        /// Hide quest indicator
        /// </summary>
        public void HideQuestIndicator()
        {
            if (questIndicator != null)
            {
                questIndicator.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Set NPC name
        /// </summary>
        public void SetName(string name)
        {
            npcName = name;
            UpdateNameplate();
        }

        /// <summary>
        /// Check if player is nearby
        /// </summary>
        public bool IsPlayerNearby()
        {
            return isPlayerNearby;
        }
    }

    public enum QuestIndicatorState
    {
        Available,
        InProgress,
        Complete
    }
}
