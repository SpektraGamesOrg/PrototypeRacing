using System.Globalization;

namespace Utils
{
    /// <summary>
    /// Shared formatting for soft-currency (gold/coin) amounts shown to the player. Produces a compact,
    /// human-readable string: "950", "1K", "10K", "150K", "1.5M", "15M", "1.2B" - one optional decimal,
    /// dropped when the value is whole. Always <see cref="CultureInfo.InvariantCulture"/> so the decimal
    /// separator is a dot on every device, regardless of the active locale. Route ALL gold/coin text through
    /// here so the look stays consistent across the game (mirrors <see cref="DistanceFormat"/> for distance).
    /// </summary>
    public static class GoldFormat
    {
        /// <summary>
        /// Compact, human-readable amount: "950", "1K", "10K", "150K", "1.5M", "15M", "1.2B". One optional
        /// decimal, dropped when whole. Invariant culture so the decimal is always a dot.
        /// </summary>
        public static string Abbreviate(int value)
        {
            if (value < 1_000)
                return value.ToString(CultureInfo.InvariantCulture);
            if (value < 1_000_000)
                return (value / 1_000d).ToString("0.#", CultureInfo.InvariantCulture) + "K";
            if (value < 1_000_000_000)
                return (value / 1_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            return (value / 1_000_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + "B";
        }
    }
}
