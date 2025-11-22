# Save System Setup Guide

Complete guide for setting up the save/load system in Unity.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [SaveSystem Manager Setup](#savesystem-manager-setup)
3. [Save Slot UI Setup](#save-slot-ui-setup)
4. [Main Menu Integration](#main-menu-integration)
5. [Pause Menu Integration](#pause-menu-integration)
6. [Testing Checklist](#testing-checklist)
7. [Common Issues & Solutions](#common-issues--solutions)

---

## Prerequisites

**Required Systems:**
- GameManager (Phase 6)
- SceneTransitionManager (Phase 6)
- PlayerController (Phase 5)
- PlayerStats (Phase 5)
- QuestManager (Phase 4)
- RiddleManager (Phase 4)

**Required Scenes:**
- MainMenu scene
- Gameplay scene

**Important:** All manager scripts (GameManager, PlayerController, PlayerStats, QuestManager, RiddleManager) must be singletons with DontDestroyOnLoad.

---

## SaveSystem Manager Setup

### Step 1: Create SaveSystem GameObject (5 min)

1. **In MainMenu Scene:**
   - Locate the existing "GameManagers" GameObject
   - Add `SaveSystem.cs` component to it

2. **Configure SaveSystem Component:**
   - **Save Settings:**
     - Number of Save Slots: 5
     - Save Directory Name: "Saves"
     - Encrypt Saves: ✓ (optional)
     - Create Backups: ✓ (optional)

   - **Auto-Save:**
     - Enable Auto Save: ✓
     - Auto Save Interval: 300 (5 minutes)

   - **Quick Save/Load:**
     - Enable Quick Save: ✓
     - Quick Save Key: F5
     - Quick Load Key: F9

   - **Debug:**
     - Enable Debug Logs: ✓

3. **Save File Location:**
   - Windows: `%USERPROFILE%/AppData/LocalLow/{CompanyName}/{ProductName}/Saves/`
   - Mac: `~/Library/Application Support/{CompanyName}/{ProductName}/Saves/`
   - Linux: `~/.config/unity3d/{CompanyName}/{ProductName}/Saves/`

---

## Save Slot UI Setup

### Step 1: Create Save Slot Prefab (30 min)

1. **Create UI Structure:**
   ```
   Canvas
   └── SaveSlotPrefab
       ├── Background (Image)
       ├── EmptySlotIndicator (Text - "Empty Slot")
       ├── Content
       │   ├── SaveNameText (TextMeshPro)
       │   ├── DateTimeText (TextMeshPro)
       │   ├── PlaytimeText (TextMeshPro)
       │   └── LevelText (TextMeshPro)
       ├── SlotButton (Button - entire slot)
       └── DeleteButton (Button - small, top-right)
   ```

2. **Configure SaveSlotPrefab:**
   - Size: 600x120
   - Layout: Horizontal with padding

3. **Add SaveSlotUI Component:**
   - Add `SaveSlotUI.cs` to SaveSlotPrefab root
   - Assign all UI references:
     - Slot Button
     - Save Name Text
     - Date Time Text
     - Playtime Text
     - Level Text
     - Empty Slot Indicator
     - Delete Button

   - **Visual States:**
     - Empty Slot Color: Gray (128, 128, 128, 255)
     - Occupied Slot Color: White (255, 255, 255, 255)
     - Selected Slot Color: Yellow (255, 235, 4, 255)

4. **Save as Prefab:**
   - Drag to `Prefabs/UI/` folder
   - Name: "SaveSlotPrefab"

### Step 2: Create Save Slot Menu (30 min)

1. **Create Save/Load Menu UI:**
   ```
   Canvas
   └── SaveSlotMenuPanel (initially inactive)
       ├── Background (Dark overlay)
       ├── Panel (centered)
       │   ├── Title (TextMeshPro - "Save Game" or "Load Game")
       │   ├── SlotsScrollView
       │   │   └── Content (Vertical Layout Group)
       │   │       └── [Slots created at runtime]
       │   └── Buttons
       │       ├── ConfirmButton (Text: "Save" or "Load")
       │       └── BackButton (Text: "Back")
       └── DeleteConfirmDialog (initially inactive)
       │   ├── Title ("Delete Save?")
       │   ├── Message (TextMeshPro)
       │   ├── ConfirmButton ("Delete")
       │   └── CancelButton ("Cancel")
       └── OverwriteConfirmDialog (initially inactive)
           ├── Title ("Overwrite Save?")
           ├── Message (TextMeshPro)
           ├── ConfirmButton ("Overwrite")
           └── CancelButton ("Cancel")
   ```

2. **Configure Scroll View:**
   - Content size fitter: Vertical (Preferred Size)
   - Vertical Layout Group:
     - Spacing: 10
     - Child Force Expand: Width ✓, Height ✗
     - Child Control Size: Width ✓, Height ✓

3. **Add SaveSlotMenuController:**
   - Add `SaveSlotMenuController.cs` to SaveSlotMenuPanel
   - Assign all references:
     - Menu Mode: Load (or Save)
     - Slots Container: ScrollView/Content
     - Save Slot Prefab: (drag SaveSlotPrefab)
     - Number of Slots: 5
     - Save Slot Panel: SaveSlotMenuPanel
     - Delete Confirm Dialog
     - Overwrite Confirm Dialog
     - Delete Confirm Message
     - Overwrite Confirm Message
     - Back Button
     - Confirm Button

4. **Wire Up Confirmation Dialogs:**
   - Delete Confirm Button → `SaveSlotMenuController.ConfirmDelete()`
   - Delete Cancel Button → `SaveSlotMenuController.CancelDelete()`
   - Overwrite Confirm Button → `SaveSlotMenuController.ConfirmOverwrite()`
   - Overwrite Cancel Button → `SaveSlotMenuController.CancelOverwrite()`

---

## Main Menu Integration

### Step 1: Add Load Game Panel to Main Menu (15 min)

1. **In MainMenu Scene:**
   - Create a copy of SaveSlotMenuPanel
   - Place it in MainMenu Canvas
   - Rename to "LoadGamePanel"
   - Set Menu Mode: Load

2. **Update MainMenuController:**
   - Select MainMenuController GameObject
   - Find MainMenuController component
   - Assign "Load Game Panel" field: LoadGamePanel

3. **Verify Continue Button:**
   - Continue button should now:
     - Check if save data exists
     - Enable/disable automatically
     - Open LoadGamePanel when clicked

---

## Pause Menu Integration

### Step 1: Add Save Game to Pause Menu (15 min)

1. **In Gameplay Scene:**
   - Open PauseMenuCanvas
   - Create a copy of SaveSlotMenuPanel
   - Place it in PauseMenuCanvas
   - Rename to "SaveGamePanel"
   - Set Menu Mode: Save

2. **Add Save Button to Pause Menu:**
   - In PauseMenuPanel, add new Button:
     - Position: Between "Settings" and "Restart"
     - Text: "Save Game"
     - Size: Same as other buttons

3. **Update PauseMenuController:**
   - Select PauseMenuController GameObject
   - Find PauseMenuController component
   - Assign:
     - Save Game Button: (new button)
     - Save Slots Panel: SaveGamePanel

4. **Verify Pause Menu:**
   - "Save Game" button should open SaveGamePanel
   - Panel should show in Save mode
   - Back button should return to pause menu

---

## Testing Checklist

### Save System Testing

- [ ] SaveSystem initializes on game start
- [ ] SaveSystem persists across scene loads (DontDestroyOnLoad)
- [ ] Save directory is created in persistent data path
- [ ] Multiple save slots work correctly

### Saving Game State

- [ ] Save game from pause menu
- [ ] All player data saved (position, rotation, stats, level, exp)
- [ ] Quest progress saved correctly
- [ ] Riddle progress saved correctly
- [ ] Plant states saved (if applicable)
- [ ] World data saved (time, weather, scene)
- [ ] Save metadata displays correctly (name, date, playtime, level)

### Loading Game State

- [ ] Load game from main menu
- [ ] Player position/rotation restored
- [ ] Player stats restored (health, mana, stamina, level, exp)
- [ ] Quest progress restored
- [ ] Riddle progress restored
- [ ] Scene loads correctly
- [ ] Continue button enabled when save exists

### Save Slot UI

- [ ] Empty slots display "Empty Slot" message
- [ ] Occupied slots show save metadata
- [ ] Slot selection highlights correctly
- [ ] Delete button appears only for occupied slots
- [ ] Delete confirmation dialog works
- [ ] Overwrite confirmation dialog works (Save mode)
- [ ] Confirm button disabled for empty slots (Load mode)
- [ ] Back button returns to previous menu

### Auto-Save

- [ ] Auto-save triggers at configured interval
- [ ] Auto-save doesn't interrupt gameplay
- [ ] Auto-save uses correct slot (last used or dedicated slot 0)
- [ ] Auto-save notification appears (if enabled)

### Quick Save/Load

- [ ] F5 quick saves to dedicated slot
- [ ] F9 quick loads from last save
- [ ] Quick save/load works during gameplay
- [ ] Quick save/load hotkeys configurable

### Multiple Saves

- [ ] Create saves in different slots
- [ ] Load from different slots
- [ ] Delete saves from specific slots
- [ ] Overwrite existing saves
- [ ] Save metadata updates correctly

### Edge Cases

- [ ] Save with no quests active
- [ ] Save with no riddles answered
- [ ] Save with partial quest progress
- [ ] Load after deleting save file manually
- [ ] Load corrupted save file (shows error)
- [ ] Save when disk is full (shows error)
- [ ] Multiple rapid saves (debouncing)

### Scene Transitions

- [ ] Save in Gameplay, load from Main Menu
- [ ] Scene transition shows loading screen
- [ ] Player spawns at saved position
- [ ] Game state restored before player control enabled
- [ ] No errors during scene transitions

---

## Common Issues & Solutions

### Issue: SaveSystem not found

**Solution:**
- Ensure SaveSystem.cs is attached to persistent GameObject
- GameObject must have DontDestroyOnLoad
- Check SaveSystem.Instance is not null before use

### Issue: Save slots don't display

**Solution:**
- Verify SaveSlotPrefab is assigned in SaveSlotMenuController
- Check SlotsContainer reference is correct
- Ensure numberOfSlots > 0
- Check SaveSlotPrefab has SaveSlotUI component

### Issue: Save data not persisting

**Solution:**
- Check save directory path exists
- Verify write permissions
- Look for errors in console during save
- Check encryptSaves setting (try disabling)

### Issue: Continue button always disabled

**Solution:**
- Verify SaveSystem.Instance exists in MainMenu scene
- Check HasSaveData() is called correctly
- Create at least one save file for testing
- Call RefreshContinueButton() after saving

### Issue: Player position not restored

**Solution:**
- Ensure PlayerController.Instance exists
- Check PlayerController has Teleport() method
- Verify scene loads before ApplyGameState() is called
- Check SaveData.playerData.position is valid

### Issue: Quest/Riddle progress not saved

**Solution:**
- Verify QuestManager/RiddleManager implement GetSaveData()
- Check managers are initialized before SaveSystem
- Ensure managers persist across scenes (DontDestroyOnLoad)
- Look for errors in CaptureGameState()

### Issue: Auto-save not working

**Solution:**
- Check "Enable Auto Save" is enabled
- Verify autoSaveInterval > 0
- Ensure game is in Playing state (not paused)
- Check timeSinceLastSave counter in SaveSystem

### Issue: Scene doesn't load on game load

**Solution:**
- Verify SceneTransitionManager.Instance exists
- Check scene name in SaveData.worldData.currentScene
- Ensure scene is added to Build Settings
- Look for errors in SceneTransitionManager

### Issue: Save file encrypted/unreadable

**Solution:**
- This is normal if encryptSaves is enabled
- Use JSON viewer with XOR decryption
- Disable encryptSaves for debugging
- Backup saves are stored unencrypted (if createBackups enabled)

---

## Advanced Configuration

### Custom Save Data

To save additional game data:

1. **Add to SaveData.cs:**
   ```csharp
   [System.Serializable]
   public class CustomSystemSaveData
   {
       public int customValue;
       public string customState;
   }

   // In SaveData class:
   public CustomSystemSaveData customData;
   ```

2. **Update SaveSystem.CaptureGameState():**
   ```csharp
   // Capture custom system data
   if (CustomSystem.Instance != null)
   {
       saveData.customData = CustomSystem.Instance.GetSaveData();
   }
   ```

3. **Update SaveSystem.ApplyGameState():**
   ```csharp
   // Apply custom system data
   if (CustomSystem.Instance != null)
   {
       CustomSystem.Instance.LoadSaveData(saveData.customData);
   }
   ```

### Custom Save Slots

To add more save slots:

1. **In SaveSystem:**
   - Increase "Number Of Save Slots" in Inspector

2. **In SaveSlotMenuController:**
   - Increase "Number Of Slots" to match

### Cloud Save Integration

For cloud saves (Steam, Epic, etc.):

1. **Override save path:**
   ```csharp
   // In SaveSystem.cs, modify GetSaveDirectoryPath():
   #if STEAMWORKS
       return SteamRemoteStorage.GetCloudPath();
   #else
       return Application.persistentDataPath + "/" + saveDirectoryName;
   #endif
   ```

2. **Sync after save:**
   ```csharp
   // After WriteSaveFile():
   #if STEAMWORKS
       SteamRemoteStorage.FilePersisted(filePath);
   #endif
   ```

---

## Performance Tips

1. **Optimize save frequency:**
   - Don't auto-save too frequently (5+ minutes recommended)
   - Debounce rapid save requests
   - Save only on important events (quest complete, level up)

2. **Reduce save file size:**
   - Only save changed data (delta saves)
   - Use binary serialization instead of JSON
   - Compress save files with GZip

3. **Async saving:**
   - Save to disk on separate thread
   - Show saving indicator during save
   - Don't block gameplay during save

---

## Estimated Setup Time

- SaveSystem Manager: 5 min
- Save Slot Prefab: 30 min
- Save Slot Menu: 30 min
- Main Menu Integration: 15 min
- Pause Menu Integration: 15 min
- Testing: 30 min

**Total: ~2 hours** for complete save system setup

---

**Save system setup complete!** Your game now has full save/load functionality! 💾
