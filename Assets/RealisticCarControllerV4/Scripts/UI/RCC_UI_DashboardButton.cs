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
/// Multi-purpose UI button used in the dashboard options panel. Each button instance is assigned a
/// <see cref="ButtonType"/> that determines its behavior when clicked (e.g., toggling ABS/ESP/TCS,
/// switching headlights, shifting gears, changing camera, controlling recording, or managing customization).
/// The button image is illuminated or dimmed to reflect the active state of its associated feature.
/// </summary>
public class RCC_UI_DashboardButton : RCC_Core, IPointerClickHandler {

    /// <summary>
    /// Reference to the Button component on this GameObject. Used for reading the button image.
    /// </summary>
    private Button button;

    /// <summary>
    /// Determines what action this button performs when clicked.
    /// </summary>
    [Tooltip("Determines what action this button performs when clicked.")]
    public ButtonType _buttonType = ButtonType.ABS;

    /// <summary>
    /// All available dashboard button types, covering engine start, driving aids, lights, gears,
    /// quality settings, recording, camera, and customization controls.
    /// </summary>
    public enum ButtonType { Start, ABS, ESP, TCS, Headlights, LeftIndicator, RightIndicator, Gear, Low, Med, High, SH, GearUp, GearDown, HazardLights, SlowMo, Record, Replay, Neutral, ChangeCamera, CustomizeOn, CustomizeOff, Restore };

    /// <summary>
    /// Scrollbar used for the gear selector (D/N/R). Only used when <see cref="_buttonType"/> is Gear.
    /// </summary>
    private Scrollbar gearSlider;

    /// <summary>
    /// Current gear direction from the gear scrollbar. 0 = Drive, 1 = Neutral, 2 = Reverse.
    /// </summary>
    [Tooltip("Current gear direction from the gear scrollbar. 0 = Drive, 1 = Neutral, 2 = Reverse.")]
    [Range(0, 2)]
    public int gearDirection = 0;

    /// <summary>
    /// Cached delegate for the gear slider listener so it can be removed on destroy.
    /// </summary>
    private UnityEngine.Events.UnityAction<float> gearSliderListener;

    /// <summary>
    /// Handles pointer click events on this button, delegating to the internal OnClicked handler.
    /// </summary>
    /// <param name="eventData">Pointer event data from the EventSystem.</param>
    public void OnPointerClick(PointerEventData eventData) {

        OnClicked();

    }

    private void Awake() {

        button = GetComponent<Button>();

        //  If this button type is a gear selector, get scrollbar and add listener.
        if (_buttonType == ButtonType.Gear && GetComponentInChildren<Scrollbar>()) {

            gearSlider = GetComponentInChildren<Scrollbar>();
            gearSliderListener = (value) => ChangeGear();
            gearSlider.onValueChanged.AddListener(gearSliderListener);

        }

    }

    private void OnEnable() {

        //  Updating image of the button.
        UpdateImageOfButton();

    }

