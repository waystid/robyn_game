// ============================================================================
// UI Controller - Waystid Game Forge
// Handles all HTML/CSS UI interactions and integrates with PlayCanvas systems
// ============================================================================

var UIController = pc.createScript('uiController');

UIController.getInstance = function() {
    return UIController._instance;
};

UIController.prototype.initialize = function() {
    UIController._instance = this;

    // UI Elements
    this.loadingScreen = document.getElementById('loading-screen');
    this.mainMenu = document.getElementById('main-menu');
    this.gameHud = document.getElementById('game-hud');
    this.pauseMenu = document.getElementById('pause-menu');

    // Panels
    this.inventoryPanel = document.getElementById('inventory-panel');
    this.questPanel = document.getElementById('quest-panel');
    this.shopPanel = document.getElementById('shop-panel');
    this.craftingPanel = document.getElementById('crafting-panel');
    this.settingsPanel = document.getElementById('settings-panel');

    // Dialogue
    this.dialogueBox = document.getElementById('dialogue-box');

    // Notification
    this.notificationContainer = document.getElementById('notification-container');

    // Mobile
    this.mobileControls = document.getElementById('mobile-controls');

    // State
    this.currentPanel = null;
    this.isPaused = false;

    console.log('[UIController] Initialized');

    // Setup event listeners
    this.setupEventListeners();

    // Listen to game events
    this.setupGameEvents();

    // Detect mobile
    this.detectMobile();

    // Show loading screen initially
    this.showLoading();
};

// ============================================================================
// Event Listeners Setup
// ============================================================================

UIController.prototype.setupEventListeners = function() {
    // Main Menu
    var btnNewGame = document.getElementById('btn-new-game');
    var btnContinueGame = document.getElementById('btn-continue-game');
    var btnSettings = document.getElementById('btn-settings');

    if (btnNewGame) {
        btnNewGame.addEventListener('click', this.onNewGame.bind(this));
    }
    if (btnContinueGame) {
        btnContinueGame.addEventListener('click', this.onContinueGame.bind(this));
    }
    if (btnSettings) {
        btnSettings.addEventListener('click', this.onShowSettings.bind(this));
    }

    // HUD Shortcuts
    var btnInventory = document.getElementById('btn-inventory');
    var btnQuests = document.getElementById('btn-quests');

    if (btnInventory) {
        btnInventory.addEventListener('click', this.toggleInventory.bind(this));
    }
    if (btnQuests) {
        btnQuests.addEventListener('click', this.toggleQuests.bind(this));
    }

    // Close buttons (all panels)
    var closeButtons = document.querySelectorAll('.btn-close');
    closeButtons.forEach(function(btn) {
        btn.addEventListener('click', this.closeCurrentPanel.bind(this));
    }.bind(this));

    // Keyboard shortcuts
    document.addEventListener('keydown', this.onKeyDown.bind(this));

    // Pause menu
    var btnResume = document.getElementById('btn-resume');
    if (btnResume) {
        btnResume.addEventListener('click', this.resumeGame.bind(this));
    }
};

UIController.prototype.setupGameEvents = function() {
    var eventBus = EventBus.getInstance();
    if (!eventBus) return;

    // Listen to game events
    eventBus.on('data:loaded', this.onDataLoaded, this);
    eventBus.on('game:stateChanged', this.onGameStateChanged, this);
    eventBus.on('inventory:changed', this.updateInventoryUI, this);
    eventBus.on('quest:started', this.onQuestStarted, this);
    eventBus.on('quest:updated', this.updateQuestUI, this);
    eventBus.on('quest:completed', this.onQuestCompleted, this);
    eventBus.on('dialogue:started', this.onDialogueStarted, this);
    eventBus.on('dialogue:ended', this.onDialogueEnded, this);
    eventBus.on('farming:harvested', this.onFarmingHarvested, this);
};

// ============================================================================
// Loading Screen
// ============================================================================

UIController.prototype.showLoading = function() {
    this.loadingScreen.classList.remove('hidden');
};

UIController.prototype.hideLoading = function() {
    this.loadingScreen.classList.add('hidden');
};

UIController.prototype.updateLoadingProgress = function(progress, text) {
    var progressBar = document.getElementById('loading-progress');
    var loadingText = document.getElementById('loading-text');

    if (progressBar) {
        progressBar.style.width = (progress * 100) + '%';
    }
    if (loadingText && text) {
        loadingText.textContent = text;
    }
};

