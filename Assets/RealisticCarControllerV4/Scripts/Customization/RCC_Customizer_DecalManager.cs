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
/// Manager for upgradable decals on all four sides of the vehicle (front, back, left, right).
/// Handles initialization, upgrade, and restoration of decal materials from the customizer loadout.
/// </summary>
public class RCC_Customizer_DecalManager : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this decal manager.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (!modApplier)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Decal projector for the front of the vehicle.
    /// </summary>
    [Tooltip("Decal projector for the front of the vehicle.")]
    public RCC_Customizer_Decal decalFront;

    /// <summary>
    /// Decal projector for the back of the vehicle.
    /// </summary>
    [Tooltip("Decal projector for the back of the vehicle.")]
    public RCC_Customizer_Decal decalBack;

    /// <summary>
    /// Decal projector for the left side of the vehicle.
    /// </summary>
    [Tooltip("Decal projector for the left side of the vehicle.")]
    public RCC_Customizer_Decal decalLeft;

    /// <summary>
    /// Decal projector for the right side of the vehicle.
    /// </summary>
    [Tooltip("Decal projector for the right side of the vehicle.")]
    public RCC_Customizer_Decal decalRight;

    /// <summary>
    /// Currently selected decal index for the front. -1 means no decal applied.
    /// </summary>
    [Tooltip("Currently selected decal index for the front. -1 means no decal applied.")]
    [Min(-1)] public int index_decalFront = -1;

    /// <summary>
    /// Currently selected decal index for the back. -1 means no decal applied.
    /// </summary>
    [Tooltip("Currently selected decal index for the back. -1 means no decal applied.")]
    [Min(-1)] public int index_decalBack = -1;

    /// <summary>
    /// Currently selected decal index for the left side. -1 means no decal applied.
    /// </summary>
    [Tooltip("Currently selected decal index for the left side. -1 means no decal applied.")]
    [Min(-1)] public int index_decalLeft = -1;

    /// <summary>
    /// Currently selected decal index for the right side. -1 means no decal applied.
    /// </summary>
    [Tooltip("Currently selected decal index for the right side. -1 means no decal applied.")]
    [Min(-1)] public int index_decalRight = -1;

    /// <summary>
    /// Decal materials.
    /// </summary>
    [Tooltip("Decal materials.")]
    public Material[] decals;

    /// <summary>
    /// Empty decal material.
    /// </summary>
    [Tooltip("Empty decal material.")]
    public Material decal_Null;

    /// <summary>
    /// Initializes the decal manager by clearing all decals, then loading and applying saved decal indices from the loadout.
    /// </summary>
    public void Initialize() {

        //  If empty decal is null, return.
        if (decal_Null != null) {

            if (decalFront != null)
                decalFront.SetDecal(decal_Null);
            if (decalBack != null)
                decalBack.SetDecal(decal_Null);
            if (decalLeft != null)
                decalLeft.SetDecal(decal_Null);
            if (decalRight != null)
                decalRight.SetDecal(decal_Null);

        }

        //  And then getting index values from the loadout. -1 means it's empty.
        index_decalFront = ModApplier.loadout.decalIndexFront;
        index_decalBack = ModApplier.loadout.decalIndexBack;
        index_decalLeft = ModApplier.loadout.decalIndexLeft;
        index_decalRight = ModApplier.loadout.decalIndexRight;

        if (decalFront != null)
            decalFront.gameObject.SetActive(false);
        if (decalBack != null)
            decalBack.gameObject.SetActive(false);
        if (decalLeft != null)
            decalLeft.gameObject.SetActive(false);
        if (decalRight != null)
            decalRight.gameObject.SetActive(false);

        //  If decals is null, return.
        if (decals == null)
            return;

        //  If decals is null, return
        if (decals.Length < 1)
            return;

        //  Clamping decal indices to valid range.
        if (index_decalFront >= decals.Length)
            index_decalFront = -1;
        if (index_decalBack >= decals.Length)
            index_decalBack = -1;
        if (index_decalLeft >= decals.Length)
            index_decalLeft = -1;
        if (index_decalRight >= decals.Length)
            index_decalRight = -1;

        //  If index is not -1, set material of the decal by the loadout.
        if (index_decalFront != -1 && decalFront != null) {

            decalFront.gameObject.SetActive(true);
            decalFront.SetDecal(decals[index_decalFront]);

        }

        if (index_decalBack != -1 && decalBack != null) {

            decalBack.gameObject.SetActive(true);
            decalBack.SetDecal(decals[index_decalBack]);

        }

        if (index_decalLeft != -1 && decalLeft != null) {

            decalLeft.gameObject.SetActive(true);
            decalLeft.SetDecal(decals[index_decalLeft]);

        }

        if (index_decalRight != -1 && decalRight != null) {

            decalRight.gameObject.SetActive(true);
            decalRight.SetDecal(decals[index_decalRight]);

        }

    }

    /// <summary>
    /// Disables all decal projectors and resets them to the empty material.
    /// </summary>
    public void DisableAll() {

        //  If empty decal is null, return.
        if (decal_Null != null) {

            if (decalFront != null)
                decalFront.SetDecal(decal_Null);
            if (decalBack != null)
                decalBack.SetDecal(decal_Null);
            if (decalLeft != null)
                decalLeft.SetDecal(decal_Null);
            if (decalRight != null)
                decalRight.SetDecal(decal_Null);

        }

        if (decalFront != null)
            decalFront.gameObject.SetActive(false);
        if (decalBack != null)
            decalBack.gameObject.SetActive(false);
        if (decalLeft != null)
            decalLeft.gameObject.SetActive(false);
        if (decalRight != null)
            decalRight.gameObject.SetActive(false);

    }

    /// <summary>
    /// Enables all decal projector GameObjects and resets them to the empty material.
    /// </summary>
    public void EnableAll() {

        //  If empty decal is null, return.
        if (decal_Null != null) {

            if (decalFront != null)
                decalFront.SetDecal(decal_Null);
            if (decalBack != null)
                decalBack.SetDecal(decal_Null);
            if (decalLeft != null)
                decalLeft.SetDecal(decal_Null);
            if (decalRight != null)
                decalRight.SetDecal(decal_Null);

        }

        if (decalFront != null)
            decalFront.gameObject.SetActive(true);
        if (decalBack != null)
            decalBack.gameObject.SetActive(true);
        if (decalLeft != null)
            decalLeft.gameObject.SetActive(true);
        if (decalRight != null)
            decalRight.gameObject.SetActive(true);

    }

    /// <summary>
    /// Applies a decal material to the specified location and saves the loadout.
    /// </summary>
    /// <param name="location">Decal location: 0 = front, 1 = back, 2 = left, 3 = right.</param>
    /// <param name="material">Decal material to apply.</param>
    public void Upgrade(int location, Material material) {

        //  Setting material depending on the location. 0 is front, 1 is back, 2 is left, and 3 is right.
        switch (location) {

            case 0:
                decalFront.gameObject.SetActive(true);
                decalFront.SetDecal(material);
                index_decalFront = FindMaterialIndex(material);
                break;

            case 1:
                decalBack.gameObject.SetActive(true);
                decalBack.SetDecal(material);
                index_decalBack = FindMaterialIndex(material);
                break;

            case 2:
                decalLeft.gameObject.SetActive(true);
                decalLeft.SetDecal(material);
                index_decalLeft = FindMaterialIndex(material);
                break;

            case 3:
                decalRight.gameObject.SetActive(true);
                decalRight.SetDecal(material);
                index_decalRight = FindMaterialIndex(material);
                break;

        }

        //  Refreshing the loadout.
        ModApplier.Refresh(this);

        //  Saving the loadout.
        if (ModApplier.autoSave)
            ModApplier.Save();

    }

    /// <summary>
    /// Applies a decal material to the specified location without saving the loadout. Useful for previewing decals.
    /// </summary>
    /// <param name="location">Decal location: 0 = front, 1 = back, 2 = left, 3 = right.</param>
    /// <param name="material">Decal material to apply.</param>
    public void UpgradeWithoutSave(int location, Material material) {

        //  Setting material depending on the location. 0 is front, 1 is back, 2 is left, and 3 is right.
        switch (location) {

            case 0:
                decalFront.gameObject.SetActive(true);
                decalFront.SetDecal(material);
                index_decalFront = FindMaterialIndex(material);
                break;

            case 1:
                decalBack.gameObject.SetActive(true);
                decalBack.SetDecal(material);
                index_decalBack = FindMaterialIndex(material);
                break;

            case 2:
                decalLeft.gameObject.SetActive(true);
                decalLeft.SetDecal(material);
                index_decalLeft = FindMaterialIndex(material);
                break;

            case 3:
                decalRight.gameObject.SetActive(true);
                decalRight.SetDecal(material);
                index_decalRight = FindMaterialIndex(material);
                break;

        }

    }

    /// <summary>
    /// Restores the settings to default.
    /// </summary>
    public void Restore() {

        //  If empty decal is null, return.
        if (decal_Null != null) {

            if (decalFront != null)
                decalFront.SetDecal(decal_Null);
            if (decalBack != null)
                decalBack.SetDecal(decal_Null);
            if (decalLeft != null)
                decalLeft.SetDecal(decal_Null);
            if (decalRight != null)
                decalRight.SetDecal(decal_Null);

        }

        if (decalFront != null)
            decalFront.gameObject.SetActive(false);
        if (decalBack != null)
            decalBack.gameObject.SetActive(false);
        if (decalLeft != null)
            decalLeft.gameObject.SetActive(false);
        if (decalRight != null)
            decalRight.gameObject.SetActive(false);

    }

    /// <summary>
    /// Finds index of the material.
    /// </summary>
    /// <param name="_material"></param>
    /// <returns></returns>
    private int FindMaterialIndex(Material _material) {

        int index = -1;

        if (decals != null) {

            for (int i = 0; i < decals.Length; i++) {

                if (decals[i] != null && Equals(decals[i], _material))
                    index = i;

            }

        }

        return index;

    }

}
