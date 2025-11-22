using UnityEngine;
using UnityEngine.SceneManagement;

namespace CozyGame
{
    /// <summary>
    /// Main game manager singleton.
    /// Manages overall game state, scene transitions, and core game systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene Names")]
        [Tooltip("Name of the main menu scene")]
        public string mainMenuSceneName = "MainMenu";

        [Tooltip("Name of the gameplay scene")]
        public string gameplaySceneName = "Gameplay";

        [Header("Game Settings")]
        [Tooltip("Target frame rate (0 = unlimited)")]
        public int targetFrameRate = 60;

        [Tooltip("Enable VSync")]
        public bool enableVSync = true;

        [Header("Startup")]
        [Tooltip("Skip main menu and start game directly (for testing)")]
        public bool skipMainMenu = false;

        [Tooltip("Auto-load save on start")]
        public bool autoLoadSave = false;

        // Game state
        private GameState currentGameState = GameState.MainMenu;
        private bool isGamePaused = false;
        private float timeScale = 1f;

        // Events
        public static event System.Action<GameState> OnGameStateChanged;
        public static event System.Action OnGamePaused;
        public static event System.Action OnGameResumed;
        public static event System.Action OnGameStarted;
        public static event System.Action OnGameEnded;

        private void Awake()
        {
            // Singleton setup with DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Handle startup based on settings
            if (skipMainMenu)
            {
                StartNewGame();
            }
        }

        /// <summary>
        /// Initialize game manager
        /// </summary>
        private void Initialize()
        {
            // Set target frame rate
            Application.targetFrameRate = targetFrameRate;

            // Set VSync
            QualitySettings.vSyncCount = enableVSync ? 1 : 0;

            // Initialize cursor
            SetCursorState(true, CursorLockMode.None);

            Debug.Log("[GameManager] Initialized");
        }

        /// <summary>
        /// Start a new game
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("[GameManager] Starting new game...");

            ChangeGameState(GameState.Loading);

            // Load gameplay scene
            SceneTransitionManager.Instance?.LoadScene(gameplaySceneName, () =>
            {
                OnNewGameStarted();
            });
        }

        /// <summary>
        /// Called when new game scene is loaded
        /// </summary>
        private void OnNewGameStarted()
        {
            ChangeGameState(GameState.Playing);

            // Initialize player stats
            if (PlayerStats.Instance != null)
            {
                // Reset to starting values
                PlayerStats.Instance.currentHealth = PlayerStats.Instance.maxHealth;
                PlayerStats.Instance.currentMana = PlayerStats.Instance.maxMana;
                PlayerStats.Instance.currentStamina = PlayerStats.Instance.maxStamina;
                PlayerStats.Instance.level = 1;
                PlayerStats.Instance.currentExp = 0;
            }

            // Set cursor for gameplay
            SetCursorState(false, CursorLockMode.Locked);

            OnGameStarted?.Invoke();

            Debug.Log("[GameManager] New game started!");
        }

        /// <summary>
        /// Continue from saved game
        /// </summary>
        public void ContinueGame()
        {
            Debug.Log("[GameManager] Continuing game...");

            ChangeGameState(GameState.Loading);

            // Load gameplay scene
            SceneTransitionManager.Instance?.LoadScene(gameplaySceneName, () =>
            {
                OnGameContinued();
            });
        }

        /// <summary>
        /// Called when continuing saved game
        /// </summary>
        private void OnGameContinued()
        {
            ChangeGameState(GameState.Playing);

            // TODO: Load save data here when save system is implemented
            // SaveSystem.LoadGame();

            SetCursorState(false, CursorLockMode.Locked);

            OnGameStarted?.Invoke();

            Debug.Log("[GameManager] Game continued!");
        }

        /// <summary>
        /// Return to main menu
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("[GameManager] Returning to main menu...");

            ChangeGameState(GameState.Loading);

            // Unpause game before loading
            if (isGamePaused)
            {
                ResumeGame();
            }

            // Load main menu scene
            SceneTransitionManager.Instance?.LoadScene(mainMenuSceneName, () =>
            {
                OnMainMenuLoaded();
            });
        }

        /// <summary>
        /// Called when main menu is loaded
        /// </summary>
        private void OnMainMenuLoaded()
        {
            ChangeGameState(GameState.MainMenu);

            SetCursorState(true, CursorLockMode.None);

            Debug.Log("[GameManager] Main menu loaded");
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (isGamePaused || currentGameState != GameState.Playing)
                return;

            isGamePaused = true;
            timeScale = Time.timeScale;
            Time.timeScale = 0f;

            SetCursorState(true, CursorLockMode.None);

            // Disable player controls
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.SetControlEnabled(false);
            }

            OnGamePaused?.Invoke();

            Debug.Log("[GameManager] Game paused");
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (!isGamePaused)
                return;

            isGamePaused = false;
            Time.timeScale = timeScale;

            SetCursorState(false, CursorLockMode.Locked);

            // Re-enable player controls
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.SetControlEnabled(true);
            }

            OnGameResumed?.Invoke();

            Debug.Log("[GameManager] Game resumed");
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            if (isGamePaused)
                ResumeGame();
            else
                PauseGame();
        }

        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        /// <summary>
        /// Change game state
        /// </summary>
        private void ChangeGameState(GameState newState)
        {
            if (currentGameState == newState)
                return;

            GameState previousState = currentGameState;
            currentGameState = newState;

            OnGameStateChanged?.Invoke(newState);

            Debug.Log($"[GameManager] Game state changed: {previousState} -> {newState}");
        }

        /// <summary>
        /// Set cursor visibility and lock state
        /// </summary>
        public void SetCursorState(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;
        }

        /// <summary>
        /// Get current game state
        /// </summary>
        public GameState GetGameState()
        {
            return currentGameState;
        }

        /// <summary>
        /// Check if game is paused
        /// </summary>
        public bool IsGamePaused()
        {
            return isGamePaused;
        }

        /// <summary>
        /// Check if currently in gameplay
        /// </summary>
        public bool IsPlaying()
        {
            return currentGameState == GameState.Playing && !isGamePaused;
        }

        /// <summary>
        /// Check if a save file exists
        /// </summary>
        public bool HasSaveData()
        {
            // TODO: Implement when save system is added
            // return SaveSystem.HasSaveFile();
            return false; // Placeholder
        }

        /// <summary>
        /// Restart current level
        /// </summary>
        public void RestartLevel()
        {
            Debug.Log("[GameManager] Restarting level...");

            ChangeGameState(GameState.Loading);

            string currentScene = SceneManager.GetActiveScene().name;

            SceneTransitionManager.Instance?.LoadScene(currentScene, () =>
            {
                ChangeGameState(GameState.Playing);
                SetCursorState(false, CursorLockMode.Locked);
            });
        }

        private void Update()
        {
            // Handle pause input
            if (currentGameState == GameState.Playing)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TogglePause();
                }
            }
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[GameManager] Application quitting...");
            OnGameEnded?.Invoke();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            // Auto-pause when losing focus (optional)
            if (!hasFocus && currentGameState == GameState.Playing)
            {
                // Uncomment to enable auto-pause on focus loss
                // PauseGame();
            }
        }
    }

    /// <summary>
    /// Game state enum
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        GameOver
    }
}
