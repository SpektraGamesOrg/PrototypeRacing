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
/// Upgradable handling component. Increases traction helper strength of the parent vehicle's car controller based on the current upgrade level and efficiency multiplier.
/// </summary>
public class RCC_Customizer_Handling : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this handling upgrade component.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Backing field for the handling upgrade level.
    /// </summary>
    private int _handlingLevel = 0;

    /// <summary>
    /// Current handling upgrade level. Maximum is 5.
    /// </summary>
    public int HandlingLevel {
        get {
            return _handlingLevel;
        }
        set {
            if (value <= 5)
                _handlingLevel = value;
        }
    }

    /// <summary>
    /// Default handling strength.
    /// </summary>
    [HideInInspector] public float defHandling = -1f;

    /// <summary>
    /// Efficiency of the upgrade.
    /// </summary>
    [Tooltip("Efficiency of the upgrade.")]
    [Range(1f, 2f)] public float efficiency = 1.2f;

    /// <summary>
    /// Updates handling and initializes it.
    /// </summary>
    public void Initialize() {

        if (!CarController)
            return;

        if (defHandling <= 0)
            defHandling = CarController.tractionHelperStrength;

        CarController.tractionHelperStrength = Mathf.Lerp(defHandling, defHandling * efficiency, HandlingLevel / 5f);

    }

    /// <summary>
    /// Updates handling strength and save it.
    /// </summary>
    public void UpdateStats() {

        if (!CarController)
            return;

        if (defHandling <= 0)
            defHandling = CarController.tractionHelperStrength;

        CarController.tractionHelperStrength = Mathf.Lerp(defHandling, defHandling * efficiency, HandlingLevel / 5f);

    }

    /// <summary>
    /// Restores traction helper strength to the original default value and resets the upgrade level to 0.
    /// </summary>
    public void Restore() {

        if (!CarController)
            return;

        HandlingLevel = 0;

        if (defHandling <= 0)
            defHandling = CarController.tractionHelperStrength;

        CarController.tractionHelperStrength = defHandling;

    }

}
