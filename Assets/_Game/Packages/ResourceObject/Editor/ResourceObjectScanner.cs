using System;
using System.Collections.Generic;
using System.IO;
using SpektraGames.ResourceObject.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace SpektraGames.ResourceObject.Editor
{
    /// <summary>
    /// Shared, dependency-light helpers that find and heal <see cref="ResourceObject{T}"/> values inside serialized data.
    /// Used by both <see cref="ResourceObjectAssetTracker"/> (live, incremental) and <see cref="ResourceObjectRegistry"/>
    /// (full project sync), so the detection and healing rules live in exactly one place.
    ///
    /// A "ResourceObject node" is detected structurally: a Generic serialized property that has two string children named
    /// "guid" and "resourcesPath" - exactly the two serialized fields of <see cref="ResourceObject{T}"/>. This is the same
    /// heuristic the original tracker used, kept identical so behavior does not change.
    /// </summary>
    internal static class ResourceObjectScanner
    {
        // The serialized field that is (practically) unique to a ResourceObject. Used as a cheap text prefilter so we only
        // pay for SerializedObject parsing on files that could possibly contain one.
        private const string FieldMarker = "resourcesPath";

        // =====================================================================
        // Detection
        // =====================================================================

        private static bool IsResourceObjectNode(SerializedProperty guidProp, SerializedProperty pathProp)
        {
            return guidProp != null && pathProp != null &&
                   guidProp.propertyType == SerializedPropertyType.String &&
                   pathProp.propertyType == SerializedPropertyType.String;
        }

        // =====================================================================
        // Collect (read-only) - which asset guids does this object reference?
        // =====================================================================

        /// <summary>
        /// Walk every serialized property and add the (non-empty) guid of every ResourceObject node into
        /// <paramref name="into"/>. Read-only: never mutates the object.
        /// </summary>
        public static void CollectReferencedGuids(SerializedObject serializedObject, HashSet<string> into)
        {
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.Next(enterChildren))
            {
                enterChildren = true;
                if (iterator.propertyType != SerializedPropertyType.Generic)
                    continue;

                var element = iterator.Copy();
                var guidProp = element.FindPropertyRelative("guid");
                var pathProp = element.FindPropertyRelative("resourcesPath");
                if (!IsResourceObjectNode(guidProp, pathProp))
                    continue;

                // A ResourceObject node - record its guid and don't descend into its string children.
                enterChildren = false;

                var guid = guidProp.stringValue;
                if (!string.IsNullOrEmpty(guid))
                    into.Add(guid);
            }
        }

        /// <summary>Collect referenced guids from a prefab/.asset on disk (its main asset plus any sub-assets).</summary>
        public static void CollectFromAssetOwner(string assetPath, HashSet<string> into)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (obj)
                    CollectReferencedGuids(new SerializedObject(obj), into);
            }
        }

        /// <summary>Collect referenced guids from every component of every GameObject in a (loaded) scene.</summary>
        public static void CollectFromScene(Scene scene, HashSet<string> into)
        {
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                var components = roots[r].GetComponentsInChildren<Component>(true);
                for (int c = 0; c < components.Length; c++)
                {
                    if (components[c])
                        CollectReferencedGuids(new SerializedObject(components[c]), into);
                }
            }
        }

        /// <summary>
        /// Collect referenced guids from a scene by path. If the scene is currently loaded it is read in place; otherwise it
        /// is opened additively, read, and closed again (without saving), restoring its prior hierarchy membership.
        /// </summary>
        public static void CollectFromSceneAtPath(string scenePath, HashSet<string> into)
        {
            ProcessScene(scenePath, scene => { CollectFromScene(scene, into); return false; }, saveIfChanged: false);
        }

        // =====================================================================
        // Heal (write) - recompute resourcesPath from the (stable) guid
        // =====================================================================

        /// <summary>
        /// Recompute <c>resourcesPath</c> from the guid for every ResourceObject node. When <paramref name="restrictToGuids"/>
        /// is non-null, only nodes whose guid is in that set are touched (the fast path for a move, where only a few guids
        /// changed location). Returns true if anything changed (and applies the change without registering an undo step).
        /// </summary>
        public static bool Heal(SerializedObject serializedObject, HashSet<string> restrictToGuids)
        {
            bool changed = false;
            var iterator = serializedObject.GetIterator();
            bool enterChildren = true;

            while (iterator.Next(enterChildren))
            {
                enterChildren = true;
                if (iterator.propertyType != SerializedPropertyType.Generic)
                    continue;

                var element = iterator.Copy();
                var guidProp = element.FindPropertyRelative("guid");
                var pathProp = element.FindPropertyRelative("resourcesPath");
                if (!IsResourceObjectNode(guidProp, pathProp))
                    continue;

                // A ResourceObject node - don't descend into its string children.
                enterChildren = false;

                var guid = guidProp.stringValue;
                if (string.IsNullOrEmpty(guid))
                    continue;
                if (restrictToGuids != null && !restrictToGuids.Contains(guid))
                    continue;

                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(assetPath))
                    continue; // deleted asset -> leave as-is, the drawer flags it as missing

                // Null means it is no longer under a Resources folder -> invalidate the load path.
                var desired = ResourceObject<Object>.ToResourcesPath(assetPath) ?? string.Empty;
                if (desired != pathProp.stringValue)
                {
                    pathProp.stringValue = desired;
                    changed = true;
                }
            }

            if (changed)
                serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }

        /// <summary>Heal one prefab/.asset owner on disk. Marks it dirty when changed; the caller is responsible for saving.</summary>
        public static bool HealAssetOwner(string assetPath, HashSet<string> restrictToGuids)
        {
            bool changed = false;
            var objects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];
                if (obj && Heal(new SerializedObject(obj), restrictToGuids))
                    changed = true;
            }

            if (changed)
            {
                var main = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (main)
                    EditorUtility.SetDirty(main);
            }

            return changed;
        }

        /// <summary>Heal one loaded scene in memory. Marks the scene dirty when changed; saving is left to the caller/user.</summary>
        public static bool HealLoadedScene(Scene scene, HashSet<string> restrictToGuids)
        {
            return HealSceneRoots(scene, restrictToGuids);
        }

        /// <summary>
        /// Heal a scene that is not currently loaded - either absent from the hierarchy or present-but-unloaded: open it,
        /// heal, save when changed, and restore its prior membership. No-op in play mode. Returns true only once a change
        /// has been successfully saved (a throwing/failed save returns false rather than masking the failure).
        /// </summary>
        public static bool HealSceneAtPath(string scenePath, HashSet<string> restrictToGuids)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return false;
            return ProcessScene(scenePath, scene => HealSceneRoots(scene, restrictToGuids), saveIfChanged: true);
        }

        /// <summary>
        /// In a single open, heal the scene's ResourceObjects (restricted to <paramref name="restrictToGuids"/>) AND collect
        /// every guid it references into <paramref name="collectInto"/>. Used to both fix and index a not-yet-indexed scene
        /// when a move occurs. Restores prior membership. No-op in play mode.
        /// </summary>
        public static void HealAndCollectSceneAtPath(string scenePath, HashSet<string> restrictToGuids, HashSet<string> collectInto)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            ProcessScene(scenePath, scene =>
            {
                CollectFromScene(scene, collectInto);
                return HealSceneRoots(scene, restrictToGuids);
            }, saveIfChanged: true);
        }

        private static bool HealSceneRoots(Scene scene, HashSet<string> restrictToGuids)
        {
            bool changed = false;
            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                var components = roots[r].GetComponentsInChildren<Component>(true);
                for (int c = 0; c < components.Length; c++)
                {
                    if (components[c] && Heal(new SerializedObject(components[c]), restrictToGuids))
                        changed = true;
                }
            }

            if (changed)
                EditorSceneManager.MarkSceneDirty(scene);

            return changed;
        }

        /// <summary>
        /// Open <paramref name="scenePath"/> so it can be read/modified, run <paramref name="action"/> on the loaded Scene,
        /// then restore the scene's PRIOR hierarchy membership so the user's multi-scene layout is never changed:
        ///  - already loaded (e.g. the user's open scene): operated on in place, never closed or auto-saved;
        ///  - present but unloaded: loaded in place, then closed back to unloaded (the entry is kept);
        ///  - absent: added, then removed again.
        /// <paramref name="action"/> returns true when it changed the scene; when it did and <paramref name="saveIfChanged"/>
        /// is set, the scene is saved before closing. Returns true only when a change was actually persisted.
        /// </summary>
        private static bool ProcessScene(string scenePath, Func<Scene, bool> action, bool saveIfChanged)
        {
            // Find the scene's current membership in the multi-scene hierarchy (loaded OR unloaded).
            Scene present = default;
            bool isPresent = false;
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var candidate = EditorSceneManager.GetSceneAt(i);
                if (candidate.path == scenePath)
                {
                    present = candidate;
                    isPresent = true;
                    break;
                }
            }

            // Already loaded: operate in place and never close/auto-save the user's scene.
            if (isPresent && present.isLoaded)
                return action(present);

            Scene opened = default;
            bool result = false;
            try
            {
                // For a present-but-unloaded entry this loads it in place (no duplicate); for an absent scene it adds one.
                opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                bool changed = action(opened);
                if (changed && saveIfChanged)
                {
                    EditorSceneManager.SaveScene(opened);
                    result = true; // only after the save has actually succeeded
                }
                else
                {
                    result = changed;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ResourceObject] Failed to process scene '{scenePath}': {e.Message}");
            }
            finally
            {
                if (opened.IsValid() && opened.isLoaded)
                {
                    // Restore prior membership: a kept (unloaded) entry stays in the hierarchy; a scene that was not
                    // present is removed entirely. removeScene:false keeps the entry but unloads it.
                    EditorSceneManager.CloseScene(opened, removeScene: !isPresent);
                }
            }

            return result;
        }

        // =====================================================================
        // Path / file / scene helpers
        // =====================================================================

        /// <summary>Cheap text prefilter: does the asset file even mention the ResourceObject path field?</summary>
        public static bool FileMightContainResourceObject(string assetPath)
        {
            var text = ReadAllTextOrNull(assetPath);
            return text != null && text.Contains(FieldMarker);
        }

        /// <summary>
        /// Whether a file should be parsed/opened to look for ResourceObjects. In ForceText serialization the cheap text
        /// prefilter is authoritative; in any other serialization mode the on-disk text cannot be trusted (a binary asset
        /// would never show the marker even when it contains one), so we must always inspect to stay correct.
        /// </summary>
        public static bool ShouldInspectFile(string assetPath)
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
                return true;
            return FileMightContainResourceObject(assetPath);
        }

        /// <summary>True when the file's text contains any of the given guids - a cheap way to target only the assets a move affects.</summary>
        public static bool FileMentionsAnyGuid(string assetPath, HashSet<string> guids)
        {
            var text = ReadAllTextOrNull(assetPath);
            if (text == null)
                return false;
            foreach (var guid in guids)
            {
                if (text.Contains(guid))
                    return true;
            }

            return false;
        }

        public static bool IsUnderResources(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
                return false;
            assetPath = assetPath.Replace('\\', '/');
            return assetPath.Contains("/Resources/") || assetPath.StartsWith("Resources/", StringComparison.Ordinal);
        }

        public static bool IsAssetOwnerPath(string path)
        {
            return path.EndsWith(".prefab", StringComparison.Ordinal) || path.EndsWith(".asset", StringComparison.Ordinal);
        }

        public static bool IsScenePath(string path)
        {
            return path.EndsWith(".unity", StringComparison.Ordinal);
        }

        public static bool TryGetLoadedScene(string scenePath, out Scene scene)
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var candidate = EditorSceneManager.GetSceneAt(i);
                if (candidate.isLoaded && candidate.path == scenePath)
                {
                    scene = candidate;
                    return true;
                }
            }

            scene = default;
            return false;
        }

        private static string ReadAllTextOrNull(string assetPath)
        {
            try
            {
                return File.ReadAllText(assetPath);
            }
            catch
            {
                return null;
            }
        }
    }
}
