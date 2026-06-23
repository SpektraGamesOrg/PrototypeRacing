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
/// Serializable data class that stores all vehicle customization settings including suspension, camber, driving assists, and visual options.
/// Used by the customization system to persist and restore vehicle tuning configurations.
/// </summary>
[System.Serializable]
public class RCC_CustomizationData {

    /// <summary>
    /// Whether this customization data has been initialized with values from the vehicle.
    /// </summary>
    [Tooltip("Whether this customization data has been initialized with values from the vehicle.")]
    public bool initialized = false;

    /// <summary>
    /// Front suspension travel distance in meters.
    /// </summary>
    [Header("Suspension")]
    [Tooltip("Front suspension travel distance in meters.")]
    [Min(0f)]
    public float suspensionDistanceFront = .2f;

    /// <summary>
    /// Rear suspension travel distance in meters.
    /// </summary>
    [Tooltip("Rear suspension travel distance in meters.")]
    [Min(0f)]
    public float suspensionDistanceRear = .2f;

    /// <summary>
    /// Front suspension spring force in Newtons.
    /// </summary>
    [Tooltip("Front suspension spring force in Newtons.")]
    [Min(0f)]
    public float suspensionSpringForceFront = 55000f;

    /// <summary>
    /// Rear suspension spring force in Newtons.
    /// </summary>
    [Tooltip("Rear suspension spring force in Newtons.")]
    [Min(0f)]
    public float suspensionSpringForceRear = 55000f;

    /// <summary>
    /// Front suspension damper strength.
    /// </summary>
    [Tooltip("Front suspension damper strength.")]
    [Min(0f)]
    public float suspensionDamperFront = 3500f;

    /// <summary>
    /// Rear suspension damper strength.
    /// </summary>
    [Tooltip("Rear suspension damper strength.")]
    [Min(0f)]
    public float suspensionDamperRear = 3500f;

    /// <summary>
    /// Front suspension spring target position (0 to 1).
    /// </summary>
    [Tooltip("Front suspension spring target position (0 to 1).")]
    [Range(0f, 1f)]
    public float suspensionTargetFront = .5f;

    /// <summary>
    /// Rear suspension spring target position (0 to 1).
    /// </summary>
    [Tooltip("Rear suspension spring target position (0 to 1).")]
    [Range(0f, 1f)]
    public float suspensionTargetRear = .5f;

    /// <summary>
    /// Front wheel camber angle in degrees.
    /// </summary>
    [Header("Camber")]
    [Tooltip("Front wheel camber angle in degrees.")]
    [Range(-5f, 5f)]
    public float cambersFront = 0f;

    /// <summary>
    /// Rear wheel camber angle in degrees.
    /// </summary>
    [Tooltip("Rear wheel camber angle in degrees.")]
    [Range(-5f, 5f)]
    public float cambersRear = 0f;

    /// <summary>
    /// Gear shifting threshold for automatic transmission. Lower values shift earlier, higher values shift later.
    /// </summary>
    [Header("Transmission")]
    [Tooltip("Gear shifting threshold for automatic transmission. Lower values shift earlier, higher values shift later.")]
    [Range(0.25f, 1f)]
    public float gearShiftingThreshold = .8f;

    /// <summary>
    /// Clutch engagement threshold. Controls how quickly the clutch engages during gear shifts.
    /// </summary>
    [Tooltip("Clutch engagement threshold. Controls how quickly the clutch engages during gear shifts.")]
    [Range(0f, 1f)]
    public float clutchThreshold = .1f;

    /// <summary>
    /// Enables counter steering assistance to help prevent spinning during drifts.
    /// </summary>
    [Header("Driving Assists")]
    [Tooltip("Enables counter steering assistance to help prevent spinning during drifts.")]
    public bool counterSteering = true;

    /// <summary>
    /// Enables steering angle limiter to prevent over-steering at high speeds.
    /// </summary>
    [Tooltip("Enables steering angle limiter to prevent over-steering at high speeds.")]
    public bool steeringLimiter = true;

    /// <summary>
    /// Enables Anti-lock Braking System (ABS).
    /// </summary>
    [Tooltip("Enables Anti-lock Braking System (ABS).")]
    public bool ABS;

    /// <summary>
    /// Enables Electronic Stability Program (ESP).
    /// </summary>
    [Tooltip("Enables Electronic Stability Program (ESP).")]
    public bool ESP;

    /// <summary>
    /// Enables Traction Control System (TCS).
    /// </summary>
    [Tooltip("Enables Traction Control System (TCS).")]
    public bool TCS;

    /// <summary>
    /// Enables Steering Helper for improved vehicle stability.
    /// </summary>
    [Tooltip("Enables Steering Helper for improved vehicle stability.")]
    public bool SH;

