using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Events
{
    /// <summary>
    /// One placed piece in a <see cref="LevelData"/>: which spawnable prefab, and its WORLD-space transform.
    /// The Level Designer authors a level at its real world location and the runtime spawns each piece at exactly
    /// that world position/rotation/scale - so what you place in the editor is what you get in play. The
    /// <see cref="EventArea"/> is only a trigger; it does NOT offset the layout (avoids local-space surprises when
    /// an area is moved/rotated/scaled).
    ///
    /// Rotation is stored as euler angles for a clean, readable inspector; <see cref="WorldRotation"/> converts.
    /// The fields kept their old "local*" serialized names via <see cref="FormerlySerializedAsAttribute"/> so
    /// levels authored before the world-space switch migrate without data loss.
    /// </summary>
    [Serializable]
    public sealed class LevelPlacement
    {
        [SerializeField] private LevelObject prefab;

        [FormerlySerializedAs("localPosition")]
        [SerializeField] private Vector3 worldPosition;

        [FormerlySerializedAs("localEulerAngles")]
        [SerializeField] private Vector3 worldEulerAngles;

        [FormerlySerializedAs("localScale")]
        [SerializeField] private Vector3 worldScale = Vector3.one;

        public LevelObject Prefab => prefab;
        public Vector3 WorldPosition => worldPosition;
        public Quaternion WorldRotation => Quaternion.Euler(worldEulerAngles);
        public Vector3 WorldScale => worldScale;

        public LevelPlacement()
        {
        }

        public LevelPlacement(LevelObject prefab, Vector3 worldPosition, Vector3 worldEulerAngles, Vector3 worldScale)
        {
            this.prefab = prefab;
            this.worldPosition = worldPosition;
            this.worldEulerAngles = worldEulerAngles;
            this.worldScale = worldScale;
        }
    }
}
