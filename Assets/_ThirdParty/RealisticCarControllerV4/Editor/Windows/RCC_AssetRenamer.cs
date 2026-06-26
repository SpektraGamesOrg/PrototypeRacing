//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Renames assets with typos or inconsistent naming conventions.
/// Uses AssetDatabase.RenameAsset to preserve GUIDs and all references.
/// </summary>
public static class RCC_AssetRenamer {

    /// <summary>
    /// Stores a single rename operation (old path and new name).
    /// </summary>
    private struct RenameEntry {

        public string oldPath;
        public string newName;

        public RenameEntry(string oldPath, string newName) {

            this.oldPath = oldPath;
            this.newName = newName;

        }

    }

    private static readonly RenameEntry[] renameMap = new RenameEntry[] {

        // Typos (High Priority)
        new RenameEntry("Assets/RealisticCarControllerV4/Prefabs/Particles/RCC_SratchSparkles.prefab", "RCC_ScratchSparkles"),
        new RenameEntry("Assets/RealisticCarControllerV4/Materials/Particles/RCC_Skidmarks_ Grass.mat", "RCC_Skidmarks_Grass"),
        new RenameEntry("Assets/RealisticCarControllerV4/Materials/Particles/RCC_Skidmarks_ Sand.mat", "RCC_Skidmarks_Sand"),

        // Animation / Controller spacing (Medium Priority)
        new RenameEntry("Assets/RealisticCarControllerV4/Animators & Animations/RCC Cinematic Animation.anim", "RCC_Cinematic_Animation"),
        new RenameEntry("Assets/RealisticCarControllerV4/Animators & Animations/RCC Cinematic Animation 2.anim", "RCC_Cinematic_Animation_2"),
        new RenameEntry("Assets/RealisticCarControllerV4/Animators & Animations/RCC Cinematic Animation 3.anim", "RCC_Cinematic_Animation_3"),
        new RenameEntry("Assets/RealisticCarControllerV4/Animators & Animations/RCC Plane Animation.anim", "RCC_Plane_Animation"),
        new RenameEntry("Assets/RealisticCarControllerV4/Animators & Animations/RCC Cinematic Animator.controller", "RCC_Cinematic_Animator"),

    };

    /// <summary>
    /// Walks the rename map and renames any assets still found at their old paths, preserving GUIDs.
    /// </summary>
    [MenuItem("Tools/BoneCracker Games/Realistic Car Controller/Other/Fix Asset Names", false, 1002)]
    public static void FixAssetNames() {

        int totalCount = renameMap.Length;

        // Count how many assets actually need renaming (exist and haven't been renamed already).
        int pendingCount = 0;

        for (int i = 0; i < totalCount; i++) {

            string guid = AssetDatabase.AssetPathToGUID(renameMap[i].oldPath);

            if (!string.IsNullOrEmpty(guid))
                pendingCount++;

        }

        if (pendingCount == 0) {

            EditorUtility.DisplayDialog(
                "RCC Asset Renamer",
                "All assets are already named correctly. Nothing to do.",
                "OK");
            return;

        }

        bool confirm = EditorUtility.DisplayDialog(
            "RCC Asset Renamer",
            pendingCount + " asset(s) will be renamed to fix typos and spacing.\n\n" +
            "This uses AssetDatabase.RenameAsset which preserves GUIDs — all references remain intact.\n\n" +
            "Proceed?",
            "Rename",
            "Cancel");

        if (!confirm)
            return;

        int successCount = 0;
        int skipCount = 0;
        int failCount = 0;

        for (int i = 0; i < totalCount; i++) {

            RenameEntry entry = renameMap[i];

            // Check if the asset exists at the old path.
            string guid = AssetDatabase.AssetPathToGUID(entry.oldPath);

            if (string.IsNullOrEmpty(guid)) {

                Debug.Log("[RCC Asset Renamer] Skipped (already renamed or not found): " + entry.oldPath);
                skipCount++;
                continue;

            }

            string result = AssetDatabase.RenameAsset(entry.oldPath, entry.newName);

            if (string.IsNullOrEmpty(result)) {

                Debug.Log("[RCC Asset Renamer] Renamed: " + entry.oldPath + " -> " + entry.newName);
                successCount++;

            } else {

                Debug.LogWarning("[RCC Asset Renamer] Failed: " + entry.oldPath + " | Error: " + result);
                failCount++;

            }

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = "Rename complete.\n\n" +
            "Renamed: " + successCount + "\n" +
            "Skipped (already correct): " + skipCount + "\n" +
            "Failed: " + failCount;

        Debug.Log("[RCC Asset Renamer] " + summary.Replace("\n\n", " | ").Replace("\n", " | "));

        EditorUtility.DisplayDialog("RCC Asset Renamer", summary, "OK");

    }

}
