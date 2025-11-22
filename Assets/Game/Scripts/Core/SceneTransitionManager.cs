using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CozyGame
{
    /// <summary>
    /// Manages scene loading and transitions with loading screens and callbacks.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        [Tooltip("Fade duration in seconds")]
        public float fadeDuration = 0.5f;

        [Tooltip("Minimum loading screen display time")]
        public float minLoadingTime = 1f;

        [Tooltip("Loading screen prefab (optional)")]
        public GameObject loadingScreenPrefab;

        [Header("Audio")]
        [Tooltip("Play sound on scene transition")]
        public bool playTransitionSound = true;

        [Tooltip("Transition sound name")]
        public string transitionSoundName = "scene_transition";

        // Loading state
        private bool isLoading = false;
        private LoadingScreen currentLoadingScreen;

        // Events
        public static event System.Action<string> OnSceneLoadStart;
        public static event System.Action<string> OnSceneLoadComplete;
        public static event System.Action<float> OnLoadProgressChanged;

        private void Awake()
        {
            // Singleton setup
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

        /// <summary>
        /// Load a scene by name with loading screen
        /// </summary>
        public void LoadScene(string sceneName, System.Action onComplete = null)
        {
            if (isLoading)
            {
                Debug.LogWarning("[SceneTransitionManager] Already loading a scene!");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName, onComplete));
        }

        /// <summary>
        /// Load scene asynchronously with loading screen
        /// </summary>
        private IEnumerator LoadSceneCoroutine(string sceneName, System.Action onComplete)
        {
            isLoading = true;

            Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");

            OnSceneLoadStart?.Invoke(sceneName);

            // Play transition sound
            if (playTransitionSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(transitionSoundName);
            }

            // Show loading screen
            ShowLoadingScreen();

            // Small delay to ensure loading screen is visible
            yield return new WaitForSeconds(0.1f);

            float startTime = Time.realtimeSinceStartup;

            // Start loading scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                // Progress goes from 0 to 0.9 while loading
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

                OnLoadProgressChanged?.Invoke(progress);

                if (currentLoadingScreen != null)
                {
                    currentLoadingScreen.SetProgress(progress);
                }

                // Scene is loaded, waiting for activation
                if (asyncLoad.progress >= 0.9f)
                {
                    // Ensure minimum loading time for smooth experience
                    float elapsedTime = Time.realtimeSinceStartup - startTime;
                    if (elapsedTime < minLoadingTime)
                    {
                        yield return new WaitForSecondsRealtime(minLoadingTime - elapsedTime);
                    }

                    // Activate scene
                    asyncLoad.allowSceneActivation = true;
                }

                yield return null;
            }

            // Scene loaded
            OnLoadProgressChanged?.Invoke(1f);

            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.SetProgress(1f);
            }

            // Small delay before hiding loading screen
            yield return new WaitForSecondsRealtime(0.2f);

            // Hide loading screen
            HideLoadingScreen();

            OnSceneLoadComplete?.Invoke(sceneName);

            // Invoke callback
            onComplete?.Invoke();

            isLoading = false;

            Debug.Log($"[SceneTransitionManager] Scene loaded: {sceneName}");
        }

        /// <summary>
        /// Load scene additively
        /// </summary>
        public void LoadSceneAdditive(string sceneName, System.Action onComplete = null)
        {
            StartCoroutine(LoadSceneAdditiveCoroutine(sceneName, onComplete));
        }

        /// <summary>
        /// Load scene additively (for multiple scenes at once)
        /// </summary>
        private IEnumerator LoadSceneAdditiveCoroutine(string sceneName, System.Action onComplete)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// Unload a scene by name
        /// </summary>
        public void UnloadScene(string sceneName, System.Action onComplete = null)
        {
            StartCoroutine(UnloadSceneCoroutine(sceneName, onComplete));
        }

        /// <summary>
        /// Unload scene coroutine
        /// </summary>
        private IEnumerator UnloadSceneCoroutine(string sceneName, System.Action onComplete)
        {
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);

            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            onComplete?.Invoke();
        }

        /// <summary>
        /// Reload current scene
        /// </summary>
        public void ReloadCurrentScene(System.Action onComplete = null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            LoadScene(currentScene, onComplete);
        }

        /// <summary>
        /// Show loading screen
        /// </summary>
        private void ShowLoadingScreen()
        {
            if (loadingScreenPrefab != null)
            {
                GameObject loadingScreenObj = Instantiate(loadingScreenPrefab);
                currentLoadingScreen = loadingScreenObj.GetComponent<LoadingScreen>();

                if (currentLoadingScreen != null)
                {
                    currentLoadingScreen.Show();
                }
            }
            else
            {
                // Try to find existing loading screen in scene
                currentLoadingScreen = FindObjectOfType<LoadingScreen>();

                if (currentLoadingScreen != null)
                {
                    currentLoadingScreen.Show();
                }
            }
        }

        /// <summary>
        /// Hide loading screen
        /// </summary>
        private void HideLoadingScreen()
        {
            if (currentLoadingScreen != null)
            {
                currentLoadingScreen.Hide(() =>
                {
                    if (loadingScreenPrefab != null)
                    {
                        // Destroy instantiated loading screen
                        Destroy(currentLoadingScreen.gameObject);
                    }

                    currentLoadingScreen = null;
                });
            }
        }

        /// <summary>
        /// Check if currently loading
        /// </summary>
        public bool IsLoading()
        {
            return isLoading;
        }

        /// <summary>
        /// Get current active scene name
        /// </summary>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// Check if a scene is loaded
        /// </summary>
        public bool IsSceneLoaded(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.isLoaded;
        }
    }
}
