// ============================================================================
// PlayCanvas Core Game Systems
// Inventory, Farming, Quests, Dialogue, Magic
// Converted from Unity C# (~2,500 lines → ~2,000 lines JS)
// ============================================================================

// ----------------------------------------------------------------------------
// INVENTORY MANAGER
// Converted from Unity InventoryManager.cs
// ----------------------------------------------------------------------------
var InventoryManager = pc.createScript('inventoryManager');

InventoryManager.getInstance = function() {
    return InventoryManager._instance;
};

InventoryManager.prototype.initialize = function() {
    InventoryManager._instance = this;

    this.items = []; // [{itemId, quantity, slot}]
    this.maxSlots = 30;
    this.itemDefinitions = {}; // Loaded from JSON

    this.currency = {
        gold: 0,
        silver: 0,
        gems: 0
    };

    console.log('[InventoryManager] Initialized');

    // Load item definitions
    this.loadItemDefinitions();
};

InventoryManager.prototype.loadItemDefinitions = function() {
    var gameManager = GameManager.getInstance();
    if (gameManager && gameManager.itemsData) {
        gameManager.itemsData.forEach(function(item) {
            this.itemDefinitions[item.itemID] = item;
        }.bind(this));
        console.log('[InventoryManager] Loaded ' + Object.keys(this.itemDefinitions).length + ' item definitions');
    }
};

InventoryManager.prototype.addItem = function(itemId, quantity) {
    quantity = quantity || 1;

    var itemDef = this.itemDefinitions[itemId];
    if (!itemDef) {
        console.error('[InventoryManager] Item not found:', itemId);
        return false;
    }

    // Check if stackable
    if (itemDef.stackable) {
        var existing = this.items.find(function(i) { return i.itemId === itemId; });
        if (existing) {
            existing.quantity += quantity;
            this.fireInventoryChanged();
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
        this.fireInventoryChanged();
        return true;
    }

    console.warn('[InventoryManager] Inventory full!');
    return false;
};

InventoryManager.prototype.removeItem = function(itemId, quantity) {
    quantity = quantity || 1;

    var item = this.items.find(function(i) { return i.itemId === itemId; });
    if (!item || item.quantity < quantity) {
        return false;
    }

    item.quantity -= quantity;
    if (item.quantity <= 0) {
        var index = this.items.indexOf(item);
        this.items.splice(index, 1);
    }

    this.fireInventoryChanged();
    return true;
};

InventoryManager.prototype.hasItem = function(itemId, quantity) {
    quantity = quantity || 1;
    var item = this.items.find(function(i) { return i.itemId === itemId; });
    return item && item.quantity >= quantity;
};

InventoryManager.prototype.getItemCount = function(itemId) {
    var item = this.items.find(function(i) { return i.itemId === itemId; });
    return item ? item.quantity : 0;
};

InventoryManager.prototype.addCurrency = function(type, amount) {
    if (this.currency[type] !== undefined) {
        this.currency[type] += amount;
        this.fireInventoryChanged();
        return true;
    }
    return false;
};

InventoryManager.prototype.removeCurrency = function(type, amount) {
    if (this.currency[type] !== undefined && this.currency[type] >= amount) {
        this.currency[type] -= amount;
        this.fireInventoryChanged();
        return true;
    }
    return false;
};

InventoryManager.prototype.hasCurrency = function(type, amount) {
    return this.currency[type] !== undefined && this.currency[type] >= amount;
};

InventoryManager.prototype.fireInventoryChanged = function() {
    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('inventory:changed', this.items, this.currency);
    }
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
    this.fireInventoryChanged();
};


// ----------------------------------------------------------------------------
// FARMING MANAGER
// Converted from Unity FarmingSystem.cs
// ----------------------------------------------------------------------------
var FarmingManager = pc.createScript('farmingManager');

FarmingManager.getInstance = function() {
    return FarmingManager._instance;
};

