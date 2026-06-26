//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


#pragma warning disable 0414

using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Mobile UI drag handler that forwards touch/mouse drag events to the showroom camera for orbit control.
/// Place this on a full-screen UI panel in the showroom/customization scene to capture drag input.
/// </summary>
public class RCC_UI_ShowroomCameraDrag : RCC_Core, IDragHandler, IEndDragHandler {

    /// <summary>
    /// Reference to the showroom camera in the scene, found at Awake.
    /// </summary>
    private RCC_ShowroomCamera showroomCamera;

    private void Awake() {

#if !UNITY_2022_1_OR_NEWER
        showroomCamera = FindObjectOfType<RCC_ShowroomCamera>();
#else
        showroomCamera = FindFirstObjectByType<RCC_ShowroomCamera>();
#endif

    }

    /// <summary>
    /// Called while the user is dragging on this UI element. Forwards the drag data to the showroom camera.
    /// </summary>
    /// <param name="data">Pointer event data containing the drag delta and position.</param>
    public void OnDrag(PointerEventData data) {

        if (showroomCamera)
            showroomCamera.OnDrag(data);

    }

    /// <summary>
    /// Called when the user stops dragging. Required by <see cref="IEndDragHandler"/> but currently unused.
    /// </summary>
    /// <param name="data">Pointer event data for the end drag event.</param>
    public void OnEndDrag(PointerEventData data) {



    }

}
