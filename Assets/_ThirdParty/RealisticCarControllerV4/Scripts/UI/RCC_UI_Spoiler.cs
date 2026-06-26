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
using UnityEngine.UI;

/// <summary>
/// UI button that equips a spoiler on the active player vehicle when clicked.
/// Each instance is configured with a spoiler index corresponding to a spoiler prefab in the spoiler manager.
/// </summary>
public class RCC_UI_Spoiler : RCC_Core {

    /// <summary>
    /// Index of the spoiler prefab to equip from the spoiler manager's available spoilers.
    /// </summary>
    [Tooltip("Index of the spoiler prefab to equip from the spoiler manager's available spoilers.")]
    [Min(0)]
    public int index = 0;

    /// <summary>
    /// Equips the spoiler at the configured index on the active player vehicle via the customization manager.
    /// </summary>
    public void Upgrade() {

        RCC_CustomizationManager handler = RCC_CustomizationManager.Instance;

        if (!handler) {

#if UNITY_EDITOR
            Debug.LogError("You are trying to customize the vehicle, but there is no ''RCC_CustomizationManager'' in your scene yet.");
#endif
            return;

        }

        handler.Spoiler(index);

    }

}
