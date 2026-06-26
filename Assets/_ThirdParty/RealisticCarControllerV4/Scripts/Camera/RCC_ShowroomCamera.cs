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
using UnityEngine.EventSystems;

/// <summary>
/// Showroom camera that orbits around a target transform, typically used in main menu or vehicle preview scenes.
/// Supports automatic orbiting, smooth interpolation, and user drag interaction via UI events.
/// </summary>
public class RCC_ShowroomCamera : RCC_Core {

    /// <summary>
    /// The target transform the camera orbits around. Typically the vehicle spawn point.
    /// </summary>
    [Header("Target")]
    [Tooltip("The target transform the camera orbits around. Typically the vehicle spawn point.")]
    public Transform target;

    /// <summary>
    /// Distance between the camera and the target along the Z axis.
    /// </summary>
    [Tooltip("Distance between the camera and the target along the Z axis.")]
    [Min(0f)]
    public float distance = 8f;

    /// <summary>
    /// Whether the camera is currently auto-orbiting around the target.
    /// </summary>
    [Header("Orbit")]
    [Tooltip("Whether the camera is currently auto-orbiting around the target.")]
    public bool orbitingNow = true;

    /// <summary>
    /// Speed of the automatic orbit rotation in degrees per second.
    /// </summary>
    [Tooltip("Speed of the automatic orbit rotation in degrees per second.")]
    [Min(0f)]
    public float orbitSpeed = 5f;

    /// <summary>
    /// When enabled, camera position and rotation are smoothly interpolated instead of snapping instantly.
    /// </summary>
    [Header("Smoothing")]
    [Tooltip("When enabled, camera position and rotation are smoothly interpolated instead of snapping instantly.")]
    public bool smooth = true;

    /// <summary>
    /// Interpolation speed factor when smooth orbiting is enabled.
    /// </summary>
    [Tooltip("Interpolation speed factor when smooth orbiting is enabled.")]
    [Min(0f)]
    public float smoothingFactor = 5f;

    /// <summary>
    /// Minimum vertical angle (degrees) the camera can orbit to.
    /// </summary>
    [Header("Vertical Limits")]
    [Tooltip("Minimum vertical angle (degrees) the camera can orbit to.")]
    [Range(-90f, 90f)]
    public float minY = 5f;

    /// <summary>
    /// Maximum vertical angle (degrees) the camera can orbit to.
    /// </summary>
    [Tooltip("Maximum vertical angle (degrees) the camera can orbit to.")]
    [Range(-90f, 90f)]
    public float maxY = 35f;

    /// <summary>
    /// Whether the player is currently dragging to rotate the camera.
    /// </summary>
    [Space]
    private bool draggingNow = false;

    /// <summary>
    /// Sensitivity multiplier for drag-based camera rotation.
    /// </summary>
    [Header("Drag")]
    [Tooltip("Sensitivity multiplier for drag-based camera rotation.")]
    [Min(0f)]
    public float dragSpeed = 10f;

    /// <summary>
    /// Current horizontal orbit angle (degrees).
    /// </summary>
    [Tooltip("Current horizontal orbit angle (degrees).")]
    public float orbitX = 0f;

    /// <summary>
    /// Current vertical orbit angle (degrees).
    /// </summary>
    [Tooltip("Current vertical orbit angle (degrees).")]
    public float orbitY = 0f;

    private void LateUpdate() {

        // If there is no target, return.
        if (!target)
            return;

        // If auto orbiting is enabled, increase orbitX slowly with orbitSpeed factor.
        if (orbitingNow)
            orbitX += Time.deltaTime * orbitSpeed;

        //  Clamping orbit Y.
        orbitY = ClampAngle(orbitY, minY, maxY);

        // Calculating rotation and position of the camera.
        Quaternion rotation = Quaternion.Euler(orbitY, orbitX, 0);
        Vector3 position = rotation * new Vector3(0f, 0f, -distance) + target.transform.position;

        // Setting position and rotation of the camera.
        if (!smooth) {

            transform.rotation = rotation;
            transform.position = position;

        } else {

            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.unscaledDeltaTime * 10f);
            transform.position = Vector3.Lerp(transform.position, position, Time.unscaledDeltaTime * 10f);

        }

    }

    /// <summary>
    /// Sets whether the player is currently dragging to rotate the camera.
    /// </summary>
    /// <param name="state">True if the player is dragging, false otherwise.</param>
    public void SetDrag(bool state) {

        draggingNow = state;

    }

    /// <summary>
    /// Clamps an angle between a minimum and maximum value, normalizing it to the -360 to 360 range first.
    /// </summary>
    /// <param name="angle">The angle to clamp.</param>
    /// <param name="min">Minimum allowed angle.</param>
    /// <param name="max">Maximum allowed angle.</param>
    /// <returns>The clamped angle value.</returns>
    private float ClampAngle(float angle, float min, float max) {

        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);

    }

    /// <summary>
    /// Enables or disables automatic orbit rotation around the target.
    /// </summary>
    /// <param name="state">True to enable auto-rotation, false to disable it.</param>
    public void ToggleAutoRotation(bool state) {

        orbitingNow = state;

    }

    /// <summary>
    /// Handles drag input from the UI EventSystem to rotate the camera around the target.
    /// </summary>
    /// <param name="pointerData">Pointer event data containing the drag delta.</param>
    public void OnDrag(PointerEventData pointerData) {

        // Receiving drag input from UI.
        orbitX += pointerData.delta.x * dragSpeed * .02f;
        orbitY -= pointerData.delta.y * dragSpeed * .02f;

    }

}
