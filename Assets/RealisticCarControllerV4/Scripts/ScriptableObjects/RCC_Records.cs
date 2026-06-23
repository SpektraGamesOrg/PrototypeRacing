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
/// ScriptableObject that stores a collection of recorded vehicle replay clips.
/// Each clip captures vehicle transform and input data over time for playback via <see cref="RCC_Recorder"/>.
/// Loaded as a singleton from "Resources/RCC Assets/RCC_Records".
/// </summary>
public class RCC_Records : ScriptableObject {

    #region singleton

    /// <summary>
    /// Cached singleton instance.
    /// </summary>
    private static RCC_Records instance;

    /// <summary>
    /// Singleton accessor. Loads the asset from "Resources/RCC Assets/RCC_Records" on first access.
    /// </summary>
    public static RCC_Records Instance { get { if (instance == null) instance = Resources.Load("RCC Assets/RCC_Records") as RCC_Records; return instance; } }

    #endregion

    /// <summary>
    /// List of all saved recorded clips. Each clip contains frame-by-frame vehicle position, rotation, and input data.
    /// </summary>
    [Tooltip("List of all saved recorded clips. Each clip contains frame-by-frame vehicle position, rotation, and input data.")]
    public List<RCC_Recorder.RecordedClip> records = new List<RCC_Recorder.RecordedClip>();

}
