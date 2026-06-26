//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2025 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Centralized event hub for all RCC events. Subscribe to any event below from your game scripts.
/// All events are static — subscribe in OnEnable, unsubscribe in OnDisable.
/// </summary>
public class RCC_Events {

    #region Vehicle Lifecycle

    /// <summary>
    /// Fired when a player-controlled vehicle spawns or becomes active.
    /// </summary>
    /// <param name="RCC">The vehicle controller that was spawned.</param>
    public delegate void onRCCPlayerSpawned(RCC_CarControllerV4 RCC);

    /// <summary>
    /// Fires after a player vehicle is registered as the active player with RCC_SceneManager. Subscribe to react to spawns (e.g., camera setup, UI binding).
    /// </summary>
    public static event onRCCPlayerSpawned OnRCCPlayerSpawned;

    /// <summary>
    /// Fired when a player vehicle is destroyed or disabled.
    /// </summary>
    /// <param name="RCC">The vehicle controller that was destroyed.</param>
    public delegate void onRCCPlayerDestroyed(RCC_CarControllerV4 RCC);

    /// <summary>
    /// Fires when the active player vehicle is deregistered from RCC_SceneManager (destroyed or disabled). Useful for tearing down UI or camera bindings.
    /// </summary>
    public static event onRCCPlayerDestroyed OnRCCPlayerDestroyed;

    /// <summary>
    /// Fired upon collisions involving the active player vehicle.
    /// </summary>
    /// <param name="RCC">The vehicle controller involved in the collision.</param>
    /// <param name="collision">The collision data from Unity.</param>
    public delegate void onRCCPlayerCollision(RCC_CarControllerV4 RCC, Collision collision);

    /// <summary>
    /// Fires inside the player vehicle's OnCollisionEnter handler. Subscribe to trigger crash audio, score updates, or damage-related game logic.
    /// </summary>
    public static event onRCCPlayerCollision OnRCCPlayerCollision;

    #endregion

    #region AI Lifecycle

    /// <summary>
    /// Fired when an AI vehicle is spawned or enabled.
    /// </summary>
    /// <param name="RCCAI">The AI car controller instance that was spawned.</param>
    public delegate void onRCCAISpawned(RCC_AICarController RCCAI);

    /// <summary>
    /// Fires when an AI-controlled vehicle becomes active in the scene. Subscribe to track AI traffic, populate minimap markers, or assign AI waypoints.
    /// </summary>
    public static event onRCCAISpawned OnRCCAISpawned;

    /// <summary>
    /// Fired when an AI vehicle is disabled or destroyed.
    /// </summary>
    /// <param name="RCCAI">The AI car controller instance that was destroyed.</param>
    public delegate void onRCCAIDestroyed(RCC_AICarController RCCAI);

    /// <summary>
    /// Fires when an AI vehicle is removed from the scene. Useful for cleaning up tracking, minimap entries, or AI-related references.
    /// </summary>
    public static event onRCCAIDestroyed OnRCCAIDestroyed;

    #endregion

    #region Camera

    /// <summary>
    /// Fired when the RCC camera is spawned and enabled in the scene.
    /// </summary>
    /// <param name="BCGCamera">The camera GameObject that was spawned.</param>
    public delegate void onBCGCameraSpawned(GameObject BCGCamera);

    /// <summary>
    /// Fires after the RCC_Camera is instantiated and registered in the scene. Subscribe to attach post-processing, configure camera offsets, or apply custom follow logic.
    /// </summary>
    public static event onBCGCameraSpawned OnBCGCameraSpawned;

    #endregion

    #region Scene Management

    /// <summary>
    /// Fired whenever the active driving behavior changes (e.g., switching from arcade to realistic).
    /// </summary>
    public delegate void onBehaviorChanged();

    /// <summary>
    /// Fires when RCC_SceneManager.SetBehavior or RCC.SetBehavior switches the global driving behavior preset. Subscribe to update UI labels or apply behavior-specific tuning to custom systems.
    /// </summary>
    public static event onBehaviorChanged OnBehaviorChanged;