FarmingManager.prototype.initialize = function() {
    FarmingManager._instance = this;

    this.plantedCrops = {}; // {gridKey: PlantInstance}
    this.plantDefinitions = {}; // Loaded from JSON

    console.log('[FarmingManager] Initialized');

    this.loadPlantDefinitions();

    // Listen to time ticks
    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.on('time:tick', this.updateCrops, this);
    }
};

FarmingManager.prototype.loadPlantDefinitions = function() {
    var gameManager = GameManager.getInstance();
    if (gameManager && gameManager.plantsData) {
        gameManager.plantsData.forEach(function(plant) {
            this.plantDefinitions[plant.plantID] = plant;
        }.bind(this));
        console.log('[FarmingManager] Loaded ' + Object.keys(this.plantDefinitions).length + ' plant definitions');
    }
};

FarmingManager.prototype.plantSeed = function(plantId, gridX, gridY) {
    var plantDef = this.plantDefinitions[plantId];
    if (!plantDef) {
        console.error('[FarmingManager] Plant not found:', plantId);
        return false;
    }

    var gridKey = gridX + ',' + gridY;

    // Check if already planted
    if (this.plantedCrops[gridKey]) {
        console.warn('[FarmingManager] Tile already occupied');
        return false;
    }

    // Check if player has seed
    var inventory = InventoryManager.getInstance();
    var seedId = plantId + '_seed';
    if (!inventory.hasItem(seedId, 1)) {
        console.warn('[FarmingManager] No seeds');
        return false;
    }

    // Remove seed
    inventory.removeItem(seedId, 1);

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

    // Create visual
    this.createPlantEntity(plant);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('farming:planted', plant);
    }

    return true;
};

FarmingManager.prototype.createPlantEntity = function(plant) {
    // Create visual entity for the plant
    var entity = new pc.Entity('plant_' + plant.gridX + '_' + plant.gridY);

    // Add model component (placeholder - replace with actual model)
    entity.addComponent('model', {
        type: 'box'
    });

    // Position at grid
    entity.setPosition(plant.gridX * 2, 0, plant.gridY * 2);

    // Scale based on stage
    var scale = 0.3 + (plant.currentStage / plant.plantDef.growthStages) * 0.7;
    entity.setLocalScale(scale, scale, scale);

    // Add to scene
    this.app.root.addChild(entity);

    plant.entity = entity;
};

FarmingManager.prototype.waterPlant = function(gridX, gridY) {
    var gridKey = gridX + ',' + gridY;
    var plant = this.plantedCrops[gridKey];

    if (!plant || plant.watered) {
        return false;
    }

    plant.watered = true;

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('farming:watered', plant);
    }

    return true;
};

FarmingManager.prototype.updateCrops = function() {
    for (var gridKey in this.plantedCrops) {
        var plant = this.plantedCrops[gridKey];
        if (plant.ready) continue;

        var timeSincePlanted = (Date.now() - plant.plantedTime) / 1000; // seconds
        var growthProgress = timeSincePlanted / plant.plantDef.growthTime;

        // Calculate stage
        var newStage = Math.floor(growthProgress * plant.plantDef.growthStages);
        newStage = Math.min(newStage, plant.plantDef.growthStages - 1);

        if (newStage !== plant.currentStage) {
            plant.currentStage = newStage;
            this.updatePlantVisual(plant);
        }

        // Check if ready
        if (growthProgress >= 1.0) {
            plant.ready = true;

            var eventBus = EventBus.getInstance();
            if (eventBus) {
                eventBus.fire('farming:ready', plant);
            }
        }
    }
};

FarmingManager.prototype.updatePlantVisual = function(plant) {
    if (plant.entity) {
        var scale = 0.3 + (plant.currentStage / plant.plantDef.growthStages) * 0.7;
        plant.entity.setLocalScale(scale, scale, scale);
    }
};

