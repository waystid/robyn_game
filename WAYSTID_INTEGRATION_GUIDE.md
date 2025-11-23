# Waystid Game Forge - PlayCanvas Template & Agent Integration Guide

**Version**: 1.0.0
**Date**: November 22, 2024
**Target Branch**: waystid-game-forge (main)
**Purpose**: Import PlayCanvas Cozy Game Template + Playground Porter Agent

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [What's Being Imported](#whats-being-imported)
3. [Repository Structure](#repository-structure)
4. [Step-by-Step Integration](#step-by-step-integration)
5. [Template Files](#template-files)
6. [Agent Files](#agent-files)
7. [Deployment Commands](#deployment-commands)
8. [Testing & Validation](#testing--validation)
9. [Documentation](#documentation)

---

## 🎯 Overview

This guide provides complete instructions to integrate:

1. **Cozy Game Template** - Production-ready PlayCanvas game template
2. **Playground Porter Agent** - AI agent for converting Unity/Unreal assets to PlayCanvas

Both will be added to the `waystid-game-forge` repository for community use.

---

## 📦 What's Being Imported

### Package 1: Cozy Game Template

**Source**: `CozyGameTemplate-PlayCanvas-v1.0.0.zip`
**Size**: 41 KB
**Files**: 20 files
**Type**: PlayCanvas game template

**Contents**:
- ✅ 4 JavaScript game systems (~2,300 lines)
- ✅ Complete HTML/CSS UI (~1,150 lines)
- ✅ Sample game data (5 JSON files)
- ✅ Comprehensive documentation (3 files)

### Package 2: Playground Porter Agent

**Source**: `agents/PlaygroundPorter/`
**Files**: 5 markdown files
**Lines**: 2,500+ lines
**Type**: AI agent specification

**Contents**:
- ✅ Complete agent specification
- ✅ System prompt for deployment
- ✅ Example conversions
- ✅ Documentation and guides

---

## 🗂️ Repository Structure

After integration, your `waystid-game-forge` repository will have:

```
waystid-game-forge/
├── templates/
│   └── playcanvas/
│       └── cozy-game-template/
│           ├── scripts/
│           │   ├── managers/
│           │   │   ├── core-managers.js
│           │   │   └── game-systems.js
│           │   └── player/
│           │       └── player-world-systems.js
│           ├── ui/
│           │   ├── index.html
│           │   ├── styles.css
│           │   └── ui-controller.js
│           ├── data/
│           │   ├── items.json
│           │   ├── quests.json
│           │   ├── plants.json
│           │   ├── spells.json
│           │   └── npcs.json
│           ├── docs/
│           │   ├── README.md
│           │   ├── PACKAGE_INFO.md
│           │   └── UI_VALIDATION.md
│           ├── template-info.json
│           └── CHANGELOG.md
├── agents/
│   └── playground-porter/
│       ├── AGENT_SPEC.md
│       ├── SYSTEM_PROMPT.md
│       ├── EXAMPLE_CONVERSION.md
│       ├── README.md
│       ├── QUICK_REFERENCE.md
│       └── agent-config.json
├── docs/
│   └── playcanvas-integration.md
└── README.md (update with new content)
```

---

## 🚀 Step-by-Step Integration

### Phase 1: Prepare Repository

```bash
# Clone waystid-game-forge repository
git clone https://github.com/waystid/waystid-game-forge.git
cd waystid-game-forge

# Create feature branch
git checkout -b feature/add-playcanvas-template

# Create directory structure
mkdir -p templates/playcanvas/cozy-game-template/{scripts/managers,scripts/player,ui,data,docs}
mkdir -p agents/playground-porter
mkdir -p docs
```

### Phase 2: Import Template Files

#### Step 2.1: Create Core Managers Script

Create `templates/playcanvas/cozy-game-template/scripts/managers/core-managers.js`:

```javascript
// ============================================================================
// Core Managers - EventBus, SaveSystem, GameManager
// Part of Waystid Game Forge - Cozy Game Template
// ============================================================================

// ============================================================================
// EVENT BUS - Global event system for decoupled communication
// ============================================================================

var EventBus = pc.createScript('eventBus');

// Singleton accessor
EventBus.getInstance = function() {
    if (!EventBus._instance) {
        var root = pc.app.root.findByName('EventBus');
        if (root) {
            EventBus._instance = root.script.eventBus;
        }
    }
    return EventBus._instance;
};

EventBus.prototype.initialize = function() {
    EventBus._instance = this;
    this.events = new pc.EventHandler();
    console.log('[EventBus] Initialized');
};

/**
 * Subscribe to an event
 * @param {string} eventName - Event name
 * @param {function} callback - Callback function
 * @param {object} scope - Callback scope
 */
EventBus.prototype.on = function(eventName, callback, scope) {
    this.events.on(eventName, callback, scope);
};

/**
 * Unsubscribe from an event
 * @param {string} eventName - Event name
 * @param {function} callback - Callback function
 * @param {object} scope - Callback scope
 */
EventBus.prototype.off = function(eventName, callback, scope) {
    this.events.off(eventName, callback, scope);
};

/**
 * Fire an event
 * @param {string} eventName - Event name
 * @param {...*} args - Event arguments
 */
EventBus.prototype.fire = function(eventName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
    this.events.fire(eventName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
};

// ============================================================================
// SAVE SYSTEM - LocalStorage-based persistence
// ============================================================================

var SaveSystem = pc.createScript('saveSystem');

SaveSystem.getInstance = function() {
    return SaveSystem._instance;
};

SaveSystem.attributes.add('autoSaveInterval', {
    type: 'number',
    default: 60,
    description: 'Auto-save interval in seconds (0 = disabled)'
});

SaveSystem.prototype.initialize = function() {
    SaveSystem._instance = this;
    this.autoSaveTimer = 0;
    console.log('[SaveSystem] Initialized');
};

SaveSystem.prototype.update = function(dt) {
    if (this.autoSaveInterval > 0) {
        this.autoSaveTimer += dt;
        if (this.autoSaveTimer >= this.autoSaveInterval) {
            this.autoSaveTimer = 0;
            this.saveGame();
        }
    }
};

/**
 * Save game data
 * @param {string} slotName - Save slot name (default: 'save_1')
 */
SaveSystem.prototype.saveGame = function(slotName) {
    slotName = slotName || 'save_1';

    var saveData = {
        timestamp: Date.now(),
        version: '1.0.0',
        managers: {}
    };

    // Collect save data from all managers
    var managers = [
        'inventoryManager',
        'farmingManager',
        'questManager',
        'dialogueManager',
        'magicManager',
        'playerController',
        'timeManager',
        'weatherSystem'
    ];

    managers.forEach(function(managerName) {
        var manager = this.app.root.findByName('GameManager');
        if (manager && manager.script && manager.script[managerName]) {
            var script = manager.script[managerName];
            if (script.getSaveData) {
                saveData.managers[managerName] = script.getSaveData();
            }
        }
    }.bind(this));

    try {
        localStorage.setItem(slotName, JSON.stringify(saveData));
        console.log('[SaveSystem] Game saved to slot:', slotName);

        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('game:saved', slotName);
        }
        return true;
    } catch (e) {
        console.error('[SaveSystem] Save failed:', e);
        return false;
    }
};

/**
 * Load game data
 * @param {string} slotName - Save slot name (default: 'save_1')
 */
SaveSystem.prototype.loadGame = function(slotName) {
    slotName = slotName || 'save_1';

    try {
        var saveDataStr = localStorage.getItem(slotName);
        if (!saveDataStr) {
            console.warn('[SaveSystem] No save data found in slot:', slotName);
            return false;
        }

        var saveData = JSON.parse(saveDataStr);
        console.log('[SaveSystem] Loading save from:', new Date(saveData.timestamp));

        // Load data into managers
        for (var managerName in saveData.managers) {
            var manager = this.app.root.findByName('GameManager');
            if (manager && manager.script && manager.script[managerName]) {
                var script = manager.script[managerName];
                if (script.loadSaveData) {
                    script.loadSaveData(saveData.managers[managerName]);
                }
            }
        }

        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('game:loaded', slotName);
        }

        console.log('[SaveSystem] Game loaded successfully');
        return true;
    } catch (e) {
        console.error('[SaveSystem] Load failed:', e);
        return false;
    }
};

/**
 * Check if save exists
 * @param {string} slotName - Save slot name
 * @returns {boolean} True if save exists
 */
SaveSystem.prototype.hasSave = function(slotName) {
    slotName = slotName || 'save_1';
    return localStorage.getItem(slotName) !== null;
};

/**
 * Delete save
 * @param {string} slotName - Save slot name
 */
SaveSystem.prototype.deleteSave = function(slotName) {
    slotName = slotName || 'save_1';
    localStorage.removeItem(slotName);
    console.log('[SaveSystem] Deleted save:', slotName);
};

// ============================================================================
// GAME MANAGER - Central game state coordinator
// ============================================================================

var GameManager = pc.createScript('gameManager');

GameManager.getInstance = function() {
    return GameManager._instance;
};

GameManager.attributes.add('dataPath', {
    type: 'string',
    default: 'data/',
    description: 'Path to game data JSON files'
});

GameManager.prototype.initialize = function() {
    GameManager._instance = this;
    this.gameState = 'loading';
    this.dataLoaded = false;

    console.log('[GameManager] Initialized');
    this.loadGameData();
};

/**
 * Load all game data from JSON files
 */
GameManager.prototype.loadGameData = function() {
    var dataFiles = [
        'items.json',
        'quests.json',
        'plants.json',
        'spells.json',
        'npcs.json'
    ];

    var loadedCount = 0;
    var totalFiles = dataFiles.length;

    dataFiles.forEach(function(filename) {
        var url = this.dataPath + filename;

        this.app.assets.loadFromUrl(url, 'json', function(err, asset) {
            if (err) {
                console.error('[GameManager] Failed to load:', filename, err);
            } else {
                console.log('[GameManager] Loaded:', filename);
                this['_' + filename.replace('.json', 'Data')] = asset.resource;
            }

            loadedCount++;
            if (loadedCount === totalFiles) {
                this.onDataLoaded();
            }
        }.bind(this));
    }.bind(this));
};

/**
 * Called when all data is loaded
 */
GameManager.prototype.onDataLoaded = function() {
    this.dataLoaded = true;
    this.gameState = 'ready';

    console.log('[GameManager] All data loaded successfully');

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('data:loaded');
    }
};

/**
 * Get loaded data
 * @param {string} dataType - Data type (items, quests, plants, spells, npcs)
 * @returns {Array} Data array
 */
GameManager.prototype.getData = function(dataType) {
    var key = '_' + dataType + 'Data';
    return this[key] || [];
};

/**
 * Change game state
 * @param {string} newState - New state (loading, ready, playing, paused, gameover)
 */
GameManager.prototype.setGameState = function(newState) {
    var oldState = this.gameState;
    this.gameState = newState;

    console.log('[GameManager] State changed:', oldState, '->', newState);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('gameState:changed', newState, oldState);
    }
};

/**
 * Get current game state
 * @returns {string} Current state
 */
GameManager.prototype.getGameState = function() {
    return this.gameState;
};
```

**File saved to**: `templates/playcanvas/cozy-game-template/scripts/managers/core-managers.js`

---

#### Step 2.2: Create Game Systems Script

Due to length, create `templates/playcanvas/cozy-game-template/scripts/managers/game-systems.js` with the full content from the template package. This file contains:
- InventoryManager (30-slot inventory, currency, stacking)
- FarmingManager (plant, water, harvest)
- QuestManager (objectives, rewards)
- DialogueManager (NPC conversations)
- MagicManager (spells, cooldowns)

**Note**: Copy from `PlayCanvas/scripts/managers/game-systems.js` in source repository.

---

#### Step 2.3: Create Player & World Systems

Create `templates/playcanvas/cozy-game-template/scripts/player/player-world-systems.js` with:
- PlayerController (WASD movement, stats)
- TimeManager (day/night cycle)
- WeatherSystem (dynamic weather)

**Note**: Copy from `PlayCanvas/scripts/player/player-world-systems.js` in source repository.

---

#### Step 2.4: Create UI Files

Create `templates/playcanvas/cozy-game-template/ui/index.html`:

**Note**: Copy from `PlayCanvas/ui/index.html` in source repository.

Create `templates/playcanvas/cozy-game-template/ui/styles.css`:

**Note**: Copy from `PlayCanvas/ui/styles.css` in source repository.

Create `templates/playcanvas/cozy-game-template/ui/ui-controller.js`:

**Note**: Copy from `PlayCanvas/ui/ui-controller.js` in source repository.

---

#### Step 2.5: Create Data Files

Create all 5 JSON files in `templates/playcanvas/cozy-game-template/data/`:

1. **items.json**
2. **quests.json**
3. **plants.json**
4. **spells.json**
5. **npcs.json**

**Note**: Copy from `PlayCanvas/data/*.json` in source repository.

---

#### Step 2.6: Create Template Metadata

Create `templates/playcanvas/cozy-game-template/template-info.json`:

```json
{
  "templateName": "Cozy Game Template",
  "version": "1.0.0",
  "forge": "waystid-game-forge",
  "category": "game-templates",
  "engine": "playcanvas",
  "tags": ["cozy", "farming", "rpg", "adventure", "simulation"],
  "description": "Complete cozy game template with farming, quests, dialogue, magic, and more",
  "author": "Waystid Game Forge",
  "license": "MIT",
  "created": "2024-11-22",
  "updated": "2024-11-22",
  "playcanvasVersion": "1.50+",
  "features": [
    "30-slot inventory system",
    "Farming system (plant, water, harvest)",
    "Quest system with objectives",
    "Dialogue system for NPCs",
    "Magic system with 6 spells",
    "Day/night cycle",
    "Dynamic weather",
    "Save/load system",
    "Complete HTML/CSS UI",
    "Mobile touch controls",
    "Event-driven architecture"
  ],
  "systems": {
    "core": [
      "EventBus",
      "SaveSystem",
      "GameManager"
    ],
    "game": [
      "InventoryManager",
      "FarmingManager",
      "QuestManager",
      "DialogueManager",
      "MagicManager"
    ],
    "world": [
      "PlayerController",
      "TimeManager",
      "WeatherSystem"
    ],
    "ui": [
      "UIController"
    ]
  },
  "files": {
    "scripts": 4,
    "data": 5,
    "ui": 3,
    "docs": 3
  },
  "codeStats": {
    "totalLines": 5000,
    "javascript": 2300,
    "html": 350,
    "css": 800,
    "json": 200,
    "documentation": 1600
  },
  "compatibility": {
    "mobile": true,
    "desktop": true,
    "tablet": true,
    "webgl2": true
  },
  "installation": {
    "difficulty": "easy",
    "estimatedTime": "5 minutes",
    "requiresSetup": true
  },
  "support": {
    "documentation": "docs/README.md",
    "examples": "docs/EXAMPLES.md",
    "api": "docs/API.md"
  },
  "links": {
    "repository": "https://github.com/waystid/waystid-game-forge",
    "issues": "https://github.com/waystid/waystid-game-forge/issues",
    "discussions": "https://github.com/waystid/waystid-game-forge/discussions"
  }
}
```

---

### Phase 3: Import Agent Files

#### Step 3.1: Create Agent Configuration

Create `agents/playground-porter/agent-config.json`:

```json
{
  "agentName": "Playground Porter",
  "version": "1.0.0",
  "type": "asset-converter",
  "forge": "waystid-game-forge",
  "description": "AI agent specialized in converting Unity/Unreal assets to PlayCanvas playgrounds",
  "capabilities": [
    "unity-to-playcanvas",
    "unreal-to-playcanvas",
    "code-translation",
    "documentation-generation",
    "playground-packaging"
  ],
  "supportedSources": {
    "unity": {
      "languages": ["csharp"],
      "components": ["MonoBehaviour", "ScriptableObject", "Prefab", "Coroutine"],
      "apis": ["Unity UI", "UnityEvents", "Input System"]
    },
    "unreal": {
      "languages": ["cpp", "blueprints"],
      "components": ["Actor", "ActorComponent", "DataAsset"],
      "apis": ["UMG", "Gameplay Framework"]
    }
  },
  "outputFormat": {
    "type": "playground",
    "structure": {
      "scripts": "JavaScript (PlayCanvas format)",
      "data": "JSON files",
      "ui": "HTML/CSS",
      "docs": "Markdown"
    }
  },
  "deployment": {
    "method": "system-prompt",
    "promptFile": "SYSTEM_PROMPT.md",
    "model": "claude-sonnet-4-5",
    "contextWindow": 200000
  },
  "performance": {
    "simpleConversion": "15-30 min",
    "mediumConversion": "1-2 hours",
    "complexConversion": "2-4 hours",
    "largeTemplate": "2-5 days"
  },
  "quality": {
    "syntaxValidation": true,
    "apiCorrectness": true,
    "documentationComplete": true,
    "examplesIncluded": true,
    "mobileCompatible": true
  },
  "files": {
    "specification": "AGENT_SPEC.md",
    "systemPrompt": "SYSTEM_PROMPT.md",
    "examples": "EXAMPLE_CONVERSION.md",
    "readme": "README.md",
    "quickReference": "QUICK_REFERENCE.md"
  },
  "usage": {
    "command": "Convert [AssetName] from Unity/Unreal to PlayCanvas playground",
    "inputFormats": ["unity package", "unreal asset", "local files", "asset description"],
    "outputFormat": "Playground package (zip)"
  },
  "links": {
    "repository": "https://github.com/waystid/waystid-game-forge",
    "documentation": "agents/playground-porter/README.md",
    "examples": "agents/playground-porter/EXAMPLE_CONVERSION.md"
  }
}
```

---

#### Step 3.2: Copy Agent Files

Copy these files from source repository to `agents/playground-porter/`:

1. **AGENT_SPEC.md** - Complete specification
2. **SYSTEM_PROMPT.md** - Deployment prompt
3. **EXAMPLE_CONVERSION.md** - Working example
4. **README.md** - Main documentation
5. **QUICK_REFERENCE.md** - Quick guide

**Note**: All files available in source at `agents/PlaygroundPorter/`

---

### Phase 4: Create Integration Documentation

Create `docs/playcanvas-integration.md`:

```markdown
# PlayCanvas Integration Guide - Waystid Game Forge

## Overview

This guide explains how to use PlayCanvas templates and the Playground Porter agent in Waystid Game Forge.

## Available Resources

### 1. Cozy Game Template

**Location**: `templates/playcanvas/cozy-game-template/`

**What it includes**:
- Complete game systems (inventory, farming, quests, dialogue, magic)
- Player controller and world systems (time, weather)
- Full HTML/CSS UI
- Sample game data (items, quests, plants, spells, NPCs)
- Comprehensive documentation

**Quick Start**:
1. Navigate to `templates/playcanvas/cozy-game-template/`
2. Read `docs/README.md` for setup instructions
3. Upload scripts to your PlayCanvas project
4. Follow the 5-minute quick start guide

### 2. Playground Porter Agent

**Location**: `agents/playground-porter/`

**What it does**:
- Converts Unity assets to PlayCanvas playgrounds
- Converts Unreal assets to PlayCanvas playgrounds
- Generates complete documentation
- Validates all conversions

**How to use**:
1. Read `agents/playground-porter/README.md`
2. Load `agents/playground-porter/SYSTEM_PROMPT.md` into Claude
3. Provide Unity/Unreal asset to convert
4. Receive production-ready playground package

## Integration Workflow

### For Game Developers

1. **Use Template**:
   - Start with cozy-game-template
   - Customize for your game
   - Add custom features

2. **Convert Assets**:
   - Use Playground Porter to convert Unity/Unreal assets
   - Integrate converted playgrounds
   - Build complete game

### For Template Creators

1. **Create Template**:
   - Build in Unity/Unreal
   - Use Playground Porter to convert
   - Submit to Waystid Game Forge

2. **Share with Community**:
   - Package as playground
   - Add documentation
   - Publish to forge

## Support

- **Documentation**: See template and agent docs
- **Issues**: GitHub Issues
- **Community**: GitHub Discussions
```

---

### Phase 5: Update Main README

Update `README.md` to include:

```markdown
## 🎮 PlayCanvas Templates

### Cozy Game Template

A complete, production-ready game template for PlayCanvas featuring:
- Inventory, farming, quest, dialogue, and magic systems
- Day/night cycle and dynamic weather
- Complete HTML/CSS UI
- Mobile support

**Location**: `templates/playcanvas/cozy-game-template/`
**Documentation**: See template README

## 🤖 AI Agents

### Playground Porter

AI agent that converts Unity/Unreal assets to PlayCanvas playgrounds.

**Capabilities**:
- Unity C# → PlayCanvas JavaScript
- Unreal C++/Blueprints → PlayCanvas JavaScript
- Complete documentation generation
- Playground packaging

**Location**: `agents/playground-porter/`
**Documentation**: See agent README

## 📚 Getting Started

### Use PlayCanvas Template

```bash
cd templates/playcanvas/cozy-game-template
# Follow docs/README.md for setup
```

### Use Playground Porter Agent

```bash
cd agents/playground-porter
# Load SYSTEM_PROMPT.md into Claude
# Start converting assets!
```
```

---

## 🔧 Deployment Commands

### Complete Integration Script

Create `scripts/import-playcanvas.sh`:

```bash
#!/bin/bash

# Waystid Game Forge - PlayCanvas Template & Agent Import Script
# This script automates the import process

set -e

echo "========================================="
echo "Waystid Game Forge - PlayCanvas Import"
echo "========================================="
echo ""

# Check if we're in the right directory
if [ ! -d ".git" ]; then
    echo "Error: Must run from waystid-game-forge root directory"
    exit 1
fi

# Create feature branch
echo "Creating feature branch..."
git checkout -b feature/add-playcanvas-template

# Create directory structure
echo "Creating directory structure..."
mkdir -p templates/playcanvas/cozy-game-template/{scripts/managers,scripts/player,ui,data,docs}
mkdir -p agents/playground-porter
mkdir -p docs

echo "✅ Directory structure created"

# Copy template files
echo "Copying template files..."
# Note: Assumes source files are in ../robyn_game/PlayCanvas/
SOURCE_TEMPLATE="../robyn_game/PlayCanvas"

if [ -d "$SOURCE_TEMPLATE" ]; then
    cp -r $SOURCE_TEMPLATE/scripts/* templates/playcanvas/cozy-game-template/scripts/
    cp -r $SOURCE_TEMPLATE/ui/* templates/playcanvas/cozy-game-template/ui/
    cp -r $SOURCE_TEMPLATE/data/* templates/playcanvas/cozy-game-template/data/
    cp $SOURCE_TEMPLATE/README.md templates/playcanvas/cozy-game-template/docs/
    cp $SOURCE_TEMPLATE/PACKAGE_INFO.md templates/playcanvas/cozy-game-template/docs/
    cp $SOURCE_TEMPLATE/UI_VALIDATION.md templates/playcanvas/cozy-game-template/docs/
    echo "✅ Template files copied"
else
    echo "⚠️  Source template directory not found: $SOURCE_TEMPLATE"
    echo "   Please copy files manually"
fi

# Copy agent files
echo "Copying agent files..."
SOURCE_AGENT="../robyn_game/agents/PlaygroundPorter"

if [ -d "$SOURCE_AGENT" ]; then
    cp $SOURCE_AGENT/* agents/playground-porter/
    echo "✅ Agent files copied"
else
    echo "⚠️  Source agent directory not found: $SOURCE_AGENT"
    echo "   Please copy files manually"
fi

# Create metadata files
echo "Creating metadata files..."

# template-info.json
cat > templates/playcanvas/cozy-game-template/template-info.json << 'EOF'
{
  "templateName": "Cozy Game Template",
  "version": "1.0.0",
  "forge": "waystid-game-forge",
  "category": "game-templates",
  "engine": "playcanvas",
  "license": "MIT"
}
EOF

# agent-config.json
cat > agents/playground-porter/agent-config.json << 'EOF'
{
  "agentName": "Playground Porter",
  "version": "1.0.0",
  "type": "asset-converter",
  "forge": "waystid-game-forge"
}
EOF

echo "✅ Metadata files created"

# Create integration docs
echo "Creating integration documentation..."
cat > docs/playcanvas-integration.md << 'EOF'
# PlayCanvas Integration Guide

See templates/playcanvas/cozy-game-template/ for template
See agents/playground-porter/ for agent
EOF

echo "✅ Documentation created"

# Git operations
echo "Staging files..."
git add templates/
git add agents/
git add docs/

echo ""
echo "========================================="
echo "Import Complete!"
echo "========================================="
echo ""
echo "Next steps:"
echo "1. Review changes: git status"
echo "2. Commit: git commit -m 'Add PlayCanvas template and Playground Porter agent'"
echo "3. Push: git push origin feature/add-playcanvas-template"
echo "4. Create pull request on GitHub"
echo ""
echo "Template location: templates/playcanvas/cozy-game-template/"
echo "Agent location: agents/playground-porter/"
echo ""
```

Make executable:
```bash
chmod +x scripts/import-playcanvas.sh
```

---

### Manual Import Commands

If not using script:

```bash
# 1. Create directories
mkdir -p templates/playcanvas/cozy-game-template/{scripts/managers,scripts/player,ui,data,docs}
mkdir -p agents/playground-porter

# 2. Copy template files (adjust paths as needed)
cp -r source/PlayCanvas/scripts/* templates/playcanvas/cozy-game-template/scripts/
cp -r source/PlayCanvas/ui/* templates/playcanvas/cozy-game-template/ui/
cp -r source/PlayCanvas/data/* templates/playcanvas/cozy-game-template/data/
cp source/PlayCanvas/*.md templates/playcanvas/cozy-game-template/docs/

# 3. Copy agent files
cp source/agents/PlaygroundPorter/* agents/playground-porter/

# 4. Create metadata
# (Create template-info.json and agent-config.json as shown above)

# 5. Git operations
git add templates/ agents/ docs/
git commit -m "Add PlayCanvas template and Playground Porter agent

- Add Cozy Game Template with complete game systems
- Add Playground Porter agent for asset conversion
- Add integration documentation"

git push origin feature/add-playcanvas-template
```

---

## ✅ Testing & Validation

### Validate Template Import

```bash
# Check file structure
tree templates/playcanvas/cozy-game-template/

# Should show:
# templates/playcanvas/cozy-game-template/
# ├── scripts/
# │   ├── managers/
# │   │   ├── core-managers.js
# │   │   └── game-systems.js
# │   └── player/
# │       └── player-world-systems.js
# ├── ui/
# │   ├── index.html
# │   ├── styles.css
# │   └── ui-controller.js
# ├── data/
# │   ├── items.json
# │   ├── quests.json
# │   ├── plants.json
# │   ├── spells.json
# │   └── npcs.json
# ├── docs/
# │   ├── README.md
# │   ├── PACKAGE_INFO.md
# │   └── UI_VALIDATION.md
# └── template-info.json

# Verify JavaScript syntax
node -c templates/playcanvas/cozy-game-template/scripts/managers/core-managers.js
node -c templates/playcanvas/cozy-game-template/scripts/managers/game-systems.js
node -c templates/playcanvas/cozy-game-template/scripts/player/player-world-systems.js
node -c templates/playcanvas/cozy-game-template/ui/ui-controller.js

# Verify JSON files
for file in templates/playcanvas/cozy-game-template/data/*.json; do
    echo "Validating $file"
    python3 -m json.tool "$file" > /dev/null || echo "❌ Invalid JSON: $file"
done
```

### Validate Agent Import

```bash
# Check agent files
ls -la agents/playground-porter/

# Should include:
# AGENT_SPEC.md
# SYSTEM_PROMPT.md
# EXAMPLE_CONVERSION.md
# README.md
# QUICK_REFERENCE.md
# agent-config.json

# Verify markdown files
for file in agents/playground-porter/*.md; do
    echo "Checking $file"
    wc -l "$file"
done

# Verify agent config
cat agents/playground-porter/agent-config.json | python3 -m json.tool
```

---

## 📖 Documentation Checklist

Ensure these docs are accessible:

### Template Documentation
- [ ] `templates/playcanvas/cozy-game-template/docs/README.md` - Main guide
- [ ] `templates/playcanvas/cozy-game-template/docs/PACKAGE_INFO.md` - Quick start
- [ ] `templates/playcanvas/cozy-game-template/docs/UI_VALIDATION.md` - Integration guide
- [ ] `templates/playcanvas/cozy-game-template/template-info.json` - Metadata

### Agent Documentation
- [ ] `agents/playground-porter/README.md` - Agent overview
- [ ] `agents/playground-porter/SYSTEM_PROMPT.md` - Deployment prompt
- [ ] `agents/playground-porter/AGENT_SPEC.md` - Complete specification
- [ ] `agents/playground-porter/EXAMPLE_CONVERSION.md` - Working example
- [ ] `agents/playground-porter/QUICK_REFERENCE.md` - Quick guide
- [ ] `agents/playground-porter/agent-config.json` - Configuration

### Integration Documentation
- [ ] `docs/playcanvas-integration.md` - Integration guide
- [ ] `README.md` - Updated with PlayCanvas content

---

## 🚀 Deployment Workflow

### For Your AI Team

**Mission**: Import PlayCanvas template and Playground Porter agent into waystid-game-forge

**Steps**:

1. **Clone Repository**
   ```bash
   git clone https://github.com/waystid/waystid-game-forge.git
   cd waystid-game-forge
   ```

2. **Create Feature Branch**
   ```bash
   git checkout -b feature/add-playcanvas-template
   ```

3. **Run Import Script** (or manual commands)
   ```bash
   ./scripts/import-playcanvas.sh
   ```

4. **Verify Import**
   - Check all files copied correctly
   - Validate JavaScript syntax
   - Validate JSON files
   - Review documentation

5. **Commit & Push**
   ```bash
   git add .
   git commit -m "Add PlayCanvas template and Playground Porter agent"
   git push origin feature/add-playcanvas-template
   ```

6. **Create Pull Request**
   - Go to GitHub
   - Create PR from feature branch to main
   - Add description
   - Request review

7. **After Merge**
   - Template available at: `templates/playcanvas/cozy-game-template/`
   - Agent available at: `agents/playground-porter/`
   - Documentation at: `docs/playcanvas-integration.md`

---

## 📝 Commit Message Template

```
Add PlayCanvas template and Playground Porter agent

This commit adds two major components to Waystid Game Forge:

1. Cozy Game Template (PlayCanvas)
   - Complete game systems (inventory, farming, quests, dialogue, magic)
   - Player controller and world systems (time, weather)
   - Full HTML/CSS UI with mobile support
   - Sample game data (items, quests, plants, spells, NPCs)
   - Comprehensive documentation

2. Playground Porter Agent
   - AI agent for converting Unity/Unreal assets to PlayCanvas
   - Complete specification and system prompt
   - Example conversions and documentation
   - Quick reference guide

Location:
- Template: templates/playcanvas/cozy-game-template/
- Agent: agents/playground-porter/

Both components are production-ready and include complete documentation.
```

---

## 🎯 Success Criteria

Integration is successful when:

✅ **File Structure**
- [ ] All template files in correct locations
- [ ] All agent files in correct locations
- [ ] Documentation files present

✅ **Validation**
- [ ] JavaScript files have valid syntax
- [ ] JSON files are valid
- [ ] Markdown files are readable

✅ **Documentation**
- [ ] Template README complete
- [ ] Agent README complete
- [ ] Integration guide created
- [ ] Main README updated

✅ **Git**
- [ ] Changes committed
- [ ] Pushed to feature branch
- [ ] Pull request created
- [ ] Merged to main

✅ **Community Access**
- [ ] Template accessible in repository
- [ ] Agent accessible in repository
- [ ] Documentation clear and helpful

---

## 💡 Additional Notes

### File Locations Reference

**Source Files** (from robyn_game):
```
/home/user/robyn_game/
├── PlayCanvas/                    # Template source
│   ├── scripts/
│   ├── ui/
│   ├── data/
│   └── *.md
├── agents/PlaygroundPorter/       # Agent source
│   └── *.md
└── CozyGameTemplate-PlayCanvas-v1.0.0.zip  # Complete package
```

**Target Files** (in waystid-game-forge):
```
waystid-game-forge/
├── templates/playcanvas/cozy-game-template/
├── agents/playground-porter/
└── docs/playcanvas-integration.md
```

### Customization Options

After import, consider:
- Adding more example data (items, quests, etc.)
- Creating additional templates
- Extending agent capabilities
- Adding tutorial videos
- Creating community showcase

---

## 📞 Support

If you encounter issues during import:

1. **Check file paths** - Ensure source files are accessible
2. **Verify permissions** - Ensure write access to repository
3. **Review errors** - Check git output for specific errors
4. **Validate syntax** - Use provided validation commands
5. **Consult docs** - Reference template and agent documentation

---

## ✨ Final Checklist

Before submitting PR:

- [ ] All files copied correctly
- [ ] Directory structure matches specification
- [ ] JavaScript syntax validated
- [ ] JSON files validated
- [ ] Markdown files readable
- [ ] template-info.json created
- [ ] agent-config.json created
- [ ] Integration docs created
- [ ] README.md updated
- [ ] Git history clean
- [ ] PR description complete

---

**Ready to Import!** 🚀

Follow the steps above to successfully integrate the PlayCanvas template and Playground Porter agent into Waystid Game Forge.

**Questions?** Review the documentation in:
- `templates/playcanvas/cozy-game-template/docs/`
- `agents/playground-porter/`

---

*Created for Waystid Game Forge - Building the future of game development together*
