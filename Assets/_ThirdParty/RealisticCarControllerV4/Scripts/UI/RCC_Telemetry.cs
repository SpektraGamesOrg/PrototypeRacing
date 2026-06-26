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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays real-time vehicle telemetry data on a UI panel for debugging and diagnostics.
/// Shows per-wheel data (RPM, torque, brake, force, steer angle, slip), driving aids status (ABS, ESP, TCS),
/// ground hit info, and overall vehicle state (speed, engine RPM, gear, drivetrain, inputs).
/// The panel visibility is controlled by <see cref="RCC_Settings.useTelemetry"/>.
/// </summary>
public class RCC_Telemetry : RCC_Core {

    /// <summary>
    /// Reference to the active player vehicle. Automatically assigned each frame from the scene manager.
    /// </summary>
    private RCC_CarControllerV4 targetVehicle;

    /// <summary>
    /// Root panel GameObject for the telemetry UI. Shown or hidden based on the useTelemetry setting.
    /// </summary>
    [Tooltip("Root panel GameObject for the telemetry UI. Shown or hidden based on the useTelemetry setting.")]
    public GameObject mainPanel;

    /// <summary>
    /// Text displaying the RPM of the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the RPM of the front-left wheel.")]
    public TextMeshProUGUI RPM_WheelFL;

    /// <summary>
    /// Text displaying the RPM of the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the RPM of the front-right wheel.")]
    public TextMeshProUGUI RPM_WheelFR;

    /// <summary>
    /// Text displaying the RPM of the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the RPM of the rear-left wheel.")]
    public TextMeshProUGUI RPM_WheelRL;

    /// <summary>
    /// Text displaying the RPM of the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the RPM of the rear-right wheel.")]
    public TextMeshProUGUI RPM_WheelRR;

    /// <summary>
    /// Text displaying the motor torque applied to the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the motor torque applied to the front-left wheel.")]
    public TextMeshProUGUI Torque_WheelFL;

    /// <summary>
    /// Text displaying the motor torque applied to the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the motor torque applied to the front-right wheel.")]
    public TextMeshProUGUI Torque_WheelFR;

    /// <summary>
    /// Text displaying the motor torque applied to the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the motor torque applied to the rear-left wheel.")]
    public TextMeshProUGUI Torque_WheelRL;

    /// <summary>
    /// Text displaying the motor torque applied to the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the motor torque applied to the rear-right wheel.")]
    public TextMeshProUGUI Torque_WheelRR;

    /// <summary>
    /// Text displaying the brake torque applied to the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the brake torque applied to the front-left wheel.")]
    public TextMeshProUGUI Brake_WheelFL;

    /// <summary>
    /// Text displaying the brake torque applied to the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the brake torque applied to the front-right wheel.")]
    public TextMeshProUGUI Brake_WheelFR;

    /// <summary>
    /// Text displaying the brake torque applied to the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the brake torque applied to the rear-left wheel.")]
    public TextMeshProUGUI Brake_WheelRL;

    /// <summary>
    /// Text displaying the brake torque applied to the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the brake torque applied to the rear-right wheel.")]
    public TextMeshProUGUI Brake_WheelRR;

    /// <summary>
    /// Text displaying the bump force of the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the bump force of the front-left wheel.")]
    public TextMeshProUGUI Force_WheelFL;

    /// <summary>
    /// Text displaying the bump force of the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the bump force of the front-right wheel.")]
    public TextMeshProUGUI Force_WheelFR;

    /// <summary>
    /// Text displaying the bump force of the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the bump force of the rear-left wheel.")]
    public TextMeshProUGUI Force_WheelRL;

    /// <summary>
    /// Text displaying the bump force of the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the bump force of the rear-right wheel.")]
    public TextMeshProUGUI Force_WheelRR;

    /// <summary>
    /// Text displaying the steer angle of the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the steer angle of the front-left wheel.")]
    public TextMeshProUGUI Angle_WheelFL;

    /// <summary>
    /// Text displaying the steer angle of the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the steer angle of the front-right wheel.")]
    public TextMeshProUGUI Angle_WheelFR;