    /// <summary>
    /// Fired whenever the active player vehicle changes (e.g., new car spawned or user switched).
    /// </summary>
    public delegate void onVehicleChanged();

    /// <summary>
    /// Fires when RCC_SceneManager swaps the active player vehicle (e.g., car selection menu, vehicle change). Subscribe to refresh HUD bindings, telemetry, or AI targets.
    /// </summary>
    public static event onVehicleChanged OnVehicleChanged;

    #endregion

    #region Input Events

    /// <summary>
    /// Fired when the engine toggle (start/stop) is performed.
    /// </summary>
    public delegate void onStartStopEngine();

    /// <summary>
    /// Fires when the engine start/stop input action is triggered by RCC_InputManager. Subscribe for ignition audio, dashboard indicators, or custom engine state handling.
    /// </summary>
    public static event onStartStopEngine OnStartStopEngine;

    /// <summary>
    /// Fired when toggling low-beam headlights.
    /// </summary>
    public delegate void onLowBeamHeadlights();

    /// <summary>
    /// Fires when the low-beam headlight input action is triggered. Subscribe for HUD updates or to apply custom light behavior beyond RCC_Light handling.
    /// </summary>
    public static event onLowBeamHeadlights OnLowBeamHeadlights;

    /// <summary>
    /// Fired when toggling high-beam headlights.
    /// </summary>
    public delegate void onHighBeamHeadlights();

    /// <summary>
    /// Fires when the high-beam headlight input action is triggered. Subscribe for HUD updates or AI-visibility logic.
    /// </summary>
    public static event onHighBeamHeadlights OnHighBeamHeadlights;

    /// <summary>
    /// Fired when requesting a camera mode change.
    /// </summary>
    public delegate void onChangeCamera();

    /// <summary>
    /// Fires when the change-camera input action is triggered. Subscribe to cycle additional custom camera modes or update HUD overlays per view.
    /// </summary>
    public static event onChangeCamera OnChangeCamera;

    /// <summary>
    /// Fired when toggling the left indicator.
    /// </summary>
    public delegate void onIndicatorLeft();

    /// <summary>
    /// Fires when the left-indicator input action is triggered. Subscribe for HUD blinkers or driving-school style validation logic.
    /// </summary>
    public static event onIndicatorLeft OnIndicatorLeft;

    /// <summary>
    /// Fired when toggling the right indicator.
    /// </summary>
    public delegate void onIndicatorRight();

    /// <summary>
    /// Fires when the right-indicator input action is triggered. Subscribe for HUD blinkers or driving-school style validation logic.
    /// </summary>
    public static event onIndicatorRight OnIndicatorRight;

    /// <summary>
    /// Fired when toggling the hazard lights.
    /// </summary>
    public delegate void onIndicatorHazard();

    /// <summary>
    /// Fires when the hazard-lights input action is triggered. Subscribe to update HUD or trigger custom hazard logic on the vehicle.
    /// </summary>
    public static event onIndicatorHazard OnIndicatorHazard;

    /// <summary>
    /// Fired when toggling the interior lights.
    /// </summary>
    public delegate void onInteriorlights();

    /// <summary>
    /// Fires when the interior-lights input action is triggered. Subscribe for cabin ambience changes or HUD indicators.
    /// </summary>
    public static event onInteriorlights OnInteriorlights;

    /// <summary>
    /// Fired when shifting up a gear.
    /// </summary>
    public delegate void onGearShiftUp();

    /// <summary>
    /// Fires when the gear-up input action is triggered. Subscribe for shift audio, force-feedback cues, or telemetry logging.
    /// </summary>
    public static event onGearShiftUp OnGearShiftUp;

    /// <summary>
    /// Fired when shifting down a gear.
    /// </summary>
    public delegate void onGearShiftDown();