// ============================================================================
// Game State
// ============================================================================

UIController.prototype.onDataLoaded = function() {
    this.updateLoadingProgress(1, 'Ready!');

    setTimeout(function() {
        this.hideLoading();
        this.showMainMenu();
    }.bind(this), 500);
};

UIController.prototype.onGameStateChanged = function(newState, oldState) {
    console.log('[UIController] Game state:', newState);

    switch (newState) {
        case 'menu':
            this.showMainMenu();
            this.hideGameHud();
            break;
        case 'playing':
            this.hideMainMenu();
            this.showGameHud();
            break;
        case 'paused':
            this.showPauseMenu();
            break;
    }
};

// ============================================================================
// Main Menu
// ============================================================================

UIController.prototype.showMainMenu = function() {
    this.mainMenu.classList.remove('hidden');

    // Check if save file exists
    var saveSystem = SaveSystem.getInstance();
    var btnContinue = document.getElementById('btn-continue-game');
    if (btnContinue && saveSystem) {
        btnContinue.disabled = !saveSystem.hasSaveFile();
    }
};

UIController.prototype.hideMainMenu = function() {
    this.mainMenu.classList.add('hidden');
};

UIController.prototype.onNewGame = function() {
    var gameManager = GameManager.getInstance();
    if (gameManager) {
        gameManager.newGame();
    }
};

UIController.prototype.onContinueGame = function() {
    var gameManager = GameManager.getInstance();
    if (gameManager) {
        gameManager.continueGame();
    }
};

UIController.prototype.onShowSettings = function() {
    this.openPanel(this.settingsPanel);
};

// ============================================================================
// Game HUD
// ============================================================================

UIController.prototype.showGameHud = function() {
    this.gameHud.classList.remove('hidden');
    this.updateAllHUD();
};

UIController.prototype.hideGameHud = function() {
    this.gameHud.classList.add('hidden');
};

UIController.prototype.updateAllHUD = function() {
    this.updateHealthBar();
    this.updateManaBar();
    this.updateCurrency();
    this.updateTime();
};

UIController.prototype.updateHealthBar = function() {
    // This would get data from PlayerController
    var player = this.app.root.findByName('Player');
    if (!player || !player.script || !player.script.playerController) return;

    var stats = player.script.playerController.stats;
    var healthFill = document.getElementById('health-fill');
    var healthText = document.getElementById('health-text');

    if (healthFill && healthText) {
        var percent = (stats.health / stats.maxHealth) * 100;
        healthFill.style.width = percent + '%';
        healthText.textContent = stats.health + '/' + stats.maxHealth;
    }
};

UIController.prototype.updateManaBar = function() {
    var player = this.app.root.findByName('Player');
    if (!player || !player.script || !player.script.playerController) return;

    var stats = player.script.playerController.stats;
    var manaFill = document.getElementById('mana-fill');
    var manaText = document.getElementById('mana-text');

    if (manaFill && manaText) {
        var percent = (stats.mana / stats.maxMana) * 100;
        manaFill.style.width = percent + '%';
        manaText.textContent = stats.mana + '/' + stats.maxMana;
    }
};

UIController.prototype.updateCurrency = function() {
    var inventory = InventoryManager.getInstance();
    if (!inventory) return;

    var goldText = document.getElementById('gold-amount');
    var silverText = document.getElementById('silver-amount');
    var gemText = document.getElementById('gem-amount');

    if (goldText) goldText.textContent = this.formatNumber(inventory.currency.gold);
    if (silverText) silverText.textContent = this.formatNumber(inventory.currency.silver);
    if (gemText) gemText.textContent = this.formatNumber(inventory.currency.gems);
};

UIController.prototype.updateTime = function() {
    var timeManager = TimeManager.getInstance();
    if (!timeManager) return;

    var timeText = document.getElementById('time-text');
    var dateText = document.getElementById('date-text');

    if (timeText) {
        timeText.textContent = timeManager.getTimeString();
    }
    if (dateText) {
        dateText.textContent = timeManager.currentSeason + ' ' + timeManager.dayOfSeason;
    }
};

// ============================================================================
// Inventory
// ============================================================================

UIController.prototype.toggleInventory = function() {
    if (this.currentPanel === this.inventoryPanel) {
        this.closeCurrentPanel();
    } else {
        this.openPanel(this.inventoryPanel);
        this.updateInventoryUI();
    }
};

