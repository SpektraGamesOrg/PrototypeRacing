//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------


using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// AI vehicle controller that drives an RCC vehicle autonomously. Supports three navigation modes:
/// following waypoints along a predefined path, chasing a target GameObject, or following a target
/// while maintaining a safe distance. Uses Unity's NavMeshAgent for pathfinding and raycasts for
/// obstacle avoidance. Requires <see cref="RCC_CarControllerV4"/> on the same GameObject.
/// </summary>
[RequireComponent(typeof(RCC_CarControllerV4))]
public class RCC_AICarController : RCC_Core {

    /// <summary>
    /// Reference to the waypoints container that holds the path for this AI vehicle to follow.
    /// If not assigned in the Inspector, it will be auto-found in the scene at Awake.
    /// </summary>
    [Tooltip("Reference to the waypoints container that holds the path for this AI vehicle to follow. If not assigned in the Inspector, it will be auto-found in the scene at Awake.")]
    public RCC_AIWaypointsContainer waypointsContainer;

    /// <summary>
    /// Current index of the active waypoint in the <see cref="waypointsContainer"/> waypoints list.
    /// </summary>
    [Tooltip("Current index of the active waypoint in the <see cref=\"waypointsContainer\"/> waypoints list.")]
    [Min(0)]
    public int currentWaypointIndex = 0;

    /// <summary>
    /// Tag used to identify target GameObjects when in ChaseTarget or FollowTarget navigation mode.
    /// GameObjects with this tag within the <see cref="detectorRadius"/> will be considered as potential targets.
    /// </summary>
    [Tooltip("Tag used to identify target GameObjects when in ChaseTarget or FollowTarget navigation mode. GameObjects with this tag within the <see cref=\"detectorRadius\"/> will be considered as potential targets.")]
    public string targetTag = "Player";

    /// <summary>
    /// Navigation mode for the AI (FollowWaypoints, ChaseTarget, FollowTarget).
    /// </summary>
    [Tooltip("Navigation mode for the AI (FollowWaypoints, ChaseTarget, FollowTarget).")]
    public NavigationMode navigationMode = NavigationMode.FollowWaypoints;

    /// <summary>
    /// Defines how the AI vehicle navigates in the scene.
    /// </summary>
    public enum NavigationMode {

        /// <summary>
        /// AI follows waypoints in the <see cref="waypointsContainer"/>, looping back to the first waypoint after reaching the last.
        /// </summary>
        FollowWaypoints,

        /// <summary>
        /// AI actively chases the closest target with the matching <see cref="targetTag"/>, driving at full speed toward it.
        /// </summary>
        ChaseTarget,

        /// <summary>
        /// AI follows the closest target with the matching <see cref="targetTag"/>, slowing down and stopping within <see cref="stopFollowDistance"/>.
        /// </summary>
        FollowTarget

    }

    /// <summary>
    /// Maximum length (in meters) of the raycasts used for detecting obstacles in front of the AI vehicle.
    /// </summary>
    [Tooltip("Maximum length (in meters) of the raycasts used for detecting obstacles in front of the AI vehicle.")]
    [Range(5f, 30f)] public float raycastLength = 3f;

    /// <summary>
    /// Maximum spread angle (in degrees) for the sideways raycasts used to detect obstacles at the front of the vehicle.
    /// </summary>
    [Tooltip("Maximum spread angle (in degrees) for the sideways raycasts used to detect obstacles at the front of the vehicle.")]
    [Range(10f, 90f)] public float raycastAngle = 30f;

    /// <summary>
    /// Layers considered as obstacles for raycasts.
    /// </summary>
    [Tooltip("Layers considered as obstacles for raycasts.")]
    public LayerMask obstacleLayers = -1;

    /// <summary>
    /// Currently detected obstacle GameObject hit by the AI's raycasts, or null if no obstacle is detected.
    /// </summary>
    [Tooltip("Currently detected obstacle GameObject hit by the AI's raycasts, or null if no obstacle is detected.")]
    public GameObject obstacle;

    /// <summary>
    /// When enabled, the AI uses forward and angled raycasts to detect and steer around obstacles at runtime.
    /// </summary>
    [Tooltip("When enabled, the AI uses forward and angled raycasts to detect and steer around obstacles at runtime.")]
    public bool useRaycasts = true;

