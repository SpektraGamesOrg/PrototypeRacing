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

/// <summary>
/// Renders a mirror effect by inverting the attached Camera's projection matrix.
/// Must be attached to a dedicated Camera GameObject on the vehicle (e.g., side mirror or rear-view mirror).
/// </summary>
public class RCC_Mirror : RCC_Core {

    /// <summary>
    /// The Camera component used to render the mirror view.
    /// </summary>
    private Camera cam;

    private void Awake() {

        //  Getting camera. If no camera found, return.
        if (!TryGetComponent(out cam)) {
            enabled = false;
            return;
        }

        //  Inverting the camera for mirror effect.
        InvertCamera();

    }

    private void OnEnable() {

        StartCoroutine(FixDepth());

    }

    /// <summary>
    /// Adjusts the camera depth after a frame to ensure correct render order for mirror rendering.
    /// </summary>
    private IEnumerator FixDepth() {

        if (!cam)
            yield break;

        yield return new WaitForEndOfFrame();
        cam.depth = 1f;

    }

    /// <summary>
    /// Inverts the camera projection matrix horizontally to create a mirror reflection effect.
    /// </summary>
    private void InvertCamera() {

        cam.ResetWorldToCameraMatrix();
        cam.ResetProjectionMatrix();
        cam.projectionMatrix *= Matrix4x4.Scale(new Vector3(-1, 1, 1));

    }

    private void OnPreRender() {

        GL.invertCulling = true;

    }

    private void OnPostRender() {

        GL.invertCulling = false;

    }

    private void Update() {

        //  Enable or disable with controllable state of the vehicle.
        if (!CarController)
            return;

        cam.enabled = CarController.canControl;

    }

}
