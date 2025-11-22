# 🎮 Multi-Platform Development Guide
## PC, Mobile, and WebGL Support

> **Your Game Target:** PC (primary), Mobile (touch), WebGL (browser)

---

## 📱 Platform Overview

### PC (Windows/Mac/Linux)
- **Controls:** Mouse + Keyboard
- **Performance:** Best (no limitations)
- **Testing:** Fastest iteration
- **Use for:** Primary development and testing

### Mobile (Android/iOS)
- **Controls:** Touch screen
- **Performance:** Limited (battery, heat, memory)
- **Testing:** Use emulator or real device
- **Considerations:** Various screen sizes, touch input

### WebGL (Browser)
- **Controls:** Mouse + Keyboard (desktop browser) or Touch (mobile browser)
- **Performance:** Limited (runs in browser, no multi-threading)
- **Testing:** Upload to itch.io or host locally
- **Considerations:** File size, loading times, memory limits

---

## 🎯 Input System Strategy

### Multi-Platform Input Architecture

We'll implement **input abstraction** so one codebase works everywhere:
- PC: Click to move, WASD, Mouse for camera
- Mobile: Touch to move, Virtual joystick, Touch for camera
- WebGL: Same as PC (inherits PC controls)

### Input Manager Setup

#### Step 1: Create Input Manager Script
Location: `Assets/Game/Scripts/Utilities/InputManager.cs`

```csharp
using UnityEngine;
using System.Collections;

namespace CozyGame
{
    /// <summary>
    /// Handles input across all platforms (PC, Mobile, WebGL)
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Platform Detection")]
        public bool isMobile = false;
        public bool isWebGL = false;
        public bool isPC = false;

        [Header("Input Settings")]
        public bool useVirtualJoystick = false;
        public LayerMask groundLayer;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DetectPlatform();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void DetectPlatform()
        {
#if UNITY_WEBGL
            isWebGL = true;
            isPC = false;
            isMobile = false;
#elif UNITY_ANDROID || UNITY_IOS
            isMobile = true;
            isPC = false;
            isWebGL = false;
            useVirtualJoystick = true;
#else
            isPC = true;
            isMobile = false;
            isWebGL = false;
#endif

            Debug.Log($"Platform detected - PC: {isPC}, Mobile: {isMobile}, WebGL: {isWebGL}");
        }

        /// <summary>
        /// Get movement input (works on all platforms)
        /// </summary>
        public Vector2 GetMovementInput()
        {
            if (isMobile && useVirtualJoystick)
            {
                // Virtual joystick input (we'll implement this later)
                return GetVirtualJoystickInput();
            }
            else
            {
                // PC/WebGL keyboard input
                float horizontal = Input.GetAxis("Horizontal"); // A/D or Arrow Keys
                float vertical = Input.GetAxis("Vertical");     // W/S or Arrow Keys
                return new Vector2(horizontal, vertical);
            }
        }

        /// <summary>
        /// Check if player pressed interact button
        /// </summary>
        public bool GetInteractInput()
        {
            if (isMobile)
            {
                // Touch input for mobile
                return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            }
            else
            {
                // E key or Left Click for PC/WebGL
                return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
            }
        }

        /// <summary>
        /// Get world position of pointer (mouse or touch)
        /// </summary>
        public Vector3 GetPointerWorldPosition()
        {
            Vector3 screenPosition;

            if (isMobile && Input.touchCount > 0)
            {
                screenPosition = Input.GetTouch(0).position;
            }
            else
            {
                screenPosition = Input.mousePosition;
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Virtual joystick input (placeholder - implement with UI joystick)
        /// </summary>
        private Vector2 GetVirtualJoystickInput()
        {
            // TODO: Integrate with virtual joystick UI
            // For now, return zero (we'll add joystick in Week 1)
            return Vector2.zero;
        }

        /// <summary>
        /// Check if pointer is over UI (prevents clicking through UI)
        /// </summary>
        public bool IsPointerOverUI()
        {
            if (isMobile && Input.touchCount > 0)
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            else
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            }
        }
    }
}
```

#### Step 2: Add Input Manager to Scene
1. Create empty GameObject in Scene: **GameObject** → **Create Empty**
2. Rename it to `InputManager`
3. Add script: **Add Component** → search "Input Manager"
4. In Inspector, set "Ground Layer" to "Default" (or your ground layer)

---

## 📏 UI & Screen Adaptation

### Canvas Setup for All Platforms

#### Step 1: Configure Canvas Scaler
1. Select your Canvas in Hierarchy
2. In **Canvas Scaler** component:
   - UI Scale Mode: **Scale With Screen Size**
   - Reference Resolution: **1920 x 1080**
   - Screen Match Mode: **Match Width Or Height**
   - Match: **0.5** (balance between width/height)

#### Step 2: Safe Area Support (Mobile)
Location: `Assets/Game/Scripts/UI/SafeAreaHandler.cs`

```csharp
using UnityEngine;

namespace CozyGame.UI
{
    /// <summary>
    /// Handles safe area for mobile devices (notches, rounded corners)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Check if safe area changed (device rotation, etc.)
            if (lastSafeArea != Screen.safeArea)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            lastSafeArea = safeArea;

            // Convert safe area to anchors
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}
```