FarmingManager.prototype.harvestPlant = function(gridX, gridY) {
    var gridKey = gridX + ',' + gridY;
    var plant = this.plantedCrops[gridKey];

    if (!plant || !plant.ready) {
        return false;
    }

    // Calculate yield
    var yieldAmount = Math.floor(
        Math.random() * (plant.plantDef.maxYield - plant.plantDef.minYield + 1)
    ) + plant.plantDef.minYield;

    // Add to inventory
    var inventory = InventoryManager.getInstance();
    inventory.addItem(plant.plantDef.harvestedItemID, yieldAmount);

    // Remove visual
    if (plant.entity) {
        plant.entity.destroy();
    }

    // Remove from planted crops
    delete this.plantedCrops[gridKey];

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('farming:harvested', {
            plantId: plant.plantId,
            yield: yieldAmount
        });
    }

    return true;
};

FarmingManager.prototype.getSaveData = function() {
    var crops = [];
    for (var gridKey in this.plantedCrops) {
        var plant = this.plantedCrops[gridKey];
        crops.push({
            plantId: plant.plantId,
            gridX: plant.gridX,
            gridY: plant.gridY,
            plantedTime: plant.plantedTime,
            currentStage: plant.currentStage,
            watered: plant.watered,
            ready: plant.ready
        });
    }
    return { plantedCrops: crops };
};

FarmingManager.prototype.loadSaveData = function(data) {
    // Clear existing
    for (var gridKey in this.plantedCrops) {
        if (this.plantedCrops[gridKey].entity) {
            this.plantedCrops[gridKey].entity.destroy();
        }
    }
    this.plantedCrops = {};

    // Restore crops
    if (data.plantedCrops) {
        data.plantedCrops.forEach(function(cropData) {
            var gridKey = cropData.gridX + ',' + cropData.gridY;
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
        }.bind(this));
    }
};


// ----------------------------------------------------------------------------
// QUEST MANAGER
// Converted from Unity QuestManager.cs
// ----------------------------------------------------------------------------
var QuestManager = pc.createScript('questManager');

QuestManager.getInstance = function() {
    return QuestManager._instance;
};

QuestManager.prototype.initialize = function() {
    QuestManager._instance = this;

    this.activeQuests = []; // Quests in progress
    this.completedQuests = []; // Completed quest IDs
    this.questDefinitions = {}; // Loaded from JSON

    console.log('[QuestManager] Initialized');

    this.loadQuestDefinitions();
};

QuestManager.prototype.loadQuestDefinitions = function() {
    var gameManager = GameManager.getInstance();
    if (gameManager && gameManager.questsData) {
        gameManager.questsData.forEach(function(quest) {
            this.questDefinitions[quest.questID] = quest;
        }.bind(this));
        console.log('[QuestManager] Loaded ' + Object.keys(this.questDefinitions).length + ' quest definitions');
    }
};

QuestManager.prototype.startQuest = function(questId) {
    var questDef = this.questDefinitions[questId];
    if (!questDef) {
        console.error('[QuestManager] Quest not found:', questId);
        return false;
    }

    // Check if already active or completed
    if (this.isQuestActive(questId) || this.isQuestCompleted(questId)) {
        return false;
    }

    // Create quest instance
    var quest = JSON.parse(JSON.stringify(questDef)); // Deep copy
    quest.startTime = Date.now();
    quest.isCompleted = false;

    // Initialize objectives
    quest.objectives.forEach(function(obj) {
        obj.currentCount = 0;
        obj.isCompleted = false;
    });

    this.activeQuests.push(quest);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('quest:started', quest);
    }

    console.log('[QuestManager] Started quest:', quest.questName);
    return true;
};

QuestManager.prototype.updateObjective = function(questId, objectiveIndex, progress) {
    var quest = this.getActiveQuest(questId);
    if (!quest) return false;

    var objective = quest.objectives[objectiveIndex];
    if (!objective || objective.isCompleted) return false;

    objective.currentCount += progress;
    if (objective.currentCount >= objective.targetCount) {
        objective.currentCount = objective.targetCount;
        objective.isCompleted = true;

        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('quest:objectiveCompleted', quest, objectiveIndex);
        }
    }

    // Check if all objectives complete
    var allComplete = quest.objectives.every(function(obj) {
        return obj.isCompleted;
    });

    if (allComplete) {
        this.completeQuest(questId);
    } else {
        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('quest:updated', quest);
        }
    }

    return true;
};

