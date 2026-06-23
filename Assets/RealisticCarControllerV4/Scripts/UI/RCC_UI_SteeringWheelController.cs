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
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mobile UI steering wheel controller. Tracks touch/mouse input on a circular Image to produce
/// a normalized steering input value (-1 to 1). The wheel visually rotates to match the input
/// and auto-centers when released. Only active when the mobile controller is set to SteeringWheel.
/// </summary>
public class RCC_UI_SteeringWheelController : RCC_Core {

    /// <summary>
    /// Reference to the steering wheel Image's GameObject, used for activation/deactivation.
    /// </summary>
    private GameObject steeringWheelGameObject;

    /// <summary>
    /// The Image component representing the steering wheel visual.
    /// </summary>
    private Image steeringWheelTexture;

    /// <summary>
    /// Current normalized steering input value (-1 = full left, 0 = center, 1 = full right).
    /// </summary>
    [Tooltip("Current normalized steering input value (-1 = full left, 0 = center, 1 = full right).")]
    [Range(-1f, 1f)]
    public float input = 0f;

    /// <summary>
    /// Current angle of the steering wheel in degrees.
    /// </summary>
    private float steeringWheelAngle = 0f;

    /// <summary>
    /// Maximum rotation angle (in degrees) the steering wheel can reach in either direction.
    /// </summary>
    [Tooltip("Maximum rotation angle (in degrees) the steering wheel can reach in either direction.")]
    [Min(0f)]
    public float steeringWheelMaximumsteerAngle = 270f;

    /// <summary>
    /// Speed at which the steering wheel returns to center when released.
    /// </summary>
    [Tooltip("Speed at which the steering wheel returns to center when released.")]
    [Min(0f)]
    public float steeringWheelResetPosSpeed = 20f;

    /// <summary>
    /// Radius (in pixels) of the center dead zone where touch input does not affect the wheel.
    /// </summary>
    [Tooltip("Radius (in pixels) of the center dead zone where touch input does not affect the wheel.")]
    [Min(0f)]
    public float steeringWheelCenterDeadZoneRadius = 5f;

    /// <summary>
    /// RectTransform of the steering wheel Image, used for rotation.
    /// </summary>
    private RectTransform steeringWheelRect;

    /// <summary>
    /// CanvasGroup of the steering wheel, checked for existence to validate the wheel is set up.
    /// </summary>
    private CanvasGroup steeringWheelCanvasGroup;

    /// <summary>
    /// Temporary angle from the previous frame, and the newly calculated angle for the current frame.
    /// </summary>
    private float steeringWheelTempAngle, steeringWheelNewAngle = 0f;

    /// <summary>
    /// Whether the steering wheel is currently being pressed/touched by the user.
    /// </summary>
    private bool steeringWheelPressed = false;

    /// <summary>
    /// Screen-space center position of the steering wheel.
    /// </summary>
    private Vector2 steeringWheelCenter = new Vector2();

    /// <summary>
    /// Current touch/mouse position while interacting with the steering wheel.
    /// </summary>
    private Vector2 steeringWheelTouchPos = new Vector2();

    /// <summary>
    /// EventTrigger component used to register pointer down, drag, and end drag events.
    /// </summary>
    private EventTrigger eventTrigger;

    private void Awake() {

        //	Initializing the ui wheel with proper event triggers.
        if (!TryGetComponent(out steeringWheelTexture))
            return;

        steeringWheelGameObject = steeringWheelTexture.gameObject;
        steeringWheelRect = steeringWheelTexture.rectTransform;
        steeringWheelCanvasGroup = steeringWheelTexture.GetComponent<CanvasGroup>();
        steeringWheelCenter = steeringWheelRect.position;

        SteeringWheelEventsInit();

    }

    private void OnEnable() {

        //  Make sure input is 0 on enable / disable.
        steeringWheelPressed = false;
        input = 0f;

    }

    private void LateUpdate() {

        //	No need to go further if current controller is not mobile controller.
        if (Settings.mobileController != RCC_Settings.MobileController.SteeringWheel)
            return;

        //	Receiving input.
        input = GetSteeringWheelInput();

        //	Visual steering wheel controlling.
        SteeringWheelControlling();

    }