    /// <summary>
    /// Executes the action corresponding to the assigned <see cref="_buttonType"/>, then updates the button image.
    /// </summary>
    private void OnClicked() {

        switch (_buttonType) {

            case ButtonType.Low:

                QualitySettings.SetQualityLevel(1);

                break;

            case ButtonType.Med:

                QualitySettings.SetQualityLevel(3);

                break;

            case ButtonType.High:

                QualitySettings.SetQualityLevel(5);

                break;

            case ButtonType.SlowMo:

                if (Time.timeScale != .2f)
                    Time.timeScale = .2f;
                else
                    Time.timeScale = 1f;

                break;

            case ButtonType.Record:

                RCC.StartStopRecord();

                break;

            case ButtonType.Replay:

                RCC.StartStopReplay();

                break;

            case ButtonType.Neutral:

                RCC.StopRecordReplay();

                break;

            case ButtonType.ChangeCamera:

                RCC.ChangeCamera();

                break;

            case ButtonType.CustomizeOn:

                if (RCCSceneManager.activePlayerCanvas)
                    RCCSceneManager.activePlayerCanvas.SetDisplayType(RCC_UI_DashboardDisplay.DisplayType.Customization);

                break;

            case ButtonType.CustomizeOff:

                if (RCCSceneManager.activePlayerCanvas)
                    RCCSceneManager.activePlayerCanvas.SetDisplayType(RCC_UI_DashboardDisplay.DisplayType.Full);

                if (RCC_CustomizationDemo.Instance)
                    RCC_CustomizationDemo.Instance.DisableCustomization();

                break;

            case ButtonType.Restore:

                if (RCCSceneManager.activePlayerVehicle && RCCSceneManager.activePlayerVehicle.Customizer) {

                    RCCSceneManager.activePlayerVehicle.Customizer.Delete();
                    RCCSceneManager.activePlayerVehicle.Customizer.Initialize();

#if !UNITY_2022_1_OR_NEWER
                    RCC_UI_Upgrade[] upgraderButtons = FindObjectsOfType<RCC_UI_Upgrade>();
#else
                    RCC_UI_Upgrade[] upgraderButtons = FindObjectsByType<RCC_UI_Upgrade>(FindObjectsSortMode.None);
#endif

                    foreach (var item in upgraderButtons)
                        item.CheckLevel();

                }

                break;


            case ButtonType.Start:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.KillOrStartEngine();

                break;

            case ButtonType.ABS:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.ABS = !RCCSceneManager.activePlayerVehicle.ABS;

                break;

            case ButtonType.ESP:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.ESP = !RCCSceneManager.activePlayerVehicle.ESP;

                break;

            case ButtonType.TCS:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.TCS = !RCCSceneManager.activePlayerVehicle.TCS;

                break;

            case ButtonType.SH:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.steeringHelper = !RCCSceneManager.activePlayerVehicle.steeringHelper;

                break;

            case ButtonType.Headlights:

                if (RCCSceneManager.activePlayerVehicle) {

                    if (!RCCSceneManager.activePlayerVehicle.highBeamHeadLightsOn && RCCSceneManager.activePlayerVehicle.lowBeamHeadLightsOn) {

                        RCCSceneManager.activePlayerVehicle.highBeamHeadLightsOn = true;
                        RCCSceneManager.activePlayerVehicle.lowBeamHeadLightsOn = true;
                        break;

                    }

                    if (!RCCSceneManager.activePlayerVehicle.lowBeamHeadLightsOn)
                        RCCSceneManager.activePlayerVehicle.lowBeamHeadLightsOn = true;

                    if (RCCSceneManager.activePlayerVehicle.highBeamHeadLightsOn) {

                        RCCSceneManager.activePlayerVehicle.lowBeamHeadLightsOn = false;
                        RCCSceneManager.activePlayerVehicle.highBeamHeadLightsOn = false;

                    }

                }

                break;

            case ButtonType.LeftIndicator:

                if (RCCSceneManager.activePlayerVehicle) {

                    if (RCCSceneManager.activePlayerVehicle.indicatorsOn != RCC_CarControllerV4.IndicatorsOn.Left)
                        RCCSceneManager.activePlayerVehicle.indicatorsOn = RCC_CarControllerV4.IndicatorsOn.Left;
                    else
                        RCCSceneManager.activePlayerVehicle.indicatorsOn = RCC_CarControllerV4.IndicatorsOn.Off;

                }

                break;

            case ButtonType.RightIndicator:

                if (RCCSceneManager.activePlayerVehicle) {

                    if (RCCSceneManager.activePlayerVehicle.indicatorsOn != RCC_CarControllerV4.IndicatorsOn.Right)
                        RCCSceneManager.activePlayerVehicle.indicatorsOn = RCC_CarControllerV4.IndicatorsOn.Right;
                    else
                        RCCSceneManager.activePlayerVehicle.indicatorsOn = RCC_CarControllerV4.IndicatorsOn.Off;

                }

                break;

            case ButtonType.HazardLights:

                if (RCCSceneManager.activePlayerVehicle) {

                    if (RCCSceneManager.activePlayerVehicle.indicatorsOn != RCC_CarControllerV4.IndicatorsOn.All)
                        RCCSceneManager.activePlayerVehicle.indicatorsOn = RCC_CarControllerV4.IndicatorsOn.All;
                    else
                        RCCSceneManager.activePlayerVehicle.indicatorsOn = RCC_CarControllerV4.IndicatorsOn.Off;

                }

                break;

            case ButtonType.GearUp:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.GearShiftUp();

                break;

            case ButtonType.GearDown:

                if (RCCSceneManager.activePlayerVehicle)
                    RCCSceneManager.activePlayerVehicle.GearShiftDown();

                break;

        }

        UpdateImageOfButton();

    }

