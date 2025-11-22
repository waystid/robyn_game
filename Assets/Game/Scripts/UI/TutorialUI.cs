using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CozyGame.Tutorial;

namespace CozyGame.UI
{
    /// <summary>
    /// Tutorial UI controller.
    /// Displays tutorial steps with various display modes.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        public static TutorialUI Instance { get; private set; }

        [Header("Display Panels")]
        [Tooltip("Popup panel (center screen)")]
        public GameObject popupPanel;

        [Tooltip("Tooltip panel (small)")]
        public GameObject tooltipPanel;

        [Tooltip("Highlight panel")]
        public GameObject highlightPanel;

        [Tooltip("Fullscreen panel")]
        public GameObject fullscreenPanel;

        [Tooltip("Message panel (simple text)")]
        public GameObject messagePanel;

        [Header("Popup Elements")]
        [Tooltip("Popup title text")]
        public TextMeshProUGUI popupTitleText;

        [Tooltip("Popup description text")]
        public TextMeshProUGUI popupDescriptionText;

        [Tooltip("Popup icon image")]
        public Image popupIconImage;

        [Tooltip("Popup background image")]
        public Image popupBackgroundImage;

        [Tooltip("Continue button")]
        public Button continueButton;

        [Tooltip("Skip button")]
        public Button skipButton;

        [Header("Tooltip Elements")]
        [Tooltip("Tooltip text")]
        public TextMeshProUGUI tooltipText;

        [Tooltip("Tooltip arrow")]
        public RectTransform tooltipArrow;

        [Header("Highlight Elements")]
        [Tooltip("Highlight frame")]
        public RectTransform highlightFrame;

        [Tooltip("Highlight arrow")]
        public RectTransform highlightArrow;

        [Tooltip("Highlight text")]
        public TextMeshProUGUI highlightText;

        [Header("Fullscreen Elements")]
        [Tooltip("Fullscreen title")]
        public TextMeshProUGUI fullscreenTitleText;

        [Tooltip("Fullscreen description")]
        public TextMeshProUGUI fullscreenDescriptionText;

        [Tooltip("Fullscreen image")]
        public Image fullscreenImage;

        [Header("Message Elements")]
        [Tooltip("Message text")]
        public TextMeshProUGUI messageText;

        [Header("Effects")]
        [Tooltip("Fade overlay")]
        public Image fadeOverlay;

        [Tooltip("Fade duration")]
        public float fadeDuration = 0.3f;

        [Header("Progress")]
        [Tooltip("Step counter text")]
        public TextMeshProUGUI stepCounterText;

        [Tooltip("Progress bar")]
        public Slider progressBar;

