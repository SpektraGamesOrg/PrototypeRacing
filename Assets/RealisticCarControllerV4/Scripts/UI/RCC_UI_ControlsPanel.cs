//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;

/// <summary>
/// Simple panel toggle controller for the keyboard controls reference overlay.
/// Used in the AIO demo scene selector to show/hide a keybinding reference panel.
/// </summary>
public class RCC_UI_ControlsPanel : RCC_Core {

    /// <summary>
    /// The controls panel GameObject to show or hide.
    /// </summary>
    [Tooltip("The controls panel GameObject to show or hide.")]
    public GameObject controlsPanel;

    /// <summary>
    /// Toggles the controls panel visibility.
    /// </summary>
    public void TogglePanel() {

        if (!controlsPanel)
            return;

        controlsPanel.SetActive(!controlsPanel.activeSelf);

    }

    /// <summary>
    /// Opens the controls panel.
    /// </summary>
    public void Open() {

        if (!controlsPanel)
            return;

        controlsPanel.SetActive(true);

    }

    /// <summary>
    /// Closes the controls panel.
    /// </summary>
    public void Close() {

        if (!controlsPanel)
            return;

        controlsPanel.SetActive(false);

    }

}
