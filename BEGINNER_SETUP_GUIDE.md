# 🚀 Complete Beginner's Setup Guide

> **Note:** This guide assumes you're a complete Unity beginner. Follow each step carefully and in order.

## 📥 Part 1: Installing Unity (30 minutes)

### Step 1: Download Unity Hub
1. Go to https://unity.com/download
2. Click "Download Unity Hub"
3. Install Unity Hub (it's like a launcher for Unity)
4. Open Unity Hub when installation completes

### Step 2: Create Unity Account
1. Click "Sign In" in Unity Hub
2. Create a free Unity account (Personal license is free)
3. Activate your free Personal license

### Step 3: Install Unity Editor
**Recommended Version:** Unity 2022.3 LTS (Long Term Support)

1. In Unity Hub, click "Installs" on the left
2. Click "Install Editor" or "Add"
3. Choose **Unity 2022.3 LTS** (latest patch version)
4. When selecting modules, check:
   - ✅ Microsoft Visual Studio Community (for coding)
   - ✅ WebGL Build Support (for browser games)
   - ✅ Android Build Support (if targeting mobile)
   - ✅ iOS Build Support (if you have a Mac and targeting iOS)
5. Click "Install" (this takes 30-60 minutes)

---

## 🛒 Part 2: Getting Survival Engine (15 minutes)

### Option A: If You Already Own It
1. Open Unity Asset Store in your browser
2. Go to "My Assets"
3. Find "Survival Engine"
4. Download to your asset library

### Option B: If You Need to Purchase
1. Go to Unity Asset Store: https://assetstore.unity.com
2. Search for "Survival Engine"
3. Purchase the template (check for sales!)
4. It will appear in "My Assets"

### What is Survival Engine?
- A complete template with inventory, crafting, building, NPCs
- Saves you months of programming
- We'll customize it for your magical game

---

## 🎮 Part 3: Creating Your Project (15 minutes)

### Step 1: Create New Unity Project
1. Open Unity Hub
2. Click "Projects" → "New Project"
3. Select **3D (Built-in Render Pipeline)** template
   - *Why? Survival Engine uses Built-in RP*
4. Name it: `CozyMagicalGame`
5. Choose a location on your computer
6. Click "Create Project"
7. Wait for Unity to open (first time takes 5-10 minutes)

### Step 2: Import Survival Engine
1. In Unity, go to **Window** → **Asset Store**
2. Sign in with your Unity account
3. Search your downloaded assets for "Survival Engine"
4. Click "Import" or "Download" then "Import"
5. In the import dialog, click "All" then "Import"
6. Wait for import (5-10 minutes)
7. ⚠️ If you get errors, click "Continue" - we'll fix them later

### Step 3: Verify Installation
1. Look in your **Project** window (bottom panel)
2. You should see `Assets/SurvivalEngine` folder
3. Click **Tools** → **Survival Engine** in the top menu
4. If you see a Survival Engine menu, it's installed! ✅

---

## 📁 Part 4: Setting Up Folder Structure (10 minutes)

### Understanding Unity's Folders
- **Assets/** - Everything in your game lives here
- **Scenes/** - Different levels/areas of your game
- **Scripts/** - Your code files (.cs files)
- **Prefabs/** - Reusable game objects

### Create Your Game Folders
1. In Unity's **Project** window, right-click in **Assets**
2. Select **Create** → **Folder**
3. Create these folders one by one:

```
Assets/
├── SurvivalEngine/        (already exists)
└── Game/                  (create this)
    ├── Scenes/            (create this)
    ├── Scripts/           (create this)
    ├── Data/              (create this)
    ├── Prefabs/           (create this)
    ├── Art/               (create this)
    └── UI/                (create this)
```

#### Detailed Subfolder Structure
**In `Assets/Game/Scripts/`, create:**
- Magic/
- NPCs/
- UI/
- CustomActions/
- Utilities/

**In `Assets/Game/Data/`, create:**
- Items/
- Characters/
- Plants/
- Crafting/
- Buildings/
- Attributes/

**In `Assets/Game/Prefabs/`, create:**
- Player/
- NPCs/
- Items/
- Plants/
- Buildings/
- Environment/

**In `Assets/Game/Art/`, create:**
- Models/
- Materials/
- Textures/
- Animations/

### Quick Folder Creation Tip
You can create nested folders faster:
1. Right-click **Assets/Game**
2. Create → Folder → name it "Scripts"
3. Right-click **Scripts** → Create → Folder → name it "Magic"
4. Repeat for all subfolders

---

## 🎨 Part 5: Setting Up for Multi-Platform (15 minutes)

### Configure Build Settings
1. Go to **File** → **Build Settings**
2. Add current scene: Click "Add Open Scenes"

### For PC Build
1. Select **PC, Mac & Linux Standalone**
2. Click "Switch Platform" (if not already selected)

### For WebGL Build
1. In Build Settings, select **WebGL**
2. Click "Switch Platform" (takes a few minutes)
3. Go to **Edit** → **Project Settings** → **Player**
4. Under "WebGL settings":
   - Set "Compression Format" to **Gzip** or **Disabled** (for testing)
   - Memory Size: 512 MB minimum

### For Mobile Build (Optional)
**Android:**
1. In Build Settings, select **Android**
2. Click "Switch Platform"
3. Install Android Build Support if prompted

**iOS (Mac only):**
1. In Build Settings, select **iOS**
2. Click "Switch Platform"

### Platform Considerations
- Start with **PC** for development (fastest testing)
- Test **WebGL** weekly
- Test **Mobile** before final release

---

## 🎯 Part 6: Project Settings for Optimal Performance (10 minutes)

### Quality Settings (Important for Mobile/WebGL)
1. **Edit** → **Project Settings** → **Quality**
2. Create quality levels for each platform:
   - **PC:** "High" quality
   - **Mobile:** "Medium" quality
   - **WebGL:** "Low" to "Medium"

### Input System Setup
1. **Edit** → **Project Settings** → **Player**
2. Under "Other Settings", find "Active Input Handling"
3. Select **Both** (supports old and new input system)
4. Click "Apply" if it asks to restart

### Physics Settings (for better performance)
1. **Edit** → **Project Settings** → **Physics**
2. Set "Default Solver Iterations" to **6**
3. Set "Default Solver Velocity Iterations" to **1**

---

## ✅ Part 7: Verification Checklist

Before moving forward, verify:
- [ ] Unity 2022.3 LTS is installed
- [ ] New project created and opens without errors
- [ ] Survival Engine imported (Assets/SurvivalEngine folder exists)
- [ ] Folder structure created (Assets/Game/ with subfolders)
- [ ] Build settings configured for PC
- [ ] Can access **Tools** → **Survival Engine** menu

---

## 🐛 Troubleshooting Common Issues

### "Survival Engine not in Asset Store"
- Make sure you're logged into the correct Unity account
- Check "My Assets" in your browser first
- Try re-downloading from Unity Asset Store website

### "Import Errors" or "Red Console Messages"
- Most common: Click "Ignore" or "Continue"
- Try: **Assets** → **Reimport All**
- Last resort: Delete `Library` folder and reopen project

### "Unity is Slow"
- Unity needs 8GB+ RAM
- Close other applications
- First project load is always slower

### "Can't Find Project Window"
- Go to **Window** → **General** → **Project**
- Reset layout: **Window** → **Layouts** → **Default**

### "Visual Studio Not Opening Code"
- **Edit** → **Preferences** → **External Tools**
- Set "External Script Editor" to Visual Studio

---

## 📚 Next Steps

After completing setup:
1. ✅ Read `REWARD_LOOPS_DESIGN.md` to understand game feel
2. ✅ Read `PLATFORM_GUIDE.md` for multi-platform tips
3. ✅ Read `ASSET_INTEGRATION_GUIDE.md` for art setup
4. ✅ Start **Week 1** from `DEVELOPMENT_SCHEDULE.md`

---

## 💡 Unity Beginner Tips

### Learn While Building
- **Don't memorize** - reference docs as needed
- **Copy/paste code** - I'll provide everything
- **Test frequently** - hit Play button often
- **Save often** - Ctrl+S (Cmd+S on Mac)

### Understanding the Unity Interface
- **Scene View:** Where you build your world (3D view)
- **Game View:** What players see (camera view)
- **Hierarchy:** List of objects in current scene
- **Project:** Your files and folders
- **Inspector:** Properties of selected object
- **Console:** Shows errors and messages

### Keyboard Shortcuts
- `Ctrl/Cmd + S` - Save
- `Ctrl/Cmd + D` - Duplicate
- `F` - Focus on selected object
- `Ctrl/Cmd + P` - Play/Stop game
- `Ctrl/Cmd + Shift + N` - New GameObject

### Getting Help
- **Unity Learn:** https://learn.unity.com (free tutorials)
- **Unity Manual:** https://docs.unity3d.com/Manual/
- **Ask me!** I'll help with all code and setup

---

## 🎉 Congratulations!

You've completed the setup! Your Unity project is ready for development.

**Time to start building:** Proceed to Week 1 in the development schedule.

Remember: I'll provide all the code - you just need to copy/paste and follow instructions. Don't worry if things seem confusing at first; it gets easier quickly!