    /// <summary>
    /// Registers PointerDown, Drag, and EndDrag event triggers on the steering wheel GameObject
    /// to track touch/mouse input for steering wheel rotation.
    /// </summary>
    private void SteeringWheelEventsInit() {

        if (!steeringWheelGameObject.TryGetComponent(out eventTrigger))
            return;

        var a = new EventTrigger.TriggerEvent();
        a.AddListener(data => {
            var evData = (PointerEventData)data;
            data.Use();

            steeringWheelPressed = true;
            steeringWheelTouchPos = evData.position;
            steeringWheelTempAngle = Vector2.Angle(Vector2.up, evData.position - steeringWheelCenter);
        });

        eventTrigger.triggers.Add(new EventTrigger.Entry { callback = a, eventID = EventTriggerType.PointerDown });


        var b = new EventTrigger.TriggerEvent();
        b.AddListener(data => {
            var evData = (PointerEventData)data;
            data.Use();
            steeringWheelTouchPos = evData.position;
        });

        eventTrigger.triggers.Add(new EventTrigger.Entry { callback = b, eventID = EventTriggerType.Drag });


        var c = new EventTrigger.TriggerEvent();
        c.AddListener(data => {
            steeringWheelPressed = false;
        });

        eventTrigger.triggers.Add(new EventTrigger.Entry { callback = c, eventID = EventTriggerType.EndDrag });

    }

    /// <summary>
    /// Gets the current normalized steering wheel input value.
    /// </summary>
    /// <returns>A value between -1 (full left) and 1 (full right), rounded to two decimal places.</returns>
    public float GetSteeringWheelInput() {

        return Mathf.Round(steeringWheelAngle / steeringWheelMaximumsteerAngle * 100) / 100;

    }

    /// <summary>
    /// Checks whether the steering wheel is currently being pressed by the user.
    /// </summary>
    /// <returns>True if the user is currently touching or clicking on the steering wheel.</returns>
    public bool IsPressed() {

        return steeringWheelPressed;

    }

    /// <summary>
    /// Manages the visual rotation of the steering wheel and auto-centering behavior.
    /// Hides the wheel if the mobile controller mode is not set to SteeringWheel.
    /// When pressed, rotates the wheel based on touch position; when released, smoothly returns to center.
    /// </summary>
    private void SteeringWheelControlling() {

        if (!steeringWheelGameObject || !steeringWheelCanvasGroup || !steeringWheelRect || Settings.mobileController != RCC_Settings.MobileController.SteeringWheel) {

            if (steeringWheelGameObject && steeringWheelGameObject.activeSelf)
                steeringWheelGameObject.SetActive(false);

            return;

        }

        if (!steeringWheelGameObject.activeSelf)
            steeringWheelGameObject.SetActive(true);

        if (steeringWheelPressed) {

            steeringWheelNewAngle = Vector2.Angle(Vector2.up, steeringWheelTouchPos - steeringWheelCenter);

            if (Vector2.Distance(steeringWheelTouchPos, steeringWheelCenter) > steeringWheelCenterDeadZoneRadius) {

                if (steeringWheelTouchPos.x > steeringWheelCenter.x)
                    steeringWheelAngle += steeringWheelNewAngle - steeringWheelTempAngle;
                else
                    steeringWheelAngle -= steeringWheelNewAngle - steeringWheelTempAngle;

            }

            if (steeringWheelAngle > steeringWheelMaximumsteerAngle)
                steeringWheelAngle = steeringWheelMaximumsteerAngle;
            else if (steeringWheelAngle < -steeringWheelMaximumsteerAngle)
                steeringWheelAngle = -steeringWheelMaximumsteerAngle;

            steeringWheelTempAngle = steeringWheelNewAngle;

        } else {

            if (!Mathf.Approximately(0f, steeringWheelAngle)) {

                float deltaAngle = steeringWheelResetPosSpeed;

                if (Mathf.Abs(deltaAngle) > Mathf.Abs(steeringWheelAngle)) {
                    steeringWheelAngle = 0f;
                    return;
                }

                steeringWheelAngle = Mathf.MoveTowards(steeringWheelAngle, 0f, deltaAngle * (Time.deltaTime * 100f));

            }

        }

        steeringWheelRect.eulerAngles = new Vector3(0f, 0f, -steeringWheelAngle);

    }

    private void OnDisable() {

        //  Make sure input is 0 on enable / disable.
        steeringWheelPressed = false;
        input = 0f;

    }

}
