//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// UI button that changes the wheels on the active player vehicle when clicked.
/// Each instance is configured with a wheel index corresponding to a wheel model in <see cref="RCC_ChangableWheels"/>.
/// </summary>
public class RCC_UI_Wheel : RCC_Core {

    /// <summary>
    /// Index of the wheel model to apply from the changeable wheels collection.
    /// </summary>
    [Tooltip("Index of the wheel model to apply from the changeable wheels collection.")]
    [Min(0)]
    public int wheelIndex = 0;

    /// <summary>
    /// Called when the button is clicked. Applies the wheel at the configured index to the active player vehicle
    /// via the <see cref="RCC_CustomizationManager"/>.
    /// </summary>
    public void OnClick() {

        RCC_CustomizationManager handler = RCC_CustomizationManager.Instance;

        if (!handler) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the vehicle, but there is no ''RCC_CustomizationManager'' in your scene yet.");
#endif
            return;

        }

        handler.ChangeWheels(wheelIndex);

    }

}
