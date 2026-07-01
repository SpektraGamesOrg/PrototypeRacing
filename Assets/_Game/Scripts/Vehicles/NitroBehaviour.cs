using Core;
using Save;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Vehicles
{
    /// <summary>
    /// Per-vehicle nitro boost (GDD 5.1), modelled on Rocket League's boost. A successful
    /// <see cref="TryActivate"/> consumes one saved nitro charge (<see cref="SaveManager.NitroCount"/>) and
    /// opens a boost window whose length and feel come from the shared <see cref="NitroConfig"/> asset.
    /// While the window is open, this drives the vehicle's Rigidbody directly in <see cref="FixedUpdate"/>:
    ///
    /// - FORWARD THRUST (throttle-independent): a constant acceleration along the car's nose, applied every
    ///   physics step regardless of the gas pedal - pressing only nitro accelerates the car by itself. On the
    ///   ground the thrust is projected onto the ground plane so it can never launch the car off flat ground;
    ///   in the air it follows the raw nose direction so pointing up + boosting climbs (gravity is never
    ///   cancelled - level boosting just goes fast and arcs back down, exactly like Rocket League).
    /// - AIR CONTROL (only while airborne): a closed-loop target-angular-velocity controller lets the player
    ///   rotate the car with the inputs they already have - steering yaws, gas/brake pitches. Releasing input
    ///   drives the spin back toward zero, so the car self-settles and never tumbles uncontrollably. This runs
    ///   ONLY when <c>!controller.isGrounded</c>, where RCC's own SteerHelper/TractionHelper early-return, so
    ///   we never fight RCC's rotational helpers.
    ///
    /// Why it is safe against RCC:
    /// - Forward thrust is a plain AddForce at the centre of mass (no induced torque) that simply sums with
    ///   RCC's wheel forces on the ground - both push the same body.
    /// - Air torque is gated to the airborne state, which is exactly where RCC applies no steering/traction.
    /// - We never modify RCC config (maxAngularVelocity, collision mode) and never edit third-party RCC.
    ///
    /// The body/controller references are serialized and wired at edit time by
    /// <see cref="MainVehicleBehaviour.Validate"/> - there is no runtime component lookup. A high execution
    /// order keeps our <see cref="FixedUpdate"/> running after RCC's, so <c>isGrounded</c> and the input
    /// fields are the current step's values before we add our forces.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(10000)] // After RCC_CarControllerV4 so isGrounded/inputs are fresh when we add forces.
    public class NitroBehaviour : VehicleBehaviourBase
    {
        [Title("References")]
        [Tooltip("Controller read for isGrounded and the air-control inputs (steer/throttle/brake). " +
                 "Wired by MainVehicleBehaviour.Validate.")]
        [SerializeField]
        private RCC_CarControllerV4 controller = null;

        [Tooltip("Body the boost thrust/torque is applied to. Wired by MainVehicleBehaviour.Validate.")]
        [SerializeField]
        private Rigidbody body = null;

        // Shared nitro tuning, resolved once in Awake from NitroConfig.Instance (a global
        // SingletonScriptableObject loaded from Resources). If the asset is missing we fall back to a fresh
        // NitroConfig with its authored defaults and log an error, so the boost still works. All feel
        // parameters live on this asset now, not on the component - see NitroConfig.
        private NitroConfig _config;

        // Seconds left in the current boost window. Runtime-only; surfaced for debugging via Odin.
        [ShowInInspector, ReadOnly]
        private float _timeRemaining;

        // Grounded state from the previous FixedUpdate, used to detect the air->ground (landing) edge and to
        // ramp pitch authority in after take-off.
        [ShowInInspector, ReadOnly]
        private bool _wasGrounded = true;

        // Seconds spent airborne in the current airborne stretch; drives the take-off pitch ramp.
        private float _airborneTime;

        // True while THIS behaviour owns the RCC camera's nose-follow override (TPSFreeFall forced false).
        // Surfaced read-only for debugging; runtime-only, never serialized.
        [ShowInInspector, ReadOnly]
        private bool _cameraNoseFollowOwned;

        // The TPSFreeFall value captured at the instant we took ownership, restored verbatim on release.
        private bool _cameraFreeFallCached;

        /// <summary>True while a boost window is open.</summary>
        public bool IsActive => _timeRemaining > 0f;

        /// <summary>Remaining boost time normalized to 0..1 (1 = just activated). For UI bars.</summary>
        public float NormalizedTimeRemaining
        {
            get
            {
                float duration = Config.NitroDuration;
                return duration > 0f ? Mathf.Clamp01(_timeRemaining / duration) : 0f;
            }
        }

        /// <summary>
        /// Whole seconds left in the current boost window, for the on-button countdown. Counts down
        /// 5 -> 4 -> 3 -> 2 -> 1 for a 5s window and never shows 0 while active (the label is hidden the
        /// moment the boost ends). Returns 0 only when no boost is running.
        /// </summary>
        public int SecondsRemaining => IsActive ? Mathf.Max(1, Mathf.CeilToInt(_timeRemaining)) : 0;

        // The shared nitro tuning, resolved lazily so any accessor is safe regardless of call order. Falls
        // back to a fresh NitroConfig (authored defaults) and logs once if the asset is missing from
        // Resources, so the boost keeps working instead of null-referencing.
        private NitroConfig Config
        {
            get
            {
                if (!_config)
                {
                    _config = NitroConfig.Instance;
                    if (!_config)
                    {
                        Debug.LogError("[NitroBehaviour] NitroConfig asset not found in Resources; using " +
                                       "default tuning. Create one via Assets > Create > DRIVE01 > Nitro " +
                                       "Config and place it in a Resources folder.", this);
                        _config = ScriptableObject.CreateInstance<NitroConfig>();
                    }
                }

                return _config;
            }
        }

        private void Awake()
        {
            if (!controller || !body)
            {
                Debug.LogError($"[NitroBehaviour] {name} is missing its controller/body reference; nitro " +
                               "disabled. Re-run MainVehicleBehaviour.Validate to wire it.", this);
                enabled = false;
                return;
            }

            // Resolve (and log-if-missing) the shared tuning up front.
            _ = Config;
        }

        private void Start()
        {
            if (!GameManager.Exists())
            {
                Debug.Log($"[NitroBehaviour] {name} disabled because we are not in the game scene.", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Spends one saved nitro charge and opens a fresh boost window. Returns false (and changes nothing)
        /// when a boost is already running or no charge is available - the caller (HUD) handles the
        /// out-of-nitro / rewarded-ad path. Flushes the save so a charge is never lost on a crash.
        /// </summary>
        public bool TryActivate()
        {
            if (IsActive)
                return false;

            if (SaveManager.NitroCount <= 0)
                return false;

            SaveManager.AddNitro(-1);
            SaveManager.Save();

            float duration = Config.NitroDuration;
            _timeRemaining = duration;
            Debug.Log($"[NitroBehaviour] Nitro used on '{name}' for {duration:0.#}s. Remaining charges: {SaveManager.NitroCount}.");
            return true;
        }

        // Drives the boost while the window is open. Runs after RCC's FixedUpdate (high DefaultExecutionOrder)
        // so isGrounded and the input fields are the current step's values. All struct math + a couple of
        // physics calls - zero allocations, so there is no GC while boosting.
        private void FixedUpdate()
        {
            if (!IsActive)
                return;

            _timeRemaining -= Time.fixedDeltaTime;

            bool grounded = controller.isGrounded;

            if (grounded)
                _airborneTime = 0f;
            else
                _airborneTime += Time.fixedDeltaTime;

            ApplyForwardThrust(grounded);

            if (!grounded)
                ApplyAirControl();

            // Landing edge: bleed off inherited air spin so the car settles instead of cartwheeling.
            if (grounded && !_wasGrounded)
                ApplyLandingDamp();

            _wasGrounded = grounded;

            // Make the TPS camera follow the nose while flying so the view points where the boost thrusts.
            // Runs on the boost-closing step too (we entered with _timeRemaining > 0), so the falling edge
            // fires when the window ends: shouldFollow then reads false because _timeRemaining is already 0.
            UpdateCameraNoseFollow(grounded);

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                Debug.Log($"[NitroBehaviour] Nitro boost ended on '{name}'.");
            }
        }

        // Forces RCC's TPS camera to track the vehicle's nose while airborne + boosting (RCC otherwise freezes
        // the camera's rotation in the air via TPSFreeFall), then restores its prior free-fall behaviour.
        // Writes TPSFreeFall only on a transition edge (never per-frame), caches the LIVE value at acquire so
        // it never clobbers a value another owner set (e.g. the dev kinematic mover), and restores exactly
        // that cached value. Access is via the sanctioned RCC_Camera.Instance singleton, implicit-bool guarded.
        private void UpdateCameraNoseFollow(bool grounded)
        {
            bool shouldFollow = _timeRemaining > 0f && !grounded;
            if (shouldFollow == _cameraNoseFollowOwned)
                return; // no transition -> no write

            if (shouldFollow)
            {
                // Only take ownership if the camera actually exists; otherwise retry on a later step.
                if (RCC_Camera.Instance)
                {
                    _cameraFreeFallCached = RCC_Camera.Instance.TPSFreeFall;
                    RCC_Camera.Instance.TPSFreeFall = false;
                    _cameraNoseFollowOwned = true;
                }
            }
            else
            {
                if (RCC_Camera.Instance)
                    RCC_Camera.Instance.TPSFreeFall = _cameraFreeFallCached;
                _cameraNoseFollowOwned = false;
            }
        }

        // Constant nose-direction thrust, mass-agnostic (Acceleration), applied at the centre of mass so it
        // induces no torque. Grounded: projected onto the ground plane so it can never lift the car off flat
        // ground. Airborne: raw nose direction, so pitching up + boosting climbs. Tapers off near top speed.
        private void ApplyForwardThrust(bool grounded)
        {
            Vector3 dir = grounded
                ? Vector3.ProjectOnPlane(transform.forward, Vector3.up)
                : transform.forward;

            // Degenerate only if the nose points exactly straight up while grounded; skip that step safely.
            if (dir.sqrMagnitude < 1e-6f)
                return;
            dir.Normalize();

            NitroConfig cfg = Config;
            float accel = grounded ? cfg.GroundThrustAccel : cfg.AirThrustAccel;

            // Taper the thrust to zero as forward speed approaches the cap (bounds top speed / tunnelling).
            float forwardSpeed = Vector3.Dot(body.linearVelocity, dir);
            float taper = 1f - Mathf.InverseLerp(cfg.MaxBoostSpeed - cfg.MaxSpeedFalloff, cfg.MaxBoostSpeed, forwardSpeed);
            accel *= taper;

            if (accel > 0f)
                body.AddForce(dir * accel, ForceMode.Acceleration);
        }

        // Air control: steer the Rigidbody's angular velocity DIRECTLY toward a target built from the
        // driver's inputs, in world space, instead of applying torque.
        //
        // Why direct angular-velocity control (not AddRelativeTorque):
        // - AddTorque/AddRelativeTorque integrate through the Rigidbody's inertia tensor, whose principal
        //   axes are slightly rotated from the body axes (inertiaTensorRotation ~= 3.5deg on this car) and
        //   whose magnitudes are very anisotropic (roll responds ~4x pitch/yaw). Commanding "local yaw" then
        //   leaks into pitch/roll and couples the axes, which reads as the car spinning/glitching on the
        //   wrong axis when the player gives input mid-aerial.
        // - Setting angularVelocity directly is inertia-tensor-independent: pure yaw stays pure yaw at any
        //   attitude, so aerial rotation is crisp and predictable (the arcade/Rocket-League feel we want).
        // Releasing input sets the target to zero, so the car actively and smoothly de-spins - it can never
        // tumble uncontrollably. We move toward the target at a bounded rate (angularResponse) so the
        // response is snappy but not an instant snap.
        private void ApplyAirControl()
        {
            NitroConfig cfg = Config;

            float yawInput = controller.steerInput;
            float pitchInput = (controller.throttleInput - controller.brakeInput) * (cfg.InvertPitch ? -1f : 1f);
            float rollInput = cfg.EnableRoll ? controller.steerInput : 0f;

            // Ramp pitch authority in over the first moments airborne so flooring the gas off a ramp does
            // not instantly backflip the car.
            float pitchRamp = cfg.TakeoffPitchRampTime > 0f
                ? Mathf.Clamp01(_airborneTime / cfg.TakeoffPitchRampTime)
                : 1f;
            pitchInput *= pitchRamp;

            // Target angular velocity in the car's local space (x = pitch, y = yaw, z = roll).
            Vector3 targetLocal = new Vector3(pitchInput * cfg.PitchRate, yawInput * cfg.YawRate, -rollInput * cfg.RollRate);

            // Clamp the target VECTOR magnitude (not each axis) so a combined pitch+yaw command stays under
            // the engine's angular-velocity cap.
            float maxAngular = cfg.MaxAirAngularSpeed;
            if (targetLocal.magnitude > maxAngular)
                targetLocal = targetLocal.normalized * maxAngular;

            // Convert the body-frame target to world space (angularVelocity is world-space) and move the
            // current spin toward it at a bounded per-step rate. This bypasses the inertia tensor entirely.
            Vector3 targetWorld = transform.TransformDirection(targetLocal);
            float maxStep = cfg.AngularResponse * Time.fixedDeltaTime; // rad/s of change allowed this step
            body.angularVelocity = Vector3.MoveTowards(body.angularVelocity, targetWorld, maxStep);
        }

        // One-shot de-spin on touchdown: a counter-torque opposing the current spin, capped so it can only
        // remove spin (never reverse it into a counter-spin) before RCC's traction/anti-roll take over.
        private void ApplyLandingDamp()
        {
            Vector3 angVel = body.angularVelocity;
            float dt = Time.fixedDeltaTime;
            if (dt <= 0f || angVel.sqrMagnitude < 1e-6f)
                return;

            // Acceleration needed to fully cancel the spin this step; never command more than that.
            float cancelAccel = angVel.magnitude / dt;
            float applied = Mathf.Min(Config.LandingDamp, cancelAccel);
            body.AddTorque(-angVel.normalized * applied, ForceMode.Acceleration);
        }

        // Clear the window and airborne state if the car is torn down / pooled away mid-boost so a reused
        // instance never starts already boosting. Also hand the camera back if we still own the nose-follow
        // override (covers despawn / pool / scene exit while airborne mid-boost).
        private void OnDisable()
        {
            if (_cameraNoseFollowOwned)
            {
                if (RCC_Camera.Instance)
                    RCC_Camera.Instance.TPSFreeFall = _cameraFreeFallCached;
                _cameraNoseFollowOwned = false;
            }

            _timeRemaining = 0f;
            _airborneTime = 0f;
            _wasGrounded = true;
        }

#if UNITY_EDITOR
        // Runs once when the component is first added, and again on any inspector/load validation.
        private void Reset()
        {
            EditorAutoWire();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            EditorAutoWire();
        }

        /// <summary>
        /// Editor-only self-wiring: resolves the serialized controller/body from this GameObject's own
        /// components. GetComponent is permitted here because this is edit-time setup code (see CLAUDE.md),
        /// exactly like <see cref="VehicleKmTracker.EditorAutoWire"/>. Also invoked by
        /// <see cref="MainVehicleBehaviour.Validate"/> right after it adds the component. Returns true if a
        /// reference changed.
        /// </summary>
        public bool EditorAutoWire()
        {
            if (Application.isPlaying)
                return false;

            bool changed = false;

            RCC_CarControllerV4 foundController = GetComponent<RCC_CarControllerV4>();
            if (controller != foundController)
            {
                controller = foundController;
                changed = true;
            }

            Rigidbody foundBody = GetComponent<Rigidbody>();
            if (body != foundBody)
            {
                body = foundBody;
                changed = true;
            }

            if (changed)
                UnityEditor.EditorUtility.SetDirty(this);

            return changed;
        }
#endif
    }
}
