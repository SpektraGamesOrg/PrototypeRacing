namespace Clutch
{
    /// <summary>
    /// Canonical Clutch flag keys the game reads. Keep these in sync with the flag keys configured in the
    /// Clutch dashboard and with the entries in the <see cref="ClutchConfig"/> fallback asset.
    /// </summary>
    public static class ClutchFlagKeys
    {
        /// <summary>string -> int gold price per vehicle, e.g. {"R35":200,"Supra":1000}.</summary>
        public const string VehiclePrices = "VehiclePrices";

        /// <summary>string -> int ad tuning, e.g. {"interstitial_frequency":200}.</summary>
        public const string AdConfig = "AdConfig";

        /// <summary>All keys requested from Clutch in one evaluate-batch call.</summary>
        public static readonly string[] All = { VehiclePrices, AdConfig };
    }
}
