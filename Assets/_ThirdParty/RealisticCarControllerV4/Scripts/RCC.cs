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
/// Provides an API for spawning, registering, and modifying RCC vehicles at runtime.
/// This class contains static methods for handling vehicle instantiation, player registration,
/// control state management, engine state changes, mobile controller settings, and other runtime modifications.
/// </summary>
public class RCC {

    /// <summary>
    /// Spawns an RCC vehicle prefab at a given position and rotation, and configures its control and engine state.
    /// </summary>
    /// <param name="vehiclePrefab">The vehicle prefab to be instantiated.</param>
    /// <param name="position">The spawn position.</param>
    /// <param name="rotation">The spawn rotation.</param>
    /// <param name="registerAsPlayerVehicle">Whether to register the vehicle as the player's vehicle.</param>
    /// <param name="isControllable">Whether the vehicle should be controllable.</param>
    /// <param name="isEngineRunning">Whether the vehicle's engine should start running immediately.</param>
    /// <returns>The instantiated RCC vehicle instance.</returns>
    public static RCC_CarControllerV4 SpawnRCC(RCC_CarControllerV4 vehiclePrefab, Vector3 position, Quaternion rotation, bool registerAsPlayerVehicle, bool isControllable, bool isEngineRunning) {

        RCC_CarControllerV4 spawnedRCC = (RCC_CarControllerV4)GameObject.Instantiate(vehiclePrefab, position, rotation);
        spawnedRCC.gameObject.SetActive(true);
        spawnedRCC.SetCanControl(isControllable);

        if (registerAsPlayerVehicle)
            RCC_SceneManager.Instance.RegisterPlayer(spawnedRCC);

        if (isEngineRunning)
            spawnedRCC.StartEngine(true);
        else
            spawnedRCC.KillEngine();

        return spawnedRCC;

    }

    /// <summary>
    /// Registers the specified vehicle as the player vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle to register as the player vehicle.</param>
    public static void RegisterPlayerVehicle(RCC_CarControllerV4 vehicle) {

        RCC_SceneManager.Instance.RegisterPlayer(vehicle);

    }

    /// <summary>
    /// Registers the specified vehicle as the player vehicle and sets its controllable state.
    /// </summary>
    /// <param name="vehicle">The vehicle to register as the player vehicle.</param>
    /// <param name="isControllable">Whether the vehicle should be controllable on registration.</param>
    public static void RegisterPlayerVehicle(RCC_CarControllerV4 vehicle, bool isControllable) {

        RCC_SceneManager.Instance.RegisterPlayer(vehicle, isControllable);

    }

    /// <summary>
    /// Registers the specified vehicle as the player vehicle, sets its controllable state, and configures its engine state.
    /// </summary>
    /// <param name="vehicle">The vehicle to register as the player vehicle.</param>
    /// <param name="isControllable">Whether the vehicle should be controllable on registration.</param>
    /// <param name="engineState">Whether the engine should be running on registration.</param>
    public static void RegisterPlayerVehicle(RCC_CarControllerV4 vehicle, bool isControllable, bool engineState) {

        RCC_SceneManager.Instance.RegisterPlayer(vehicle, isControllable, engineState);

    }

    /// <summary>
    /// De-registers the current player vehicle, removing its control.
    /// </summary>
    public static void DeRegisterPlayerVehicle() {

        RCC_SceneManager.Instance.DeRegisterPlayer();

    }

    /// <summary>
    /// Toggles the control state of the specified vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle to modify.</param>
    /// <param name="isControllable">Whether the vehicle should accept input.</param>
    public static void SetControl(RCC_CarControllerV4 vehicle, bool isControllable) {

        vehicle.SetCanControl(isControllable);

    }

    /// <summary>
    /// Toggles the engine state of the specified vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle whose engine to toggle.</param>
    /// <param name="engineState">True to start the engine, false to kill it.</param>
    public static void SetEngine(RCC_CarControllerV4 vehicle, bool engineState) {

        if (engineState)
            vehicle.StartEngine();
        else
            vehicle.KillEngine();

    }

