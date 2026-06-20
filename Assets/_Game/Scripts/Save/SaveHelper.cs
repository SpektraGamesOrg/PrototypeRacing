using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UI;
using UIManager;
using UnityEngine;
using Vehicles;

namespace Save
{
    /// <summary>
    /// Tiny persistent component that auto-flushes the save system to disk when the app
    /// is paused or quit, so mobile players never lose progress without an explicit Save().
    /// Self-spawns at runtime, so no scene or prefab wiring is required.
    /// </summary>
    public class SaveHelper : SingletonComponent<SaveHelper>
    {
#if UNITY_EDITOR
        [ShowInInspector, ReadOnly, BoxGroup("Save State")]
        private int CurrentCoins => Application.isPlaying ? SaveManager.Coins : 0;

        [ShowInInspector, ReadOnly, BoxGroup("Save State")]
        private List<VehicleID> OwnedVehicles => Application.isPlaying
            ? SaveManager.GetOwnedVehicles().Select(v => v.id).ToList()
            : new List<VehicleID>();

        [ShowInInspector, ReadOnly, BoxGroup("Save State")]
        private List<VehicleID> NotOwnedVehicles => Application.isPlaying && VehicleContainer.Instance != null
            ? VehicleContainer.Instance.Vehicles.Where(v => !SaveManager.IsOwned(v.ID)).Select(v => v.ID).ToList()
            : new List<VehicleID>();
#endif

        [Button]
        private void AddCurrency()
        {
            int half = int.MaxValue / 2;
            int current = SaveManager.Coins;
            SaveManager.Coins = current > int.MaxValue - half ? int.MaxValue : current + half;
            SaveManager.Save();
            GameUIManager.Instance?.GetScreen<MainMenuScreen>()?.RefreshCoinDisplay();
        }

        [Button]
        private void ClearPlayerPrefs()
        {
            SaveManager.ResetAll();
            GameUIManager.Instance?.GetScreen<MainMenuScreen>()?.RefreshCoinDisplay();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
                SaveManager.Save();
        }

        private void OnApplicationQuit()
        {
            SaveManager.Save();
        }
    }
}