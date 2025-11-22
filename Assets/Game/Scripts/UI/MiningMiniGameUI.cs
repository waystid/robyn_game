using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using CozyGame.Mining;

namespace CozyGame.UI
{
    /// <summary>
    /// Mining mini-game type
    /// </summary>
    public enum MiningMiniGameType
    {
        Timing,         // Hit within timing window
        Rhythm,         // Hit on beat
        Mashing,        // Rapid button presses
        Sequence        // Hit correct button sequence
    }

    /// <summary>
    /// Mining mini-game UI controller.
    /// Implements timing/rhythm based mining challenges.
    /// </summary>
    public class MiningMiniGameUI : MonoBehaviour
    {
        public static MiningMiniGameUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("Main mini-game panel")]
        public GameObject miniGamePanel;

        [Header("Timing Mini-Game")]
        [Tooltip("Progress bar background")]
        public Image progressBarBackground;

        [Tooltip("Moving indicator")]
        public Image movingIndicator;

        [Tooltip("Success zone (green area)")]
        public Image successZone;

        [Tooltip("Perfect zone (gold area, smaller)")]
        public Image perfectZone;

        [Header("Display")]
        [Tooltip("Resource name text")]
        public TextMeshProUGUI resourceNameText;

        [Tooltip("Instruction text")]
        public TextMeshProUGUI instructionText;

        [Tooltip("Result text")]
        public TextMeshProUGUI resultText;

        [Tooltip("Durability text")]
        public TextMeshProUGUI durabilityText;

        [Header("Settings")]
        [Tooltip("Mini-game type")]
        public MiningMiniGameType gameType = MiningMiniGameType.Timing;

        [Tooltip("Success zone size (0-1)")]
        [Range(0.1f, 0.5f)]
        public float successZoneSize = 0.3f;

        [Tooltip("Perfect zone size (0-1, smaller than success)")]
        [Range(0.05f, 0.3f)]
        public float perfectZoneSize = 0.1f;

        [Tooltip("Indicator speed")]
        [Range(0.5f, 3f)]
        public float indicatorSpeed = 1f;

        [Tooltip("Result display time")]
        public float resultDisplayTime = 0.5f;

        [Header("Colors")]
        [Tooltip("Success color")]
        public Color successColor = Color.green;

        [Tooltip("Perfect color")]
        public Color perfectColor = Color.yellow;

        [Tooltip("Fail color")]
        public Color failColor = Color.red;

        [Header("Input")]
        [Tooltip("Mining action key")]
        public KeyCode miningKey = KeyCode.Space;