    /// <summary>
    /// Fires when the gear-down input action is triggered. Subscribe for shift audio, force-feedback cues, or telemetry logging.
    /// </summary>
    public static event onGearShiftDown OnGearShiftDown;

    /// <summary>
    /// Fired when toggling neutral gear.
    /// </summary>
    /// <param name="state">True when neutral gear is engaged, false when released.</param>
    public delegate void onNGear(bool state);

    /// <summary>
    /// Fires when the neutral-gear input action changes state (press / release). Subscribe to display the N indicator on the HUD or block throttle in custom logic.
    /// </summary>
    public static event onNGear OnNGear;

    /// <summary>
    /// Fired when toggling slow-motion gameplay.
    /// </summary>
    /// <param name="state">True to activate slow motion, false to deactivate.</param>
    public delegate void onSlowMotion(bool state);

    /// <summary>
    /// Fires when slow motion is toggled via the input system. Subscribe to scale audio pitch, animation speed, or trigger cinematic effects alongside Time.timeScale changes.
    /// </summary>
    public static event onSlowMotion OnSlowMotion;

    /// <summary>
    /// Fired when starting a recording session.
    /// </summary>
    public delegate void onRecord();

    /// <summary>
    /// Fires when the record input action is triggered, signaling that RCC_Recorder should start or stop capturing the player vehicle. Subscribe for HUD overlays or telemetry handoff.
    /// </summary>
    public static event onRecord OnRecord;

    /// <summary>
    /// Fired when replaying a previous recording.
    /// </summary>
    public delegate void onReplay();

    /// <summary>
    /// Fires when the replay input action is triggered, signaling that RCC_Recorder should play back the latest recorded clip. Subscribe for HUD overlays or to disable user controls during playback.
    /// </summary>
    public static event onReplay OnReplay;

    /// <summary>
    /// Fired when toggling the look-back camera.
    /// </summary>
    /// <param name="state">True when the look-back camera is active, false when released.</param>
    public delegate void onLookBack(bool state);

    /// <summary>
    /// Fires when the look-back camera input action changes state (press / release). Subscribe to adjust mirror logic or post-processing while the player looks behind.
    /// </summary>
    public static event onLookBack OnLookBack;

    /// <summary>
    /// Fired when detaching a trailer from the vehicle.
    /// </summary>
    public delegate void onTrailerDetach();

    /// <summary>
    /// Fires when the trailer-detach input action is triggered. Subscribe for HUD updates or to handle game-specific trailer-release logic (cargo drops, mission triggers).
    /// </summary>
    public static event onTrailerDetach OnTrailerDetach;

    #endregion

#if BCG_ENTEREXIT
    #region Enter/Exit Events

    /// <summary>
    /// Fired when a player character enters a vehicle.
    /// </summary>
    /// <param name="player">The player entering the vehicle.</param>
    /// <param name="vehicle">The vehicle being entered.</param>
    public delegate void onPlayerEnteredVehicle(RCC_EnterExitPlayer player, RCC_EnterExitVehicle vehicle);

    /// <summary>
    /// Fires when an enter/exit-enabled player character takes control of a vehicle. Subscribe to swap input mappings, update HUD, or hand off camera control.
    /// </summary>
    public static event onPlayerEnteredVehicle OnPlayerEnteredVehicle;

    /// <summary>
    /// Fired when a player character exits a vehicle.
    /// </summary>
    /// <param name="player">The player exiting the vehicle.</param>
    /// <param name="vehicle">The vehicle being exited.</param>
    public delegate void onPlayerExitedVehicle(RCC_EnterExitPlayer player, RCC_EnterExitVehicle vehicle);

    /// <summary>
    /// Fires when an enter/exit-enabled player character leaves a vehicle. Subscribe to restore on-foot controls, update HUD, or detach camera.
    /// </summary>
    public static event onPlayerExitedVehicle OnPlayerExitedVehicle;

    #endregion
#endif

    #region Invocation Methods

    //  ──────────────────────────────────────────────
    //  Vehicle Lifecycle
    //  ──────────────────────────────────────────────