    /// <summary>
    /// Local-space offset from the vehicle's transform used as the origin point for obstacle-detection raycasts.
    /// </summary>
    [Tooltip("Local-space offset from the vehicle's transform used as the origin point for obstacle-detection raycasts.")]
    public Vector3 rayOrigin = new Vector3(0f, .1f, 2f);

    /// <summary>
    /// Accumulated steering input from obstacle-detection raycasts. Negative values steer left, positive values steer right.
    /// </summary>
    private float rayInput = 0f;

    /// <summary>
    /// Whether any of the obstacle-detection raycasts are currently hitting an obstacle.
    /// </summary>
    private bool raycasting = false;

    /// <summary>
    /// Timer (in seconds) that tracks how long the vehicle has been stuck. When this exceeds a threshold, the AI reverses.
    /// </summary>
    private float resetTime = 0f;

    /// <summary>
    /// Whether the AI vehicle is currently reversing to recover from being stuck or after a collision.
    /// </summary>
    private bool reversingNow = false;

    // Steer, Motor, And Brake inputs. Will feed RCC_CarController with these inputs.

    /// <summary>
    /// Calculated steering input for the AI vehicle, ranging from -1 (full left) to 1 (full right).
    /// </summary>
    [Tooltip("Calculated steering input for the AI vehicle, ranging from -1 (full left) to 1 (full right).")]
    public float steerInput = 0f;

    /// <summary>
    /// Calculated throttle input for the AI vehicle, ranging from 0 (no throttle) to 1 (full throttle).
    /// </summary>
    [Tooltip("Calculated throttle input for the AI vehicle, ranging from 0 (no throttle) to 1 (full throttle).")]
    public float throttleInput = 0f;

    /// <summary>
    /// Calculated brake input for the AI vehicle, ranging from 0 (no braking) to 1 (full brake).
    /// </summary>
    [Tooltip("Calculated brake input for the AI vehicle, ranging from 0 (no braking) to 1 (full brake).")]
    public float brakeInput = 0f;

    /// <summary>
    /// Calculated handbrake input for the AI vehicle, ranging from 0 (released) to 1 (fully engaged).
    /// </summary>
    [Tooltip("Calculated handbrake input for the AI vehicle, ranging from 0 (released) to 1 (fully engaged).")]
    public float handbrakeInput = 0f;

    /// <summary>
    /// When enabled, the AI vehicle will not exceed the <see cref="maximumSpeed"/> value. When disabled, the vehicle's own max speed is used.
    /// </summary>
    [Tooltip("When enabled, the AI vehicle will not exceed the <see cref=\"maximumSpeed\"/> value. When disabled, the vehicle's own max speed is used.")]
    public bool limitSpeed = false;

    /// <summary>
    /// Maximum speed (km/h) the AI vehicle is allowed to reach when <see cref="limitSpeed"/> is enabled.
    /// </summary>
    [Tooltip("Maximum speed (km/h) the AI vehicle is allowed to reach when <see cref=\"limitSpeed\"/> is enabled.")]
    [Min(0f)]
    public float maximumSpeed = 100f;

    /// <summary>
    /// When enabled, steering input is smoothly interpolated over time rather than applied instantly, resulting in smoother turns.
    /// </summary>
    [Tooltip("When enabled, steering input is smoothly interpolated over time rather than applied instantly, resulting in smoother turns.")]
    public bool smoothedSteer = true;

    // Counts laps and how many waypoints were passed.

    /// <summary>
    /// Number of completed laps (full circuits through all waypoints). Incremented each time the AI passes the last waypoint.
    /// </summary>
    [Tooltip("Number of completed laps (full circuits through all waypoints). Incremented each time the AI passes the last waypoint.")]
    [Min(0)]
    public int lap = 0;

    /// <summary>
    /// When enabled, the AI vehicle will stop driving after completing the number of laps specified by <see cref="stopLap"/>.
    /// </summary>
    [Tooltip("When enabled, the AI vehicle will stop driving after completing the number of laps specified by <see cref=\"stopLap\"/>.")]
    public bool stopAfterLap = false;

    /// <summary>
    /// The lap number at which the AI vehicle will stop driving, if <see cref="stopAfterLap"/> is enabled.
    /// </summary>
    [Tooltip("The lap number at which the AI vehicle will stop driving, if <see cref=\"stopAfterLap\"/> is enabled.")]
    [Min(0)]
    public int stopLap = 10;

