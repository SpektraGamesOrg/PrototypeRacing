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

/// <summary>
/// UI color picker that uses three sliders (R, G, B) to let the player choose a color
/// for wheel smoke particles or headlight tint on the active vehicle.
/// </summary>
public class RCC_ColorPickerBySliders : RCC_Core {

    /// <summary>
    /// Determines which vehicle property this color picker controls.
    /// </summary>
    [Tooltip("Determines which vehicle property this color picker controls.")]
    public ColorType colorType = ColorType.WheelSmoke;

    /// <summary>
    /// Available color target types for the picker.
    /// </summary>
    public enum ColorType {

        /// <summary>
        /// Controls the color of wheel smoke particles (tire smoke).
        /// </summary>
        WheelSmoke,

        /// <summary>
        /// Controls the tint color of headlight Light components.
        /// </summary>
        Headlights

    }

    /// <summary>
    /// The current selected color, composed from the R/G/B slider values.
    /// </summary>
    [Tooltip("The current selected color, composed from the R/G/B slider values.")]
    public Color color;

    /// <summary>
    /// Previous frame's color, used to detect changes and avoid redundant updates.
    /// </summary>
    private Color oldColor;

    /// <summary>
    /// UI slider controlling the red channel (0-1).
    /// </summary>
    [Tooltip("UI slider controlling the red channel (0-1).")]
    public Slider redSlider;

    /// <summary>
    /// UI slider controlling the green channel (0-1).
    /// </summary>
    [Tooltip("UI slider controlling the green channel (0-1).")]
    public Slider greenSlider;

    /// <summary>
    /// UI slider controlling the blue channel (0-1).
    /// </summary>
    [Tooltip("UI slider controlling the blue channel (0-1).")]
    public Slider blueSlider;

    private void OnEnable() {

        //  Finding the player vehicle.
        RCC_CarControllerV4 playerVehicle = RCCSceneManager.activePlayerVehicle;

        //  If player vehicle doesn't have the customizer component, return.
        if (playerVehicle && playerVehicle.Customizer) {

            switch (colorType) {

                case ColorType.Headlights:

                    for (int i = 0; i < playerVehicle.AllLights.Length; i++) {

                        if (playerVehicle.AllLights[i].lightType == RCC_Light.LightType.HeadLight) {

                            color = playerVehicle.AllLights[i].LightSource.color;
                            break;

                        }

                    }
                    break;

                case ColorType.WheelSmoke:

                    for (int i = 0; i < playerVehicle.AllWheelColliders.Length; i++) {

                        if (playerVehicle.AllWheelColliders[i] != null) {

                            foreach (ParticleSystem wheelParticle in playerVehicle.AllWheelColliders[i].GetComponentsInChildren<ParticleSystem>(true)) {

                                if (wheelParticle.transform.GetSiblingIndex() == 0) {

                                    ParticleSystem.MainModule psmain = wheelParticle.main;
                                    color = psmain.startColor.color;
                                    break;

                                }

                            }

                        }

                    }
                    break;

            }

        }

        oldColor = color;

        if (redSlider)
            redSlider.SetValueWithoutNotify(color.r);
        if (greenSlider)
            greenSlider.SetValueWithoutNotify(color.g);
        if (blueSlider)
            blueSlider.SetValueWithoutNotify(color.b);

    }

    private void Update() {

        // Assigning new color to main color.
        color = new Color(redSlider.value, greenSlider.value, blueSlider.value);

        if (oldColor != color) {

            if (!enabled)
                return;

            //  Finding the player vehicle.
            RCC_CarControllerV4 playerVehicle = RCCSceneManager.activePlayerVehicle;

            //  If no player vehicle found, return.
            if (!playerVehicle)
                return;

            //  If player vehicle doesn't have the customizer component, return.
            if (!playerVehicle.Customizer)
                return;

            switch (colorType) {

                case ColorType.Headlights:

                    //  If player vehicle doesn't have the decal manager component, return.
                    if (!playerVehicle.Customizer.CustomizationManager)
                        return;

                    //  Set the decal.
                    playerVehicle.Customizer.CustomizationManager.SetHeadlightsColor(color);

                    break;

                case ColorType.WheelSmoke:

                    //  If player vehicle doesn't have the decal manager component, return.
                    if (!playerVehicle.Customizer.CustomizationManager)
                        return;

                    //  Set the decal.
                    playerVehicle.Customizer.CustomizationManager.SetSmokeColor(color);

                    break;

            }

        }

        oldColor = color;

    }

}
