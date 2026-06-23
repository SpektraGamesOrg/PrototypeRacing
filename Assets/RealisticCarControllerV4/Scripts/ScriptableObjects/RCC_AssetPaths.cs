//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ScriptableObject that stores asset references, project paths, and external links used by the RCC editor tools and setup wizards.
/// Loaded as a singleton from "Resources/RCC Assets/RCC_AssetPaths".
/// </summary>
public class RCC_AssetPaths : ScriptableObject {

    #region singleton

    /// <summary>
    /// Cached singleton instance.
    /// </summary>
    private static RCC_AssetPaths instance;

    /// <summary>
    /// Singleton accessor. Loads the asset from "Resources/RCC Assets/RCC_AssetPaths" on first access.
    /// </summary>
    public static RCC_AssetPaths Instance { get { if (instance == null) instance = Resources.Load("RCC Assets/RCC_AssetPaths") as RCC_AssetPaths; return instance; } }

    #endregion

    /// <summary>
    /// Reference to the default resources package that ships with RCC.
    /// </summary>
    [Tooltip("Reference to the default resources package that ships with RCC.")]
    public Object importDefaultResources;

    /// <summary>
    /// Reference to the demo resources package containing demo scenes and assets.
    /// </summary>
    [Tooltip("Reference to the demo resources package containing demo scenes and assets.")]
    public Object importDemoResources;

    /// <summary>
    /// URL to the RCC Asset Store product page.
    /// </summary>
    public const string assetStorePath = "https://assetstore.unity.com/packages/tools/physics/realistic-car-controller-16296#content";

    /// <summary>
    /// URL to the Photon PUN 2 Asset Store page (networking addon dependency).
    /// </summary>
    public const string photonPUN2 = "https://assetstore.unity.com/packages/tools/network/pun-2-free-119922";

    /// <summary>
    /// URL to the ProFlares Asset Store page (lens flare addon dependency).
    /// </summary>
    public const string proFlares = "https://assetstore.unity.com/packages/tools/particles-effects/proflares-ultimate-lens-flares-for-unity3d-12845";

    /// <summary>
    /// URL to the RCC YouTube tutorial playlist.
    /// </summary>
    public const string YTVideos = "https://www.youtube.com/playlist?list=PLRXTqAVrLDpoW58lKf8XA1AWD6kDkoKb1";

    /// <summary>
    /// URL to the BoneCracker Games publisher page on the Asset Store.
    /// </summary>
    public const string otherAssets = "https://assetstore.unity.com/publishers/5425";

    /// <summary>
    /// Reference to the BCG Shared Assets addon package.
    /// </summary>
    [Tooltip("Reference to the BCG Shared Assets addon package.")]
    [Space()]
    public Object addon_BCGSharedAssets;

    /// <summary>
    /// Reference to the Photon PUN 2 addon integration package.
    /// </summary>
    [Tooltip("Reference to the Photon PUN 2 addon integration package.")]
    public Object addon_PhotonPUN2;

    /// <summary>
    /// Reference to the ProFlare addon integration package.
    /// </summary>
    [Tooltip("Reference to the ProFlare addon integration package.")]
    public Object addon_ProFlare;

    /// <summary>
    /// Reference to the Input Action Map asset used by the new Unity Input System.
    /// </summary>
    [Tooltip("Reference to the Input Action Map asset used by the new Unity Input System.")]
    public Object inputActionMap;

    /// <summary>
    /// Reference to the All-In-One demo scene asset.
    /// </summary>
    [Tooltip("Reference to the All-In-One demo scene asset.")]
    [Space()]
    public Object demo_AIO;

    /// <summary>
    /// Reference to the City demo scene asset.
    /// </summary>
    [Tooltip("Reference to the City demo scene asset.")]
    public Object demo_City;

    /// <summary>
    /// Reference to the Car Selection demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Car Selection demo scene asset.")]
    public Object demo_CarSelection;

    /// <summary>
    /// Reference to the Car Selection (Load Next Scene) demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Car Selection (Load Next Scene) demo scene asset.")]
    public Object demo_CarSelectionLoadNextScene;

