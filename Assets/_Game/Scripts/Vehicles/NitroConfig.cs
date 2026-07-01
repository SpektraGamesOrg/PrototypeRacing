using Sirenix.OdinInspector;
using SpektraGames.SpektraUtilities.Runtime;
using UnityEngine;

namespace Vehicles
{
    /// <summary>
    /// Shared, global tuning for the Rocket League-style nitro boost. Every vehicle's
    /// <see cref="NitroBehaviour"/> reads these values from a single asset, so nitro feel is tuned in one
    /// place instead of per prefab.
    ///
    /// A <see cref="SingletonScriptableObject{T}"/>, so <see cref="Instance"/> resolves via
    /// Resources.Load("NitroConfig"). The asset MUST be named "NitroConfig" and live directly inside a
    /// Resources folder (e.g. Assets/_Game/Data/Resources/NitroConfig.asset). If the asset is missing at
    /// runtime, <see cref="NitroBehaviour"/> logs an error and falls back to safe hardcoded defaults, so the
    /// game keeps working.
    ///
    /// Only the tuning lives here. The per-vehicle references (controller / Rigidbody) stay on
    /// <see cref="NitroBehaviour"/> because they are specific to each spawned car.
    /// </summary>
    [CreateAssetMenu(fileName = "NitroConfig", menuName = "DRIVE01/Nitro Config")]
    public class NitroConfig : SingletonScriptableObject<NitroConfig>
    {
        [InfoBox("Global nitro / boost tuning. Every vehicle reads THIS one asset, so a change here applies " +
                 "to the whole roster - there are no per-car values.\n\n" +
                 "The boost is a Rocket League-style thruster: it pushes the car along its nose. On the " +
                 "ground it accelerates you; in the air, point the nose up and boost to fly (gravity is " +
                 "never cancelled - you only climb when aimed above the horizon). Hover a field for details.")]
        [Title("Timing")]
        [Tooltip("How long one nitro charge boosts the car, in seconds.\n" +
                 "Example: 5 = a 5-second burst per charge; 3 = shorter, snappier bursts.\n" +
                 "Default: 5 (GDD 5.1).")]
        [SerializeField, Min(0.1f)]
        private float nitroDuration = 5f;

        [Title("Forward Thrust")]
        [InfoBox("How hard the boost pushes the car forward (m/s^2, mass-independent so it feels the same on " +
                 "every car).\n" +
                 "- Air Thrust Accel is the 'fly power': higher = climbs more easily / floatier, lower = " +
                 "harder, more skilful climb. Below gravity (9.81) you can't gain height at all.\n" +
                 "- Max Boost Speed caps top speed; keep it sane to avoid clipping through walls at speed.")]
        [Tooltip("How hard boost pushes the car forward while ON THE GROUND, in m/s^2 (mass-independent - " +
                 "feels the same on every car).\n" +
                 "Example: 12 = a strong shove (gravity is 9.81 for scale); 20 = very aggressive launch; " +
                 "6 = gentle nudge.\n" +
                 "Default: 12.")]
        [SerializeField, Min(0f)]
        private float groundThrustAccel = 12f;

        [Tooltip("How hard boost pushes the car forward while IN THE AIR, in m/s^2. This is the 'fly power' - " +
                 "you climb only when the nose is aimed above the break-even angle vs gravity (9.81).\n" +
                 "Example: 16 = climb when aimed ~38 deg+ above horizon (skilful); 22 = floatier, climbs " +
                 "easily; below 9.81 = can't gain height at all.\n" +
                 "Default: 16.")]
        [SerializeField, Min(0f)]
        private float airThrustAccel = 16f;

        [Tooltip("Top speed boost will push you to, in m/s (thrust fades out approaching it). Bounds speed and " +
                 "prevents clipping through walls.\n" +
                 "Example: 90 m/s ~= 324 km/h; 60 = ~216 km/h, tamer and safer.\n" +
                 "Default: 90.")]
        [SerializeField, Min(1f)]
        private float maxBoostSpeed = 90f;

        [Tooltip("How gently thrust eases off as you near Max Boost Speed, in m/s. Bigger = smoother ramp-down.\n" +
                 "Example: 10 = thrust fades over the last 10 m/s (from 80->90); 2 = abrupt cutoff at the cap.\n" +
                 "Default: 10.")]
        [SerializeField, Min(0.1f)]
        private float maxSpeedFalloff = 10f;

