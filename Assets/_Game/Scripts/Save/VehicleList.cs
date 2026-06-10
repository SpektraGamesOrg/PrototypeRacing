using System;
using System.Collections.Generic;

namespace Save
{
    /// <summary>
    /// Serializable container that holds every vehicle the player has data for.
    /// Stored as a single JSON blob (one PlayerPrefs key) so the whole garage saves together.
    /// JsonUtility needs a wrapper class like this to serialize a List.
    /// </summary>
    [Serializable]
    public class VehicleList
    {
        public List<VehicleSaveData> vehicles = new List<VehicleSaveData>();
    }
}
