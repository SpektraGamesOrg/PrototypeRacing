//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sets the layer of this GameObject (and optionally its children) on enable.
/// Useful for ensuring vehicle parts are on the correct physics or rendering layer at runtime.
/// </summary>
public class RCC_SetLayer : RCC_Core {

    /// <summary>
    /// Name of the layer to assign to this GameObject.
    /// </summary>
    [Tooltip("Name of the layer to assign to this GameObject.")]
    public string layer = "Default";

    /// <summary>
    /// If true, also applies the layer to all direct child transforms.
    /// </summary>
    [Tooltip("If true, also applies the layer to all direct child transforms.")]
    public bool setChildren = false;

    private void OnEnable() {

        int layerIndex = LayerMask.NameToLayer(layer);

        if (layerIndex == -1)
            return;

        gameObject.layer = layerIndex;

        if (setChildren) {

            foreach (Transform item in transform)
                item.gameObject.layer = layerIndex;

        }

    }

}
