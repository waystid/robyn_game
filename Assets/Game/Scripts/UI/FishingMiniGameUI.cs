using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CozyGame.Fishing;

namespace CozyGame.UI
{
    /// <summary>
    /// Fishing mini-game UI.
    /// Timing-based button press mini-game for catching fish.
    /// Player must press button when catch window is in the target zone.
    /// </summary>
    public class FishingMiniGameUI : MonoBehaviour
    {
        public static FishingMiniGameUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Mini-game panel")]
        public GameObject miniGamePanel;

        [Header("Progress Bar")]
        [Tooltip("Progress bar background")]
        public Image progressBarBackground;

        [Tooltip("Target zone indicator")]
        public Image targetZone;

        [Tooltip("Catch window indicator")]
        public Image catchWindow;

        [Tooltip("Success zone (green)")]
        public Image successZone;

        [Header("Fish Info")]
        [Tooltip("Fish name text")]
        public TextMeshProUGUI fishNameText;

        [Tooltip("Fish size text")]
        public TextMeshProUGUI fishSizeText;

        [Tooltip("Fish rarity text")]
        public TextMeshProUGUI fishRarityText;

        [Tooltip("Fish icon")]
        public Image fishIcon;

        [Header("Instructions")]
        [Tooltip("Instruction text")]
        public TextMeshProUGUI instructionText;

        [Tooltip("Timer text")]
        public TextMeshProUGUI timerText;

        [Header("Colors")]
        [Tooltip("Success zone color")]
        public Color successColor = Color.green;

        [Tooltip("Catch window color")]
        public Color catchWindowColor = Color.yellow;

        [Tooltip("Failed color")]
        public Color failedColor = Color.red;

        [Header("Settings")]
        [Tooltip("Input key for catch attempt")]
        public KeyCode catchKey = KeyCode.Space;

        [Tooltip("Number of successful catches required")]
        public int requiredCatches = 3;

        [Tooltip("Allow misses")]
        public bool allowMisses = true;

        [Tooltip("Max allowed misses")]
        public int maxMisses = 3;

        // State
        private bool isActive = false;
        private Fish currentFish;
        private float currentFishSize;
        private FishingRod currentRod;

        private float catchWindowPosition = 0f;
        private float catchWindowDirection = 1f;
        private int successfulCatches = 0;
        private int missCount = 0;
        private float gameTimer = 0f;

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
            Hide();
        }

        private void Update()
        {
            if (!isActive)
                return;

            // Update game timer
            gameTimer += Time.deltaTime;

            // Move catch window
            UpdateCatchWindow();

            // Check for input
            if (Input.GetKeyDown(catchKey))
            {
                AttemptCatch();
            }

            // Update timer display
            if (timerText != null && currentFish != null)
            {
                float timeRemaining = currentFish.fightDuration - gameTimer;
                timerText.text = $"Time: {Mathf.Max(0, timeRemaining):F1}s";

                // Check timeout
                if (timeRemaining <= 0f)
                {
                    EndMiniGame(false);
                }
            }
        }

        /// <summary>
        /// Start the mini-game
        /// </summary>
        public void StartMiniGame(Fish fish, float fishSize, FishingRod rod)
        {
            currentFish = fish;
            currentFishSize = fishSize;
            currentRod = rod;

            isActive = true;
            successfulCatches = 0;
            missCount = 0;
            gameTimer = 0f;
            catchWindowPosition = 0f;
            catchWindowDirection = 1f;

            // Update UI
            UpdateFishInfo();
            UpdateInstruction();

            // Show panel
            Show();

            Debug.Log($"Mini-game started: {fish.fishName}");
        }

