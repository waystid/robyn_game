// ============================================================================
// PlayCanvas Core Managers
// Converted from Unity C# to JavaScript
// ============================================================================

// ----------------------------------------------------------------------------
// EVENT BUS - Global event system
// Replaces Unity's UnityEvents
// ----------------------------------------------------------------------------
var EventBus = pc.createScript('eventBus');

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

EventBus.prototype.on = function(eventName, callback, scope) {
    this.events.on(eventName, callback, scope);
};

EventBus.prototype.off = function(eventName, callback, scope) {
    this.events.off(eventName, callback, scope);
};

EventBus.prototype.fire = function(eventName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) {
    this.events.fire(eventName, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
};

EventBus.prototype.once = function(eventName, callback, scope) {
    this.events.once(eventName, callback, scope);
};


// ----------------------------------------------------------------------------
// SAVE SYSTEM - LocalStorage-based persistence
// Replaces Unity's PlayerPrefs/BinaryFormatter
// ----------------------------------------------------------------------------
var SaveSystem = pc.createScript('saveSystem');

SaveSystem.getInstance = function() {
    if (!SaveSystem._instance) {
        var root = pc.app.root.findByName('SaveSystem');
        if (root) {
            SaveSystem._instance = root.script.saveSystem;
        }
    }
    return SaveSystem._instance;
};

SaveSystem.prototype.initialize = function() {
    SaveSystem._instance = this;

    this.saveKey = 'cozy_game_save';
    this.autoSaveInterval = 60; // Auto-save every 60 seconds
    this.autoSaveEnabled = true;
    this.lastSaveTime = 0;

    console.log('[SaveSystem] Initialized');
};

SaveSystem.prototype.update = function(dt) {
    if (this.autoSaveEnabled) {
        this.lastSaveTime += dt;
        if (this.lastSaveTime >= this.autoSaveInterval) {
            this.saveGame();
            this.lastSaveTime = 0;
        }
    }
};

SaveSystem.prototype.saveGame = function() {
    try {
        var saveData = this.collectSaveData();
        var jsonString = JSON.stringify(saveData);
        localStorage.setItem(this.saveKey, jsonString);

        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('game:saved', saveData);
        }

        console.log('[SaveSystem] Game saved successfully');
        return true;
    } catch (e) {
        console.error('[SaveSystem] Failed to save game:', e);
        return false;
    }
};

SaveSystem.prototype.loadGame = function() {
    try {
        var jsonString = localStorage.getItem(this.saveKey);
        if (!jsonString) {
            console.log('[SaveSystem] No save file found');
            return null;
        }

        var saveData = JSON.parse(jsonString);
        this.applySaveData(saveData);

        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('game:loaded', saveData);
        }

        console.log('[SaveSystem] Game loaded successfully');
        return saveData;
    } catch (e) {
        console.error('[SaveSystem] Failed to load game:', e);
        return null;
    }
};

SaveSystem.prototype.collectSaveData = function() {
    var saveData = {
        version: '1.0.0',
        timestamp: Date.now(),
        data: {}
    };

    // Collect data from all managers
    var managers = [
        'InventoryManager',
        'FarmingManager',
        'QuestManager',
        'PlayerStats',
        'TimeManager',
        'BuildingManager',
        'RelationshipManager'
    ];

    managers.forEach(function(managerName) {
        var managerClass = window[managerName];
        if (managerClass && managerClass.getInstance) {
            var instance = managerClass.getInstance();
            if (instance && instance.getSaveData) {
                saveData.data[managerName] = instance.getSaveData();
            }
        }
    });

    return saveData;
};

SaveSystem.prototype.applySaveData = function(saveData) {
    if (!saveData || !saveData.data) {
        console.error('[SaveSystem] Invalid save data');
        return;
    }

    // Apply data to all managers
    for (var managerName in saveData.data) {
        var managerClass = window[managerName];
        if (managerClass && managerClass.getInstance) {
            var instance = managerClass.getInstance();
            if (instance && instance.loadSaveData) {
                instance.loadSaveData(saveData.data[managerName]);
            }
        }
    }
};

SaveSystem.prototype.deleteSave = function() {
    localStorage.removeItem(this.saveKey);
    console.log('[SaveSystem] Save file deleted');
};

SaveSystem.prototype.hasSaveFile = function() {
    return localStorage.getItem(this.saveKey) !== null;
};


// ----------------------------------------------------------------------------
// GAME MANAGER - Core game state and coordination
// Replaces Unity's GameManager singleton
// ----------------------------------------------------------------------------
var GameManager = pc.createScript('gameManager');

GameManager.getInstance = function() {
    return GameManager._instance;
};

