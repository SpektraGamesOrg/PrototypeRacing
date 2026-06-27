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

#if BCG_URP
using UnityEngine.Rendering.Universal;
#endif

#if BCG_URP

/// <summary>
/// Represents a single decal slot on a vehicle. Wraps a URP DecalProjector and exposes a SetDecal method that swaps in the chosen material at runtime. The scale, pivot, and draw distance of the projector are auto-configured in OnValidate; a default decal material from RCC_Settings is applied if the projector has no material assigned.
/// </summary>
[RequireComponent(typeof(DecalProjector))]
public class RCC_Customizer_Decal : RCC_Core {

    /// <summary>
    /// Decal projector component used to render the decal on the vehicle surface.
    /// </summary>
    private DecalProjector decalRenderer;

    /// <summary>
    /// Sets target material of the decal.
    /// </summary>
    /// <param name="material"></param>
    public void SetDecal(Material material) {

        //  Getting the mesh renderer.
        if (!decalRenderer)
            decalRenderer = GetComponentInChildren<DecalProjector>();

        //  Return if renderer not found.
        if (!decalRenderer)
            return;

        //  Setting material of the renderer.
        decalRenderer.material = material;

    }

    public void OnValidate() {

        if (!TryGetComponent(out DecalProjector dp))
            return;

        dp.scaleMode = DecalScaleMode.InheritFromHierarchy;
        dp.pivot = Vector3.zero;
        dp.drawDistance = 500f;
        
        if(!RCC_Settings.Instance)
            return;

        if (dp.material == null)
            dp.material = RCC_Settings.Instance.defaultDecalMaterial;

        if (dp.material.name.Contains("Default"))
            dp.material = RCC_Settings.Instance.defaultDecalMaterial;

    }

}

#else

/// <summary>
/// Represents a single decal slot on a vehicle. Built-in / HDRP variant — decals are URP-only in RCC, so SetDecal is a no-op outside URP. Define BCG_URP to enable the DecalProjector-backed implementation.
/// </summary>
public class RCC_Customizer_Decal : RCC_Core {

    /// <summary>
    /// Sets target material of the decal.
    /// </summary>
    /// <param name="material"></param>
    public void SetDecal(Material material) {

        //Debug.LogError("Decals are working with URP only!");
        return;

    }

}
#endif