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
/// Editor helper that checks whether a vehicle model's mesh bounds align with RCC's expected axis convention (length along Z, height along Y).
/// </summary>
public class RCC_CheckAxisOrientation {

    /// <summary>
    /// Determines whether the supplied vehicle's combined mesh bounds match RCC's expected axis orientation.
    /// </summary>
    /// <param name="vehicle">The vehicle root GameObject whose child MeshFilters will be inspected.</param>
    /// <returns>True when length (Z) is at least as large as width (X) and height (Y); false otherwise.</returns>
    public static bool IsAxisOrientationCorrect(GameObject vehicle) {

        if (vehicle == null)
            return false;

        Bounds combinedBounds = new Bounds(Vector3.zero, Vector3.one);
        MeshFilter[] meshFilters = vehicle.GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length == 0) {

            Debug.LogWarning("No MeshFilters found in the car model.");
            return false;

        }

        // Aggregate the bounds of all MeshFilters
        foreach (MeshFilter meshFilter in meshFilters) {

            if (meshFilter.sharedMesh != null)
                combinedBounds.Encapsulate(meshFilter.sharedMesh.bounds);

        }

        // Compare the dimensions
        float width = combinedBounds.size.x;
        float height = combinedBounds.size.y;
        float length = combinedBounds.size.z;

        if (width <= length && length >= height) {

            return true;

        } else {

            return false;

        }

    }

}
