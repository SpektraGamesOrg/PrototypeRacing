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
/// UI button that applies a neon light material to the active player vehicle's neon manager when clicked.
/// </summary>
public class RCC_UI_Neon : RCC_Core {

    /// <summary>
    /// The neon material to apply to the vehicle's underbody neon lights.
    /// </summary>
    [Tooltip("The neon material to apply to the vehicle's underbody neon lights.")]
    public Material material;

    /// <summary>
    /// Applies the assigned neon material to the active player vehicle via the neon manager.
    /// </summary>
    public void Upgrade() {

        //  If no material assigned, return.
        if (!material)
            return;

        //  Finding the player vehicle.
        RCC_CarControllerV4 playerVehicle = RCCSceneManager.activePlayerVehicle;

        //  If no player vehicle found, return.
        if (!playerVehicle)
            return;

        //  If player vehicle doesn't have the customizer component, return.
        if (!playerVehicle.Customizer)
            return;

        //  If player vehicle doesn't have the neon manager component, return.
        if (!playerVehicle.Customizer.NeonManager)
            return;

        //  Set the neon.
        playerVehicle.Customizer.NeonManager.Upgrade(material);

    }

}
