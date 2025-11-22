using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Content
{
    /// <summary>
    /// Additional spell content for the game.
    /// Contains 15 diverse spells across different schools of magic.
    /// </summary>
    public static class AdditionalSpells
    {
        /// <summary>
        /// Get all additional spells
        /// </summary>
        public static List<SpellDefinition> GetAllSpells()
        {
            return new List<SpellDefinition>
            {
                // Fire Spells
                new SpellDefinition
                {
                    spellID = "spell_inferno_blast",
                    spellName = "Inferno Blast",
                    description = "Unleash a devastating explosion of flames that damages all enemies in a large area.",
                    school = MagicSchool.Fire,
                    tier = SpellTier.Advanced,

                    manaCost = 80,
                    castTime = 2.0f,
                    cooldown = 15f,
                    range = 15f,
                    areaOfEffect = 8f,

                    damage = 150,
                    damageType = DamageType.Fire,
                    hasDOT = true,
                    dotDamage = 20,
                    dotDuration = 5f,

                    vfxEffect = "inferno_explosion",
                    soundEffect = "fire_blast",
                    requiredLevel = 15,
                    learningCost = 500
                },

                new SpellDefinition
                {
                    spellID = "spell_phoenix_rebirth",
                    spellName = "Phoenix Rebirth",
                    description = "Channel the power of the phoenix. Upon death, revive with 50% health and deal fire damage around you.",
                    school = MagicSchool.Fire,
                    tier = SpellTier.Master,

                    manaCost = 100,
                    castTime = 0f,
                    cooldown = 300f, // 5 minute cooldown
                    range = 0f,
                    areaOfEffect = 10f,

                    isPassive = true,
                    damage = 200,
                    damageType = DamageType.Fire,
                    healAmount = 0, // Heals 50% on trigger
                    specialEffect = "Revive on death with 50% HP",

                    vfxEffect = "phoenix_flames",
                    soundEffect = "phoenix_cry",
                    requiredLevel = 25,
                    learningCost = 2000
                },

                // Ice Spells
                new SpellDefinition
                {
                    spellID = "spell_frozen_domain",
                    spellName = "Frozen Domain",
                    description = "Create a zone of absolute zero that freezes all enemies and slows their movement dramatically.",
                    school = MagicSchool.Ice,
                    tier = SpellTier.Advanced,

                    manaCost = 70,
                    castTime = 1.5f,
                    cooldown = 20f,
                    range = 12f,
                    areaOfEffect = 10f,
                    duration = 8f,

                    damage = 80,
                    damageType = DamageType.Ice,
                    hasCrowdControl = true,
                    crowdControlType = CrowdControl.Slow,
                    crowdControlChance = 100f,
                    slowAmount = 70f,

                    vfxEffect = "ice_field",
                    soundEffect = "freeze_zone",
                    requiredLevel = 12,
                    learningCost = 400
                },

                new SpellDefinition
                {
                    spellID = "spell_glacial_spike",
                    spellName = "Glacial Spike",
                    description = "Launch a massive spike of ice that pierces through enemies, dealing damage and freezing them solid.",
                    school = MagicSchool.Ice,
                    tier = SpellTier.Expert,

                    manaCost = 60,
                    castTime = 1.2f,
                    cooldown = 10f,
                    range = 20f,

                    damage = 120,
                    damageType = DamageType.Ice,
                    hasCrowdControl = true,
                    crowdControlType = CrowdControl.Freeze,
                    crowdControlChance = 50f,
                    freezeDuration = 3f,
                    piercing = true,

                    vfxEffect = "ice_spike",
                    soundEffect = "ice_shatter",
                    requiredLevel = 18,
                    learningCost = 800
                },

                // Lightning Spells
                new SpellDefinition
                {
                    spellID = "spell_chain_lightning",
                    spellName = "Chain Lightning",
                    description = "Strike an enemy with lightning that chains to nearby foes, hitting up to 5 targets.",
                    school = MagicSchool.Lightning,
                    tier = SpellTier.Advanced,

                    manaCost = 65,
                    castTime = 0.8f,
                    cooldown = 8f,
                    range = 15f,

                    damage = 90,
                    damageType = DamageType.Lightning,
                    chainTargets = 5,
                    chainRange = 8f,
                    chainDamageReduction = 20f, // Each chain does 20% less

                    vfxEffect = "chain_lightning",
                    soundEffect = "thunder_crack",
                    requiredLevel = 14,
                    learningCost = 600
                },

                new SpellDefinition
                {
                    spellID = "spell_storm_call",
                    spellName = "Storm Call",
                    description = "Summon a lightning storm that repeatedly strikes random enemies in the area.",
                    school = MagicSchool.Lightning,
                    tier = SpellTier.Master,

                    manaCost = 100,
                    castTime = 2.5f,
                    cooldown = 30f,
                    range = 20f,
                    areaOfEffect = 15f,
                    duration = 10f,

                    damage = 60, // Per strike
                    damageType = DamageType.Lightning,
                    ticksPerSecond = 2, // 20 total strikes
                    randomTargeting = true,

                    vfxEffect = "lightning_storm",
                    soundEffect = "thunder_storm",
                    requiredLevel = 22,
                    learningCost = 1500
                },

                // Nature Spells
                new SpellDefinition
                {
                    spellID = "spell_thorny_vine",
                    spellName = "Thorny Vines",
                    description = "Summon vines that entangle enemies, dealing damage and rooting them in place.",
                    school = MagicSchool.Nature,
                    tier = SpellTier.Intermediate,

                    manaCost = 45,
                    castTime = 1.0f,
                    cooldown = 12f,
                    range = 10f,
                    areaOfEffect = 5f,
                    duration = 5f,

                    damage = 60,
                    damageType = DamageType.Nature,
                    hasDOT = true,
                    dotDamage = 15,
                    dotDuration = 5f,
                    hasCrowdControl = true,
                    crowdControlType = CrowdControl.Root,
                    crowdControlChance = 80f,

                    vfxEffect = "vine_entangle",
                    soundEffect = "vines_grow",
                    requiredLevel = 10,
                    learningCost = 300
                },

                new SpellDefinition
                {
                    spellID = "spell_natures_blessing",
                    spellName = "Nature's Blessing",
                    description = "Channel the power of nature to heal yourself and allies over time while boosting regeneration.",
                    school = MagicSchool.Nature,
                    tier = SpellTier.Advanced,

                    manaCost = 70,
                    castTime = 1.5f,
                    cooldown = 25f,
                    range = 15f,
                    areaOfEffect = 10f,
                    duration = 10f,

                    healAmount = 150, // Total over duration
                    hasHOT = true,
                    hotAmount = 15, // Per tick
                    ticksPerSecond = 1,
                    buffType = BuffType.Regeneration,
                    buffDuration = 10f,

                    vfxEffect = "nature_aura",
                    soundEffect = "nature_heal",
                    requiredLevel = 16,
                    learningCost = 700
                },

                // Light Spells
                new SpellDefinition
                {
                    spellID = "spell_holy_smite",
                    spellName = "Holy Smite",
                    description = "Call down a beam of holy light that smites enemies and heals allies in its path.",
                    school = MagicSchool.Light,
                    tier = SpellTier.Advanced,

                    manaCost = 75,
                    castTime = 1.2f,
                    cooldown = 15f,
                    range = 18f,
                    areaOfEffect = 4f,

                    damage = 130,
                    damageType = DamageType.Holy,
                    healAmount = 60,
                    damageUndead = true,
                    extraDamageVsUndead = 100f, // +100% vs undead

                    vfxEffect = "holy_beam",
                    soundEffect = "divine_strike",
                    requiredLevel = 17,
                    learningCost = 750
                },

                new SpellDefinition
                {
                    spellID = "spell_radiant_shield",
                    spellName = "Radiant Shield",
                    description = "Surround yourself with a shield of light that absorbs damage and reflects a portion back.",
                    school = MagicSchool.Light,
                    tier = SpellTier.Expert,

                    manaCost = 60,
                    castTime = 0.5f,
                    cooldown = 20f,
                    duration = 12f,

                    shieldAmount = 200,
                    reflectDamage = true,
                    reflectPercentage = 30f,
                    buffType = BuffType.Shield,

                    vfxEffect = "light_shield",
                    soundEffect = "shield_up",
                    requiredLevel = 19,
                    learningCost = 900
                },

                // Dark Spells
                new SpellDefinition
                {
                    spellID = "spell_shadow_bolt",
                    spellName = "Shadow Bolt",
                    description = "Launch a bolt of pure shadow energy that damages and weakens enemies.",
                    school = MagicSchool.Dark,
                    tier = SpellTier.Intermediate,

                    manaCost = 40,
                    castTime = 0.8f,
                    cooldown = 5f,
                    range = 15f,

                    damage = 70,
                    damageType = DamageType.Shadow,
                    debuffType = DebuffType.Weakness,
                    debuffDuration = 6f,
                    debuffAmount = 20f, // -20% damage

                    vfxEffect = "shadow_bolt",
                    soundEffect = "dark_whisper",
                    requiredLevel = 8,
                    learningCost = 250
                },

                new SpellDefinition
                {
                    spellID = "spell_life_drain",
                    spellName = "Life Drain",
                    description = "Drain the life force from an enemy, dealing damage and healing yourself for a portion.",
                    school = MagicSchool.Dark,
                    tier = SpellTier.Advanced,

                    manaCost = 55,
                    castTime = 1.0f,
                    cooldown = 10f,
                    range = 12f,

                    damage = 100,
                    damageType = DamageType.Shadow,
                    healAmount = 50, // 50% of damage
                    lifesteal = true,
                    lifestealPercent = 50f,

                    vfxEffect = "life_drain",
                    soundEffect = "soul_siphon",
                    requiredLevel = 13,
                    learningCost = 550
                },

                // Arcane Spells
                new SpellDefinition
                {
                    spellID = "spell_arcane_missiles",
                    spellName = "Arcane Missiles",
                    description = "Fire a barrage of magical missiles that home in on enemies.",
                    school = MagicSchool.Arcane,
                    tier = SpellTier.Intermediate,

                    manaCost = 50,
                    castTime = 1.5f,
                    cooldown = 8f,
                    range = 20f,

                    damage = 40, // Per missile
                    damageType = DamageType.Arcane,
                    projectileCount = 5,
                    homingProjectiles = true,

                    vfxEffect = "arcane_missiles",
                    soundEffect = "arcane_whoosh",
                    requiredLevel = 11,
                    learningCost = 400
                },

                new SpellDefinition
                {
                    spellID = "spell_time_stop",
                    spellName = "Time Stop",
                    description = "Freeze time in a small area, stunning all enemies for a brief moment.",
                    school = MagicSchool.Arcane,
                    tier = SpellTier.Master,

                    manaCost = 120,
                    castTime = 2.0f,
                    cooldown = 60f,
                    range = 10f,
                    areaOfEffect = 8f,
                    duration = 4f,

                    hasCrowdControl = true,
                    crowdControlType = CrowdControl.Stun,
                    crowdControlChance = 100f,
                    specialEffect = "Frozen in time",

                    vfxEffect = "time_freeze",
                    soundEffect = "time_stop",
                    requiredLevel = 30,
                    learningCost = 3000
                },

                // Utility Spell
                new SpellDefinition
                {
                    spellID = "spell_teleport",
                    spellName = "Teleport",
                    description = "Instantly teleport to a target location within range.",
                    school = MagicSchool.Arcane,
                    tier = SpellTier.Expert,

                    manaCost = 40,
                    castTime = 0f,
                    cooldown = 15f,
                    range = 15f,

                    isTeleport = true,
                    specialEffect = "Instant movement",

                    vfxEffect = "teleport_flash",
                    soundEffect = "blink",
                    requiredLevel = 20,
                    learningCost = 1000
                }
            };
        }

        /// <summary>
        /// Magic school
        /// </summary>
        public enum MagicSchool
        {
            Fire,
            Ice,
            Lightning,
            Nature,
            Light,
            Dark,
            Arcane,
            Summoning
        }

        /// <summary>
        /// Spell tier
        /// </summary>
        public enum SpellTier
        {
            Novice,
            Intermediate,
            Advanced,
            Expert,
            Master
        }

        /// <summary>
        /// Damage type
        /// </summary>
        public enum DamageType
        {
            Physical,
            Fire,
            Ice,
            Lightning,
            Nature,
            Holy,
            Shadow,
            Arcane
        }

        /// <summary>
        /// Crowd control type
        /// </summary>
        public enum CrowdControl
        {
            Stun,
            Root,
            Slow,
            Freeze,
            Silence,
            Knockback
        }

        /// <summary>
        /// Buff type
        /// </summary>
        public enum BuffType
        {
            Damage,
            Speed,
            Regeneration,
            Shield,
            Resistance
        }

        /// <summary>
        /// Debuff type
        /// </summary>
        public enum DebuffType
        {
            Weakness,
            Slow,
            Vulnerability,
            Poison,
            Curse
        }

        /// <summary>
        /// Spell definition
        /// </summary>
        [System.Serializable]
        public class SpellDefinition
        {
            public string spellID;
            public string spellName;
            public string description;
            public MagicSchool school;
            public SpellTier tier;

            // Costs and Timing
            public int manaCost;
            public float castTime;
            public float cooldown;
            public float range;
            public float areaOfEffect;
            public float duration;

            // Damage
            public int damage;
            public DamageType damageType;
            public bool hasDOT; // Damage Over Time
            public int dotDamage;
            public float dotDuration;
            public bool damageUndead;
            public float extraDamageVsUndead;

            // Healing
            public int healAmount;
            public bool hasHOT; // Heal Over Time
            public int hotAmount;
            public float ticksPerSecond = 1f;

            // Special Mechanics
            public bool isPassive;
            public bool isTeleport;
            public bool lifesteal;
            public float lifestealPercent;
            public bool reflectDamage;
            public float reflectPercentage;
            public int shieldAmount;
            public bool piercing;
            public bool homingProjectiles;
            public int projectileCount = 1;
            public bool randomTargeting;

            // Chain Effects
            public int chainTargets;
            public float chainRange;
            public float chainDamageReduction;

            // Crowd Control
            public bool hasCrowdControl;
            public CrowdControl crowdControlType;
            public float crowdControlChance;
            public float slowAmount;
            public float freezeDuration;

            // Buffs/Debuffs
            public BuffType buffType;
            public float buffDuration;
            public DebuffType debuffType;
            public float debuffDuration;
            public float debuffAmount;

            // Effects
            public string vfxEffect;
            public string soundEffect;
            public string specialEffect;

            // Requirements
            public int requiredLevel;
            public int learningCost;
            public string prerequisiteSpell;
        }
    }
}
