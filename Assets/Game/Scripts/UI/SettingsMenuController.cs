using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyGame.UI
{
    /// <summary>
    /// Settings menu controller.
    /// Manages audio, graphics, and control settings.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Audio Settings")]
        [Tooltip("Master volume slider")]
        public Slider masterVolumeSlider;

        [Tooltip("Music volume slider")]
        public Slider musicVolumeSlider;

        [Tooltip("SFX volume slider")]
        public Slider sfxVolumeSlider;

        [Tooltip("Master volume text")]
        public TextMeshProUGUI masterVolumeText;

        [Tooltip("Music volume text")]
        public TextMeshProUGUI musicVolumeText;

        [Tooltip("SFX volume text")]
        public TextMeshProUGUI sfxVolumeText;

        [Header("Graphics Settings")]
        [Tooltip("Quality dropdown")]
        public TMP_Dropdown qualityDropdown;

        [Tooltip("Resolution dropdown")]
        public TMP_Dropdown resolutionDropdown;

        [Tooltip("Fullscreen toggle")]
        public Toggle fullscreenToggle;

        [Tooltip("VSync toggle")]
        public Toggle vsyncToggle;

        [Header("Controls Settings")]
        [Tooltip("Mouse sensitivity slider")]
        public Slider mouseSensitivitySlider;

        [Tooltip("Mouse sensitivity text")]
        public TextMeshProUGUI mouseSensitivityText;

        [Tooltip("Invert Y toggle")]
        public Toggle invertYToggle;

        [Header("Buttons")]
        [Tooltip("Back button")]
        public Button backButton;

        [Tooltip("Apply button")]
        public Button applyButton;

        [Tooltip("Reset to defaults button")]
        public Button resetButton;

        [Header("Audio")]
        [Tooltip("Button click sound")]
        public string buttonClickSound = "button_click";

        private System.Action backCallback;
        private Resolution[] resolutions;

        private void Start()
        {
            Initialize();
            LoadSettings();

            // Setup button listeners
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }

            if (applyButton != null)
            {
                applyButton.onClick.AddListener(OnApplyClicked);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }

            // Setup slider listeners
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            }

            // Setup dropdown listeners
            if (qualityDropdown != null)
            {
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (resolutionDropdown != null)
            {
                resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            }

            // Setup toggle listeners
            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            }

            if (vsyncToggle != null)
            {
                vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }

            if (invertYToggle != null)
            {
                invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
            }
        }

        /// <summary>
        /// Initialize settings UI
        /// </summary>
        private void Initialize()
        {
            // Populate quality dropdown
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
            }

            // Populate resolution dropdown
            if (resolutionDropdown != null)
            {
                resolutions = Screen.resolutions;
                resolutionDropdown.ClearOptions();

                System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();
                int currentResolutionIndex = 0;

                for (int i = 0; i < resolutions.Length; i++)
                {
                    string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "Hz";
                    options.Add(option);

                    if (resolutions[i].width == Screen.currentResolution.width &&
                        resolutions[i].height == Screen.currentResolution.height)
                    {
                        currentResolutionIndex = i;
                    }
                }

                resolutionDropdown.AddOptions(options);
                resolutionDropdown.value = currentResolutionIndex;
                resolutionDropdown.RefreshShownValue();
            }
        }

        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        private void LoadSettings()
        {
            // Audio settings
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

            if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;

            UpdateVolumeTexts();

            // Graphics settings
            if (qualityDropdown != null) qualityDropdown.value = QualitySettings.GetQualityLevel();
            if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
            if (vsyncToggle != null) vsyncToggle.isOn = QualitySettings.vSyncCount > 0;

            // Controls settings
            float mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 3f);
            bool invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;

            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = mouseSensitivity;
            if (invertYToggle != null) invertYToggle.isOn = invertY;

            UpdateMouseSensitivityText();
        }

        /// <summary>
        /// Save settings to PlayerPrefs
        /// </summary>
        private void SaveSettings()
        {
            // Audio settings
            if (masterVolumeSlider != null)
                PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);

            if (musicVolumeSlider != null)
                PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

            if (sfxVolumeSlider != null)
                PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);

            // Controls settings
            if (mouseSensitivitySlider != null)
                PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivitySlider.value);

            if (invertYToggle != null)
                PlayerPrefs.SetInt("InvertY", invertYToggle.isOn ? 1 : 0);

            PlayerPrefs.Save();
        }

        /// <summary>
        /// Master volume changed
        /// </summary>
        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }

            UpdateVolumeTexts();
        }

        /// <summary>
        /// Music volume changed
        /// </summary>
        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }

            UpdateVolumeTexts();
        }

        /// <summary>
        /// SFX volume changed
        /// </summary>
        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);
            }

            UpdateVolumeTexts();
        }

        /// <summary>
        /// Update volume text displays
        /// </summary>
        private void UpdateVolumeTexts()
        {
            if (masterVolumeText != null && masterVolumeSlider != null)
                masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";

            if (musicVolumeText != null && musicVolumeSlider != null)
                musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";

            if (sfxVolumeText != null && sfxVolumeSlider != null)
                sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";
        }

        /// <summary>
        /// Quality level changed
        /// </summary>
        private void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index);
        }

        /// <summary>
        /// Resolution changed
        /// </summary>
        private void OnResolutionChanged(int index)
        {
            if (resolutions != null && index < resolutions.Length)
            {
                Resolution resolution = resolutions[index];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            }
        }

        /// <summary>
        /// Fullscreen toggle changed
        /// </summary>
        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        /// <summary>
        /// VSync toggle changed
        /// </summary>
        private void OnVSyncChanged(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
        }

        /// <summary>
        /// Mouse sensitivity changed
        /// </summary>
        private void OnMouseSensitivityChanged(float value)
        {
            // Apply to camera controller if in game
            CameraController cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null)
            {
                cam.mouseSensitivity = value;
            }

            UpdateMouseSensitivityText();
        }

        /// <summary>
        /// Invert Y toggle changed
        /// </summary>
        private void OnInvertYChanged(bool inverted)
        {
            // Apply to camera controller if in game
            CameraController cam = Camera.main?.GetComponent<CameraController>();
            if (cam != null)
            {
                cam.invertY = inverted;
            }
        }

        /// <summary>
        /// Update mouse sensitivity text
        /// </summary>
        private void UpdateMouseSensitivityText()
        {
            if (mouseSensitivityText != null && mouseSensitivitySlider != null)
            {
                mouseSensitivityText.text = mouseSensitivitySlider.value.ToString("F1");
            }
        }

        /// <summary>
        /// Back button clicked
        /// </summary>
        private void OnBackClicked()
        {
            PlayButtonSound();
            SaveSettings();
            backCallback?.Invoke();
        }

        /// <summary>
        /// Apply button clicked
        /// </summary>
        private void OnApplyClicked()
        {
            PlayButtonSound();
            SaveSettings();
        }

        /// <summary>
        /// Reset to defaults button clicked
        /// </summary>
        private void OnResetClicked()
        {
            PlayButtonSound();

            // Reset to default values
            if (masterVolumeSlider != null) masterVolumeSlider.value = 1f;
            if (musicVolumeSlider != null) musicVolumeSlider.value = 0.8f;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = 1f;
            if (qualityDropdown != null) qualityDropdown.value = 2; // Medium quality
            if (fullscreenToggle != null) fullscreenToggle.isOn = true;
            if (vsyncToggle != null) vsyncToggle.isOn = true;
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = 3f;
            if (invertYToggle != null) invertYToggle.isOn = false;

            SaveSettings();
        }

        /// <summary>
        /// Set callback for back button
        /// </summary>
        public void SetBackCallback(System.Action callback)
        {
            backCallback = callback;
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
