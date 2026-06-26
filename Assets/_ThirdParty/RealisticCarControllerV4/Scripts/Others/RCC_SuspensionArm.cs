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
/// Visually animates a suspension arm (e.g., wishbone, trailing arm) by translating or rotating it
/// based on the linked WheelCollider's current suspension compression distance.
/// </summary>
public class RCC_SuspensionArm : RCC_Core {

    /// <summary>
    /// The RCC_WheelCollider whose suspension travel drives this arm's movement.
    /// </summary>
    [Tooltip("The RCC_WheelCollider whose suspension travel drives this arm's movement.")]
    public RCC_WheelCollider wheelcollider;

    /// <summary>
    /// Whether the arm reacts to suspension travel by changing its position or rotation.
    /// </summary>
    [Tooltip("Whether the arm reacts to suspension travel by changing its position or rotation.")]
    public SuspensionType suspensionType;

    /// <summary>
    /// Determines how the suspension arm responds to wheel travel.
    /// </summary>
    public enum SuspensionType {

        /// <summary>
        /// Arm translates along the chosen axis based on suspension compression.
        /// </summary>
        Position,

        /// <summary>
        /// Arm rotates around the chosen axis based on suspension compression.
        /// </summary>
        Rotation

    }

    /// <summary>
    /// The local axis along which the arm moves or rotates.
    /// </summary>
    [Tooltip("The local axis along which the arm moves or rotates.")]
    public Axis axis;

    /// <summary>
    /// Defines which local axis the suspension arm operates on.
    /// </summary>
    public enum Axis {

        /// <summary>
        /// Local X axis (right).
        /// </summary>
        X,

        /// <summary>
        /// Local Y axis (up).
        /// </summary>
        Y,

        /// <summary>
        /// Local Z axis (forward).
        /// </summary>
        Z

    }

    /// <summary>
    /// Original local position of the arm, captured on Start for resetting each frame.
    /// </summary>
    private Vector3 orgPos;

    /// <summary>
    /// Original local euler angles of the arm, captured on Start for resetting each frame.
    /// </summary>
    private Vector3 orgRot;

    /// <summary>
    /// Baseline suspension distance measured on Start. Used to calculate relative compression.
    /// </summary>
    private float totalSuspensionDistance = 0;

    /// <summary>
    /// Rotation offset in degrees applied before the suspension-driven rotation (used in Rotation mode only).
    /// </summary>
    [Tooltip("Rotation offset in degrees applied before the suspension-driven rotation (used in Rotation mode only).")]
    public float offsetAngle = 30;

    /// <summary>
    /// Multiplier that converts suspension travel distance into rotation degrees (used in Rotation mode only).
    /// </summary>
    [Tooltip("Multiplier that converts suspension travel distance into rotation degrees (used in Rotation mode only).")]
    public float angleFactor = 150;

    private void Start() {

        //  Getting default position and rotation of the arm.
        orgPos = transform.localPosition;
        orgRot = transform.localEulerAngles;

        //  Getting suspension distance.
        totalSuspensionDistance = GetSuspensionDistance();

    }

    private void Update() {

        //  If no wheelcollider found, return.
        if (!wheelcollider)
            return;

        //  If wheelcollider is not active, return.
        if (!wheelcollider.gameObject.activeSelf)
            return;

        float suspensionCourse = GetSuspensionDistance() - totalSuspensionDistance;

        //  Setting position and rotation of the arm to original.
        transform.localPosition = orgPos;
        transform.localEulerAngles = orgRot;

        //  And then change position or rotation of the arm related to calculated suspension distance.
        switch (suspensionType) {

            case SuspensionType.Position:

                switch (axis) {

                    case Axis.X:
                        transform.position += transform.right * suspensionCourse;
                        break;
                    case Axis.Y:
                        transform.position += transform.up * suspensionCourse;
                        break;
                    case Axis.Z:
                        transform.position += transform.forward * suspensionCourse;
                        break;

                }

                break;

            case SuspensionType.Rotation:

                switch (axis) {

                    case Axis.X:
                        transform.Rotate(Vector3.right, suspensionCourse * angleFactor - offsetAngle, Space.Self);
                        break;
                    case Axis.Y:
                        transform.Rotate(Vector3.up, suspensionCourse * angleFactor - offsetAngle, Space.Self);
                        break;
                    case Axis.Z:
                        transform.Rotate(Vector3.forward, suspensionCourse * angleFactor - offsetAngle, Space.Self);
                        break;

                }

                break;

        }

    }

    /// <summary>
    /// Calculates the current vertical suspension distance by converting the wheel's world pose to local space.
    /// </summary>
    /// <returns>The local Y position of the wheel contact point, representing the current suspension compression.</returns>
    private float GetSuspensionDistance() {

        if (!wheelcollider || !wheelcollider.WheelCollider)
            return 0f;

        wheelcollider.WheelCollider.GetWorldPose(out Vector3 position, out Quaternion quat);
        Vector3 local = wheelcollider.transform.InverseTransformPoint(position);
        return local.y;

    }

}
