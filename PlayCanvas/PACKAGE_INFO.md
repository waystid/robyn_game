# 🎮 Cozy Game Template - PlayCanvas Edition
## Waystid Game Forge Template Package v1.0.0

---

## 📦 Package Contents

This package contains a complete, production-ready game template for creating cozy farming/adventure games with PlayCanvas.

### Directory Structure

```
PlayCanvas/
├── scripts/
│   ├── managers/
│   │   ├── core-managers.js              # EventBus, SaveSystem, GameManager
│   │   └── game-systems.js               # Inventory, Farming, Quest, Dialogue, Magic
│   └── player/
│       └── player-world-systems.js       # Player, Time, Weather
├── ui/
│   ├── index.html                        # Complete UI structure
│   ├── styles.css                        # Responsive styling
│   └── ui-controller.js                  # UI integration logic
├── data/
│   ├── items.json                        # 6 example items
│   ├── quests.json                       # 3 example quests
│   ├── plants.json                       # 4 example plants
│   ├── spells.json                       # 6 example spells
│   └── npcs.json                         # 3 example NPCs
├── README.md                             # Complete documentation
├── UI_VALIDATION.md                      # Integration validation guide
└── PACKAGE_INFO.md                       # This file
```

---

## 🚀 Quick Start (5 Minutes)

### Step 1: Create PlayCanvas Project

