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
using System.IO;

/// <summary>
/// Utility for locating the root folder of the Realistic Car Controller asset,
/// even if the user renames or moves the package folder.
/// </summary>
public static class RCC_AssetUtilities {

    /// <summary>
    /// Cached project-relative path to this asset root (e.g. "Assets/Realistic Car Controller/").
    /// </summary>
    private static string basePath;

    /// <summary>
    /// Returns the project-relative root folder of this asset, with a trailing slash.
    /// e.g. "Assets/Realistic Car Controller/"
    /// </summary>
    public static string BasePath {
        get {
            if (string.IsNullOrEmpty(basePath)) {
                string scriptPath = GetScriptPath();

                if (string.IsNullOrEmpty(scriptPath)) {
                    basePath = "Assets/RealisticCarControllerV4/";
                    return basePath;
                }

                string scriptFolder = Path.GetDirectoryName(scriptPath);

                if (string.IsNullOrEmpty(scriptFolder)) {
                    basePath = "Assets/RealisticCarControllerV4/";
                    return basePath;
                }

                // if this script lives under an 'Editor' subfolder, go up one level
                if (Path.GetFileName(scriptFolder) == "Editor") {
                    string parentFolder = Path.GetDirectoryName(scriptFolder);
                    if (!string.IsNullOrEmpty(parentFolder))
                        scriptFolder = parentFolder;
                }

                // normalize separators and ensure trailing slash
                basePath = scriptFolder.Replace("\\", "/") + "/";
            }
            return basePath;
        }
    }

    /// <summary>
    /// Finds this script's own .cs file via AssetDatabase.FindAssets.
    /// Falls back to an empty string (and logs an error) if not found.
    /// </summary>
    private static string GetScriptPath() {
        // look for any MonoScript asset named RCC_AssetUtilities
        string[] guids = AssetDatabase.FindAssets("t:MonoScript RCC_AssetUtilities");

        if (guids != null && guids.Length > 0) {
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        Debug.LogError("RCC_AssetUtilities: Failed to locate script via AssetDatabase.FindAssets");
        return string.Empty;
    }

}
