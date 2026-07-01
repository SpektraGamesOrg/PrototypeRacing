using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// The ordered level tables for the level-based events (GDD: 15 levels each for Jump Challenge and Time
    /// Trial). A single <see cref="SingletonScriptableObject{T}"/> loaded from Resources by type name, so the
    /// asset MUST be named "EventLevelContainer" and live in a Resources folder
    /// (e.g. Assets/_Game/Data/Resources/EventLevelContainer.asset).
    ///
    /// Levels are NOT tied to a specific area: every area of a mode plays the player's CURRENT level for that
    /// mode (tracked in <see cref="Save.SaveManager"/>). Beating a level advances the counter for the whole map,
    /// and it wraps back to level 1 after the last one - an endless loop (per the design answer).
    /// </summary>
    [CreateAssetMenu(fileName = "EventLevelContainer", menuName = "EventSystem/Event Level Container")]
    public sealed class EventLevelContainer : SingletonScriptableObject<EventLevelContainer>
    {
        [Title("Jump Challenge")]
        [Tooltip("Ordered Jump Challenge levels (index 0 = level 1). Played in order, then wraps.")]
        [SerializeField] private List<LevelData> jumpChallengeLevels = new List<LevelData>();

        [Title("Time Trial")]
        [Tooltip("Ordered Time Trial levels (index 0 = level 1). Played in order, then wraps.")]
        [SerializeField] private List<LevelData> timeTrialLevels = new List<LevelData>();

        /// <summary>Number of authored levels for a mode (0 for Watch &amp; Earn, which has no levels).</summary>
        public int GetLevelCount(EventType type)
        {
            List<LevelData> list = ListFor(type);
            return list?.Count ?? 0;
        }

        /// <summary>
        /// Resolves the <see cref="LevelData"/> for a 1-based level number, wrapping past the end so the loop is
        /// endless. Returns null (with an error) when the mode has no levels authored.
        /// </summary>
        public LevelData GetLevel(EventType type, int levelNumber)
        {
            List<LevelData> list = ListFor(type);
            if (list == null || list.Count == 0)
            {
                Debug.LogError($"[EventLevelContainer] No levels authored for {type}.", this);
                return null;
            }

            int index = (levelNumber - 1) % list.Count;
            if (index < 0)
                index += list.Count;

            return list[index];
        }

        private List<LevelData> ListFor(EventType type)
        {
            switch (type)
            {
                case EventType.JumpChallenge: return jumpChallengeLevels;
                case EventType.TimeTrial: return timeTrialLevels;
                default: return null; // Watch & Earn has no levels
            }
        }
    }
}
