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
/// Upgradable brake component. Increases brake torque of the parent vehicle's car controller based on the current upgrade level and efficiency multiplier.
/// </summary>
public class RCC_Customizer_Brake : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this brake upgrade component.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Backing field for the brake upgrade level.
    /// </summary>
    private int _brakeLevel = 0;

    /// <summary>
    /// Current brake level. Maximum is 5.
    /// </summary>
    public int BrakeLevel {
        get {
            return _brakeLevel;
        }
        set {
            if (value <= 5)
                _brakeLevel = value;
        }
    }

    /// <summary>
    /// Default brake torque of the vehicle.
    /// </summary>
    [HideInInspector] public float defBrake = -1f;

    /// <summary>
    /// Efficiency of the upgrade.
    /// </summary>
    [Tooltip("Efficiency of the upgrade.")]
    [Range(1f, 2f)] public float efficiency = 1.2f;

    /// <summary>
    /// Updates brake torque and initializes it.
    /// </summary>
    public void Initialize() {

        if (!CarController)
            return;

        if (defBrake <= 0)
            defBrake = CarController.brakeTorque;

        CarController.brakeTorque = Mathf.Lerp(defBrake, defBrake * efficiency, BrakeLevel / 5f);

    }

    /// <summary>
    /// Updates brake torque and save it.
    /// </summary>
    public void UpdateStats() {

        if (!CarController)
            return;

        if (defBrake <= 0)
            defBrake = CarController.brakeTorque;

        CarController.brakeTorque = Mathf.Lerp(defBrake, defBrake * efficiency, BrakeLevel / 5f);

    }

    /// <summary>
    /// Restores brake torque to the original default value and resets the upgrade level to 0.
    /// </summary>
    public void Restore() {

        if (!CarController)
            return;

        BrakeLevel = 0;

        if (defBrake <= 0)
            defBrake = CarController.brakeTorque;

        CarController.brakeTorque = defBrake;

    }

}
