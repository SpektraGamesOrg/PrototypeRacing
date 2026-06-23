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
/// Trigger-based speed limiter that slows vehicles by increasing their Rigidbody drag while inside the trigger zone.
/// Restores original drag when the vehicle exits the zone.
/// </summary>
public class RCC_SpeedLimiter : RCC_Core {

    /// <summary>
    /// Stores each vehicle's original drag value before modification, keyed by instance ID.
    /// </summary>
    private Dictionary<int, float> defaultDrags = new Dictionary<int, float>();

    private void OnTriggerStay(Collider other) {

        RCC_CarControllerV4 carController = other.GetComponentInParent<RCC_CarControllerV4>();

        if (!carController)
            return;

        int id = carController.GetInstanceID();

        if (!defaultDrags.ContainsKey(id))
            defaultDrags[id] = carController.Rigid.linearDamping;

        carController.Rigid.linearDamping = .02f * carController.speed;

    }

    private void OnTriggerExit(Collider other) {

        RCC_CarControllerV4 carController = other.GetComponentInParent<RCC_CarControllerV4>();

        if (!carController)
            return;

        int id = carController.GetInstanceID();

        if (defaultDrags.ContainsKey(id)) {
            carController.Rigid.linearDamping = defaultDrags[id];
            defaultDrags.Remove(id);
        }

    }

}
