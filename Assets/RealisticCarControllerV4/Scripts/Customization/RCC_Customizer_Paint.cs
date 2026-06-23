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
/// Individual paint component that can change the color of a specific material on the vehicle.
/// Multiple paint components can target different materials (body, roof, trim, etc.) under a single PaintManager.
/// </summary>
public class RCC_Customizer_Paint : RCC_Core {

    /// <summary>
    /// Cached reference to the parent paint manager.
    /// </summary>
    private RCC_Customizer_PaintManager paintManager;

    /// <summary>
    /// Parent paint manager that owns this paint component.
    /// </summary>
    private RCC_Customizer_PaintManager PaintManager {

        get {

            if (!paintManager)
                paintManager = GetComponentInParent<RCC_Customizer_PaintManager>(true);

            return paintManager;

        }

    }

    /// <summary>
    /// Target material for painting.
    /// </summary>
    [Tooltip("Target material for painting.")]
    public Material paintMaterial;

    /// <summary>
    /// Target keyword for painting. Use "_BaseColor" for URP shaders.
    /// </summary>
    [Tooltip("Target keyword for painting. Use \"_BaseColor\" for URP shaders.")]
    public string id = "_Color";

    /// <summary>
    /// Instanced materials.
    /// </summary>
    private List<Material> instanceMaterials = new List<Material>();

    /// <summary>
    /// Paint the material with target color.
    /// </summary>
    /// <param name="newColor"></param>
    public void UpdatePaint(Color newColor) {

        //  Return if paint material is null.
        if (!paintMaterial) {

#if UNITY_EDITOR
            Debug.LogError("Body material is not selected for this painter, disabling this painter!");
#endif
            enabled = false;
            return;

        }

        if (instanceMaterials == null || (instanceMaterials != null && instanceMaterials.Count == 0))
            instanceMaterials = new List<Material>();

        //  Return if paint manager, mod applier, or car controller is not found.
        if (!PaintManager || !PaintManager.ModApplier || !PaintManager.ModApplier.CarController)
            return;

        //  Getting all mesh renderers and instance of materials.
        MeshRenderer[] meshRenderers = PaintManager.ModApplier.CarController.transform.GetComponentsInChildren<MeshRenderer>(true);

        foreach (MeshRenderer item in meshRenderers) {

            for (int i = 0; i < item.sharedMaterials.Length; i++) {

                if (item.sharedMaterials[i] != null && Equals(item.sharedMaterials[i], paintMaterial))
                    instanceMaterials.Add(item.materials[i]);

            }

        }

        //  Painting all instances.
        for (int i = 0; i < instanceMaterials.Count; i++) {

            if (instanceMaterials[i] != null)
                instanceMaterials[i].SetColor(id, newColor);

        }

    }

    private void OnValidate() {

#if BCG_URP || BCG_HDRP

        if (id == "_Color")
            id = "_BaseColor";

#endif

    }

}
