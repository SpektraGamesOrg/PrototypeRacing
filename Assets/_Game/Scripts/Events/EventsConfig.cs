using Newtonsoft.Json;

namespace Events
{
    /// <summary>
    /// Remote tuning for the in-game events, resolved from the "EventsConfig" Clutch flag (remote, with the
    /// ClutchConfig SO fallback). Newtonsoft DTO (snake_case to match the flag JSON); never Unity-serialized.
    /// Read through <see cref="Clutch.ClutchConfigResolver"/>. Field initializers are the schema defaults used
    /// only when neither Clutch nor the SO fallback has the flag. Mirrors <see cref="Save.CurrencyConfig"/> /
    /// <see cref="global::Gold.FreeGoldConfig"/>.
    /// </summary>
    public class EventsConfig
    {
        /// <summary>Gold granted by a completed Watch &amp; Earn ad (GDD 3.3).</summary>
        [JsonProperty("watch_and_earn_gold")]
        public int WatchAndEarnGold = 5000;

        /// <summary>Rewarded-ad multiplier offered on a win (GDD: 3X).</summary>
        [JsonProperty("win_ad_multiplier")]
        public int WinAdMultiplier = 3;

        /// <summary>Fail reward = win reward / this (GDD 3.1: fail reward is 1/3 of the win reward).</summary>
        [JsonProperty("fail_reward_divider")]
        public int FailRewardDivider = 3;
    }
}