    /// <summary>
    /// Reference to the Car Selection (Loaded Scene) demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Car Selection (Loaded Scene) demo scene asset.")]
    public Object demo_CarSelectionLoadedScene;

    /// <summary>
    /// Reference to the Override Inputs demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Override Inputs demo scene asset.")]
    public Object demo_OverrideInputs;

    /// <summary>
    /// Reference to the Customization demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Customization demo scene asset.")]
    public Object demo_Customization;

    /// <summary>
    /// Reference to the API Blank demo scene asset.
    /// </summary>
    [Tooltip("Reference to the API Blank demo scene asset.")]
    public Object demo_APIBlank;

    /// <summary>
    /// Reference to the Blank Mobile demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Blank Mobile demo scene asset.")]
    public Object demo_BlankMobile;

    /// <summary>
    /// Reference to the Damage demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Damage demo scene asset.")]
    public Object demo_Damage;

    /// <summary>
    /// Reference to the Multiple Terrain demo scene asset.
    /// </summary>
    [Tooltip("Reference to the Multiple Terrain demo scene asset.")]
    public Object demo_MultipleTerrain;

    /// <summary>
    /// Reference to the City FPS demo scene asset.
    /// </summary>
    [Tooltip("Reference to the City FPS demo scene asset.")]
    public Object demo_CityFPS;

    /// <summary>
    /// Reference to the City TPS demo scene asset.
    /// </summary>
    [Tooltip("Reference to the City TPS demo scene asset.")]
    public Object demo_CityTPS;

    /// <summary>
    /// Reference to the PUN 2 Lobby demo scene asset.
    /// </summary>
    [Tooltip("Reference to the PUN 2 Lobby demo scene asset.")]
    public Object demo_PUN2Lobby;

    /// <summary>
    /// Reference to the PUN 2 City demo scene asset.
    /// </summary>
    [Tooltip("Reference to the PUN 2 City demo scene asset.")]
    public Object demo_PUN2City;

    /// <summary>
    /// Reference to the Built-in Render Pipeline shader import package.
    /// </summary>
    [Tooltip("Reference to the Built-in Render Pipeline shader import package.")]
    [Space()]
    public Object importBuiltinShaders;

    /// <summary>
    /// Reference to the Universal Render Pipeline (URP) shader import package.
    /// </summary>
    [Tooltip("Reference to the Universal Render Pipeline (URP) shader import package.")]
    public Object importURPShaders;

    /// <summary>
    /// Reference to the High Definition Render Pipeline (HDRP) shader import package.
    /// </summary>
    [Tooltip("Reference to the High Definition Render Pipeline (HDRP) shader import package.")]
    public Object importHDRPShaders;

    /// <summary>
    /// Reference to the HDRP Volume Profile import package.
    /// </summary>
    [Tooltip("Reference to the HDRP Volume Profile import package.")]
    public Object importHDRPVolumeProfile;

    /// <summary>
    /// Reference to the Built-in Render Pipeline shader content folder.
    /// </summary>
    [Tooltip("Reference to the Built-in Render Pipeline shader content folder.")]
    [Space()]
    public Object builtinShadersContent;

    /// <summary>
    /// Reference to the URP shader content folder.
    /// </summary>
    [Tooltip("Reference to the URP shader content folder.")]
    public Object URPShadersContent;

    /// <summary>
    /// Reference to the HDRP shader content folder.
    /// </summary>
    [Tooltip("Reference to the HDRP shader content folder.")]
    public Object HDRPShadersContent;

    /// <summary>
    /// Array of demo content assets that can be deleted when stripping demo resources from the project.
    /// </summary>
    [Tooltip("Array of demo content assets that can be deleted when stripping demo resources from the project.")]
    [Space()]
    public Object[] demoContentToDelete;

#if UNITY_EDITOR
    /// <summary>
    /// Returns the asset path of the given Unity Object using AssetDatabase.
    /// </summary>
    /// <param name="objectField">The Unity Object whose asset path is requested.</param>
    /// <returns>The asset path string for the given object.</returns>
    public string GetPath(Object objectField) {

        string objectPath = AssetDatabase.GetAssetPath(objectField);
        return objectPath;

    }
#endif

}
