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
/// Static API class providing vehicle customization methods for RCC. Includes suspension tuning, camber adjustment, wheel changes,
/// headlight and smoke color changes, driving assists configuration, and save/load via PlayerPrefs.
/// </summary>
public class RCC_Customization_API : RCC_Core {

    /// <summary>
    /// Set Customization Mode. This will enable / disable controlling the vehicle, and enable / disable orbit camera mode.
    /// </summary>
    /// <param name="vehicle">Target vehicle to put into (or take out of) customization mode.</param>
    /// <param name="state">True to enter customization mode (disable driving, switch UI to Customization), false to exit and restore normal driving.</param>
    public static void SetCustomizationMode(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!vehicle) {

#if UNITY_EDITOR
            Debug.LogError("Player vehicle is not selected for customization! Use RCC_Customization.SetCustomizationMode(playerVehicle, true/false); for enabling / disabling customization mode for player vehicle.");
#endif
            return;

        }

        //  Finding camera and dashboard.
        RCC_Camera cam = RCCSceneManager.activePlayerCamera;
        RCC_UI_DashboardDisplay UI = RCCSceneManager.activePlayerCanvas;

        //  If enabled customization mode, set camera mode to TPS and set UI type to Customization. Set controllable state of the vehicle to false, we don't want to control the vehicle while customizing.
        if (state) {

            vehicle.SetCanControl(false);

            if (cam)
                cam.ChangeCamera(RCC_Camera.CameraMode.TPS);

            if (UI)
                UI.SetDisplayType(RCC_UI_DashboardDisplay.DisplayType.Customization);

        } else {

            //  If disabled the customization mode, set camera mode to TPS and set UI type to Full. Set controllable state of the vehicle to true, and make sure previewing flames and exhaust is set to false.
            SetSmokeParticle(vehicle, false);
            SetExhaustFlame(vehicle, false);

            vehicle.SetCanControl(true);

            if (cam)
                cam.ChangeCamera(RCC_Camera.CameraMode.TPS);

            if (UI)
                UI.SetDisplayType(RCC_UI_DashboardDisplay.DisplayType.Full);

        }

    }

    /// <summary>
    ///	 Enable / Disable Smoke Particles. You can use it for previewing current wheel smokes.
    /// </summary>
    /// <param name="vehicle">Target vehicle whose wheel-smoke preview state is toggled.</param>
    /// <param name="state">True to force-show wheel smoke particles for preview, false to stop the preview.</param>
    public static void SetSmokeParticle(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.PreviewSmokeParticle(state);

    }

    /// <summary>
    /// Sets the wheel smoke particle color for all wheels on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="indexOfGroundMaterial">Index of the ground material (reserved for future use).</param>
    /// <param name="color">New smoke particle color.</param>
    public static void SetSmokeColor(RCC_CarControllerV4 vehicle, int indexOfGroundMaterial, Color color) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Getting all wheelcolliders.
        RCC_WheelCollider[] wheels = vehicle.GetComponentsInChildren<RCC_WheelCollider>();

        //  And setting color of the particles.
        foreach (RCC_WheelCollider wheel in wheels) {

            for (int i = 0; i < wheel.allWheelParticles.Count; i++) {

                ParticleSystem ps = wheel.allWheelParticles[i];
                ParticleSystem.MainModule psmain = ps.main;
                color.a = psmain.startColor.color.a;
                psmain.startColor = color;

            }

        }

    }

    /// <summary>
    /// Sets the headlight color for all headlight and high-beam lights on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="color">New headlight color.</param>
    public static void SetHeadlightsColor(RCC_CarControllerV4 vehicle, Color color) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Enabling headlights.
        vehicle.lowBeamHeadLightsOn = true;

        //  Getting all lights.
        RCC_Light[] lights = vehicle.GetComponentsInChildren<RCC_Light>();

        //  If light is headlight, set color.
        foreach (RCC_Light l in lights) {

            if (l.lightType == RCC_Light.LightType.HeadLight || l.lightType == RCC_Light.LightType.HighBeamHeadLight)
                l.GetComponent<Light>().color = color;

        }

    }

    /// <summary>
    /// Enables or disables exhaust flame particle preview on all exhaust components.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable flame preview, false to disable.</param>
    public static void SetExhaustFlame(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Getting all exhausts.
        RCC_Exhaust[] exhausts = vehicle.GetComponentsInChildren<RCC_Exhaust>();

        //  Enabling preview mode for all exhausts.
        foreach (RCC_Exhaust exhaust in exhausts)
            exhaust.previewFlames = state;

    }

    /// <summary>
    /// Sets the camber angle for front wheels.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="camberAngle">Camber angle in degrees.</param>
    public static void SetFrontCambers(RCC_CarControllerV4 vehicle, float camberAngle) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Getting all wheelcolliders.
        RCC_WheelCollider[] wc = vehicle.GetComponentsInChildren<RCC_WheelCollider>();

        //  Setting camber variable of front wheelcolliders.
        foreach (RCC_WheelCollider w in wc) {

            if (w == vehicle.FrontLeftWheelCollider || w == vehicle.FrontRightWheelCollider)
                w.camber = camberAngle;

        }

    }

    /// <summary>
    /// Sets the camber angle for rear wheels.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="camberAngle">Camber angle in degrees.</param>
    public static void SetRearCambers(RCC_CarControllerV4 vehicle, float camberAngle) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Getting all wheelcolliders.
        RCC_WheelCollider[] wc = vehicle.GetComponentsInChildren<RCC_WheelCollider>();

        //  Setting camber variable of rear wheelcolliders.
        foreach (RCC_WheelCollider w in wc) {

            if (w != vehicle.FrontLeftWheelCollider && w != vehicle.FrontRightWheelCollider)
                w.camber = camberAngle;

        }

    }

    /// <summary>
    /// Changes all wheel models on the vehicle. Wheel prefabs are configured via Tools > BCG > RCC > Configure Changable Wheels.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="wheel">Wheel model prefab to instantiate.</param>
    /// <param name="applyRadius">If true, recalculates WheelCollider radius from the new wheel model bounds.</param>
    public static void ChangeWheels(RCC_CarControllerV4 vehicle, GameObject wheel, bool applyRadius) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Getting all wheelcolliders.
        for (int i = 0; i < vehicle.AllWheelColliders.Length; i++) {

            //  Skip if wheel model is not assigned.
            if (!vehicle.AllWheelColliders[i].wheelModel)
                continue;

            //  Disabling renderer of the wheelmodel.
            if (vehicle.AllWheelColliders[i].wheelModel.GetComponent<MeshRenderer>())
                vehicle.AllWheelColliders[i].wheelModel.GetComponent<MeshRenderer>().enabled = false;

            //  Disabling all child models of the wheel.
            foreach (Transform t in vehicle.AllWheelColliders[i].wheelModel.GetComponentInChildren<Transform>())
                t.gameObject.SetActive(false);

            //  Instantiating new wheel.
            GameObject newWheel = Instantiate(wheel, vehicle.AllWheelColliders[i].wheelModel.position, vehicle.AllWheelColliders[i].wheelModel.rotation, vehicle.AllWheelColliders[i].wheelModel);

            //  If wheel is at right side, multiply scale X by -1 for symetry.
            if (vehicle.AllWheelColliders[i].wheelModel.localPosition.x > 0f)
                newWheel.transform.localScale = new Vector3(newWheel.transform.localScale.x * -1f, newWheel.transform.localScale.y, newWheel.transform.localScale.z);

            //  If apply radius is set to true, calculate the radius.
            if (applyRadius)
                vehicle.AllWheelColliders[i].WheelCollider.radius = RCC_GetBounds.MaxBoundsExtent(wheel.transform);

        }

    }

    /// <summary>
    /// Sets the suspension spring target position for front WheelColliders. Value is clamped between 0 and 1.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetPosition">Target position (0 = fully extended, 1 = fully compressed).</param>
    public static void SetFrontSuspensionsTargetPos(RCC_CarControllerV4 vehicle, float targetPosition) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Sets target position.
        targetPosition = Mathf.Clamp01(targetPosition);

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider)
            return;

        JointSpring spring1 = vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring;
        spring1.targetPosition = 1f - targetPosition;

        vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring = spring1;

        JointSpring spring2 = vehicle.FrontRightWheelCollider.WheelCollider.suspensionSpring;
        spring2.targetPosition = 1f - targetPosition;

        vehicle.FrontRightWheelCollider.WheelCollider.suspensionSpring = spring2;

    }

    /// <summary>
    /// Sets the suspension spring target position for rear WheelColliders. Value is clamped between 0 and 1.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetPosition">Target position (0 = fully extended, 1 = fully compressed).</param>
    public static void SetRearSuspensionsTargetPos(RCC_CarControllerV4 vehicle, float targetPosition) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Sets target position.
        targetPosition = Mathf.Clamp01(targetPosition);

        if (!vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        JointSpring spring1 = vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring;
        spring1.targetPosition = 1f - targetPosition;

        vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring = spring1;

        JointSpring spring2 = vehicle.RearRightWheelCollider.WheelCollider.suspensionSpring;
        spring2.targetPosition = 1f - targetPosition;

        vehicle.RearRightWheelCollider.WheelCollider.suspensionSpring = spring2;

    }

    /// <summary>
    /// Sets the suspension spring target position for all WheelColliders (front and rear). Value is clamped between 0 and 1.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetPosition">Target position (0 = fully extended, 1 = fully compressed).</param>
    public static void SetAllSuspensionsTargetPos(RCC_CarControllerV4 vehicle, float targetPosition) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Sets target position.
        targetPosition = Mathf.Clamp01(targetPosition);

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider || !vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        JointSpring spring1 = vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring;
        spring1.targetPosition = 1f - targetPosition;

        vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring = spring1;

        JointSpring spring2 = vehicle.RearRightWheelCollider.WheelCollider.suspensionSpring;
        spring2.targetPosition = 1f - targetPosition;

        vehicle.RearRightWheelCollider.WheelCollider.suspensionSpring = spring2;

        JointSpring spring3 = vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring;
        spring3.targetPosition = 1f - targetPosition;

        vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring = spring3;

        JointSpring spring4 = vehicle.FrontRightWheelCollider.WheelCollider.suspensionSpring;
        spring4.targetPosition = 1f - targetPosition;

        vehicle.FrontRightWheelCollider.WheelCollider.suspensionSpring = spring4;

    }

    /// <summary>
    /// Sets the suspension travel distance for front WheelColliders. Minimum clamped to 0.05 meters.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="distance">Suspension distance in meters.</param>
    public static void SetFrontSuspensionsDistances(RCC_CarControllerV4 vehicle, float distance) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Make sure new distance is not close to 0.
        if (distance <= .01)
            distance = .05f;

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider)
            return;

        //  Setting suspension distance of front wheelcolliders.
        vehicle.FrontLeftWheelCollider.WheelCollider.suspensionDistance = distance;
        vehicle.FrontRightWheelCollider.WheelCollider.suspensionDistance = distance;

    }

    /// <summary>
    /// Sets the suspension travel distance for rear WheelColliders, including extra rear wheels. Minimum clamped to 0.05 meters.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="distance">Suspension distance in meters.</param>
    public static void SetRearSuspensionsDistances(RCC_CarControllerV4 vehicle, float distance) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  Make sure new distance is not close to 0.
        if (distance <= .01)
            distance = .05f;

        if (!vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        //  Setting suspension distance of front wheelcolliders.
        vehicle.RearLeftWheelCollider.WheelCollider.suspensionDistance = distance;
        vehicle.RearRightWheelCollider.WheelCollider.suspensionDistance = distance;

        if (vehicle.ExtraRearWheelsCollider != null && vehicle.ExtraRearWheelsCollider.Length > 0) {

            foreach (RCC_WheelCollider wc in vehicle.ExtraRearWheelsCollider)
                wc.WheelCollider.suspensionDistance = distance;

        }

    }

    /// <summary>
    /// Sets the drivetrain mode (FWD, RWD, or AWD) on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="mode">Drivetrain type to apply.</param>
    public static void SetDrivetrainMode(RCC_CarControllerV4 vehicle, RCC_CarControllerV4.WheelType mode) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.wheelTypeChoise = mode;

    }

    /// <summary>
    /// Sets the gear shifting threshold. Lower values cause earlier upshifts, higher values cause later upshifts.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Gear shifting threshold value.</param>
    public static void SetGearShiftingThreshold(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.gearShiftingThreshold = targetValue;

    }

    /// <summary>
    /// Sets the clutch inertia threshold. Controls how quickly the clutch engages during gear shifts.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Clutch inertia value.</param>
    public static void SetClutchThreshold(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.clutchInertia = targetValue;

    }

    /// <summary>
    /// Enables or disables counter steering while the vehicle is drifting. Helps prevent spinning.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable counter steering, false to disable.</param>
    public static void SetCounterSteering(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.useCounterSteering = state;

    }

    /// <summary>
    /// Enables or disables the steering limiter while the vehicle is drifting. Helps prevent spinning.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable steering limiter, false to disable.</param>
    public static void SetSteeringLimit(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.useSteeringLimiter = state;

    }

    /// <summary>
    /// Enables or disables the Nitrous Oxide System (NOS) on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable NOS, false to disable.</param>
    public static void SetNOS(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.useNOS = state;

    }

    /// <summary>
    /// Enables or disables the turbo on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable turbo, false to disable.</param>
    public static void SetTurbo(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.useTurbo = state;

    }

    /// <summary>
    /// Enables or disables exhaust flame effects on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable exhaust flames, false to disable.</param>
    public static void SetUseExhaustFlame(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.useExhaustFlame = state;

    }

    /// <summary>
    /// Enables or disables the engine rev limiter on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable rev limiter, false to disable.</param>
    public static void SetRevLimiter(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.useRevLimiter = state;

    }

    /// <summary>
    /// Sets the suspension spring force for front WheelColliders.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Spring force value in Newtons.</param>
    public static void SetFrontSuspensionsSpringForce(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider)
            return;

        JointSpring spring = vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring;
        spring.spring = targetValue;
        vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;
        vehicle.FrontRightWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;

    }

    /// <summary>
    /// Sets the suspension spring force for rear WheelColliders.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Spring force value in Newtons.</param>
    public static void SetRearSuspensionsSpringForce(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        if (!vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        JointSpring spring = vehicle.RearLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring;
        spring.spring = targetValue;
        vehicle.RearLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;
        vehicle.RearRightWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;

    }

    /// <summary>
    /// Sets the suspension spring damper for front WheelColliders.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Damper strength value.</param>
    public static void SetFrontSuspensionsSpringDamper(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider)
            return;

        JointSpring spring = vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring;
        spring.damper = targetValue;
        vehicle.FrontLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;
        vehicle.FrontRightWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;

    }

    /// <summary>
    /// Sets the suspension spring damper for rear WheelColliders.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Damper strength value.</param>
    public static void SetRearSuspensionsSpringDamper(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        if (!vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        JointSpring spring = vehicle.RearLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring;
        spring.damper = targetValue;
        vehicle.RearLeftWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;
        vehicle.RearRightWheelCollider.GetComponent<WheelCollider>().suspensionSpring = spring;

    }

    /// <summary>
    /// Sets the maximum speed of the vehicle. Clamped between 10 and 400 km/h.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Maximum speed in km/h.</param>
    public static void SetMaximumSpeed(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.limitMaxSpeedAt = Mathf.Clamp(targetValue, 10f, 400f);

    }

    /// <summary>
    /// Sets the maximum engine torque of the vehicle. Clamped between 50 and 50000 Nm.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Maximum engine torque in Nm.</param>
    public static void SetMaximumTorque(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.maxEngineTorque = Mathf.Clamp(targetValue, 50f, 50000f);

    }

    /// <summary>
    /// Sets the maximum brake torque of the vehicle. Clamped between 0 and 50000 Nm.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="targetValue">Maximum brake torque in Nm.</param>
    public static void SetMaximumBrake(RCC_CarControllerV4 vehicle, float targetValue) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.brakeTorque = Mathf.Clamp(targetValue, 0f, 50000f);

    }

    /// <summary>
    /// Repairs all damage on the vehicle, restoring deformed meshes, detached parts, and broken components.
    /// </summary>
    /// <param name="vehicle">Target vehicle to repair.</param>
    public static void Repair(RCC_CarControllerV4 vehicle) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.Repair();

    }

    /// <summary>
    /// Enables or disables the Electronic Stability Program (ESP) on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable ESP, false to disable.</param>
    public static void SetESP(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.ESP = state;

    }

    /// <summary>
    /// Enables or disables the Anti-lock Braking System (ABS) on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable ABS, false to disable.</param>
    public static void SetABS(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.ABS = state;

    }

    /// <summary>
    /// Enables or disables the Traction Control System (TCS) on the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable TCS, false to disable.</param>
    public static void SetTCS(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.TCS = state;

    }

    /// <summary>
    /// Enables or disables the Steering Helper on the vehicle for improved stability.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="state">True to enable Steering Helper, false to disable.</param>
    public static void SetSH(RCC_CarControllerV4 vehicle, bool state) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.steeringHelper = state;

    }

    /// <summary>
    /// Sets the Steering Helper strength. Enables the steering helper and applies the given value to both linear and angular velocity strengths.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="value">Steering helper strength value.</param>
    public static void SetSHStrength(RCC_CarControllerV4 vehicle, float value) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.steeringHelper = true;
        vehicle.steerHelperLinearVelStrength = value;
        vehicle.steerHelperAngularVelStrength = value;

    }

    /// <summary>
    /// Sets the transmission type of the vehicle to automatic or manual.
    /// </summary>
    /// <param name="vehicle">Target vehicle.</param>
    /// <param name="automatic">True for automatic transmission, false for manual.</param>
    public static void SetTransmission(RCC_CarControllerV4 vehicle, bool automatic) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        vehicle.automaticGear = automatic;

    }

    /// <summary>
    /// Saves all vehicle customization stats (suspension, cambers, engine, assists, colors) to PlayerPrefs using the vehicle name as key prefix.
    /// </summary>
    /// <param name="vehicle">Target vehicle whose stats will be saved.</param>
    public static void SaveStats(RCC_CarControllerV4 vehicle) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider || !vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        //  Saving all major settings of the vehicle with PlayerPrefs.
        PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontCamber", vehicle.FrontLeftWheelCollider.camber);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_RearCamber", vehicle.RearLeftWheelCollider.camber);

        PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontSuspensionsDistance", vehicle.FrontLeftWheelCollider.WheelCollider.suspensionDistance);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_RearSuspensionsDistance", vehicle.RearLeftWheelCollider.WheelCollider.suspensionDistance);

        PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontSuspensionsSpring", vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring.spring);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_RearSuspensionsSpring", vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring.spring);

        PlayerPrefs.SetFloat(vehicle.transform.name + "_FrontSuspensionsDamper", vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring.damper);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_RearSuspensionsDamper", vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring.damper);

        PlayerPrefs.SetFloat(vehicle.transform.name + "_MaximumSpeed", vehicle.limitMaxSpeedAt);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_MaximumBrake", vehicle.brakeTorque);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_MaximumTorque", vehicle.maxEngineTorque);

        PlayerPrefs.SetString(vehicle.transform.name + "_DrivetrainMode", vehicle.wheelTypeChoise.ToString());

        PlayerPrefs.SetFloat(vehicle.transform.name + "_GearShiftingThreshold", vehicle.gearShiftingThreshold);
        PlayerPrefs.SetFloat(vehicle.transform.name + "_ClutchingThreshold", vehicle.clutchInertia);

        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "_CounterSteering", vehicle.useCounterSteering);

        foreach (RCC_Light _light in vehicle.GetComponentsInChildren<RCC_Light>()) {

            if (_light.lightType == RCC_Light.LightType.HeadLight) {

                RCC_PlayerPrefsX.SetColor(vehicle.transform.name + "_HeadlightsColor", _light.GetComponentInChildren<Light>().color);
                break;

            }

        }

        if (vehicle.RearLeftWheelCollider.allWheelParticles != null && vehicle.RearLeftWheelCollider.allWheelParticles.Count > 0) {

            ParticleSystem ps = vehicle.RearLeftWheelCollider.allWheelParticles[0];
            ParticleSystem.MainModule psmain = ps.main;

            RCC_PlayerPrefsX.SetColor(vehicle.transform.name + "_WheelsSmokeColor", psmain.startColor.color);

        }

        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "_ABS", vehicle.ABS);
        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "_ESP", vehicle.ESP);
        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "_TCS", vehicle.TCS);
        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "_SH", vehicle.steeringHelper);

        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "NOS", vehicle.useNOS);
        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "Turbo", vehicle.useTurbo);
        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "ExhaustFlame", vehicle.useExhaustFlame);
        RCC_PlayerPrefsX.SetBool(vehicle.transform.name + "RevLimiter", vehicle.useRevLimiter);

    }

    /// <summary>
    /// Loads all previously saved vehicle customization stats from PlayerPrefs and applies them to the vehicle.
    /// </summary>
    /// <param name="vehicle">Target vehicle to load stats into.</param>
    public static void LoadStats(RCC_CarControllerV4 vehicle) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        if (!vehicle.FrontLeftWheelCollider || !vehicle.FrontRightWheelCollider || !vehicle.RearLeftWheelCollider || !vehicle.RearRightWheelCollider)
            return;

        //  Loading all major settings of the vehicle with PlayerPrefs.
        SetFrontCambers(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontCamber", vehicle.FrontLeftWheelCollider.camber));
        SetRearCambers(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearCamber", vehicle.RearLeftWheelCollider.camber));

        SetFrontSuspensionsDistances(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontSuspensionsDistance", vehicle.FrontLeftWheelCollider.WheelCollider.suspensionDistance));
        SetRearSuspensionsDistances(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearSuspensionsDistance", vehicle.RearLeftWheelCollider.WheelCollider.suspensionDistance));

        SetFrontSuspensionsSpringForce(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontSuspensionsSpring", vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring.spring));
        SetRearSuspensionsSpringForce(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearSuspensionsSpring", vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring.spring));

        SetFrontSuspensionsSpringDamper(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_FrontSuspensionsDamper", vehicle.FrontLeftWheelCollider.WheelCollider.suspensionSpring.damper));
        SetRearSuspensionsSpringDamper(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_RearSuspensionsDamper", vehicle.RearLeftWheelCollider.WheelCollider.suspensionSpring.damper));

        SetMaximumSpeed(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_MaximumSpeed", vehicle.limitMaxSpeedAt));
        SetMaximumBrake(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_MaximumBrake", vehicle.brakeTorque));
        SetMaximumTorque(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_MaximumTorque", vehicle.maxEngineTorque));

        string drvtrn = PlayerPrefs.GetString(vehicle.transform.name + "_DrivetrainMode", vehicle.wheelTypeChoise.ToString());

        switch (drvtrn) {

            case "FWD":
                vehicle.wheelTypeChoise = RCC_CarControllerV4.WheelType.FWD;
                break;

            case "RWD":
                vehicle.wheelTypeChoise = RCC_CarControllerV4.WheelType.RWD;
                break;

            case "AWD":
                vehicle.wheelTypeChoise = RCC_CarControllerV4.WheelType.AWD;
                break;

        }

        SetGearShiftingThreshold(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_GearShiftingThreshold", vehicle.gearShiftingThreshold));
        SetClutchThreshold(vehicle, PlayerPrefs.GetFloat(vehicle.transform.name + "_ClutchingThreshold", vehicle.clutchInertia));

        SetCounterSteering(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "_CounterSteering", vehicle.useCounterSteering));

        SetABS(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "_ABS", vehicle.ABS));
        SetESP(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "_ESP", vehicle.ESP));
        SetTCS(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "_TCS", vehicle.TCS));
        SetSH(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "_SH", vehicle.steeringHelper));

        SetNOS(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "NOS", vehicle.useNOS));
        SetTurbo(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "Turbo", vehicle.useTurbo));
        SetUseExhaustFlame(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "ExhaustFlame", vehicle.useExhaustFlame));
        SetRevLimiter(vehicle, RCC_PlayerPrefsX.GetBool(vehicle.transform.name + "RevLimiter", vehicle.useRevLimiter));

        if (PlayerPrefs.HasKey(vehicle.transform.name + "_WheelsSmokeColor"))
            SetSmokeColor(vehicle, 0, RCC_PlayerPrefsX.GetColor(vehicle.transform.name + "_WheelsSmokeColor"));

        if (PlayerPrefs.HasKey(vehicle.transform.name + "_HeadlightsColor"))
            SetHeadlightsColor(vehicle, RCC_PlayerPrefsX.GetColor(vehicle.transform.name + "_HeadlightsColor"));

    }

    /// <summary>
    /// Resets all vehicle stats to the default vehicle's values and saves them to PlayerPrefs.
    /// </summary>
    /// <param name="vehicle">Target vehicle to reset.</param>
    /// <param name="defaultCar">Reference vehicle containing the default values.</param>
    public static void ResetStats(RCC_CarControllerV4 vehicle, RCC_CarControllerV4 defaultCar) {

        //  If no vehicle found, return.
        if (!CheckVehicle(vehicle))
            return;

        //  If no default vehicle found, return.
        if (!CheckVehicle(defaultCar))
            return;

        if (!defaultCar.FrontLeftWheelCollider || !defaultCar.RearLeftWheelCollider)
            return;

        SetFrontCambers(vehicle, defaultCar.FrontLeftWheelCollider.camber);
        SetRearCambers(vehicle, defaultCar.RearLeftWheelCollider.camber);

        SetFrontSuspensionsDistances(vehicle, defaultCar.FrontLeftWheelCollider.WheelCollider.suspensionDistance);
        SetRearSuspensionsDistances(vehicle, defaultCar.RearLeftWheelCollider.WheelCollider.suspensionDistance);

        SetFrontSuspensionsSpringForce(vehicle, defaultCar.FrontLeftWheelCollider.WheelCollider.suspensionSpring.spring);
        SetRearSuspensionsSpringForce(vehicle, defaultCar.RearLeftWheelCollider.WheelCollider.suspensionSpring.spring);

        SetFrontSuspensionsSpringDamper(vehicle, defaultCar.FrontLeftWheelCollider.WheelCollider.suspensionSpring.damper);
        SetRearSuspensionsSpringDamper(vehicle, defaultCar.RearLeftWheelCollider.WheelCollider.suspensionSpring.damper);

        SetMaximumSpeed(vehicle, defaultCar.limitMaxSpeedAt);
        SetMaximumBrake(vehicle, defaultCar.brakeTorque);
        SetMaximumTorque(vehicle, defaultCar.maxEngineTorque);

        string drvtrn = defaultCar.wheelTypeChoise.ToString();

        switch (drvtrn) {

            case "FWD":
                vehicle.wheelTypeChoise = RCC_CarControllerV4.WheelType.FWD;
                break;

            case "RWD":
                vehicle.wheelTypeChoise = RCC_CarControllerV4.WheelType.RWD;
                break;

            case "AWD":
                vehicle.wheelTypeChoise = RCC_CarControllerV4.WheelType.AWD;
                break;

        }

        SetGearShiftingThreshold(vehicle, defaultCar.gearShiftingThreshold);
        SetClutchThreshold(vehicle, defaultCar.clutchInertia);

        SetCounterSteering(vehicle, defaultCar.useCounterSteering);

        SetABS(vehicle, defaultCar.ABS);
        SetESP(vehicle, defaultCar.ESP);
        SetTCS(vehicle, defaultCar.TCS);
        SetSH(vehicle, defaultCar.steeringHelper);

        SetNOS(vehicle, defaultCar.useNOS);
        SetTurbo(vehicle, defaultCar.useTurbo);
        SetUseExhaustFlame(vehicle, defaultCar.useExhaustFlame);
        SetRevLimiter(vehicle, defaultCar.useRevLimiter);

        SetSmokeColor(vehicle, 0, Color.white);
        SetHeadlightsColor(vehicle, Color.white);

        SaveStats(vehicle);

    }

    /// <summary>
    /// Validates that the vehicle reference is not null. Logs an error if the vehicle is missing.
    /// </summary>
    /// <param name="vehicle">Vehicle reference to validate.</param>
    /// <returns>True if the vehicle exists, false otherwise.</returns>
    public static bool CheckVehicle(RCC_CarControllerV4 vehicle) {

        //  If no vehicle found, return with an error.
        if (!vehicle) {

#if UNITY_EDITOR
            Debug.LogError("Vehicle is missing!");
#endif
            return false;

        }

        return true;

    }

}
