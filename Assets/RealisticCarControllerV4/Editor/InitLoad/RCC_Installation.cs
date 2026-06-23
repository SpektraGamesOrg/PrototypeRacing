//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Static installer that provisions RCC's required tags, layers, and physics collision matrix on first project import.
/// </summary>
public class RCC_Installation {

    /// <summary>
    /// Detects missing RCC layers (RCC_Vehicle, RCC_WheelCollider, RCC_DetachablePart, RCC_Prop) and prompts the user to add them.
    /// </summary>
    public static void Check() {

        bool layer_RCC = false;
        bool layer_RCC_WheelCollider = false;
        bool layer_RCC_DetachablePart = false;
        bool layer_RCC_Prop = false;

        string[] missingLayers = new string[4];

        layer_RCC = LayerExists("RCC_Vehicle");
        layer_RCC_WheelCollider = LayerExists("RCC_WheelCollider");
        layer_RCC_DetachablePart = LayerExists("RCC_DetachablePart");
        layer_RCC_Prop = LayerExists("RCC_Prop");

        if (!layer_RCC)
            missingLayers[0] = "RCC_Vehicle";

        if (!layer_RCC_WheelCollider)
            missingLayers[1] = "RCC_WheelCollider";

        if (!layer_RCC_DetachablePart)
            missingLayers[2] = "RCC_DetachablePart";

        if (!layer_RCC_Prop)
            missingLayers[3] = "RCC_Prop";

        if (!layer_RCC || !layer_RCC_WheelCollider || !layer_RCC_DetachablePart || !layer_RCC_Prop) {

            if (EditorUtility.DisplayDialog("Realistic Car Controller | Found Missing Layers For Realistic Car Controller", "These layers will be added to the Tags and Layers\n\n" + missingLayers[0] + "\n" + missingLayers[1] + "\n" + missingLayers[2] + "\n" + missingLayers[3], "Add")) {

                CheckLayer("RCC_Vehicle");
                CheckLayer("RCC_WheelCollider");
                CheckLayer("RCC_DetachablePart");
                CheckLayer("RCC_Prop");

            }

        }

    }

