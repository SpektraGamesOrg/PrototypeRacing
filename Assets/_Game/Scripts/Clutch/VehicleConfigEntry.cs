using System;
using Newtonsoft.Json;
using UnityEngine;
using Vehicles;

namespace Clutch
{
    /// <summary>
    /// One vehicle's remote obtain config from the Clutch "VehicleConfig" flag, e.g.
    /// <c>{"value":1500,"obtain_type":"ByGold"}</c>. <see cref="value"/> is the numeric target
    /// (gold price for ByGold, ad count for ByWatchAds, km for DistanceMilestoneKm; ignored for Free).
    /// <see cref="obtain_type"/> is one or more <see cref="VehicleObtainType"/> flag names combined with
    /// '|' or ',' (case-insensitive). Field names are snake_case to match the flag JSON verbatim.
    /// </summary>
    [Serializable]
    public class VehicleConfigEntry
    {
        public int value;

        [JsonProperty("obtain_type")]
        public string obtain_type;
    }

    /// <summary>
    /// The effective obtain config for a vehicle after resolving Clutch over the SO fallback:
    /// which obtain path(s) apply and the numeric target. Consumers use this instead of reading
    /// <see cref="VehicleEntry.VehicleObtainType"/> / <see cref="VehicleEntry.VehicleObtainTargetAmount"/>
    /// directly, so remote config can change both.
    /// </summary>
    public readonly struct ResolvedVehicleConfig
    {
        public readonly VehicleObtainType ObtainType;
        public readonly int Amount;

        public ResolvedVehicleConfig(VehicleObtainType obtainType, int amount)
        {
            ObtainType = obtainType;
            Amount = amount;
        }
    }

    /// <summary>
    /// Parses the <c>obtain_type</c> string of a <see cref="VehicleConfigEntry"/> into the
    /// <see cref="VehicleObtainType"/> [Flags] enum. Accepts a single name ("ByGold") or several combined
    /// with '|' or ',' ("ByGold|ByWatchAds"), case-insensitive and whitespace-tolerant.
    /// </summary>
    public static class ObtainTypeParser
    {
        private static readonly char[] Separators = { '|', ',' };

        /// <summary>
        /// Parses <paramref name="raw"/> into a flag set. Returns false (and leaves
        /// <paramref name="obtainType"/> at default) when the string is empty or contains no recognizable
        /// flag, so callers can fall back to the serialized type.
        /// </summary>
        public static bool TryParse(string raw, out VehicleObtainType obtainType)
        {
            obtainType = default;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            bool any = false;
            string[] tokens = raw.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i++)
            {
                string token = tokens[i].Trim();
                if (token.Length == 0)
                    continue;

                if (Enum.TryParse(token, ignoreCase: true, out VehicleObtainType parsed) &&
                    Enum.IsDefined(typeof(VehicleObtainType), parsed))
                {
                    obtainType |= parsed;
                    any = true;
                }
                else
                {
                    Debug.LogError($"[ObtainTypeParser] Unknown obtain_type token '{token}' in '{raw}'.");
                }
            }

            // Free is exclusive (mirrors VehicleObtainTypeDrawer): if present, it wins outright.
            if (any && (obtainType & VehicleObtainType.Free) != 0)
                obtainType = VehicleObtainType.Free;

            return any;
        }
    }
}
