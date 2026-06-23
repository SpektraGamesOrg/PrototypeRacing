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
using TMPro;

/// <summary>
/// Reads the current value from a UI Slider each frame and displays it as formatted text (one decimal place).
/// Automatically finds the Slider in the parent and the TextMeshProUGUI in children if not assigned.
/// </summary>
public class RCC_UI_SliderTextReader : RCC_Core {

    /// <summary>
    /// The UI Slider whose value will be displayed. Auto-found in parent if not assigned.
    /// </summary>
    [Tooltip("The UI Slider whose value will be displayed. Auto-found in parent if not assigned.")]
    public Slider slider;

    /// <summary>
    /// The TextMeshProUGUI component that displays the slider's current value. Auto-found in children if not assigned.
    /// </summary>
    [Tooltip("The TextMeshProUGUI component that displays the slider's current value. Auto-found in children if not assigned.")]
    public TextMeshProUGUI text;

    private void Awake() {

        if (!slider)
            slider = GetComponentInParent<Slider>();

        if (!text)
            text = GetComponentInChildren<TextMeshProUGUI>();

    }

    private void Update() {

        if (!slider || !text)
            return;

        text.text = slider.value.ToString("F1");

    }

}
