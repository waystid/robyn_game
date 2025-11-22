# Example Conversion - Unity Inventory System

This document shows a complete example of how Playground Porter converts a Unity asset.

---

## Source Asset

**Name**: Simple Inventory System
**Platform**: Unity Asset Store
**Version**: 1.5.0
**Scripts**: 4 C# files
**Complexity**: Low-Medium

---

## Phase 1: Analysis

### Input Files

```
SimpleInventory/
├── Scripts/
│   ├── InventoryManager.cs        (180 lines)
│   ├── InventorySlot.cs           (65 lines)
│   ├── Item.cs                    (45 lines)
│   └── InventoryUI.cs             (120 lines)
├── ScriptableObjects/
│   └── Items/
│       ├── HealthPotion.asset
│       ├── Sword.asset
│       └── Shield.asset
└── Prefabs/
    └── InventoryPanel.prefab
```

### Agent Analysis Output

```markdown
## 📊 Asset Analysis Complete

**Source**: Unity Asset Store - "Simple Inventory System v1.5.0"
**Type**: Game System - Inventory Management
**Complexity**: Medium
**Estimated Conversion**: 1-2 hours

### Components Identified

**Scripts (4 files)**:
- ✅ InventoryManager.cs - Main inventory logic
- ✅ InventorySlot.cs - Individual slot management
- ✅ Item.cs - Base item class
- ✅ InventoryUI.cs - UI controller

**Data Assets (3 ScriptableObjects)**:
- ✅ Item definitions (HealthPotion, Sword, Shield)

**UI Elements (1 Prefab)**:
- ✅ InventoryPanel.prefab - UI layout

**Dependencies**:
- Unity UI system
- UnityEvents
- No third-party plugins ✅

### Conversion Plan

**Step 1**: Convert InventoryManager.cs → inventory-manager.js
- Singleton pattern
- Add/remove items
- Event system integration

**Step 2**: Convert Item.cs → items.json schema
- ScriptableObject → JSON
- Create item database loader

**Step 3**: Convert InventorySlot.cs → inventory-slot.js
- Slot UI logic
- Drag & drop (HTML5)

**Step 4**: Convert InventoryUI.cs → inventory-ui-controller.js
- UI management
- Event listeners

**Step 5**: Convert UI → HTML/CSS
- InventoryPanel.prefab → HTML structure
- Unity UI → CSS styling

**Step 6**: Package Playground
- Create documentation
- Add examples
- Validate

### Feasibility Assessment

✅ **Can Fully Convert**:
- All game logic (C# → JavaScript)
- Item data (ScriptableObject → JSON)
- UI layout (Prefab → HTML)
- Events (UnityEvent → EventBus)

⚠️ **Needs Adjustment**:
- Drag & drop (Unity UI → HTML5 drag API)
- Sprite rendering (Unity → CSS/images)

❌ **Cannot Convert**:
- None! This asset is fully convertible.

**Recommendation**: Proceed with conversion. Expected result: 100% functional playground.

Proceed? (yes/no)
```

---

## Phase 2: Conversion

### File 1: InventoryManager.cs → inventory-manager.js

#### Original Unity C#
```csharp
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;
    public static InventoryManager Instance => instance;

    [SerializeField] private int maxSlots = 20;
    [SerializeField] private List<Item> items = new List<Item>();

    public UnityEvent<Item> OnItemAdded;
    public UnityEvent<Item> OnItemRemoved;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AddItem(Item item)
    {
        if (items.Count >= maxSlots)
        {
            Debug.LogWarning("Inventory full!");
            return false;
        }

        items.Add(item);
        OnItemAdded?.Invoke(item);
        return true;
    }

    public bool RemoveItem(Item item)
    {
        if (items.Remove(item))
        {
            OnItemRemoved?.Invoke(item);
            return true;
        }
        return false;
    }

    public Item GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
            return items[index];
        return null;
    }

    public int GetItemCount()
    {
        return items.Count;
    }
}
```

