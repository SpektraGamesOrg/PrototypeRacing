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
/// Operates a swinging hammer obstacle in the damage demo scene. The hammer is attached via a
/// ConfigurableJoint hinge and driven by a sinusoidal force to create a pendulum-like motion.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RCC_CrashHammer : RCC_Core {

    /// <summary>
    /// Transform representing the hinge pivot point where the ConfigurableJoint is anchored.
    /// </summary>
    [Tooltip("Transform representing the hinge pivot point where the ConfigurableJoint is anchored.")]
    public Transform hingePoint;

    /// <summary>
    /// Rigidbody component attached to this hammer.
    /// </summary>
    private Rigidbody rigid;

    /// <summary>
    /// Direction and magnitude of the torque force applied to swing the hammer.
    /// </summary>
    [Tooltip("Direction and magnitude of the torque force applied to swing the hammer.")]
    public Vector3 torque;

    /// <summary>
    /// Amplitude of the sinusoidal force wave that drives the hammer's swinging motion.
    /// </summary>
    [Tooltip("Amplitude of the sinusoidal force wave that drives the hammer's swinging motion.")]
    [Min(0f)]
    public float length = 1f;

    /// <summary>
    /// Frequency of the sinusoidal force wave. Higher values make the hammer swing faster.
    /// </summary>
    [Tooltip("Frequency of the sinusoidal force wave. Higher values make the hammer swing faster.")]
    [Min(0f)]
    public float speed = 1f;

    private void Start() {

        //  Getting rigidbody.
        rigid = GetComponent<Rigidbody>();

        //  Creating hinge with configurable joint.
        CreateHinge();

    }

    private void FixedUpdate() {

        //  If no rigid, return.
        if (!rigid)
            return;

        //  Apply force.
        rigid.AddRelativeForce(torque * ((float)Mathf.Sin(Time.time * speed) * length), ForceMode.Acceleration);

    }

    /// <summary>
    /// Creates hinge with configurable joint.
    /// </summary>
    private void CreateHinge() {

        if (!hingePoint)
            return;

        GameObject hinge = new GameObject("Hinge_" + transform.name);
        hinge.transform.position = hingePoint.position;
        hinge.transform.rotation = hingePoint.rotation;

        Rigidbody hingeRigid = hinge.AddComponent<Rigidbody>();
        hingeRigid.isKinematic = true;
        hingeRigid.useGravity = false;

        AttachHinge(hingeRigid);

    }

    /// <summary>
    /// Sets the connected body of the ConfigurableJoint to the provided kinematic hinge rigidbody.
    /// </summary>
    /// <param name="hingeRigid">The kinematic rigidbody acting as the fixed anchor point for the joint.</param>
    private void AttachHinge(Rigidbody hingeRigid) {

        if (!TryGetComponent(out ConfigurableJoint joint)) {

#if UNITY_EDITOR
            print("Configurable Joint of the " + transform.name + " not found! Be sure this gameobject has Configurable Joint with right config.");
#endif
            enabled = false;
            return;

        }

        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = hingeRigid;
        joint.connectedAnchor = Vector3.zero;

    }

    private void Reset() {

        if (hingePoint == null) {

            hingePoint = new GameObject("Hinge Point").transform;
            hingePoint.SetParent(transform, false);

        }

    }

}
