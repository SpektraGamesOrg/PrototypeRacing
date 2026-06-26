//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

/// <summary>
/// Central ScriptableObject that stores all shared RCC configuration including behavior presets,
/// physics overrides, UI settings, audio clips, mobile controls, layer assignments, and prefab references.
/// Loaded as a singleton from "Resources/RCC Assets/RCC_Settings".
/// </summary>
[System.Serializable]
public class RCC_Settings : ScriptableObject {

    #region singleton

    /// <summary>
    /// Resets the cached instance when entering Play mode. Required for projects with domain reload disabled.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance() {
        instance = null;
    }

    /// <summary>
    /// Cached singleton instance. During Play mode, this is a runtime clone to prevent asset modifications.
    /// </summary>
    private static RCC_Settings instance;

    /// <summary>
    /// Singleton accessor. Loads the asset from "Resources/RCC Assets/RCC_Settings" on first access.
    /// During Play mode, returns a runtime clone so that changes (e.g., behavior preset switching)
    /// do not persist to the ScriptableObject asset after exiting Play mode.
    /// </summary>
    public static RCC_Settings Instance {

        get {

            if (instance == null) {

                instance = Resources.Load("RCC Assets/RCC_Settings") as RCC_Settings;

                if (Application.isPlaying)
                    instance = Instantiate(instance);

            }

            return instance;

        }

    }

    #endregion

    /// <summary>
    /// Index of the currently selected behavior preset in the <see cref="behaviorTypes"/> array.
    /// </summary>
    [Tooltip("Index of the currently selected behavior preset in the <see cref=\"behaviorTypes\"/> array.")]
    [Min(0)]
    public int behaviorSelectedIndex = 0;

    /// <summary>
    /// Returns the currently active <see cref="BehaviorType"/> based on <see cref="behaviorSelectedIndex"/>,
    /// or null if <see cref="overrideBehavior"/> is disabled.
    /// </summary>
    public BehaviorType selectedBehaviorType {

        get {

            if (overrideBehavior && behaviorTypes != null && behaviorSelectedIndex >= 0 && behaviorSelectedIndex < behaviorTypes.Length)
                return behaviorTypes[behaviorSelectedIndex];
            else
                return null;

        }

    }

    /// <summary>
    /// When true, the selected behavior preset overrides per-vehicle driving assist and friction settings.
    /// </summary>
    [Tooltip("When true, the selected behavior preset overrides per-vehicle driving assist and friction settings.")]
    public bool overrideBehavior = true;

    /// <summary>
    /// When true, overrides the application's target frame rate with <see cref="maxFPS"/>.
    /// </summary>
    [Tooltip("When true, overrides the application's target frame rate with <see cref=\"maxFPS\"/>.")]
    public bool overrideFPS = true;

    /// <summary>
    /// When true, overrides Unity's fixed timestep with <see cref="fixedTimeStep"/>.
    /// </summary>
    [Tooltip("When true, overrides Unity's fixed timestep with <see cref=\"fixedTimeStep\"/>.")]
    public bool overrideFixedTimeStep = true;

    /// <summary>
    /// The fixed timestep value (in seconds) used when <see cref="overrideFixedTimeStep"/> is enabled. Range: 0.005 to 0.06.
    /// </summary>
    [Tooltip("The fixed timestep value (in seconds) used when <see cref=\"overrideFixedTimeStep\"/> is enabled. Range: 0.005 to 0.06.")]
    [Range(.005f, .06f)] public float fixedTimeStep = .02f;

    /// <summary>
    /// Maximum angular velocity (in radians/sec) allowed for vehicle rigidbodies. Range: 0.5 to 20.
    /// </summary>
    [Tooltip("Maximum angular velocity (in radians/sec) allowed for vehicle rigidbodies. Range: 0.5 to 20.")]
    [Range(.5f, 20f)] public float maxAngularVelocity = 6;

    /// <summary>
    /// Target frame rate cap applied when <see cref="overrideFPS"/> is enabled.
    /// </summary>
    [Tooltip("Target frame rate cap applied when <see cref=\"overrideFPS\"/> is enabled.")]
    [Range(15, 240)]
    public int maxFPS = 60;

    /// <summary>
    /// When true, enables editor keyboard shortcuts (Shift+E: In-Scene GUI, Shift+R: Add Controller, Shift+S: Settings).
    /// </summary>
    [Tooltip("When true, enables editor keyboard shortcuts (Shift+E: In-Scene GUI, Shift+R: Add Controller, Shift+S: Settings).")]
    public bool useShortcuts = false;

    /// <summary>
    /// Defines a driving behavior preset that controls steering assists, traction helpers, friction curves,
    /// and other handling parameters. Multiple presets can be configured and switched at runtime.
    /// </summary>
    [System.Serializable]
    public class BehaviorType {

        /// <summary>
        /// Display name for this behavior preset (e.g., "Simulator", "Arcade", "Drift").
        /// </summary>
        [Tooltip("Display name for this behavior preset (e.g., \"Simulator\", \"Arcade\", \"Drift\").")]
        public string behaviorName = "New Behavior";

        /// <summary>
        /// Enables the steering helper that corrects the vehicle's direction to reduce fishtailing.
        /// </summary>
        [Tooltip("Enables the steering helper that corrects the vehicle's direction to reduce fishtailing.")]
        [Header("Steering Helpers")]
        public bool steeringHelper = true;

        /// <summary>
        /// Enables the traction helper that limits wheel spin by reducing engine torque when slip is detected.
        /// </summary>
        [Tooltip("Enables the traction helper that limits wheel spin by reducing engine torque when slip is detected.")]
        public bool tractionHelper = true;

        /// <summary>
        /// Enables the angular drag helper that dynamically adjusts angular drag based on vehicle speed.
        /// </summary>
        [Tooltip("Enables the angular drag helper that dynamically adjusts angular drag based on vehicle speed.")]
        public bool angularDragHelper = false;

        /// <summary>
        /// Enables automatic counter-steering to help stabilize the vehicle during oversteer.
        /// </summary>
        [Tooltip("Enables automatic counter-steering to help stabilize the vehicle during oversteer.")]
        public bool counterSteering = true;

        /// <summary>
        /// Limits the maximum steering angle at high speeds to improve stability.
        /// </summary>
        [Tooltip("Limits the maximum steering angle at high speeds to improve stability.")]
        public bool limitSteering = true;

        /// <summary>
        /// Enables speed-dependent steering sensitivity, making steering less responsive at higher speeds.
        /// </summary>
        [Tooltip("Enables speed-dependent steering sensitivity, making steering less responsive at higher speeds.")]
        public bool steeringSensitivity = true;

        /// <summary>
        /// Enables Anti-lock Braking System, which prevents wheel lockup during hard braking.
        /// </summary>
        [Tooltip("Enables Anti-lock Braking System, which prevents wheel lockup during hard braking.")]
        public bool ABS = false;

        /// <summary>
        /// Enables Electronic Stability Program, which applies selective braking to prevent skidding.
        /// </summary>
        [Tooltip("Enables Electronic Stability Program, which applies selective braking to prevent skidding.")]
        public bool ESP = false;

        /// <summary>
        /// Enables Traction Control System, which reduces engine power when wheel spin is detected.
        /// </summary>
        [Tooltip("Enables Traction Control System, which reduces engine power when wheel spin is detected.")]
        public bool TCS = false;

        /// <summary>
        /// When true, applies external wheel friction curves from the behavior preset instead of per-vehicle values.
        /// </summary>
        [Tooltip("When true, applies external wheel friction curves from the behavior preset instead of per-vehicle values.")]
        public bool applyExternalWheelFrictions = false;

        /// <summary>
        /// When true, applies relative torque forces to help stabilize or destabilize the vehicle's rotation.
        /// </summary>
        [Tooltip("When true, applies relative torque forces to help stabilize or destabilize the vehicle's rotation.")]
        public bool applyRelativeTorque = false;

        /// <summary>
        /// Steering input mode: constant angle or speed-based curve.
        /// </summary>
        [Tooltip("Steering input mode: constant angle or speed-based curve.")]
        public RCC_CarControllerV4.SteeringType steeringType = RCC_CarControllerV4.SteeringType.Curve;

        /// <summary>
        /// Animation curve mapping vehicle speed (x-axis, km/h) to maximum steering angle (y-axis, degrees).
        /// Only used when <see cref="steeringType"/> is set to Curve.
        /// </summary>
        [Tooltip("Animation curve mapping vehicle speed (x-axis, km/h) to maximum steering angle (y-axis, degrees). Only used when <see cref=\"steeringType\"/> is set to Curve.")]
        public AnimationCurve steeringCurve = new AnimationCurve(new Keyframe(0f, 40f), new Keyframe(50f, 20f), new Keyframe(100f, 11f), new Keyframe(150f, 6f), new Keyframe(200f, 5f));

        /// <summary>
        /// Center of Mass assister mode that dynamically adjusts COM for improved stability.
        /// </summary>
        [Tooltip("Center of Mass assister mode that dynamically adjusts COM for improved stability.")]
        public RCC_CarControllerV4.COMAssisterTypes comAssister = RCC_CarControllerV4.COMAssisterTypes.Off;

        /// <summary>
        /// Minimum allowed high-speed steering angle (degrees). Used as a clamp when <see cref="limitSteering"/> is enabled.
        /// </summary>
        [Tooltip("Minimum allowed high-speed steering angle (degrees). Used as a clamp when <see cref=\"limitSteering\"/> is enabled.")]
        [Space()]
        [Range(0f, 60f)]
        public float highSpeedSteerAngleMinimum = 20f;

        /// <summary>
        /// Maximum allowed high-speed steering angle (degrees). Used as a clamp when <see cref="limitSteering"/> is enabled.
        /// </summary>
        [Tooltip("Maximum allowed high-speed steering angle (degrees). Used as a clamp when <see cref=\"limitSteering\"/> is enabled.")]
        [Range(0f, 60f)]
        public float highSpeedSteerAngleMaximum = 40f;

        /// <summary>
        /// Minimum speed (km/h) at which high-speed steering angle limits begin to apply.
        /// </summary>
        [Tooltip("Minimum speed (km/h) at which high-speed steering angle limits begin to apply.")]
        [Min(0f)]
        public float highSpeedSteerAngleAtspeedMinimum = 100f;

        /// <summary>
        /// Maximum speed (km/h) at which the high-speed steering angle limit is fully applied.
        /// </summary>
        [Tooltip("Maximum speed (km/h) at which the high-speed steering angle limit is fully applied.")]
        [Min(0f)]
        public float highSpeedSteerAngleAtspeedMaximum = 200f;

        /// <summary>
        /// Minimum counter-steering strength multiplier.
        /// </summary>
        [Tooltip("Minimum counter-steering strength multiplier.")]
        [Space()]
        [Range(0f, 1f)]
        public float counterSteeringMinimum = .1f;

        /// <summary>
        /// Maximum counter-steering strength multiplier.
        /// </summary>
        [Tooltip("Maximum counter-steering strength multiplier.")]
        [Range(0f, 1f)]
        public float counterSteeringMaximum = 1f;

        /// <summary>
        /// Minimum steering sensitivity multiplier applied at high speed.
        /// </summary>
        [Tooltip("Minimum steering sensitivity multiplier applied at high speed.")]
        [Space()]
        [Range(0f, 1f)]
        public float steeringSensitivityMinimum = .5f;

        /// <summary>
        /// Maximum steering sensitivity multiplier applied at low speed.
        /// </summary>
        [Tooltip("Maximum steering sensitivity multiplier applied at low speed.")]
        [Range(0f, 1f)]
        public float steeringSensitivityMaximum = 1f;

        /// <summary>
        /// Minimum angular velocity correction strength for the steering helper. Range: 0 to 1.
        /// </summary>
        [Tooltip("Minimum angular velocity correction strength for the steering helper. Range: 0 to 1.")]
        [Space()]
        [Range(0f, 1f)] public float steerHelperAngularVelStrengthMinimum = .1f;

        /// <summary>
        /// Maximum angular velocity correction strength for the steering helper. Range: 0 to 1.
        /// </summary>
        [Tooltip("Maximum angular velocity correction strength for the steering helper. Range: 0 to 1.")]
        [Range(0f, 1f)] public float steerHelperAngularVelStrengthMaximum = 1;

        /// <summary>
        /// Minimum linear velocity correction strength for the steering helper. Range: 0 to 1.
        /// </summary>
        [Tooltip("Minimum linear velocity correction strength for the steering helper. Range: 0 to 1.")]
        [Range(0f, 1f)] public float steerHelperLinearVelStrengthMinimum = .1f;

        /// <summary>
        /// Maximum linear velocity correction strength for the steering helper. Range: 0 to 1.
        /// </summary>
        [Tooltip("Maximum linear velocity correction strength for the steering helper. Range: 0 to 1.")]
        [Range(0f, 1f)] public float steerHelperLinearVelStrengthMaximum = 1f;

        /// <summary>
        /// Minimum traction helper strength multiplier. Range: 0 to 1.
        /// </summary>
        [Tooltip("Minimum traction helper strength multiplier. Range: 0 to 1.")]
        [Range(0f, 1f)] public float tractionHelperStrengthMinimum = .1f;

        /// <summary>
        /// Maximum traction helper strength multiplier. Range: 0 to 1.
        /// </summary>
        [Tooltip("Maximum traction helper strength multiplier. Range: 0 to 1.")]
        [Range(0f, 1f)] public float tractionHelperStrengthMaximum = 1f;

        /// <summary>
        /// Minimum anti-roll bar force for the front axle (Newtons). Prevents excessive body roll in turns.
        /// </summary>
        [Tooltip("Minimum anti-roll bar force for the front axle (Newtons). Prevents excessive body roll in turns.")]
        [Space()]
        [Min(0f)]
        public float antiRollFrontHorizontalMinimum = 1000f;

        /// <summary>
        /// Minimum anti-roll bar force for the rear axle (Newtons). Prevents excessive body roll in turns.
        /// </summary>
        [Tooltip("Minimum anti-roll bar force for the rear axle (Newtons). Prevents excessive body roll in turns.")]
        [Min(0f)]
        public float antiRollRearHorizontalMinimum = 1000f;

        /// <summary>
        /// Maximum gear shifting delay (seconds). Limits how fast gear changes can occur. Range: 0 to 1.
        /// </summary>
        [Tooltip("Maximum gear shifting delay (seconds). Limits how fast gear changes can occur. Range: 0 to 1.")]
        [Space()]
        [Range(0f, 1f)] public float gearShiftingDelayMaximum = .15f;

        /// <summary>
        /// Base angular drag value applied to the vehicle rigidbody for this behavior. Range: 0 to 10.
        /// </summary>
        [Tooltip("Base angular drag value applied to the vehicle rigidbody for this behavior. Range: 0 to 10.")]
        [Range(0f, 10f)] public float angularDrag = .1f;

        /// <summary>
        /// Minimum dynamic angular drag helper strength. Range: 0 to 1.
        /// </summary>
        [Tooltip("Minimum dynamic angular drag helper strength. Range: 0 to 1.")]
        [Range(0f, 1f)] public float angularDragHelperMinimum = .1f;

        /// <summary>
        /// Maximum dynamic angular drag helper strength. Range: 0 to 1.
        /// </summary>
        [Tooltip("Maximum dynamic angular drag helper strength. Range: 0 to 1.")]
        [Range(0f, 1f)] public float angularDragHelperMaximum = 1f;

        /// <summary>
        /// Forward friction extremum slip value for this behavior preset.
        /// </summary>
        [Tooltip("Forward friction extremum slip value for this behavior preset.")]
        [Header("Wheel Frictions Forward")]
        [Min(0f)]
        public float forwardExtremumSlip = .4f;

        /// <summary>
        /// Forward friction extremum force value for this behavior preset.
        /// </summary>
        [Tooltip("Forward friction extremum force value for this behavior preset.")]
        [Min(0f)]
        public float forwardExtremumValue = 1f;

        /// <summary>
        /// Forward friction asymptote slip value for this behavior preset.
        /// </summary>
        [Tooltip("Forward friction asymptote slip value for this behavior preset.")]
        [Min(0f)]
        public float forwardAsymptoteSlip = .8f;

        /// <summary>
        /// Forward friction asymptote force value for this behavior preset.
        /// </summary>
        [Tooltip("Forward friction asymptote force value for this behavior preset.")]
        [Min(0f)]
        public float forwardAsymptoteValue = .5f;

        /// <summary>
        /// Sideways friction extremum slip value for this behavior preset.
        /// </summary>
        [Tooltip("Sideways friction extremum slip value for this behavior preset.")]
        [Header("Wheel Frictions Sideways")]
        [Min(0f)]
        public float sidewaysExtremumSlip = .2f;

        /// <summary>
        /// Sideways friction extremum force value for this behavior preset.
        /// </summary>
        [Tooltip("Sideways friction extremum force value for this behavior preset.")]
        [Min(0f)]
        public float sidewaysExtremumValue = 1f;

        /// <summary>
        /// Sideways friction asymptote slip value for this behavior preset.
        /// </summary>
        [Tooltip("Sideways friction asymptote slip value for this behavior preset.")]
        [Min(0f)]
        public float sidewaysAsymptoteSlip = .5f;

        /// <summary>
        /// Sideways friction asymptote force value for this behavior preset.
        /// </summary>
        [Tooltip("Sideways friction asymptote force value for this behavior preset.")]
        [Min(0f)]
        public float sidewaysAsymptoteValue = .75f;

    }

    /// <summary>
    /// When true, uses fixed wheel colliders with higher mass for improved physics stability.
    /// </summary>
    [Tooltip("When true, uses fixed wheel colliders with higher mass for improved physics stability.")]
    public bool useFixedWheelColliders = true;

    /// <summary>
    /// When true, the cursor is automatically locked during gameplay and unlocked when paused or in menus.
    /// </summary>
    [Tooltip("When true, the cursor is automatically locked during gameplay and unlocked when paused or in menus.")]
    public bool lockAndUnlockCursor = true;

    /// <summary>
    /// Array of available behavior presets. Each preset defines a complete set of driving assist and friction parameters.
    /// </summary>
    [Tooltip("Array of available behavior presets. Each preset defines a complete set of driving assist and friction parameters.")]
    public BehaviorType[] behaviorTypes;

    /// <summary>
    /// When true, vehicles automatically reset (flip upright) if they are upside down for a period of time.
    /// </summary>
    [Tooltip("When true, vehicles automatically reset (flip upright) if they are upside down for a period of time.")]
    public bool autoReset = true;

    /// <summary>
    /// Particle effect prefab spawned at collision contact points.
    /// </summary>
    [Tooltip("Particle effect prefab spawned at collision contact points.")]
    public GameObject contactParticles;

    /// <summary>
    /// Particle effect prefab spawned during surface scratching/scraping collisions.
    /// </summary>
    [Tooltip("Particle effect prefab spawned during surface scratching/scraping collisions.")]
    public GameObject scratchParticles;

    /// <summary>
    /// Particle effect prefab spawned when a wheel deflates.
    /// </summary>
    [Tooltip("Particle effect prefab spawned when a wheel deflates.")]
    public GameObject wheelDeflateParticles;

    /// <summary>
    /// The speed unit system used for display (speedometer, telemetry, UI).
    /// </summary>
    [Tooltip("The speed unit system used for display (speedometer, telemetry, UI).")]
    public Units units = Units.KMH;

    /// <summary>
    /// Speed unit options: kilometers per hour or miles per hour.
    /// </summary>
    public enum Units {

        /// <summary>
        /// Kilometers per hour.
        /// </summary>
        KMH,

        /// <summary>
        /// Miles per hour.
        /// </summary>
        MPH

    }

    /// <summary>
    /// The UI dashboard system to use for displaying vehicle information.
    /// </summary>
    [Tooltip("The UI dashboard system to use for displaying vehicle information.")]
    public UIType uiType = UIType.UI;

    /// <summary>
    /// Dashboard UI framework options.
    /// </summary>
    public enum UIType {

        /// <summary>
        /// Unity's built-in UI system (Canvas/EventSystem).
        /// </summary>
        UI,

        /// <summary>
        /// NGUI third-party UI framework (legacy support).
        /// </summary>
        NGUI,

        /// <summary>
        /// No dashboard UI will be displayed.
        /// </summary>
        None

    }

    /// <summary>
    /// When true, displays a real-time telemetry overlay with vehicle performance data.
    /// </summary>
    [Tooltip("When true, displays a real-time telemetry overlay with vehicle performance data.")]
    public bool useTelemetry = false;

    /// <summary>
    /// Mobile input controller types for touch-based devices.
    /// </summary>
    public enum MobileController {

        /// <summary>
        /// On-screen touch buttons for throttle, brake, and steering.
        /// </summary>
        TouchScreen,

        /// <summary>
        /// Device gyroscope/accelerometer for steering input.
        /// </summary>
        Gyro,

        /// <summary>
        /// On-screen virtual steering wheel for steering input.
        /// </summary>
        SteeringWheel,

        /// <summary>
        /// On-screen joystick for steering input.
        /// </summary>
        Joystick

    }

    /// <summary>
    /// The currently selected mobile input controller type.
    /// </summary>
    [Tooltip("The currently selected mobile input controller type.")]
    public MobileController mobileController = MobileController.TouchScreen;

    /// <summary>
    /// When true, enables mobile controller UI and reads input from mobile controls instead of keyboard/gamepad.
    /// </summary>
    [Tooltip("When true, enables mobile controller UI and reads input from mobile controls instead of keyboard/gamepad.")]
    public bool mobileControllerEnabled = false;

    /// <summary>
    /// Sensitivity of the on-screen UI buttons for throttle and brake input. Higher values respond faster.
    /// </summary>
    [Tooltip("Sensitivity of the on-screen UI buttons for throttle and brake input. Higher values respond faster.")]
    [Min(0f)]
    public float UIButtonSensitivity = 10f;

    /// <summary>
    /// Gravity (return-to-center speed) of the on-screen UI buttons when released.
    /// </summary>
    [Tooltip("Gravity (return-to-center speed) of the on-screen UI buttons when released.")]
    [Min(0f)]
    public float UIButtonGravity = 10f;

    /// <summary>
    /// Sensitivity multiplier for gyroscope-based steering on mobile devices.
    /// </summary>
    [Tooltip("Sensitivity multiplier for gyroscope-based steering on mobile devices.")]
    [Min(0f)]
    public float gyroSensitivity = 2f;

    /// <summary>
    /// When true, renders headlights as vertex (pixel) lights instead of real-time lights for better performance.
    /// </summary>
    [Tooltip("When true, renders headlights as vertex (pixel) lights instead of real-time lights for better performance.")]
    public bool useHeadLightsAsVertexLights = false;

    /// <summary>
    /// When true, renders brake lights as vertex (pixel) lights instead of real-time lights for better performance.
    /// </summary>
    [Tooltip("When true, renders brake lights as vertex (pixel) lights instead of real-time lights for better performance.")]
    public bool useBrakeLightsAsVertexLights = true;

    /// <summary>
    /// When true, renders reverse lights as vertex (pixel) lights instead of real-time lights for better performance.
    /// </summary>
    [Tooltip("When true, renders reverse lights as vertex (pixel) lights instead of real-time lights for better performance.")]
    public bool useReverseLightsAsVertexLights = true;

    /// <summary>
    /// When true, renders indicator lights as vertex (pixel) lights instead of real-time lights for better performance.
    /// </summary>
    [Tooltip("When true, renders indicator lights as vertex (pixel) lights instead of real-time lights for better performance.")]
    public bool useIndicatorLightsAsVertexLights = true;

    /// <summary>
    /// When true, renders other auxiliary lights as vertex (pixel) lights instead of real-time lights for better performance.
    /// </summary>
    [Tooltip("When true, renders other auxiliary lights as vertex (pixel) lights instead of real-time lights for better performance.")]
    public bool useOtherLightsAsVertexLights = true;

    /// <summary>
    /// When true, RCC automatically assigns physics layers to vehicle GameObjects on creation.
    /// </summary>
    [Tooltip("When true, RCC automatically assigns physics layers to vehicle GameObjects on creation.")]
    public bool setLayers = true;

    /// <summary>
    /// Layer name assigned to the main vehicle body colliders.
    /// </summary>
    [Tooltip("Layer name assigned to the main vehicle body colliders.")]
    public string RCCLayer = "RCC_Vehicle";

    /// <summary>
    /// Layer name assigned to wheel collider GameObjects.
    /// </summary>
    [Tooltip("Layer name assigned to wheel collider GameObjects.")]
    public string WheelColliderLayer = "RCC_WheelCollider";

    /// <summary>
    /// Layer name assigned to detachable body parts (doors, hoods, bumpers).
    /// </summary>
    [Tooltip("Layer name assigned to detachable body parts (doors, hoods, bumpers).")]
    public string DetachablePartLayer = "RCC_DetachablePart";

    /// <summary>
    /// Layer name assigned to prop objects (cones, barriers, etc.).
    /// </summary>
    [Tooltip("Layer name assigned to prop objects (cones, barriers, etc.).")]
    public string PropLayer = "RCC_Prop";

    /// <summary>
    /// PhysicMaterial applied to the main body colliders of all RCC vehicles.
    /// </summary>
    [Tooltip("PhysicMaterial applied to the main body colliders of all RCC vehicles.")]
    public PhysicsMaterial colliderMaterial;

    /// <summary>
    /// Prefab used for exhaust smoke particle effects.
    /// </summary>
    [Tooltip("Prefab used for exhaust smoke particle effects.")]
    public GameObject exhaustGas;

    /// <summary>
    /// Reference to the skidmarks manager prefab responsible for pooled skidmark rendering.
    /// </summary>
    [Tooltip("Reference to the skidmarks manager prefab responsible for pooled skidmark rendering.")]
    public RCC_SkidmarksManager skidmarksManager;

    /// <summary>
    /// Lens flare asset reference for URP (Universal Render Pipeline). Typed as Object for pipeline-agnostic compatibility.
    /// </summary>
    [Tooltip("Lens flare asset reference for URP (Universal Render Pipeline). Typed as Object for pipeline-agnostic compatibility.")]
    public Object lensflareURP;

    /// <summary>
    /// Prefab for headlight components added to vehicles.
    /// </summary>
    [Tooltip("Prefab for headlight components added to vehicles.")]
    public GameObject headLights;

    /// <summary>
    /// Prefab for brake light components added to vehicles.
    /// </summary>
    [Tooltip("Prefab for brake light components added to vehicles.")]
    public GameObject brakeLights;

    /// <summary>
    /// Prefab for reverse light components added to vehicles.
    /// </summary>
    [Tooltip("Prefab for reverse light components added to vehicles.")]
    public GameObject reverseLights;

    /// <summary>
    /// Prefab for indicator (turn signal) light components added to vehicles.
    /// </summary>
    [Tooltip("Prefab for indicator (turn signal) light components added to vehicles.")]
    public GameObject indicatorLights;

    /// <summary>
    /// Prefab for interior light components added to vehicles.
    /// </summary>
    [Tooltip("Prefab for interior light components added to vehicles.")]
    public GameObject interiorLights;

    /// <summary>
    /// Prefab for trailer light components.
    /// </summary>
    [Tooltip("Prefab for trailer light components.")]
    public GameObject lightTrailers;

    /// <summary>
    /// Prefab for side mirror components added to vehicles.
    /// </summary>
    [Tooltip("Prefab for side mirror components added to vehicles.")]
    public GameObject mirrors;

    /// <summary>
    /// Prefab for the main RCC follow camera that tracks the player vehicle.
    /// </summary>
    [Tooltip("Prefab for the main RCC follow camera that tracks the player vehicle.")]
    public RCC_Camera RCCMainCamera;

    /// <summary>
    /// Prefab for the hood/bonnet camera view.
    /// </summary>
    [Tooltip("Prefab for the hood/bonnet camera view.")]
    public GameObject hoodCamera;

    /// <summary>
    /// Prefab for the cinematic camera that provides automated cinematic shots.
    /// </summary>
    [Tooltip("Prefab for the cinematic camera that provides automated cinematic shots.")]
    public GameObject cinematicCamera;

    /// <summary>
    /// Prefab for the main RCC UI Canvas containing dashboard, buttons, and mobile controls.
    /// </summary>
    [Tooltip("Prefab for the main RCC UI Canvas containing dashboard, buttons, and mobile controls.")]
    public GameObject RCCCanvas;

    /// <summary>
    /// Prefab for the telemetry UI overlay that displays real-time vehicle data.
    /// </summary>
    [Tooltip("Prefab for the telemetry UI overlay that displays real-time vehicle data.")]
    public GameObject RCCTelemetry;

    /// <summary>
    /// When true, disables all particle effects (contact sparks, exhaust smoke, wheel dust) for performance.
    /// </summary>
    [Tooltip("When true, disables all particle effects (contact sparks, exhaust smoke, wheel dust) for performance.")]
    public bool dontUseAnyParticleEffects = false;

    /// <summary>
    /// When true, disables skidmark rendering for performance.
    /// </summary>
    [Tooltip("When true, disables skidmark rendering for performance.")]
    public bool dontUseSkidmarks = false;

    /// <summary>
    /// Audio mixer group that all RCC sound effects are routed through for centralized volume control.
    /// </summary>
    [Tooltip("Audio mixer group that all RCC sound effects are routed through for centralized volume control.")]
    public AudioMixerGroup audioMixer;

    /// <summary>
    /// Array of audio clips played randomly during gear shifts.
    /// </summary>
    [Tooltip("Array of audio clips played randomly during gear shifts.")]
    public AudioClip[] gearShiftingClips;

    /// <summary>
    /// Array of audio clips played randomly on collision impacts.
    /// </summary>
    [Tooltip("Array of audio clips played randomly on collision impacts.")]
    public AudioClip[] crashClips;

    /// <summary>
    /// Audio clip played while the vehicle is in reverse gear.
    /// </summary>
    [Tooltip("Audio clip played while the vehicle is in reverse gear.")]
    public AudioClip reversingClip;

    /// <summary>
    /// Audio clip for wind noise that increases in volume with vehicle speed.
    /// </summary>
    [Tooltip("Audio clip for wind noise that increases in volume with vehicle speed.")]
    public AudioClip windClip;

    /// <summary>
    /// Audio clip for brake squealing sound effects.
    /// </summary>
    [Tooltip("Audio clip for brake squealing sound effects.")]
    public AudioClip brakeClip;

    /// <summary>
    /// Audio clip played when a tire deflates.
    /// </summary>
    [Tooltip("Audio clip played when a tire deflates.")]
    public AudioClip wheelDeflateClip;

    /// <summary>
    /// Audio clip played when a tire is inflated/repaired.
    /// </summary>
    [Tooltip("Audio clip played when a tire is inflated/repaired.")]
    public AudioClip wheelInflateClip;

    /// <summary>
    /// Audio clip for the continuous sound of driving on a flat tire.
    /// </summary>
    [Tooltip("Audio clip for the continuous sound of driving on a flat tire.")]
    public AudioClip wheelFlatClip;

    /// <summary>
    /// Audio clip for the turn indicator clicking sound.
    /// </summary>
    [Tooltip("Audio clip for the turn indicator clicking sound.")]
    public AudioClip indicatorClip;

    /// <summary>
    /// Audio clip played when the vehicle drives over a bump or rough surface.
    /// </summary>
    [Tooltip("Audio clip played when the vehicle drives over a bump or rough surface.")]
    public AudioClip bumpClip;

    /// <summary>
    /// Audio clip for the nitrous oxide (NOS) boost activation sound.
    /// </summary>
    [Tooltip("Audio clip for the nitrous oxide (NOS) boost activation sound.")]
    public AudioClip NOSClip;

    /// <summary>
    /// Audio clip for the turbocharger whistle/spool sound.
    /// </summary>
    [Tooltip("Audio clip for the turbocharger whistle/spool sound.")]
    public AudioClip turboClip;

    /// <summary>
    /// Array of audio clips played randomly during tire blowout events.
    /// </summary>
    [Tooltip("Array of audio clips played randomly during tire blowout events.")]
    public AudioClip[] blowoutClip;

    /// <summary>
    /// Array of audio clips for exhaust backfire/flame pop sounds.
    /// </summary>
    [Tooltip("Array of audio clips for exhaust backfire/flame pop sounds.")]
    public AudioClip[] exhaustFlameClips;

    /// <summary>
    /// Reference to the HDRP Volume Profile prefab used when running on High Definition Render Pipeline.
    /// </summary>
    [Tooltip("Reference to the HDRP Volume Profile prefab used when running on High Definition Render Pipeline.")]
    public Object hdrpVolumeProfilePrefab;

    /// <summary>
    /// Default material used for vehicle decals in the customization system.
    /// </summary>
    [Tooltip("Default material used for vehicle decals in the customization system.")]
    public Material defaultDecalMaterial;

    /// <summary>
    /// Default material used for underglow neon lights in the customization system.
    /// </summary>
    [Tooltip("Default material used for underglow neon lights in the customization system.")]
    public Material defaultNeonMaterial;

    /// <summary>
    /// Maximum volume for gear shifting sound effects. Range: 0 to 1.
    /// </summary>
    [Tooltip("Maximum volume for gear shifting sound effects. Range: 0 to 1.")]
    [Range(0f, 1f)] public float maxGearShiftingSoundVolume = .25f;

    /// <summary>
    /// Maximum volume for crash/collision sound effects. Range: 0 to 1.
    /// </summary>
    [Tooltip("Maximum volume for crash/collision sound effects. Range: 0 to 1.")]
    [Range(0f, 1f)] public float maxCrashSoundVolume = 1f;

    /// <summary>
    /// Maximum volume for the wind noise sound effect. Range: 0 to 1.
    /// </summary>
    [Tooltip("Maximum volume for the wind noise sound effect. Range: 0 to 1.")]
    [Range(0f, 1f)] public float maxWindSoundVolume = .1f;

    /// <summary>
    /// Maximum volume for the brake squealing sound effect. Range: 0 to 1.
    /// </summary>
    [Tooltip("Maximum volume for the brake squealing sound effect. Range: 0 to 1.")]
    [Range(0f, 1f)] public float maxBrakeSoundVolume = .1f;

    /// <summary>
    /// Editor fold state for the General Settings section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the General Settings section in the RCC Settings inspector.")]
    public bool foldGeneralSettings = false;

    /// <summary>
    /// Editor fold state for the Behavior Settings section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the Behavior Settings section in the RCC Settings inspector.")]
    public bool foldBehaviorSettings = false;

    /// <summary>
    /// Editor fold state for the Controller Settings section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the Controller Settings section in the RCC Settings inspector.")]
    public bool foldControllerSettings = false;

    /// <summary>
    /// Editor fold state for the UI Settings section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the UI Settings section in the RCC Settings inspector.")]
    public bool foldUISettings = false;

    /// <summary>
    /// Editor fold state for the Wheel Physics section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the Wheel Physics section in the RCC Settings inspector.")]
    public bool foldWheelPhysics = false;

    /// <summary>
    /// Editor fold state for the Sound FX section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the Sound FX section in the RCC Settings inspector.")]
    public bool foldSFX = false;

    /// <summary>
    /// Editor fold state for the Optimization section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the Optimization section in the RCC Settings inspector.")]
    public bool foldOptimization = false;

    /// <summary>
    /// Editor fold state for the Tags and Layers section in the RCC Settings inspector.
    /// </summary>
    [Tooltip("Editor fold state for the Tags and Layers section in the RCC Settings inspector.")]
    public bool foldTagsAndLayers = false;

}
