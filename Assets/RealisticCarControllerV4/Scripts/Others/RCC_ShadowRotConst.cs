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
/// Locks the rotation of a shadow projector to face straight down while following the vehicle's Y-axis rotation.
/// This prevents shadow stretching and distortion when the vehicle tilts on uneven terrain.
/// </summary>
public class RCC_ShadowRotConst : RCC_Core {

    private void Update() {

        if (!CarController)
            return;

        transform.rotation = Quaternion.Euler(90f, CarController.transform.eulerAngles.y, 0f);

    }

}
