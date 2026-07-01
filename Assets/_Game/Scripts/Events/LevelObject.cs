using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Functional role of a spawnable level piece. The level designer sets this on each spawnable prefab so
    /// the runtime knows what each object is without any component lookups:
    ///  - <see cref="Prop"/>    : purely physical/visual (ramps, ground, decoration). No trigger behaviour.
    ///  - <see cref="Start"/>   : marks where the car is placed at the start of the run (its transform pose).
    ///  - <see cref="Finish"/>  : a trigger; the run is WON when the player vehicle enters it.
    ///  - <see cref="Obstacle"/>: a trigger; in Jump Challenge, touching it FAILS the run (car jumped too low).
    /// </summary>
    public enum LevelObjectRole
    {
        Prop = 0,
        Start = 1,
        Finish = 2,
        Obstacle = 3,
    }

    /// <summary>
    /// The single component every spawnable event-level piece carries. One component (instead of a family of
    /// marker scripts) keeps the palette simple and lets the runtime read a piece's <see cref="Role"/> straight
    /// off the pooled instance it already holds - no <c>GetComponent</c> at spawn time.
    ///
    /// Finish/Obstacle pieces detect the player vehicle exactly like <see cref="Gold.Gold"/> does
    /// (root-tag match on "Vehicle", allocation-free) and raise <see cref="VehicleEntered"/> so
    /// <see cref="EventManager"/> can resolve the run outcome. Props do nothing at runtime.
    /// </summary>
    public sealed class LevelObject : MonoBehaviour
    {
        [Title("Level Object")]
        [Tooltip("What this piece does in a level. Props are inert; Start marks the car's spawn pose; Finish " +
                 "wins the run on touch; Obstacle fails a Jump Challenge on touch.")]
        [SerializeField] private LevelObjectRole role = LevelObjectRole.Prop;

        [Tooltip("Tag the colliding object's root must have to count as the player vehicle. Matches the Gold " +
                 "pickup convention.")]
        [SerializeField] private string vehicleTag = "Vehicle";

        /// <summary>
        /// Raised (Finish/Obstacle only) the first time the player vehicle enters this piece's trigger while it
        /// is armed. Instance event - <see cref="EventManager"/> subscribes to the specific spawned pieces and
        /// unsubscribes on tear-down, so there is no static state to leak across runs.
        /// </summary>
        public event Action<LevelObject> VehicleEntered;

        public LevelObjectRole Role => role;

        // Finish/Obstacle triggers are only "armed" while a run is actually in progress, so a stray touch during
        // spawn/countdown/tear-down can never resolve the run. Set by EventManager via Arm/Disarm.
        private bool _armed;

        private void Awake()
        {
            // Finish and Obstacle pieces detect the car via a trigger collider.
            if (role == LevelObjectRole.Finish || role == LevelObjectRole.Obstacle)
            {
                Collider col = GetComponent<Collider>();
                if (col)
                    col.isTrigger = true;
                else
                    Debug.LogError($"[LevelObject] '{name}' has role {role} but no Collider to use as a trigger.", this);
            }
        }

        /// <summary>Arms the trigger so the next vehicle entry resolves the run. No-op for Prop/Start.</summary>
        public void Arm() => _armed = true;

        /// <summary>Disarms the trigger (spawn, countdown, tear-down). No further <see cref="VehicleEntered"/> until re-armed.</summary>
        public void Disarm() => _armed = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!_armed)
                return;

            if (role != LevelObjectRole.Finish && role != LevelObjectRole.Obstacle)
                return;

            if (!other || !other.transform.root.CompareTag(vehicleTag))
                return;

            VehicleEntered?.Invoke(this);
        }
    }
}
