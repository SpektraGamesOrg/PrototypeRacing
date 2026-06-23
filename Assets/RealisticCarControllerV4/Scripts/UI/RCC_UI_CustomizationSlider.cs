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
using System.Collections.Generic;
using TMPro;

/// <summary>
/// UI slider for vehicle suspension and camber customization. Each instance controls a specific
/// customization parameter (front/rear cambers, spring force, distance, damper, or target position)
/// and syncs its value with the vehicle's customization data on enable and on slider change.
/// </summary>
public class RCC_UI_CustomizationSlider : RCC_Core {

    /// <summary>
    /// Target customization class for this slider.
    /// </summary>
    [Tooltip("Target customization class for this slider.")]
    public CustomizationClass customizationClass = CustomizationClass.Cambers_Front;
    /// <summary>
    /// Defines the available suspension and camber customization parameters that a slider can control.
    /// </summary>
    public enum CustomizationClass { Cambers_Front, Cambers_Rear, SuspensionSprings_Front, SuspensionSprings_Rear, SuspensionDistances_Front, SuspensionDistances_Rear, SuspensionDampers_Front, SuspensionDampers_Rear, SuspensionTargets_Front, SuspensionTargets_Rear }

    /// <summary>
    /// Reference to the UI Slider component on this GameObject.
    /// </summary>
    private Slider slider;

    /// <summary>
    /// Optional text label that displays the current numeric value of the slider.
    /// </summary>
    [Tooltip("Optional text label that displays the current numeric value of the slider.")]
    public TextMeshProUGUI sliderValue;

    private void Awake() {

        slider = GetComponent<Slider>();

    }

    private void OnEnable() {

        if (!slider)
            return;

        //  Finding the player vehicle.
        RCC_CarControllerV4 playerVehicle = RCCSceneManager.activePlayerVehicle;

        //  If no player vehicle found, return.
        if (!playerVehicle)
            return;

        //  If player vehicle doesn't have the customizer component, return.
        if (!playerVehicle.Customizer)
            return;

        RCC_Customizer_Loadout loadout = playerVehicle.Customizer.GetLoadout();

        //  If no loadout found, return.
        if (loadout == null)
            return;

        RCC_CustomizationData customizationData = loadout.customizationData;

        switch (customizationClass) {

            case CustomizationClass.Cambers_Front:
                slider.SetValueWithoutNotify(customizationData.cambersFront);
                break;

            case CustomizationClass.Cambers_Rear:
                slider.SetValueWithoutNotify(customizationData.cambersRear);
                break;

            case CustomizationClass.SuspensionSprings_Front:
                slider.SetValueWithoutNotify(customizationData.suspensionSpringForceFront);
                break;

            case CustomizationClass.SuspensionSprings_Rear:
                slider.SetValueWithoutNotify(customizationData.suspensionSpringForceRear);
                break;

            case CustomizationClass.SuspensionDistances_Front:
                slider.SetValueWithoutNotify(customizationData.suspensionDistanceFront);
                break;

            case CustomizationClass.SuspensionDistances_Rear:
                slider.SetValueWithoutNotify(customizationData.suspensionDistanceRear);
                break;

            case CustomizationClass.SuspensionDampers_Front:
                slider.SetValueWithoutNotify(customizationData.suspensionDamperFront);
                break;

            case CustomizationClass.SuspensionDampers_Rear:
                slider.SetValueWithoutNotify(customizationData.suspensionDamperRear);
                break;

            case CustomizationClass.SuspensionTargets_Front:
                slider.SetValueWithoutNotify(customizationData.suspensionTargetFront);
                break;

            case CustomizationClass.SuspensionTargets_Rear:
                slider.SetValueWithoutNotify(customizationData.suspensionTargetRear);
                break;

        }

        if (sliderValue)
            sliderValue.text = slider.value.ToString("F0");

    }

    /// <summary>
    /// Called when the slider value changes. Applies the new value to the corresponding
    /// customization parameter on the active player vehicle.
    /// </summary>
    public void OnSlider() {

        //  Finding the player vehicle.
        RCC_CarControllerV4 playerVehicle = RCCSceneManager.activePlayerVehicle;

        //  If no player vehicle found, return.
        if (!playerVehicle)
            return;

        //  If player vehicle doesn't have the customizer component, return.
        if (!playerVehicle.Customizer)
            return;

        if (!playerVehicle.Customizer.CustomizationManager)
            return;

        switch (customizationClass) {

            case CustomizationClass.Cambers_Front:
                playerVehicle.Customizer.CustomizationManager.SetFrontCambers(slider.value);
                break;

            case CustomizationClass.Cambers_Rear:
                playerVehicle.Customizer.CustomizationManager.SetRearCambers(slider.value);
                break;

            case CustomizationClass.SuspensionSprings_Front:
                playerVehicle.Customizer.CustomizationManager.SetFrontSuspensionsSpringForce(slider.value);
                break;

            case CustomizationClass.SuspensionSprings_Rear:
                playerVehicle.Customizer.CustomizationManager.SetRearSuspensionsSpringForce(slider.value);
                break;

            case CustomizationClass.SuspensionDistances_Front:
                playerVehicle.Customizer.CustomizationManager.SetFrontSuspensionsDistances(slider.value);
                break;

            case CustomizationClass.SuspensionDistances_Rear:
                playerVehicle.Customizer.CustomizationManager.SetRearSuspensionsDistances(slider.value);
                break;

            case CustomizationClass.SuspensionDampers_Front:
                playerVehicle.Customizer.CustomizationManager.SetFrontSuspensionsSpringDamper(slider.value);
                break;

            case CustomizationClass.SuspensionDampers_Rear:
                playerVehicle.Customizer.CustomizationManager.SetRearSuspensionsSpringDamper(slider.value);
                break;

            case CustomizationClass.SuspensionTargets_Front:
                playerVehicle.Customizer.CustomizationManager.SetFrontSuspensionsTargetPos(slider.value);
                break;

            case CustomizationClass.SuspensionTargets_Rear:
                playerVehicle.Customizer.CustomizationManager.SetRearSuspensionsTargetPos(slider.value);
                break;

        }

        if (sliderValue)
            sliderValue.text = slider.value.ToString("F0");

    }

}
