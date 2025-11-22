# 🎨 Asset Acquisition & Integration Guide
## Mixamo, Asset Store, and AI-Generated Art for 3D Low Poly Chibi Style

> **Your Style:** 3D Low Poly, Cute Chibi, Cozy Aesthetic

---

## 📚 Table of Contents

1. [Mixamo Character Setup](#mixamo-character-setup)
2. [Unity Asset Store Recommendations](#unity-asset-store-recommendations)
3. [AI Art Generation](#ai-art-generation)
4. [Asset Integration Workflows](#asset-integration-workflows)
5. [Optimization & Best Practices](#optimization--best-practices)

---

## 🎭 Mixamo Character Setup

### What is Mixamo?
- **Free** character animation service by Adobe
- Automatic rigging for 3D characters
- Hundreds of free animations
- Perfect for prototyping

### Step 1: Download Mixamo Character

1. **Go to:** https://www.mixamo.com
2. **Sign in** with Adobe account (free)
3. **Select a character:**
   - **Search:** "chibi" or "stylized"
   - **Recommended:**
     - "Kaya" (cute, gender-neutral)
     - "Amy" (chibi proportions)
     - "Peasant Girl" (simple, low poly)
   - Click character → "Download"

4. **Download Settings:**
   - **Format:** FBX for Unity (.fbx)
   - **Pose:** T-pose
   - **Skin:** With Skin (check)
   - Click "Download"

### Step 2: Download Animations

**Essential Animations for MVP:**

1. **Idle** (standing still)
   - Search: "Idle"
   - Download: "Idle" or "Breathing Idle"

2. **Walking**
   - Search: "Walk"
   - Download: "Walking" or "Casual Walk"

3. **Running** (optional)
   - Search: "Run"
   - Download: "Running" or "Jog"

4. **Interacting**
   - Search: "Pick up" → Download "Picking Up"
   - Search: "Use" → Download "Using Tablet" (repurpose for crafting)

5. **Casting Magic**
   - Search: "Spell" → Download "Spellcasting"
   - Search: "Magic" → Download "Casting Spell"

6. **Emotes**
   - Search: "Happy" → Download "Happy Idle"
   - Search: "Wave" → Download "Waving"

**Download Settings for Animations:**
- **Format:** FBX for Unity (.fbx)
- **Skin:** Without Skin (uncheck)
- **Frame Rate:** 30
- **Keyframe Reduction:** None

### Step 3: Import to Unity

1. **Create folder:** `Assets/Game/Art/Mixamo/`
2. **Drag and drop** all .fbx files into this folder
3. Unity will import automatically

### Step 4: Setup Character in Unity

#### Configure Character FBX
1. Select character FBX in Project window
2. In Inspector:
   - **Rig Tab:**
     - Animation Type: **Humanoid**
     - Avatar Definition: **Create From This Model**
     - Click "Apply"

#### Configure Animation FBXs
1. Select animation FBX
2. In Inspector:
   - **Rig Tab:**
     - Animation Type: **Humanoid**
     - Avatar Definition: **Copy From Other Avatar**
     - Source: Select your character's Avatar
   - **Animation Tab:**
     - Rename clip (e.g., "Idle", "Walk")
     - Loop Time: ✅ (check for Idle, Walk, Run)
     - Loop Pose: ✅ (check)
     - Click "Apply"

### Step 5: Create Animator Controller

1. Right-click in **Prefabs/Player/** folder
2. Create → **Animator Controller**
3. Name it: `PlayerAnimator`
4. Double-click to open Animator window

5. **Add animations:**
   - Drag animation clips into Animator window
   - Create states: Idle, Walk, Interact, Cast
   - Create transitions between states

**Basic Setup:**
```
[Entry] → [Idle]
[Idle] ↔ [Walk] (parameter: Speed > 0.1)
[Any State] → [Cast] (trigger: CastSpell)
[Cast] → [Idle] (has exit time)
```

6. **Add to player:**
   - Select Player prefab
   - Add component: **Animator**
   - Assign PlayerAnimator controller

---

## 🛍️ Unity Asset Store Recommendations

### Free Asset Packs for Low Poly Chibi Style

#### 1. **3D Characters**

**Recommended Packs:**
- **"Low Poly Ultimate Pack"** (free) - Animals, NPCs
- **"Polygon Starter Pack"** (free) - Environment
- **"Simple People"** (free) - NPCs
- **"Quirky Series"** ($) - Cute animals

**How to Get:**
1. Open Unity Editor
2. **Window** → **Asset Store**
3. Search for pack name
4. Click "Add to My Assets"
5. Click "Import"

#### 2. **Environment Assets**

**Recommended:**
- **"Low Poly Nature Pack"** (free)
  - Trees, rocks, plants
  - Perfect for forest area

- **"Polygon Town Pack"** (free)
  - Buildings, props
  - Good for village

- **"Simple Polygon Nature"** (free)
  - Grass, flowers, stones

**Search terms:** "low poly", "polygon", "stylized", "cute"

#### 3. **Props & Items**

**Recommended:**
- **"Low Poly Props"** (free) - Generic items
- **"Magic Pack"** (search) - Wands, potions, crystals
- **"Garden Pack"** (search) - Plants, tools

#### 4. **VFX (Particles)**

**Essential for Reward Loops:**
- **"Cartoon FX Free"** (free) - Sparkles, magic
- **"Particle Effects Pack"** (free) - General VFX
- **"Magic Effects Free"** (free) - Spells, auras

**Must-have particles:**
- ✨ Sparkle (for pickups)
- 💫 Magic circle (for spells)
- 🌟 Star burst (for rewards)
- 💎 Shimmer (for rare items)

#### 5. **UI Assets**

**Recommended:**
- **"UI Pack: RPG"** (free) - Inventory, health bars
- **"Casual Game UI"** (free) - Buttons, panels
- **"Fantasy Wooden GUI"** ($) - Cozy aesthetic

---

## 🤖 AI Art Generation

### For Textures and 2D Art

#### Recommended AI Tools

**1. Leonardo.ai** (Free tier available)
- **Use for:** Item icons, UI elements, concept art
- **Style prompt:** "low poly, chibi, cute, pastel colors, game asset, white background"

**2. DALL-E / ChatGPT** (Paid)
- **Use for:** Concept art, reference images
- **Prompt example:** "cute chibi dragon character, 3D low poly style, pastel colors, game asset"

**3. Midjourney** (Paid)
- **Use for:** High-quality concept art
- **Style:** "--style cute --niji" for anime/chibi style

#### Example Prompts

**For Item Icons:**
```
"magic wand icon, 3D low poly style, cute, glowing blue crystal,
simple design, game asset, white background, isometric view"
```

**For Character Concepts:**
```
"cute chibi dragon NPC, friendly expression, pastel purple scales,
low poly 3D style, T-pose, game character, white background"
```

**For Environment Concepts:**
```
"cozy magical village, low poly 3D style, pastel colors,
cute houses, gardens, chibi aesthetic, game environment"
```

### For 3D Models (Advanced)

**Text-to-3D Tools:**
- **Meshy.ai** (Free trial) - Text to 3D model
- **Rodin** - AI 3D generation
- **Luma AI** - 3D from images

**Workflow:**
1. Generate concept with DALL-E/Leonardo
2. Use concept as reference in Meshy.ai
3. Download as .fbx or .obj
4. Import to Unity

**Note:** AI 3D is still experimental - best for simple props, not characters

---

## 🔧 Asset Integration Workflows

### Workflow 1: Character (Mixamo to Unity)

```
1. Download character from Mixamo (.fbx)
2. Import to Assets/Game/Art/Mixamo/
3. Configure as Humanoid
4. Download animations (.fbx)
5. Configure animations, copy Avatar
6. Create Animator Controller
7. Create prefab with character + Animator
8. Test in scene
```

**Time:** 30 minutes for first character

### Workflow 2: Environment (Asset Store to Scene)

```
1. Find free pack on Asset Store
2. Import to Unity
3. Browse imported assets
4. Drag props into scene
5. Adjust scale/rotation
6. Create prefabs for reusable objects
7. Add colliders if needed
```

**Time:** 15 minutes per pack

### Workflow 3: Props (3D Model to Unity)

```
1. Download/create 3D model (.fbx, .obj)
2. Import to Assets/Game/Art/Models/
3. Check import settings:
   - Scale Factor: Adjust if too big/small
   - Generate Colliders: ✅ (if interactable)
4. Create material and assign
5. Add to scene or create prefab
6. Add components (Item, Selectable, etc.)
```

**Time:** 10 minutes per prop

### Workflow 4: UI Icons (AI to Unity)

```
1. Generate icon with Leonardo.ai
2. Download as PNG (512x512 or higher)
3. Import to Assets/Game/Art/Textures/Icons/
4. Configure as Sprite:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single
   - Pixels Per Unit: 100
5. Use in UI Image components
```

**Time:** 5 minutes per icon

### Workflow 5: Particles (Asset Store to Effect)

```
1. Import Cartoon FX Free pack
2. Find prefab (e.g., "Sparkle")
3. Create folder: Assets/Game/Prefabs/VFX/
4. Duplicate prefab to your folder
5. Customize colors/size
6. Reference in scripts:
   - public GameObject pickupParticle;
   - Instantiate(pickupParticle, position, Quaternion.identity);
```

**Time:** 10 minutes per effect

---

## 🎨 Creating Cohesive Low Poly Chibi Style

### Visual Consistency Rules

**1. Color Palette**
Use pastel, saturated colors:
```
🟣 Purple: #B794F6 (magic)
🟢 Green: #86E3CE (nature)
🔵 Blue: #A8DADC (water/sky)
🟡 Yellow: #FFE156 (light/sun)
🔴 Pink: #FFB6D9 (cute accent)
```

**2. Model Guidelines**
- **Triangle count:** 500-5000 per model
- **Proportions:** Big head (1:3 body ratio)
- **Features:** Simple, rounded shapes
- **No:** Complex details, realistic textures

**3. Texture Style**
- **Solid colors** > Detailed textures
- **Gradients** for depth
- **Simple patterns** (dots, stars)
- **Avoid:** Photo-realistic textures

### Unity Materials for Low Poly

**Create Standard Material:**
1. Right-click in Materials folder
2. Create → Material
3. Name: "Character_Skin" (example)
4. Set color: Pastel tone
5. **Smoothness:** 0.2-0.4 (slightly rough)
6. **Metallic:** 0 (no shine)

**Shader Options:**
- **Standard:** Default, works everywhere
- **Unlit:** Better performance, flat look
- **Toon Shader:** Cel-shaded look (requires download)

---

## 📦 Asset Organization

### Folder Structure

```
Assets/Game/Art/
├── Mixamo/
│   ├── Characters/
│   │   ├── Player.fbx
│   │   └── NPC_Base.fbx
│   └── Animations/
│       ├── Idle.fbx
│       ├── Walk.fbx
│       └── Cast.fbx
│
├── Models/
│   ├── Environment/
│   │   ├── Trees/
│   │   ├── Rocks/
│   │   └── Buildings/
│   ├── Items/
│   │   ├── MagicWand.fbx
│   │   └── Potion.fbx
│   └── NPCs/
│       ├── Dragon.fbx
│       └── Sphinx.fbx
│
├── Materials/
│   ├── Characters/
│   ├── Environment/
│   └── Items/
│
├── Textures/
│   ├── Icons/
│   │   ├── item_wood.png
│   │   ├── item_crystal.png
│   │   └── spell_growth.png
│   ├── UI/
│   └── Patterns/
│
└── Particles/
    ├── Magic/
    ├── Pickups/
    └── Environment/
```

### Naming Conventions

**Models:**
- `character_player.fbx`
- `npc_dragon.fbx`
- `prop_rock_01.fbx`
- `building_house_small.fbx`

**Textures:**
- `icon_item_wood.png`
- `ui_button_confirm.png`
- `pattern_stars.png`

**Materials:**
- `mat_character_skin.mat`
- `mat_grass.mat`
- `mat_crystal_blue.mat`

**Prefabs:**
- `Player.prefab`
- `NPC_Dragon.prefab`
- `Item_MagicWand.prefab`

---

## ⚡ Optimization Tips

### For Mobile & WebGL

**1. Model Optimization**
- **Use:** LOD (Level of Detail) for distant objects
- **Target:** Under 5k triangles for characters
- **Combine:** Merge small props into single mesh

**2. Texture Optimization**
```
Desktop: 1024x1024 max
WebGL: 512x512 max
Mobile: 512x512 max (or 256x256 for small items)

Compression:
- Android: ASTC
- iOS: ASTC
- WebGL: DXT
```

**How to compress:**
1. Select texture in Unity
2. Inspector → Platform override
3. Select platform (Android, iOS, WebGL)
4. Max Size: 512
5. Compression: ASTC (or best for platform)

**3. Particle Optimization**
- Max particles per system: 50
- Max particle systems active: 5
- Use GPU Instancing on particle materials

**4. Animation Optimization**
- Compress animation clips
- Remove unnecessary keyframes
- Use Animation Compression: Optimal

---

## 🎯 Asset Checklist by Week

### Week 1: Foundation Assets
- [ ] Player character (Mixamo)
- [ ] Basic animations (Idle, Walk, Interact)
- [ ] 3 basic materials (grass, stone, wood)
- [ ] Pickup particle effect
- [ ] 5 item icons (wood, stone, crystal, wand, potion)

### Week 2: Crafting & Plants
- [ ] Plant models (3 types, 4 stages each)
- [ ] Workbench model
- [ ] House model
- [ ] Crafting particle effect
- [ ] 5 more item icons

### Week 3: NPCs
- [ ] Dragon model (Mixamo or Asset Store)
- [ ] Sphinx model
- [ ] NPC idle animations
- [ ] Quest UI elements

### Week 4: Creatures & Polish
- [ ] Firefly model
- [ ] Baby Slime model
- [ ] Magic spell VFX
- [ ] Environment decorations

### Week 5: Final Polish
- [ ] All missing icons
- [ ] Additional particles
- [ ] UI beautification
- [ ] Screenshot/promo art

---

## 🔍 Asset Finding Strategies

### Unity Asset Store Search Terms

**For Characters:**
- "low poly character"
- "chibi"
- "cute character"
- "stylized character"
- "cartoon character"

**For Environment:**
- "low poly nature"
- "polygon environment"
- "stylized world"
- "cartoon world"

**For Props:**
- "magic items"
- "fantasy props"
- "cute items"
- "game assets"

### Free Asset Websites

**3D Models:**
- **Sketchfab** (sketchfab.com) - Filter by "Downloadable" + "Free"
- **Poly Pizza** (poly.pizza) - Low poly models
- **Kenney Assets** (kenney.nl) - Game assets

**Textures:**
- **Poly Haven** (polyhaven.com) - Free textures
- **Textures.com** (free tier)

**UI/Icons:**
- **Game-icons.net** - Free game icons
- **Flaticon** (free tier) - Vector icons

---

## 💡 Pro Tips

### Start with Placeholders
**Week 1-2:** Use Unity primitives (cubes, spheres)
- Quick to test
- Focus on gameplay
- Replace with art later

**Week 3-4:** Add real assets gradually
- One category at a time
- Test performance
- Iterate on style

**Week 5:** Final art pass
- Replace all placeholders
- Add polish
- Screenshot-ready

### Mixamo Alternatives
If Mixamo character doesn't fit:
- **Create in Blender:** Free 3D software
- **Commission artist:** Fiverr ($20-50 for simple chibi)
- **Use Asset Store pack:** "Simple People" or similar

### AI Art Best Practices
- Generate multiple variations
- Use as reference, not final
- Maintain consistent style
- Always check license/terms

---

## ✅ Integration Checklist

Before using any asset:
- [ ] License allows commercial use (if applicable)
- [ ] File format compatible (.fbx, .png, .wav)
- [ ] Polygon count appropriate for platform
- [ ] Texture size optimized
- [ ] Style matches game aesthetic
- [ ] Properly named and organized
- [ ] Prefab created (for reusable objects)
- [ ] Tested in scene

---

## 🎉 You're Ready to Build!

With Mixamo, Asset Store, and AI tools, you have everything needed for a beautiful low poly chibi game!

**Next Steps:**
1. Download first Mixamo character (Week 1, Day 1)
2. Import free environment pack
3. Generate item icons as needed
4. Build gameplay first, art second

**Remember:** Placeholder art is your friend during development!
