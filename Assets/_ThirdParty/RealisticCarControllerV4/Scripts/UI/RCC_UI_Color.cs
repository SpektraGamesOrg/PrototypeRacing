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
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI button that applies a preset paint color to the active player vehicle when clicked.
/// Each instance is configured with a <see cref="PickedColor"/> enum value to determine which color to apply.
/// </summary>
public class RCC_UI_Color : RCC_Core {

    /// <summary>
    /// The preset color this button will apply to the vehicle when clicked.
    /// </summary>
    [Tooltip("The preset color this button will apply to the vehicle when clicked.")]
    public PickedColor _pickedColor = PickedColor.Orange;

    /// <summary>
    /// Available preset paint colors for vehicle customization.
    /// </summary>
    public enum PickedColor { Orange, Red, Green, Blue, Black, White, Cyan, Magenta, Pink }

    /// <summary>
    /// Called when the button is clicked. Converts the selected <see cref="PickedColor"/> to a Unity Color
    /// and applies it to the vehicle via the <see cref="RCC_CustomizationManager"/>.
    /// </summary>
    public void OnClick() {

        RCC_CustomizationManager handler = RCC_CustomizationManager.Instance;

        if (!handler) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the vehicle, but there is no ''RCC_CustomizationManager'' in your scene yet.");
#endif
            return;

        }

        Color selectedColor = new Color();

        switch (_pickedColor) {

            case PickedColor.Orange:
                selectedColor = Color.red + (Color.green / 2f);
                break;

            case PickedColor.Red:
                selectedColor = Color.red;
                break;

            case PickedColor.Green:
                selectedColor = Color.green;
                break;

            case PickedColor.Blue:
                selectedColor = Color.blue;
                break;

            case PickedColor.Black:
                selectedColor = Color.black;
                break;

            case PickedColor.White:
                selectedColor = Color.white;
                break;

            case PickedColor.Cyan:
                selectedColor = Color.cyan;
                break;

            case PickedColor.Magenta:
                selectedColor = Color.magenta;
                break;

            case PickedColor.Pink:
                selectedColor = new Color(1, 0f, .5f);
                break;

        }

        handler.Paint(selectedColor);

    }

}
