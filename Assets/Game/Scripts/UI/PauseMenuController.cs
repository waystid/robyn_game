using UnityEngine;
using UnityEngine.UI;

namespace CozyGame.UI
{
    /// <summary>
    /// Pause menu UI controller.
    /// Handles pause menu buttons and navigation.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        [Tooltip("Pause menu panel")]
        public GameObject pauseMenuPanel;

        [Tooltip("Settings panel")]
        public GameObject settingsPanel;

        [Header("Buttons")]
        [Tooltip("Resume button")]
        public Button resumeButton;

        [Tooltip("Settings button")]
        public Button settingsButton;

        [Tooltip("Restart button")]
        public Button restartButton;

        [Tooltip("Main Menu button")]
        public Button mainMenuButton;

        [Tooltip("Quit button (optional, for desktop)")]
        public Button quitButton;

        [Header("Confirmation Dialogs")]
        [Tooltip("Restart confirmation dialog")]
        public GameObject restartConfirmDialog;

        [Tooltip("Main menu confirmation dialog")]
        public GameObject mainMenuConfirmDialog;

        [Tooltip("Quit confirmation dialog")]
        public GameObject quitConfirmDialog;

        [Header("Audio")]
        [Tooltip("Button click sound")]
        public string buttonClickSound = "button_click";

        [Tooltip("Pause sound")]
        public string pauseSound = "pause";

        [Tooltip("Resume sound")]
        public string resumeSound = "unpause";

        private SettingsMenuController settingsController;

        private void Awake()
        {
            // Setup button listeners
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
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

            // Hide all panels initially
            HideAll();
        }

        private void Start()
        {
            // Subscribe to game manager events
            if (GameManager.Instance != null)
            {
                GameManager.OnGamePaused += OnGamePaused;
                GameManager.OnGameResumed += OnGameResumed;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameManager.Instance != null)
            {
                GameManager.OnGamePaused -= OnGamePaused;
                GameManager.OnGameResumed -= OnGameResumed;
            }
        }

        /// <summary>
        /// Called when game is paused
        /// </summary>
        private void OnGamePaused()
        {
            ShowPauseMenu();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(pauseSound);
            }
        }

        /// <summary>
        /// Called when game is resumed
        /// </summary>
        private void OnGameResumed()
        {
            HideAll();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(resumeSound);
            }
        }

        /// <summary>
        /// Show pause menu panel
        /// </summary>
        private void ShowPauseMenu()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            HideAllConfirmDialogs();
        }

        /// <summary>
        /// Hide all panels
        /// </summary>
        private void HideAll()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            HideAllConfirmDialogs();
        }

        /// <summary>
        /// Hide all confirmation dialogs
        /// </summary>
        private void HideAllConfirmDialogs()
        {
            if (restartConfirmDialog != null) restartConfirmDialog.SetActive(false);
            if (mainMenuConfirmDialog != null) mainMenuConfirmDialog.SetActive(false);
            if (quitConfirmDialog != null) quitConfirmDialog.SetActive(false);
        }

        /// <summary>
        /// Resume button clicked
        /// </summary>
        private void OnResumeClicked()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        /// <summary>
        /// Settings button clicked
        /// </summary>
        private void OnSettingsClicked()
        {
            PlayButtonSound();

            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);

            if (settingsController != null)
            {
                settingsController.SetBackCallback(ShowPauseMenu);
            }
        }

        /// <summary>
        /// Restart button clicked
        /// </summary>
        private void OnRestartClicked()
        {
            PlayButtonSound();

            if (restartConfirmDialog != null)
            {
                restartConfirmDialog.SetActive(true);
            }
            else
            {
                ConfirmRestart();
            }
        }

        /// <summary>
        /// Main Menu button clicked
        /// </summary>
        private void OnMainMenuClicked()
        {
            PlayButtonSound();

            if (mainMenuConfirmDialog != null)
            {
                mainMenuConfirmDialog.SetActive(true);
            }
            else
            {
                ConfirmMainMenu();
            }
        }

        /// <summary>
        /// Quit button clicked
        /// </summary>
        private void OnQuitClicked()
        {
            PlayButtonSound();

            if (quitConfirmDialog != null)
            {
                quitConfirmDialog.SetActive(true);
            }
            else
            {
                ConfirmQuit();
            }
        }

        /// <summary>
        /// Confirm restart (called by confirmation dialog)
        /// </summary>
        public void ConfirmRestart()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        /// <summary>
        /// Confirm return to main menu (called by confirmation dialog)
        /// </summary>
        public void ConfirmMainMenu()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }

        /// <summary>
        /// Confirm quit (called by confirmation dialog)
        /// </summary>
        public void ConfirmQuit()
        {
            PlayButtonSound();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }

        /// <summary>
        /// Cancel confirmation dialog
        /// </summary>
        public void CancelConfirmation()
        {
            PlayButtonSound();
            HideAllConfirmDialogs();
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
    }
}
