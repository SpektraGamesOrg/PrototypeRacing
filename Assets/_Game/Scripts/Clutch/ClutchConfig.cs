using System;
using System.Collections.Generic;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Clutch
{
    /// <summary>
    /// Offline FALLBACK values for Clutch flags, authored in the editor (and refreshable from live Clutch
    /// via Tools/Clutch). A <see cref="SingletonScriptableObject{T}"/> so <see cref="Instance"/> resolves
    /// from Resources at runtime. Used only when Clutch fails AND the player has no cached value yet
    /// (clean install, offline) — see ClutchConfigService for the full resolution order.
    ///
    /// Each entry stores the flag's JSON verbatim (same shape Clutch returns) so the runtime path
    /// deserializes fallbacks identically to remote values. Generic by design: add a flag by adding an
    /// entry, no code change.
    /// </summary>
    [CreateAssetMenu(fileName = "ClutchConfig", menuName = "Clutch/Clutch Config (Fallback)")]
    public class ClutchConfig : SingletonScriptableObject<ClutchConfig>
    {
        [Serializable]
        public class ClutchFlagFallback
        {
            [Tooltip("Clutch flag key, e.g. \"VehiclePrices\" or \"AdConfig\".")]
            public string key;

            [Tooltip("Fallback value as JSON, matching the exact shape Clutch returns for this flag.")]
            [TextArea(2, 8)]
            public string fallbackJson;
        }

        [SerializeField]
        [Tooltip("One entry per Clutch flag the game reads.")]
        private List<ClutchFlagFallback> fallbacks = new List<ClutchFlagFallback>();

        public IReadOnlyList<ClutchFlagFallback> Fallbacks => fallbacks;

        /// <summary>Returns the authored fallback JSON for a flag, or false when none is configured.</summary>
        public bool TryGetFallback(string flagKey, out string json)
        {
            json = null;
            if (string.IsNullOrEmpty(flagKey))
                return false;

            for (int i = 0; i < fallbacks.Count; i++)
            {
                if (fallbacks[i] != null && fallbacks[i].key == flagKey)
                {
                    json = fallbacks[i].fallbackJson;
                    return !string.IsNullOrEmpty(json);
                }
            }

            return false;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: writes (or replaces) a flag's fallback JSON. Used by the Tools/Clutch fetch tool
        /// to refresh authored fallbacks from live Clutch. Caller is responsible for SetDirty/SaveAssets.
        /// </summary>
        public void SetFallbackEditor(string flagKey, string json)
        {
            if (string.IsNullOrEmpty(flagKey))
                return;

            for (int i = 0; i < fallbacks.Count; i++)
            {
                if (fallbacks[i] != null && fallbacks[i].key == flagKey)
                {
                    fallbacks[i].fallbackJson = json;
                    return;
                }
            }

            fallbacks.Add(new ClutchFlagFallback { key = flagKey, fallbackJson = json });
        }
#endif
    }
}