    /// <summary>
    /// Cumulative count of all waypoints the AI vehicle has passed, including across multiple laps.
    /// </summary>
    [Tooltip("Cumulative count of all waypoints the AI vehicle has passed, including across multiple laps.")]
    [Min(0)]
    public int totalWaypointPassed = 0;

    /// <summary>
    /// When true, the AI ignores the navigator's waypoint-based steering and relies solely on raycast-based obstacle avoidance steering.
    /// </summary>
    [Tooltip("When true, the AI ignores the navigator's waypoint-based steering and relies solely on raycast-based obstacle avoidance steering.")]
    public bool ignoreWaypointNow = false;

    /// <summary>
    /// Radius (in meters) of the sphere used to detect nearby targets and brake zones via Physics.OverlapSphere.
    /// </summary>
    [Tooltip("Radius (in meters) of the sphere used to detect nearby targets and brake zones via Physics.OverlapSphere.")]
    [Min(0)]
    public int detectorRadius = 200;

    /// <summary>
    /// Distance (in meters) at which the AI begins following or chasing a detected target.
    /// </summary>
    [Tooltip("Distance (in meters) at which the AI begins following or chasing a detected target.")]
    [Min(0)]
    public int startFollowDistance = 300;

    /// <summary>
    /// Distance (in meters) at which the AI stops and holds position near the target in FollowTarget mode.
    /// </summary>
    [Tooltip("Distance (in meters) at which the AI stops and holds position near the target in FollowTarget mode.")]
    [Min(0)]
    public int stopFollowDistance = 30;

    /// <summary>
    /// Flag indicating whether the target detection sphere query should run on the next Update cycle.
    /// </summary>
    private bool updateTargets = false;

    /// <summary>
    /// Time (in seconds) elapsed since the last target detection update. Triggers a new update every 1 second.
    /// </summary>
    private float lastUpdatedTargets = 0f;

    /// <summary>
    /// NavMeshAgent used for pathfinding to calculate the desired velocity direction toward the current destination.
    /// Created as a child GameObject during Awake.
    /// </summary>
    private NavMeshAgent navigator;

    /// <summary>
    /// List of potential target transforms currently within the <see cref="detectorRadius"/>.
    /// Used in ChaseTarget and FollowTarget modes to select the closest target.
    /// </summary>
    [Tooltip("List of potential target transforms currently within the <see cref=\"detectorRadius\"/>. Used in ChaseTarget and FollowTarget modes to select the closest target.")]
    public List<Transform> targetsInZone = new List<Transform>();

    /// <summary>
    /// List of <see cref="RCC_AIBrakeZone"/> instances currently within the <see cref="detectorRadius"/>.
    /// The closest one is assigned to <see cref="targetBrake"/> each frame.
    /// </summary>
    [Tooltip("List of <see cref=\"RCC_AIBrakeZone\"/> instances currently within the <see cref=\"detectorRadius\"/>. The closest one is assigned to <see cref=\"targetBrake\"/> each frame.")]
    public List<RCC_AIBrakeZone> brakeZones = new List<RCC_AIBrakeZone>();

    /// <summary>
    /// The closest target transform currently being chased or followed. Automatically assigned from <see cref="targetsInZone"/>.
    /// </summary>
    [Tooltip("The closest target transform currently being chased or followed. Automatically assigned from <see cref=\"targetsInZone\"/>.")]
    public Transform targetChase;

    /// <summary>
    /// The closest brake zone to the AI vehicle. When the vehicle is within this zone's distance and exceeds its target speed, braking is applied.
    /// </summary>
    [Tooltip("The closest brake zone to the AI vehicle. When the vehicle is within this zone's distance and exceeds its target speed, braking is applied.")]
    public RCC_AIBrakeZone targetBrake;

