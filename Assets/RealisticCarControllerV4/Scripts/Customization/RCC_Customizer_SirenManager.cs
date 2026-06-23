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
/// Manager for all upgradable sirens on the vehicle. Only one siren can be active at a time. Handles initialization, selection, and save/load via the customizer loadout.
/// </summary>
public class RCC_Customizer_SirenManager : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this siren manager.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// All sirens.
    /// </summary>
    [Tooltip("All sirens.")]
    public RCC_Customizer_Siren[] sirens;

    /// <summary>
    /// Last selected siren index.
    /// </summary>
    [Tooltip("Last selected siren index.")]
    [Min(-1)] public int sirenIndex = -1;

    /// <summary>
    /// Initializes the siren manager by disabling all sirens, then loading and activating the saved siren index from the loadout.
    /// </summary>
    public void Initialize() {

        //  If sirens is null, return.
        if (sirens == null)
            return;

        //  If sirens is null, return.
        if (sirens.Length < 1)
            return;

        //  Disabling all sirens.
        for (int i = 0; i < sirens.Length; i++) {

            if (sirens[i] != null)
                sirens[i].gameObject.SetActive(false);

        }

        //  Getting index of the selected siren.
        sirenIndex = ModApplier.loadout.siren;

        //  Clamping siren index to valid range.
        if (sirenIndex >= sirens.Length)
            sirenIndex = -1;

        //  If siren index is -1, return.
        if (sirenIndex == -1)
            return;

        //  If index is not -1, enable the corresponding siren.
        if (sirens[sirenIndex] != null)
            sirens[sirenIndex].gameObject.SetActive(true);

    }

    /// <summary>
    /// Finds and caches all child RCC_Customizer_Siren components.
    /// </summary>
    public void GetAllSirens() {

        sirens = GetComponentsInChildren<RCC_Customizer_Siren>(true);

    }

    /// <summary>
    /// Disables all siren GameObjects.
    /// </summary>
    public void DisableAll() {

        //  If sirens is null, return.
        if (sirens == null)
            return;

        //  If sirens is null, return.
        if (sirens.Length < 1)
            return;

        //  Disabling all sirens.
        for (int i = 0; i < sirens.Length; i++) {

            if (sirens[i] != null)
                sirens[i].gameObject.SetActive(false);

        }

    }

    /// <summary>
    /// Enables all siren GameObjects.
    /// </summary>
    public void EnableAll() {

        //  If sirens is null, return.
        if (sirens == null)
            return;

        //  If sirens is null, return.
        if (sirens.Length < 1)
            return;

        //  Enabling all sirens.
        for (int i = 0; i < sirens.Length; i++) {

            if (sirens[i] != null)
                sirens[i].gameObject.SetActive(true);

        }

    }

    /// <summary>
    /// Activates the siren at the given index (disabling all others) and saves the loadout.
    /// </summary>
    /// <param name="index">Index of the siren to activate. -1 to deactivate all.</param>
    public void Upgrade(int index) {

        //  If sirens is null, return.
        if (sirens == null)
            return;

        //  If sirens is null, return.
        if (sirens.Length < 1)
            return;

        //  If index is out of range, return.
        if (index >= sirens.Length)
            return;

        //  Index.
        sirenIndex = index;

        //  Disabling all sirens.
        for (int i = 0; i < sirens.Length; i++) {

            if (sirens[i] != null)
                sirens[i].gameObject.SetActive(false);

        }

        //  If index is not -1, enable the corresponding siren.
        if (sirenIndex != -1 && sirens[sirenIndex] != null)
            sirens[sirenIndex].gameObject.SetActive(true);

        //  Refreshing the loadout.
        ModApplier.Refresh(this);

        //  Saving the loadout.
        if (ModApplier.autoSave)
            ModApplier.Save();

    }

    /// <summary>
    /// Activates the siren at the given index (disabling all others) without saving the loadout. Useful for previewing sirens.
    /// </summary>
    /// <param name="index">Index of the siren to activate. -1 to deactivate all.</param>
    public void UpgradeWithoutSave(int index) {

        //  If sirens is null, return.
        if (sirens == null)
            return;

        //  If sirens is null, return.
        if (sirens.Length < 1)
            return;

        //  If index is out of range, return.
        if (index >= sirens.Length)
            return;

        //  Index.
        sirenIndex = index;

        //  Disabling all sirens.
        for (int i = 0; i < sirens.Length; i++) {

            if (sirens[i] != null)
                sirens[i].gameObject.SetActive(false);

        }

        //  If index is not -1, enable the corresponding siren.
        if (sirenIndex != -1 && sirens[sirenIndex] != null)
            sirens[sirenIndex].gameObject.SetActive(true);

    }

    /// <summary>
    /// Restores the settings to default.
    /// </summary>
    public void Restore() {

        sirenIndex = -1;

        //  If sirens is null, return.
        if (sirens == null)
            return;

        //  If sirens is null, return.
        if (sirens.Length < 1)
            return;

        //  Disabling all sirens.
        for (int i = 0; i < sirens.Length; i++) {

            if (sirens[i] != null)
                sirens[i].gameObject.SetActive(false);

        }

    }

}
