//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI button that sets the decal placement location on all <see cref="RCC_UI_Decal"/> buttons in the scene.
/// When clicked, it updates every decal button's location so that subsequent decal applications
/// target the desired vehicle surface (front, back, left, or right).
/// </summary>
public class RCC_UI_DecalSetLocation : RCC_Core {

    /// <summary>
    /// Location of the decal. 0 is front, 1 is back, 2 is left, and 3 is right.
    /// </summary>
    [Tooltip("Location of the decal. 0 is front, 1 is back, 2 is left, and 3 is right.")]
    [Min(0)] public int location = 0;

    /// <summary>
    /// Finds all <see cref="RCC_UI_Decal"/> instances in the scene and sets their location to this button's configured location.
    /// </summary>
    public void Upgrade() {

#if !UNITY_2022_1_OR_NEWER
        RCC_UI_Decal[] decalButtons = FindObjectsOfType<RCC_UI_Decal>(true);
#else
        RCC_UI_Decal[] decalButtons = FindObjectsByType<RCC_UI_Decal>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif

        if (decalButtons == null)
            return;

        if (decalButtons.Length < 1)
            return;

        foreach (RCC_UI_Decal item in decalButtons)
            item.SetLocation(location);

    }

    /// <summary>
    /// Sets the location of the decal. 0 = front, 1 = back, 2 = left, 3 = right.
    /// </summary>
    /// <param name="_location">The target decal location index.</param>
    public void SetLocation(int _location) {

        location = _location;

    }

}
