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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the vehicle customization UI canvas, handling panel visibility and button interactability
/// based on which customization sub-managers are available on the active player vehicle.
/// Also provides showroom camera controls for auto-rotation and orbit angles.
/// </summary>
public class RCC_UI_Canvas_Customizer : RCC_Core {

    /// <summary>
    /// Currently active player vehicle, updated each frame from the scene manager.
    /// </summary>
    private RCC_CarControllerV4 currentPlayer;

    //UI Panels.
    /// <summary>
    /// Panel containing the body paint color customization UI.
    /// </summary>
    [Tooltip("Panel containing the body paint color customization UI.")]
    [Header("Modify Panels")]
    public GameObject colorClass;

    /// <summary>
    /// Panel containing the wheel/rim selection UI.
    /// </summary>
    [Tooltip("Panel containing the wheel/rim selection UI.")]
    public GameObject wheelClass;

    /// <summary>
    /// Panel containing general modification options (suspension, camber, etc.).
    /// </summary>
    [Tooltip("Panel containing general modification options (suspension, camber, etc.).")]
    public GameObject modificationClass;

    /// <summary>
    /// Panel containing the performance upgrade UI (engine, brake, handling, speed).
    /// </summary>
    [Tooltip("Panel containing the performance upgrade UI (engine, brake, handling, speed).")]
    public GameObject upgradesClass;

    /// <summary>
    /// Panel containing the spoiler selection UI.
    /// </summary>
    [Tooltip("Panel containing the spoiler selection UI.")]
    public GameObject spoilerClass;

    /// <summary>
    /// Panel containing the siren selection UI.
    /// </summary>
    [Tooltip("Panel containing the siren selection UI.")]
    public GameObject sirenClass;

    /// <summary>
    /// Panel containing the decal customization UI.
    /// </summary>
    [Tooltip("Panel containing the decal customization UI.")]
    public GameObject decalsClass;

    /// <summary>
    /// Panel containing the neon light customization UI.
    /// </summary>
    [Tooltip("Panel containing the neon light customization UI.")]
    public GameObject neonsClass;

    //UI Buttons.
    /// <summary>
    /// Button that opens the body paint customization panel.
    /// </summary>
    [Tooltip("Button that opens the body paint customization panel.")]
    [Header("Modify Buttons")]
    public Button bodyPaintButton;

    /// <summary>
    /// Button that opens the wheel/rim selection panel.
    /// </summary>
    [Tooltip("Button that opens the wheel/rim selection panel.")]
    public Button rimButton;

    /// <summary>
    /// Button that opens the general customization panel (suspension, camber, etc.).
    /// </summary>
    [Tooltip("Button that opens the general customization panel (suspension, camber, etc.).")]
    public Button customizationButton;

    /// <summary>
    /// Button that opens the performance upgrades panel.
    /// </summary>
    [Tooltip("Button that opens the performance upgrades panel.")]
    public Button upgradeButton;

    /// <summary>
    /// Button that opens the spoiler selection panel.
    /// </summary>
    [Tooltip("Button that opens the spoiler selection panel.")]
    public Button spoilersButton;

    /// <summary>
    /// Button that opens the siren selection panel.
    /// </summary>
    [Tooltip("Button that opens the siren selection panel.")]
    public Button sirensButton;

    /// <summary>
    /// Button that opens the decal customization panel.
    /// </summary>
    [Tooltip("Button that opens the decal customization panel.")]
    public Button decalsButton;

    /// <summary>
    /// Button that opens the neon light customization panel.
    /// </summary>
    [Tooltip("Button that opens the neon light customization panel.")]
    public Button neonsButton;

    /// <summary>
    /// Original color of the buttons, captured at Awake. Used to reset button colors when switching panels.
    /// </summary>
    private Color orgButtonColor;

    private void Awake() {

        //Getting original color of the button.
        if (bodyPaintButton)
            orgButtonColor = bodyPaintButton.image.color;

    }

    private void Update() {

        currentPlayer = RCCSceneManager.activePlayerVehicle;

        // If no any player vehicle, disable all buttons and return.
        if (!currentPlayer || (currentPlayer && !currentPlayer.Customizer)) {

            if (upgradeButton)
                upgradeButton.interactable = false;

            if (spoilersButton)
                spoilersButton.interactable = false;

            if (customizationButton)
                customizationButton.interactable = false;

            if (sirensButton)
                sirensButton.interactable = false;

            if (rimButton)
                rimButton.interactable = false;

            if (bodyPaintButton)
                bodyPaintButton.interactable = false;

            return;

        }

        // Setting interactable states of the buttons depending on upgrade managers. 
        //	Ex. If spoiler manager not found, spoiler button will be disabled.
        if (upgradeButton)
            upgradeButton.interactable = currentPlayer.Customizer.UpgradeManager;

        if (spoilersButton)
            spoilersButton.interactable = currentPlayer.Customizer.SpoilerManager;

        if (customizationButton)
            customizationButton.interactable = currentPlayer.Customizer.CustomizationManager;

        if (sirensButton)
            sirensButton.interactable = currentPlayer.Customizer.SirenManager;

        if (rimButton)
            rimButton.interactable = currentPlayer.Customizer.WheelManager;

        if (bodyPaintButton)
            bodyPaintButton.interactable = currentPlayer.Customizer.PaintManager;

        if (decalsButton)
            decalsButton.interactable = currentPlayer.Customizer.DecalManager;

        if (neonsButton)
            neonsButton.interactable = currentPlayer.Customizer.NeonManager;

    }