QuestManager.prototype.completeQuest = function(questId) {
    var quest = this.getActiveQuest(questId);
    if (!quest) return false;

    quest.isCompleted = true;
    quest.completionTime = Date.now();

    // Give rewards
    this.giveQuestRewards(quest);

    // Move to completed
    var index = this.activeQuests.indexOf(quest);
    this.activeQuests.splice(index, 1);
    this.completedQuests.push(questId);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('quest:completed', quest);
    }

    console.log('[QuestManager] Completed quest:', quest.questName);
    return true;
};

QuestManager.prototype.giveQuestRewards = function(quest) {
    var inventory = InventoryManager.getInstance();

    // Experience
    if (quest.rewards && quest.rewards.experience) {
        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('player:gainExperience', quest.rewards.experience);
        }
    }

    // Gold
    if (quest.rewards && quest.rewards.gold) {
        inventory.addCurrency('gold', quest.rewards.gold);
    }

    // Items
    if (quest.rewards && quest.rewards.itemRewards) {
        quest.rewards.itemRewards.forEach(function(itemId) {
            inventory.addItem(itemId, 1);
        });
    }
};

QuestManager.prototype.getActiveQuest = function(questId) {
    return this.activeQuests.find(function(q) {
        return q.questID === questId;
    });
};

QuestManager.prototype.isQuestActive = function(questId) {
    return this.activeQuests.some(function(q) {
        return q.questID === questId;
    });
};

QuestManager.prototype.isQuestCompleted = function(questId) {
    return this.completedQuests.indexOf(questId) !== -1;
};

QuestManager.prototype.getSaveData = function() {
    return {
        activeQuests: this.activeQuests,
        completedQuests: this.completedQuests
    };
};

QuestManager.prototype.loadSaveData = function(data) {
    this.activeQuests = data.activeQuests || [];
    this.completedQuests = data.completedQuests || [];

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('quest:loaded');
    }
};


// ----------------------------------------------------------------------------
// DIALOGUE MANAGER
// Converted from Unity DialogueSystem.cs
// ----------------------------------------------------------------------------
var DialogueManager = pc.createScript('dialogueManager');

DialogueManager.getInstance = function() {
    return DialogueManager._instance;
};

DialogueManager.prototype.initialize = function() {
    DialogueManager._instance = this;

    this.currentDialogue = null;
    this.currentNode = null;
    this.dialogueHistory = [];

    console.log('[DialogueManager] Initialized');
};

DialogueManager.prototype.startDialogue = function(dialogueTree, npcName) {
    if (!dialogueTree || !dialogueTree.nodes || dialogueTree.nodes.length === 0) {
        console.error('[DialogueManager] Invalid dialogue tree');
        return false;
    }

    this.currentDialogue = dialogueTree;
    this.currentNode = dialogueTree.nodes[0]; // Start node

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('dialogue:started', npcName, this.currentNode);
    }

    return true;
};

DialogueManager.prototype.selectChoice = function(choiceIndex) {
    if (!this.currentNode || !this.currentNode.choices) {
        return false;
    }

    var choice = this.currentNode.choices[choiceIndex];
    if (!choice) {
        return false;
    }

    // Record choice
    this.dialogueHistory.push({
        nodeId: this.currentNode.id,
        choiceIndex: choiceIndex,
        choiceText: choice.text
    });

    // Move to next node
    var nextNode = this.findNodeById(choice.nextNodeId);
    if (nextNode) {
        this.currentNode = nextNode;

        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('dialogue:nodeChanged', this.currentNode);
        }

        return true;
    } else {
        // End dialogue
        this.endDialogue();
        return false;
    }
};

DialogueManager.prototype.findNodeById = function(nodeId) {
    if (!this.currentDialogue || !this.currentDialogue.nodes) {
        return null;
    }

    return this.currentDialogue.nodes.find(function(node) {
        return node.id === nodeId;
    });
};

