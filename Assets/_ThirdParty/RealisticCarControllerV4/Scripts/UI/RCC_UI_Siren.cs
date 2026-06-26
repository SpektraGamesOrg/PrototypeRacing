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
using UnityEngine.UI;

/// <summary>
/// UI button that equips a siren on the active player vehicle when clicked.
/// Each instance is configured with a siren index corresponding to a siren prefab in the siren manager.
/// </summary>
public class RCC_UI_Siren : RCC_Core {

    /// <summary>
    /// Index of the siren prefab to equip from the siren manager's available sirens.
    /// </summary>
    [Tooltip("Index of the siren prefab to equip from the siren manager's available sirens.")]
    [Min(0)]
    public int index = 0;

    /// <summary>
    /// Equips the siren at the configured index on the active player vehicle via the customization manager.
    /// </summary>
    public void Upgrade() {

        RCC_CustomizationManager handler = RCC_CustomizationManager.Instance;

        if (!handler) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the vehicle, but there is no ''RCC_CustomizationManager'' in your scene yet.");
#endif
            return;

        }

        handler.Siren(index);

    }

}
