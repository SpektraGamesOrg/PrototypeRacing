using System.Collections.Generic;
using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// The palette of spawnable level pieces the Level Designer window offers - the "container objects register
    /// to". Mirrors the project's singleton-container convention (<see cref="Vehicles.VehicleContainer"/>): a
    /// <see cref="SingletonScriptableObject{T}"/> loaded from Resources by its type name, so the asset MUST be
    /// named "EventObjectContainer" and live in a Resources folder
    /// (e.g. Assets/_Game/Data/Resources/EventObjectContainer.asset).
    ///
    /// This is an editor-facing catalogue: at runtime a level references its prefabs directly through
    /// <see cref="LevelData"/>/<see cref="LevelPlacement"/>, so nothing here is loaded during play. The
    /// "Sync From Folder" button auto-registers every prefab with a <see cref="LevelObject"/> under
    /// <see cref="scanFolder"/>, so a designer just drops a prefab in that folder and re-syncs.
    /// </summary>
    [CreateAssetMenu(fileName = "EventObjectContainer", menuName = "EventSystem/Event Object Container")]
    public sealed class EventObjectContainer : SingletonScriptableObject<EventObjectContainer>
    {
        [Title("Palette")]
        [Tooltip("Spawnable level pieces (ramps, obstacles, start/finish markers, ...) offered by the Level " +
                 "Designer window. Populate manually or via \"Sync From Folder\".")]
        [SerializeField] private List<LevelObject> presets = new List<LevelObject>();

        public IReadOnlyList<LevelObject> Presets => presets;

#if UNITY_EDITOR
        [Title("Editor")]
        [Tooltip("Folder scanned by \"Sync From Folder\" for prefabs carrying a LevelObject component.")]
        [FolderPath, SerializeField]
        private string scanFolder = "Assets/_Game/Prefabs/EventObjects";

        /// <summary>
        /// Editor-only: rebuilds <see cref="presets"/> from every prefab under <see cref="scanFolder"/> that has
        /// a <see cref="LevelObject"/> on its root. Editor LINQ is allowed (CLAUDE.md). Existing manual entries
        /// outside the folder are dropped, keeping the palette a faithful mirror of the folder.
        /// </summary>
        [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        private void SyncFromFolder()
        {
            var found = new List<LevelObject>();
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { scanFolder });
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!go)
                    continue;

                var levelObject = go.GetComponent<LevelObject>();
                if (levelObject && !found.Contains(levelObject))
                    found.Add(levelObject);
            }

            presets = found;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
            Debug.Log($"[EventObjectContainer] Synced {presets.Count} level piece(s) from '{scanFolder}'.", this);
        }
#endif
    }
}
