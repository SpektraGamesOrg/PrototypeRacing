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
/// Upgradable speed component. Adjusts the vehicle's final drive ratio (differential) to increase top speed based on the current upgrade level and efficiency multiplier.
/// </summary>
public class RCC_Customizer_Speed : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this speed upgrade component.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Backing field for the speed upgrade level.
    /// </summary>
    private int _speedLevel = 0;

    /// <summary>
    /// Current speed level. Maximum is 5.
    /// </summary>
    public int SpeedLevel {
        get {
            return _speedLevel;
        }
        set {
            if (value <= 5)
                _speedLevel = value;
        }
    }

    /// <summary>
    /// Default final drive ratio of the vehicle before any upgrades. Set to -1 to indicate uninitialized.
    /// </summary>
    [HideInInspector] public float defSpeed = -1f;

    /// <summary>
    /// Efficiency of the upgrade.
    /// </summary>
    [Tooltip("Efficiency of the upgrade.")]
    [Range(1f, 2f)] public float efficiency = 1.1f;

    /// <summary>
    /// Initializes the speed upgrade by capturing the default final drive ratio and applying the current upgrade level.
    /// </summary>
    public void Initialize() {

        if (!CarController)
            return;

        if (defSpeed <= 0)
            defSpeed = CarController.finalRatio;

        CarController.finalRatio = Mathf.Lerp(defSpeed, defSpeed / efficiency, SpeedLevel / 5f);

    }

    /// <summary>
    /// Recalculates and applies the final drive ratio based on the current speed upgrade level.
    /// </summary>
    public void UpdateStats() {

        if (!CarController)
            return;

        if (defSpeed <= 0)
            defSpeed = CarController.finalRatio;

        CarController.finalRatio = Mathf.Lerp(defSpeed, defSpeed / efficiency, SpeedLevel / 5f);

    }

    /// <summary>
    /// Restores the final drive ratio to the original default value and resets the upgrade level to 0.
    /// </summary>
    public void Restore() {

        if (!CarController)
            return;

        SpeedLevel = 0;

        if (defSpeed <= 0)
            defSpeed = CarController.finalRatio;

        CarController.finalRatio = defSpeed;

    }

}
