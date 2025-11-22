using UnityEngine;

namespace CozyGame.Magic
{
    /// <summary>
    /// Defines a spell that can be cast by the player
    /// Create instances via: Right-click → Create → Cozy Game → Spell Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Spell", menuName = "Cozy Game/Magic/Spell Data", order = 10)]
    public class SpellData : ScriptableObject
    {
        [Header("Spell Identity")]
        [Tooltip("Display name of the spell")]
        public string spellName = "Magic Spell";

        [Tooltip("Unique identifier for this spell")]
        public string spellID;

        [Tooltip("Description shown to player")]
        [TextArea(2, 4)]
        public string description = "A magical spell...";

        [Header("Mana Cost")]
        [Tooltip("How much mana this spell costs to cast")]
        public float manaCost = 20f;

        [Header("Casting Properties")]
        [Tooltip("Maximum range this spell can be cast")]
        public float castRange = 10f;

        [Tooltip("Time to complete casting (0 = instant)")]
        public float castTime = 0f;

        [Tooltip("Cooldown before this specific spell can be cast again")]
        public float cooldown = 0f;

        [Header("Spell Type")]
        public SpellType spellType = SpellType.Targeted;

        [Tooltip("What this spell affects")]
        public SpellTarget targetType = SpellTarget.Environment;

        [Header("Visual & Audio")]
        [Tooltip("Particle effect prefab for this spell")]
        public GameObject spellEffectPrefab;

        [Tooltip("Sound to play when casting")]
        public string castSoundName = "spell_cast";

        [Tooltip("Color tint for spell effects")]
        public Color spellColor = Color.cyan;

        [Header("Spell Effects")]
        [Tooltip("What does this spell do?")]
        public SpellEffect primaryEffect = SpellEffect.Growth;

        [Tooltip("Power/magnitude of the effect")]
        public float effectPower = 1f;

        [Tooltip("Duration of spell effect (0 = instant)")]
        public float duration = 0f;

        [Tooltip("Radius of area of effect (0 = single target)")]
        public float aoeRadius = 0f;

        [Header("Requirements")]
        [Tooltip("Minimum magic level required to cast (0 = no requirement)")]
        public int requiredLevel = 0;

        [Tooltip("Can this spell be cast while moving?")]
        public bool canCastWhileMoving = true;

        [Header("State (Runtime)")]
        [HideInInspector]
        public float lastCastTime = 0f;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(spellID))
            {
                spellID = "spell_" + name.ToLower().Replace(" ", "_");
            }
        }

        /// <summary>
        /// Check if spell is off cooldown
        /// </summary>
        public bool IsOffCooldown()
        {
            if (cooldown == 0f) return true;
            return (Time.time - lastCastTime) >= cooldown;
        }

        /// <summary>
        /// Get remaining cooldown time
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (cooldown == 0f) return 0f;
            float remaining = cooldown - (Time.time - lastCastTime);
            return Mathf.Max(0f, remaining);
        }

        /// <summary>
        /// Mark spell as cast (starts cooldown)
        /// </summary>
        public void MarkAsCast()
        {
            lastCastTime = Time.time;
        }

        /// <summary>
        /// Reset cooldown
        /// </summary>
        public void ResetCooldown()
        {
            lastCastTime = 0f;
        }

        /// <summary>
        /// Get spell info for UI display
        /// </summary>
        public string GetSpellInfo()
        {
            string info = $"{spellName}\n";
            info += $"{description}\n\n";
            info += $"Mana Cost: {manaCost}\n";
            info += $"Range: {castRange}m\n";

            if (cooldown > 0f)
                info += $"Cooldown: {cooldown}s\n";

            if (castTime > 0f)
                info += $"Cast Time: {castTime}s\n";

            return info;
        }
    }

    /// <summary>
    /// Type of spell casting
    /// </summary>
    public enum SpellType
    {
        Instant,      // Cast immediately
        Targeted,     // Requires selecting a target
        Channeled,    // Must channel for cast time
        AoE           // Area of Effect spell
    }

    /// <summary>
    /// What the spell can target
    /// </summary>
    public enum SpellTarget
    {
        Self,         // Only affects caster
        Environment,  // Targets world objects (plants, resources)
        Creature,     // Targets creatures/NPCs
        Player,       // Targets other players (if multiplayer)
        Ground        // Targets ground position
    }

    /// <summary>
    /// Primary effect of the spell
    /// </summary>
    public enum SpellEffect
    {
        Growth,       // Makes plants grow faster
        Harvest,      // Auto-harvests nearby plants
        Light,        // Creates light source
        Teleport,     // Teleport to location
        Summon,       // Summon creatures/items
        Transform,    // Transform objects
        Heal,         // Restore health/attributes
        Buff,         // Temporary stat increase
        Utility       // Misc utility effect
    }
}