    /// <summary>
    /// Configures the physics collision matrix so RCC vehicles, wheel colliders, detachable parts, and props ignore each other where appropriate.
    /// </summary>
    public static void CollisionMatrix() {

        if (!RCC_Settings.Instance)
            return;

        bool layer_RCC;
        bool layer_RCC_WheelCollider;
        bool layer_RCC_DetachablePart;
        bool layer_RCC_Prop;

        layer_RCC = LayerExists("RCC_Vehicle");
        layer_RCC_WheelCollider = LayerExists("RCC_WheelCollider");
        layer_RCC_DetachablePart = LayerExists("RCC_DetachablePart");
        layer_RCC_Prop = LayerExists("RCC_Prop");

        if (layer_RCC && layer_RCC_DetachablePart)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer), LayerMask.NameToLayer(RCC_Settings.Instance.DetachablePartLayer), true);

        if (layer_RCC_WheelCollider && layer_RCC_DetachablePart)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(RCC_Settings.Instance.WheelColliderLayer), LayerMask.NameToLayer(RCC_Settings.Instance.DetachablePartLayer), true);

        if (layer_RCC_WheelCollider && layer_RCC_Prop)
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer(RCC_Settings.Instance.WheelColliderLayer), LayerMask.NameToLayer(RCC_Settings.Instance.PropLayer), true);

    }

    /// <summary>
    /// Ensures the given tag exists in the project's TagManager, adding it if missing.
    /// </summary>
    /// <param name="tagName">Name of the tag to verify or create.</param>
    /// <returns>True if the tag exists or was successfully added.</returns>
    public static bool CheckTag(string tagName) {

        if (TagExists(tagName))
            return true;

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        if (!PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName)) {

            int index = tagsProp.arraySize;

            tagsProp.InsertArrayElementAtIndex(index);
            SerializedProperty sp = tagsProp.GetArrayElementAtIndex(index);

            sp.stringValue = tagName;
            Debug.Log("Tag: " + tagName + " has been added.");

            tagManager.ApplyModifiedProperties();

            return true;

        }

        return false;

    }

    /// <summary>
    /// Creates the named tag if missing and returns its sanitized name, defaulting to "Untagged" when empty.
    /// </summary>
    /// <param name="name">Desired tag name.</param>
    /// <returns>The resolved tag name, or "Untagged" if the input was null/empty.</returns>
    public static string NewTag(string name) {

        CheckTag(name);

        if (name == null || name == "")
            name = "Untagged";

        return name;

    }

    /// <summary>
    /// Removes the given tag from the project's TagManager if it exists.
    /// </summary>
    /// <param name="tagName">Name of the tag to remove.</param>
    /// <returns>True if the tag was found and removed.</returns>
    public static bool RemoveTag(string tagName) {

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        if (PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName)) {

            SerializedProperty sp;

            for (int i = 0, j = tagsProp.arraySize; i < j; i++) {

                sp = tagsProp.GetArrayElementAtIndex(i);

                if (sp.stringValue == tagName) {

                    tagsProp.DeleteArrayElementAtIndex(i);
                    Debug.Log("Tag: " + tagName + " has been removed.");
                    tagManager.ApplyModifiedProperties();
                    return true;

                }

            }

        }

        return false;

    }

    /// <summary>
    /// Checks whether the given tag is already registered in the project's TagManager.
    /// </summary>
    /// <param name="tagName">Name of the tag to look up.</param>
    /// <returns>True if the tag is present in the TagManager.</returns>
    public static bool TagExists(string tagName) {

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        return PropertyExists(tagsProp, 0, tagsProp.arraySize, tagName);

    }

    /// <summary>
    /// Ensures the given layer exists in the project's TagManager, adding it to the first free user-layer slot (8-30) if missing.
    /// </summary>
    /// <param name="layerName">Name of the layer to verify or create.</param>
    /// <returns>True if the layer exists or was successfully added.</returns>
    public static bool CheckLayer(string layerName) {

        if (LayerExists(layerName))
            return true;

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        if (!PropertyExists(layersProp, 0, 31, layerName)) {

            SerializedProperty sp;

            for (int i = 8, j = 31; i < j; i++) {

                sp = layersProp.GetArrayElementAtIndex(i);

                if (sp.stringValue == "") {

                    sp.stringValue = layerName;
                    Debug.Log("Layer: " + layerName + " has been added.");
                    tagManager.ApplyModifiedProperties();
                    return true;

                }

            }

            Debug.Log("All allowed layers have been filled.");

        }

        return false;

    }

    /// <summary>
    /// Creates the named layer if missing and returns it unchanged.
    /// </summary>
    /// <param name="name">Desired layer name.</param>
    /// <returns>The layer name passed in.</returns>
    public static string NewLayer(string name) {

        if (name != null && name != "")
            CheckLayer(name);

        return name;

    }

    /// <summary>
    /// Removes the given layer from the project's TagManager if it exists.
    /// </summary>
    /// <param name="layerName">Name of the layer to remove.</param>
    /// <returns>True if the layer was found and cleared.</returns>
    public static bool RemoveLayer(string layerName) {

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        if (PropertyExists(layersProp, 0, layersProp.arraySize, layerName)) {

            SerializedProperty sp;

            for (int i = 0, j = layersProp.arraySize; i < j; i++) {

                sp = layersProp.GetArrayElementAtIndex(i);

                if (sp.stringValue == layerName) {

                    sp.stringValue = "";
                    Debug.Log("Layer: " + layerName + " has been removed.");
                    // Save settings
                    tagManager.ApplyModifiedProperties();
                    return true;

                }

            }

        }

        return false;

    }

    /// <summary>
    /// Checks whether the given layer is already registered in the project's TagManager (slots 0-30).
    /// </summary>
    /// <param name="layerName">Name of the layer to look up.</param>
    /// <returns>True if the layer is present in the TagManager.</returns>
    public static bool LayerExists(string layerName) {

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        return PropertyExists(layersProp, 0, 31, layerName);

    }

    /// <summary>
    /// Scans a SerializedProperty array between the given indices to see whether any element's string value matches.
    /// </summary>
    /// <param name="property">Serialized array property to scan.</param>
    /// <param name="start">Inclusive start index.</param>
    /// <param name="end">Exclusive end index.</param>
    /// <param name="value">String value to search for.</param>
    /// <returns>True if a matching element is found in the range.</returns>
    public static bool PropertyExists(SerializedProperty property, int start, int end, string value) {

        for (int i = start; i < end; i++) {

            SerializedProperty t = property.GetArrayElementAtIndex(i);

            if (t.stringValue.Equals(value))
                return true;

        }

        return false;

    }

    /// <summary>
    /// Reassigns layers across all RCC vehicle, detachable part, trailer, and prop prefabs in the project.
    /// </summary>
    public static void CheckAllLayers() {

        CheckAllVehicleLayers();
        CheckAllDetachablePartLayers();
        CheckAllTrailerLayers();
        CheckAllPropLayers();

    }

    /// <summary>
    /// Recursively checks a hierarchy for missing MonoBehaviours.
    /// </summary>
    private static bool HasMissingScripts(GameObject root) {

        // Unity-provided helper – returns *only* the count on this GO, not its children.
        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root) > 0)
            return true;

        foreach (Transform child in root.transform) {

            if (HasMissingScripts(child.gameObject))
                return true;

        }

        return false;

    }

    /// <summary>
    /// Scans every prefab for RCC_CarControllerV4 components and assigns the vehicle, wheel collider, and detachable part layers across their hierarchies.
    /// </summary>
    public static void CheckAllVehicleLayers() {

        List<RCC_CarControllerV4> foundPrefabs = new List<RCC_CarControllerV4>();

        bool cancelled = false;

        try {

            string[] paths = SearchByFilter("t:prefab").ToArray();
            int progress = 0;

            foreach (string path in paths) {

                if (EditorUtility.DisplayCancelableProgressBar("Searching for RCC Vehicles..", $"Scanned {progress}/{paths.Length} prefabs. Found {foundPrefabs.Count} new vehicles.", progress / (float)paths.Length)) {

                    cancelled = true;
                    break;

                }

                progress++;

                RCC_CarControllerV4 rcc = AssetDatabase.LoadAssetAtPath<RCC_CarControllerV4>(path);

                if (!rcc)
                    continue;

                var prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool broken = HasMissingScripts(prefabRoot);
                PrefabUtility.UnloadPrefabContents(prefabRoot);

                if (broken) {

                    Debug.LogWarning($"Skipped “{path}” – prefab contains missing script references.");
                    continue;

                }

                foundPrefabs.Add(rcc);

            }

        } finally {

            EditorUtility.ClearProgressBar();

            if (!cancelled) {

                List<RCC_CarControllerV4> allVehicles = new List<RCC_CarControllerV4>();

                for (int i = 0; i < foundPrefabs.Count; i++) {

                    if (!allVehicles.Contains(foundPrefabs[i]))
                        allVehicles.Add(foundPrefabs[i]);

                }

                if (RCC_Settings.Instance) {

                    foreach (RCC_CarControllerV4 target in allVehicles) {

                        if (target == null)
                            continue;

                        target.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer);

                        foreach (Transform item in target.GetComponentsInChildren<Transform>(true))
                            item.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer);

                        foreach (RCC_WheelCollider item in target.GetComponentsInChildren<RCC_WheelCollider>(true))
                            item.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.WheelColliderLayer);

                        foreach (RCC_DetachablePart item in target.GetComponentsInChildren<RCC_DetachablePart>(true))
                            item.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.DetachablePartLayer);

                    }

                }

            }

            Resources.UnloadUnusedAssets();

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CollisionMatrix();

    }

    /// <summary>
    /// Scans every prefab for RCC_DetachablePart components and assigns the detachable part layer across their hierarchies.
    /// </summary>
    public static void CheckAllDetachablePartLayers() {

        List<RCC_DetachablePart> foundPrefabs = new List<RCC_DetachablePart>();

        bool cancelled = false;

        try {

            string[] paths = SearchByFilter("t:prefab").ToArray();
            int progress = 0;

            foreach (string path in paths) {

                if (EditorUtility.DisplayCancelableProgressBar("Searching for RCC Detachable Parts..", $"Scanned {progress}/{paths.Length} prefabs. Found {foundPrefabs.Count} new detachable parts.", progress / (float)paths.Length)) {

                    cancelled = true;
                    break;

                }

                progress++;

                RCC_DetachablePart rcc = AssetDatabase.LoadAssetAtPath<RCC_DetachablePart>(path);

                if (!rcc)
                    continue;

                var prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool broken = HasMissingScripts(prefabRoot);
                PrefabUtility.UnloadPrefabContents(prefabRoot);

                if (broken) {

                    Debug.LogWarning($"Skipped “{path}” – prefab contains missing script references.");
                    continue;

                }

                foundPrefabs.Add(rcc);

            }

        } finally {

            EditorUtility.ClearProgressBar();

            if (!cancelled) {

                List<RCC_DetachablePart> allParts = new List<RCC_DetachablePart>();

                for (int i = 0; i < foundPrefabs.Count; i++) {

                    if (!allParts.Contains(foundPrefabs[i]))
                        allParts.Add(foundPrefabs[i]);

                }

                if (RCC_Settings.Instance) {

                    foreach (RCC_DetachablePart target in allParts) {

                        if (target == null)
                            continue;

                        target.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.DetachablePartLayer);

                        foreach (Transform item in target.GetComponentsInChildren<Transform>(true))
                            item.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.DetachablePartLayer);

                    }

                }

            }

            Resources.UnloadUnusedAssets();

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CollisionMatrix();

    }

    /// <summary>
    /// Scans every prefab for RCC_Prop components and assigns the prop layer across their hierarchies.
    /// </summary>
    public static void CheckAllPropLayers() {

        List<RCC_Prop> foundPrefabs = new List<RCC_Prop>();

        bool cancelled = false;

        try {

            string[] paths = SearchByFilter("t:prefab").ToArray();
            int progress = 0;

            foreach (string path in paths) {

                if (EditorUtility.DisplayCancelableProgressBar("Searching for RCC Props..", $"Scanned {progress}/{paths.Length} prefabs. Found {foundPrefabs.Count} new props.", progress / (float)paths.Length)) {

                    cancelled = true;
                    break;

                }

                progress++;

                RCC_Prop rcc = AssetDatabase.LoadAssetAtPath<RCC_Prop>(path);

                if (!rcc)
                    continue;

                var prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool broken = HasMissingScripts(prefabRoot);
                PrefabUtility.UnloadPrefabContents(prefabRoot);

                if (broken) {

                    Debug.LogWarning($"Skipped “{path}” – prefab contains missing script references.");
                    continue;

                }

                foundPrefabs.Add(rcc);

            }

        } finally {

            EditorUtility.ClearProgressBar();

            if (!cancelled) {

                List<RCC_Prop> allProps = new List<RCC_Prop>();

                for (int i = 0; i < foundPrefabs.Count; i++) {

                    if (!allProps.Contains(foundPrefabs[i]))
                        allProps.Add(foundPrefabs[i]);

                }

                if (RCC_Settings.Instance) {

                    foreach (RCC_Prop target in allProps) {

                        if (target == null)
                            continue;

                        target.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.PropLayer);

                        foreach (Transform item in target.GetComponentsInChildren<Transform>(true))
                            item.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.PropLayer);

                    }

                }

            }

            Resources.UnloadUnusedAssets();

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CollisionMatrix();

    }

    /// <summary>
    /// Scans every prefab for RCC_TruckTrailer components and assigns the vehicle layer across their hierarchies.
    /// </summary>
    public static void CheckAllTrailerLayers() {

        List<RCC_TruckTrailer> foundPrefabs = new List<RCC_TruckTrailer>();

        bool cancelled = false;

        try {

            string[] paths = SearchByFilter("t:prefab").ToArray();
            int progress = 0;

            foreach (string path in paths) {

                if (EditorUtility.DisplayCancelableProgressBar("Searching for RCC Trailers..", $"Scanned {progress}/{paths.Length} prefabs. Found {foundPrefabs.Count} new trailers.", progress / (float)paths.Length)) {

                    cancelled = true;
                    break;

                }

                progress++;

                RCC_TruckTrailer rcc = AssetDatabase.LoadAssetAtPath<RCC_TruckTrailer>(path);

                if (!rcc)
                    continue;

                var prefabRoot = PrefabUtility.LoadPrefabContents(path);
                bool broken = HasMissingScripts(prefabRoot);
                PrefabUtility.UnloadPrefabContents(prefabRoot);

                if (broken) {

                    Debug.LogWarning($"Skipped “{path}” – prefab contains missing script references.");
                    continue;

                }

                foundPrefabs.Add(rcc);

            }

        } finally {

            EditorUtility.ClearProgressBar();

            if (!cancelled) {

                List<RCC_TruckTrailer> allTrailers = new List<RCC_TruckTrailer>();

                for (int i = 0; i < foundPrefabs.Count; i++) {

                    if (!allTrailers.Contains(foundPrefabs[i]))
                        allTrailers.Add(foundPrefabs[i]);

                }

                if (RCC_Settings.Instance) {

                    foreach (RCC_TruckTrailer target in allTrailers) {

                        if (target == null)
                            continue;

                        target.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer);

                        foreach (Transform item in target.GetComponentsInChildren<Transform>(true))
                            item.gameObject.layer = LayerMask.NameToLayer(RCC_Settings.Instance.RCCLayer);

                    }

                }

            }

            Resources.UnloadUnusedAssets();

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        CollisionMatrix();

    }

    /// <summary>
    /// Streams asset paths matching the given AssetDatabase search filter.
    /// </summary>
    /// <param name="filter">AssetDatabase search filter (e.g. "t:prefab").</param>
    public static IEnumerable<string> SearchByFilter(string filter) {

        foreach (string guid in AssetDatabase.FindAssets(filter))
            yield return AssetDatabase.GUIDToAssetPath(guid);

    }

}
