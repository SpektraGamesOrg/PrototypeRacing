//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI button that applies a decal material to the active player vehicle at a specified location.
/// The decal location and material are configured per-instance in the Inspector.
/// </summary>
public class RCC_UI_Decal : RCC_Core {

    /// <summary>
    /// Target location of the decal. 0 is front, 1 is back, 2 is left, and 3 is right.
    /// </summary>
    [Tooltip("Target location of the decal. 0 is front, 1 is back, 2 is left, and 3 is right.")]
    [Min(0)] public int location = 0;

    /// <summary>
    /// Target material.
    /// </summary>
    [Tooltip("Target material.")]
    public Material material;

    /// <summary>
    /// Applies the assigned decal material to the active player vehicle at the configured location.
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

        //  If player vehicle doesn't have the decal manager component, return.
        if (!playerVehicle.Customizer.DecalManager)
            return;

        //  Set the decal.
        playerVehicle.Customizer.DecalManager.Upgrade(location, material);

    }

    /// <summary>
    /// Sets the location of the decal. 0 = front, 1 = back, 2 = left, 3 = right.
    /// </summary>
    /// <param name="_location">The target decal location index.</param>
    public void SetLocation(int _location) {

        location = _location;

    }

}