    /// <summary>
    /// Enables Nitrous Oxide System (NOS) boost.
    /// </summary>
    [Tooltip("Enables Nitrous Oxide System (NOS) boost.")]
    public bool NOS;

    /// <summary>
    /// Enables the engine rev limiter.
    /// </summary>
    [Tooltip("Enables the engine rev limiter.")]
    public bool revLimiter;

    /// <summary>
    /// Enables automatic transmission mode.
    /// </summary>
    [Tooltip("Enables automatic transmission mode.")]
    public bool automaticTransmission;

    /// <summary>
    /// Color of the vehicle headlights.
    /// </summary>
    [Header("Colors")]
    [Tooltip("Color of the vehicle headlights.")]
    public Color headlightColor = Color.white;

    /// <summary>
    /// Color of the wheel smoke particles.
    /// </summary>
    [Tooltip("Color of the wheel smoke particles.")]
    public Color wheelSmokeColor = Color.white;

    /// <summary>
    /// Default constructor with preset values.
    /// </summary>
    public RCC_CustomizationData() { }

    /// <summary>
    /// Constructor that initializes all customization data fields.
    /// </summary>
    /// <param name="initialized">Whether data has been initialized from vehicle values.</param>
    /// <param name="suspensionDistanceFront">Front suspension travel distance.</param>
    /// <param name="suspensionDistanceRear">Rear suspension travel distance.</param>
    /// <param name="suspensionSpringForceFront">Front suspension spring force.</param>
    /// <param name="suspensionSpringForceRear">Rear suspension spring force.</param>
    /// <param name="suspensionDamperFront">Front suspension damper strength.</param>
    /// <param name="suspensionDamperRear">Rear suspension damper strength.</param>
    /// <param name="suspensionTargetFront">Front suspension target position.</param>
    /// <param name="suspensionTargetRear">Rear suspension target position.</param>
    /// <param name="cambersFront">Front wheel camber angle.</param>
    /// <param name="cambersRear">Rear wheel camber angle.</param>
    /// <param name="gearShiftingThreshold">Automatic gear shifting threshold.</param>
    /// <param name="clutchThreshold">Clutch engagement threshold.</param>
    /// <param name="counterSteering">Enable counter steering.</param>
    /// <param name="steeringLimiter">Enable steering limiter.</param>
    /// <param name="ABS">Enable Anti-lock Braking System.</param>
    /// <param name="ESP">Enable Electronic Stability Program.</param>
    /// <param name="TCS">Enable Traction Control System.</param>
    /// <param name="SH">Enable Steering Helper.</param>
    /// <param name="NOS">Enable Nitrous Oxide System.</param>
    /// <param name="revLimiter">Enable rev limiter.</param>
    /// <param name="automaticTransmission">Enable automatic transmission.</param>
    /// <param name="headlightColor">Headlight color.</param>
    /// <param name="wheelSmokeColor">Wheel smoke particle color.</param>
    public RCC_CustomizationData(bool initialized, float suspensionDistanceFront, float suspensionDistanceRear, float suspensionSpringForceFront, float suspensionSpringForceRear, float suspensionDamperFront, float suspensionDamperRear, float suspensionTargetFront, float suspensionTargetRear, float cambersFront, float cambersRear, float gearShiftingThreshold, float clutchThreshold, bool counterSteering, bool steeringLimiter, bool ABS, bool ESP, bool TCS, bool SH, bool NOS, bool revLimiter, bool automaticTransmission, Color headlightColor, Color wheelSmokeColor) {

        this.initialized = initialized;
        this.suspensionDistanceFront = suspensionDistanceFront;
        this.suspensionDistanceRear = suspensionDistanceRear;
        this.suspensionSpringForceFront = suspensionSpringForceFront;
        this.suspensionSpringForceRear = suspensionSpringForceRear;
        this.suspensionDamperFront = suspensionDamperFront;
        this.suspensionDamperRear = suspensionDamperRear;
        this.suspensionTargetFront = suspensionTargetFront;
        this.suspensionTargetRear = suspensionTargetRear;
        this.cambersFront = cambersFront;
        this.cambersRear = cambersRear;
        this.gearShiftingThreshold = gearShiftingThreshold;
        this.clutchThreshold = clutchThreshold;
        this.counterSteering = counterSteering;
        this.steeringLimiter = steeringLimiter;
        this.ABS = ABS;
        this.ESP = ESP;
        this.TCS = TCS;
        this.SH = SH;
        this.NOS = NOS;
        this.revLimiter = revLimiter;
        this.automaticTransmission = automaticTransmission;
        this.headlightColor = headlightColor;
        this.wheelSmokeColor = wheelSmokeColor;

    }

}
