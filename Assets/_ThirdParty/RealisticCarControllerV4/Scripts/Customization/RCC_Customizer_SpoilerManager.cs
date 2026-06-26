//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manager for all upgradable spoilers on the vehicle. Only one spoiler can be active at a time. Handles initialization, selection, painting, and save/load via the customizer loadout.
/// </summary>
public class RCC_Customizer_SpoilerManager : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this spoiler manager.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// All upgradable spoilers.
    /// </summary>
    [Tooltip("All upgradable spoilers.")]
    public RCC_Customizer_Spoiler[] spoilers;

    /// <summary>
    /// Last selected spoiler index.
    /// </summary>
    [Tooltip("Last selected spoiler index.")]
    [Min(-1)] public int spoilerIndex = -1;

    /// <summary>
    /// Painting the spoilers?
    /// </summary>
    [Tooltip("Painting the spoilers?")]
    public bool paintSpoilers = true;

    /// <summary>
    /// Initializes the spoiler manager by disabling all spoilers, then loading and activating the saved spoiler index from the loadout. Also applies paint if saved.
    /// </summary>
    public void Initialize() {

        //  If spoilers is null, return.
        if (spoilers == null)
            return;

        //  If spoilers is null, return
        if (spoilers.Length < 1)
            return;

        //  Disabling all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].gameObject.SetActive(false);

        }

        //  Getting index of the loadouts spoiler.
        spoilerIndex = ModApplier.loadout.spoiler;

        //  Clamping spoiler index to valid range.
        if (spoilerIndex >= spoilers.Length)
            spoilerIndex = -1;

        //  If spoiler index is -1, return.
        if (spoilerIndex == -1)
            return;

        //  If index is not -1, enable the corresponding spoiler.
        if (spoilers[spoilerIndex] != null)
            spoilers[spoilerIndex].gameObject.SetActive(true);

        //  Getting saved color of the spoiler.
        if (ModApplier.loadout.paint != new Color(1f, 1f, 1f, 0f))
            Paint(ModApplier.loadout.paint);

    }

    /// <summary>
    /// Finds and caches all child RCC_Customizer_Spoiler components.
    /// </summary>
    public void GetAllSpoilers() {

        spoilers = GetComponentsInChildren<RCC_Customizer_Spoiler>(true);

    }

    /// <summary>
    /// Disables all spoiler GameObjects.
    /// </summary>
    public void DisableAll() {

        //  If spoilers is null, return.
        if (spoilers == null)
            return;

        //  If spoilers is null, return
        if (spoilers.Length < 1)
            return;

        //  Disabling all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].gameObject.SetActive(false);

        }

    }

    /// <summary>
    /// Enables all spoiler GameObjects.
    /// </summary>
    public void EnableAll() {

        //  If spoilers is null, return.
        if (spoilers == null)
            return;

        //  If spoilers is null, return
        if (spoilers.Length < 1)
            return;

        //  Enabling all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].gameObject.SetActive(true);

        }

    }

    /// <summary>
    /// Activates the spoiler at the given index (disabling all others), applies paint if available, and saves the loadout.
    /// </summary>
    /// <param name="index">Index of the spoiler to activate. -1 to deactivate all.</param>
    public void Upgrade(int index) {

        //  If sirens is null, return.
        if (spoilers == null)
            return;

        if (spoilers.Length < 1)
            return;

        //  If index is out of range, return.
        if (index >= spoilers.Length)
            return;

        //  Index of the spoiler.
        spoilerIndex = index;

        //  Disabling all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].gameObject.SetActive(false);

        }

        //  If spoiler index is -1, return.
        if (spoilerIndex == -1)
            return;

        //  If index is not -1, enable the corresponding spoiler.
        if (spoilerIndex != -1 && spoilers[spoilerIndex] != null)
            spoilers[spoilerIndex].gameObject.SetActive(true);

        if (spoilerIndex != -1 && ModApplier.loadout.paint != new Color(1f, 1f, 1f, 0f) && spoilers[spoilerIndex].bodyRenderer != null)
            Paint(ModApplier.loadout.paint);

        //  Refreshing the loadout.
        ModApplier.Refresh(this);

        //  Saving the loadout.
        if (ModApplier.autoSave)
            ModApplier.Save();

    }

    /// <summary>
    /// Activates the spoiler at the given index (disabling all others) without saving the loadout. Useful for previewing spoilers.
    /// </summary>
    /// <param name="index">Index of the spoiler to activate. -1 to deactivate all.</param>
    public void UpgradeWithoutSave(int index) {

        //  If sirens is null, return.
        if (spoilers == null)
            return;

        if (spoilers.Length < 1)
            return;

        //  If index is out of range, return.
        if (index >= spoilers.Length)
            return;

        //  Index of the spoiler.
        spoilerIndex = index;

        //  Disabling all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].gameObject.SetActive(false);

        }

        //  If spoiler index is -1, return.
        if (spoilerIndex == -1)
            return;

        //  If index is not -1, enable the corresponding spoiler.
        if (spoilers[spoilerIndex] != null)
            spoilers[spoilerIndex].gameObject.SetActive(true);

        if (ModApplier.loadout.paint != new Color(1f, 1f, 1f, 0f) && spoilers[spoilerIndex].bodyRenderer != null)
            Paint(ModApplier.loadout.paint);

    }

    /// <summary>
    /// Paints all spoilers with the given color.
    /// </summary>
    /// <param name="newColor">Color to apply to all spoiler body renderers.</param>
    public void Paint(Color newColor) {

        //  If spoilers is null, return.
        if (spoilers == null)
            return;

        //  If spoilers is null, return.
        if (spoilers.Length < 1)
            return;

        //  If spoiler index is -1, return.
        if (spoilerIndex == -1)
            return;

        //  Painting all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].UpdatePaint(newColor);

        }

    }

    /// <summary>
    /// Restores the settings to default.
    /// </summary>
    public void Restore() {

        spoilerIndex = -1;

        //  If sirens is null, return.
        if (spoilers == null)
            return;

        if (spoilers.Length < 1)
            return;

        //  Disabling all spoilers.
        for (int i = 0; i < spoilers.Length; i++) {

            if (spoilers[i] != null)
                spoilers[i].gameObject.SetActive(false);

        }

    }

}