    /// <summary>
    /// Updates the button's image color to reflect the current state of its associated feature.
    /// Illuminates the button (white) when the feature is active and dims it (dark gray) when inactive.
    /// Only applies to ABS, ESP, TCS, Steering Helper, and Headlights buttons.
    /// </summary>
    public void UpdateImageOfButton() {

        if (!button)
            return;

        //  If no image attached to the button, return.
        if (!button.image)
            return;

        //  If no player vehicle found, return.
        if (!RCCSceneManager.activePlayerVehicle)
            return;

        //  Illuminating the image of the button when it's on.
        switch (_buttonType) {

            case ButtonType.ABS:

                if (RCCSceneManager.activePlayerVehicle.ABS)
                    button.image.color = new Color(1, 1, 1, 1);
                else
                    button.image.color = new Color(.25f, .25f, .25f, 1);

                break;

            case ButtonType.ESP:

                if (RCCSceneManager.activePlayerVehicle.ESP)
                    button.image.color = new Color(1, 1, 1, 1);
                else
                    button.image.color = new Color(.25f, .25f, .25f, 1);

                break;

            case ButtonType.TCS:

                if (RCCSceneManager.activePlayerVehicle.TCS)
                    button.image.color = new Color(1, 1, 1, 1);
                else
                    button.image.color = new Color(.25f, .25f, .25f, 1);

                break;

            case ButtonType.SH:

                if (RCCSceneManager.activePlayerVehicle.steeringHelper)
                    button.image.color = new Color(1, 1, 1, 1);
                else
                    button.image.color = new Color(.25f, .25f, .25f, 1);

                break;

            case ButtonType.Headlights:

                if (RCCSceneManager.activePlayerVehicle.lowBeamHeadLightsOn || RCCSceneManager.activePlayerVehicle.highBeamHeadLightsOn)
                    button.image.color = new Color(1, 1, 1, 1);
                else
                    button.image.color = new Color(.25f, .25f, .25f, 1);

                break;

        }

    }

    /// <summary>
    /// Called when the gear scrollbar value changes. Maps the scrollbar position to Drive (0), Neutral (1),
    /// or Reverse (2) and applies the corresponding gear change to the active player vehicle.
    /// </summary>
    public void ChangeGear() {

        if (!RCCSceneManager.activePlayerVehicle)
            return;

        if (!gearSlider)
            return;

        if (gearDirection == Mathf.CeilToInt(gearSlider.value * 2))
            return;

        gearDirection = Mathf.CeilToInt(gearSlider.value * 2);

        RCCSceneManager.activePlayerVehicle.semiAutomaticGear = true;

        switch (gearDirection) {

            case 0:
                RCCSceneManager.activePlayerVehicle.StartCoroutine("ChangeGear", 0);
                RCCSceneManager.activePlayerVehicle.NGear = false;
                break;

            case 1:
                RCCSceneManager.activePlayerVehicle.NGear = true;
                break;

            case 2:
                RCCSceneManager.activePlayerVehicle.StartCoroutine("ChangeGear", -1);
                RCCSceneManager.activePlayerVehicle.NGear = false;
                break;

        }

    }

    private void OnDestroy() {

        // Remove gear slider listener to prevent delegate leak.
        if (gearSlider && gearSliderListener != null)
            gearSlider.onValueChanged.RemoveListener(gearSliderListener);

    }

}
