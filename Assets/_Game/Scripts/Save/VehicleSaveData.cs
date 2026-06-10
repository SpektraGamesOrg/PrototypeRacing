using System;
using SpektraGames.AddressableLoader.Runtime;
using UnityEngine.AddressableAssets;
using Vehicles;

namespace Save
{
    /// <summary>
    /// Serializable, persisted state for a single vehicle.
    /// Plain class (not MonoBehaviour / ScriptableObject) so it can be stored as JSON.
    /// Add new customization fields here as the game grows - everything in this class
    /// is saved/loaded automatically.
    /// </summary>
    [Serializable]
    public class VehicleSaveData
    {
        // Default values
        public const string DefaultPaintColorHex = "#FFFFFF";

        public VehicleNameType nameType;
        public bool owned;

        // --- Customization (extend freely) ---
        // Example paint color. Maps to the CarShader color properties when applied.
        public string paintColorHex = DefaultPaintColorHex;

        public VehicleSaveData() { }
        
        public VehicleSaveData(VehicleNameType nameType)
        {
            this.nameType = nameType;
        }
    }
}