**How to use:**
- Add this script to your **main UI Canvas** or a **SafeArea Panel**
- All UI elements inside will respect device safe areas

---

## 🎮 Virtual Controls for Mobile

### Virtual Joystick Setup

**We'll use a free asset:** Search Unity Asset Store for "Joystick Pack" (free)

#### Alternative: Simple Touch Joystick Script
Location: `Assets/Game/Scripts/UI/VirtualJoystick.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CozyGame.UI
{
    /// <summary>
    /// Simple virtual joystick for mobile touch input
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Joystick Components")]
        public RectTransform joystickBackground;
        public RectTransform joystickHandle;

        [Header("Settings")]
        public float handleRange = 50f;
        public bool fixedPosition = true;

        private Vector2 inputVector;
        private Vector2 joystickOrigin;

        public Vector2 InputDirection => inputVector;

        private void Start()
        {
            joystickOrigin = joystickBackground.anchoredPosition;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!fixedPosition)
            {
                joystickBackground.position = eventData.position;
            }
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                eventData.pressEventCamera,
                out position
            );

            position = Vector2.ClampMagnitude(position, handleRange);
            joystickHandle.anchoredPosition = position;

            inputVector = position / handleRange;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
            inputVector = Vector2.zero;

            if (!fixedPosition)
            {
                joystickBackground.anchoredPosition = joystickOrigin;
            }
        }
    }
}
```

**UI Setup:**
1. Create UI: **Right-click Canvas** → **UI** → **Image** (background)
2. Add child Image for handle
3. Add VirtualJoystick script to background
4. Assign references in Inspector
5. Only enable for mobile builds

---

## ⚡ Performance Optimization

### WebGL Specific Optimizations

#### 1. Reduce Memory Usage
```csharp
// In your startup script
void Start()
{
#if UNITY_WEBGL
    // Reduce quality for WebGL
    QualitySettings.SetQualityLevel(1); // Low or Medium
    Application.targetFrameRate = 30;   // Limit FPS
#endif
}
```

#### 2. Asset Compression
- **Textures:** Use **ASTC** or **DXT** compression
- **Audio:** Use **Vorbis** compression
- **Models:** Keep under 10k triangles per model

#### 3. Code Stripping
1. **Edit** → **Project Settings** → **Player** → **WebGL**
2. Set "Managed Stripping Level" to **High**
3. Enable "Strip Engine Code"

### Mobile Specific Optimizations

#### 1. Quality Settings Per Platform
```csharp
void Awake()
{
#if UNITY_ANDROID || UNITY_IOS
    QualitySettings.SetQualityLevel(2); // Medium quality
    Application.targetFrameRate = 30;
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
}
```

#### 2. Battery & Heat Management
- Target 30 FPS (not 60)
- Use low-poly models
- Reduce particle effects
- Minimize light sources (max 2-3 active lights)

#### 3. Touch Optimization
```csharp
void Update()
{
#if UNITY_ANDROID || UNITY_IOS
    // Only check touches, not mouse
    if (Input.touchCount > 0)
    {
        // Handle touch
    }
#endif
}
```

---

## 🧪 Testing Strategy

### Development Phase Testing

**Week 1-3: PC Only**
- Fastest iteration
- Build and test on PC
- Use mouse/keyboard

**Week 3-4: Add WebGL Testing**
- Weekly WebGL builds
- Test in Chrome/Firefox
- Check loading times

**Week 5: Mobile Testing**
- Build to Android (easiest to test)
- Test on real device if possible
- Check touch controls

### Testing Checklist

#### PC Testing
- [ ] Mouse controls responsive
- [ ] Keyboard shortcuts work
- [ ] 60 FPS in editor
- [ ] All UI readable at 1920x1080

#### WebGL Testing
- [ ] Game loads in under 30 seconds
- [ ] No crashes after 10 minutes
- [ ] Controls work in browser
- [ ] UI scales properly
- [ ] Test in: Chrome, Firefox, Safari

#### Mobile Testing
- [ ] Virtual joystick responsive
- [ ] Touch interactions work
- [ ] UI fits in safe area
- [ ] 30 FPS stable
- [ ] Battery doesn't drain too fast
- [ ] No overheating

---

## 🏗️ Build Configuration

### PC Build Settings
```
Platform: PC, Mac & Linux Standalone
Architecture: x86_64
Compression: Default
Development Build: ✅ (for testing)
```

### WebGL Build Settings
```
Platform: WebGL
Compression: Gzip (or Disabled for testing)
Memory Size: 512 MB
Enable Exceptions: Explicitly Thrown Exceptions Only
Development Build: ✅ (for testing)
```

**WebGL Publishing Options:**
- **itch.io** (easiest, free hosting)
- **GitHub Pages** (free)
- **Netlify** (free)

### Mobile Build Settings

**Android:**
```
Platform: Android
Minimum API Level: Android 5.0 (API 21)
Target API Level: Automatic (highest installed)
Scripting Backend: IL2CPP
Target Architectures: ARM64
```

