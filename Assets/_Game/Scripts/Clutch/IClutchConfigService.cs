using System;
using System.Collections.Generic;
using System.Threading;
using _Game.Scripts.Utils.VContainer;
using Cysharp.Threading.Tasks;

namespace Clutch
{
    /// <summary>
    /// Remote-config access for the game. Resolves Clutch flags through the cache/fallback flow
    /// (see <see cref="ClutchConfigService"/>) and exposes the resolved values to consumers. Registered
    /// as a singleton in MainLifetimeScope; resolve via ServiceLocator.
    /// </summary>
    public interface IClutchConfigService : IService
    {
        /// <summary>True once <see cref="InitializeAsync"/> has resolved a value for every flag.</summary>
        bool IsReady { get; }

        /// <summary>Raised after initialization resolves the config (once per successful init).</summary>
        event Action OnConfigUpdated;

        /// <summary>
        /// Runs the resolution flow once: fetch from Clutch, then cache-overwrite on success, or fall
        /// back to the cached value / fallback SO on failure. Safe to call once at startup.
        /// </summary>
        UniTask InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>The resolved raw JSON for a flag, or null when neither Clutch, cache, nor SO has it.</summary>
        string GetRawJson(string flagKey);

        /// <summary>
        /// The resolved flag parsed as a string-&gt;int map (the shape of VehiclePrices / AdConfig).
        /// Returns an empty dictionary when the flag is absent or unparseable.
        /// </summary>
        IReadOnlyDictionary<string, int> GetIntMap(string flagKey);

        /// <summary>
        /// Convenience over <see cref="GetIntMap"/>: the int for a key within a flag, or
        /// <paramref name="fallback"/> when missing.
        /// </summary>
        int GetInt(string flagKey, string entryKey, int fallback);
    }
}
