using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// A drive-in event entry point scattered around the city (GDD "Entry"). When the player vehicle enters the
    /// trigger it asks <see cref="EventManager"/> to show the entry pop-up for this area's <see cref="EventType"/>;
    /// leaving starts the pop-up's 3-second auto-hide. The area is ONLY a trigger: it carries no level and does
    /// not position the layout - every area of a mode plays the player's current level, and that level spawns at
    /// its own authored world location (the car is teleported there and back).
    ///
    /// Detection reuses the Gold pickup convention exactly: a trigger collider + a root-tag match on "Vehicle"
    /// (allocation-free). Put a <see cref="Minimap.MinimapMarker"/> on the same prefab to show it on the minimap.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class EventArea : MonoBehaviour
    {
        [Title("Event Area")]
        [Tooltip("Which event this entry point launches. The level played is the player's current level for this " +
                 "mode (Watch & Earn has no level).")]
        [SerializeField] private EventType eventType = EventType.JumpChallenge;

        [Tooltip("Tag the colliding object's root must have to count as the player vehicle (Gold convention).")]
        [SerializeField] private string vehicleTag = "Vehicle";

        // Lifetime registry of every area, so EventManager can disable/enable them all at event start/end without
        // any scene scan. Registered on Awake / removed on OnDestroy (NOT OnEnable/OnDisable) so it survives the
        // SetActive toggling this registry is used for.
        private static readonly List<EventArea> Registry = new List<EventArea>();

        public EventType EventType => eventType;

        private void Awake()
        {
            // Entry detection needs a trigger collider (same as Gold).
            Collider col = GetComponent<Collider>();
            col.isTrigger = true;

            if (!Registry.Contains(this))
                Registry.Add(this);
        }

        private void OnDestroy()
        {
            Registry.Remove(this);
        }

        /// <summary>
        /// Enables or disables EVERY event area (all modes) via SetActive. Called by <see cref="EventManager"/> so
        /// no area (Jump / Time Trial / Watch &amp; Earn) can be triggered while an event is running. Registry-based
        /// - no scene scan. Iteration is safe because SetActive raises OnDisable (which does not touch the
        /// registry), only OnDestroy does.
        /// </summary>
        public static void SetAllActive(bool active)
        {
            for (int i = 0; i < Registry.Count; i++)
            {
                EventArea area = Registry[i];
                if (area && area.gameObject.activeSelf != active)
                    area.gameObject.SetActive(active);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other || !other.transform.root.CompareTag(vehicleTag))
                return;

            if (EventManager.Exists())
                EventManager.Instance.NotifyAreaEntered(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other || !other.transform.root.CompareTag(vehicleTag))
                return;

            if (EventManager.Exists())
                EventManager.Instance.NotifyAreaExited(this);
        }
    }
}
