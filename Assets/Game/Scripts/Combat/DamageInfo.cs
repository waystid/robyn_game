using UnityEngine;

namespace CozyGame.Combat
{
    /// <summary>
    /// Damage type categories
    /// </summary>
    public enum DamageType
    {
        Physical,
        Magic,
        Fire,
        Ice,
        Lightning,
        Poison,
        True // Ignores armor
    }

    /// <summary>
    /// Damage information structure.
    /// Contains all data about a damage event.
    /// </summary>
    [System.Serializable]
    public class DamageInfo
    {
        public float amount;
        public DamageType damageType;
        public GameObject attacker;
        public GameObject victim;
        public Vector3 hitPoint;
        public Vector3 hitDirection;
        public bool isCritical;
        public bool isBlocked;

        public DamageInfo(float dmg, DamageType type, GameObject source, GameObject target)
        {
            amount = dmg;
            damageType = type;
            attacker = source;
            victim = target;
            isCritical = false;
            isBlocked = false;
        }

        public DamageInfo(float dmg, DamageType type, GameObject source, GameObject target, Vector3 point, Vector3 direction)
        {
            amount = dmg;
            damageType = type;
            attacker = source;
            victim = target;
            hitPoint = point;
            hitDirection = direction;
            isCritical = false;
            isBlocked = false;
        }
    }
}
