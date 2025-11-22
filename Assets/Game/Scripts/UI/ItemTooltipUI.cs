using UnityEngine;
using TMPro;
using CozyGame.Inventory;

namespace CozyGame.UI
{
    /// <summary>
    /// Item tooltip UI singleton.
    /// Shows item information on hover.
    /// </summary>
    public class ItemTooltipUI : MonoBehaviour
    {
        public static ItemTooltipUI Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("Tooltip panel")]
        public GameObject tooltipPanel;

        [Tooltip("Tooltip text")]
        public TextMeshProUGUI tooltipText;

        [Header("Settings")]
        [Tooltip("Offset from cursor")]
        public Vector2 offset = new Vector2(10, -10);

        [Tooltip("Follow cursor")]
        public bool followCursor = true;

        private Canvas canvas;
        private RectTransform canvasRect;
        private RectTransform tooltipRect;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            canvas = GetComponentInParent<Canvas>();
            canvasRect = canvas.GetComponent<RectTransform>();
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();

            HideTooltip();
        }

        private void Update()
        {
            if (tooltipPanel.activeSelf && followCursor)
            {
                UpdatePosition(Input.mousePosition);
            }
        }

        /// <summary>
        /// Show tooltip for item
        /// </summary>
        public void ShowTooltip(Item item, Vector3 position)
        {
            if (item == null)
            {
                HideTooltip();
                return;
            }

            // Set tooltip text
            if (tooltipText != null)
            {
                tooltipText.text = item.GetTooltipText();
            }

            // Show panel
            tooltipPanel.SetActive(true);

            // Update position
            UpdatePosition(position);
        }

        /// <summary>
        /// Show tooltip with custom text
        /// </summary>
        public void ShowTooltip(string text, Vector3 position)
        {
            if (string.IsNullOrEmpty(text))
            {
                HideTooltip();
                return;
            }

            // Set tooltip text
            if (tooltipText != null)
            {
                tooltipText.text = text;
            }

            // Show panel
            tooltipPanel.SetActive(true);

            // Update position
            UpdatePosition(position);
        }

        /// <summary>
        /// Hide tooltip
        /// </summary>
        public void HideTooltip()
        {
            tooltipPanel.SetActive(false);
        }

        /// <summary>
        /// Update tooltip position
        /// </summary>
        private void UpdatePosition(Vector3 screenPosition)
        {
            if (tooltipRect == null || canvasRect == null)
                return;

            // Convert screen position to canvas position
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                canvas.worldCamera,
                out localPosition
            );

            // Apply offset
            localPosition += offset;

            // Clamp to canvas bounds
            Vector2 sizeDelta = tooltipRect.sizeDelta;
            Vector2 canvasSize = canvasRect.sizeDelta;

            float halfWidth = sizeDelta.x * 0.5f;
            float halfHeight = sizeDelta.y * 0.5f;

            localPosition.x = Mathf.Clamp(localPosition.x, -canvasSize.x * 0.5f + halfWidth, canvasSize.x * 0.5f - halfWidth);
            localPosition.y = Mathf.Clamp(localPosition.y, -canvasSize.y * 0.5f + halfHeight, canvasSize.y * 0.5f - halfHeight);

            // Set position
            tooltipRect.anchoredPosition = localPosition;
        }
    }
}
