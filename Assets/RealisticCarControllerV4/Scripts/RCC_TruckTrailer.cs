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
/// Manages a towable trailer with its own wheel colliders, lights, center of mass, and attach/detach logic.
/// Connected to a towing vehicle via a ConfigurableJoint. Does not inherit RCC_Core as it is not a vehicle component.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class RCC_TruckTrailer : MonoBehaviour {

    /// <summary>
    /// Reference to the towing vehicle's car controller. Null when detached.
    /// </summary>
    private RCC_CarControllerV4 carController;

    /// <summary>
    /// Rigidbody of this trailer.
    /// </summary>
    private Rigidbody rigid;

    /// <summary>
    /// ConfigurableJoint connecting this trailer to the towing vehicle.
    /// </summary>
    private ConfigurableJoint joint;

    /// <summary>
    /// Wheel colliders and models.
    /// </summary>
    /// <summary>
    /// Represents a single trailer wheel with its collider and visual model.
    /// </summary>
    [System.Serializable]
    public class TrailerWheel {

        /// <summary>
        /// The WheelCollider for this trailer wheel.
        /// </summary>
        [Tooltip("The WheelCollider for this trailer wheel.")]
        public WheelCollider wheelCollider;

        /// <summary>
        /// The visual wheel model transform that gets aligned to the WheelCollider pose.
        /// </summary>
        [Tooltip("The visual wheel model transform that gets aligned to the WheelCollider pose.")]
        public Transform wheelModel;

        /// <summary>
        /// Applies motor torque to this wheel.
        /// </summary>
        /// <param name="torque">Motor torque value.</param>
        public void Torque(float torque) {

            wheelCollider.motorTorque = torque;

        }

        /// <summary>
        /// Applies brake torque to this wheel.
        /// </summary>
        /// <param name="torque">Brake torque value.</param>
        public void Brake(float torque) {

            wheelCollider.brakeTorque = torque;

        }

    }

    /// <summary>
    /// All wheel collider/model pairs on this trailer.
    /// </summary>
    [Header("Wheels")]
    [Tooltip("All wheel collider/model pairs on this trailer.")]
    public TrailerWheel[] trailerWheels;

    /// <summary>
    /// Center of mass transform for the trailer's Rigidbody.
    /// </summary>
    [Header("Rigidbody")]
    [Tooltip("Center of mass transform for the trailer's Rigidbody.")]
    public Transform COM;

    /// <summary>
    /// Optional legs GameObject that becomes visible when the trailer is detached.
    /// </summary>
    [Tooltip("Optional legs GameObject that becomes visible when the trailer is detached.")]
    public GameObject legs;

    /// <summary>
    /// True when the trailer's Rigidbody velocity is near zero (sleeping optimization).
    /// </summary>
    private bool isSleeping = false;

    /// <summary>
    /// Cooldown timer preventing rapid attach/detach cycling.
    /// </summary>
    private float timer = 0f;

    /// <summary>
    /// True when the trailer is currently connected to a towing vehicle.
    /// </summary>
    [Header("Attachment")]
    [Tooltip("True when the trailer is currently connected to a towing vehicle.")]
    public bool attached = false;

    /// <summary>
    /// If true, applies brake force to the trailer wheels when detached.
    /// </summary>
    [Tooltip("If true, applies brake force to the trailer wheels when detached.")]
    public bool brakeWhenDetached = false;

    /// <summary>
    /// Brake torque applied to trailer wheels when detached and brakeWhenDetached is enabled.
    /// </summary>
    [Tooltip("Brake torque applied to trailer wheels when detached and brakeWhenDetached is enabled.")]
    [Min(0f)]
    public float brakeForce = 5000f;

    /// <summary>
    /// Joint restrictions of the trailer.
    /// </summary>
    private class JointRestrictions {

        public ConfigurableJointMotion motionX;
        public ConfigurableJointMotion motionY;
        public ConfigurableJointMotion motionZ;

        public ConfigurableJointMotion angularMotionX;
        public ConfigurableJointMotion angularMotionY;
        public ConfigurableJointMotion angularMotionZ;

        /// <summary>
        /// Captures the current linear and angular motion settings of the supplied joint into this restrictions snapshot, so they can be reapplied later via Set.
        /// </summary>
        /// <param name="configurableJoint">Joint whose linear and angular motion settings are copied into this snapshot.</param>
        public void Get(ConfigurableJoint configurableJoint) {

            motionX = configurableJoint.xMotion;
            motionY = configurableJoint.yMotion;
            motionZ = configurableJoint.zMotion;

            angularMotionX = configurableJoint.angularXMotion;
            angularMotionY = configurableJoint.angularYMotion;
            angularMotionZ = configurableJoint.angularZMotion;

        }

        /// <summary>
        /// Applies this snapshot's stored linear and angular motion settings back to the supplied joint, restoring the previously captured restrictions.
        /// </summary>
        /// <param name="configurableJoint">Joint whose linear and angular motion settings are overwritten with this snapshot's values.</param>
        public void Set(ConfigurableJoint configurableJoint) {

            configurableJoint.xMotion = motionX;
            configurableJoint.yMotion = motionY;
            configurableJoint.zMotion = motionZ;

            configurableJoint.angularXMotion = angularMotionX;
            configurableJoint.angularYMotion = angularMotionY;
            configurableJoint.angularZMotion = angularMotionZ;

        }

        public void Reset(ConfigurableJoint configurableJoint) {

            configurableJoint.xMotion = ConfigurableJointMotion.Free;
            configurableJoint.yMotion = ConfigurableJointMotion.Free;
            configurableJoint.zMotion = ConfigurableJointMotion.Free;

            configurableJoint.angularXMotion = ConfigurableJointMotion.Free;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Free;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Free;

        }

    }
    private JointRestrictions jointRestrictions = new JointRestrictions();
    private RCC_Light[] lights;

    private void Start() {

        rigid = GetComponent<Rigidbody>();      //	Getting rigidbody.
        joint = GetComponentInParent<ConfigurableJoint>(true);      //	Getting configurable joint.
        jointRestrictions.Get(joint);       //	Getting current limitations of the joint.

        // Fixing stutering bug of the rigid.
        rigid.interpolation = RigidbodyInterpolation.None;
        rigid.interpolation = RigidbodyInterpolation.Interpolate;
        joint.configuredInWorldSpace = true;

        //	If joint is connected as default, attach the trailer. Otherwise detach.
        if (joint.connectedBody) {

            if (joint.connectedBody.gameObject.TryGetComponent(out RCC_CarControllerV4 connectedVehicle))
                AttachTrailer(connectedVehicle);

        } else {

            carController = null;
            joint.connectedBody = null;
            jointRestrictions.Reset(joint);

        }

    }

    private void FixedUpdate() {

        if (!COM)
            return;

        attached = joint.connectedBody;     //	Is trailer attached now?
        rigid.centerOfMass = transform.InverseTransformPoint(COM.transform.position);       //	Setting center of mass.

        //	Applying torque to the wheels.
        if (trailerWheels == null || trailerWheels.Length == 0)
            return;

        for (int i = 0; i < trailerWheels.Length; i++) {

            if (trailerWheels[i] == null || trailerWheels[i].wheelCollider == null)
                continue;

            if (carController) {

                trailerWheels[i].Torque(carController.throttleInput * (attached ? 1f : 0f));
                trailerWheels[i].Brake((attached ? 0f : 5000f));

            } else {

                trailerWheels[i].Torque(0f);
                trailerWheels[i].Brake((brakeWhenDetached ? brakeForce : 0f));

            }

        }

    }

    private void Update() {

        //	If trailer is not moving, enable sleeping mode.
        if (rigid.linearVelocity.magnitude < .01f && Mathf.Abs(rigid.angularVelocity.magnitude) < .01f)
            isSleeping = true;
        else
            isSleeping = false;

        // Timer was used for attach/detach delay.
        if (timer > 0f)
            timer -= Time.deltaTime;

        timer = Mathf.Clamp01(timer);       //	Clamping timer between 0f - 1f.

        WheelAlign();  // Aligning wheel model position and rotation.

    }

    /// <summary>
    /// Aligning wheel model position and rotation.
    /// </summary>
    private void WheelAlign() {

        //	If trailer is sleeping, return.
        if (isSleeping)
            return;

        if (trailerWheels == null || trailerWheels.Length == 0)
            return;

        for (int i = 0; i < trailerWheels.Length; i++) {

            // Return if no wheel model selected.
            if (!trailerWheels[i].wheelModel) {

#if UNITY_EDITOR
                Debug.LogError(transform.name + " wheel of the " + transform.name + " is missing wheel model. This wheel is disabled");
#endif
                enabled = false;
                return;

            }

            // Locating correct position and rotation for the wheel.
            Vector3 wheelPosition;
            Quaternion wheelRotation;

            trailerWheels[i].wheelCollider.GetWorldPose(out wheelPosition, out wheelRotation);

            //	Assigning position and rotation to the wheel model.
            trailerWheels[i].wheelModel.transform.SetPositionAndRotation(wheelPosition, wheelRotation);

        }

    }

    /// <summary>
    /// Detaches the trailer from its towing vehicle, frees joint constraints, shows legs, and triggers camera auto-focus.
    /// </summary>
    public void DetachTrailer() {

        if (lights != null) {

            foreach (RCC_Light item in lights) {

                item.CarController = null;

            }

        }

        // Resetting attachedTrailer of car controller.
        if (carController)
            carController.attachedTrailer = null;
        carController = null;
        lights = null;
        timer = 1f;
        joint.connectedBody = null;
        jointRestrictions.Reset(joint);

        if (legs)
            legs.SetActive(true);

        if (RCC_SceneManager.Instance && RCC_SceneManager.Instance.activePlayerCamera && RCC_SceneManager.Instance.activePlayerCamera.TPSAutoFocus)
            StartCoroutine(RCC_SceneManager.Instance.activePlayerCamera.AutoFocus());

    }

    /// <summary>
    /// Attaches the trailer to the specified vehicle, connects the joint, hides legs, and initializes trailer lights.
    /// </summary>
    /// <param name="vehicle">The towing vehicle to connect to.</param>
    public void AttachTrailer(RCC_CarControllerV4 vehicle) {

        // If delay is short, return.
        if (timer > 0)
            return;

        carController = vehicle;        //	Assigning car controller.
        lights = GetComponentsInChildren<RCC_Light>();       //	Getting car controller lights.
        timer = 1f;     //	Setting timer.

        joint.connectedBody = vehicle.Rigid;        //	Connecting joint.
                                                    //joint.autoConfigureConnectedAnchor = false;		//	Setting auto configuration off of the joint.
                                                    //Vector3 jointVector = joint.connectedAnchor;		//	Resetting X axis of the connected anchor on attachment.
                                                    //jointVector.x = 0f;
                                                    //joint.connectedAnchor = jointVector;
        jointRestrictions.Set(joint);       //	Enabling limitations of the joint.

        // If trailer has legs, disable on attach.
        if (legs)
            legs.SetActive(false);

        //	Initializing lights of the trailer. Parent car controller will take control of them.
        foreach (RCC_Light item in lights) {

            item.CarController = carController;

        }

        // Assigning attachedTrailer of car controller.
        vehicle.attachedTrailer = this;
        rigid.isKinematic = false;

        // If autofocus is enabled on RCC Camera, run it.
        if (RCC_SceneManager.Instance && RCC_SceneManager.Instance.activePlayerCamera && RCC_SceneManager.Instance.activePlayerCamera.TPSAutoFocus)
            StartCoroutine(RCC_SceneManager.Instance.activePlayerCamera.AutoFocus(transform, carController.transform));

    }

    private void Reset() {

        if (COM == null) {

            GameObject com = new GameObject("COM");
            com.transform.SetParent(transform, false);
            com.transform.localPosition = Vector3.zero;
            com.transform.localRotation = Quaternion.identity;
            com.transform.localScale = Vector3.one;
            COM = com.transform;

        }

        if (transform.Find("Wheel Models") == null) {

            GameObject com = new GameObject("Wheel Models");
            com.transform.SetParent(transform, false);
            com.transform.localPosition = Vector3.zero;
            com.transform.localRotation = Quaternion.identity;
            com.transform.localScale = Vector3.one;

        }

        if (transform.Find("Wheel Colliders") == null) {

            GameObject com = new GameObject("Wheel Colliders");
            com.transform.SetParent(transform, false);
            com.transform.localPosition = Vector3.zero;
            com.transform.localRotation = Quaternion.identity;
            com.transform.localScale = Vector3.one;

        }

        GetComponent<Rigidbody>().mass = 5000;

    }

}
