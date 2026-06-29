using System.Globalization;

namespace Utils
{
    /// <summary>
    /// Shared formatting for distance values shown to the player as kilometres. Always uses
    /// <see cref="CultureInfo.InvariantCulture"/> so the decimal separator is a dot and the thousands
    /// separator a comma on every device, regardless of the active locale (e.g. "0.76", "1,234.00").
    /// Route ALL km/distance text through here so the precision stays consistent across the game.
    /// </summary>
    public static class DistanceFormat
    {
        /// <summary>
        /// A current / live distance with two decimals and thousands grouping, e.g. "0.76", "1,234.00".
        /// Use for the value the player is progressing (the numerator in "0.76 / 1 KM").
        /// </summary>
        public static string Km(float km) => km.ToString("N2", CultureInfo.InvariantCulture);

        /// <summary>
        /// A whole-km target / threshold with thousands grouping, e.g. "1", "1,234". Targets are integers,
        /// so they carry no decimals (the denominator in "0.76 / 1 KM").
        /// </summary>
        public static string KmTarget(int km) => km.ToString("N0", CultureInfo.InvariantCulture);
    }
}