**iOS (Mac only):**
```
Platform: iOS
Target Minimum iOS Version: 12.0
Architecture: ARM64
```

---

## 🎨 Asset Considerations

### Texture Sizes by Platform

| Asset Type | PC | WebGL | Mobile |
|------------|----|----|--------|
| Character Textures | 1024x1024 | 512x512 | 512x512 |
| Environment | 2048x2048 | 1024x1024 | 512x512 |
| UI Icons | 256x256 | 256x256 | 128x128 |
| Particle Textures | 256x256 | 128x128 | 128x128 |

### Model Complexity

| Asset Type | PC | WebGL | Mobile |
|------------|----|----|--------|
| Player Character | <15k tris | <10k tris | <5k tris |
| NPCs | <10k tris | <7k tris | <3k tris |
| Environment Props | <5k tris | <3k tris | <2k tris |
| Plants/Small Items | <1k tris | <500 tris | <300 tris |

**Your 3D Low Poly Chibi Style:** Perfect for all platforms! Aim for:
- Characters: 3k-5k triangles
- Simple shapes, cute proportions
- Minimal texture detail

---

## 🔧 Platform-Specific Scripts

### Platform Detector Utility
Location: `Assets/Game/Scripts/Utilities/PlatformSettings.cs`

```csharp
using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Automatically adjusts settings based on platform
    /// </summary>
    public class PlatformSettings : MonoBehaviour
    {
        [Header("Quality Settings")]
        public int pcQualityLevel = 4;      // High
        public int webGLQualityLevel = 2;   // Medium
        public int mobileQualityLevel = 1;  // Low

        [Header("Performance")]
        public int pcTargetFrameRate = 60;
        public int webGLTargetFrameRate = 30;
        public int mobileTargetFrameRate = 30;

        [Header("UI")]
        public GameObject mobileUIControls;
        public GameObject pcUIControls;

        private void Awake()
        {
            ApplyPlatformSettings();
        }

        private void ApplyPlatformSettings()
        {
#if UNITY_WEBGL
            QualitySettings.SetQualityLevel(webGLQualityLevel);
            Application.targetFrameRate = webGLTargetFrameRate;
            SetupUI(false, true);
#elif UNITY_ANDROID || UNITY_IOS
            QualitySettings.SetQualityLevel(mobileQualityLevel);
            Application.targetFrameRate = mobileTargetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SetupUI(true, false);
#else
            QualitySettings.SetQualityLevel(pcQualityLevel);
            Application.targetFrameRate = pcTargetFrameRate;
            SetupUI(false, true);
#endif
        }

        private void SetupUI(bool showMobileUI, bool showPCUI)
        {
            if (mobileUIControls != null)
                mobileUIControls.SetActive(showMobileUI);

            if (pcUIControls != null)
                pcUIControls.SetActive(showPCUI);
        }
    }
}
```

---

## 📱 Recommended Development Workflow

### Phase 1: PC Development (Weeks 1-3)
1. Build all systems on PC
2. Test with mouse/keyboard
3. Ignore mobile controls

### Phase 2: WebGL Integration (Week 4)
1. Make first WebGL build
2. Test in browser
3. Optimize if needed
4. Use same PC controls

### Phase 3: Mobile Polish (Week 5)
1. Add virtual joystick
2. Test touch controls
3. Adjust UI for mobile
4. Final platform testing

---

## 🚀 Deployment Guide

### Quick Deploy: itch.io (All Platforms)

**PC Build:**
1. Build to folder
2. Zip the build folder
3. Upload to itch.io
4. Set as "Windows" build

**WebGL Build:**
1. Build to folder
2. Upload entire folder to itch.io
3. Select "This file will be played in the browser"

**Mobile:**
1. Android: Upload .apk file
2. iOS: Requires Apple Developer account ($99/year)

---

## ✅ Platform Checklist

Before release, verify:
- [ ] PC build runs at 60 FPS
- [ ] WebGL loads under 60 seconds
- [ ] WebGL runs at 30 FPS
- [ ] Mobile touch controls work
- [ ] Mobile runs at 30 FPS without overheating
- [ ] UI readable on smallest target screen (375x667 iPhone SE)
- [ ] All platforms tested for 15+ minutes
- [ ] No crashes on any platform

---

## 💡 Pro Tips

### For WebGL Success
- Keep build under 100 MB
- Use texture atlases
- Minimize script count
- Test on slow internet connection

### For Mobile Success
- Test on older devices (3-4 years old)
- Heat test: Play for 30 minutes straight
- Battery test: Monitor drain rate
- Always test on real device before release

### For All Platforms
- Start with PC (fastest development)
- Add platform-specific code last
- Test regularly on all targets
- Profile performance weekly

---

## 🎉 You're Ready!

With this guide, your game will run smoothly on PC, Mobile, and WebGL!

**Next Steps:**
1. Implement InputManager (Week 1)
2. Test on PC throughout development
3. Add mobile controls in Week 5
4. Test WebGL weekly

**Remember:** Build for PC first, optimize for other platforms later!
