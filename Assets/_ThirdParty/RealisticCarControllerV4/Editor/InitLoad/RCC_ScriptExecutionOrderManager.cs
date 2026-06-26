//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Manages script execution order for RCC scripts. Automatically applies the correct order on import
/// so the values travel with the package (Project Settings is not included in the .unitypackage).
/// </summary>
[InitializeOnLoad]
public class RCC_ScriptExecutionOrderManager {

    /// <summary>
    /// Script execution order definitions. Negative values execute earlier, positive values execute later.
    ///
    /// -1000: RCC_SceneManager - central singleton tracking the active player vehicle, camera, UI,
    ///        and every spawned RCC vehicle. Must initialize before any vehicle's Awake/Start tries
    ///        to register itself via RCC_SceneManager.RegisterPlayer().
    ///
    ///   -50: Singleton pre-creation - RCC_InputManager, RCC_SkidmarksManager. Both are first-touched
    ///        from FixedUpdate via Singleton<T>.Instance auto-create (CarController reads InputManager
    ///        each fixed frame; WheelCollider.SkidMarks calls SkidmarksManager.AddSkidMark). Pre-creating
    ///        them here moves the construction cost out of the first physics frame and avoids inline
    ///        singleton creation during another component's FixedUpdate.
    ///
    ///   -11: Input writers - RCC_AICarController and RCC_Recorder write directly to
    ///        CarController.throttleInput / brakeInput / steerInput / handbrakeInput in their
    ///        FixedUpdate, which CarController.FixedUpdate reads same frame at -10. Without this
    ///        ordering, AI / replay inputs lag 1 fixed frame.
    ///
    ///   -10: RCC_CarControllerV4 - main vehicle controller. Steering() writes underSteering /
    ///        overSteering / frontSlip / rearSlip / brakeTorque which RCC_WheelCollider.ESP() reads
    ///        in its FixedUpdate at default 0. Running CarController first ensures the WheelCollider
    ///        sees current-frame slip state instead of last-frame.
    ///
    ///    10: RCC_Customizer - applies the saved customization loadout to the vehicle. Must run after
    ///        the vehicle's components (lights, wheels, sub-managers) are wired up so it can mutate
    ///        them. Mirrors the [DefaultExecutionOrder(10)] attribute on the class itself.
    /// </summary>
    private static readonly Dictionary<string, int> ExecutionOrders = new Dictionary<string, int>() {

        // === CENTRAL SINGLETON (-1000) ===
        { "RCC_SceneManager", -1000 },

        // === SINGLETON PRE-CREATION (-50) ===
        // Moves Singleton<T>.Instance auto-create cost out of the first FixedUpdate.
        { "RCC_InputManager", -50 },
        { "RCC_SkidmarksManager", -50 },

        // === INPUT WRITERS (-11) ===
        // Must run before CarController (at -10) reads throttle / brake / steer / handbrake inputs.
        { "RCC_AICarController", -11 },
        { "RCC_Recorder", -11 },

        // === CORE CONTROLLER (-10) ===
        // Writes slip + steering state that RCC_WheelCollider.ESP() reads same frame at default 0.
        { "RCC_CarControllerV4", -10 },

        // === LATE EXECUTION (10) ===
        { "RCC_Customizer", 10 },

    };

    /// <summary>
    /// Static constructor - called when Unity loads assemblies.
    /// </summary>
    static RCC_ScriptExecutionOrderManager() {

        // Delay execution to ensure all scripts are loaded
        EditorApplication.delayCall += OnDelayedInit;

    }

    /// <summary>
    /// Delayed initialization to ensure Unity is fully ready.
    /// </summary>
    private static void OnDelayedInit() {

        EditorApplication.delayCall -= OnDelayedInit;
        ValidateExecutionOrders();

    }

    /// <summary>
    /// Validates and sets execution orders for all RCC scripts.
    /// </summary>
    public static void ValidateExecutionOrders() {

        bool anyChanged = false;

        foreach (var kvp in ExecutionOrders) {

            string scriptName = kvp.Key;
            int targetOrder = kvp.Value;

            MonoScript script = FindScript(scriptName);

            if (script == null) {
                // Script not found - might not be installed yet
                continue;
            }

            int currentOrder = MonoImporter.GetExecutionOrder(script);

            if (currentOrder != targetOrder) {

                MonoImporter.SetExecutionOrder(script, targetOrder);
                anyChanged = true;
                Debug.Log($"[RCC] Set execution order for {scriptName}: {targetOrder}");

            }

        }

        if (anyChanged)
            Debug.Log("[RCC] Script execution order validated successfully.");

    }

    /// <summary>
    /// Resets all RCC script execution orders to default (0).
    /// </summary>
    public static void ResetExecutionOrders() {

        foreach (var kvp in ExecutionOrders) {

            string scriptName = kvp.Key;
            MonoScript script = FindScript(scriptName);

            if (script == null)
                continue;

            int currentOrder = MonoImporter.GetExecutionOrder(script);

            if (currentOrder != 0) {

                MonoImporter.SetExecutionOrder(script, 0);
                Debug.Log($"[RCC] Reset execution order for {scriptName} to 0");

            }

        }

        Debug.Log("[RCC] All script execution orders have been reset to default.");

    }

    /// <summary>
    /// Shows current execution orders in the console.
    /// </summary>
    public static void ShowExecutionOrders() {

        Debug.Log("[RCC] Current Script Execution Orders:");

        foreach (var kvp in ExecutionOrders) {

            string scriptName = kvp.Key;
            int targetOrder = kvp.Value;

            MonoScript script = FindScript(scriptName);

            if (script == null) {
                Debug.Log($"  {scriptName}: NOT FOUND");
                continue;
            }

            int currentOrder = MonoImporter.GetExecutionOrder(script);
            string status = currentOrder == targetOrder ? "OK" : $"MISMATCH (expected {targetOrder})";
            Debug.Log($"  {scriptName}: {currentOrder} [{status}]");

        }

    }

    /// <summary>
    /// Finds a MonoScript by class name.
    /// </summary>
    /// <param name="className">Name of the class to find.</param>
    /// <returns>MonoScript asset if found, null otherwise.</returns>
    private static MonoScript FindScript(string className) {

        // Search for script assets matching the class name
        string[] guids = AssetDatabase.FindAssets($"t:MonoScript {className}");

        foreach (string guid in guids) {

            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);

            if (script != null && script.name == className)
                return script;

        }

        return null;

    }

}
#endif
