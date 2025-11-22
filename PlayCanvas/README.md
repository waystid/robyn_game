# 🎮 Cozy Game Template
## Waystid Game Forge - PlayCanvas Edition

A complete, production-ready game template for creating cozy farming/adventure games with PlayCanvas.

---

## 📋 Table of Contents

- [Features](#features)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Systems Overview](#systems-overview)
- [Customization Guide](#customization-guide)
- [API Reference](#api-reference)
- [Mobile Support](#mobile-support)
- [Performance](#performance)
- [License](#license)

---

## ✨ Features

### Core Gameplay Systems
- ✅ **Inventory System** - 30-slot inventory with stacking, currency, weight
- ✅ **Farming System** - Plant seeds, water crops, growth stages, harvest
- ✅ **Quest System** - Objectives, tracking, rewards, multiple quest types
- ✅ **Dialogue System** - NPC conversations, choices, branching dialogue
- ✅ **Magic System** - Learn spells, mana costs, cooldowns, spell effects

### World & Time
- ✅ **Day/Night Cycle** - Dynamic lighting based on time of day
- ✅ **Weather System** - Clear, cloudy, rain, storm, snow with transitions
- ✅ **Seasons** - Spring, summer, autumn, winter (28 days each)
- ✅ **Time Management** - Configurable time scale, pause/resume

### UI & UX
- ✅ **Complete HTML/CSS UI** - Professional, responsive interface
- ✅ **10+ UI Panels** - Inventory, quests, dialogue, shop, crafting, settings
- ✅ **Real-time HUD** - Health, mana, XP, time, weather, currency
- ✅ **Notifications** - Toast notifications for game events
- ✅ **Mobile-Responsive** - Adaptive UI for all screen sizes
- ✅ **Touch Controls** - Virtual joystick and buttons for mobile

### Technical
- ✅ **Save/Load System** - LocalStorage persistence with auto-save
- ✅ **Event Bus** - Decoupled system communication
- ✅ **Data-Driven** - JSON-based content (items, quests, plants, spells)
- ✅ **Singleton Pattern** - Consistent manager architecture
- ✅ **Performance Optimized** - 60 FPS target, efficient rendering

---

## 🚀 Quick Start

### 1. Create PlayCanvas Project

1. Go to [playcanvas.com](https://playcanvas.com)
2. Create new project
3. Name it "My Cozy Game"

### 2. Import Template Files

Upload these files to your PlayCanvas project:

```
PlayCanvas/
├── scripts/
│   ├── managers/
│   │   ├── core-managers.js
│   │   └── game-systems.js
│   └── player/
│       └── player-world-systems.js
├── data/
│   ├── items.json
│   ├── quests.json
│   ├── plants.json
│   └── spells.json
└── ui/
    ├── index.html
    ├── styles.css
    └── ui-controller.js
```

### 3. Setup Scene

1. Create empty entities:
   - `GameManager` (add core-managers.js scripts)
   - `Player` (add playerController script)
   - `MainCamera` (camera component)
   - `DirectionalLight` (light component)

2. Configure GameManager:
   - Attach: `EventBus`, `SaveSystem`, `GameManager` scripts
   - Attach: `InventoryManager`, `FarmingManager`, `QuestManager`, `DialogueManager`, `MagicManager`

3. Configure Player:
   - Attach: `PlayerController` script
   - Set speed: 5
   - Assign camera reference

4. Configure TimeManager:
   - Create entity: `TimeManager`
   - Attach: `TimeManager` script
   - Assign directional light reference

### 4. Setup UI

1. Replace `index.html` with template HTML
2. Add `styles.css` to your project
3. Create entity `UIController`, attach `ui-controller.js`

### 5. Launch!

Click "Launch" and your game is running! 🎉

---

## 📁 Project Structure

```
PlayCanvas Project
├── scripts/
│   ├── managers/
│   │   ├── core-managers.js              # GameManager, EventBus, SaveSystem
│   │   └── game-systems.js               # Inventory, Farming, Quest, Dialogue, Magic
│   ├── player/
│   │   └── player-world-systems.js       # Player, Time, Weather
│   └── ui/
│       └── ui-controller.js              # UI management
├── data/
│   ├── items.json                        # Item definitions
│   ├── quests.json                       # Quest definitions
│   ├── plants.json                       # Plant definitions
│   ├── spells.json                       # Spell definitions
│   └── npcs.json                         # NPC definitions
├── ui/
│   ├── index.html                        # Main HTML UI
│   └── styles.css                        # UI styles
└── assets/
    ├── models/                           # 3D models
    ├── textures/                         # Textures
    ├── sounds/                           # Audio files
    └── fonts/                            # Custom fonts
```

---

## 🎮 Systems Overview

### GameManager
Central game coordinator. Loads data, manages state, coordinates systems.

```javascript
var gameManager = GameManager.getInstance();
gameManager.setGameState('playing');
gameManager.newGame();
gameManager.quitGame();
```

### EventBus
Global event system for decoupled communication.

```javascript
var eventBus = EventBus.getInstance();

// Listen to events
eventBus.on('inventory:changed', function(items, currency) {
    console.log('Inventory updated');
}, this);

// Fire events
eventBus.fire('quest:completed', questData);
```

### InventoryManager
Manages items, currency, and inventory slots.

```javascript
var inventory = InventoryManager.getInstance();

// Add item
inventory.addItem('apple', 5);

// Remove item
inventory.removeItem('apple', 2);

// Check item
if (inventory.hasItem('apple', 3)) {
    console.log('You have enough apples!');
}

// Currency
inventory.addCurrency('gold', 100);
inventory.removeCurrency('gold', 50);
```

### FarmingManager
Plant seeds, water crops, and harvest.

```javascript
var farming = FarmingManager.getInstance();

// Plant seed
farming.plantSeed('plant_moonflower', gridX, gridY);

// Water plant
farming.waterPlant(gridX, gridY);

// Harvest plant
farming.harvestPlant(gridX, gridY);
```

### QuestManager
Quest tracking and management.

```javascript
var questManager = QuestManager.getInstance();

// Start quest
questManager.startQuest('main_ancient_library');

// Update objective
questManager.updateObjective('main_ancient_library', 0, 1);

// Complete quest (automatic when all objectives done)
```

### TimeManager
Day/night cycle and seasons.

```javascript
var timeManager = TimeManager.getInstance();

// Get time
var timeString = timeManager.getTimeString(); // "06:30"
var isNight = timeManager.isNight();

// Get season
var season = timeManager.currentSeason; // 'spring'
```

---

## 🎨 Customization Guide

### Changing Colors

Edit `styles.css` CSS variables:

```css
:root {
    /* Primary color (buttons, highlights) */
    --color-primary: #4A90E2;

    /* Background colors */
    --color-background: #2C3E50;
    --color-panel: rgba(44, 62, 80, 0.95);

    /* Status colors */
    --color-health: #E74C3C;
    --color-mana: #3498DB;
    --color-exp: #F39C12;
}
```

### Adding Custom Items

Edit `data/items.json`:

```json
{
  "itemID": "my_custom_item",
  "name": "Magic Potion",
  "description": "A mysterious potion",
  "rarity": "rare",
  "stackable": true,
  "maxStack": 99,
  "sellValue": 50
}
```

### Creating New Quests

Edit `data/quests.json`:

```json
{
  "questID": "my_quest",
  "questName": "My Quest",
  "description": "Do something amazing",
  "questType": "side",
  "objectives": [
    {
      "description": "Collect 5 apples",
      "targetCount": 5,
      "targetItemID": "apple"
    }
  ],
  "rewards": {
    "experience": 100,
    "gold": 50
  }
}
```

### Adding New UI Panels

1. Add HTML in `index.html`:
```html
<div id="my-panel" class="panel hidden">
    <div class="panel-header">
        <h2>My Panel</h2>
        <button class="btn-close">×</button>
    </div>
    <div class="panel-content">
        <!-- Your content -->
    </div>
</div>
```

2. Add toggle function in `ui-controller.js`:
```javascript
UIController.prototype.toggleMyPanel = function() {
    if (this.currentPanel === this.myPanel) {
        this.closeCurrentPanel();
    } else {
        this.openPanel(this.myPanel);
    }
};
```

---

## 📱 Mobile Support

### Touch Controls
The template includes virtual joystick and buttons for mobile devices.

### Responsive UI
All UI panels automatically adapt to mobile screen sizes:
- Larger touch targets (44px minimum)
- Simplified layouts
- Bottom-aligned controls
- Safe area support

### Performance
Optimized for mobile browsers:
- 30 FPS target on mobile
- Reduced particle effects
- LOD system
- Asset streaming

---

## ⚡ Performance

### Optimization Tips

1. **Use Object Pooling** for frequently created/destroyed objects
2. **Limit Draw Calls** - Combine meshes where possible
3. **Use Texture Atlases** - Reduce texture switches
4. **Implement LOD** - Lower detail for distant objects
5. **Lazy Load** - Load assets on demand

### Performance Targets

| Platform | FPS | Resolution |
|----------|-----|------------|
| Desktop | 60 | 1920x1080 |
| Tablet | 60 | 1280x720 |
| Mobile | 30 | 720x1280 |

---

## 🔧 API Reference

### GameManager API

```javascript
// Singleton access
GameManager.getInstance()

// State management
gameManager.setGameState(newState)
gameManager.newGame()
gameManager.continueGame()
gameManager.quitGame()

// Data access
gameManager.itemsData
gameManager.questsData
gameManager.plantsData
gameManager.spellsData
```

### InventoryManager API

```javascript
// Singleton access
InventoryManager.getInstance()

// Items
inventory.addItem(itemId, quantity)
inventory.removeItem(itemId, quantity)
inventory.hasItem(itemId, quantity)
inventory.getItemCount(itemId)

// Currency
inventory.addCurrency(type, amount)
inventory.removeCurrency(type, amount)
inventory.hasCurrency(type, amount)

// Save/Load
inventory.getSaveData()
inventory.loadSaveData(data)
```

### EventBus API

```javascript
// Singleton access
EventBus.getInstance()

// Event handling
eventBus.on(eventName, callback, scope)
eventBus.off(eventName, callback, scope)
eventBus.once(eventName, callback, scope)
eventBus.fire(eventName, arg1, arg2, ...)
```

---

## 📚 Events Reference

### Game Events
- `data:loaded` - All JSON data loaded
- `game:stateChanged` - Game state changed
- `game:saved` - Game saved
- `game:loaded` - Save file loaded

### Inventory Events
- `inventory:changed` - Inventory updated

### Quest Events
- `quest:started` - Quest started
- `quest:updated` - Quest objective updated
- `quest:objectiveCompleted` - Objective completed
- `quest:completed` - Quest completed

### Dialogue Events
- `dialogue:started` - Dialogue started
- `dialogue:nodeChanged` - Moved to new dialogue node
- `dialogue:ended` - Dialogue ended

### Farming Events
- `farming:planted` - Seed planted
- `farming:watered` - Plant watered
- `farming:ready` - Plant ready to harvest
- `farming:harvested` - Plant harvested

### Time Events
- `time:tick` - Time advanced (every second)
- `time:hourChanged` - Hour changed
- `time:dayChanged` - Day changed
- `time:seasonChanged` - Season changed

### Weather Events
- `weather:changing` - Weather changing
- `weather:changed` - Weather changed

---

## 🤝 Contributing to Waystid Game Forge

This template is part of the [Waystid Game Forge](https://github.com/waystid/waystid-game-forge) project.

To contribute:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## 📄 License

MIT License - Feel free to use this template for your own games!

---

## 🙏 Credits

**Template Created By:** Waystid Game Forge
**Engine:** PlayCanvas
**Fonts:** Google Fonts (Press Start 2P, Roboto)

---

## 📞 Support

- **Documentation:** [Full docs on GitHub](https://github.com/waystid/waystid-game-forge)
- **Community:** [Discord Server](#)
- **Issues:** [GitHub Issues](https://github.com/waystid/waystid-game-forge/issues)

---

**Happy Game Making! 🎮**

*Powered by Waystid Game Forge*
