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
/// Spawns the last saved vehicle selection from PlayerPrefs at this transform's position and rotation on Start.
/// Used in demo scenes to carry the player's vehicle choice from a car selection scene to a gameplay scene.
/// </summary>
public class RCC_Spawner : RCC_Core {

    private void Start() {

        int selectedIndex = PlayerPrefs.GetInt("SelectedRCCVehicle", 0);

        // Clamp selected index to valid range.
        if (RCC_DemoVehicles.Instance.vehicles.Length > 0)
            selectedIndex = Mathf.Clamp(selectedIndex, 0, RCC_DemoVehicles.Instance.vehicles.Length - 1);
        else
            return;

        RCC.SpawnRCC(RCC_DemoVehicles.Instance.vehicles[selectedIndex], transform.position, transform.rotation, true, true, true);

    }

}