    /// <summary>
    /// Text displaying the steer angle of the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the steer angle of the rear-left wheel.")]
    public TextMeshProUGUI Angle_WheelRL;

    /// <summary>
    /// Text displaying the steer angle of the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the steer angle of the rear-right wheel.")]
    public TextMeshProUGUI Angle_WheelRR;

    /// <summary>
    /// Text displaying the sideways slip amount of the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the sideways slip amount of the front-left wheel.")]
    public TextMeshProUGUI Sideways_WheelFL;

    /// <summary>
    /// Text displaying the sideways slip amount of the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the sideways slip amount of the front-right wheel.")]
    public TextMeshProUGUI Sideways_WheelFR;

    /// <summary>
    /// Text displaying the sideways slip amount of the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the sideways slip amount of the rear-left wheel.")]
    public TextMeshProUGUI Sideways_WheelRL;

    /// <summary>
    /// Text displaying the sideways slip amount of the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the sideways slip amount of the rear-right wheel.")]
    public TextMeshProUGUI Sideways_WheelRR;

    /// <summary>
    /// Text displaying the forward slip amount of the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the forward slip amount of the front-left wheel.")]
    public TextMeshProUGUI Forward_WheelFL;

    /// <summary>
    /// Text displaying the forward slip amount of the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the forward slip amount of the front-right wheel.")]
    public TextMeshProUGUI Forward_WheelFR;

    /// <summary>
    /// Text displaying the forward slip amount of the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the forward slip amount of the rear-left wheel.")]
    public TextMeshProUGUI Forward_WheelRL;

    /// <summary>
    /// Text displaying the forward slip amount of the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the forward slip amount of the rear-right wheel.")]
    public TextMeshProUGUI Forward_WheelRR;

    /// <summary>
    /// Text displaying whether ABS (Anti-lock Braking System) is currently engaged.
    /// </summary>
    [Tooltip("Text displaying whether ABS (Anti-lock Braking System) is currently engaged.")]
    public TextMeshProUGUI ABS;

    /// <summary>
    /// Text displaying whether ESP (Electronic Stability Program) is currently engaged.
    /// </summary>
    [Tooltip("Text displaying whether ESP (Electronic Stability Program) is currently engaged.")]
    public TextMeshProUGUI ESP;

    /// <summary>
    /// Text displaying whether TCS (Traction Control System) is currently engaged.
    /// </summary>
    [Tooltip("Text displaying whether TCS (Traction Control System) is currently engaged.")]
    public TextMeshProUGUI TCS;

    /// <summary>
    /// Text displaying the ground collider name hit by the front-left wheel.
    /// </summary>
    [Tooltip("Text displaying the ground collider name hit by the front-left wheel.")]
    public TextMeshProUGUI GroundHit_WheelFL;

    /// <summary>
    /// Text displaying the ground collider name hit by the front-right wheel.
    /// </summary>
    [Tooltip("Text displaying the ground collider name hit by the front-right wheel.")]
    public TextMeshProUGUI GroundHit_WheelFR;

    /// <summary>
    /// Text displaying the ground collider name hit by the rear-left wheel.
    /// </summary>
    [Tooltip("Text displaying the ground collider name hit by the rear-left wheel.")]
    public TextMeshProUGUI GroundHit_WheelRL;

    /// <summary>
    /// Text displaying the ground collider name hit by the rear-right wheel.
    /// </summary>
    [Tooltip("Text displaying the ground collider name hit by the rear-right wheel.")]
    public TextMeshProUGUI GroundHit_WheelRR;

    /// <summary>
    /// Text displaying the current vehicle speed.
    /// </summary>
    [Tooltip("Text displaying the current vehicle speed.")]
    public TextMeshProUGUI speed;

    /// <summary>
    /// Text displaying the current engine RPM.
    /// </summary>
    [Tooltip("Text displaying the current engine RPM.")]
    public TextMeshProUGUI engineRPM;

    /// <summary>
    /// Text displaying the current gear number.
    /// </summary>
    [Tooltip("Text displaying the current gear number.")]
    public TextMeshProUGUI gear;

    /// <summary>
    /// Text displaying the final drive torque output.
    /// </summary>
    [Tooltip("Text displaying the final drive torque output.")]
    public TextMeshProUGUI finalTorque;