        /// <summary>
        /// Update catch window position
        /// </summary>
        private void UpdateCatchWindow()
        {
            if (currentFish == null)
                return;

            // Move window back and forth
            float speed = currentFish.catchWindowSpeed * catchWindowDirection;
            catchWindowPosition += speed * Time.deltaTime;

            // Bounce at edges
            if (catchWindowPosition >= 1f)
            {
                catchWindowPosition = 1f;
                catchWindowDirection = -1f;
            }
            else if (catchWindowPosition <= 0f)
            {
                catchWindowPosition = 0f;
                catchWindowDirection = 1f;
            }

            // Fish struggles - random direction changes
            if (Random.value < currentFish.struggleStrength * Time.deltaTime)
            {
                catchWindowDirection *= -1f;
            }

            // Update catch window visual
            if (catchWindow != null)
            {
                RectTransform rectTransform = catchWindow.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float barWidth = progressBarBackground.rectTransform.rect.width;
                    float windowWidth = barWidth * currentFish.catchWindowSize;

                    // Position window
                    float xPos = (catchWindowPosition * (barWidth - windowWidth)) - (barWidth * 0.5f) + (windowWidth * 0.5f);
                    rectTransform.anchoredPosition = new Vector2(xPos, 0f);
                    rectTransform.sizeDelta = new Vector2(windowWidth, rectTransform.sizeDelta.y);
                }
            }
        }

        /// <summary>
        /// Attempt to catch (button press)
        /// </summary>
        private void AttemptCatch()
        {
            if (currentFish == null)
                return;

            // Check if catch window is in success zone
            bool success = IsInSuccessZone();

            if (success)
            {
                OnSuccessfulCatch();
            }
            else
            {
                OnMissedCatch();
            }
        }

        /// <summary>
        /// Check if catch window is in success zone
        /// </summary>
        private bool IsInSuccessZone()
        {
            if (successZone == null || catchWindow == null)
                return false;

            // Get positions
            RectTransform successRect = successZone.GetComponent<RectTransform>();
            RectTransform catchRect = catchWindow.GetComponent<RectTransform>();

            if (successRect == null || catchRect == null)
                return false;

            // Simple overlap check
            float successCenter = successRect.anchoredPosition.x;
            float successHalfWidth = successRect.rect.width * 0.5f;
            float successMin = successCenter - successHalfWidth;
            float successMax = successCenter + successHalfWidth;

            float catchCenter = catchRect.anchoredPosition.x;
            float catchHalfWidth = catchRect.rect.width * 0.5f;
            float catchMin = catchCenter - catchHalfWidth;
            float catchMax = catchCenter + catchHalfWidth;

            // Check overlap
            return (catchMin <= successMax && catchMax >= successMin);
        }

        /// <summary>
        /// Successful catch
        /// </summary>
        private void OnSuccessfulCatch()
        {
            successfulCatches++;

            // Flash green
            if (catchWindow != null)
            {
                catchWindow.color = successColor;
                Invoke(nameof(ResetCatchWindowColor), 0.2f);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("fishing_success");
            }

            // Update instruction
            UpdateInstruction();

            Debug.Log($"Successful catch! ({successfulCatches}/{requiredCatches})");

            // Check if won
            if (successfulCatches >= requiredCatches)
            {
                EndMiniGame(true);
            }
        }

        /// <summary>
        /// Missed catch
        /// </summary>
        private void OnMissedCatch()
        {
            missCount++;

            // Flash red
            if (catchWindow != null)
            {
                catchWindow.color = failedColor;
                Invoke(nameof(ResetCatchWindowColor), 0.2f);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("fishing_fail");
            }

            Debug.Log($"Missed! ({missCount}/{maxMisses})");

            // Check if lost
            if (allowMisses && missCount >= maxMisses)
            {
                EndMiniGame(false);
            }
        }

        /// <summary>
        /// Reset catch window color
        /// </summary>
        private void ResetCatchWindowColor()
        {
            if (catchWindow != null)
            {
                catchWindow.color = catchWindowColor;
            }
        }

        /// <summary>
        /// End the mini-game
        /// </summary>
        private void EndMiniGame(bool success)
        {
            isActive = false;

            // Notify fishing rod
            if (currentRod != null)
            {
                currentRod.OnMiniGameComplete(success);
            }

            // Show result
            if (success)
            {
                Debug.Log($"Fish caught! {currentFish.fishName} ({currentFishSize:F1}cm)");

                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Caught {currentFish.fishName}!",
                        currentRod.transform.position,
                        currentFish.GetRarityColor()
                    );
                }
            }
            else
            {
                Debug.Log("Fish got away!");

                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(
                        "Fish got away!",
                        currentRod.transform.position,
                        Color.red
                    );
                }
            }

            // Hide panel
            Hide();

            // Reset
            currentFish = null;
            currentRod = null;
        }

        /// <summary>
        /// Update fish info display
        /// </summary>
        private void UpdateFishInfo()
        {
            if (currentFish == null)
                return;

            if (fishNameText != null)
            {
                fishNameText.text = currentFish.fishName;
            }

            if (fishSizeText != null)
            {
                fishSizeText.text = $"{currentFishSize:F1}cm ({currentFish.GetSizeCategory(currentFishSize)})";
            }

            if (fishRarityText != null)
            {
                fishRarityText.text = currentFish.rarity.ToString();
                fishRarityText.color = currentFish.GetRarityColor();
            }

            if (fishIcon != null && currentFish.icon != null)
            {
                fishIcon.sprite = currentFish.icon;
                fishIcon.color = currentFish.fishColor;
            }
        }

        /// <summary>
        /// Update instruction text
        /// </summary>
        private void UpdateInstruction()
        {
            if (instructionText != null)
            {
                instructionText.text = $"Press {catchKey} when in green zone!\n" +
                                      $"Progress: {successfulCatches}/{requiredCatches}";

                if (allowMisses)
                {
                    instructionText.text += $"\nMisses: {missCount}/{maxMisses}";
                }
            }
        }

        /// <summary>
        /// Show mini-game panel
        /// </summary>
        public void Show()
        {
            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(true);
            }
        }

        /// <summary>
        /// Hide mini-game panel
        /// </summary>
        public void Hide()
        {
            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Is mini-game active?
        /// </summary>
        public bool IsActive()
        {
            return isActive;
        }
    }
}
