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
/// Trigger-based attachment point for trailers. When this trigger collider overlaps another
/// RCC_TrailerAttachPoint on a vehicle, it sends an attach message to the parent trailer's ConfigurableJoint.
/// </summary>
public class RCC_TrailerAttachPoint : RCC_Core {

    private void OnTriggerEnter(Collider col) {

        //  Getting other attacher. If no attacher found, return.
        if (!col.gameObject.TryGetComponent(out RCC_TrailerAttachPoint otherAttacher))
            return;

        //  Other vehicle.
        RCC_CarControllerV4 otherVehicle = otherAttacher.gameObject.GetComponentInParent<RCC_CarControllerV4>();

        //  If no vehicle found, return.
        if (!otherVehicle)
            return;

        //  Attach the trailer.
        ConfigurableJoint parentJoint = GetComponentInParent<ConfigurableJoint>();

        if (!parentJoint)
            return;

        parentJoint.transform.SendMessage("AttachTrailer", otherVehicle, SendMessageOptions.DontRequireReceiver);

    }

    private void Reset() {

        if (!TryGetComponent<BoxCollider>(out _))
            gameObject.AddComponent<BoxCollider>().isTrigger = true;

    }

}