        [Title("Air Control")]
        [InfoBox("How the car rotates in the AIR (ignored on the ground). Controls use the pedals/steering " +
                 "the player already has: steering = turn (yaw), gas = nose up, brake = nose down (pitch).\n" +
                 "- Pitch / Yaw / Roll Rate = how fast it spins on each axis (rad/s). ~5.5 is Rocket-League-like.\n" +
                 "- Angular Response = snappiness: higher reaches the target spin faster (crisper), lower is " +
                 "smoother/heavier.\n" +
                 "- Keep the rates so a combined pitch+yaw stays under Max Air Angular Speed (8).\n" +
                 "- Roll is off by default (no mobile input yet); Invert Pitch swaps gas/brake if up/down feels wrong.")]
        [Tooltip("How fast the nose tips up/down in the air, in rad/s (driven by gas = up, brake = down).\n" +
                 "Example: 5.5 ~= 315 deg/sec (a full front-flip in ~1.1s); 3 = slow, heavy; 8 = very fast.\n" +
                 "Default: 5.5.")]
        [SerializeField, Min(0f)]
        private float pitchRate = 5.5f;

        [Tooltip("How fast the car turns left/right in the air, in rad/s (driven by steering).\n" +
                 "Example: 5.5 ~= 315 deg/sec; lower = wider, slower turns; higher = twitchier.\n" +
                 "Default: 5.5.")]
        [SerializeField, Min(0f)]
        private float yawRate = 5.5f;

        [Tooltip("How fast the car barrel-rolls in the air, in rad/s. Only used when Enable Roll is on.\n" +
                 "Example: 5.5 ~= 315 deg/sec. Has no effect while Enable Roll is off.\n" +
                 "Default: 5.5.")]
        [SerializeField, Min(0f)]
        private float rollRate = 5.5f;

        [Tooltip("Snappiness of air rotation: how quickly the car reaches the target spin rate, in rad/s^2.\n" +
                 "Example: 40 = reaches full spin in ~0.14s (crisp); 15 = sluggish/heavy; 80 = near-instant.\n" +
                 "Default: 40.")]
        [SerializeField, Min(0f)]
        private float angularResponse = 40f;

        [Tooltip("Hard ceiling on total spin speed in the air, in rad/s. Stops a combined pitch+yaw from " +
                 "over-spinning; keep at/below the physics engine cap (8).\n" +
                 "Example: 8 = allows a diagonal pitch+yaw (~7.8) without clipping; lower = tamer aerials.\n" +
                 "Default: 8.")]
        [SerializeField, Min(0.1f)]
        private float maxAirAngularSpeed = 8f;

        [Tooltip("Brief delay after take-off before pitch control kicks in, in seconds, so flooring the gas " +
                 "off a ramp doesn't instantly backflip the car.\n" +
                 "Example: 0.1 = 0.1s grace after leaving ground; 0 = pitch active the instant you're airborne.\n" +
                 "Default: 0.1.")]
        [SerializeField, Min(0f)]
        private float takeoffPitchRampTime = 0.1f;

        [Tooltip("Allow barrel-roll (Z-axis) air control. Off for launch because mobile has no roll input yet.\n" +
                 "Example: On = Roll Rate takes effect; Off = no rolling, only pitch + yaw.\n" +
                 "Default: Off.")]
        [SerializeField]
        private bool enableRoll = false;

        [Tooltip("Flip which pedal pitches which way in the air.\n" +
                 "Example: Off = gas tips nose UP (default); On = gas tips nose DOWN. Toggle if up/down feels wrong.\n" +
                 "Default: Off.")]
        [SerializeField]
        private bool invertPitch = false;

        [Title("Landing")]
        [InfoBox("Softens touchdown after an aerial. When the car lands, this bleeds off leftover spin so it " +
                 "settles instead of cartwheeling. Higher = snappier settle; it only ever removes spin, " +
                 "never adds it. Usually no need to touch.")]
        [Tooltip("How hard leftover air spin is killed the moment the car lands, in rad/s^2, so it settles " +
                 "instead of cartwheeling. Only ever removes spin, never adds it.\n" +
                 "Example: 8 = firm settle on touchdown; 0 = no assist (car keeps its spin); 15 = very abrupt.\n" +
                 "Default: 8.")]
        [SerializeField, Min(0f)]
        private float landingDamp = 8f;

        // ---------------------------------------------------------------------
        // Read-only accessors used by NitroBehaviour.
        // ---------------------------------------------------------------------

        public float NitroDuration => nitroDuration;
        public float GroundThrustAccel => groundThrustAccel;
        public float AirThrustAccel => airThrustAccel;
        public float MaxBoostSpeed => maxBoostSpeed;
        public float MaxSpeedFalloff => maxSpeedFalloff;
        public float PitchRate => pitchRate;
        public float YawRate => yawRate;
        public float RollRate => rollRate;
        public float AngularResponse => angularResponse;
        public float MaxAirAngularSpeed => maxAirAngularSpeed;
        public float TakeoffPitchRampTime => takeoffPitchRampTime;
        public bool EnableRoll => enableRoll;
        public bool InvertPitch => invertPitch;
        public float LandingDamp => landingDamp;
    }
}
