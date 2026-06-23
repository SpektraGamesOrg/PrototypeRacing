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

/// <summary>
/// ScriptableObject singleton that holds prefab references for all customization sub-manager setups.
/// Used by the editor to automatically create customization components on vehicles.
/// Loaded from Resources via "RCC Assets/RCC_CustomizationSetups".
/// </summary>
public class RCC_CustomizationSetups : ScriptableObject {

    #region singleton

    /// <summary>
    /// Cached singleton instance.
    /// </summary>
    private static RCC_CustomizationSetups instance;

    /// <summary>
    /// Singleton accessor. Loads from Resources if not already cached.
    /// </summary>
    public static RCC_CustomizationSetups Instance { get { if (instance == null) instance = Resources.Load("RCC Assets/RCC_CustomizationSetups") as RCC_CustomizationSetups; return instance; } }

    #endregion

    /// <summary>
    /// Prefab for the main customization manager sub-component (suspension, camber, assists).
    /// </summary>
    [Tooltip("Prefab for the main customization manager sub-component (suspension, camber, assists).")]
    public GameObject customization;

    /// <summary>
    /// Prefab for the decal manager sub-component.
    /// </summary>
    [Tooltip("Prefab for the decal manager sub-component.")]
    public GameObject decals;

    /// <summary>
    /// Prefab for the neon manager sub-component.
    /// </summary>
    [Tooltip("Prefab for the neon manager sub-component.")]
    public GameObject neons;

    /// <summary>
    /// Prefab for the paint manager sub-component.
    /// </summary>
    [Tooltip("Prefab for the paint manager sub-component.")]
    public GameObject paints;

    /// <summary>
    /// Prefab for the siren manager sub-component.
    /// </summary>
    [Tooltip("Prefab for the siren manager sub-component.")]
    public GameObject sirens;

    /// <summary>
    /// Prefab for the spoiler manager sub-component.
    /// </summary>
    [Tooltip("Prefab for the spoiler manager sub-component.")]
    public GameObject spoilers;

    /// <summary>
    /// Prefab for the upgrade manager sub-component (engine, brake, handling, speed).
    /// </summary>
    [Tooltip("Prefab for the upgrade manager sub-component (engine, brake, handling, speed).")]
    public GameObject upgrades;

    /// <summary>
    /// Prefab for the wheel manager sub-component.
    /// </summary>
    [Tooltip("Prefab for the wheel manager sub-component.")]
    public GameObject wheels;

}
