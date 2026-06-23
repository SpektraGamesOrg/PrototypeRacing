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
using UnityEditor;

/// <summary>
/// Helper that scans a vehicle GameObject for likely wheel meshes by analysing each MeshFilter's bounds and aspect ratio. Used by the Quick Vehicle Setup Wizard to auto-detect front and rear wheel candidates.
/// </summary>
public class RCC_DetectPossibleWheels {

    /// <summary>
    /// Returns every child mesh that looks like a wheel based on cylindrical bounds and reasonable size.
    /// </summary>
    /// <param name="vehicle">The vehicle root GameObject to scan.</param>
    /// <returns>Array of wheel-candidate GameObjects; empty if none are found or the vehicle is null.</returns>
    public static GameObject[] DetectPossibleAllWheels(GameObject vehicle) {

        if (vehicle == null) {

            Debug.LogError("RCC: No vehicle GameObject was provided!");
            return new GameObject[0];

        }

        List<GameObject> allWheels = new List<GameObject>();
        MeshFilter[] meshFilters = vehicle.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0) {

            Debug.LogWarning("RCC: No MeshFilters found in the vehicle. Are you sure this model has wheels?");
            return new GameObject[0];

        }

        foreach (MeshFilter meshFilter in meshFilters) {

            if (meshFilter.sharedMesh == null)
                continue;

            Bounds bounds = meshFilter.sharedMesh.bounds;
            float depth = bounds.size.x;
            float height = bounds.size.y;
            float width = bounds.size.z;

            // More flexible wheel detection
            float aspectRatioTolerance = 0.1f;
            bool isCylindrical = Mathf.Abs(width - height) < aspectRatioTolerance && depth < width * 1.2f;
            bool isReasonableSize = width > 0.1f && width < 2.5f;

            if (isCylindrical && isReasonableSize) {

                allWheels.Add(meshFilter.gameObject);

            }

        }

        return allWheels.ToArray();

    }

    /// <summary>
    /// Returns the front-half subset of wheel candidates detected on the given vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle root GameObject to scan.</param>
    /// <returns>Array of wheel GameObjects located in front of the vehicle origin.</returns>
    public static GameObject[] DetectPossibleFrontWheels(GameObject vehicle) {

        if (vehicle == null) {

            Debug.LogError("RCC: No vehicle GameObject was provided!");
            return new GameObject[0];

        }

        GameObject[] allWheels = DetectPossibleAllWheels(vehicle);
        List<GameObject> frontWheels = new List<GameObject>();

        foreach (GameObject wheel in allWheels) {

            if (IsInFront(vehicle, wheel))
                frontWheels.Add(wheel);

        }

        return frontWheels.ToArray();

    }

    /// <summary>
    /// Returns the rear-half subset of wheel candidates detected on the given vehicle.
    /// </summary>
    /// <param name="vehicle">The vehicle root GameObject to scan.</param>
    /// <returns>Array of wheel GameObjects located behind the vehicle origin.</returns>
    public static GameObject[] DetectPossibleRearWheels(GameObject vehicle) {

        if (vehicle == null) {

            Debug.LogError("RCC: No vehicle GameObject was provided!");
            return new GameObject[0];

        }

        GameObject[] allWheels = DetectPossibleAllWheels(vehicle);
        List<GameObject> rearWheels = new List<GameObject>();

        foreach (GameObject wheel in allWheels) {

            if (!IsInFront(vehicle, wheel))
                rearWheels.Add(wheel);

        }

        return rearWheels.ToArray();

    }

    /// <summary>
    /// Returns true if the wheel sits in front of the vehicle origin along the vehicle's forward axis.
    /// </summary>
    /// <param name="vehicle">The vehicle root GameObject providing the forward direction.</param>
    /// <param name="wheel">The wheel GameObject to classify.</param>
    /// <returns>True when the wheel is ahead of the vehicle origin; false otherwise (including null inputs).</returns>
    public static bool IsInFront(GameObject vehicle, GameObject wheel) {

        if (vehicle == null || wheel == null)
            return false;

        Vector3 parentForward = vehicle.transform.forward;
        Vector3 directionToChild = wheel.transform.position - vehicle.transform.position;

        return Vector3.Dot(parentForward, directionToChild) > 0;

    }

}
