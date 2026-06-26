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
/// Customization trigger zone used in the customization demo scene. Enables customization mode when the player vehicle enters the trigger collider, and re-enables the trigger when the vehicle moves far enough away.
/// </summary>
public class RCC_CustomizationTrigger : RCC_Core {

    /// <summary>
    /// Visual trigger object that is shown/hidden based on vehicle proximity.
    /// </summary>
    [Tooltip("Visual trigger object that is shown/hidden based on vehicle proximity.")]
    public GameObject trigger;

    /// <summary>
    /// The vehicle that entered the trigger zone. Used to track distance for re-enabling the trigger.
    /// </summary>
    private RCC_CarControllerV4 vehicle;

    private void OnTriggerEnter(Collider other) {

        //  Getting car controller.
        RCC_CarControllerV4 carController = other.GetComponentInParent<RCC_CarControllerV4>();

        //  If trigger is not a vehicle, return.
        if (!carController)
            return;

        if (!RCC_CustomizationDemo.Instance) {

#if UNITY_EDITOR
            Debug.LogError("''RCC_CustomizationDemo'' couldn't found in the scene!");
#endif
            return;

        }

        //  Enable customization mode, disable trigger.
        RCC_CustomizationDemo.Instance.EnableCustomization(carController);

        if (trigger)
            trigger.SetActive(false);

        vehicle = carController;

    }

    private void Update() {

        //  If no any vehicle triggered, return.
        if (!trigger || trigger.activeSelf || !vehicle)
            return;

        //  Id distance is higher than 20 meters, reenable the trigger again.
        if (Vector3.Distance(transform.position, vehicle.transform.position) > 20f) {

            trigger.SetActive(true);
            vehicle = null;

        }

    }

}
