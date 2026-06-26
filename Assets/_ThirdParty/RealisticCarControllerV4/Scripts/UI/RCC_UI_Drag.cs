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
using UnityEngine.EventSystems;

/// <summary>
/// Mobile UI drag handler that forwards touch/mouse drag events to the active RCC player camera
/// for orbiting control. Place this on a full-screen UI panel to capture drag input on mobile devices.
/// </summary>
public class RCC_UI_Drag : RCC_Core, IDragHandler, IEndDragHandler {

    /// <summary>
    /// Called while the user is dragging on this UI element. Forwards the drag data to the active player camera.
    /// </summary>
    /// <param name="data">Pointer event data containing the drag delta and position.</param>
    public void OnDrag(PointerEventData data) {

        //  Return if no player camera found.
        if (!RCCSceneManager.activePlayerCamera)
            return;

        RCCSceneManager.activePlayerCamera.OnDrag(data);

    }

    /// <summary>
    /// Called when the user stops dragging. Required by <see cref="IEndDragHandler"/> but currently unused.
    /// </summary>
    /// <param name="data">Pointer event data for the end drag event.</param>
    public void OnEndDrag(PointerEventData data) {



    }

}
