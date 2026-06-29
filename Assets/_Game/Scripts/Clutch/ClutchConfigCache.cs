using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Save;
using UnityEngine;

namespace Clutch
{
    /// <summary>
    /// PlayerPrefs cache for resolved Clutch flag values. Everything lives in ONE combined JSON blob
    /// under <see cref="SaveKeys.ClutchConfig"/>, keyed by flag name:
    /// <code>{ "VehicleConfig": "{...}", "AdConfig": "{...}" }</code>
    /// Each value is the flag's own JSON serialized to a string (Clutch returns native JSON; we keep it
    /// verbatim so consumers deserialize the exact shape). This is the user-storage layer in the
    /// scenarios: Clutch overwrites it on success; it is the source of truth when Clutch fails but a
    /// value was cached; and it gets seeded from the fallback SO when Clutch fails on a clean install.
    /// </summary>
    public static class ClutchConfigCache
    {
        // Returns the full cached blob as a JObject (empty when nothing is cached yet).
        private static JObject Load()
        {
            string raw = PlayerPrefs.GetString(SaveKeys.ClutchConfig, string.Empty);
            if (string.IsNullOrEmpty(raw))
                return new JObject();

            try
            {
                return JObject.Parse(raw);
            }
            catch (JsonException e)
            {
                // Corrupt blob: drop it rather than crash the boot path.
                Debug.LogError($"[ClutchConfigCache] Failed to parse cached config, discarding: {e.Message}");
                return new JObject();
            }
        }

        private static void Store(JObject blob)
        {
            PlayerPrefs.SetString(SaveKeys.ClutchConfig, blob.ToString(Formatting.None));
        }

        /// <summary>True when at least one flag value has been cached.</summary>
        public static bool HasAny()
        {
            return Load().Count > 0;
        }

        /// <summary>True when the given flag has a cached value.</summary>
        public static bool Has(string flagKey)
        {
            return !string.IsNullOrEmpty(flagKey) && Load()[flagKey] != null;
        }

        /// <summary>
        /// Reads a cached flag's JSON string. Returns false when the flag is not cached.
        /// </summary>
        public static bool TryGet(string flagKey, out string json)
        {
            json = null;
            if (string.IsNullOrEmpty(flagKey))
                return false;

            JToken token = Load()[flagKey];
            if (token == null || token.Type == JTokenType.Null)
                return false;

            // Stored as a JSON string token; hand the raw inner value back to the caller.
            json = token.Type == JTokenType.String ? token.Value<string>() : token.ToString(Formatting.None);
            return !string.IsNullOrEmpty(json);
        }

        /// <summary>
        /// Writes (or overwrites) a flag's JSON value. Does NOT flush; call <see cref="Save"/> after a
        /// batch of writes (mirrors the project's SaveManager flush convention).
        /// </summary>
        public static void Set(string flagKey, string json)
        {
            if (string.IsNullOrEmpty(flagKey))
            {
                Debug.LogError("[ClutchConfigCache] Set called with empty flag key.");
                return;
            }

            JObject blob = Load();
            blob[flagKey] = json ?? string.Empty;
            Store(blob);
        }

        /// <summary>Flushes staged writes to disk.</summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>Clears the entire cached blob. Intended for development / debug tooling.</summary>
        public static void Clear()
        {
            PlayerPrefs.DeleteKey(SaveKeys.ClutchConfig);
            PlayerPrefs.Save();
        }
    }
}
