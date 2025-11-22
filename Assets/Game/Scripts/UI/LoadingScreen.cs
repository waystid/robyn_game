using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyGame
{
    /// <summary>
    /// Loading screen UI controller.
    /// Shows progress bar and loading tips during scene transitions.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Main loading panel")]
        public GameObject loadingPanel;

        [Tooltip("Progress bar fill image")]
        public Image progressBar;

        [Tooltip("Progress text (percentage)")]
        public TextMeshProUGUI progressText;

        [Tooltip("Loading tip text")]
        public TextMeshProUGUI tipText;

        [Tooltip("Spinner/animation object")]
        public GameObject spinner;

        [Header("Fade Settings")]
        [Tooltip("Canvas group for fading")]
        public CanvasGroup canvasGroup;

        [Tooltip("Fade in duration")]
        public float fadeInDuration = 0.3f;

        [Tooltip("Fade out duration")]
        public float fadeOutDuration = 0.3f;

        [Header("Loading Tips")]
        [Tooltip("List of loading tips to display")]
        public string[] loadingTips = new string[]
        {
            "Tip: Talk to the Dragon for quests!",
            "Tip: Answer the Sphinx's riddles for rewards!",
            "Tip: Water plants regularly for faster growth!",
            "Tip: Cast spells to speed up your tasks!",
            "Tip: Explore the forest to find rare items!",
            "Tip: Complete quests to level up!",
            "Tip: Harvest plants when they're fully grown!",
            "Tip: Hold Shift to run faster!",
            "Tip: Press E to interact with objects!",
            "Tip: Manage your mana wisely when casting spells!"
        };

        [Header("Spinner Animation")]
        [Tooltip("Spinner rotation speed (degrees per second)")]
        public float spinnerSpeed = 180f;

        private Coroutine fadeCoroutine;

        private void Awake()
        {
            // Ensure canvas group exists
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Start hidden
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            // Rotate spinner
            if (spinner != null && spinner.activeSelf)
            {
                spinner.transform.Rotate(0f, 0f, -spinnerSpeed * Time.unscaledDeltaTime);
            }
        }

        /// <summary>
        /// Show loading screen with fade in
        /// </summary>
        public void Show()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }

            // Show random tip
            ShowRandomTip();

            // Reset progress
            SetProgress(0f);

            // Fade in
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeIn());
        }

        /// <summary>
        /// Hide loading screen with fade out
        /// </summary>
        public void Hide(System.Action onComplete = null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeOut(onComplete));
        }

        /// <summary>
        /// Set loading progress (0-1)
        /// </summary>
        public void SetProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);

            // Update progress bar
            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }

            // Update progress text
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// Show a random loading tip
        /// </summary>
        private void ShowRandomTip()
        {
            if (tipText != null && loadingTips != null && loadingTips.Length > 0)
            {
                int randomIndex = Random.Range(0, loadingTips.Length);
                tipText.text = loadingTips[randomIndex];
            }
        }

        /// <summary>
        /// Fade in coroutine
        /// </summary>
        private IEnumerator FadeIn()
        {
            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            fadeCoroutine = null;
        }

        /// <summary>
        /// Fade out coroutine
        /// </summary>
        private IEnumerator FadeOut(System.Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;

            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            onComplete?.Invoke();

            fadeCoroutine = null;
        }

        /// <summary>
        /// Set custom loading tip
        /// </summary>
        public void SetTip(string tip)
        {
            if (tipText != null)
            {
                tipText.text = tip;
            }
        }
    }
}
