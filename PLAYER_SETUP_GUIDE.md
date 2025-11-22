# Player Setup Guide

Complete guide for setting up the player character with movement, camera, interactions, and animations.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Player GameObject Setup](#player-gameobject-setup)
3. [Camera Setup](#camera-setup)
4. [Animation Setup](#animation-setup)
5. [UI Setup](#ui-setup)
6. [Testing Checklist](#testing-checklist)

---

## Prerequisites

**Required Assets:**
- Character model (humanoid recommended for Mixamo animations)
- Character animations:
  - Idle
  - Walk
  - Run
  - Jump (optional)
  - Cast/Attack (optional)
  - Harvest/Interact (optional)

**Imported Systems:**
- InputManager
- AudioManager (optional, for footstep sounds)
- FloatingTextManager (optional, for damage numbers)
- MagicSystem (optional, integrates with PlayerStats)

---

## Player GameObject Setup

### Step 1: Create Player GameObject (5 min)

1. **Create Empty GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name it "Player"
   - Position: (0, 0, 0)
   - Tag: "Player"
   - Layer: "Player" (create if doesn't exist)

2. **Add Character Model:**
   - Drag your character model as child of Player
   - Name it "CharacterModel"
   - Position: (0, 0, 0)
   - Ensure model is facing forward (Z-axis)

3. **Add Character Controller:**
   - Select Player GameObject
   - Add Component → Character Controller
   - Settings:
     - Height: 2 (adjust to match your character)
     - Radius: 0.5
     - Center: (0, 1, 0)
     - Min Move Distance: 0.001

### Step 2: Add Player Scripts (10 min)

**Add PlayerController:**
```
Select Player GameObject
Add Component → PlayerController

Settings:
- Walk Speed: 3
- Run Speed: 6
- Rotation Speed: 720
- Jump Height: 2
- Gravity: -15
- Enable Jumping: ✓
- Ground Check Radius: 0.3
- Ground Check Distance: 0.1
- Ground Mask: Default (select ground layer)
- Camera Relative Movement: ✓
```

**Add PlayerStats:**
```
Add Component → PlayerStats

Settings:
- Max Health: 100
- Health Regen Rate: 1
- Max Mana: 100
- Max Stamina: 100
- Stamina Drain Rate: 10
- Level: 1
- Use Magic System: ✓ (if using MagicSystem)
```

**Add PlayerInteraction:**
```
Add Component → PlayerInteraction

Settings:
- Interaction Range: 3
- Interaction Key: E
- Interaction Layers: Everything (or specific layers)
- Raycast From Camera: ✓
- Use Sphere Cast: ✓
- Sphere Cast Radius: 0.3
- Show Debug Ray: ✓ (for testing)
```

**Add PlayerAnimationController:**
```
Add Component → PlayerAnimationController

Settings:
- Speed Parameter: "Speed"
- Grounded Parameter: "IsGrounded"
- Running Parameter: "IsRunning"
- Jump Trigger: "Jump"
- Cast Trigger: "Cast"
- Harvest Trigger: "Harvest"
- Speed Smooth Time: 0.1
```

### Step 3: Setup Animator (15 min)

1. **Create Animator Controller:**
   - Right-click in Project → Create → Animator Controller
   - Name it "PlayerAnimator"
   - Drag onto CharacterModel GameObject

2. **Add Parameters:**
   - Float: `Speed` (default: 0)
   - Bool: `IsGrounded` (default: true)
   - Bool: `IsRunning` (default: false)
   - Trigger: `Jump`
   - Trigger: `Cast`
   - Trigger: `Harvest`
   - Trigger: `Hit`

3. **Add Animation States:**
   - Idle (default state)
   - Walk
   - Run
   - Jump (optional)
   - Cast (optional)
   - Harvest (optional)

4. **Setup Transitions:**
   ```
   Idle → Walk: Speed > 0.1
   Walk → Idle: Speed < 0.1
   Walk → Run: IsRunning == true AND Speed > 1.0
   Run → Walk: IsRunning == false

   Any State → Jump: Jump trigger
   Jump → Idle: Animation ends

   Any State → Cast: Cast trigger
   Cast → previous state: Animation ends
   ```

5. **Blend Tree (Optional - Better Movement):**
   - Create Blend Tree for locomotion
   - Type: 2D Simple Directional
   - Parameters: Speed
   - States:
     - 0.0: Idle
     - 1.0: Walk
     - 2.0: Run

---

## Camera Setup

### Step 1: Create Camera GameObject (5 min)

1. **Find Main Camera:**
   - Usually "Main Camera" exists in scene
   - If not: Create → Camera
   - Tag: "MainCamera"

2. **Add CameraController:**
   ```
   Select Main Camera
   Add Component → CameraController

   Settings:
   - Target: Player (drag Player GameObject here)
   - Target Offset: (0, 1.5, 0)
   - Distance: 5
   - Min Distance: 2
   - Max Distance: 10
   - Height: 2
   - Initial Yaw: 0
   - Initial Pitch: 20
   - Min Pitch: -20
   - Max Pitch: 80
   - Mouse Sensitivity: 3
   - Use Right Mouse Button: ✓
   - Position Smoothing: 5
   - Rotation Smoothing: 10
   - Enable Collision: ✓
   - Collision Layers: Default + Environment
   ```

### Step 2: Configure Camera Collision (5 min)

1. **Set Collision Layers:**
   - Create layer "Environment" if it doesn't exist
   - Assign all static objects (walls, terrain, buildings) to Environment layer
   - Collision Mask in CameraController: Include Environment layer

2. **Test Camera:**
   - Play scene
   - Hold Right Mouse Button and move mouse to rotate camera
   - Walk player into wall - camera should move closer to avoid clipping

---

## Animation Setup

### Importing Mixamo Animations (20 min)

1. **Download Animations:**
   - Go to Mixamo.com
   - Upload your character FBX (first time only)
   - Download animations:
     - Idle
     - Walking
     - Running
     - Jump (optional)
     - Spell Cast (optional)
     - Pick Up (for harvesting)

2. **Import to Unity:**
   - Drag animation FBX files into Unity
   - Select each animation FBX
   - In Inspector:
     - Rig tab:
       - Animation Type: Humanoid
       - Avatar Definition: Create From This Model
     - Animation tab:
       - Loop Time: ✓ (for Idle, Walk, Run)
       - Loop Time: ✗ (for Jump, Cast, etc.)
   - Click Apply

3. **Add to Animator:**
   - Double-click PlayerAnimator
   - Drag animation clips from FBX into Animator window
   - Connect transitions as described above

---

## UI Setup

### Step 1: Create Player HUD Canvas (15 min)

1. **Create Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Name it "PlayerHUD"
   - Render Mode: Screen Space - Overlay

2. **Add Health Bar:**
   ```
   Create UI Hierarchy:
   PlayerHUD
   └── HealthBar
       ├── Background (Image - dark color)
       ├── Fill (Image - red/green color)
       └── Text (TextMeshPro - "HP: 100/100")

   Position: Top-left corner
   Size: 200x30
   ```

3. **Add Mana Bar:**
   ```
   PlayerHUD
   └── ManaBar
       ├── Background (Image - dark color)
       ├── Fill (Image - blue color)
       └── Text (TextMeshPro - "MP: 100/100")

   Position: Below health bar
   Size: 200x30
   ```

4. **Add Stamina Bar:**
   ```
   PlayerHUD
   └── StaminaBar
       ├── Background (Image - dark color)
       ├── Fill (Image - yellow color)
       └── Text (TextMeshPro - "Stamina: 100/100")

   Position: Below mana bar
   Size: 200x30
   ```

5. **Add Experience Bar:**
   ```
   PlayerHUD
   └── ExpBar
       ├── Background (Image - dark color)
       ├── Fill (Image - purple color)
       └── Text (TextMeshPro - "Level 1 - 0/100 XP")

   Position: Bottom of screen, centered
   Size: 300x20
   ```

6. **Add Interaction Prompt:**
   ```
   PlayerHUD
   └── InteractionPrompt
       └── Text (TextMeshPro - "Press E to interact")

   Position: Center-bottom
   Font Size: 24
   Color: Yellow/White
   Initially: Hidden
   ```

### Step 2: Create UI Controller Script (10 min)

Create `PlayerHUDController.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CozyGame;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Health Bar")]
    public Image healthFill;
    public TextMeshProUGUI healthText;

    [Header("Mana Bar")]
    public Image manaFill;
    public TextMeshProUGUI manaText;

    [Header("Stamina Bar")]
    public Image staminaFill;
    public TextMeshProUGUI staminaText;

    [Header("Experience Bar")]
    public Image expFill;
    public TextMeshProUGUI expText;

    private void Start()
    {
        // Subscribe to PlayerStats events
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged.AddListener(UpdateHealth);
            PlayerStats.Instance.OnManaChanged.AddListener(UpdateMana);
            PlayerStats.Instance.OnStaminaChanged.AddListener(UpdateStamina);
            PlayerStats.Instance.OnExpChanged.AddListener(UpdateExp);
            PlayerStats.Instance.OnLevelUp.AddListener(OnLevelUp);
        }
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthFill != null)
            healthFill.fillAmount = current / max;

        if (healthText != null)
            healthText.text = $"HP: {current:F0}/{max:F0}";
    }

    private void UpdateMana(float current, float max)
    {
        if (manaFill != null)
            manaFill.fillAmount = current / max;

        if (manaText != null)
            manaText.text = $"MP: {current:F0}/{max:F0}";
    }

    private void UpdateStamina(float current, float max)
    {
        if (staminaFill != null)
            staminaFill.fillAmount = current / max;

        if (staminaText != null)
            staminaText.text = $"Stamina: {current:F0}/{max:F0}";
    }

    private void UpdateExp(int current, int toNext)
    {
        if (expFill != null)
            expFill.fillAmount = (float)current / toNext;

        if (expText != null)
        {
            int level = PlayerStats.Instance != null ? PlayerStats.Instance.level : 1;
            expText.text = $"Level {level} - {current}/{toNext} XP";
        }
    }

    private void OnLevelUp(int newLevel)
    {
        Debug.Log($"Level up! Now level {newLevel}");
        // Add level up animation/effects here
    }
}
```

Attach this script to PlayerHUD GameObject and assign all UI references.

### Step 3: Mobile Controls (15 min)

1. **Add Virtual Joystick:**
   ```
   PlayerHUD
   └── VirtualJoystick (already created in earlier phase)
       ├── Background (Image - circular)
       └── Handle (Image - smaller circle)

   Position: Bottom-left corner
   Size: 150x150
   ```

2. **Assign to PlayerController:**
   - Select Player GameObject
   - In PlayerController component:
     - Virtual Joystick: Drag VirtualJoystick GameObject here

3. **Add Mobile Run Button (Optional):**
   ```
   PlayerHUD
   └── MobileRunButton (Button)
       └── Text ("Run")

   Position: Bottom-right corner
   Size: 80x80
   ```

4. **Link Run Button:**
   - Select Player GameObject
   - In PlayerController component:
     - Mobile Run Button: Drag MobileRunButton here

---

## Testing Checklist

### Movement Testing

- [ ] Player moves with WASD keys
- [ ] Player moves with virtual joystick (mobile)
- [ ] Walk speed is correct
- [ ] Run speed is correct (hold Shift)
- [ ] Run toggles on mobile button
- [ ] Player rotates to face movement direction
- [ ] Movement is smooth and responsive

### Camera Testing

- [ ] Camera follows player smoothly
- [ ] Right mouse button rotates camera
- [ ] Camera rotation is smooth
- [ ] Camera doesn't go through walls
- [ ] Camera zoom works (mouse wheel)
- [ ] Camera auto-rotates behind player (if enabled)
- [ ] Touch controls work on mobile

### Animation Testing

- [ ] Idle animation plays when standing still
- [ ] Walk animation plays when moving slowly
- [ ] Run animation plays when running
- [ ] Jump animation plays when jumping
- [ ] Animations blend smoothly
- [ ] Character faces movement direction

### Interaction Testing

- [ ] Interaction prompt appears when near NPCs
- [ ] E key interacts with NPCs
- [ ] Tap to interact works on mobile
- [ ] Interaction range is correct
- [ ] Highlight appears on interactable objects
- [ ] Raycast debug line shows (if enabled)

### Stats & UI Testing

- [ ] Health bar displays correctly
- [ ] Mana bar displays correctly
- [ ] Stamina bar displays correctly
- [ ] Experience bar displays correctly
- [ ] Bars update when stats change
- [ ] Level up triggers correctly
- [ ] Level up restores health/mana/stamina
- [ ] Death triggers correctly
- [ ] Respawn works correctly

### Ground Detection Testing

- [ ] Player is grounded on flat surfaces
- [ ] Player falls when walking off ledges
- [ ] Gravity applies correctly
- [ ] Jump works correctly
- [ ] Ground check gizmo shows in Scene view

---

## Common Issues & Solutions

### Issue: Player falls through floor
**Solution:**
- Ensure floor has Collider component
- Check Ground Mask in PlayerController includes floor layer
- Increase Ground Check Radius slightly

### Issue: Camera jerky/stuttering
**Solution:**
- Move camera code to LateUpdate (already done in CameraController)
- Increase Position/Rotation Smoothing values
- Ensure camera has no parent objects

### Issue: Animations not playing
**Solution:**
- Check Animator Controller is assigned to CharacterModel
- Verify animation parameters match PlayerAnimationController settings
- Ensure animations are set to Humanoid rig type

### Issue: Character sliding when stopping
**Solution:**
- Increase Deceleration Time in PlayerController
- Add root motion to Idle animation
- Adjust Speed Smooth Time in PlayerAnimationController

### Issue: Interaction not working
**Solution:**
- Check Interaction Layers includes target object's layer
- Verify NPC has IInteractable component or NPCInteractable script
- Increase Interaction Range
- Enable Show Debug Ray to see raycast

### Issue: Virtual joystick not appearing on mobile
**Solution:**
- Verify Canvas Scaler is set correctly
- Check VirtualJoystick is assigned in PlayerController
- Ensure InputManager detects mobile platform correctly

---

## Performance Tips

1. **Animation Optimization:**
   - Use Animation Compression: Keyframe Reduction
   - Disable "Resample Curves" if not needed
   - Reduce animation quality for distant players (LOD)

2. **Camera Optimization:**
   - Reduce collision check frequency for mobile
   - Use simpler collision shapes (spheres instead of meshes)
   - Disable auto-rotation on low-end devices

3. **UI Optimization:**
   - Use Sprite Atlas for UI images
   - Batch UI elements when possible
   - Hide UI elements that are off-screen

---

## Next Steps

After completing player setup:

1. **Test in actual gameplay:**
   - Walk around the environment
   - Interact with NPCs
   - Test all animations
   - Try combat/spells
   - Test on mobile device

2. **Customize to your needs:**
   - Adjust movement speeds
   - Tweak camera settings
   - Add more animations
   - Customize UI appearance

3. **Add more features:**
   - Inventory system
   - Equipment system
   - Skills/abilities
   - Save/load system

---

## Estimated Setup Time

- Player GameObject: 5 min
- Scripts Configuration: 10 min
- Animation Setup: 20-30 min
- Camera Setup: 10 min
- UI Setup: 25 min
- Testing & Tweaking: 20 min

**Total: ~90 minutes** for complete player setup

---

**Player setup complete!** Your character is now ready for gameplay! 🎮