    /// <summary>
    /// Calls the OnRCCPlayerSpawned event.
    /// </summary>
    /// <param name="RCC">The vehicle controller that was spawned.</param>
    public static void Event_OnRCCPlayerSpawned(RCC_CarControllerV4 RCC) {

        if (OnRCCPlayerSpawned != null)
            OnRCCPlayerSpawned(RCC);

    }

    /// <summary>
    /// Calls the OnRCCPlayerDestroyed event.
    /// </summary>
    /// <param name="RCC">The vehicle controller that was destroyed.</param>
    public static void Event_OnRCCPlayerDestroyed(RCC_CarControllerV4 RCC) {

        if (OnRCCPlayerDestroyed != null)
            OnRCCPlayerDestroyed(RCC);

    }

    /// <summary>
    /// Calls the OnRCCPlayerCollision event.
    /// </summary>
    /// <param name="RCC">The vehicle controller involved in the collision.</param>
    /// <param name="collision">The collision data from Unity.</param>
    public static void Event_OnRCCPlayerCollision(RCC_CarControllerV4 RCC, Collision collision) {

        if (OnRCCPlayerCollision != null)
            OnRCCPlayerCollision(RCC, collision);

    }

    //  ──────────────────────────────────────────────
    //  AI Lifecycle
    //  ──────────────────────────────────────────────

    /// <summary>
    /// Calls the OnRCCAISpawned event.
    /// </summary>
    /// <param name="RCCAI">The AI car controller that was spawned.</param>
    public static void Event_OnRCCAISpawned(RCC_AICarController RCCAI) {

        if (OnRCCAISpawned != null)
            OnRCCAISpawned(RCCAI);

    }

    /// <summary>
    /// Calls the OnRCCAIDestroyed event.
    /// </summary>
    /// <param name="RCCAI">The AI car controller that was destroyed.</param>
    public static void Event_OnRCCAIDestroyed(RCC_AICarController RCCAI) {

        if (OnRCCAIDestroyed != null)
            OnRCCAIDestroyed(RCCAI);

    }

    //  ──────────────────────────────────────────────
    //  Camera
    //  ──────────────────────────────────────────────

    /// <summary>
    /// Calls the OnBCGCameraSpawned event.
    /// </summary>
    /// <param name="BCGCamera">The camera GameObject that was spawned.</param>
    public static void Event_OnBCGCameraSpawned(GameObject BCGCamera) {

        if (OnBCGCameraSpawned != null)
            OnBCGCameraSpawned(BCGCamera);

    }

    //  ──────────────────────────────────────────────
    //  Scene Management
    //  ──────────────────────────────────────────────

    /// <summary>
    /// Calls the OnBehaviorChanged event.
    /// </summary>
    public static void Event_OnBehaviorChanged() {

        if (OnBehaviorChanged != null)
            OnBehaviorChanged();

    }

    /// <summary>
    /// Calls the OnVehicleChanged event.
    /// </summary>
    public static void Event_OnVehicleChanged() {

        if (OnVehicleChanged != null)
            OnVehicleChanged();

    }

    //  ──────────────────────────────────────────────
    //  Input Events
    //  ──────────────────────────────────────────────

    /// <summary>
    /// Calls the OnStartStopEngine event.
    /// </summary>
    public static void Event_OnStartStopEngine() {

        if (OnStartStopEngine != null)
            OnStartStopEngine();

    }

    /// <summary>
    /// Calls the OnLowBeamHeadlights event.
    /// </summary>
    public static void Event_OnLowBeamHeadlights() {

        if (OnLowBeamHeadlights != null)
            OnLowBeamHeadlights();

    }

    /// <summary>
    /// Calls the OnHighBeamHeadlights event.
    /// </summary>
    public static void Event_OnHighBeamHeadlights() {

        if (OnHighBeamHeadlights != null)
            OnHighBeamHeadlights();

    }