        // State
        private TutorialStep currentStep;
        private GameObject currentPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Setup buttons
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            // Hide all panels initially
            HideAllPanels();
        }

        /// <summary>
        /// Show tutorial step
        /// </summary>
        public void ShowStep(TutorialStep step)
        {
            if (step == null)
                return;

            currentStep = step;

            // Hide previous panel
            HideAllPanels();

            // Show appropriate panel based on display mode
            switch (step.displayMode)
            {
                case TutorialDisplayMode.Popup:
                    ShowPopup(step);
                    break;
                case TutorialDisplayMode.Tooltip:
                    ShowTooltip(step);
                    break;
                case TutorialDisplayMode.Highlight:
                    ShowHighlight(step);
                    break;
                case TutorialDisplayMode.Fullscreen:
                    ShowFullscreen(step);
                    break;
                case TutorialDisplayMode.Message:
                    ShowMessage(step);
                    break;
            }

            // Update progress
            UpdateProgress();

            // Show fade overlay if enabled
            if (fadeOverlay != null && TutorialSystem.Instance != null && TutorialSystem.Instance.useFadeOverlay)
            {
                fadeOverlay.gameObject.SetActive(true);
                FadeIn();
            }
        }

        /// <summary>
        /// Hide tutorial
        /// </summary>
        public void HideTutorial()
        {
            FadeOut(() =>
            {
                HideAllPanels();
                currentStep = null;
            });
        }

        /// <summary>
        /// Show popup panel
        /// </summary>
        private void ShowPopup(TutorialStep step)
        {
            if (popupPanel == null)
                return;

            currentPanel = popupPanel;
            popupPanel.SetActive(true);

            // Title
            if (popupTitleText != null)
            {
                popupTitleText.text = step.title;
            }

            // Description
            if (popupDescriptionText != null)
            {
                popupDescriptionText.text = step.description;
            }

            // Icon
            if (popupIconImage != null && step.icon != null)
            {
                popupIconImage.sprite = step.icon;
                popupIconImage.gameObject.SetActive(true);
            }
            else if (popupIconImage != null)
            {
                popupIconImage.gameObject.SetActive(false);
            }

            // Background color
            if (popupBackgroundImage != null)
            {
                popupBackgroundImage.color = step.backgroundColor;
            }

            // Skip button
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(step.canSkip);
            }
        }

        /// <summary>
        /// Show tooltip panel
        /// </summary>
        private void ShowTooltip(TutorialStep step)
        {
            if (tooltipPanel == null)
                return;

            currentPanel = tooltipPanel;
            tooltipPanel.SetActive(true);

            // Text
            if (tooltipText != null)
            {
                tooltipText.text = step.description;
            }

            // Position near highlight target
            if (!string.IsNullOrEmpty(step.highlightTarget))
            {
                PositionTooltipNearTarget(step.highlightTarget);
            }

            // Arrow
            if (tooltipArrow != null)
            {
                UpdateArrowDirection(tooltipArrow, step.arrowDirection);
            }
        }

        /// <summary>
        /// Show highlight panel
        /// </summary>
        private void ShowHighlight(TutorialStep step)
        {
            if (highlightPanel == null)
                return;

            currentPanel = highlightPanel;
            highlightPanel.SetActive(true);

            // Find and highlight target
            if (!string.IsNullOrEmpty(step.highlightTarget))
            {
                GameObject target = GameObject.Find(step.highlightTarget);
                if (target != null)
                {
                    RectTransform targetRect = target.GetComponent<RectTransform>();
                    if (targetRect != null && highlightFrame != null)
                    {
                        // Position highlight frame around target
                        highlightFrame.position = targetRect.position;
                        highlightFrame.sizeDelta = targetRect.sizeDelta * 1.1f; // Slightly larger
                    }
                }
            }

            // Text
            if (highlightText != null)
            {
                highlightText.text = step.description;
            }

            // Arrow
            if (highlightArrow != null)
            {
                UpdateArrowDirection(highlightArrow, step.arrowDirection);
            }
        }

        /// <summary>
        /// Show fullscreen panel
        /// </summary>
        private void ShowFullscreen(TutorialStep step)
        {
            if (fullscreenPanel == null)
                return;

            currentPanel = fullscreenPanel;
            fullscreenPanel.SetActive(true);

            // Title
            if (fullscreenTitleText != null)
            {
                fullscreenTitleText.text = step.title;
            }

            // Description
            if (fullscreenDescriptionText != null)
            {
                fullscreenDescriptionText.text = step.description;
            }

            // Image
            if (fullscreenImage != null && step.icon != null)
            {
                fullscreenImage.sprite = step.icon;
                fullscreenImage.gameObject.SetActive(true);
            }
            else if (fullscreenImage != null)
            {
                fullscreenImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Show message panel
        /// </summary>
        private void ShowMessage(TutorialStep step)
        {
            if (messagePanel == null)
                return;

            currentPanel = messagePanel;
            messagePanel.SetActive(true);

            // Text
            if (messageText != null)
            {
                messageText.text = step.description;
            }
        }

        /// <summary>
        /// Hide all panels
        /// </summary>
        private void HideAllPanels()
        {
            if (popupPanel != null) popupPanel.SetActive(false);
            if (tooltipPanel != null) tooltipPanel.SetActive(false);
            if (highlightPanel != null) highlightPanel.SetActive(false);
            if (fullscreenPanel != null) fullscreenPanel.SetActive(false);
            if (messagePanel != null) messagePanel.SetActive(false);
            if (fadeOverlay != null) fadeOverlay.gameObject.SetActive(false);

            currentPanel = null;
        }

        /// <summary>
        /// Position tooltip near target
        /// </summary>
        private void PositionTooltipNearTarget(string targetName)
        {
            GameObject target = GameObject.Find(targetName);
            if (target == null || tooltipPanel == null)
                return;

            RectTransform targetRect = target.GetComponent<RectTransform>();
            RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();

            if (targetRect != null && tooltipRect != null)
            {
                // Position above target
                Vector3 targetPos = targetRect.position;
                targetPos.y += targetRect.rect.height / 2f + tooltipRect.rect.height / 2f + 20f;
                tooltipRect.position = targetPos;
            }
        }

        /// <summary>
        /// Update arrow direction
        /// </summary>
        private void UpdateArrowDirection(RectTransform arrow, Vector2 direction)
        {
            if (arrow == null)
                return;

            // Calculate rotation from direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.rotation = Quaternion.Euler(0f, 0f, angle - 90f); // -90 because arrow points up by default
        }

        /// <summary>
        /// Update progress display
        /// </summary>
        private void UpdateProgress()
        {
            if (TutorialSystem.Instance == null)
                return;

            // Step counter
            if (stepCounterText != null)
            {
                // Get current sequence
                var currentSequence = TutorialSystem.Instance.tutorialSequences.Find(s =>
                {
                    foreach (var step in s.steps)
                    {
                        if (step == currentStep)
                            return true;
                    }
                    return false;
                });

                if (currentSequence != null)
                {
                    int currentIndex = currentSequence.currentStepIndex + 1;
                    int totalSteps = currentSequence.steps.Count;
                    stepCounterText.text = $"Step {currentIndex}/{totalSteps}";
                }
            }

            // Progress bar
            if (progressBar != null)
            {
                float progress = TutorialSystem.Instance.GetCompletionPercentage();
                progressBar.value = progress;
            }
        }

        /// <summary>
        /// Fade in
        /// </summary>
        private void FadeIn()
        {
            if (fadeOverlay == null)
                return;

            LeanTween.alpha(fadeOverlay.rectTransform, 0.5f, fadeDuration).setIgnoreTimeScale(true);
        }

        /// <summary>
        /// Fade out
        /// </summary>
        private void FadeOut(System.Action onComplete = null)
        {
            if (fadeOverlay == null)
            {
                onComplete?.Invoke();
                return;
            }

            LeanTween.alpha(fadeOverlay.rectTransform, 0f, fadeDuration)
                .setIgnoreTimeScale(true)
                .setOnComplete(() =>
                {
                    if (fadeOverlay != null)
                        fadeOverlay.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        /// <summary>
        /// Continue button clicked
        /// </summary>
        private void OnContinueClicked()
        {
            if (TutorialSystem.Instance != null)
            {
                TutorialSystem.Instance.CompleteCurrentStep();
            }
        }

        /// <summary>
        /// Skip button clicked
        /// </summary>
        private void OnSkipClicked()
        {
            if (TutorialSystem.Instance != null)
            {
                TutorialSystem.Instance.SkipCurrentSequence();
            }
        }
    }
}