UIController.prototype.updateInventoryUI = function() {
    var inventory = InventoryManager.getInstance();
    if (!inventory) return;

    var grid = document.getElementById('inventory-grid');
    if (!grid) return;

    // Clear grid
    grid.innerHTML = '';

    // Create slots
    for (var i = 0; i < inventory.maxSlots; i++) {
        var slot = document.createElement('div');
        slot.className = 'inventory-slot';
        slot.dataset.slot = i;

        // Find item in this slot
        var item = inventory.items.find(function(it) { return it.slot === i; });

        if (item) {
            var itemDef = inventory.itemDefinitions[item.itemId];
            if (itemDef) {
                // Add item icon (placeholder)
                var icon = document.createElement('div');
                icon.className = 'item-icon';
                icon.textContent = itemDef.name.charAt(0); // Placeholder
                slot.appendChild(icon);

                // Add quantity if stackable
                if (itemDef.stackable && item.quantity > 1) {
                    var quantity = document.createElement('div');
                    quantity.className = 'item-quantity';
                    quantity.textContent = item.quantity;
                    slot.appendChild(quantity);
                }

                // Add rarity class
                if (itemDef.rarity) {
                    slot.classList.add('rarity-' + itemDef.rarity);
                }

                // Click handler
                slot.addEventListener('click', function() {
                    this.onItemClicked(item, itemDef);
                }.bind(this));
            }
        }

        grid.appendChild(slot);
    }

    // Update footer
    var slotsText = document.getElementById('inventory-slots');
    if (slotsText) {
        slotsText.textContent = inventory.items.length;
    }

    // Update currency
    this.updateCurrency();
};

UIController.prototype.onItemClicked = function(item, itemDef) {
    console.log('[UIController] Item clicked:', itemDef.name);
    // Show item tooltip or use item
};

// ============================================================================
// Quests
// ============================================================================

UIController.prototype.toggleQuests = function() {
    if (this.currentPanel === this.questPanel) {
        this.closeCurrentPanel();
    } else {
        this.openPanel(this.questPanel);
        this.updateQuestUI();
    }
};

UIController.prototype.updateQuestUI = function() {
    var questManager = QuestManager.getInstance();
    if (!questManager) return;

    var questList = document.getElementById('active-quests');
    if (!questList) return;

    // Clear list
    questList.innerHTML = '';

    // Add active quests
    questManager.activeQuests.forEach(function(quest) {
        var questItem = document.createElement('div');
        questItem.className = 'quest-item';

        var title = document.createElement('div');
        title.className = 'quest-title';
        title.textContent = quest.questName;

        var objectives = document.createElement('div');
        objectives.className = 'quest-objectives';

        quest.objectives.forEach(function(obj) {
            var objDiv = document.createElement('div');
            objDiv.className = 'objective' + (obj.isCompleted ? ' completed' : '');
            objDiv.textContent = obj.description + ' (' + obj.currentCount + '/' + obj.targetCount + ')';
            objectives.appendChild(objDiv);
        });

        questItem.appendChild(title);
        questItem.appendChild(objectives);
        questList.appendChild(questItem);
    });
};

UIController.prototype.onQuestStarted = function(quest) {
    this.showNotification('New Quest: ' + quest.questName, 'success');
    this.updateQuestUI();
};

UIController.prototype.onQuestCompleted = function(quest) {
    this.showNotification('Quest Completed: ' + quest.questName, 'success');
    this.updateQuestUI();
};

// ============================================================================
// Dialogue
// ============================================================================

UIController.prototype.onDialogueStarted = function(npcName, node) {
    this.dialogueBox.classList.remove('hidden');

    var nameText = document.getElementById('dialogue-npc-name');
    var dialogueText = document.getElementById('dialogue-text');

    if (nameText) nameText.textContent = npcName;
    if (dialogueText) dialogueText.textContent = node.text;

    this.updateDialogueChoices(node);
};

UIController.prototype.updateDialogueChoices = function(node) {
    var choicesDiv = document.getElementById('dialogue-choices');
    if (!choicesDiv) return;

    choicesDiv.innerHTML = '';

    if (node.choices && node.choices.length > 0) {
        node.choices.forEach(function(choice, index) {
            var choiceBtn = document.createElement('button');
            choiceBtn.className = 'dialogue-choice';
            choiceBtn.textContent = choice.text;
            choiceBtn.addEventListener('click', function() {
                this.onDialogueChoiceSelected(index);
            }.bind(this));
            choicesDiv.appendChild(choiceBtn);
        }.bind(this));
    }
};