    /// <summary>
    /// Calls the OnChangeCamera event.
    /// </summary>
    public static void Event_OnChangeCamera() {

        if (OnChangeCamera != null)
            OnChangeCamera();

    }

    /// <summary>
    /// Calls the OnIndicatorLeft event.
    /// </summary>
    public static void Event_OnIndicatorLeft() {

        if (OnIndicatorLeft != null)
            OnIndicatorLeft();

    }

    /// <summary>
    /// Calls the OnIndicatorRight event.
    /// </summary>
    public static void Event_OnIndicatorRight() {

        if (OnIndicatorRight != null)
            OnIndicatorRight();

    }

    /// <summary>
    /// Calls the OnIndicatorHazard event.
    /// </summary>
    public static void Event_OnIndicatorHazard() {

        if (OnIndicatorHazard != null)
            OnIndicatorHazard();

    }

    /// <summary>
    /// Calls the OnInteriorlights event.
    /// </summary>
    public static void Event_OnInteriorlights() {

        if (OnInteriorlights != null)
            OnInteriorlights();

    }

    /// <summary>
    /// Calls the OnGearShiftUp event.
    /// </summary>
    public static void Event_OnGearShiftUp() {

        if (OnGearShiftUp != null)
            OnGearShiftUp();

    }

    /// <summary>
    /// Calls the OnGearShiftDown event.
    /// </summary>
    public static void Event_OnGearShiftDown() {

        if (OnGearShiftDown != null)
            OnGearShiftDown();

    }

    /// <summary>
    /// Calls the OnNGear event.
    /// </summary>
    /// <param name="state">True when neutral gear is engaged, false when released.</param>
    public static void Event_OnNGear(bool state) {

        if (OnNGear != null)
            OnNGear(state);

    }

    /// <summary>
    /// Calls the OnSlowMotion event.
    /// </summary>
    /// <param name="state">True to activate slow motion, false to deactivate.</param>
    public static void Event_OnSlowMotion(bool state) {

        if (OnSlowMotion != null)
            OnSlowMotion(state);

    }

    /// <summary>
    /// Calls the OnRecord event.
    /// </summary>
    public static void Event_OnRecord() {

        if (OnRecord != null)
            OnRecord();

    }

    /// <summary>
    /// Calls the OnReplay event.
    /// </summary>
    public static void Event_OnReplay() {

        if (OnReplay != null)
            OnReplay();

    }

    /// <summary>
    /// Calls the OnLookBack event.
    /// </summary>
    /// <param name="state">True when the look-back camera is active, false when released.</param>
    public static void Event_OnLookBack(bool state) {

        if (OnLookBack != null)
            OnLookBack(state);

    }

    /// <summary>
    /// Calls the OnTrailerDetach event.
    /// </summary>
    public static void Event_OnTrailerDetach() {

        if (OnTrailerDetach != null)
            OnTrailerDetach();

    }

#if BCG_ENTEREXIT
    //  ──────────────────────────────────────────────
    //  Enter/Exit Events
    //  ──────────────────────────────────────────────

    /// <summary>
    /// Calls the OnPlayerEnteredVehicle event.
    /// </summary>
    /// <param name="player">The player entering the vehicle.</param>
    /// <param name="vehicle">The vehicle being entered.</param>
    public static void Event_OnPlayerEnteredVehicle(RCC_EnterExitPlayer player, RCC_EnterExitVehicle vehicle) {

        if (OnPlayerEnteredVehicle != null)
            OnPlayerEnteredVehicle(player, vehicle);

    }

    /// <summary>
    /// Calls the OnPlayerExitedVehicle event.
    /// </summary>
    /// <param name="player">The player exiting the vehicle.</param>
    /// <param name="vehicle">The vehicle being exited.</param>
    public static void Event_OnPlayerExitedVehicle(RCC_EnterExitPlayer player, RCC_EnterExitVehicle vehicle) {

        if (OnPlayerExitedVehicle != null)
            OnPlayerExitedVehicle(player, vehicle);

    }
#endif

    #endregion

}