    /// <summary>
    /// Text displaying the current drivetrain type (FWD, RWD, or AWD).
    /// </summary>
    [Tooltip("Text displaying the current drivetrain type (FWD, RWD, or AWD).")]
    public TextMeshProUGUI drivetrain;

    /// <summary>
    /// Text displaying the rigidbody's angular velocity vector.
    /// </summary>
    [Tooltip("Text displaying the rigidbody's angular velocity vector.")]
    public TextMeshProUGUI angularVelocity;

    /// <summary>
    /// Text displaying whether the vehicle is currently controllable by the player.
    /// </summary>
    [Tooltip("Text displaying whether the vehicle is currently controllable by the player.")]
    public TextMeshProUGUI controllable;

    /// <summary>
    /// Text displaying the current throttle input value.
    /// </summary>
    [Tooltip("Text displaying the current throttle input value.")]
    public TextMeshProUGUI throttle;

    /// <summary>
    /// Text displaying the current steering input value.
    /// </summary>
    [Tooltip("Text displaying the current steering input value.")]
    public TextMeshProUGUI steer;

    /// <summary>
    /// Text displaying the current brake input value.
    /// </summary>
    [Tooltip("Text displaying the current brake input value.")]
    public TextMeshProUGUI brake;

    /// <summary>
    /// Text displaying the current handbrake input value.
    /// </summary>
    [Tooltip("Text displaying the current handbrake input value.")]
    public TextMeshProUGUI handbrake;

    /// <summary>
    /// Text displaying the current clutch input value.
    /// </summary>
    [Tooltip("Text displaying the current clutch input value.")]
    public TextMeshProUGUI clutch;

