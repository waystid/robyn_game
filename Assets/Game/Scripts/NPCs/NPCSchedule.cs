using UnityEngine;
using System.Collections.Generic;
using CozyGame.Dialogue;

namespace CozyGame.NPCs
{
    /// <summary>
    /// NPC activity type
    /// </summary>
    public enum NPCActivityType
    {
        Idle,           // Stand still
        Wander,         // Random wandering
        Work,           // Working at a location
        Sleep,          // Sleeping/resting
        Eat,            // Eating
        Socialize,      // Talking with others
        Custom          // Custom behavior
    }

    /// <summary>
    /// Schedule entry for NPC
    /// </summary>
    [System.Serializable]
    public class NPCScheduleEntry
    {
        [Header("Time")]
        [Tooltip("Start hour (0-24)")]
        [Range(0f, 24f)]
        public float startHour = 8f;

        [Tooltip("End hour (0-24)")]
        [Range(0f, 24f)]
        public float endHour = 12f;

        [Header("Activity")]
        [Tooltip("Activity type")]
        public NPCActivityType activityType = NPCActivityType.Idle;

        [Tooltip("Activity description")]
        public string activityDescription = "Standing idle";

        [Header("Location")]
        [Tooltip("Target location name (optional)")]
        public string locationName = "";

        [Tooltip("Target location (optional)")]
        public Transform targetLocation;

        [Tooltip("Movement speed multiplier")]
        [Range(0.1f, 2f)]
        public float speedMultiplier = 1f;

        [Tooltip("Stay radius (for wandering)")]
        public float stayRadius = 5f;

        [Header("Behavior")]
        [Tooltip("Animation state name")]
        public string animationStateName = "Idle";

        [Tooltip("Custom dialogue for this time period")]
        public DialogueData customDialogue;

        [Tooltip("Look at player when idle")]
        public bool lookAtPlayer = true;

        [Header("Advanced")]
        [Tooltip("Custom behavior script name")]
        public string customBehaviorName = "";

        [Tooltip("Priority (higher = more important)")]
        public int priority = 0;

        /// <summary>
        /// Check if this entry is active at given hour
        /// </summary>
        public bool IsActiveAtTime(float currentHour)
        {
            // Handle wrap-around (e.g., 22:00 - 2:00)
            if (endHour < startHour)
            {
                return currentHour >= startHour || currentHour < endHour;
            }
            else
            {
                return currentHour >= startHour && currentHour < endHour;
            }
        }

        /// <summary>
        /// Get time until this entry starts
        /// </summary>
        public float GetTimeUntilStart(float currentHour)
        {
            if (currentHour <= startHour)
            {
                return startHour - currentHour;
            }
            else
            {
                // Next day
                return (24f - currentHour) + startHour;
            }
        }
    }

    /// <summary>
    /// NPC schedule ScriptableObject.
    /// Defines time-based activities and locations for NPCs.
    /// Create instances via: Right-click → Create → Cozy Game → NPCs → NPC Schedule
    /// </summary>
    [CreateAssetMenu(fileName = "New NPC Schedule", menuName = "Cozy Game/NPCs/NPC Schedule", order = 10)]
    public class NPCSchedule : ScriptableObject
    {
        [Header("Info")]
        [Tooltip("Schedule name")]
        public string scheduleName = "Daily Schedule";

        [Tooltip("Schedule description")]
        [TextArea(2, 3)]
        public string description = "Default daily schedule for NPC";

        [Header("Schedule Entries")]
        [Tooltip("Schedule entries (time-based activities)")]
        public NPCScheduleEntry[] scheduleEntries;

        [Header("Default Behavior")]
        [Tooltip("Default activity when no schedule entry matches")]
        public NPCActivityType defaultActivity = NPCActivityType.Idle;

        [Tooltip("Default location")]
        public Transform defaultLocation;

        [Header("Weather Overrides")]
        [Tooltip("Override schedule during rain")]
        public bool overrideDuringRain = false;