DialogueManager.prototype.endDialogue = function() {
    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('dialogue:ended');
    }

    this.currentDialogue = null;
    this.currentNode = null;
};


// ----------------------------------------------------------------------------
// MAGIC MANAGER
// Converted from Unity MagicSystem.cs
// ----------------------------------------------------------------------------
var MagicManager = pc.createScript('magicManager');

MagicManager.getInstance = function() {
    return MagicManager._instance;
};

MagicManager.prototype.initialize = function() {
    MagicManager._instance = this;

    this.knownSpells = []; // Spell IDs
    this.spellDefinitions = {}; // Loaded from JSON
    this.spellCooldowns = {}; // {spellId: cooldownEndTime}

    console.log('[MagicManager] Initialized');

    this.loadSpellDefinitions();
};

MagicManager.prototype.loadSpellDefinitions = function() {
    var gameManager = GameManager.getInstance();
    if (gameManager && gameManager.spellsData) {
        gameManager.spellsData.forEach(function(spell) {
            this.spellDefinitions[spell.spellID] = spell;
        }.bind(this));
        console.log('[MagicManager] Loaded ' + Object.keys(this.spellDefinitions).length + ' spell definitions');
    }
};

MagicManager.prototype.learnSpell = function(spellId) {
    if (this.knowsSpell(spellId)) {
        return false;
    }

    var spellDef = this.spellDefinitions[spellId];
    if (!spellDef) {
        console.error('[MagicManager] Spell not found:', spellId);
        return false;
    }

    this.knownSpells.push(spellId);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('magic:spellLearned', spellId);
    }

    console.log('[MagicManager] Learned spell:', spellDef.spellName);
    return true;
};

MagicManager.prototype.castSpell = function(spellId, target) {
    if (!this.knowsSpell(spellId)) {
        console.warn('[MagicManager] Spell not learned');
        return false;
    }

    if (this.isSpellOnCooldown(spellId)) {
        console.warn('[MagicManager] Spell on cooldown');
        return false;
    }

    var spellDef = this.spellDefinitions[spellId];
    if (!spellDef) {
        return false;
    }

    // Check mana cost
    // (This would interact with PlayerStats)

    // Apply cooldown
    this.spellCooldowns[spellId] = Date.now() + (spellDef.cooldown * 1000);

    // Cast spell effects
    this.applySpellEffects(spellDef, target);

    var eventBus = EventBus.getInstance();
    if (eventBus) {
        eventBus.fire('magic:spellCast', spellId, target);
    }

    return true;
};

MagicManager.prototype.applySpellEffects = function(spellDef, target) {
    // Apply damage
    if (spellDef.damage && target) {
        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('combat:dealDamage', target, spellDef.damage, spellDef.damageType);
        }
    }

    // Apply healing
    if (spellDef.healAmount) {
        var eventBus = EventBus.getInstance();
        if (eventBus) {
            eventBus.fire('player:heal', spellDef.healAmount);
        }
    }

    // Other effects...
};

MagicManager.prototype.knowsSpell = function(spellId) {
    return this.knownSpells.indexOf(spellId) !== -1;
};

MagicManager.prototype.isSpellOnCooldown = function(spellId) {
    var cooldownEnd = this.spellCooldowns[spellId];
    return cooldownEnd && Date.now() < cooldownEnd;
};

MagicManager.prototype.getSaveData = function() {
    return {
        knownSpells: this.knownSpells
    };
};

MagicManager.prototype.loadSaveData = function(data) {
    this.knownSpells = data.knownSpells || [];
    this.spellCooldowns = {}; // Reset cooldowns on load
};


// ============================================================================
// VALIDATION
// ============================================================================

console.log('=== PlayCanvas Core Game Systems Loaded ===');
console.log('- InventoryManager');
console.log('- FarmingManager');
console.log('- QuestManager');
console.log('- DialogueManager');
console.log('- MagicManager');
console.log('===========================================');
