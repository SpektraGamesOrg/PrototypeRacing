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
/// Initializes a UI Dropdown to reflect the current RCC setting value on Awake.
/// Used in demo scenes to synchronize dropdown selections with behavior mode,
/// mobile controller type, or graphics quality settings.
/// </summary>
public class RCC_Useless : RCC_Core {

    /// <summary>
    /// Defines which RCC setting this dropdown represents.
    /// </summary>
    public enum Useless {

        /// <summary>
        /// Main controller selection dropdown.
        /// </summary>
        MainController,

        /// <summary>
        /// Mobile controller type dropdown (TouchScreen, Gyro, SteeringWheel, Joystick).
        /// </summary>
        MobileControllers,

        /// <summary>
        /// Behavior/driving preset dropdown.
        /// </summary>
        Behavior,

        /// <summary>
        /// Graphics quality level dropdown.
        /// </summary>
        Graphics

    }

    /// <summary>
    /// Which RCC setting category this dropdown should sync with on Awake.
    /// </summary>
    [Tooltip("Which RCC setting category this dropdown should sync with on Awake.")]
    public Useless useless = Useless.MainController;

    private void Awake() {

        int type = 0;

        if (useless == Useless.Behavior)
            type = Settings.behaviorSelectedIndex;

        if (useless == Useless.MobileControllers) {

            switch (Settings.mobileController) {

                case RCC_Settings.MobileController.TouchScreen:

                    type = 0;

                    break;

                case RCC_Settings.MobileController.Gyro:

                    type = 1;

                    break;

                case RCC_Settings.MobileController.SteeringWheel:

                    type = 2;

                    break;

                case RCC_Settings.MobileController.Joystick:

                    type = 3;

                    break;

            }

        }

        if (useless == Useless.Graphics)
            type = QualitySettings.GetQualityLevel();

        if (!TryGetComponent(out Dropdown dropdown))
            return;

        dropdown.SetValueWithoutNotify(type);
        dropdown.RefreshShownValue();

    }

}