    /// <summary>
    /// Unity's Awake method. Initializes the WaypointsContainer if not assigned, and sets up the Navigator (NavMeshAgent).
    /// </summary>
    private void Awake() {

        // If Waypoints Container is not selected in Inspector Panel, find it on scene.
#if !UNITY_2022_1_OR_NEWER
        if (!waypointsContainer)
            waypointsContainer = FindObjectOfType(typeof(RCC_AIWaypointsContainer)) as RCC_AIWaypointsContainer;
#else
        if (!waypointsContainer)
            waypointsContainer = FindFirstObjectByType(typeof(RCC_AIWaypointsContainer)) as RCC_AIWaypointsContainer;
#endif

        // Creating our Navigator and setting properties.
        GameObject navigatorObject = new GameObject("Navigator");
        navigatorObject.transform.SetParent(transform, false);
        navigator = navigatorObject.AddComponent<NavMeshAgent>();
        navigator.radius = 1;
        navigator.speed = 1;
        navigator.angularSpeed = 100000f;
        navigator.acceleration = 100000f;
        navigator.height = 1;
        navigator.avoidancePriority = 0;

    }

    /// <summary>
    /// Called when the object becomes enabled/active. Sets external controller for the CarController and fires OnRCCAISpawned event.
    /// </summary>
    private void OnEnable() {

        //  Setting external controller on enable.
        CarController.externalController = true;

        // Calling this event when AI vehicle spawned.
        RCC_Events.Event_OnRCCAISpawned(this);

    }

    /// <summary>
    /// Unity's Update method. Checks vehicle's controllability, updates speed limit, manages target updates, and checks for targets and brake zones.
    /// </summary>
    private void Update() {

        // If not controllable, no need to go further.
        if (!CarController.canControl)
            return;

        //  If limit speed is not enabled, maximum speed is same with vehicle's maximum speed.
        if (!limitSpeed)
            maximumSpeed = CarController.maxspeed;

        // Assigning navigator's position to front wheels of the vehicle
        if (!CarController.FrontLeftWheelCollider)
            return;
        navigator.transform.localPosition = Vector3.zero;
        navigator.transform.localPosition += Vector3.forward * CarController.FrontLeftWheelCollider.transform.localPosition.z;
        navigator.updatePosition = true;
        navigator.Move((Vector3.forward * 1f));
        //navigator.(transform.position); 

        CheckTargets();     //  Checking targets if navigation mode is set to chase or follow target mode.
        CheckBrakeZones();  //  Checking existing brake zones in the scene.

        if (!updateTargets)
            lastUpdatedTargets += Time.deltaTime;

        if (lastUpdatedTargets >= 1f)
            updateTargets = true;

    }

    /// <summary>
    /// Unity's FixedUpdate method. Handles AI navigation, obstacle avoidance using raycasts, and feeds inputs to RCC.
    /// </summary>
    private void FixedUpdate() {

        // If not controllable, no need to go further.
        if (!CarController.canControl)
            return;

        //  If enabled, raycasts will be used to avoid obstacles at runtime.
        if (useRaycasts)
            FixedRaycasts();        // Recalculates steerInput if one of raycasts detects an object front of AI vehicle.

        Navigation();       // Calculates steerInput based on navigator.
        CheckReset();       // Used for deciding go back or not after crashing.
        FeedRCC();          // Feeds inputs of the RCC.

    }

