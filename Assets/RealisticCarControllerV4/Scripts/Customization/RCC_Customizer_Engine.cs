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
/// Upgradable engine component. Increases engine torque of the parent vehicle's car controller based on the current upgrade level and efficiency multiplier.
/// </summary>
public class RCC_Customizer_Engine : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this engine upgrade component.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Backing field for the engine upgrade level.
    /// </summary>
    private int _engineLevel = 0;

    /// <summary>
    /// Current engine level. Maximum is 5.
    /// </summary>
    public int EngineLevel {
        get {
            return _engineLevel;
        }
        set {
            if (value <= 5)
                _engineLevel = value;
        }
    }

    /// <summary>
    /// Default engine torque.
    /// </summary>
    [HideInInspector] public float defEngine = -1f;

    /// <summary>
    /// Efficiency of the upgrade.
    /// </summary>
    [Tooltip("Efficiency of the upgrade.")]
    [Range(1f, 2f)] public float efficiency = 1.2f;

    /// <summary>
    /// Updates engine torque and initializes it.
    /// </summary>
    public void Initialize() {

        if (!CarController)
            return;

        if (defEngine <= 0)
            defEngine = CarController.maxEngineTorque;

        CarController.maxEngineTorque = Mathf.Lerp(defEngine, defEngine * efficiency, EngineLevel / 5f);

    }

    /// <summary>
    /// Updates engine torque and save it.
    /// </summary>
    public void UpdateStats() {

        if (!CarController)
            return;

        if (defEngine <= 0)
            defEngine = CarController.maxEngineTorque;

        CarController.maxEngineTorque = Mathf.Lerp(defEngine, defEngine * efficiency, EngineLevel / 5f);

    }

    /// <summary>
    /// Restores engine torque to the original default value and resets the upgrade level to 0.
    /// </summary>
    public void Restore() {

        if (!CarController)
            return;

        EngineLevel = 0;

        if (defEngine <= 0)
            defEngine = CarController.maxEngineTorque;

        CarController.maxEngineTorque = defEngine;

    }

}