1. Go to [playcanvas.com](https://playcanvas.com)
2. Sign in or create an account
3. Click "New Project"
4. Name it "My Cozy Game"
5. Choose "Empty Project"

### Step 2: Import JavaScript Files

Upload these files to PlayCanvas Scripts folder:

1. **scripts/managers/core-managers.js**
   - EventBus (global event system)
   - SaveSystem (LocalStorage persistence)
   - GameManager (game state coordinator)

2. **scripts/managers/game-systems.js**
   - InventoryManager (30-slot inventory, currency)
   - FarmingManager (plant, water, harvest)
   - QuestManager (objectives, rewards)
   - DialogueManager (NPC conversations)
   - MagicManager (spells, cooldowns)

3. **scripts/player/player-world-systems.js**
   - PlayerController (WASD movement, stats)
   - TimeManager (day/night cycle, seasons)
   - WeatherSystem (dynamic weather)

4. **ui/ui-controller.js**
   - UIController (UI integration)

### Step 3: Setup HTML/CSS

1. Replace your project's `index.html` with `ui/index.html`
2. Upload `ui/styles.css` to your project

### Step 4: Import Data Files

Upload JSON files to PlayCanvas Assets:

1. Create folder: `data/`
2. Upload all `.json` files from `data/` folder

### Step 5: Create Scene Entities

Create these entities in your scene:

#### GameManager Entity
```
GameManager/
├── Script: eventBus
├── Script: saveSystem
├── Script: gameManager
├── Script: inventoryManager
├── Script: farmingManager
├── Script: questManager
├── Script: dialogueManager
└── Script: magicManager
```

#### Player Entity
```
Player/
├── Model (optional - add your 3D model)
├── Collision Component
└── Script: playerController
    - speed: 5
    - runMultiplier: 1.5
    - camera: [drag MainCamera here]
```

#### TimeManager Entity
```
TimeManager/
└── Script: timeManager
    - timeScale: 60
    - startHour: 6
    - directionalLight: [drag DirectionalLight here]
```

#### WeatherSystem Entity
```
WeatherSystem/
└── Script: weatherSystem
```

#### UIController Entity
```
UIController/
└── Script: uiController
```

#### MainCamera Entity
```
MainCamera/
└── Camera Component
    Position: (0, 10, 10)
    Look at: Player
```

#### DirectionalLight Entity
```
DirectionalLight/
└── Light Component
    Type: Directional
    Intensity: 1.0
```

### Step 6: Launch!

Click the **"Launch"** button and your game is running! 🎉

---

## 🎮 Controls

### Desktop
- **WASD** or **Arrow Keys**: Move
- **Shift**: Run
- **I**: Open/Close Inventory
- **Q**: Open/Close Quest Log
- **ESC**: Close current panel / Pause menu

### Mobile
- **Virtual Joystick**: Move
- **Action Buttons**: Interact
- **Touch UI**: Tap panels to open

---

## ✨ Features Included

### Core Systems (Production-Ready)
✅ Inventory System (30 slots, stacking, currency, weight)
✅ Farming System (plant, water, harvest, growth stages)
✅ Quest System (objectives, tracking, rewards)
✅ Dialogue System (NPC conversations, choices, branching)
✅ Magic System (spells, mana, cooldowns)
✅ Day/Night Cycle (dynamic lighting, time progression)
✅ Weather System (5 weather types with transitions)
✅ Save/Load System (LocalStorage, auto-save)
✅ Event Bus (decoupled system communication)

### UI System (Complete & Responsive)
✅ Game HUD (health, mana, stamina, XP bars)
✅ Time & Weather Display
✅ Currency System (gold, silver, gems)
✅ Inventory Panel (30-slot grid, rarity colors)
✅ Quest Log (active/completed quests)
✅ Dialogue Box (NPC conversations, choices)
✅ Shop Interface (buy/sell items)
✅ Crafting Panel (recipe system)
✅ Settings Menu (audio, graphics, controls)
✅ Mobile Touch Controls (virtual joystick, buttons)
✅ Notification System (toast notifications)

### Data-Driven Design
✅ JSON-based content (easy to edit without code)
✅ Example items (6 items with different types)
✅ Example quests (3 quests: main, side, collection)
✅ Example plants (4 crops with different seasons)
✅ Example spells (6 spells: offensive, healing, utility)
✅ Example NPCs (3 NPCs with dialogue trees)

---

## 🎨 Customization

### Change Theme Colors

Edit `ui/styles.css` CSS variables:

```css
:root {
    --color-primary: #4A90E2;        /* Blue theme */
    --color-background: #2C3E50;     /* Dark background */
    --color-health: #E74C3C;         /* Red health bar */
    --color-mana: #3498DB;           /* Blue mana bar */
    --color-exp: #F39C12;            /* Gold XP bar */
}
```

### Add New Items

Edit `data/items.json`:

```json
{
  "itemID": "magic_crystal",
  "name": "Magic Crystal",
  "description": "A powerful magical artifact",
  "rarity": "legendary",
  "stackable": false,
  "sellValue": 1000
}
```

### Create New Quests

Edit `data/quests.json`:

```json
{
  "questID": "my_custom_quest",
  "questName": "My Quest",
  "description": "An amazing adventure",
  "questType": "side",
  "objectives": [...],
  "rewards": { "gold": 100, "experience": 50 }
}
```

### Add More Plants

Edit `data/plants.json`:

```json
{
  "plantID": "plant_mystical_herb",
  "name": "Mystical Herb",
  "growthStages": 5,
  "timePerStage": 180,
  "seasonsToGrow": ["spring"]
}
```

---

## 📚 Documentation

### Full Documentation
See **README.md** for:
- Complete API reference
- System overview
- Event system documentation
- Mobile support details
- Performance optimization tips

### Integration Guide
See **UI_VALIDATION.md** for:
- System integration checklist
- Test scenarios
- Performance targets
- Browser compatibility

---

## 🧪 Testing Your Game

### Console Commands (for testing)

Open browser console (F12) and try these:

```javascript
// Add items
var inv = InventoryManager.getInstance();
inv.addItem('apple', 10);
inv.addCurrency('gold', 500);

// Start quest
var quests = QuestManager.getInstance();
quests.startQuest('main_ancient_library');

// Change time
var time = TimeManager.getInstance();
time.advanceHours(6);
time.advanceDays(1);

// Change weather
var weather = WeatherSystem.getInstance();
weather.changeWeather('rain');

// Start dialogue
var dialogue = DialogueManager.getInstance();
// (Configure NPC dialogue first)
```

---

## 📱 Mobile Support

### Automatically Handles
✅ Touch controls (virtual joystick appears on mobile)
✅ Responsive UI (panels resize for mobile screens)
✅ Performance optimization (30 FPS target on mobile)
✅ Touch-friendly targets (44px minimum)

### Testing on Mobile
1. Launch game in PlayCanvas
2. Click "Publish"
3. Scan QR code with phone
4. Test touch controls

---

## 🔧 System Requirements

### Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

### PlayCanvas Version
- PlayCanvas Engine v1.50+
- ES6 JavaScript support

---

## 📈 Performance Targets

| Platform | Target FPS | Resolution |
|----------|-----------|------------|
| Desktop  | 60 FPS    | 1920×1080 |
| Tablet   | 60 FPS    | 1280×720  |
| Mobile   | 30 FPS    | 720×1280  |

---

## 🎯 What's Next?

### Extend Your Game

This template includes the **foundation**. You can extend it with:

1. **More Content**
   - Add more items, quests, plants, spells
   - Create more NPCs and dialogue
   - Design additional biomes/areas

2. **Advanced Systems** (coming in future updates)
   - Crafting system
   - Building system
   - Fishing system
   - Cooking system
   - NPC relationships
   - Combat system

3. **Visual Polish**
   - Import 3D models
   - Add particle effects
   - Create custom textures
   - Add sound effects and music

4. **Your Unique Features**
   - This is your game template!
   - Add whatever systems make your game unique
   - All systems are designed to be extended

---

## 🤝 Community & Support

### Waystid Game Forge
This template is part of the **Waystid Game Forge** project.

- **GitHub**: https://github.com/waystid/waystid-game-forge
- **Community**: Join our game creation community
- **Issues**: Report bugs or request features

### Contributing
Want to improve this template?
1. Fork the repository
2. Create a feature branch
3. Submit a pull request

---

## 📄 License

**MIT License** - Free to use for your own games!

You can:
✅ Use this template for commercial games
✅ Modify and customize as needed
✅ Share with others
✅ Create derivative works

---

## 🙏 Credits

**Created By**: Waystid Game Forge
**Engine**: PlayCanvas
**Template Version**: 1.0.0
**Last Updated**: November 2024

**Special Thanks**:
- PlayCanvas team for the amazing engine
- The game development community
- All contributors to Waystid Game Forge

---

## 🎮 Happy Game Making!

You now have everything you need to create your own cozy game!

**Remember**:
- Start small and iterate
- Test frequently
- Have fun creating!
- Share your game with the community

**Need Help?**
- Check README.md for detailed docs
- Visit our GitHub for issues/discussions
- Join our community Discord (link in repo)

---

**Now go create something amazing! 🚀**

*Powered by Waystid Game Forge*
