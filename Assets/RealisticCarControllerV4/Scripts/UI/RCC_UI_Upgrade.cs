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
using TMPro;

/// <summary>
/// UI button for upgrading a specific performance category (Engine, Handling, Brake, or Speed)
/// on the active player vehicle. Displays the current upgrade level and increments it on click (max level 5).
/// </summary>
public class RCC_UI_Upgrade : RCC_Core {

    /// <summary>
    /// The performance category this button upgrades when clicked.
    /// </summary>
    [Tooltip("The performance category this button upgrades when clicked.")]
    public UpgradeClass upgradeClass = UpgradeClass.Engine;

    /// <summary>
    /// Available performance upgrade categories.
    /// </summary>
    public enum UpgradeClass { Engine, Handling, Brake, Speed }

    /// <summary>
    /// Text label displaying the current upgrade level for this category.
    /// </summary>
    [Tooltip("Text label displaying the current upgrade level for this category.")]
    public TextMeshProUGUI levelText;

    private void OnEnable() {

        CheckLevel();

    }

    /// <summary>
    /// Reads the current upgrade level from the vehicle's upgrade manager and updates the level text display.
    /// </summary>
    public void CheckLevel() {

        if (!levelText)
            return;

        RCC_CarControllerV4 currentPlayer = RCCSceneManager.activePlayerVehicle;

        if (!currentPlayer) {

            levelText.text = "-";
            return;

        }

        if (!currentPlayer.Customizer) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the player vehicle, customization is not enabled on this vehicle.");
#endif
            levelText.text = "-";
            return;

        }

        if (!currentPlayer.Customizer.UpgradeManager) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the player vehicle, customization is enabled but upgrade manager couldn't found on this vehicle.");
#endif
            levelText.text = "-";
            return;

        }

        switch (upgradeClass) {

            case UpgradeClass.Engine:
                levelText.text = currentPlayer.Customizer.UpgradeManager.EngineLevel.ToString();
                break;
            case UpgradeClass.Handling:
                levelText.text = currentPlayer.Customizer.UpgradeManager.HandlingLevel.ToString();
                break;
            case UpgradeClass.Brake:
                levelText.text = currentPlayer.Customizer.UpgradeManager.BrakeLevel.ToString();
                break;
            case UpgradeClass.Speed:
                levelText.text = currentPlayer.Customizer.UpgradeManager.SpeedLevel.ToString();
                break;

        }

    }

    /// <summary>
    /// Called when the upgrade button is clicked. Increments the upgrade level for the assigned category
    /// (if below the maximum level of 5) and refreshes the level text display.
    /// </summary>
    public void OnClick() {

        RCC_CarControllerV4 currentPlayer = RCCSceneManager.activePlayerVehicle;

        if (!currentPlayer) {

#if UNITY_EDITOR
            Debug.LogError("There are no any player controller vehicle in the scene yet.");
#endif
            return;

        }

        if (!currentPlayer.Customizer) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the player vehicle, customization is not enabled on this vehicle.");
#endif
            return;

        }

        if (!currentPlayer.Customizer.UpgradeManager) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the player vehicle, customization is enabled but upgrade manager couldn't found on this vehicle.");
#endif

            if (levelText)
                levelText.text = "-";

            return;

        }

        switch (upgradeClass) {

            case UpgradeClass.Engine:
                if (currentPlayer.Customizer.UpgradeManager.EngineLevel < 5)
                    currentPlayer.Customizer.UpgradeManager.UpgradeEngine();
                break;
            case UpgradeClass.Handling:
                if (currentPlayer.Customizer.UpgradeManager.HandlingLevel < 5)
                    currentPlayer.Customizer.UpgradeManager.UpgradeHandling();
                break;
            case UpgradeClass.Brake:
                if (currentPlayer.Customizer.UpgradeManager.BrakeLevel < 5)
                    currentPlayer.Customizer.UpgradeManager.UpgradeBrake();
                break;
            case UpgradeClass.Speed:
                if (currentPlayer.Customizer.UpgradeManager.SpeedLevel < 5)
                    currentPlayer.Customizer.UpgradeManager.UpgradeSpeed();
                break;

        }

        CheckLevel();

    }

}