    private void Update() {

        if (!mainPanel)
            return;

        if (mainPanel.activeSelf != RCC_Settings.Instance.useTelemetry)
            mainPanel.SetActive(RCC_Settings.Instance.useTelemetry);

        targetVehicle = RCC_SceneManager.Instance.activePlayerVehicle;

        if (!targetVehicle)
            return;

        RCC_WheelCollider wFL = targetVehicle.FrontLeftWheelCollider;
        RCC_WheelCollider wFR = targetVehicle.FrontRightWheelCollider;
        RCC_WheelCollider wRL = targetVehicle.RearLeftWheelCollider;
        RCC_WheelCollider wRR = targetVehicle.RearRightWheelCollider;

        if (!wFL || !wFR || !wRL || !wRR)
            return;

        if (RPM_WheelFL) RPM_WheelFL.text = wFL.WheelCollider.rpm.ToString("F0");
        if (RPM_WheelFR) RPM_WheelFR.text = wFR.WheelCollider.rpm.ToString("F0");
        if (RPM_WheelRL) RPM_WheelRL.text = wRL.WheelCollider.rpm.ToString("F0");
        if (RPM_WheelRR) RPM_WheelRR.text = wRR.WheelCollider.rpm.ToString("F0");

        if (Torque_WheelFL) Torque_WheelFL.text = wFL.WheelCollider.motorTorque.ToString("F0");
        if (Torque_WheelFR) Torque_WheelFR.text = wFR.WheelCollider.motorTorque.ToString("F0");
        if (Torque_WheelRL) Torque_WheelRL.text = wRL.WheelCollider.motorTorque.ToString("F0");
        if (Torque_WheelRR) Torque_WheelRR.text = wRR.WheelCollider.motorTorque.ToString("F0");

        if (Brake_WheelFL) Brake_WheelFL.text = wFL.WheelCollider.brakeTorque.ToString("F0");
        if (Brake_WheelFR) Brake_WheelFR.text = wFR.WheelCollider.brakeTorque.ToString("F0");
        if (Brake_WheelRL) Brake_WheelRL.text = wRL.WheelCollider.brakeTorque.ToString("F0");
        if (Brake_WheelRR) Brake_WheelRR.text = wRR.WheelCollider.brakeTorque.ToString("F0");

        if (Force_WheelFL) Force_WheelFL.text = wFL.bumpForce.ToString("F0");
        if (Force_WheelFR) Force_WheelFR.text = wFR.bumpForce.ToString("F0");
        if (Force_WheelRL) Force_WheelRL.text = wRL.bumpForce.ToString("F0");
        if (Force_WheelRR) Force_WheelRR.text = wRR.bumpForce.ToString("F0");

        if (Angle_WheelFL) Angle_WheelFL.text = wFL.WheelCollider.steerAngle.ToString("F0");
        if (Angle_WheelFR) Angle_WheelFR.text = wFR.WheelCollider.steerAngle.ToString("F0");
        if (Angle_WheelRL) Angle_WheelRL.text = wRL.WheelCollider.steerAngle.ToString("F0");
        if (Angle_WheelRR) Angle_WheelRR.text = wRR.WheelCollider.steerAngle.ToString("F0");

        if (Sideways_WheelFL) Sideways_WheelFL.text = wFL.wheelSlipAmountSideways.ToString("F");
        if (Sideways_WheelFR) Sideways_WheelFR.text = wFR.wheelSlipAmountSideways.ToString("F");
        if (Sideways_WheelRL) Sideways_WheelRL.text = wRL.wheelSlipAmountSideways.ToString("F");
        if (Sideways_WheelRR) Sideways_WheelRR.text = wRR.wheelSlipAmountSideways.ToString("F");

        if (Forward_WheelFL) Forward_WheelFL.text = wFL.wheelSlipAmountForward.ToString("F");
        if (Forward_WheelFR) Forward_WheelFR.text = wFR.wheelSlipAmountForward.ToString("F");
        if (Forward_WheelRL) Forward_WheelRL.text = wRL.wheelSlipAmountForward.ToString("F");
        if (Forward_WheelRR) Forward_WheelRR.text = wRR.wheelSlipAmountForward.ToString("F");

        if (ABS) ABS.text = targetVehicle.ABSAct ? "Engaged" : "Not Engaged";
        if (ESP) ESP.text = targetVehicle.ESPAct ? "Engaged" : "Not Engaged";
        if (TCS) TCS.text = targetVehicle.TCSAct ? "Engaged" : "Not Engaged";

        if (GroundHit_WheelFL) GroundHit_WheelFL.text = wFL.isGrounded && wFL.wheelHit.collider ? wFL.wheelHit.collider.name : "";
        if (GroundHit_WheelFR) GroundHit_WheelFR.text = wFR.isGrounded && wFR.wheelHit.collider ? wFR.wheelHit.collider.name : "";
        if (GroundHit_WheelRL) GroundHit_WheelRL.text = wRL.isGrounded && wRL.wheelHit.collider ? wRL.wheelHit.collider.name : "";
        if (GroundHit_WheelRR) GroundHit_WheelRR.text = wRR.isGrounded && wRR.wheelHit.collider ? wRR.wheelHit.collider.name : "";

        if (speed) speed.text = targetVehicle.speed.ToString("F0");
        if (engineRPM) engineRPM.text = targetVehicle.engineRPM.ToString("F0");
        if (gear) gear.text = targetVehicle.currentGear.ToString("F0");
        if (finalTorque) finalTorque.text = targetVehicle.engineTorqueCurve.Evaluate(targetVehicle.engineRPM).ToString("F0");

        if (drivetrain) {

            switch (targetVehicle.wheelTypeChoise) {

                case RCC_CarControllerV4.WheelType.FWD:

                    drivetrain.text = "FWD";
                    break;

                case RCC_CarControllerV4.WheelType.RWD:

                    drivetrain.text = "RWD";
                    break;

                case RCC_CarControllerV4.WheelType.AWD:

                    drivetrain.text = "AWD";
                    break;

            }

        }

        if (angularVelocity) angularVelocity.text = targetVehicle.Rigid.angularVelocity.ToString();
        if (controllable) controllable.text = targetVehicle.canControl ? "True" : "False";

        if (throttle) throttle.text = targetVehicle.throttleInput.ToString("F");
        if (steer) steer.text = targetVehicle.steerInput.ToString("F");
        if (brake) brake.text = targetVehicle.brakeInput.ToString("F");
        if (handbrake) handbrake.text = targetVehicle.handbrakeInput.ToString("F");
        if (clutch) clutch.text = targetVehicle.clutchInput.ToString("F");

    }

}
