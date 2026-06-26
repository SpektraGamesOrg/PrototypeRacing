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
/// Trigger-based teleporter that transports any RCC vehicle entering its zone to a designated spawn point.
/// Place a collider with isTrigger enabled on this GameObject to define the teleportation zone.
/// </summary>
public class RCC_Teleporter : RCC_Core {

    /// <summary>
    /// Target transform where vehicles will be teleported to (position and rotation).
    /// </summary>
    [Tooltip("Target transform where vehicles will be teleported to (position and rotation).")]
    public Transform spawnPoint;

    private void OnTriggerEnter(Collider col) {

        //  If trigger enabled, return.
        if (col.isTrigger)
            return;

        //  Getting car controller.
        RCC_CarControllerV4 carController = col.gameObject.GetComponentInParent<RCC_CarControllerV4>();

        //  If no car controller found, return.
        if (!carController)
            return;

        //  If no spawn point, return.
        if (!spawnPoint)
            return;

        //  Transport the vehicle.
        RCC.Transport(carController, spawnPoint.position, spawnPoint.rotation);

    }

}
