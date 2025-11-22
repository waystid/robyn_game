# Unity to PlayCanvas Conversion Guide
## Cozy Farming/Adventure Game

---

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Platform Comparison](#platform-comparison)
3. [Architecture Translation](#architecture-translation)
4. [Conversion Roadmap](#conversion-roadmap)
5. [System-by-System Migration](#system-by-system-migration)
6. [MVP Definition for PlayCanvas](#mvp-definition-for-playcanvas)
7. [Implementation Priority](#implementation-priority)
8. [Code Examples](#code-examples)

---

## Executive Summary

### Current State
- **Codebase**: ~15,000 lines of C# Unity code
- **Systems**: 30+ game systems (inventory, quests, farming, magic, etc.)
- **Content**: 52+ items (quests, riddles, plants, spells)
- **Architecture**: Unity Component-based with ScriptableObjects

### Target State
- **Platform**: PlayCanvas (WebGL-first engine)
- **Language**: JavaScript/TypeScript
- **Architecture**: PlayCanvas Script Components + JSON data
- **Deployment**: Web-based, mobile-friendly

### Key Challenges
1. ❌ **No C# support** - Must rewrite in JavaScript/TypeScript
2. ❌ **No ScriptableObjects** - Use JSON resources instead
3. ❌ **No Coroutines** - Use async/await or timers
4. ❌ **Different UI system** - HTML/CSS UI or PlayCanvas UI
5. ✅ **Better web performance** - Optimized for WebGL
6. ✅ **Easier deployment** - No build process for web

---

## Platform Comparison

### Unity vs PlayCanvas - Feature Mapping

| Unity Feature | PlayCanvas Equivalent | Notes |
|---------------|----------------------|-------|
| **MonoBehaviour** | `pc.ScriptType` | Similar component system |
| **ScriptableObject** | JSON files + ResourceLoader | Data-driven approach |
| **Coroutines** | `async/await` or timers | Modern JavaScript async |
| **Singleton Pattern** | App-level scripts | Same pattern works |
| **Prefabs** | Templates | Similar concept |
| **Canvas/UI** | HTML UI or `pc.ElementComponent` | More flexible with HTML |
| **Physics** | Ammo.js (Bullet physics) | Similar to Unity |
| **Animation** | Anim Component | Similar state machine |
| **Audio** | Sound Component | Similar API |
| **Input** | Keyboard/Mouse/Touch | Built-in support |
| **Events** | `pc.EventHandler` | Similar to UnityEvents |
| **Time.deltaTime** | `dt` parameter | Passed to update() |
| **Instantiate** | `entity.clone()` | Similar API |
| **Destroy** | `entity.destroy()` | Same concept |

### Language Comparison

```csharp
// Unity C#
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        transform.position += new Vector3(h * speed * Time.deltaTime, 0, 0);
    }
}
```

```javascript
// PlayCanvas JavaScript
var PlayerController = pc.createScript('playerController');

PlayerController.attributes.add('speed', { type: 'number', default: 5 });

PlayerController.prototype.update = function(dt) {
    var h = 0;
    if (this.app.keyboard.isPressed(pc.KEY_RIGHT)) h = 1;
    if (this.app.keyboard.isPressed(pc.KEY_LEFT)) h = -1;

    this.entity.setPosition(
        this.entity.getPosition().x + h * this.speed * dt,
        this.entity.getPosition().y,
        this.entity.getPosition().z
    );
};
```

---

## Architecture Translation

### Current Unity Architecture
```
Unity Project
├── Managers (Singletons)
│   ├── InventoryManager.cs
│   ├── QuestManager.cs
│   └── GameManager.cs
├── ScriptableObjects (Data)
│   ├── Items/
│   ├── Quests/
│   └── Spells/
└── UI (Unity Canvas)
    ├── InventoryUI.cs
    └── QuestUI.cs
```

### Proposed PlayCanvas Architecture
```
PlayCanvas Project
├── Scripts (pc.ScriptType)
│   ├── managers/
│   │   ├── inventory-manager.js
│   │   ├── quest-manager.js
│   │   └── game-manager.js
│   ├── components/
│   │   ├── player-controller.js
│   │   └── interactable.js
│   └── ui/
│       ├── inventory-ui.js
│       └── quest-ui.js
├── Data (JSON)
│   ├── items.json
│   ├── quests.json
│   └── spells.json
├── Templates (Prefabs)
│   ├── Player.template
│   ├── NPC.template
│   └── Items.template
└── UI (HTML/CSS or pc.Element)
    ├── index.html
    └── styles.css
```

---

## Conversion Roadmap

### Phase 1: Project Setup (Week 1)
**Goal**: Create PlayCanvas project and basic structure

**Tasks**:
1. ✅ Create PlayCanvas account and new project
2. ✅ Set up version control (GitHub integration)
3. ✅ Create folder structure (scripts/, data/, templates/)
4. ✅ Set up TypeScript (optional but recommended)
5. ✅ Create basic scene with camera and lighting
6. ✅ Test basic script execution

**Deliverable**: Empty PlayCanvas project with structure

---

### Phase 2: Core Framework (Week 2)
**Goal**: Implement foundational systems

**Priority Systems**:
1. **GameManager** - Core game loop, state management
2. **EventBus** - Global event system (replacing UnityEvents)
3. **SaveSystem** - LocalStorage-based persistence
4. **ResourceLoader** - Load JSON data files
5. **InputManager** - Keyboard/Mouse/Touch input

**Code Example - GameManager**:
```javascript
// scripts/managers/game-manager.js
var GameManager = pc.createScript('gameManager');

GameManager.getInstance = function() {
    if (!GameManager._instance) {
        GameManager._instance = this.app.root.findByName('GameManager').script.gameManager;
    }
    return GameManager._instance;
};

GameManager.prototype.initialize = function() {
    GameManager._instance = this;
    this.gameState = 'loading';
    this.player = null;

    // Load game data
    this.loadGameData();
};

GameManager.prototype.loadGameData = function() {
    // Load JSON files
    this.app.assets.loadFromUrl('data/items.json', 'json', (err, asset) => {
        if (!err) {
            this.itemsData = asset.resource;
            console.log('Items loaded:', this.itemsData.length);
        }
    });
};

GameManager.prototype.update = function(dt) {
    // Game loop
};
```

---

### Phase 3: Player & World (Week 3)
**Goal**: Implement player character and basic world

**Tasks**:
1. ✅ Player entity with controller script
2. ✅ Camera follow system
3. ✅ Grid-based movement or free movement
4. ✅ Collision detection
5. ✅ Basic world tiles/environment
6. ✅ Day/night cycle (TimeManager)

**Player Controller Example**:
```javascript
// scripts/player-controller.js
var PlayerController = pc.createScript('playerController');

PlayerController.attributes.add('speed', { type: 'number', default: 5 });
PlayerController.attributes.add('camera', { type: 'entity' });

PlayerController.prototype.initialize = function() {
    this.moveDir = new pc.Vec3();
};

PlayerController.prototype.update = function(dt) {
    this.moveDir.set(0, 0, 0);

    // WASD movement
    if (this.app.keyboard.isPressed(pc.KEY_W)) this.moveDir.z -= 1;
    if (this.app.keyboard.isPressed(pc.KEY_S)) this.moveDir.z += 1;
    if (this.app.keyboard.isPressed(pc.KEY_A)) this.moveDir.x -= 1;
    if (this.app.keyboard.isPressed(pc.KEY_D)) this.moveDir.x += 1;

    if (this.moveDir.length() > 0) {
        this.moveDir.normalize();
        var pos = this.entity.getPosition();
        pos.add(this.moveDir.mulScalar(this.speed * dt));
        this.entity.setPosition(pos);
    }

    // Camera follow
    if (this.camera) {
        var camPos = this.camera.getPosition();
        camPos.x = pos.x;
        camPos.z = pos.z + 10;
        this.camera.setPosition(camPos);
    }
};
```

---

### Phase 4: Core Game Systems (Week 4-6)
**Goal**: Implement MVP game systems

**Priority Order**:
1. **InventorySystem** (Week 4)
   - Item data from JSON
   - Add/remove items
   - Stack management
   - UI display

2. **FarmingSystem** (Week 4-5)
   - Plant data from JSON
   - Grid-based planting
   - Growth timer
   - Harvest interaction

3. **QuestSystem** (Week 5)
   - Quest data from JSON
   - Objective tracking
   - Completion detection
   - Rewards

4. **DialogueSystem** (Week 5-6)
   - NPC interaction
   - Dialogue trees
   - Choices

5. **ShopSystem** (Week 6)
   - Buy/sell items
   - Currency management

**InventorySystem Example**:
```javascript
// scripts/managers/inventory-manager.js
var InventoryManager = pc.createScript('inventoryManager');

InventoryManager.getInstance = function() {
    return InventoryManager._instance;
};

InventoryManager.prototype.initialize = function() {
    InventoryManager._instance = this;

    this.items = []; // Array of {itemId, quantity, slot}
    this.maxSlots = 30;
    this.currency = { gold: 0, silver: 0, gems: 0 };

    // Load item definitions from JSON
    this.itemDefinitions = {};
    this.loadItemData();

    // Fire event when inventory changes
    this.app.fire('inventory:changed');
};

InventoryManager.prototype.loadItemData = function() {
    this.app.assets.loadFromUrl('data/items.json', 'json', (err, asset) => {
        if (!err) {
            var items = asset.resource;
            items.forEach(item => {
                this.itemDefinitions[item.itemID] = item;
            });
        }
    });
};

InventoryManager.prototype.addItem = function(itemId, quantity) {
    var itemDef = this.itemDefinitions[itemId];
    if (!itemDef) return false;

    // Check if item is stackable
    if (itemDef.stackable) {
        var existing = this.items.find(i => i.itemId === itemId);
        if (existing) {
            existing.quantity += quantity;
            this.app.fire('inventory:changed');
            return true;
        }
    }

    // Add new item
    if (this.items.length < this.maxSlots) {
        this.items.push({
            itemId: itemId,
            quantity: quantity,
            slot: this.items.length
        });
        this.app.fire('inventory:changed');
        return true;
    }

    return false; // Inventory full
};

InventoryManager.prototype.removeItem = function(itemId, quantity) {
    var item = this.items.find(i => i.itemId === itemId);
    if (!item || item.quantity < quantity) return false;

    item.quantity -= quantity;
    if (item.quantity <= 0) {
        var index = this.items.indexOf(item);
        this.items.splice(index, 1);
    }

    this.app.fire('inventory:changed');
    return true;
};

InventoryManager.prototype.hasItem = function(itemId, quantity) {
    var item = this.items.find(i => i.itemId === itemId);
    return item && item.quantity >= quantity;
};

InventoryManager.prototype.getSaveData = function() {
    return {
        items: this.items,
        currency: this.currency
    };
};

InventoryManager.prototype.loadSaveData = function(data) {
    this.items = data.items || [];
    this.currency = data.currency || { gold: 0, silver: 0, gems: 0 };
    this.app.fire('inventory:changed');
};
```

---

### Phase 5: UI Implementation (Week 7-8)
**Goal**: Create all UI screens

**Approach**: Use HTML/CSS UI for flexibility

**UI Screens**:
1. Main Menu
2. HUD (health, mana, currency)
3. Inventory
4. Quest Log
5. Shop
6. Dialogue
7. Crafting
8. Settings

**HTML UI Integration**:
```html
<!-- index.html -->
<!DOCTYPE html>
<html>
<head>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <!-- PlayCanvas Canvas -->
    <canvas id="application-canvas"></canvas>

    <!-- Game UI Overlay -->
    <div id="game-ui">
        <!-- HUD -->
        <div id="hud">
            <div id="health-bar"></div>
            <div id="mana-bar"></div>
            <div id="currency">
                <span id="gold">0</span>
                <span id="silver">0</span>
                <span id="gems">0</span>
            </div>
        </div>

        <!-- Inventory (hidden by default) -->
        <div id="inventory-panel" class="panel hidden">
            <h2>Inventory</h2>
            <div id="inventory-grid"></div>
        </div>

        <!-- Quest Log -->
        <div id="quest-panel" class="panel hidden">
            <h2>Quests</h2>
            <div id="quest-list"></div>
        </div>
    </div>

    <script src="playcanvas-stable.min.js"></script>
    <script src="__start__.js"></script>
    <script src="ui-controller.js"></script>
</body>
</html>
```

**UI Controller Script**:
```javascript
// scripts/ui/ui-controller.js
var UIController = pc.createScript('uiController');

UIController.prototype.initialize = function() {
    // Get UI elements
    this.inventoryPanel = document.getElementById('inventory-panel');
    this.questPanel = document.getElementById('quest-panel');
    this.inventoryGrid = document.getElementById('inventory-grid');

    // Listen for events
    this.app.on('inventory:changed', this.updateInventoryUI, this);
    this.app.on('quest:updated', this.updateQuestUI, this);

    // Input handlers
    this.app.keyboard.on(pc.EVENT_KEYDOWN, this.onKeyDown, this);
};

UIController.prototype.onKeyDown = function(event) {
    if (event.key === pc.KEY_I) {
        this.toggleInventory();
    } else if (event.key === pc.KEY_Q) {
        this.toggleQuests();
    }
};

UIController.prototype.toggleInventory = function() {
    this.inventoryPanel.classList.toggle('hidden');
    if (!this.inventoryPanel.classList.contains('hidden')) {
        this.updateInventoryUI();
    }
};

UIController.prototype.updateInventoryUI = function() {
    var inventoryMgr = InventoryManager.getInstance();
    this.inventoryGrid.innerHTML = '';

    inventoryMgr.items.forEach(item => {
        var itemDef = inventoryMgr.itemDefinitions[item.itemId];
        var slot = document.createElement('div');
        slot.className = 'inventory-slot';
        slot.innerHTML = `
            <img src="${itemDef.iconUrl}" alt="${itemDef.name}">
            <span class="quantity">${item.quantity}</span>
        `;
        slot.addEventListener('click', () => {
            this.onItemClicked(item);
        });
        this.inventoryGrid.appendChild(slot);
    });
};
```

---

### Phase 6: Content Integration (Week 9)
**Goal**: Convert all content to JSON

**Tasks**:
1. ✅ Convert 15 quests to JSON
2. ✅ Convert 15 riddles to JSON
3. ✅ Convert 12 plants to JSON
4. ✅ Convert 15 spells to JSON
5. ✅ Create item database JSON
6. ✅ Create NPC database JSON

**JSON Structure Examples**:

```json
// data/quests.json
[
  {
    "questID": "main_ancient_library",
    "questName": "The Ancient Library",
    "description": "Mysterious energy readings...",
    "questType": "main",
    "objectives": [
      {
        "description": "Find the Ancient Library entrance",
        "targetCount": 1,
        "currentCount": 0
      },
      {
        "description": "Collect 3 Ancient Tomes",
        "targetCount": 3,
        "currentCount": 0,
        "targetItemID": "ancient_tome"
      }
    ],
    "rewards": {
      "experience": 500,
      "gold": 300,
      "unlockLoreEntry": "library_history"
    }
  }
]
```

```json
// data/plants.json
[
  {
    "plantID": "plant_moonflower",
    "plantName": "Moonflower",
    "description": "A mystical flower that blooms only under moonlight.",
    "plantType": "magical",
    "rarity": "rare",
    "growthTime": 480,
    "growthStages": 4,
    "requiresNightTime": true,
    "requiresSeason": "spring",
    "minYield": 2,
    "maxYield": 4,
    "harvestedItemID": "moonflower",
    "sellValue": 150,
    "requirements": {
      "soilQuality": 3,
      "waterNeed": "medium",
      "sunlightNeed": "low",
      "temperatureMin": 10,
      "temperatureMax": 25
    },
    "special": {
      "canRegrow": true,
      "regrowthTime": 240,
      "magicInfused": true,
      "glowsAtNight": true
    }
  }
]
```

---

### Phase 7: Polish & Testing (Week 10-11)
**Goal**: Bug fixes, optimization, mobile support

**Tasks**:
1. ✅ Performance optimization
2. ✅ Mobile touch controls
3. ✅ Responsive UI
4. ✅ Save/load testing
5. ✅ Cross-browser testing
6. ✅ Bug fixes
7. ✅ Tutorial/onboarding

---

### Phase 8: Deployment (Week 12)
**Goal**: Publish game

**Tasks**:
1. ✅ Build optimized production version
2. ✅ Test on PlayCanvas hosting
3. ✅ Deploy to custom domain (optional)
4. ✅ Set up analytics
5. ✅ Create landing page
6. ✅ Launch!

---

## System-by-System Migration

### High Priority Systems (MVP)

#### 1. InventorySystem
**Unity**: `InventoryManager.cs` (680 lines)
**PlayCanvas**: `inventory-manager.js` (~500 lines)

**Key Differences**:
- Replace ScriptableObject items with JSON
- Use EventHandler instead of UnityEvents
- LocalStorage for save/load

**Complexity**: 🟢 Low

---

#### 2. FarmingSystem
**Unity**: `FarmingSystem.cs` (720 lines)
**PlayCanvas**: `farming-manager.js` (~600 lines)

**Key Differences**:
- Grid management with entities
- Timer system using `this.app.systems.time`
- JSON plant definitions

**Complexity**: 🟡 Medium

---

#### 3. QuestSystem
**Unity**: `QuestManager.cs` (750 lines)
**PlayCanvas**: `quest-manager.js` (~650 lines)

**Key Differences**:
- JSON quest database
- Event-driven objective tracking
- UI updates via HTML/CSS

**Complexity**: 🟡 Medium

---

#### 4. DialogueSystem
**Unity**: `DialogueSystem.cs` (680 lines)
**PlayCanvas**: `dialogue-manager.js` (~550 lines)

**Key Differences**:
- HTML-based dialogue box
- JSON dialogue trees
- Choice handling with buttons

**Complexity**: 🟢 Low

---

### Medium Priority Systems

#### 5. MagicSystem
**Unity**: `MagicSystem.cs` (850 lines)
**PlayCanvas**: `magic-manager.js` (~700 lines)

**Complexity**: 🟡 Medium

---

#### 6. CraftingSystem
**Unity**: `CraftingSystem.cs` (620 lines)
**PlayCanvas**: `crafting-manager.js` (~500 lines)

**Complexity**: 🟢 Low

---

### Low Priority Systems (Post-MVP)

- WeatherSystem
- BuildingSystem
- PetSystem
- AchievementSystem
- RelationshipSystem
- etc.

---

## MVP Definition for PlayCanvas

### What's IN the MVP

✅ **Core Gameplay**:
- Player movement (keyboard + touch)
- Farming (plant, water, harvest)
- Inventory (30 slots, stacking)
- Basic quests (5 starter quests)
- Simple dialogue
- Day/night cycle
- Save/load

✅ **Content**:
- 10 items
- 5 crops/plants
- 3 quests
- 2 NPCs
- 1 small map

✅ **UI**:
- Main menu
- HUD (health, time, currency)
- Inventory screen
- Quest log
- Dialogue box
- Settings

✅ **Mobile**:
- Touch controls
- Responsive UI
- 30 FPS target

### What's OUT of the MVP

❌ Magic system
❌ Combat
❌ Crafting
❌ Building
❌ Pets
❌ Relationships
❌ Achievements
❌ Multiplayer
❌ Complex quests
❌ Multiple biomes

---

## Implementation Priority

### Sprint 1 (Weeks 1-2): Foundation
- [ ] Project setup
- [ ] Core managers (GameManager, EventBus, SaveSystem)
- [ ] Player controller
- [ ] Camera system
- [ ] Basic UI framework

### Sprint 2 (Weeks 3-4): Core Loop
- [ ] InventorySystem
- [ ] FarmingSystem (basic)
- [ ] Item data (JSON)
- [ ] Plant data (JSON)
- [ ] Inventory UI
- [ ] Farming UI

### Sprint 3 (Weeks 5-6): Content
- [ ] QuestSystem
- [ ] DialogueSystem
- [ ] NPC interaction
- [ ] Quest data (JSON)
- [ ] Quest UI
- [ ] Dialogue UI

### Sprint 4 (Weeks 7-8): Polish
- [ ] TimeManager (day/night)
- [ ] ShopSystem
- [ ] Currency system
- [ ] Save/load
- [ ] Settings
- [ ] Tutorial

### Sprint 5 (Weeks 9-10): Testing
- [ ] Bug fixing
- [ ] Mobile optimization
- [ ] Performance tuning
- [ ] Content testing
- [ ] Balance adjustments

### Sprint 6 (Weeks 11-12): Launch
- [ ] Final polish
- [ ] Build optimization
- [ ] Deployment
- [ ] Marketing materials
- [ ] Launch!

---

## Code Examples

### Complete Manager Example

```javascript
// scripts/managers/farming-manager.js
var FarmingManager = pc.createScript('farmingManager');

FarmingManager.getInstance = function() {
    return FarmingManager._instance;
};

FarmingManager.prototype.initialize = function() {
    FarmingManager._instance = this;

    // State
    this.plantedCrops = {}; // { gridKey: PlantInstance }
    this.plantDefinitions = {};

    // Load plant data
    this.loadPlantData();

    // Events
    this.app.on('time:tick', this.updateCrops, this);
};

FarmingManager.prototype.loadPlantData = function() {
    this.app.assets.loadFromUrl('data/plants.json', 'json', (err, asset) => {
        if (!err) {
            var plants = asset.resource;
            plants.forEach(plant => {
                this.plantDefinitions[plant.plantID] = plant;
            });
            console.log('Plants loaded:', Object.keys(this.plantDefinitions).length);
        }
    });
};

FarmingManager.prototype.plantSeed = function(plantId, gridX, gridY) {
    var plantDef = this.plantDefinitions[plantId];
    if (!plantDef) {
        console.error('Plant not found:', plantId);
        return false;
    }

    var gridKey = `${gridX},${gridY}`;

    // Check if already planted
    if (this.plantedCrops[gridKey]) {
        console.warn('Tile already occupied');
        return false;
    }

    // Check if player has seed
    var inventoryMgr = InventoryManager.getInstance();
    if (!inventoryMgr.hasItem(plantId + '_seed', 1)) {
        console.warn('No seeds');
        return false;
    }

    // Remove seed from inventory
    inventoryMgr.removeItem(plantId + '_seed', 1);

    // Create plant instance
    var plant = {
        plantId: plantId,
        plantDef: plantDef,
        gridX: gridX,
        gridY: gridY,
        plantedTime: Date.now(),
        currentStage: 0,
        watered: false,
        ready: false
    };

    this.plantedCrops[gridKey] = plant;

    // Create visual entity
    this.createPlantEntity(plant);

    // Fire event
    this.app.fire('farming:planted', plant);

    return true;
};

FarmingManager.prototype.createPlantEntity = function(plant) {
    // Create entity at grid position
    var entity = new pc.Entity('plant_' + plant.gridX + '_' + plant.gridY);

    // Add model component (or sprite)
    entity.addComponent('model', {
        type: 'box' // Placeholder, use actual model
    });

    // Position at grid location
    entity.setPosition(plant.gridX * 2, 0, plant.gridY * 2);

    // Add to scene
    this.app.root.addChild(entity);

    // Store reference
    plant.entity = entity;
};

FarmingManager.prototype.waterPlant = function(gridX, gridY) {
    var gridKey = `${gridX},${gridY}`;
    var plant = this.plantedCrops[gridKey];

    if (!plant) return false;
    if (plant.watered) return false;

    plant.watered = true;

    // Visual feedback (add particle effect, change color, etc.)
    this.app.fire('farming:watered', plant);

    return true;
};

FarmingManager.prototype.updateCrops = function(deltaTime) {
    // Update all planted crops
    Object.values(this.plantedCrops).forEach(plant => {
        if (plant.ready) return;

        var timeSincePlanted = (Date.now() - plant.plantedTime) / 1000; // seconds
        var growthProgress = timeSincePlanted / plant.plantDef.growthTime;

        // Calculate stage
        var newStage = Math.floor(growthProgress * plant.plantDef.growthStages);
        newStage = Math.min(newStage, plant.plantDef.growthStages - 1);

        if (newStage !== plant.currentStage) {
            plant.currentStage = newStage;
            this.updatePlantVisual(plant);
        }

        // Check if ready to harvest
        if (growthProgress >= 1.0) {
            plant.ready = true;
            this.app.fire('farming:ready', plant);
        }
    });
};

FarmingManager.prototype.updatePlantVisual = function(plant) {
    // Update model/sprite based on growth stage
    if (plant.entity) {
        var scale = 0.5 + (plant.currentStage / plant.plantDef.growthStages) * 0.5;
        plant.entity.setLocalScale(scale, scale, scale);
    }
};

FarmingManager.prototype.harvestPlant = function(gridX, gridY) {
    var gridKey = `${gridX},${gridY}`;
    var plant = this.plantedCrops[gridKey];

    if (!plant || !plant.ready) return false;

    // Calculate yield
    var yield = Math.floor(
        Math.random() * (plant.plantDef.maxYield - plant.plantDef.minYield + 1)
    ) + plant.plantDef.minYield;

    // Add items to inventory
    var inventoryMgr = InventoryManager.getInstance();
    inventoryMgr.addItem(plant.plantDef.harvestedItemID, yield);

    // Remove plant entity
    if (plant.entity) {
        plant.entity.destroy();
    }

    // Remove from planted crops
    delete this.plantedCrops[gridKey];

    // Fire event
    this.app.fire('farming:harvested', {
        plantId: plant.plantId,
        yield: yield
    });

    return true;
};

FarmingManager.prototype.getSaveData = function() {
    // Convert planted crops to serializable format
    var crops = [];
    Object.values(this.plantedCrops).forEach(plant => {
        crops.push({
            plantId: plant.plantId,
            gridX: plant.gridX,
            gridY: plant.gridY,
            plantedTime: plant.plantedTime,
            currentStage: plant.currentStage,
            watered: plant.watered,
            ready: plant.ready
        });
    });

    return { plantedCrops: crops };
};

FarmingManager.prototype.loadSaveData = function(data) {
    // Clear existing
    Object.values(this.plantedCrops).forEach(plant => {
        if (plant.entity) plant.entity.destroy();
    });
    this.plantedCrops = {};

    // Restore crops
    if (data.plantedCrops) {
        data.plantedCrops.forEach(cropData => {
            var gridKey = `${cropData.gridX},${cropData.gridY}`;
            var plant = {
                plantId: cropData.plantId,
                plantDef: this.plantDefinitions[cropData.plantId],
                gridX: cropData.gridX,
                gridY: cropData.gridY,
                plantedTime: cropData.plantedTime,
                currentStage: cropData.currentStage,
                watered: cropData.watered,
                ready: cropData.ready
            };

            this.plantedCrops[gridKey] = plant;
            this.createPlantEntity(plant);
            this.updatePlantVisual(plant);
        });
    }
};
```

---

## Key Recommendations

### 1. Use TypeScript
While PlayCanvas supports JavaScript, **TypeScript** will save you time:
- Better IDE support (autocomplete, type checking)
- Easier refactoring
- Catches errors at compile time
- Still compiles to JavaScript

### 2. HTML UI vs PlayCanvas UI
**Recommendation: HTML/CSS UI**

Pros:
- More flexible styling
- Familiar web technologies
- Easier to make responsive
- Better for complex layouts

Cons:
- Not integrated with 3D scene
- Requires manual event wiring

### 3. Data Format
**Use JSON for all data** (not ScriptableObjects)

Structure:
```
data/
├── items.json
├── quests.json
├── plants.json
├── spells.json
├── npcs.json
└── dialogue.json
```

### 4. Performance Optimization
PlayCanvas is WebGL-based, optimize for web:
- Keep draw calls low (<100)
- Use texture atlases
- Implement LOD system
- Use object pooling
- Lazy load assets

### 5. Mobile-First Design
Target mobile from day one:
- Touch controls
- UI at least 44px touch targets
- 30 FPS target
- Smaller textures
- Simple shaders

---

## Estimated Timeline

| Phase | Duration | Deliverable |
|-------|----------|-------------|
| Setup | 1 week | Empty project with structure |
| Core Framework | 1 week | Managers, events, save system |
| Player & World | 1 week | Playable character, basic world |
| Core Systems | 3 weeks | Inventory, farming, quests |
| UI Implementation | 2 weeks | All UI screens |
| Content | 1 week | JSON data files |
| Polish & Testing | 2 weeks | Bug fixes, optimization |
| Deployment | 1 week | Published game |
| **TOTAL** | **12 weeks** | **Working MVP** |

---

## Next Steps

### Immediate Actions (This Week)
1. ✅ Create PlayCanvas account
2. ✅ Create new project
3. ✅ Set up GitHub integration
4. ✅ Create folder structure
5. ✅ Write first script (GameManager)
6. ✅ Test basic functionality

### Week 2
1. ✅ Implement EventBus
2. ✅ Implement SaveSystem
3. ✅ Create ResourceLoader
4. ✅ Load first JSON file

### Week 3
1. ✅ Create player controller
2. ✅ Implement camera follow
3. ✅ Create basic world
4. ✅ Test movement

---

## Resources

### PlayCanvas Documentation
- **User Manual**: https://developer.playcanvas.com/user-manual/
- **API Reference**: https://developer.playcanvas.com/api/
- **Tutorials**: https://developer.playcanvas.com/tutorials/

### Example Projects
- **Tutorials on PlayCanvas**: https://playcanvas.com/explore/featured
- **Open Source Games**: https://github.com/playcanvas

### Community
- **Forum**: https://forum.playcanvas.com/
- **Discord**: https://discord.gg/playcanvas

---

## Conclusion

Converting from Unity to PlayCanvas is a significant undertaking, but **very achievable** in 12 weeks for an MVP. The main advantages:

✅ **Better web performance**
✅ **Easier deployment** (no build process)
✅ **Instant loading** (streaming assets)
✅ **Mobile-friendly** out of the box
✅ **Free hosting** on PlayCanvas servers

The main challenges:

❌ **Complete code rewrite** (C# → JavaScript/TypeScript)
❌ **Learning new engine** (different paradigms)
❌ **No ScriptableObjects** (JSON instead)
❌ **Different debugging** (browser tools)

**Recommendation**: Start with the MVP (12 weeks), then iterate with additional features. Focus on getting the core farming loop working first, then expand from there.

Would you like me to:
1. Generate the first few PlayCanvas scripts to get you started?
2. Create the JSON data files for items, quests, and plants?
3. Write a detailed tutorial for a specific system?
4. Create project setup instructions?

Let me know how you'd like to proceed!
