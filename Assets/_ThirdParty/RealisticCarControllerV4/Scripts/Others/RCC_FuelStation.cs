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
/// Represents a fuel station area. When a vehicle with RCC_CarControllerV4 enters this trigger,
/// it gradually refills the vehicle's fuel tank at the specified rate.
/// </summary>
public class RCC_FuelStation : RCC_Core {

    /// <summary>
    /// Reference to the vehicle currently within the fuel station trigger.
    /// </summary>
    private RCC_CarControllerV4 targetVehicle;

    /// <summary>
    /// The rate (units per second) at which the vehicle's fuel tank is replenished.
    /// </summary>
    [Tooltip("The rate (units per second) at which the vehicle's fuel tank is replenished.")]
    [Min(0f)]
    public float refillSpeed = 1f;

    /// <summary>
    /// An optional GameObject (e.g., UI text) indicating the fuel station, which 
    /// will be oriented toward the main camera each frame for readability.
    /// </summary>
    [Tooltip("An optional GameObject (e.g., UI text) indicating the fuel station, which will be oriented toward the main camera each frame for readability.")]
    public GameObject text;

    /// <summary>
    /// Cached reference to the main camera, used to orient the text label toward the viewer.
    /// </summary>
    private Camera mCamera;

    /// <summary>
    /// Initializes the camera reference and subscribes to main camera change events.
    /// </summary>
    void Awake() {

        mCamera = RCC_MainCameraProvider.MainCamera;
        RCC_MainCameraProvider.OnMainCameraChanged += HandleMainCameraChanged;

    }

    /// <summary>
    /// Unsubscribes from main camera change events to prevent memory leaks.
    /// </summary>
    void OnDestroy() {

        RCC_MainCameraProvider.OnMainCameraChanged -= HandleMainCameraChanged;

    }

    /// <summary>
    /// Callback invoked when the main camera changes. Updates the cached camera reference.
    /// </summary>
    /// <param name="cam">The new main camera.</param>
    private void HandleMainCameraChanged(Camera cam) {

        mCamera = cam;

    }

    /// <summary>
    /// Detects and stores a reference to any vehicle entering the fuel station trigger.
    /// </summary>
    /// <param name="col">The collider that enters the trigger.</param>
    private void OnTriggerStay(Collider col) {

        targetVehicle = col.gameObject.GetComponentInParent<RCC_CarControllerV4>();

    }

    private void Update() {

        // Orients the optional text object to face the camera if present.
        if (text && mCamera)
            text.transform.rotation = mCamera.transform.rotation;

        // If there's no vehicle inside the trigger, do nothing.
        if (!targetVehicle)
            return;

        // Gradually refill the vehicle's fuel tank at the specified rate.
        targetVehicle.fuelTank += refillSpeed * Time.deltaTime;

    }

    /// <summary>
    /// Clears the reference to the vehicle when it leaves the fuel station trigger.
    /// </summary>
    /// <param name="col">The collider that exits the trigger.</param>
    private void OnTriggerExit(Collider col) {

        if (col.gameObject.GetComponentInParent<RCC_CarControllerV4>())
            targetVehicle = null;

    }

}
