# Menu Setup Guide

Complete guide for setting up main menu, pause menu, settings, and loading screens.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Scene Setup](#scene-setup)
3. [Main Menu Setup](#main-menu-setup)
4. [Pause Menu Setup](#pause-menu-setup)
5. [Settings Menu Setup](#settings-menu-setup)
6. [Loading Screen Setup](#loading-screen-setup)
7. [Game Manager Setup](#game-manager-setup)
8. [Testing Checklist](#testing-checklist)

---

## Prerequisites

**Required Systems:**
- GameManager
- SceneTransitionManager
- AudioManager (optional, for menu sounds)

**Required Scenes:**
- MainMenu scene
- Gameplay scene(s)

---

## Scene Setup

### Step 1: Create Scenes (5 min)

1. **Create Main Menu Scene:**
   - File → New Scene
   - Save as "MainMenu.unity" in `Scenes/` folder
   - Add to Build Settings (File → Build Settings → Add Open Scenes)
   - Set as index 0 (first scene)

2. **Create Gameplay Scene:**
   - File → New Scene
   - Save as "Gameplay.unity" in `Scenes/` folder
   - Add to Build Settings
   - Set as index 1

### Step 2: Create Persistent Manager GameObject (5 min)

1. **In Main Menu Scene:**
   - Create Empty GameObject: "GameManagers"
   - Add `GameManager.cs` component
   - Add `SceneTransitionManager.cs` component
   - Configure GameManager:
     - Main Menu Scene Name: "MainMenu"
     - Gameplay Scene Name: "Gameplay"
     - Target Frame Rate: 60
     - Skip Main Menu: ✗ (unchecked)

---

## Main Menu Setup

### Step 1: Create Main Menu UI (30 min)

1. **Create Canvas:**
   ```
   MainMenu Scene
   └── Canvas (Screen Space - Overlay)
       ├── Background (Image - full screen)
       ├── Title (TextMeshPro - large, centered)
       ├── VersionText (TextMeshPro - bottom-right)
       └── MainMenuPanel
           ├── NewGameButton
           │   └── Text ("New Game")
           ├── ContinueButton
           │   └── Text ("Continue")
           ├── SettingsButton
           │   └── Text ("Settings")
           ├── CreditsButton
           │   └── Text ("Credits")
           └── QuitButton
               └── Text ("Quit")
   ```

2. **Button Layout:**
   - Position: Center of screen
   - Size: 300x60 per button
   - Spacing: 20 pixels between buttons
   - Use Vertical Layout Group for automatic spacing

3. **Add MainMenuController:**
   - Create Empty GameObject: "MainMenuController"
   - Add `MainMenuController.cs` component
   - Assign all UI references in Inspector

### Step 2: Create Settings Panel (20 min)

1. **Create Settings UI:**
   ```
   Canvas
   └── SettingsPanel (initially inactive)
       ├── Title ("Settings")
       ├── AudioTab
       │   ├── MasterVolumeSlider + Text
       │   ├── MusicVolumeSlider + Text
       │   └── SFXVolumeSlider + Text
       ├── GraphicsTab
       │   ├── QualityDropdown
       │   ├── ResolutionDropdown
       │   ├── FullscreenToggle
       │   └── VSyncToggle
       ├── ControlsTab
       │   ├── MouseSensitivitySlider + Text
       │   └── InvertYToggle
       └── Buttons
           ├── BackButton
           ├── ApplyButton
           └── ResetButton
   ```

2. **Add SettingsMenuController:**
   - Add `SettingsMenuController.cs` to SettingsPanel
   - Assign all UI references

### Step 3: Create Credits Panel (10 min)

1. **Create Credits UI:**
   ```
   Canvas
   └── CreditsPanel (initially inactive)
       ├── Title ("Credits")
       ├── CreditsText (ScrollView with TextMeshPro)
       └── BackButton
   ```

2. **Fill Credits Text:**
   ```
   Game Created By: [Your Name]

   Assets:
   - Mixamo for character animations
   - [List other assets used]

   Special Thanks:
   - [Anyone you want to thank]

   Powered by Unity Engine
   ```

---

## Pause Menu Setup

### Step 1: Create Pause Menu UI (25 min)

1. **In Gameplay Scene, Create Canvas:**
   ```
   Gameplay Scene
   └── PauseMenuCanvas (Screen Space - Overlay)
       ├── PauseMenuPanel (initially inactive)
       │   ├── Title ("Paused")
       │   ├── ResumeButton ("Resume")
       │   ├── SettingsButton ("Settings")
       │   ├── RestartButton ("Restart")
       │   ├── MainMenuButton ("Main Menu")
       │   └── QuitButton ("Quit")  [Desktop only]
       └── Overlay (Dark semi-transparent image behind panel)
   ```

2. **Overlay Settings:**
   - Full screen size
   - Color: Black with alpha ~128 (50% transparent)
   - Raycast Target: Enabled (blocks clicks)

3. **Add PauseMenuController:**
   - Create Empty GameObject: "PauseMenuController"
   - Add `PauseMenuController.cs` component
   - Assign UI references

### Step 2: Create Confirmation Dialogs (15 min)

1. **Create Confirmation Dialog Prefabs:**
   ```
   PauseMenuCanvas
   ├── RestartConfirmDialog (initially inactive)
   │   ├── Title ("Restart Level?")
   │   ├── Message ("Progress will be lost")
   │   ├── ConfirmButton ("Restart")
   │   └── CancelButton ("Cancel")
   ├── MainMenuConfirmDialog (initially inactive)
   │   ├── Title ("Return to Main Menu?")
   │   ├── Message ("Progress will be lost")
   │   ├── ConfirmButton ("Return")
   │   └── CancelButton ("Cancel")
   └── QuitConfirmDialog (initially inactive)
       ├── Title ("Quit Game?")
       ├── Message ("Progress will be lost")
       ├── ConfirmButton ("Quit")
       └── CancelButton ("Cancel")
   ```

2. **Link Buttons:**
   - ConfirmButton → PauseMenuController.ConfirmRestart/MainMenu/Quit()
   - CancelButton → PauseMenuController.CancelConfirmation()

### Step 3: Add Settings Panel to Pause Menu (10 min)

1. **Copy Settings Panel from Main Menu:**
   - Or create new with same structure
   - Add to PauseMenuCanvas
   - Initially inactive

2. **Link to PauseMenuController:**
   - Assign Settings Panel reference
   - Settings button will show/hide it

---

## Settings Menu Setup

Already covered in Main Menu and Pause Menu sections above.

**Key Features:**
- Master, Music, SFX volume sliders (0-100%)
- Quality dropdown (Low, Medium, High, Ultra)
- Resolution dropdown (all available resolutions)
- Fullscreen toggle
- VSync toggle
- Mouse sensitivity slider (1-10)
- Invert Y-axis toggle

**Settings Persistence:**
- Automatically saves to PlayerPrefs
- Loads on start
- Apply button for immediate changes
- Reset button for defaults

---

## Loading Screen Setup

### Step 1: Create Loading Screen Prefab (20 min)

1. **Create Prefab:**
   ```
   Create new Canvas → LoadingScreen
   └── LoadingPanel
       ├── Background (Full screen, dark image)
       ├── Logo/Title (Center-top)
       ├── ProgressBar
       │   ├── Background (Image)
       │   ├── Fill (Image, Fill Type: Filled Horizontal)
       │   └── PercentText ("0%")
       ├── TipText ("Loading tip...")
       └── Spinner (Rotating icon/animation)
   ```

2. **Configure Components:**
   - Canvas: Screen Space - Overlay
   - Sort Order: 999 (above everything)
   - Add CanvasGroup component to LoadingScreen
   - Add `LoadingScreen.cs` component

3. **Assign References:**
   - Loading Panel
   - Progress Bar (Fill Image)
   - Progress Text
   - Tip Text
   - Spinner

4. **Configure LoadingScreen Component:**
   - Fade In Duration: 0.3
   - Fade Out Duration: 0.3
   - Spinner Speed: 180
   - Add loading tips (10-20 tips)

5. **Save as Prefab:**
   - Drag to `Prefabs/` folder
   - Name: "LoadingScreen"

### Step 2: Link to Scene Transition Manager (2 min)

1. **In Main Menu Scene:**
   - Select GameManagers GameObject
   - Find SceneTransitionManager component
   - Drag LoadingScreen prefab to "Loading Screen Prefab" field

---

## Game Manager Setup

### Configure GameManager Component (5 min)

**In MainMenu Scene, select GameManagers:**

1. **Scene Names:**
   - Main Menu Scene Name: "MainMenu"
   - Gameplay Scene Name: "Gameplay"

2. **Game Settings:**
   - Target Frame Rate: 60
   - Enable VSync: ✓

3. **Startup:**
   - Skip Main Menu: ✗ (for testing, can enable)
   - Auto Load Save: ✗

### Configure Scene Transition Manager (3 min)

**On same GameObject:**

1. **Transition Settings:**
   - Fade Duration: 0.5
   - Min Loading Time: 1.0

2. **Loading Screen:**
   - Drag LoadingScreen prefab here

3. **Audio:**
   - Play Transition Sound: ✓
   - Transition Sound Name: "scene_transition"

---

## Testing Checklist

### Main Menu Testing

- [ ] Main menu appears on game start
- [ ] Title and version text display correctly
- [ ] New Game button loads gameplay scene
- [ ] Continue button is disabled (no save yet)
- [ ] Settings button opens settings panel
- [ ] Credits button opens credits panel
- [ ] Quit button closes game
- [ ] All buttons play click sound
- [ ] Menu music plays

### Pause Menu Testing

- [ ] ESC key pauses game during gameplay
- [ ] Pause menu appears with dark overlay
- [ ] Game time stops when paused
- [ ] Cursor becomes visible when paused
- [ ] Player controls disabled when paused
- [ ] Resume button unpauses game
- [ ] Settings button opens settings
- [ ] Restart button shows confirmation dialog
- [ ] Main Menu button shows confirmation dialog
- [ ] Confirmation dialogs work correctly
- [ ] Cancel buttons close dialogs

### Settings Testing

- [ ] Master volume slider changes all audio
- [ ] Music volume slider changes music only
- [ ] SFX volume slider changes sound effects
- [ ] Volume changes are instant
- [ ] Quality dropdown changes graphics quality
- [ ] Resolution dropdown changes screen resolution
- [ ] Fullscreen toggle works correctly
- [ ] VSync toggle works correctly
- [ ] Mouse sensitivity changes camera rotation speed
- [ ] Invert Y toggle inverts camera Y-axis
- [ ] Settings persist after closing menu
- [ ] Settings persist after restart
- [ ] Apply button saves settings
- [ ] Reset button restores defaults
- [ ] Back button returns to previous menu

### Loading Screen Testing

- [ ] Loading screen appears during transitions
- [ ] Progress bar fills correctly (0% → 100%)
- [ ] Progress percentage text updates
- [ ] Random loading tips display
- [ ] Spinner rotates smoothly
- [ ] Loading screen fades in smoothly
- [ ] Loading screen fades out smoothly
- [ ] Minimum loading time enforced (smooth experience)

### Scene Transitions

- [ ] Main Menu → Gameplay loads correctly
- [ ] Gameplay → Main Menu loads correctly
- [ ] Restart Level reloads gameplay scene
- [ ] Scene transitions show loading screen
- [ ] Loading screen shows appropriate progress
- [ ] No errors in console during transitions

### Game Flow

- [ ] New Game resets player stats
- [ ] Continue loads save data (when implemented)
- [ ] Pause/unpause toggles correctly
- [ ] Time scale returns to 1.0 after unpause
- [ ] Cursor lock state changes appropriately
- [ ] Player controls enable/disable correctly
- [ ] Game state changes trigger events
- [ ] Multiple pause/unpause cycles work

---

## Common Issues & Solutions

### Issue: Loading screen doesn't appear
**Solution:**
- Check Loading Screen Prefab is assigned in SceneTransitionManager
- Verify Canvas Sort Order is high (999)
- Ensure LoadingPanel is active initially

### Issue: Pause menu doesn't pause game
**Solution:**
- Verify GameManager.Instance exists
- Check ESC key binding in Update()
- Ensure Time.timeScale is set to 0 when paused

### Issue: Settings don't persist
**Solution:**
- Call SaveSettings() when applying changes
- Check PlayerPrefs keys are correct
- Verify LoadSettings() is called on Start()

### Issue: Buttons don't respond
**Solution:**
- Check Button components are enabled
- Verify onClick listeners are assigned
- Ensure EventSystem exists in scene
- Check Canvas Raycaster is present

### Issue: Continue button always disabled
**Solution:**
- This is expected until Save System is implemented
- GameManager.HasSaveData() currently returns false
- Will work after save system integration

---

## Performance Tips

1. **UI Optimization:**
   - Use Sprite Atlas for menu images
   - Disable raycasts on non-interactive UI
   - Pool confirmation dialogs instead of creating/destroying

2. **Loading Optimization:**
   - Use asynchronous scene loading (already implemented)
   - Display progress to keep player engaged
   - Enforce minimum loading time for smooth feel

3. **Audio Optimization:**
   - Preload menu music and sounds
   - Use low-quality audio for UI sounds
   - Fade music instead of abrupt stops

---

## Next Steps

After completing menu setup:

1. **Test all menu flows:**
   - Main Menu → Gameplay → Pause → Settings → Resume
   - Main Menu → Settings → Apply → Back
   - Gameplay → Restart
   - Gameplay → Main Menu

2. **Customize appearance:**
   - Add background images
   - Style buttons and panels
   - Add animations (fade in/out, scale)
   - Choose color scheme

3. **Add more features:**
   - Keybinding remapping
   - Language selection
   - Accessibility options
   - Achievement screen

4. **Integrate save system:**
   - Update HasSaveData() in GameManager
   - Enable Continue button when save exists
   - Show save slots

---

## Estimated Setup Time

- Scene Setup: 10 min
- Main Menu: 30 min
- Pause Menu: 40 min
- Settings Menu: (already included above)
- Loading Screen: 20 min
- Game Manager: 8 min
- Testing: 20 min

**Total: ~2 hours** for complete menu system

---

**Menu system setup complete!** Your game now has professional menus! 🎮
