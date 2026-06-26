//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains references for various ground materials and their associated physics and effects.
/// This includes both PhysicMaterial-based ground settings and custom terrain settings 
/// to handle diverse surfaces (e.g., asphalt, grass, sand).
/// </summary>
[System.Serializable]
public class RCC_GroundMaterials : ScriptableObject {

    #region Singleton

    /// <summary>
    /// Cached singleton instance. Loads this ScriptableObject from "RCC Assets/RCC_GroundMaterials" if needed.
    /// </summary>
    private static RCC_GroundMaterials instance;

    /// <summary>
    /// Public accessor for retrieving the single instance of RCC_GroundMaterials.
    /// </summary>
    public static RCC_GroundMaterials Instance {
        get {
            if (instance == null)
                instance = Resources.Load("RCC Assets/RCC_GroundMaterials") as RCC_GroundMaterials;
            return instance;
        }
    }

    #endregion

    /// <summary>
    /// Holds physics and audio properties for a specific PhysicMaterial ground type.
    /// </summary>
    [System.Serializable]
    public class GroundMaterialFrictions {

        /// <summary>
        /// The PhysicMaterial representing this ground type.
        /// </summary>
        [Tooltip("The PhysicMaterial representing this ground type.")]
        public PhysicsMaterial groundMaterial;

        /// <summary>
        /// Controls the forward friction stiffness on this ground surface.
        /// Higher values produce more grip, while lower values produce more slip.
        /// </summary>
        [Tooltip("Controls the forward friction stiffness on this ground surface. Higher values produce more grip, while lower values produce more slip.")]
        [Min(0f)]
        public float forwardStiffness = 1f;

        /// <summary>
        /// Controls the sideways friction stiffness on this ground surface.
        /// </summary>
        [Tooltip("Controls the sideways friction stiffness on this ground surface.")]
        [Min(0f)]
        public float sidewaysStiffness = 1f;

        /// <summary>
        /// Target slip limit for tire traction on this ground surface.
        /// </summary>
        [Tooltip("Target slip limit for tire traction on this ground surface.")]
        [Min(0f)]
        public float slip = .25f;

        /// <summary>
        /// Damp force affecting how quickly the wheel recovers from slip or bounces on this surface.
        /// </summary>
        [Tooltip("Damp force affecting how quickly the wheel recovers from slip or bounces on this surface.")]
        [Min(0f)]
        public float damp = 1f;

        /// <summary>
        /// Volume scalar for any associated ground sound effects, in the range 0 to 1.
        /// </summary>
        [Tooltip("Volume scalar for any associated ground sound effects, in the range 0 to 1.")]
        [Range(0f, 1f)]
        public float volume = 1f;

        /// <summary>
        /// Optional particle system prefab used for dust or debris on this ground surface.
        /// </summary>
        [Tooltip("Optional particle system prefab used for dust or debris on this ground surface.")]
        public GameObject groundParticles;

        /// <summary>
        /// An AudioClip that will be played while driving or drifting on this ground surface.
        /// </summary>
        [Tooltip("An AudioClip that will be played while driving or drifting on this ground surface.")]
        public AudioClip groundSound;

        /// <summary>
        /// Custom skidmark component associated with this ground type, if any.
        /// </summary>
        [Tooltip("Custom skidmark component associated with this ground type, if any.")]
        public RCC_Skidmarks skidmark;

        /// <summary>
        /// If true, the wheel may deflate (lose pressure) on this ground surface under certain conditions.
        /// </summary>
        [Tooltip("If true, the wheel may deflate (lose pressure) on this ground surface under certain conditions.")]
        public bool deflate = false;
    }

    /// <summary>
    /// An array of ground material setups. Each entry specifies custom physics, audio, and VFX for a specific PhysicMaterial.
    /// </summary>
    [Tooltip("An array of ground material setups. Each entry specifies custom physics, audio, and VFX for a specific PhysicMaterial.")]
    public GroundMaterialFrictions[] frictions;

    /// <summary>
    /// Holds references to terrain-related ground materials and their splatmap indexes, allowing more granular 
    /// control over terrain surfaces.
    /// </summary>
    [System.Serializable]
    public class TerrainFrictions {

        /// <summary>
        /// The PhysicMaterial associated with a particular terrain texture.
        /// </summary>
        [Tooltip("The PhysicMaterial associated with a particular terrain texture.")]
        public PhysicsMaterial groundMaterial;

        /// <summary>
        /// Holds index references corresponding to the splatmap layers in a terrain data.
        /// </summary>
        [System.Serializable]
        public class SplatmapIndexes {

            /// <summary>
            /// The splatmap layer index corresponding to a specific terrain texture.
            /// </summary>
            [Tooltip("The splatmap layer index corresponding to a specific terrain texture.")]
            [Min(0)]
            public int index = 0;

        }

        /// <summary>
        /// An array of splatmap indexes indicating which terrain textures map to this terrain ground setting.
        /// </summary>
        [Tooltip("An array of splatmap indexes indicating which terrain textures map to this terrain ground setting.")]
        public SplatmapIndexes[] splatmapIndexes;

    }

    /// <summary>
    /// An array of terrain friction setups, each linking a PhysicMaterial to specific terrain splatmap layers.
    /// </summary>
    [Tooltip("An array of terrain friction setups, each linking a PhysicMaterial to specific terrain splatmap layers.")]
    public TerrainFrictions[] terrainFrictions;
}