    /// <summary>
    /// Calculates the required steering, throttle, and brake inputs using the chosen NavigationMode.
    /// </summary>
    private void Navigation() {

        // Navigator Input is multiplied by 1.5f for fast reactions.
        float navigatorInput = Mathf.Clamp(transform.InverseTransformDirection(navigator.desiredVelocity).x * 1f, -1f, 1f);

        if (navigatorInput > .4f)
            navigatorInput = 1f;

        if (navigatorInput < -.4f)
            navigatorInput = -1f;

        //  Navigation has three modes.
        switch (navigationMode) {

            case NavigationMode.FollowWaypoints:

                // If our scene doesn't have a Waypoint Container, stop and return with error.
                if (!waypointsContainer) {

#if UNITY_EDITOR
                    Debug.LogError("Waypoints Container Couldn't Found!");
#endif
                    Stop();
                    return;

                }

                // If our scene has Waypoints Container and it doesn't have any waypoints, stop and return with error.
                if (waypointsContainer && waypointsContainer.waypoints.Count < 1) {

#if UNITY_EDITOR
                    Debug.LogError("Waypoints Container Doesn't Have Any Waypoints!");
#endif
                    Stop();
                    return;

                }

                //	If stop after lap is enabled, stop at target lap.
                if (stopAfterLap && lap >= stopLap) {

                    Stop();
                    return;

                }

                // Next waypoint and its position.
                RCC_Waypoint currentWaypoint = waypointsContainer.waypoints[currentWaypointIndex];

                // Checks for the distance to next waypoint. If it is less than written value, then pass to next waypoint.
                float distanceToNextWaypoint = Vector3.Distance(transform.position, currentWaypoint.transform.position);

                // Setting destination of the Navigator.
                if (navigator.isOnNavMesh)
                    navigator.SetDestination(waypointsContainer.waypoints[currentWaypointIndex].transform.position);

                //  If distance to the next waypoint is not 0, and close enough to the vehicle, increase index of the current waypoint and total waypoint.
                if (distanceToNextWaypoint != 0 && distanceToNextWaypoint < waypointsContainer.waypoints[currentWaypointIndex].radius) {

                    currentWaypointIndex++;
                    totalWaypointPassed++;

                    // If all waypoints were passed, sets the current waypoint to first waypoint and increase lap.
                    if (currentWaypointIndex >= waypointsContainer.waypoints.Count) {

                        currentWaypointIndex = 0;
                        lap++;

                    }

                    // Setting destination of the Navigator. 
                    if (navigator.isOnNavMesh)
                        navigator.SetDestination(waypointsContainer.waypoints[currentWaypointIndex].transform.position);

                }

                //  If vehicle goes forward, calculate throttle and brake inputs.
                if (!reversingNow) {

                    throttleInput = (distanceToNextWaypoint < (waypointsContainer.waypoints[currentWaypointIndex].radius * (CarController.speed / 30f))) ? (Mathf.Clamp01(currentWaypoint.targetSpeed - CarController.speed)) : 1f;
                    throttleInput *= Mathf.Clamp01(Mathf.Lerp(10f, 0f, (CarController.speed) / maximumSpeed));
                    brakeInput = (distanceToNextWaypoint < (waypointsContainer.waypoints[currentWaypointIndex].radius * (CarController.speed / 30f))) ? (Mathf.Clamp01(CarController.speed - currentWaypoint.targetSpeed)) : 0f;
                    handbrakeInput = 0f;

                    //  If vehicle speed is high enough, calculate them related to navigator input. This will reduce throttle input, and increase brake input on sharp turns.
                    if (CarController.speed > 30f) {

                        throttleInput -= Mathf.Abs(navigatorInput) / 3f;
                        brakeInput += Mathf.Abs(navigatorInput) / 3f;

                    }

                }

                break;

            case NavigationMode.ChaseTarget:

                // If our scene doesn't have a target to chase, stop and return.
                if (!targetChase) {

                    Stop();
                    return;

                }

                // Setting destination of the Navigator. 
                if (navigator.isOnNavMesh)
                    navigator.SetDestination(targetChase.position);

                //  If vehicle goes forward, calculate throttle and brake inputs.
                if (!reversingNow) {

                    throttleInput = 1f;
                    throttleInput *= Mathf.Clamp01(Mathf.Lerp(10f, 0f, (CarController.speed) / maximumSpeed));
                    brakeInput = 0f;
                    handbrakeInput = 0f;

                    //  If vehicle speed is high enough, calculate them related to navigator input. This will reduce throttle input, and increase brake input on sharp turns.
                    if (CarController.speed > 30f) {

                        throttleInput -= Mathf.Abs(navigatorInput) / 3f;
                        brakeInput += Mathf.Abs(navigatorInput) / 3f;

                    }

                }

                break;

            case NavigationMode.FollowTarget:

                // If our scene doesn't have a Waypoints Container, return with error.
                if (!targetChase) {

                    Stop();
                    return;

                }

                // Setting destination of the Navigator. 
                if (navigator.isOnNavMesh)
                    navigator.SetDestination(targetChase.position);

                // Checks for the distance to target. 
                float distanceToTarget = Vector3.Distance(transform.position, targetChase.position);

                //  If vehicle goes forward, calculate throttle and brake inputs.
                if (!reversingNow) {

                    throttleInput = distanceToTarget < (stopFollowDistance * Mathf.Lerp(1f, 5f, CarController.speed / 50f)) ? Mathf.Lerp(-5f, 1f, distanceToTarget / (stopFollowDistance / 1f)) : 1f;
                    throttleInput *= Mathf.Clamp01(Mathf.Lerp(10f, 0f, (CarController.speed) / maximumSpeed));
                    brakeInput = distanceToTarget < (stopFollowDistance * Mathf.Lerp(1f, 5f, CarController.speed / 50f)) ? Mathf.Lerp(5f, 0f, distanceToTarget / (stopFollowDistance / 1f)) : 0f;
                    handbrakeInput = 0f;

                    //  If vehicle speed is high enough, calculate them related to navigator input. This will reduce throttle input, and increase brake input on sharp turns.
                    if (CarController.speed > 30f) {

                        throttleInput -= Mathf.Abs(navigatorInput) / 3f;
                        brakeInput += Mathf.Abs(navigatorInput) / 3f;

                    }

                    if (throttleInput < .05f)
                        throttleInput = 0f;
                    if (brakeInput < .05f)
                        brakeInput = 0f;

                }

                break;

        }

        //  If vehicle is in brake zone, apply brake input.
        if (targetBrake) {

            //  If vehicle is in brake zone and speed of the vehicle is higher than the target speed, apply brake input.
            if (Vector3.Distance(transform.position, targetBrake.transform.position) < targetBrake.distance && CarController.speed > targetBrake.targetSpeed) {

                throttleInput = 0f;
                brakeInput = 1f;

            }

        }

        if (brakeInput > .25f)
            throttleInput = 0f;

        // Steer input.
        steerInput = (ignoreWaypointNow ? rayInput : navigatorInput + rayInput);
        steerInput = Mathf.Clamp(steerInput, -1f, 1f) * CarController.direction;

        //  Clamping inputs.
        throttleInput = Mathf.Clamp01(throttleInput);
        brakeInput = Mathf.Clamp01(brakeInput);
        handbrakeInput = Mathf.Clamp01(handbrakeInput);

        //  If vehicle goes backwards, set brake input to 1 for reversing.
        if (reversingNow) {

            throttleInput = 0f;
            brakeInput = 1f;
            handbrakeInput = 0f;

        } else {

            if (CarController.speed < 5f && brakeInput >= .5f) {

                brakeInput = 0f;
                handbrakeInput = 1f;

            }

        }

    }