#### Converted PlayCanvas JavaScript
```javascript
// ============================================================================
// Inventory Manager - Converted from Unity C#
// Original: InventoryManager.cs from Simple Inventory System v1.5.0
// ============================================================================

var InventoryManager = pc.createScript('inventoryManager');

// Singleton accessor
InventoryManager.getInstance = function() {
    return InventoryManager._instance;
};

// Attributes (exposed in PlayCanvas Editor)
InventoryManager.attributes.add('maxSlots', {
    type: 'number',
    default: 20,
    description: 'Maximum number of inventory slots'
});

// Initialize
InventoryManager.prototype.initialize = function() {
    // Set singleton instance
    InventoryManager._instance = this;

    // Initialize items array
    this.items = [];

    // Get EventBus for event system
    this.eventBus = EventBus.getInstance();

    console.log('[InventoryManager] Initialized with ' + this.maxSlots + ' slots');
};

/**
 * Add item to inventory
 * @param {Object} item - Item data object
 * @returns {boolean} True if added successfully
 */
InventoryManager.prototype.addItem = function(item) {
    // Check if inventory is full
    if (this.items.length >= this.maxSlots) {
        console.warn('[InventoryManager] Inventory full!');
        return false;
    }

    // Add item to array
    this.items.push(item);

    // Fire event (replaces UnityEvent)
    if (this.eventBus) {
        this.eventBus.fire('inventory:itemAdded', item);
    }

    console.log('[InventoryManager] Added item:', item.itemName);
    return true;
};

/**
 * Remove item from inventory
 * @param {Object} item - Item to remove
 * @returns {boolean} True if removed successfully
 */
InventoryManager.prototype.removeItem = function(item) {
    var index = this.items.indexOf(item);

    if (index !== -1) {
        this.items.splice(index, 1);

        // Fire event
        if (this.eventBus) {
            this.eventBus.fire('inventory:itemRemoved', item);
        }

        console.log('[InventoryManager] Removed item:', item.itemName);
        return true;
    }

    return false;
};

/**
 * Get item at specific index
 * @param {number} index - Slot index
 * @returns {Object|null} Item or null if invalid index
 */
InventoryManager.prototype.getItem = function(index) {
    if (index >= 0 && index < this.items.length) {
        return this.items[index];
    }
    return null;
};

/**
 * Get total number of items
 * @returns {number} Item count
 */
InventoryManager.prototype.getItemCount = function() {
    return this.items.length;
};

/**
 * Get all items
 * @returns {Array} Array of all items
 */
InventoryManager.prototype.getAllItems = function() {
    return this.items;
};

/**
 * Check if inventory is full
 * @returns {boolean} True if full
 */
InventoryManager.prototype.isFull = function() {
    return this.items.length >= this.maxSlots;
};

/**
 * Clear all items
 */
InventoryManager.prototype.clearInventory = function() {
    this.items = [];

    if (this.eventBus) {
        this.eventBus.fire('inventory:cleared');
    }

    console.log('[InventoryManager] Inventory cleared');
};

/**
 * Save data for persistence
 * @returns {Object} Save data
 */
InventoryManager.prototype.getSaveData = function() {
    return {
        items: this.items,
        maxSlots: this.maxSlots
    };
};

/**
 * Load saved data
 * @param {Object} data - Saved data
 */
InventoryManager.prototype.loadSaveData = function(data) {
    if (data.items) {
        this.items = data.items;
    }
    if (data.maxSlots) {
        this.maxSlots = data.maxSlots;
    }

    console.log('[InventoryManager] Loaded ' + this.items.length + ' items');
};
```

### File 2: Item ScriptableObjects → items.json

#### Original Unity ScriptableObject
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStack = 1;
    public float weight = 1f;
    public int sellValue = 0;
}
```

#### Converted JSON Schema
```json
[
  {
    "itemID": "health_potion",
    "itemName": "Health Potion",
    "description": "Restores 50 HP",
    "iconAsset": "assets/icons/health_potion.png",
    "maxStack": 10,
    "weight": 0.5,
    "sellValue": 25,
    "itemType": "consumable",
    "effects": {
      "health": 50
    }
  },
  {
    "itemID": "iron_sword",
    "itemName": "Iron Sword",
    "description": "A sturdy iron blade",
    "iconAsset": "assets/icons/iron_sword.png",
    "maxStack": 1,
    "weight": 5.0,
    "sellValue": 100,
    "itemType": "weapon",
    "stats": {
      "damage": 15,
      "durability": 100
    }
  },
  {
    "itemID": "wooden_shield",
    "itemName": "Wooden Shield",
    "description": "Basic wooden shield",
    "iconAsset": "assets/icons/wooden_shield.png",
    "maxStack": 1,
    "weight": 3.0,
    "sellValue": 50,
    "itemType": "armor",
    "stats": {
      "defense": 10,
      "durability": 80
    }
  }
]
```

### File 3: InventoryUI.cs → inventory-ui-controller.js

#### Original Unity C#
```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsContainer;

    private List<GameObject> slotObjects = new List<GameObject>();

    private void Start()
    {
        CreateSlots();
        InventoryManager.Instance.OnItemAdded.AddListener(OnItemAdded);
        InventoryManager.Instance.OnItemRemoved.AddListener(OnItemRemoved);
    }

    private void CreateSlots()
    {
        int maxSlots = 20; // Could get from InventoryManager

        for (int i = 0; i < maxSlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotsContainer);
            slotObjects.Add(slot);
        }
    }

    private void OnItemAdded(Item item)
    {
        RefreshUI();
    }

    private void OnItemRemoved(Item item)
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        var items = InventoryManager.Instance.GetAllItems();

        for (int i = 0; i < slotObjects.Count; i++)
        {
            if (i < items.Count)
            {
                // Update slot with item data
                UpdateSlot(slotObjects[i], items[i]);
            }
            else
            {
                // Clear slot
                ClearSlot(slotObjects[i]);
            }
        }
    }
}
```

#### Converted PlayCanvas + HTML/CSS

**JavaScript (inventory-ui-controller.js)**:
```javascript
var InventoryUIController = pc.createScript('inventoryUIController');