GameManager.prototype.initialize = function() {
    GameManager._instance = this;

    // Game state
    this.gameState = 'loading'; // loading, menu, playing, paused
    this.playerEntity = null;
    this.currentScene = 'main';

    // References
    this.eventBus = null;
    this.saveSystem = null;

    // Data loaded flags
    this.dataLoaded = {
        items: false,
        quests: false,
        plants: false,
        spells: false,
        npcs: false
    };

    console.log('[GameManager] Initialized');

    // Find core systems
    this.findCoreSystems();

    // Load game data
    this.loadAllGameData();
};

GameManager.prototype.findCoreSystems = function() {
    this.eventBus = EventBus.getInstance();
    this.saveSystem = SaveSystem.getInstance();

    if (!this.eventBus) {
        console.warn('[GameManager] EventBus not found!');
    }
    if (!this.saveSystem) {
        console.warn('[GameManager] SaveSystem not found!');
    }
};

GameManager.prototype.loadAllGameData = function() {
    var self = this;

    // Load all JSON data files
    var dataFiles = [
        { name: 'items', url: 'data/items.json' },
        { name: 'quests', url: 'data/quests.json' },
        { name: 'plants', url: 'data/plants.json' },
        { name: 'spells', url: 'data/spells.json' },
        { name: 'npcs', url: 'data/npcs.json' }
    ];

    var loadCount = 0;
    var totalFiles = dataFiles.length;

    dataFiles.forEach(function(file) {
        self.app.assets.loadFromUrl(file.url, 'json', function(err, asset) {
            if (err) {
                console.error('[GameManager] Failed to load ' + file.name + ':', err);
            } else {
                self[file.name + 'Data'] = asset.resource;
                self.dataLoaded[file.name] = true;
                console.log('[GameManager] Loaded ' + file.name + ':', asset.resource.length);
            }

            loadCount++;
            if (loadCount === totalFiles) {
                self.onAllDataLoaded();
            }
        });
    });
};

GameManager.prototype.onAllDataLoaded = function() {
    console.log('[GameManager] All game data loaded successfully');

    if (this.eventBus) {
        this.eventBus.fire('data:loaded');
    }

    // Try to load save file
    if (this.saveSystem && this.saveSystem.hasSaveFile()) {
        this.saveSystem.loadGame();
    }

    // Change state to menu
    this.setGameState('menu');
};

GameManager.prototype.setGameState = function(newState) {
    var oldState = this.gameState;
    this.gameState = newState;

    console.log('[GameManager] State changed: ' + oldState + ' -> ' + newState);

    if (this.eventBus) {
        this.eventBus.fire('game:stateChanged', newState, oldState);
    }

    // Handle state transitions
    switch (newState) {
        case 'playing':
            this.onGameStart();
            break;
        case 'paused':
            this.onGamePause();
            break;
        case 'menu':
            this.onGameMenu();
            break;
    }
};

GameManager.prototype.onGameStart = function() {
    console.log('[GameManager] Game started');

    // Find player
    this.playerEntity = this.app.root.findByName('Player');
    if (!this.playerEntity) {
        console.warn('[GameManager] Player entity not found!');
    }
};

GameManager.prototype.onGamePause = function() {
    console.log('[GameManager] Game paused');
    // Pause game logic (not implemented in this example)
};

GameManager.prototype.onGameMenu = function() {
    console.log('[GameManager] Show menu');
};

GameManager.prototype.newGame = function() {
    // Delete save and start fresh
    if (this.saveSystem) {
        this.saveSystem.deleteSave();
    }

    // Reset all managers
    this.resetAllManagers();

    // Start game
    this.setGameState('playing');
};

GameManager.prototype.continueGame = function() {
    // Load save and continue
    if (this.saveSystem) {
        this.saveSystem.loadGame();
    }

    this.setGameState('playing');
};

GameManager.prototype.resetAllManagers = function() {
    var managers = [
        'InventoryManager',
        'FarmingManager',
        'QuestManager',
        'PlayerStats',
        'TimeManager'
    ];

    managers.forEach(function(managerName) {
        var managerClass = window[managerName];
        if (managerClass && managerClass.getInstance) {
            var instance = managerClass.getInstance();
            if (instance && instance.reset) {
                instance.reset();
            }
        }
    });
};

GameManager.prototype.quitGame = function() {
    // Save before quitting
    if (this.saveSystem) {
        this.saveSystem.saveGame();
    }

    this.setGameState('menu');
};

GameManager.prototype.update = function(dt) {
    // Main game loop
    if (this.gameState === 'playing') {
        // Game logic updates handled by individual managers
    }
};


// ============================================================================
// VALIDATION
// ============================================================================

console.log('=== PlayCanvas Core Managers Loaded ===');
console.log('- EventBus');
console.log('- SaveSystem');
console.log('- GameManager');
console.log('=====================================');