    /// <summary>
    /// Vehicle will try to go backwards if crashed or stucked.
    /// </summary>
    private void CheckReset() {

        //  If navigation mode is set to follow, vehicle may stop near the target, so no need for reversing logic in that case.
        if (targetChase && navigationMode == NavigationMode.FollowTarget && Vector3.Distance(transform.position, targetChase.position) < stopFollowDistance) {

            reversingNow = false;
            resetTime = 0;
            return;

        }

        // If unable to move forward, puts the gear to R after a set time.
        if (CarController.speed <= 5 && transform.InverseTransformDirection(CarController.Rigid.linearVelocity).z <= 1f)
            resetTime += Time.deltaTime;

        //  If car is stucked for 2 seconds, reverse now.
        if (resetTime >= 2)
            reversingNow = true;

        //  If car is stucked for 4 seconds, or speed exceeds 25, go forward.
        if (resetTime >= 4 || CarController.speed >= 25) {

            reversingNow = false;
            resetTime = 0;

        }

    }

    /// <summary>
    /// Using forward and angled raycasts to detect and avoid obstacles in front of the vehicle.
    /// </summary>
    private void FixedRaycasts() {

        //  Creating five raycasts with angles.
        int[] anglesOfRaycasts = new int[5];
        anglesOfRaycasts[0] = 0;
        anglesOfRaycasts[1] = Mathf.FloorToInt(raycastAngle / 3f);
        anglesOfRaycasts[2] = Mathf.FloorToInt(raycastAngle / 1f);
        anglesOfRaycasts[3] = -Mathf.FloorToInt(raycastAngle / 1f);
        anglesOfRaycasts[4] = -Mathf.FloorToInt(raycastAngle / 3f);

        // Ray pivot position.
        Vector3 pivotPos = transform.position + transform.TransformVector(rayOrigin);

        //  Ray hit.
        RaycastHit hit;
        rayInput = 0f;
        bool casted = false;

        //  Casting rays.
        for (int i = 0; i < anglesOfRaycasts.Length; i++) {

            //  Drawing normal gizmos.
            Debug.DrawRay(pivotPos, Quaternion.AngleAxis(anglesOfRaycasts[i], transform.up) * transform.forward * raycastLength, Color.white);

            //  Casting the ray. If ray hits another obstacle...
            if (Physics.Raycast(pivotPos, Quaternion.AngleAxis(anglesOfRaycasts[i], transform.up) * transform.forward, out hit, raycastLength, obstacleLayers) && !hit.collider.isTrigger && hit.transform.root != transform) {

                switch (navigationMode) {

                    case NavigationMode.FollowWaypoints:

                        //  Drawing hit gizmos.
                        Debug.DrawRay(pivotPos, Quaternion.AngleAxis(anglesOfRaycasts[i], transform.up) * transform.forward * raycastLength, Color.red);
                        casted = true;

                        //  Setting ray input related to distance to the obstacle.
                        if (i != 0)
                            rayInput -= Mathf.Lerp(Mathf.Sign(anglesOfRaycasts[i]), 0f, (hit.distance / raycastLength));

                        break;

                    case NavigationMode.ChaseTarget:

                        if (targetChase && hit.transform != targetChase && !hit.transform.IsChildOf(targetChase)) {

                            //  Drawing hit gizmos.
                            Debug.DrawRay(pivotPos, Quaternion.AngleAxis(anglesOfRaycasts[i], transform.up) * transform.forward * raycastLength, Color.red);
                            casted = true;

                            //  Setting ray input related to distance to the obstacle.
                            if (i != 0)
                                rayInput -= Mathf.Lerp(Mathf.Sign(anglesOfRaycasts[i]), 0f, (hit.distance / raycastLength));

                        }

                        break;

                    case NavigationMode.FollowTarget:

                        //  Drawing hit gizmos.
                        Debug.DrawRay(pivotPos, Quaternion.AngleAxis(anglesOfRaycasts[i], transform.up) * transform.forward * raycastLength, Color.red);
                        casted = true;

                        //  Setting ray input related to distance to the obstacle.
                        if (i != 0)
                            rayInput -= Mathf.Lerp(Mathf.Sign(anglesOfRaycasts[i]), 0f, (hit.distance / raycastLength));

                        break;

                }

                //  If ray hits an obstacle, set obstacle. Otherwise set it to null.
                if (casted)
                    obstacle = hit.transform.gameObject;
                else
                    obstacle = null;

            }

        }

        //  Ray hits an obstacle or not?
        raycasting = casted;

        //  If so, clamp the ray input.
        rayInput = Mathf.Clamp(rayInput, -1f, 1f);

        //  If ray input is high enough, ignore the navigator input and directly use the ray input for steering.
        if (raycasting && Mathf.Abs(rayInput) > .5f)
            ignoreWaypointNow = true;
        else
            ignoreWaypointNow = false;

    }

