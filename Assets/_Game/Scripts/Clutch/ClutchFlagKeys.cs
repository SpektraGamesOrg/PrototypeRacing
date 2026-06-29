namespace Clutch
{
    /// <summary>
    /// Canonical Clutch flag keys the game reads. Keep these in sync with the flag keys configured in the
    /// Clutch dashboard and with the entries in the <see cref="ClutchConfig"/> fallback asset.
    /// </summary>
    public static class ClutchFlagKeys
    {
        /// <summary>
        /// Per-vehicle obtain config, keyed by vehicle key (VehicleID enum name). Each value is an object
        /// { "value": &lt;int&gt;, "obtain_type": "&lt;flags&gt;" } where obtain_type is one or more of
        /// ByGold / ByWatchAds / DistanceMilestoneKm / Free, combined with '|' or ',' (case-insensitive).
        /// "value" is the numeric target (gold price, ad count, or km). Example:
        /// {"GTR_R35":{"value":1500,"obtain_type":"ByGold"},"M4":{"value":0,"obtain_type":"Free"}}.
        /// </summary>
        public const string VehicleConfig = "VehicleConfig";

        /// <summary>string -> int ad tuning, e.g. {"interstitial_frequency":200}.</summary>
        public const string AdConfig = "AdConfig";

        /// <summary>All keys requested from Clutch in one evaluate-batch call.</summary>
        public static readonly string[] All = { VehicleConfig, AdConfig };
    }
}