UIController.prototype.onDialogueChoiceSelected = function(choiceIndex) {
    var dialogueManager = DialogueManager.getInstance();
    if (dialogueManager) {
        dialogueManager.selectChoice(choiceIndex);
    }
};

UIController.prototype.onDialogueEnded = function() {
    this.dialogueBox.classList.add('hidden');
};

// ============================================================================
// Farming
// ============================================================================

UIController.prototype.onFarmingHarvested = function(data) {
    this.showNotification('Harvested ' + data.yield + 'x items!', 'success');
};

// ============================================================================
// Notifications
// ============================================================================

UIController.prototype.showNotification = function(message, type) {
    type = type || 'info';

    var notification = document.createElement('div');
    notification.className = 'notification ' + type;
    notification.textContent = message;

    this.notificationContainer.appendChild(notification);

    // Auto-remove after 3 seconds
    setTimeout(function() {
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(100%)';
        setTimeout(function() {
            this.notificationContainer.removeChild(notification);
        }.bind(this), 300);
    }.bind(this), 3000);
};

// ============================================================================
// Panel Management
// ============================================================================

UIController.prototype.openPanel = function(panel) {
    // Close current panel
    this.closeCurrentPanel();

    // Open new panel
    if (panel) {
        panel.classList.remove('hidden');
        this.currentPanel = panel;
    }
};

UIController.prototype.closeCurrentPanel = function() {
    if (this.currentPanel) {
        this.currentPanel.classList.add('hidden');
        this.currentPanel = null;
    }
};

// ============================================================================
// Pause Menu
// ============================================================================

UIController.prototype.showPauseMenu = function() {
    this.pauseMenu.classList.remove('hidden');
    this.isPaused = true;
};

UIController.prototype.hidePauseMenu = function() {
    this.pauseMenu.classList.add('hidden');
    this.isPaused = false;
};

UIController.prototype.resumeGame = function() {
    this.hidePauseMenu();
    var gameManager = GameManager.getInstance();
    if (gameManager) {
        gameManager.setGameState('playing');
    }
};

// ============================================================================
// Keyboard Input
// ============================================================================

UIController.prototype.onKeyDown = function(event) {
    // ESC - Pause/Resume
    if (event.key === 'Escape') {
        if (this.currentPanel) {
            this.closeCurrentPanel();
        } else if (this.isPaused) {
            this.resumeGame();
        } else {
            var gameManager = GameManager.getInstance();
            if (gameManager) {
                gameManager.setGameState('paused');
            }
        }
    }

    // I - Inventory
    if (event.key === 'i' || event.key === 'I') {
        this.toggleInventory();
    }

    // Q - Quests
    if (event.key === 'q' || event.key === 'Q') {
        this.toggleQuests();
    }
};

// ============================================================================
// Mobile Detection
// ============================================================================

UIController.prototype.detectMobile = function() {
    var isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent);

    if (isMobile || window.innerWidth < 768) {
        this.mobileControls.classList.remove('hidden');
        this.setupMobileControls();
    }
};

UIController.prototype.setupMobileControls = function() {
    // Mobile joystick and button handlers would go here
    console.log('[UIController] Mobile controls enabled');
};

// ============================================================================
// Utilities
// ============================================================================

UIController.prototype.formatNumber = function(num) {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
};

UIController.prototype.update = function(dt) {
    // Update HUD every frame
    if (!this.gameHud.classList.contains('hidden')) {
        this.updateTime();

        // Update stats every 0.5s to reduce overhead
        if (!this.lastStatsUpdate || Date.now() - this.lastStatsUpdate > 500) {
            this.updateHealthBar();
            this.updateManaBar();
            this.lastStatsUpdate = Date.now();
        }
    }
};

// ============================================================================
// VALIDATION
// ============================================================================

console.log('=== UI Controller Loaded ===');
console.log('- HTML/CSS UI integration');
console.log('- Inventory, Quest, Dialogue UI');
console.log('- Notifications, Panels, HUD');
console.log('- Mobile-responsive');
console.log('===========================');