    /// <summary>
    /// Feeds calculated throttle, brake, steer, and handbrake inputs to the RCC CarController.
    /// </summary>
    private void FeedRCC() {

        // Feeding throttleInput of the RCC.
        if (!CarController.changingGear && !CarController.cutGas)
            CarController.throttleInput = (CarController.direction == 1 ? Mathf.Clamp01(throttleInput) : Mathf.Clamp01(brakeInput));
        else
            CarController.throttleInput = 0f;

        if (!CarController.changingGear && !CarController.cutGas)
            CarController.brakeInput = (CarController.direction == 1 ? Mathf.Clamp01(brakeInput) : Mathf.Clamp01(throttleInput));
        else
            CarController.brakeInput = 0f;

        // Feeding steerInput of the RCC.
        if (smoothedSteer)
            CarController.steerInput = Mathf.Lerp(CarController.steerInput, steerInput, Time.deltaTime * 20f);
        else
            CarController.steerInput = steerInput;

        CarController.handbrakeInput = handbrakeInput;

    }

    /// <summary>
    /// Stops the vehicle immediately by releasing throttle and applying handbrake.
    /// </summary>
    private void Stop() {

        throttleInput = 0f;
        brakeInput = 0f;
        steerInput = 0f;
        handbrakeInput = 1f;

    }

