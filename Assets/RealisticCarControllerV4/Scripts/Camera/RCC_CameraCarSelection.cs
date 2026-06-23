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
using UnityEngine.EventSystems;

/// <summary>
/// Controls the showroom camera while selecting vehicles. 
/// The camera orbits around the selected vehicle and allows user interaction via UI drag gestures.
/// </summary>
public class RCC_CameraCarSelection : RCC_Core {

    /// <summary>
    /// The target vehicle that the camera focuses on.
    /// </summary>
    [Header("Target")]
    [Tooltip("The target vehicle that the camera focuses on.")]
    public Transform target;

    /// <summary>
    /// Distance between the camera and the target.
    /// </summary>
    [Tooltip("Distance between the camera and the target.")]
    [Min(0f)]
    public float distance = 10.0f;

    /// <summary>
    /// Rotation speed around the X-axis (horizontal movement).
    /// </summary>
    [Header("Rotation Speed")]
    [Tooltip("Rotation speed around the X-axis (horizontal movement).")]
    [Min(0f)]
    public float xSpeed = 250f;

    /// <summary>
    /// Rotation speed around the Y-axis (vertical movement).
    /// </summary>
    [Tooltip("Rotation speed around the Y-axis (vertical movement).")]
    [Min(0f)]
    public float ySpeed = 120f;

    /// <summary>
    /// Minimum Y angle the camera can tilt.
    /// </summary>
    [Header("Vertical Limits")]
    [Tooltip("Minimum Y angle the camera can tilt.")]
    [Range(-90f, 90f)]
    public float yMinLimit = -20f;

    /// <summary>
    /// Maximum Y angle the camera can tilt.
    /// </summary>
    [Tooltip("Maximum Y angle the camera can tilt.")]
    [Range(-90f, 90f)]
    public float yMaxLimit = 80f;

    /// <summary>
    /// Field of View for the camera.
    /// </summary>
    [Header("Camera")]
    [Tooltip("Field of View for the camera.")]
    [Range(20f, 120f)]
    public float FOV = 50f;

    /// <summary>
    /// Current X rotation value.
    /// </summary>
    private float x = 0f;

    /// <summary>
    /// Current Y rotation value.
    /// </summary>
    private float y = 0f;

    /// <summary>
    /// Whether the camera should rotate automatically around the target.
    /// </summary>
    private bool selfTurn = true;

    /// <summary>
    /// Timer used to delay automatic camera rotation when the player interacts.
    /// </summary>
    private float selfTurnTime = 0f;

    private void Start() {

        // Initializes the camera's rotation angles based on the current transform.
        x = transform.eulerAngles.y;
        y = transform.eulerAngles.x;

    }

    private void LateUpdate() {

        // If no target is assigned, exit the update.
        if (!target)
            return;

        // If self-turn is enabled, gradually rotate the camera around the target.
        if (selfTurn)
            x += xSpeed / 2f * Time.deltaTime;

        // Clamp the Y rotation to prevent excessive vertical movement.
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // Calculate the new rotation and position for the camera.
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0f, 0f, -distance) + target.position;

        // Apply the new position and rotation to the camera.
        transform.SetPositionAndRotation(position, rotation);

        // Increase self-turn timer.
        if (selfTurnTime <= 1f)
            selfTurnTime += Time.deltaTime;

        // Once enough time has passed, enable self-turn again.
        if (selfTurnTime >= 1f)
            selfTurn = true;

    }

    /// <summary>
    /// Clamps an angle between a minimum and maximum value.
    /// </summary>
    /// <param name="angle">The angle to be clamped.</param>
    /// <param name="min">Minimum allowed value.</param>
    /// <param name="max">Maximum allowed value.</param>
    /// <returns>Clamped angle.</returns>
    private float ClampAngle(float angle, float min, float max) {

        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);

    }

    /// <summary>
    /// Handles camera rotation when the player drags on the UI.
    /// </summary>
    /// <param name="data">Pointer event data containing drag movement.</param>
    public void OnDrag(BaseEventData data) {

        if (!target)
            return;

        PointerEventData pointerData = data as PointerEventData;

        // Adjust rotation based on the user's drag movement.
        x += pointerData.delta.x * xSpeed * 0.02f;
        y -= pointerData.delta.y * ySpeed * 0.02f;

        // Clamp Y rotation within allowed limits.
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        // Calculate the new rotation and position based on the drag input.
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0f, 0f, -distance) + target.position;

        // Apply the new position and rotation to the camera.
        transform.SetPositionAndRotation(position, rotation);

        // Disable self-turn while the user is interacting.
        selfTurn = false;
        selfTurnTime = 0f;

    }

}
