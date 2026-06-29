using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Vehicles;

namespace _Game.Scripts.Clutch.Editor
{
    /// <summary>
    /// Editor guardrail for the Clutch remote-pricing wiring. The remote price path in
    /// MainMenuScreen.TargetOf only fires when a ByGold vehicle has a non-empty ClutchPriceKey AND that key
    /// exists in the "VehiclePrices" flag, so a missing or misspelled key silently ships inert remote
    /// pricing (the car keeps its serialized amount and never reads Clutch). This validator surfaces those
    /// gaps as errors at author time:
    ///   * a ByGold entry with an empty ClutchPriceKey,
    ///   * a ClutchPriceKey that has no matching entry in the ClutchConfig "VehiclePrices" fallback
    ///     (case-sensitive — the runtime read is a Dictionary&lt;string,int&gt; lookup),
    ///   * a fallback "VehiclePrices" key that no ByGold entry references (dead key).
    /// Editor-only; uses Debug.LogError per project rules.
    /// </summary>
    public static class ClutchVehiclePriceValidator
    {
        [MenuItem("Tools/Clutch/Validate Vehicle Price Keys")]
        public static void Validate()
        {
            VehicleContainer container = VehicleContainer.Instance;
            if (!container)
            {
                Debug.LogError("[ClutchVehiclePriceValidator] VehicleContainer asset not found in Resources.");
                return;
            }

            global::Clutch.ClutchConfig fallbackConfig = global::Clutch.ClutchConfig.Instance;

            // Parse the VehiclePrices fallback keys (case-sensitive) for cross-checking.
            HashSet<string> fallbackKeys = new HashSet<string>();
            if (fallbackConfig && fallbackConfig.TryGetFallback(global::Clutch.ClutchFlagKeys.VehiclePrices, out string pricesJson))
            {
                try
                {
                    Dictionary<string, int> map = JsonConvert.DeserializeObject<Dictionary<string, int>>(pricesJson);
                    if (map != null)
                        foreach (string k in map.Keys)
                            fallbackKeys.Add(k);
                }
                catch (JsonException e)
                {
                    Debug.LogError($"[ClutchVehiclePriceValidator] ClutchConfig VehiclePrices fallback is not a valid string->int map: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("[ClutchVehiclePriceValidator] No VehiclePrices fallback found in ClutchConfig; cars will fall back to their serialized amount when Clutch is unreachable.");
            }

            int problems = 0;
            HashSet<string> referencedKeys = new HashSet<string>();

            IReadOnlyList<VehicleEntry> vehicles = container.Vehicles;
            for (int i = 0; i < vehicles.Count; i++)
            {
                VehicleEntry entry = vehicles[i];
                if (entry == null)
                    continue;

                bool isByGold = (entry.VehicleObtainType & VehicleObtainType.ByGold) != 0;
                if (!isByGold)
                    continue;

                if (string.IsNullOrEmpty(entry.ClutchPriceKey))
                {
                    Debug.LogError($"[ClutchVehiclePriceValidator] {entry.ID} is ByGold but has an empty ClutchPriceKey; its price can never be set remotely.");
                    problems++;
                    continue;
                }

                referencedKeys.Add(entry.ClutchPriceKey);

                if (fallbackKeys.Count > 0 && !fallbackKeys.Contains(entry.ClutchPriceKey))
                {
                    Debug.LogError($"[ClutchVehiclePriceValidator] {entry.ID} ClutchPriceKey '{entry.ClutchPriceKey}' has no matching key in the VehiclePrices fallback (check spelling/case).");
                    problems++;
                }
            }

            // Fallback keys that no ByGold entry references are dead config.
            foreach (string fallbackKey in fallbackKeys)
            {
                if (!referencedKeys.Contains(fallbackKey))
                {
                    Debug.LogError($"[ClutchVehiclePriceValidator] VehiclePrices fallback key '{fallbackKey}' is not referenced by any ByGold vehicle (dead key).");
                    problems++;
                }
            }

            if (problems == 0)
                Debug.Log($"[ClutchVehiclePriceValidator] OK: {referencedKeys.Count} ByGold vehicle price key(s) validated against the fallback.");
            else
                Debug.LogError($"[ClutchVehiclePriceValidator] Found {problems} problem(s) in vehicle price key wiring.");
        }
    }
}