    /// <summary>
    /// Checks the near targets if navigation mode is set to FollowTarget or ChaseTarget. 
    /// Populates the targetsInZone list with valid targets.
    /// </summary>
    private void CheckTargets() {

        if (!updateTargets)
            return;

        updateTargets = false;
        lastUpdatedTargets = 0f;

        Collider[] colliders = Physics.OverlapSphere(transform.position, detectorRadius);

        for (int i = 0; i < colliders.Length; i++) {

            //  If a target in the zone, add it to the list.
            if (colliders[i].transform.root.CompareTag(targetTag)) {

                if (!targetsInZone.Contains(colliders[i].transform.root))
                    targetsInZone.Add(colliders[i].transform.root);

            }

            //  If a brake zone in the zone, add it to the list.
            if (colliders[i].GetComponent<RCC_AIBrakeZone>()) {

                if (!brakeZones.Contains(colliders[i].GetComponent<RCC_AIBrakeZone>()))
                    brakeZones.Add(colliders[i].GetComponent<RCC_AIBrakeZone>());

            }

        }

        // Removing unnecessary targets in list first. If target is null or not active, remove it from the list.
        for (int i = targetsInZone.Count - 1; i >= 0; i--) {

            if (targetsInZone[i] == null) {
                targetsInZone.RemoveAt(i);
                continue;
            }

            if (!targetsInZone[i].gameObject.activeInHierarchy) {
                targetsInZone.RemoveAt(i);
                continue;
            }

            //  If distance to the target is far away, remove it from the list.
            if (Vector3.Distance(transform.position, targetsInZone[i].transform.position) > (detectorRadius * 1.1f))
                targetsInZone.RemoveAt(i);

        }

        // If there is a target in the zone, get closest enemy.
        if (targetsInZone.Count > 0)
            targetChase = GetClosestEnemy(targetsInZone.ToArray());
        else
            targetChase = null;

    }

    /// <summary>
    /// Checks the brake zones in AI's detection range. 
    /// Populates or clears the brakeZones list accordingly and determines the closest brake zone.
    /// </summary>
    private void CheckBrakeZones() {

        // Removing unnecessary brake zones in list. If brake zone is null or not active, remove it from the list.
        for (int i = brakeZones.Count - 1; i >= 0; i--) {

            if (brakeZones[i] == null) {
                brakeZones.RemoveAt(i);
                continue;
            }

            if (!brakeZones[i].gameObject.activeInHierarchy) {
                brakeZones.RemoveAt(i);
                continue;
            }

            //  If distance to the brake zone is far away, remove it from the list.
            if (Vector3.Distance(transform.position, brakeZones[i].transform.position) > (detectorRadius * 1.1f))
                brakeZones.RemoveAt(i);

        }

        // If there is a brake zone, get closest one.
        if (brakeZones.Count > 0)
            targetBrake = GetClosestBrakeZone(brakeZones.ToArray());
        else
            targetBrake = null;

    }

    /// <summary>
    /// Gets the closest enemy target from a list of potential targets by comparing squared distances.
    /// </summary>
    /// <param name="enemies">Array of potential target transforms to evaluate.</param>
    /// <returns>The transform of the nearest target, or null if the array is empty.</returns>
    private Transform GetClosestEnemy(Transform[] enemies) {

        Transform bestTarget = null;

        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (Transform potentialTarget in enemies) {

            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr) {

                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;

            }

        }

        return bestTarget;

    }

    /// <summary>
    /// Gets the closest brake zone from a list by comparing squared distances.
    /// </summary>
    /// <param name="enemies">Array of brake zones to evaluate.</param>
    /// <returns>The nearest <see cref="RCC_AIBrakeZone"/>, or null if the array is empty.</returns>
    private RCC_AIBrakeZone GetClosestBrakeZone(RCC_AIBrakeZone[] enemies) {

        RCC_AIBrakeZone bestTarget = null;

        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (RCC_AIBrakeZone potentialTarget in enemies) {

            Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr) {

                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;

            }

        }

        return bestTarget;

    }

    /// <summary>
    /// Called when the object becomes disabled or inactive. Disables external controller and fires OnRCCAIDestroyed event.
    /// </summary>
    private void OnDisable() {

        //  Disabling external controller of the vehicle on disable.
        if (CarController)
            CarController.externalController = false;

        // Calling this event when AI vehicle is destroyed.
        RCC_Events.Event_OnRCCAIDestroyed(this);

    }

}
