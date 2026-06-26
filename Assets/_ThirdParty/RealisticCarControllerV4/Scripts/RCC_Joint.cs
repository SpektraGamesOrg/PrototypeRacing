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
/// Stores, applies, and locks properties of a ConfigurableJoint. Used by the damage and detachable parts systems
/// to save and restore joint configurations during runtime.
/// </summary>
public class RCC_Joint {

    /// <summary>
    /// Connected body of the configurable joint.
    /// </summary>
    [Tooltip("Connected body of the configurable joint.")]
    public Rigidbody connectedBody;

    /// <summary>
    /// Anchor position of the configurable joint in local space.
    /// </summary>
    [Tooltip("Anchor position of the configurable joint in local space.")]
    public Vector3 anchor;

    /// <summary>
    /// Primary axis of the configurable joint.
    /// </summary>
    [Tooltip("Primary axis of the configurable joint.")]
    public Vector3 axis;

    /// <summary>
    /// Angular X motion constraint of the joint.
    /// </summary>
    [Tooltip("Angular X motion constraint of the joint.")]
    public ConfigurableJointMotion jointMotionAngularX;

    /// <summary>
    /// Angular Y motion constraint of the joint.
    /// </summary>
    [Tooltip("Angular Y motion constraint of the joint.")]
    public ConfigurableJointMotion jointMotionAngularY;

    /// <summary>
    /// Angular Z motion constraint of the joint.
    /// </summary>
    [Tooltip("Angular Z motion constraint of the joint.")]
    public ConfigurableJointMotion jointMotionAngularZ;

    /// <summary>
    /// Linear X motion constraint of the joint.
    /// </summary>
    [Tooltip("Linear X motion constraint of the joint.")]
    public ConfigurableJointMotion jointMotionX;

    /// <summary>
    /// Linear Y motion constraint of the joint.
    /// </summary>
    [Tooltip("Linear Y motion constraint of the joint.")]
    public ConfigurableJointMotion jointMotionY;

    /// <summary>
    /// Linear Z motion constraint of the joint.
    /// </summary>
    [Tooltip("Linear Z motion constraint of the joint.")]
    public ConfigurableJointMotion jointMotionZ;

    /// <summary>
    /// Linear movement limit of the joint.
    /// </summary>
    [Tooltip("Linear movement limit of the joint.")]
    public SoftJointLimit linearLimit;

    /// <summary>
    /// Low angular X rotation limit.
    /// </summary>
    [Tooltip("Low angular X rotation limit.")]
    public SoftJointLimit lowAngularXLimit;

    /// <summary>
    /// High angular X rotation limit.
    /// </summary>
    [Tooltip("High angular X rotation limit.")]
    public SoftJointLimit highAngularXLimit;

    /// <summary>
    /// Angular Y rotation limit.
    /// </summary>
    [Tooltip("Angular Y rotation limit.")]
    public SoftJointLimit angularYLimit;

    /// <summary>
    /// Angular Z rotation limit.
    /// </summary>
    [Tooltip("Angular Z rotation limit.")]
    public SoftJointLimit angularZLimit;

    /// <summary>
    /// Original local position of the joint transform before any damage or detachment.
    /// </summary>
    [Tooltip("Original local position of the joint transform before any damage or detachment.")]
    public Vector3 orgLocalPosition;

    /// <summary>
    /// Original local rotation of the joint transform before any damage or detachment.
    /// </summary>
    [Tooltip("Original local rotation of the joint transform before any damage or detachment.")]
    public Quaternion orgLocalRotation;

    /// <summary>
    /// Original parent transform of the joint.
    /// </summary>
    [Tooltip("Original parent transform of the joint.")]
    public Transform orgParent;

    /// <summary>
    /// Applies the stored joint properties to the specified ConfigurableJoint, restoring its original configuration.
    /// </summary>
    /// <param name="targetJoint">The ConfigurableJoint to apply stored properties to.</param>
    public void SetProperties(ConfigurableJoint targetJoint) {

        if (!targetJoint)
            return;

        targetJoint.transform.SetParent(orgParent);
        targetJoint.transform.localPosition = orgLocalPosition;
        targetJoint.transform.localRotation = orgLocalRotation;

        targetJoint.connectedBody = connectedBody;
        targetJoint.anchor = anchor;
        targetJoint.axis = axis;

        targetJoint.angularXMotion = jointMotionAngularX;
        targetJoint.angularYMotion = jointMotionAngularY;
        targetJoint.angularZMotion = jointMotionAngularZ;

        targetJoint.xMotion = jointMotionX;
        targetJoint.yMotion = jointMotionY;
        targetJoint.zMotion = jointMotionZ;

        targetJoint.linearLimit = linearLimit;
        targetJoint.lowAngularXLimit = lowAngularXLimit;
        targetJoint.highAngularXLimit = highAngularXLimit;
        targetJoint.angularYLimit = angularYLimit;
        targetJoint.angularZLimit = angularZLimit;

    }

    /// <summary>
    /// Reads and stores the current properties from the specified ConfigurableJoint for later restoration.
    /// </summary>
    /// <param name="joint">The ConfigurableJoint to read properties from.</param>
    public void GetProperties(ConfigurableJoint joint) {

        if (!joint)
            return;

        connectedBody = joint.connectedBody;
        anchor = joint.anchor;
        axis = joint.axis;

        jointMotionAngularX = joint.angularXMotion;
        jointMotionAngularY = joint.angularYMotion;
        jointMotionAngularZ = joint.angularZMotion;

        jointMotionX = joint.xMotion;
        jointMotionY = joint.yMotion;
        jointMotionZ = joint.zMotion;

        linearLimit = joint.linearLimit;
        lowAngularXLimit = joint.lowAngularXLimit;
        highAngularXLimit = joint.highAngularXLimit;
        angularYLimit = joint.angularYLimit;
        angularZLimit = joint.angularZLimit;

        orgLocalPosition = joint.transform.localPosition;
        orgLocalRotation = joint.transform.localRotation;
        orgParent = joint.transform.parent;

    }

    /// <summary>
    /// Locks all motion axes of the ConfigurableJoint, preventing any movement or rotation.
    /// </summary>
    /// <param name="joint">The ConfigurableJoint to lock.</param>
    public static void LockPart(ConfigurableJoint joint) {

        if (!joint)
            return;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;

    }

}
