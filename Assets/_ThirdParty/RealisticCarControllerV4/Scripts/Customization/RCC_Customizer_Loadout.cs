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
/// Serializable loadout class that stores the complete customization state of a vehicle.
/// Includes paint color, upgrade levels, visual attachment indices, and tuning data.
/// Serialized to/from JSON via PlayerPrefs for save/load persistence.
/// </summary>
[System.Serializable]
public class RCC_Customizer_Loadout {

    /// <summary>
    /// Current paint color. Default white with 0 alpha indicates no paint has been applied.
    /// </summary>
    [Header("Visual Mods")]
    [Tooltip("Current paint color. Default white with 0 alpha indicates no paint has been applied.")]
    public Color paint = new Color(1f, 1f, 1f, 0f);

    /// <summary>
    /// Index of the currently equipped spoiler. -1 means no spoiler.
    /// </summary>
    [Tooltip("Index of the currently equipped spoiler. -1 means no spoiler.")]
    public int spoiler = -1;

    /// <summary>
    /// Index of the currently equipped siren. -1 means no siren.
    /// </summary>
    [Tooltip("Index of the currently equipped siren. -1 means no siren.")]
    public int siren = -1;

    /// <summary>
    /// Index of the currently equipped wheel model. -1 means default wheels.
    /// </summary>
    [Tooltip("Index of the currently equipped wheel model. -1 means default wheels.")]
    public int wheel = -1;

    /// <summary>
    /// Current engine upgrade level (0 to 5).
    /// </summary>
    [Header("Upgrade Levels")]
    [Tooltip("Current engine upgrade level (0 to 5).")]
    [Range(0, 5)]
    public int engineLevel = 0;

    /// <summary>
    /// Current handling upgrade level (0 to 5).
    /// </summary>
    [Tooltip("Current handling upgrade level (0 to 5).")]
    [Range(0, 5)]
    public int handlingLevel = 0;

    /// <summary>
    /// Current brake upgrade level (0 to 5).
    /// </summary>
    [Tooltip("Current brake upgrade level (0 to 5).")]
    [Range(0, 5)]
    public int brakeLevel = 0;

    /// <summary>
    /// Current speed upgrade level (0 to 5).
    /// </summary>
    [Tooltip("Current speed upgrade level (0 to 5).")]
    [Range(0, 5)]
    public int speedLevel = 0;

    /// <summary>
    /// Index of the front decal material. -1 means no decal.
    /// </summary>
    [Header("Decals & Neon")]
    [Tooltip("Index of the front decal material. -1 means no decal.")]
    public int decalIndexFront = -1;

    /// <summary>
    /// Index of the back decal material. -1 means no decal.
    /// </summary>
    [Tooltip("Index of the back decal material. -1 means no decal.")]
    public int decalIndexBack = -1;

    /// <summary>
    /// Index of the left side decal material. -1 means no decal.
    /// </summary>
    [Tooltip("Index of the left side decal material. -1 means no decal.")]
    public int decalIndexLeft = -1;

    /// <summary>
    /// Index of the right side decal material. -1 means no decal.
    /// </summary>
    [Tooltip("Index of the right side decal material. -1 means no decal.")]
    public int decalIndexRight = -1;

    /// <summary>
    /// Index of the neon material. -1 means no neon.
    /// </summary>
    [Tooltip("Index of the neon material. -1 means no neon.")]
    public int neonIndex = -1;

    /// <summary>
    /// Vehicle tuning data including suspension, camber, assists, and color settings.
    /// </summary>
    [Header("Tuning")]
    [Tooltip("Vehicle tuning data including suspension, camber, assists, and color settings.")]
    public RCC_CustomizationData customizationData = new RCC_CustomizationData();

    /// <summary>
    /// Updates the loadout fields from the given customization sub-manager component. Automatically detects the component type and extracts its current values.
    /// </summary>
    /// <param name="component">The customization sub-manager component to read values from.</param>
    public void UpdateLoadout(MonoBehaviour component) {

        switch (component) {

            case RCC_Customizer_WheelManager:

                RCC_Customizer_WheelManager wheelComponent = (RCC_Customizer_WheelManager)component;
                wheel = wheelComponent.wheelIndex;
                break;

            case RCC_Customizer_UpgradeManager:

                RCC_Customizer_UpgradeManager upgradeComponent = (RCC_Customizer_UpgradeManager)component;
                engineLevel = upgradeComponent.EngineLevel;
                brakeLevel = upgradeComponent.BrakeLevel;
                handlingLevel = upgradeComponent.HandlingLevel;
                speedLevel = upgradeComponent.SpeedLevel;
                break;

            case RCC_Customizer_PaintManager:

                RCC_Customizer_PaintManager paintComponent = (RCC_Customizer_PaintManager)component;
                paint = paintComponent.color;
                break;

            case RCC_Customizer_SpoilerManager:

                RCC_Customizer_SpoilerManager spoilerComponent = (RCC_Customizer_SpoilerManager)component;
                spoiler = spoilerComponent.spoilerIndex;
                break;

            case RCC_Customizer_SirenManager:

                RCC_Customizer_SirenManager sirenComponent = (RCC_Customizer_SirenManager)component;
                siren = sirenComponent.sirenIndex;
                break;

            case RCC_Customizer_CustomizationManager:

                RCC_Customizer_CustomizationManager customizationComponent = (RCC_Customizer_CustomizationManager)component;
                customizationData = customizationComponent.customizationData;
                break;

            case RCC_Customizer_DecalManager:

                RCC_Customizer_DecalManager decalManager = (RCC_Customizer_DecalManager)component;
                decalIndexFront = decalManager.index_decalFront;
                decalIndexBack = decalManager.index_decalBack;
                decalIndexLeft = decalManager.index_decalLeft;
                decalIndexRight = decalManager.index_decalRight;
                break;

            case RCC_Customizer_NeonManager:

                RCC_Customizer_NeonManager neonManager = (RCC_Customizer_NeonManager)component;
                neonIndex = neonManager.index;
                break;

        }

    }

}