        [Tooltip("Rain activity")]
        public NPCActivityType rainActivity = NPCActivityType.Idle;

        [Tooltip("Rain location")]
        public Transform rainLocation;

        [Tooltip("Override schedule during storm")]
        public bool overrideDuringStorm = true;

        [Tooltip("Storm activity")]
        public NPCActivityType stormActivity = NPCActivityType.Idle;

        [Tooltip("Storm location (usually indoors)")]
        public Transform stormLocation;

        /// <summary>
        /// Get current schedule entry for given time
        /// </summary>
        public NPCScheduleEntry GetCurrentEntry(float currentHour)
        {
            if (scheduleEntries == null || scheduleEntries.Length == 0)
                return null;

            NPCScheduleEntry bestEntry = null;
            int highestPriority = int.MinValue;

            foreach (var entry in scheduleEntries)
            {
                if (entry.IsActiveAtTime(currentHour))
                {
                    if (entry.priority > highestPriority)
                    {
                        highestPriority = entry.priority;
                        bestEntry = entry;
                    }
                }
            }

            return bestEntry;
        }

        /// <summary>
        /// Get next schedule entry
        /// </summary>
        public NPCScheduleEntry GetNextEntry(float currentHour)
        {
            if (scheduleEntries == null || scheduleEntries.Length == 0)
                return null;

            NPCScheduleEntry nextEntry = null;
            float shortestTime = float.MaxValue;

            foreach (var entry in scheduleEntries)
            {
                if (!entry.IsActiveAtTime(currentHour))
                {
                    float timeUntil = entry.GetTimeUntilStart(currentHour);
                    if (timeUntil < shortestTime)
                    {
                        shortestTime = timeUntil;
                        nextEntry = entry;
                    }
                }
            }

            return nextEntry;
        }

        /// <summary>
        /// Get all active entries at given time
        /// </summary>
        public List<NPCScheduleEntry> GetActiveEntries(float currentHour)
        {
            List<NPCScheduleEntry> activeEntries = new List<NPCScheduleEntry>();

            if (scheduleEntries == null)
                return activeEntries;

            foreach (var entry in scheduleEntries)
            {
                if (entry.IsActiveAtTime(currentHour))
                {
                    activeEntries.Add(entry);
                }
            }

            // Sort by priority
            activeEntries.Sort((a, b) => b.priority.CompareTo(a.priority));

            return activeEntries;
        }

        /// <summary>
        /// Check if schedule should be overridden by weather
        /// </summary>
        public bool ShouldOverrideForWeather(Environment.WeatherState weather, out NPCActivityType activity, out Transform location)
        {
            activity = defaultActivity;
            location = defaultLocation;

            switch (weather)
            {
                case Environment.WeatherState.Rain:
                case Environment.WeatherState.HeavyRain:
                    if (overrideDuringRain)
                    {
                        activity = rainActivity;
                        location = rainLocation;
                        return true;
                    }
                    break;

                case Environment.WeatherState.Storm:
                    if (overrideDuringStorm)
                    {
                        activity = stormActivity;
                        location = stormLocation;
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Get formatted schedule summary
        /// </summary>
        public string GetScheduleSummary()
        {
            if (scheduleEntries == null || scheduleEntries.Length == 0)
                return "No schedule entries";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>{scheduleName}</b>");
            sb.AppendLine(description);
            sb.AppendLine();

            foreach (var entry in scheduleEntries)
            {
                int startHour = Mathf.FloorToInt(entry.startHour);
                int startMin = Mathf.FloorToInt((entry.startHour - startHour) * 60f);
                int endHour = Mathf.FloorToInt(entry.endHour);
                int endMin = Mathf.FloorToInt((entry.endHour - endHour) * 60f);

                string startTime = $"{startHour:D2}:{startMin:D2}";
                string endTime = $"{endHour:D2}:{endMin:D2}";

                sb.AppendLine($"{startTime} - {endTime}: {entry.activityDescription}");
            }

            return sb.ToString();
        }
    }
}