        // State
        private bool isActive = false;
        private ResourceNode currentNode;
        private GameObject currentMiner;
        private float indicatorPosition = 0f;
        private bool movingRight = true;
        private float successZoneCenter = 0.5f;
        private MineableResource currentResource;

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
            // Hide panel initially
            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isActive)
                return;

            // Update mini-game based on type
            switch (gameType)
            {
                case MiningMiniGameType.Timing:
                    UpdateTimingGame();
                    break;
                case MiningMiniGameType.Rhythm:
                    UpdateRhythmGame();
                    break;
                case MiningMiniGameType.Mashing:
                    UpdateMashingGame();
                    break;
                case MiningMiniGameType.Sequence:
                    UpdateSequenceGame();
                    break;
            }
        }

        /// <summary>
        /// Start mini-game
        /// </summary>
        public void StartMiniGame(ResourceNode node, GameObject miner)
        {
            if (node == null || node.resourceData == null)
                return;

            currentNode = node;
            currentMiner = miner;
            currentResource = node.resourceData;
            isActive = true;

            // Show panel
            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(true);
            }

            // Setup UI
            if (resourceNameText != null)
            {
                resourceNameText.text = currentResource.resourceName;
                resourceNameText.color = currentResource.GetRarityColor();
            }

            if (instructionText != null)
            {
                instructionText.text = $"Press [{miningKey}] when in the green zone!";
            }

            if (resultText != null)
            {
                resultText.gameObject.SetActive(false);
            }

            // Randomize success zone position
            successZoneCenter = Random.Range(0.2f, 0.8f);

            // Adjust difficulty based on resource hardness
            float difficulty = currentResource.hardness;
            successZoneSize = Mathf.Lerp(0.4f, 0.15f, difficulty);
            perfectZoneSize = successZoneSize * 0.3f;
            indicatorSpeed = Mathf.Lerp(0.8f, 2f, difficulty);

            // Setup visual zones
            SetupVisualZones();

            // Reset indicator
            indicatorPosition = 0f;
            movingRight = true;

            Debug.Log($"[MiningMiniGameUI] Started mini-game for {currentResource.resourceName}");
        }

        /// <summary>
        /// Setup visual zones
        /// </summary>
        private void SetupVisualZones()
        {
            if (successZone != null)
            {
                RectTransform rect = successZone.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(successZoneCenter - successZoneSize / 2f, 0f);
                    rect.anchorMax = new Vector2(successZoneCenter + successZoneSize / 2f, 1f);
                }
                successZone.color = new Color(0f, 1f, 0f, 0.5f);
            }

            if (perfectZone != null)
            {
                RectTransform rect = perfectZone.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(successZoneCenter - perfectZoneSize / 2f, 0f);
                    rect.anchorMax = new Vector2(successZoneCenter + perfectZoneSize / 2f, 1f);
                }
                perfectZone.color = new Color(1f, 1f, 0f, 0.7f);
            }
        }

        /// <summary>
        /// Update timing mini-game
        /// </summary>
        private void UpdateTimingGame()
        {
            // Move indicator
            float speed = indicatorSpeed * (movingRight ? 1f : -1f);
            indicatorPosition += speed * Time.deltaTime;

            // Bounce at edges
            if (indicatorPosition >= 1f)
            {
                indicatorPosition = 1f;
                movingRight = false;
            }
            else if (indicatorPosition <= 0f)
            {
                indicatorPosition = 0f;
                movingRight = true;
            }

            // Update visual
            if (movingIndicator != null)
            {
                RectTransform rect = movingIndicator.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(indicatorPosition - 0.02f, 0f);
                    rect.anchorMax = new Vector2(indicatorPosition + 0.02f, 1f);
                }
            }

            // Check for input
            if (Input.GetKeyDown(miningKey) || Input.GetMouseButtonDown(0))
            {
                CheckTimingHit();
            }
        }

        /// <summary>
        /// Check timing hit result
        /// </summary>
        private void CheckTimingHit()
        {
            bool isPerfect = false;
            bool isSuccess = false;

            // Check if in perfect zone
            float perfectMin = successZoneCenter - perfectZoneSize / 2f;
            float perfectMax = successZoneCenter + perfectZoneSize / 2f;
            if (indicatorPosition >= perfectMin && indicatorPosition <= perfectMax)
            {
                isPerfect = true;
                isSuccess = true;
            }
            // Check if in success zone
            else
            {
                float successMin = successZoneCenter - successZoneSize / 2f;
                float successMax = successZoneCenter + successZoneSize / 2f;
                if (indicatorPosition >= successMin && indicatorPosition <= successMax)
                {
                    isSuccess = true;
                }
            }

            // Show result
            StartCoroutine(ShowResult(isSuccess, isPerfect));
        }

        /// <summary>
        /// Show result and close mini-game
        /// </summary>
        private IEnumerator ShowResult(bool success, bool perfect)
        {
            isActive = false;

            // Show result text
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                if (perfect)
                {
                    resultText.text = "PERFECT!";
                    resultText.color = perfectColor;
                }
                else if (success)
                {
                    resultText.text = "Success!";
                    resultText.color = successColor;
                }
                else
                {
                    resultText.text = "Miss!";
                    resultText.color = failColor;
                }
            }

            // Wait to show result
            yield return new WaitForSeconds(resultDisplayTime);

            // Send result to node
            if (currentNode != null)
            {
                currentNode.OnMiningAttempt(currentMiner, success, perfect);
            }

            // Close panel
            EndMiniGame();
        }

        /// <summary>
        /// Update rhythm mini-game (hit on beat)
        /// </summary>
        private void UpdateRhythmGame()
        {
            // TODO: Implement rhythm game with beats
            UpdateTimingGame(); // Fallback to timing for now
        }

        /// <summary>
        /// Update mashing mini-game (rapid button presses)
        /// </summary>
        private void UpdateMashingGame()
        {
            // TODO: Implement button mashing
            UpdateTimingGame(); // Fallback to timing for now
        }

        /// <summary>
        /// Update sequence mini-game (hit correct sequence)
        /// </summary>
        private void UpdateSequenceGame()
        {
            // TODO: Implement sequence game
            UpdateTimingGame(); // Fallback to timing for now
        }

        /// <summary>
        /// End mini-game
        /// </summary>
        public void EndMiniGame()
        {
            isActive = false;
            currentNode = null;
            currentMiner = null;
            currentResource = null;

            if (miniGamePanel != null)
            {
                miniGamePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Cancel mini-game
        /// </summary>
        public void CancelMiniGame()
        {
            if (!isActive)
                return;

            // Send fail to node
            if (currentNode != null)
            {
                currentNode.OnMiningAttempt(currentMiner, false, false);
            }

            EndMiniGame();
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