    /// <summary>
    /// Sets the mobile controller type in RCC settings.
    /// </summary>
    /// <param name="mobileController">The mobile controller mode to apply to all RCC vehicles.</param>
    public static void SetMobileController(RCC_Settings.MobileController mobileController) {

        RCC_Settings.Instance.mobileController = mobileController;
#if UNITY_EDITOR
        Debug.Log("Mobile Controller has been changed to " + mobileController.ToString());
#endif

    }

    /// <summary>
    /// Placeholder for setting unit preferences (not implemented).
    /// </summary>
    public static void SetUnits() { }

    /// <summary>
    /// Placeholder for setting the automatic gear system (not implemented).
    /// </summary>
    public static void SetAutomaticGear() { }

    /// <summary>
    /// Starts or stops recording the player's vehicle movement.
    /// </summary>
    public static void StartStopRecord() {

        RCC_SceneManager.Instance.Record();

    }

    /// <summary>
    /// Starts or stops replaying the last recorded session.
    /// </summary>
    public static void StartStopReplay() {

        RCC_SceneManager.Instance.Play();

    }

    /// <summary>
    /// Stops recording or replaying the last recorded session.
    /// </summary>
    public static void StopRecordReplay() {

        RCC_SceneManager.Instance.Stop();

    }

    /// <summary>
    /// Sets the driving behavior of the RCC system based on the specified behavior index.
    /// </summary>
    /// <param name="behaviorIndex">Index into <c>RCC_Settings.behaviorTypes</c>.</param>
    public static void SetBehavior(int behaviorIndex) {

        RCC_SceneManager.Instance.SetBehavior(behaviorIndex);
#if UNITY_EDITOR
        Debug.Log("Behavior has been changed to " + behaviorIndex.ToString());
#endif

    }

    /// <summary>
    /// Changes the camera mode in the scene.
    /// </summary>
    public static void ChangeCamera() {

        RCC_SceneManager.Instance.ChangeCamera();

    }

    /// <summary>
    /// Transports the player vehicle to the specified position and rotation.
    /// </summary>
    /// <param name="position">World-space target position.</param>
    /// <param name="rotation">Target rotation.</param>
    public static void Transport(Vector3 position, Quaternion rotation) {

        RCC_SceneManager.Instance.Transport(position, rotation);

    }

    /// <summary>
    /// Transports the specified vehicle to the given position and rotation.
    /// </summary>
    /// <param name="vehicle">The vehicle to transport.</param>
    /// <param name="position">World-space target position.</param>
    /// <param name="rotation">Target rotation.</param>
    public static void Transport(RCC_CarControllerV4 vehicle, Vector3 position, Quaternion rotation) {

        RCC_SceneManager.Instance.Transport(vehicle, position, rotation);

    }

    /// <summary>
    /// Cleans all skidmarks in the current scene.
    /// </summary>
    public static void CleanSkidmarks() {

        RCC_SkidmarksManager.Instance.CleanSkidmarks();

    }

    /// <summary>
    /// Cleans skidmarks for a specific vehicle by index.
    /// </summary>
    /// <param name="index">The skidmark batch index to clear.</param>
    public static void CleanSkidmarks(int index) {

        RCC_SkidmarksManager.Instance.CleanSkidmarks(index);

    }

    /// <summary>
    /// Repairs the specified vehicle instantly.
    /// </summary>
    /// <param name="carController">The vehicle to repair.</param>
    public static void Repair(RCC_CarControllerV4 carController) {

        if (!carController)
            return;

        carController.damage.repairNow = true;

    }

    /// <summary>
    /// Repairs the player's current vehicle.
    /// </summary>
    public static void Repair() {

        RCC_CarControllerV4 carController = RCC_SceneManager.Instance.activePlayerVehicle;

        if (carController)
            carController.damage.repairNow = true;

    }

}
