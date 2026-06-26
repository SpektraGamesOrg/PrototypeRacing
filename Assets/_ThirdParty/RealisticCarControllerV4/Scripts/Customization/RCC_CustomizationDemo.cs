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
/// Customization demo controller used in the demo scene. Manages enabling and disabling cameras and canvases when entering and exiting customization mode.
/// </summary>
public class RCC_CustomizationDemo : RCC_Core {

    /// <summary>
    /// Cached singleton instance of the customization demo controller.
    /// </summary>
    private static RCC_CustomizationDemo instance;

    /// <summary>
    /// Singleton accessor. Finds the instance in the scene if not already cached.
    /// </summary>
    public static RCC_CustomizationDemo Instance {

        get {

#if !UNITY_2022_1_OR_NEWER
            if (instance == null)
                instance = FindObjectOfType<RCC_CustomizationDemo>();
#else
            if (instance == null)
                instance = FindFirstObjectByType<RCC_CustomizationDemo>();
#endif

            return instance;

        }

    }

    /// <summary>
    /// Currently active vehicle being customized.
    /// </summary>
    private RCC_CarControllerV4 vehicle;

    /// <summary>
    /// Showroom camera used during customization mode for orbiting around the vehicle.
    /// </summary>
    [Tooltip("Showroom camera used during customization mode for orbiting around the vehicle.")]
    public RCC_ShowroomCamera showroomCamera;

    /// <summary>
    /// Main RCC camera used during normal gameplay.
    /// </summary>
    [Tooltip("Main RCC camera used during normal gameplay.")]
    public RCC_Camera RCCCamera;

    /// <summary>
    /// Dashboard UI canvas that switches between gameplay and customization display modes.
    /// </summary>
    [Tooltip("Dashboard UI canvas that switches between gameplay and customization display modes.")]
    public RCC_UI_DashboardDisplay RCCCanvas;

    /// <summary>
    /// Transform position and rotation where the vehicle is teleported for customization.
    /// </summary>
    [Tooltip("Transform position and rotation where the vehicle is teleported for customization.")]
    public Transform location;

    /// <summary>
    /// Enables customization mode for the given vehicle. Switches to showroom camera, sets UI to customization display, teleports the vehicle, and disables player control.
    /// </summary>
    /// <param name="carController">The vehicle to enter customization mode with.</param>
    public void EnableCustomization(RCC_CarControllerV4 carController) {

        vehicle = carController;

        if (RCCCamera)
            RCCCamera.gameObject.SetActive(false);

        if (showroomCamera)
            showroomCamera.gameObject.SetActive(true);

        if (RCCCanvas)
            RCCCanvas.SetDisplayType(RCC_UI_DashboardDisplay.DisplayType.Customization);

        if (location)
            RCC.Transport(vehicle, location.position, location.rotation);

        RCC.SetControl(vehicle, false);

    }

    /// <summary>
    /// Disables customization mode. Restores the main camera, sets UI back to full display, and re-enables player control.
    /// </summary>
    public void DisableCustomization() {

        if (!vehicle)
            return;

        if (RCCCamera)
            RCCCamera.gameObject.SetActive(true);

        if (showroomCamera)
            showroomCamera.gameObject.SetActive(false);

        if (RCCCanvas)
            RCCCanvas.SetDisplayType(RCC_UI_DashboardDisplay.DisplayType.Full);

        RCC.SetControl(vehicle, true);
        vehicle = null;

    }

}
