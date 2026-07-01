using System.Collections.Generic;
using Pooling;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Spawns and despawns an authored <see cref="LevelData"/> layout at a world origin, GC-free. Owned by
    /// <see cref="EventManager"/>; not a MonoBehaviour.
    ///
    /// Pooling mirrors <see cref="Gold.GoldSpawner"/>: one <see cref="ComponentPool{T}"/> per prefab, reused
    /// across every event so no Instantiate/Destroy churn after the first run. Because events are covered by a
    /// fade and are infrequent, spawning happens in one pass (no per-frame spread needed for the small layouts).
    /// After a spawn the resolved <see cref="StartMarker"/>/<see cref="Finishes"/>/<see cref="Obstacles"/> are
    /// read straight off the pooled instances' <see cref="LevelObject.Role"/> - no component lookups. A level may
    /// have several Finish pieces (e.g. a multi-part gate); crossing ANY of them wins, so they are all tracked.
    /// </summary>
    public sealed class LevelRuntime
    {
        private readonly Transform _poolContainer;
        private readonly Dictionary<LevelObject, ComponentPool<LevelObject>> _pools =
            new Dictionary<LevelObject, ComponentPool<LevelObject>>();

        // Parallel lists: each spawned instance and the pool it must be returned to on despawn.
        private readonly List<LevelObject> _spawned = new List<LevelObject>();
        private readonly List<ComponentPool<LevelObject>> _spawnedPools = new List<ComponentPool<LevelObject>>();

        private readonly List<LevelObject> _obstacles = new List<LevelObject>();
        private readonly List<LevelObject> _finishes = new List<LevelObject>();

        public LevelObject StartMarker { get; private set; }
        public IReadOnlyList<LevelObject> Finishes => _finishes;
        public IReadOnlyList<LevelObject> Obstacles => _obstacles;
        public bool IsSpawned { get; private set; }

        public LevelRuntime(Transform poolContainer)
        {
            _poolContainer = poolContainer;
        }

        /// <summary>
        /// Spawns every placement of <paramref name="data"/> under <paramref name="spawnRoot"/> at its exact
        /// authored WORLD transform. <paramref name="spawnRoot"/> must sit at the world origin with identity
        /// rotation and unit scale, so world == local and the layout matches what the designer authored. Any
        /// previously spawned level is despawned first.
        /// </summary>
        public void Spawn(LevelData data, Transform spawnRoot)
        {
            Despawn();

            if (!data || spawnRoot == null)
            {
                Debug.LogError("[LevelRuntime] Spawn called with null level data or spawn root.");
                return;
            }

            IReadOnlyList<LevelPlacement> placements = data.Placements;
            for (int i = 0; i < placements.Count; i++)
            {
                LevelPlacement placement = placements[i];
                if (placement == null || !placement.Prefab)
                {
                    Debug.LogError($"[LevelRuntime] Level '{data.name}' placement {i} has no prefab.", data);
                    continue;
                }

                ComponentPool<LevelObject> pool = GetPool(placement.Prefab);

                // Placements are stored in world space; the spawn root is at origin/identity/unit-scale, so we
                // apply them directly (Pop sets world position/rotation; local scale == world scale here).
                LevelObject instance = pool.Pop(placement.WorldPosition, placement.WorldRotation, spawnRoot);
                instance.transform.localScale = placement.WorldScale;
                instance.Disarm(); // stays inert until the run actually starts

                _spawned.Add(instance);
                _spawnedPools.Add(pool);

                switch (instance.Role)
                {
                    case LevelObjectRole.Start:
                        if (!StartMarker) StartMarker = instance;
                        break;
                    case LevelObjectRole.Finish:
                        _finishes.Add(instance); // arm every finish - crossing any one wins
                        break;
                    case LevelObjectRole.Obstacle:
                        _obstacles.Add(instance);
                        break;
                }
            }

            IsSpawned = true;
        }

        /// <summary>Returns every spawned piece to its pool (deactivated, not destroyed) and clears state.</summary>
        public void Despawn()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                LevelObject instance = _spawned[i];
                if (!instance)
                    continue;

                instance.Disarm();
                _spawnedPools[i].Push(instance);
            }

            _spawned.Clear();
            _spawnedPools.Clear();
            _obstacles.Clear();
            _finishes.Clear();
            StartMarker = null;
            IsSpawned = false;
        }

        private ComponentPool<LevelObject> GetPool(LevelObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out ComponentPool<LevelObject> pool))
            {
                pool = new ComponentPool<LevelObject>(prefab, _poolContainer);
                _pools.Add(prefab, pool);
            }

            return pool;
        }
    }
}
