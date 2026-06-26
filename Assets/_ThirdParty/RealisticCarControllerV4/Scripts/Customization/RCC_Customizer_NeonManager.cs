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
/// Manager for upgradable neon effects on the vehicle. Handles initialization, material swapping, and save/load via the customizer loadout.
/// </summary>
public class RCC_Customizer_NeonManager : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this neon manager.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (!modApplier)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Neon component used to render the neon effect. Auto-creates if not found as a child.
    /// </summary>
    public RCC_Customizer_Neon Neon {

        get {

            if (_neon == null)
                _neon = GetComponentInChildren<RCC_Customizer_Neon>(true);

            if (_neon == null) {

                _neon = new GameObject("Neon").AddComponent<RCC_Customizer_Neon>();
                _neon.transform.SetParent(transform);
                _neon.transform.localPosition = Vector3.zero;
                _neon.transform.localRotation = Quaternion.identity;

            }

            return _neon;

        }

    }

    /// <summary>
    /// Cached backing field for the Neon property.
    /// </summary>
    private RCC_Customizer_Neon _neon;

    /// <summary>
    /// Indexes of neons.
    /// </summary>
    [Tooltip("Indexes of neons.")]
    [Min(-1)] public int index = -1;

    /// <summary>
    /// Neon materials.
    /// </summary>
    [Tooltip("Neon materials.")]
    public Material[] neons;

    /// <summary>
    /// Empty material.
    /// </summary>
    [Tooltip("Empty material.")]
    public Material neon_Null;

    /// <summary>
    /// Initializes the neon manager by resetting to the empty material, then loading and applying the saved neon index from the loadout.
    /// </summary>
    public void Initialize() {

        //  If neon is null, return.
        if (Neon == null)
            return;

        //  Setting neon material to null.
        if (neon_Null)
            Neon.SetNeonMaterial(neon_Null);

        //  And then getting index values from the loadout. -1 means it's empty.
        index = ModApplier.loadout.neonIndex;

        //  Clamping neon index to valid range.
        if (neons == null || neons.Length < 1 || index >= neons.Length)
            index = -1;

        //  If index is not -1, set material of the neon by the loadout.
        if (index != -1) {

            Neon.gameObject.SetActive(true);
            Neon.SetNeonMaterial(neons[index]);

        }

    }

    /// <summary>
    /// Resets the neon to the empty material and disables the neon GameObject.
    /// </summary>
    public void DisableAll() {

        //  If neon is null, return.
        if (Neon == null)
            return;

        //  If index is not -1, set material of the decal by the loadout.
        Neon.SetNeonMaterial(neon_Null);

        //  Disabling the neon.
        Neon.gameObject.SetActive(false);

    }

    /// <summary>
    /// Resets the neon to the empty material and enables the neon GameObject.
    /// </summary>
    public void EnableAll() {

        //  If neon is null, return.
        if (Neon == null)
            return;

        //  If index is not -1, set material of the decal by the loadout.
        Neon.SetNeonMaterial(neon_Null);

        //  Enabling the neon.
        Neon.gameObject.SetActive(true);

    }

    /// <summary>
    /// Applies a neon material and saves the loadout.
    /// </summary>
    /// <param name="material">Neon material to apply.</param>
    public void Upgrade(Material material) {

        //  If neon is null, return.
        if (Neon == null)
            return;

        //  Enabling the neon.
        Neon.gameObject.SetActive(true);

        //  Setting neon material.
        Neon.SetNeonMaterial(material);

        //  Finding index.
        index = FindMaterialIndex(material);

        //  Refreshing the loadout.
        ModApplier.Refresh(this);

        //  Saving the loadout.
        if (ModApplier.autoSave)
            ModApplier.Save();

    }

    /// <summary>
    /// Applies a neon material without saving the loadout. Useful for previewing neon effects.
    /// </summary>
    /// <param name="material">Neon material to apply.</param>
    public void UpgradeWithoutSave(Material material) {

        //  If neon is null, return.
        if (Neon == null)
            return;

        //  Enabling the neon.
        Neon.gameObject.SetActive(true);

        //  Setting neon material.
        Neon.SetNeonMaterial(material);

        //  Finding index.
        index = FindMaterialIndex(material);

    }

    /// <summary>
    /// Restores the settings to default.
    /// </summary>
    public void Restore() {

        //  If empty decal is null, return.
        if (neon_Null == null)
            return;

        //  Setting the neon material to null.
        Neon.SetNeonMaterial(neon_Null);

        //  Disabling the neon.
        Neon.gameObject.SetActive(false);

    }

    /// <summary>
    /// Finds index of the material.
    /// </summary>
    /// <param name="_material"></param>
    /// <returns></returns>
    private int FindMaterialIndex(Material _material) {

        int index = -1;

        if (neons != null) {

            for (int i = 0; i < neons.Length; i++) {

                if (neons[i] == _material)
                    index = i;

            }

        }

        return index;

    }

}
