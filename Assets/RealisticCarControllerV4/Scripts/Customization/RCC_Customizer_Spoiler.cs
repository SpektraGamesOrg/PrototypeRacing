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
/// Represents an individual spoiler attachment on the vehicle. Can be painted to match the vehicle body color via a specified material index on its mesh renderer.
/// </summary>
public class RCC_Customizer_Spoiler : RCC_Core {

    /// <summary>
    /// Cached reference to the parent RCC_Customizer.
    /// </summary>
    private RCC_Customizer modApplier;

    /// <summary>
    /// Parent customizer that owns this spoiler.
    /// </summary>
    public RCC_Customizer ModApplier {

        get {

            if (modApplier == null)
                modApplier = GetComponentInParent<RCC_Customizer>(true);

            return modApplier;

        }

    }

    /// <summary>
    /// Mesh renderer of the spoiler used for painting.
    /// </summary>
    [Tooltip("Mesh renderer of the spoiler used for painting.")]
    public MeshRenderer bodyRenderer;

    /// <summary>
    /// Material index on the body renderer to paint. -1 means the spoiler is not paintable.
    /// </summary>
    [Tooltip("Material index on the body renderer to paint. -1 means the spoiler is not paintable.")]
    [Min(-1)]
    public int index = -1;

    /// <summary>
    /// Current paint color of the spoiler.
    /// </summary>
    private Color color = Color.gray;

    private void OnEnable() {

        //  If index is set to -1, no need to paint it.
        if (index == -1)
            return;

        //  Return if ModApplier is not found.
        if (!ModApplier)
            return;

        //  Getting saved color of the spoiler.
        if (ModApplier.loadout.paint != new Color(1f, 1f, 1f, 0f))
            color = ModApplier.loadout.paint;

        //  Painting target material.
        if (bodyRenderer && index >= 0 && index < bodyRenderer.materials.Length)
            bodyRenderer.materials[index].color = color;
#if UNITY_EDITOR
        else
            Debug.LogError("Body renderer of this spoiler is not selected!");
#endif

    }

    /// <summary>
    /// Paints the spoiler's body renderer material with the given color.
    /// </summary>
    /// <param name="newColor">New paint color to apply.</param>
    public void UpdatePaint(Color newColor) {

        if (index == -1)
            return;

        if (bodyRenderer && index >= 0 && index < bodyRenderer.materials.Length)
            bodyRenderer.materials[index].color = newColor;
#if UNITY_EDITOR
        else if (!bodyRenderer)
            Debug.LogError("Body renderer of this spoiler is not selected!");
#endif

    }

    private void Reset() {

        bodyRenderer = GetComponent<MeshRenderer>();

    }

}