    /// <summary>
    /// Deactivates all customization panels and then activates the specified panel.
    /// </summary>
    /// <param name="activeClass">The customization panel GameObject to activate.</param>
    public void ChooseClass(GameObject activeClass) {

        if (colorClass)
            colorClass.SetActive(false);

        if (wheelClass)
            wheelClass.SetActive(false);

        if (modificationClass)
            modificationClass.SetActive(false);

        if (upgradesClass)
            upgradesClass.SetActive(false);

        if (spoilerClass)
            spoilerClass.SetActive(false);

        if (sirenClass)
            sirenClass.SetActive(false);

        if (decalsClass)
            decalsClass.SetActive(false);

        if (neonsClass)
            neonsClass.SetActive(false);

        if (activeClass)
            activeClass.SetActive(true);

    }

    /// <summary>
    /// Resets all customization button colors to the default and highlights the active button in green.
    /// </summary>
    /// <param name="activeButton">The button to highlight as currently selected.</param>
    public void CheckButtonColors(Button activeButton) {

        if (bodyPaintButton)
            bodyPaintButton.image.color = orgButtonColor;

        if (rimButton)
            rimButton.image.color = orgButtonColor;

        if (customizationButton)
            customizationButton.image.color = orgButtonColor;

        if (upgradeButton)
            upgradeButton.image.color = orgButtonColor;

        if (spoilersButton)
            spoilersButton.image.color = orgButtonColor;

        if (sirensButton)
            sirensButton.image.color = orgButtonColor;

        if (decalsButton)
            decalsButton.image.color = orgButtonColor;

        if (neonsButton)
            neonsButton.image.color = orgButtonColor;

        if (activeButton)
            activeButton.image.color = new Color(0f, 1f, 0f);

    }

    /// <summary>
    /// Toggles auto-rotation of the showroom camera on or off.
    /// </summary>
    /// <param name="state">True to enable auto-rotation, false to disable it.</param>
    public void ToggleAutoRotation(bool state) {

#if !UNITY_2022_1_OR_NEWER
        RCC_ShowroomCamera showroomCamera = FindObjectOfType<RCC_ShowroomCamera>();
#else
        RCC_ShowroomCamera showroomCamera = FindFirstObjectByType<RCC_ShowroomCamera>();
#endif

        // If no any showroom camera, return.
        if (!showroomCamera)
            return;

        showroomCamera.ToggleAutoRotation(state);

    }

    /// <summary>
    /// Sets the horizontal orbit angle of the showroom camera.
    /// </summary>
    /// <param name="hor">The horizontal orbit angle in degrees.</param>
    public void SetHorizontal(float hor) {

#if !UNITY_2022_1_OR_NEWER
        RCC_ShowroomCamera showroomCamera = FindObjectOfType<RCC_ShowroomCamera>();
#else
        RCC_ShowroomCamera showroomCamera = FindFirstObjectByType<RCC_ShowroomCamera>();
#endif

        // If no any showroom camera, return.
        if (!showroomCamera)
            return;

        showroomCamera.orbitX = hor;

    }
    /// <summary>
    /// Sets the vertical orbit angle of the showroom camera.
    /// </summary>
    /// <param name="ver">The vertical orbit angle in degrees.</param>
    public void SetVertical(float ver) {

#if !UNITY_2022_1_OR_NEWER
        RCC_ShowroomCamera showroomCamera = FindObjectOfType<RCC_ShowroomCamera>();
#else
        RCC_ShowroomCamera showroomCamera = FindFirstObjectByType<RCC_ShowroomCamera>();
#endif

        // If no any showroom camera, return.
        if (!showroomCamera)
            return;

        showroomCamera.orbitY = ver;

    }

    /// <summary>
    /// Disables the customization mode by delegating to the customization demo singleton.
    /// </summary>
    public void DisableCustomization() {

        if (RCC_CustomizationDemo.Instance)
            RCC_CustomizationDemo.Instance.DisableCustomization();

    }

}
