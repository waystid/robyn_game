# Playground Porter - Quick Reference

Fast reference for using the Playground Porter agent.

---

## 🎯 Agent Commands

### Convert Unity Asset
```
Convert [AssetName] from Unity Asset Store to PlayCanvas playground
```

### Convert Unreal Asset
```
Convert [AssetName] from Unreal Marketplace to PlayCanvas playground
```

### Convert Local Files
```
I have a Unity/Unreal package at [path]. Convert it to a playground.
```

---

## 📋 Conversion Workflow

| Phase | What Happens | Output |
|-------|--------------|--------|
| 1. Analysis | Examines source files, identifies systems | Analysis report |
| 2. Conversion | Translates code to JavaScript | .js files |
| 3. Packaging | Creates playground structure | Complete package |
| 4. Validation | Tests and verifies | Ready playground |

---

## 🔄 Common Conversions

### Unity → PlayCanvas

```javascript
// Unity C#
public class MyScript : MonoBehaviour {
    void Start() { }
    void Update() { }
}

// PlayCanvas JavaScript
var MyScript = pc.createScript('myScript');
MyScript.prototype.initialize = function() { };
MyScript.prototype.update = function(dt) { };
```

### ScriptableObject → JSON

```csharp
// Unity
[CreateAssetMenu]
public class Item : ScriptableObject {
    public string itemName;
    public int value;
}

// JSON
{
  "itemID": "sword",
  "itemName": "Iron Sword",
  "value": 100
}
```

### UnityEvent → EventBus

```csharp
// Unity
public UnityEvent<int> OnHealthChanged;
OnHealthChanged.Invoke(health);

// PlayCanvas
var eventBus = EventBus.getInstance();
eventBus.fire('health:changed', health);
```

---

## 📦 Playground Structure

```
PlaygroundName/
├── scripts/          # JavaScript files
├── data/             # JSON files
├── ui/               # HTML/CSS
├── examples/         # Demo scenes
├── docs/             # Documentation
└── PLAYGROUND.json   # Metadata
```

---

## ✅ Quality Checklist

- [ ] All JavaScript syntax valid
- [ ] No Unity/Unreal APIs remaining
- [ ] PlayCanvas API correct
- [ ] Event system integrated
- [ ] Save/load implemented
- [ ] Documentation complete
- [ ] Examples provided
- [ ] Package validated

---

## 🎯 Use Cases

| Scenario | Solution | Time |
|----------|----------|------|
| Simple script (1-3 files) | Direct conversion | 15-30 min |
| Medium system (4-10 files) | System conversion | 1-2 hours |
| Complex system (11-20 files) | Multi-file conversion | 2-4 hours |
| Full template (50+ files) | Multiple playgrounds | 2-5 days |

---

## 🔧 API Mappings

### Unity Lifecycle
- `Awake()` → `initialize()`
- `Start()` → `postInitialize()`
- `Update()` → `update(dt)`
- `OnDestroy()` → `destroy()`

### Unity Components
- `GameObject` → `pc.Entity`
- `Transform` → `entity.setPosition/Rotation()`
- `Rigidbody` → `entity.rigidbody`
- `Collider` → `entity.collision`

### Unreal Lifecycle
- `BeginPlay()` → `initialize()`
- `Tick()` → `update(dt)`
- `EndPlay()` → `destroy()`

### Unreal Types
- `FVector` → `pc.Vec3`
- `FRotator` → `pc.Quat`
- `TArray<T>` → `Array`
- `AActor` → `pc.Entity + script`

---

## 📚 Documentation Files

Every playground includes:

1. **README.md** - Overview, quick start
2. **API.md** - Complete API reference
3. **GUIDE.md** - Integration guide
4. **EXAMPLES.md** - Code examples
5. **CONVERSION.md** - Conversion notes

---

## 🚀 Quick Start Example

### Input
```
"Advanced Inventory System" (Unity Asset Store)
```

### Agent Process
```
1. ✅ Analyze (5 scripts identified)
2. 🔄 Convert scripts to JavaScript
3. 📦 Package playground
4. ✅ Validate

Output: InventorySystemPlayground-v1.0.0.zip
```

### Usage
```
1. Extract zip
2. Upload scripts to PlayCanvas
3. Follow GUIDE.md
4. Launch!
```

---

## ⚡ Fast Commands

| Want to... | Say... |
|-----------|--------|
| Convert asset | "Convert [name] to playground" |
| Analyze first | "Analyze [name] asset" |
| Get roadmap | "Show conversion plan for [name]" |
| Continue | "Proceed with conversion" |
| Package only | "Package the playground" |

---

## 🎨 Customization

### Change Playground Name
Edit `PLAYGROUND.json`:
```json
{
  "name": "My Custom Playground",
  "version": "1.0.0",
  ...
}
```

### Add Features
Edit feature list in `PLAYGROUND.json`:
```json
{
  "features": [
    "Your feature here"
  ]
}
```

### Update Docs
Edit markdown files in `docs/` folder.

---

## 🐛 Troubleshooting

### Conversion Failed
- Check source code validity
- Verify dependencies
- Review error message
- Try manual conversion

### Missing Features
- Check CONVERSION.md for limitations
- Some features may need manual implementation
- Report as feedback for agent improvement

### Integration Issues
- Follow GUIDE.md step-by-step
- Check PlayCanvas version compatibility
- Verify all dependencies loaded

---

## 📊 Success Metrics

### Good Conversion
- ✅ 100% scripts converted
- ✅ All features working
- ✅ Complete documentation
- ✅ Examples provided
- ✅ Tests passing

### Needs Work
- ⚠️ Some manual steps required
- ⚠️ Limited features
- ⚠️ Partial documentation

### Cannot Convert
- ❌ Compiled plugins
- ❌ Native code
- ❌ Editor tools
- ❌ Platform-specific code

---

## 🎯 Best Practices

1. **Start Simple** - Convert small assets first
2. **Read Docs** - Check documentation thoroughly
3. **Test Early** - Validate as you integrate
4. **Customize** - Adapt playground to your needs
5. **Share** - Contribute back to community

---

## 📈 Conversion Quality

### Excellent (95-100%)
- Simple game logic
- Data structures
- UI systems
- State machines

### Good (80-95%)
- Character controllers
- Inventory systems
- Quest systems
- Save/load systems

### Fair (60-80%)
- Physics systems
- AI systems
- Animation systems

### Manual (0-60%)
- Shaders
- Native plugins
- Editor tools
- Networking

---

## 🔗 Quick Links

- [Full Specification](AGENT_SPEC.md)
- [System Prompt](SYSTEM_PROMPT.md)
- [Example Conversion](EXAMPLE_CONVERSION.md)
- [Main README](README.md)

---

## 💡 Tips

- **Provide context**: More info = better conversion
- **Check dependencies**: List all third-party plugins
- **Review output**: Always validate conversions
- **Customize freely**: Playgrounds are templates
- **Share feedback**: Help improve the agent

---

## 🎮 Happy Converting!

**Quick Start**: Load `SYSTEM_PROMPT.md` and start converting!

**Need Help?**: Check full docs in this folder.

**Ready to Build?**: Your playground awaits! 🚀

---

*Playground Porter - Making PlayCanvas development faster, one conversion at a time.*
