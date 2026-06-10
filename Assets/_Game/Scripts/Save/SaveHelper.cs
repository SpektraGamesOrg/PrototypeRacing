using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Tiny persistent component that auto-flushes the save system to disk when the app
    /// is paused or quit, so mobile players never lose progress without an explicit Save().
    /// Self-spawns at runtime, so no scene or prefab wiring is required.
    /// </summary>
    public class SaveHelper : SingletonComponent<SaveHelper>
    {
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