InventoryUIController.prototype.initialize = function() {
    // Get inventory manager
    this.inventoryManager = InventoryManager.getInstance();

    // Get event bus
    this.eventBus = EventBus.getInstance();

    // Create slots
    this.createSlots();

    // Listen to events (replaces UnityEvent listeners)
    this.eventBus.on('inventory:itemAdded', this.onItemAdded, this);
    this.eventBus.on('inventory:itemRemoved', this.onItemRemoved, this);

    console.log('[InventoryUIController] Initialized');
};

InventoryUIController.prototype.createSlots = function() {
    var container = document.getElementById('inventory-slots');
    if (!container) return;

    var maxSlots = this.inventoryManager.maxSlots;

    for (var i = 0; i < maxSlots; i++) {
        var slot = document.createElement('div');
        slot.className = 'inventory-slot';
        slot.dataset.index = i;

        // Add click handler
        slot.addEventListener('click', this.onSlotClicked.bind(this, i));

        container.appendChild(slot);
    }
};

InventoryUIController.prototype.onItemAdded = function(item) {
    this.refreshUI();
};

InventoryUIController.prototype.onItemRemoved = function(item) {
    this.refreshUI();
};

InventoryUIController.prototype.refreshUI = function() {
    var items = this.inventoryManager.getAllItems();
    var slots = document.querySelectorAll('.inventory-slot');

    slots.forEach(function(slot, index) {
        if (index < items.length) {
            this.updateSlot(slot, items[index]);
        } else {
            this.clearSlot(slot);
        }
    }.bind(this));
};

InventoryUIController.prototype.updateSlot = function(slotElement, item) {
    slotElement.innerHTML = '';
    slotElement.classList.add('has-item');

    // Create icon
    var icon = document.createElement('div');
    icon.className = 'item-icon';
    icon.textContent = item.itemName.charAt(0); // Or use actual icon
    slotElement.appendChild(icon);

    // Create name label
    var name = document.createElement('div');
    name.className = 'item-name';
    name.textContent = item.itemName;
    slotElement.appendChild(name);
};

InventoryUIController.prototype.clearSlot = function(slotElement) {
    slotElement.innerHTML = '';
    slotElement.classList.remove('has-item');
};

InventoryUIController.prototype.onSlotClicked = function(index) {
    var item = this.inventoryManager.getItem(index);
    if (item) {
        console.log('Clicked item:', item.itemName);
        // Show item details, use item, etc.
    }
};
```

**HTML Structure**:
```html
<!-- Inventory Panel UI -->
<div id="inventory-panel" class="panel hidden">
    <div class="panel-header">
        <h2>Inventory</h2>
        <button class="btn-close">×</button>
    </div>
    <div class="panel-content">
        <div id="inventory-slots" class="inventory-grid">
            <!-- Slots created dynamically by JavaScript -->
        </div>
    </div>
</div>
```

**CSS Styling**:
```css
/* Inventory Panel */
#inventory-panel {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 500px;
    background: rgba(0, 0, 0, 0.9);
    border: 2px solid #4A90E2;
    border-radius: 8px;
    padding: 20px;
}

/* Inventory Grid */
.inventory-grid {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 10px;
    margin-top: 20px;
}

