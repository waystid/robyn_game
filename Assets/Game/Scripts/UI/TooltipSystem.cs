using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace CozyGame.UI
{
    /// <summary>
    /// Tooltip trigger component.
    /// Add to UI elements to show tooltips on hover.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Tooltip Content")]
        [Tooltip("Tooltip header text")]
        public string header = "";

        [Tooltip("Tooltip description")]
        [TextArea(2, 5)]
        public string content = "Tooltip text...";

        [Header("Settings")]
        [Tooltip("Show delay (seconds)")]
        public float showDelay = 0.5f;

        [Tooltip("Tooltip position offset")]
        public Vector2 offset = new Vector2(0f, 20f);

        [Tooltip("Follow mouse")]
        public bool followMouse = true;

        // State
        private float hoverTime = 0f;
        private bool isHovering = false;

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            hoverTime = 0f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            hoverTime = 0f;

            if (TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.HideTooltip();
            }
        }

        private void Update()
        {
            if (!isHovering)
                return;

            hoverTime += Time.unscaledDeltaTime;

            if (hoverTime >= showDelay && TooltipSystem.Instance != null)
            {
                if (followMouse)
                {
                    TooltipSystem.Instance.ShowTooltipAtMouse(header, content, offset);
                }
                else
                {
                    TooltipSystem.Instance.ShowTooltipAtPosition(header, content, transform.position, offset);
                }
            }
        }

        private void OnDisable()
        {
            if (isHovering && TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.HideTooltip();
            }
        }
    }

    /// <summary>
    /// Contextual help entry
    /// </summary>
    [System.Serializable]
    public class ContextualHelp
    {
        public string helpID = "help_001";
        public string trigger = ""; // Event or condition
        public string header = "Tip";
        [TextArea(2, 4)]
        public string message = "Helpful message...";
        public float displayDuration = 5f;
        public bool showOnce = true;

        [System.NonSerialized]
        public bool hasShown = false;
    }

    /// <summary>
    /// Tooltip system singleton.
    /// Manages tooltips and contextual help messages.
    /// </summary>
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [Header("Tooltip UI")]
        [Tooltip("Tooltip panel")]
        public GameObject tooltipPanel;

        [Tooltip("Tooltip header text")]
        public TextMeshProUGUI headerText;

        [Tooltip("Tooltip content text")]
        public TextMeshProUGUI contentText;

        [Tooltip("Tooltip background")]
        public Image backgroundImage;

        [Tooltip("Layout element for dynamic sizing")]
        public LayoutElement layoutElement;

        [Header("Settings")]
        [Tooltip("Max tooltip width")]
        public float maxWidth = 400f;

        [Tooltip("Enable tooltips")]
        public bool tooltipsEnabled = true;

        [Tooltip("Fade duration")]
        public float fadeDuration = 0.2f;

        [Header("Contextual Help")]
        [Tooltip("Contextual help messages")]
        public List<ContextualHelp> contextualHelp = new List<ContextualHelp>();

        [Tooltip("Help message panel")]
        public GameObject helpMessagePanel;

        [Tooltip("Help message text")]
        public TextMeshProUGUI helpMessageText;

        [Tooltip("Help message header")]
        public TextMeshProUGUI helpMessageHeader;

        // State
        private RectTransform tooltipRect;
        private Canvas canvas;
        private bool isVisible = false;
        private List<string> shownHelpIDs = new List<string>();

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
            if (tooltipPanel != null)
            {
                tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                tooltipPanel.SetActive(false);
            }

            if (helpMessagePanel != null)
            {
                helpMessagePanel.SetActive(false);
            }

            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }
        }

        private void Update()
        {
            // Update tooltip position if following mouse
            if (isVisible && tooltipPanel != null && tooltipPanel.activeSelf)
            {
                UpdateTooltipPosition();
            }
        }

        /// <summary>
        /// Show tooltip at mouse position
        /// </summary>
        public void ShowTooltipAtMouse(string header, string content, Vector2 offset = default)
        {
            if (!tooltipsEnabled || tooltipPanel == null)
                return;

            SetTooltipText(header, content);

            tooltipPanel.SetActive(true);
            isVisible = true;

            // Position at mouse
            UpdateTooltipPosition(offset);

            // Fade in
            FadeIn();
        }

        /// <summary>
        /// Show tooltip at specific position
        /// </summary>
        public void ShowTooltipAtPosition(string header, string content, Vector3 worldPosition, Vector2 offset = default)
        {
            if (!tooltipsEnabled || tooltipPanel == null)
                return;

            SetTooltipText(header, content);

            tooltipPanel.SetActive(true);
            isVisible = true;

            // Convert world position to screen position
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);

            if (tooltipRect != null && canvas != null)
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPos,
                    canvas.worldCamera,
                    out localPos
                );

                tooltipRect.localPosition = localPos + offset;
            }

            // Fade in
            FadeIn();
        }

        /// <summary>
        /// Hide tooltip
        /// </summary>
        public void HideTooltip()
        {
            if (tooltipPanel == null)
                return;

            isVisible = false;

            // Fade out
            FadeOut(() =>
            {
                tooltipPanel.SetActive(false);
            });
        }

        /// <summary>
        /// Set tooltip text
        /// </summary>
        private void SetTooltipText(string header, string content)
        {
            // Header
            if (headerText != null)
            {
                if (!string.IsNullOrEmpty(header))
                {
                    headerText.text = header;
                    headerText.gameObject.SetActive(true);
                }
                else
                {
                    headerText.gameObject.SetActive(false);
                }
            }

            // Content
            if (contentText != null)
            {
                contentText.text = content;
            }

            // Update layout
            if (layoutElement != null)
            {
                int headerLength = header.Length;
                int contentLength = content.Length;

                if (headerLength > 50 || contentLength > 100)
                {
                    layoutElement.enabled = true;
                    layoutElement.preferredWidth = maxWidth;
                }
                else
                {
                    layoutElement.enabled = false;
                }
            }
        }

        /// <summary>
        /// Update tooltip position at mouse
        /// </summary>
        private void UpdateTooltipPosition(Vector2 offset = default)
        {
            if (tooltipRect == null || canvas == null)
                return;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out localPos
            );

            tooltipRect.localPosition = localPos + offset;

            // Keep tooltip on screen
            ClampToScreen();
        }

        /// <summary>
        /// Clamp tooltip to screen bounds
        /// </summary>
        private void Clamp ToScreen()
        {
            if (tooltipRect == null || canvas == null)
                return;

            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);

            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            // Check if tooltip goes off screen
            Vector2 offset = Vector2.zero;

            // Right edge
            if (corners[2].x > canvasCorners[2].x)
            {
                offset.x = canvasCorners[2].x - corners[2].x;
            }

            // Left edge
            if (corners[0].x < canvasCorners[0].x)
            {
                offset.x = canvasCorners[0].x - corners[0].x;
            }

            // Top edge
            if (corners[2].y > canvasCorners[2].y)
            {
                offset.y = canvasCorners[2].y - corners[2].y;
            }

            // Bottom edge
            if (corners[0].y < canvasCorners[0].y)
            {
                offset.y = canvasCorners[0].y - corners[0].y;
            }

            tooltipRect.localPosition += (Vector3)offset;
        }

        /// <summary>
        /// Show contextual help message
        /// </summary>
        public void ShowContextualHelp(string helpID)
        {
            ContextualHelp help = contextualHelp.Find(h => h.helpID == helpID);

            if (help == null)
            {
                Debug.LogWarning($"[TooltipSystem] Help '{helpID}' not found!");
                return;
            }

            // Check if already shown
            if (help.showOnce && (help.hasShown || shownHelpIDs.Contains(helpID)))
                return;

            ShowHelpMessage(help.header, help.message, help.displayDuration);

            help.hasShown = true;
            if (!shownHelpIDs.Contains(helpID))
            {
                shownHelpIDs.Add(helpID);
            }
        }

        /// <summary>
        /// Show help message panel
        /// </summary>
        public void ShowHelpMessage(string header, string message, float duration = 5f)
        {
            if (helpMessagePanel == null)
                return;

            // Set text
            if (helpMessageHeader != null)
            {
                helpMessageHeader.text = header;
            }

            if (helpMessageText != null)
            {
                helpMessageText.text = message;
            }

            // Show panel
            helpMessagePanel.SetActive(true);

            // Auto-hide after duration
            CancelInvoke(nameof(HideHelpMessage));
            Invoke(nameof(HideHelpMessage), duration);

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("help_message");
            }
        }

        /// <summary>
        /// Hide help message panel
        /// </summary>
        public void HideHelpMessage()
        {
            if (helpMessagePanel != null)
            {
                helpMessagePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Trigger contextual help by event
        /// </summary>
        public void TriggerHelpEvent(string eventName)
        {
            foreach (var help in contextualHelp)
            {
                if (help.trigger == eventName)
                {
                    ShowContextualHelp(help.helpID);
                }
            }
        }

        /// <summary>
        /// Fade in tooltip
        /// </summary>
        private void FadeIn()
        {
            if (backgroundImage == null)
                return;

            CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
            }

            LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration).setIgnoreTimeScale(true);
        }

        /// <summary>
        /// Fade out tooltip
        /// </summary>
        private void FadeOut(System.Action onComplete = null)
        {
            if (tooltipPanel == null)
            {
                onComplete?.Invoke();
                return;
            }

            CanvasGroup canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();
            }

            LeanTween.alphaCanvas(canvasGroup, 0f, fadeDuration)
                .setIgnoreTimeScale(true)
                .setOnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Enable/disable tooltips
        /// </summary>
        public void SetTooltipsEnabled(bool enabled)
        {
            tooltipsEnabled = enabled;

            if (!enabled && isVisible)
            {
                HideTooltip();
            }
        }

        /// <summary>
        /// Reset shown help messages
        /// </summary>
        public void ResetHelpMessages()
        {
            shownHelpIDs.Clear();

            foreach (var help in contextualHelp)
            {
                help.hasShown = false;
            }
        }
    }
}
