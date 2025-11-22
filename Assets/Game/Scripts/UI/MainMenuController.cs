using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyGame.UI
{
    /// <summary>
    /// Main menu UI controller.
    /// Handles main menu buttons and navigation.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [Tooltip("Main menu panel")]
        public GameObject mainMenuPanel;

        [Tooltip("Settings menu panel")]
        public GameObject settingsPanel;

        [Tooltip("Credits panel")]
        public GameObject creditsPanel;

        [Header("Buttons")]
        [Tooltip("New Game button")]
        public Button newGameButton;

        [Tooltip("Continue button")]
        public Button continueButton;

        [Tooltip("Settings button")]
        public Button settingsButton;

        [Tooltip("Credits button")]
        public Button creditsButton;

        [Tooltip("Quit button")]
        public Button quitButton;

        [Header("UI Elements")]
        [Tooltip("Game title text")]
        public TextMeshProUGUI titleText;

        [Tooltip("Version text")]
        public TextMeshProUGUI versionText;

        [Header("Audio")]
        [Tooltip("Button click sound")]
        public string buttonClickSound = "button_click";

        [Tooltip("Menu music")]
        public AudioClip menuMusic;

        private SettingsMenuController settingsController;

        private void Start()
        {
            // Setup button listeners
            if (newGameButton != null)
            {
                newGameButton.onClick.AddListener(OnNewGameClicked);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);

                // Disable continue button if no save exists
                continueButton.interactable = GameManager.Instance != null && GameManager.Instance.HasSaveData();
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            // Get settings controller
            if (settingsPanel != null)
            {
                settingsController = settingsPanel.GetComponent<SettingsMenuController>();
            }

            // Set version text
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }

            // Play menu music
            if (menuMusic != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusic(menuMusic);
            }

            // Show main menu panel
            ShowMainMenu();
        }

        /// <summary>
        /// Show main menu panel
        /// </summary>
        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        /// <summary>
        /// New Game button clicked
        /// </summary>
        private void OnNewGameClicked()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogWarning("[MainMenuController] GameManager not found!");
            }
        }

        /// <summary>
        /// Continue button clicked
        /// </summary>
        private void OnContinueClicked()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ContinueGame();
            }
            else
            {
                Debug.LogWarning("[MainMenuController] GameManager not found!");
            }
        }

        /// <summary>
        /// Settings button clicked
        /// </summary>
        private void OnSettingsClicked()
        {
            PlayButtonSound();

            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);

            if (settingsController != null)
            {
                settingsController.SetBackCallback(ShowMainMenu);
            }
        }

        /// <summary>
        /// Credits button clicked
        /// </summary>
        private void OnCreditsClicked()
        {
            PlayButtonSound();

            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);
        }

        /// <summary>
        /// Quit button clicked
        /// </summary>
        private void OnQuitClicked()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }

        /// <summary>
        /// Back from credits
        /// </summary>
        public void OnBackFromCredits()
        {
            PlayButtonSound();
            ShowMainMenu();
        }

        /// <summary>
        /// Play button click sound
        /// </summary>
        private void PlayButtonSound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(buttonClickSound);
            }
        }

        /// <summary>
        /// Update continue button availability
        /// </summary>
        public void RefreshContinueButton()
        {
            if (continueButton != null && GameManager.Instance != null)
            {
                continueButton.interactable = GameManager.Instance.HasSaveData();
            }
        }
    }
}
