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
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Mobile UI joystick controller that captures drag events and converts them to normalized horizontal
/// and vertical input values (range -1 to 1). The joystick handle moves within the background bounds
/// and automatically resets to center when released.
/// </summary>
public class RCC_UI_Joystick : RCC_Core, IDragHandler, IPointerUpHandler, IPointerDownHandler {

    /// <summary>
    /// RectTransform of the joystick background circle, defining the joystick's movement bounds.
    /// </summary>
    [Tooltip("RectTransform of the joystick background circle, defining the joystick's movement bounds.")]
    public RectTransform backgroundSprite;

    /// <summary>
    /// RectTransform of the draggable joystick handle that moves within the background.
    /// </summary>
    [Tooltip("RectTransform of the draggable joystick handle that moves within the background.")]
    public RectTransform handleSprite;

    /// <summary>
    /// Current normalized joystick input. X is horizontal (-1 to 1), Y is vertical (-1 to 1).
    /// </summary>
    private Vector2 inputVector = Vector2.zero;

    /// <summary>
    /// Normalized horizontal input from the joystick (-1 = left, 1 = right).
    /// </summary>
    public float InputHorizontal { get { return inputVector.x; } }

    /// <summary>
    /// Normalized vertical input from the joystick (-1 = down, 1 = up).
    /// </summary>
    public float InputVertical { get { return inputVector.y; } }

    /// <summary>
    /// Screen-space position of the joystick center, calculated at Start.
    /// </summary>
    private Vector2 joystickPosition = Vector2.zero;

    /// <summary>
    /// Reference camera used for screen-to-world conversion. Set to null (uses main camera).
    /// </summary>
    private readonly Camera _refCam;

    private void Start() {

        if (!backgroundSprite)
            return;

        joystickPosition = RectTransformUtility.WorldToScreenPoint(_refCam, backgroundSprite.position);

    }

    private void OnEnable() {

        //  Resetting inputs of the joystick on enable / disable.
        inputVector = Vector2.zero;

        if (handleSprite)
            handleSprite.anchoredPosition = Vector2.zero;

    }

    /// <summary>
    /// Called while the user is dragging the joystick handle. Updates the input vector and handle position.
    /// </summary>
    /// <param name="eventData">Pointer event data containing the current drag position.</param>
    public void OnDrag(PointerEventData eventData) {

        if (!backgroundSprite || !handleSprite)
            return;

        Vector2 direction = eventData.position - joystickPosition;
        float halfSize = backgroundSprite.sizeDelta.x / 2f;

        if (halfSize > 0.001f)
            inputVector = (direction.magnitude > halfSize) ? direction.normalized : direction / halfSize;
        else
            inputVector = Vector2.zero;

        handleSprite.anchoredPosition = (inputVector * halfSize) * 1f;

    }

    /// <summary>
    /// Called when the user releases the joystick. Resets the input vector and handle position to center.
    /// </summary>
    /// <param name="eventData">Pointer event data for the release event.</param>
    public void OnPointerUp(PointerEventData eventData) {

        inputVector = Vector2.zero;

        if (handleSprite)
            handleSprite.anchoredPosition = Vector2.zero;

    }

    /// <summary>
    /// Called when the user presses on the joystick. Can be overridden in subclasses for custom behavior.
    /// </summary>
    /// <param name="eventData">Pointer event data for the press event.</param>
    public virtual void OnPointerDown(PointerEventData eventData) {

        //

    }

    private void OnDisable() {

        //  Resetting inputs of the joystick on enable / disable.
        inputVector = Vector2.zero;

        if (handleSprite)
            handleSprite.anchoredPosition = Vector2.zero;

    }

}