/* Inventory Slot */
.inventory-slot {
    width: 80px;
    height: 80px;
    background: rgba(255, 255, 255, 0.1);
    border: 2px solid #555;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}

.inventory-slot:hover {
    background: rgba(255, 255, 255, 0.2);
    border-color: #4A90E2;
}

.inventory-slot.has-item {
    border-color: #4A90E2;
}

/* Item Icon */
.item-icon {
    width: 50px;
    height: 50px;
    background: #4A90E2;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;
    font-weight: bold;
    color: white;
}

/* Item Name */
.item-name {
    font-size: 10px;
    color: white;
    margin-top: 5px;
    text-align: center;
}
```

---

## Phase 3: Playground Package

### Final Structure

```
InventorySystemPlayground/
├── scripts/
│   ├── managers/
│   │   └── inventory-manager.js       (Converted from InventoryManager.cs)
│   ├── components/
│   │   └── inventory-slot.js          (Converted from InventorySlot.cs)
│   └── ui/
│       └── inventory-ui-controller.js (Converted from InventoryUI.cs)
├── data/
│   ├── items.json                     (Converted from ScriptableObjects)
│   └── config.json
├── ui/
│   ├── inventory.html
│   └── inventory.css
├── examples/
│   └── demo-scene.md
├── docs/
│   ├── README.md
│   ├── API.md
│   ├── GUIDE.md
│   ├── EXAMPLES.md
│   └── CONVERSION.md
├── tests/
│   └── validation.md
└── PLAYGROUND.json
```

### PLAYGROUND.json

```json
{
  "name": "Inventory System Playground",
  "version": "1.0.0",
  "author": "Playground Porter Agent",
  "forge": "waystid-game-forge",
  "category": "game-systems",
  "tags": ["inventory", "items", "ui", "storage"],
  "description": "Complete inventory system with slot management and UI",
  "sourceEngine": "Unity",
  "sourceAsset": "Simple Inventory System",
  "sourceVersion": "1.5.0",
  "sourceURL": "https://assetstore.unity.com/packages/...",
  "conversionDate": "2024-11-22",
  "playcanvasVersion": "1.50+",
  "dependencies": [
    "EventBus",
    "SaveSystem"
  ],
  "features": [
    "20 inventory slots",
    "Add/remove items",
    "Item data system",
    "HTML/CSS UI",
    "Event system",
    "Save/load support"
  ],
  "compatibility": {
    "mobile": true,
    "desktop": true,
    "webgl2": true
  },
  "files": {
    "scripts": 3,
    "data": 2,
    "ui": 2,
    "docs": 5
  },
  "codeStats": {
    "totalLines": 450,
    "javascript": 350,
    "html": 50,
    "css": 50
  }
}
```

---

## Phase 4: Validation

### Validation Checklist

✅ **Code Quality**
- [x] All JavaScript syntax valid
- [x] No Unity API calls remaining
- [x] PlayCanvas API used correctly
- [x] Event system integrated
- [x] Save/load implemented
- [x] Singleton pattern correct

✅ **Functionality**
- [x] Can add items
- [x] Can remove items
- [x] UI updates correctly
- [x] Events fire properly
- [x] Save/load works

✅ **Documentation**
- [x] README.md complete
- [x] API.md with all methods
- [x] GUIDE.md with setup steps
- [x] EXAMPLES.md with code
- [x] CONVERSION.md with notes

✅ **Package**
- [x] All files present
- [x] Structure correct
- [x] PLAYGROUND.json valid
- [x] Ready for distribution

---

## Completion Report

```markdown
## ✅ Playground Complete: Inventory System

**Package**: InventorySystemPlayground-v1.0.0.zip
**Size**: 28 KB
**Files**: 15 files
**Conversion Time**: 1.5 hours

### Conversion Summary

**Converted Successfully**:
- ✅ InventoryManager.cs → inventory-manager.js (100%)
- ✅ InventorySlot.cs → inventory-slot.js (100%)
- ✅ Item.cs → items.json + schema (100%)
- ✅ InventoryUI.cs → inventory-ui-controller.js (100%)
- ✅ Unity UI → HTML/CSS (100%)
- ✅ UnityEvents → EventBus (100%)

**Features**:
- 20 inventory slots
- Add/remove items
- Item database (JSON)
- Full UI with grid layout
- Event system integration
- Save/load support

**API Preserved**:
- AddItem() → addItem()
- RemoveItem() → removeItem()
- GetItem() → getItem()
- All original functionality maintained

**Improvements**:
- Added save/load system
- Added EventBus integration
- Responsive HTML/CSS UI
- Better documentation

### Quick Start

1. Upload scripts to PlayCanvas
2. Create InventoryManager entity
3. Add inventory-manager script
4. Replace index.html with UI
5. Add CSS styling
6. Launch!

### Ready for Waystid Game Forge! 🎉
```

---

This example shows the complete conversion process from Unity asset to PlayCanvas playground.
