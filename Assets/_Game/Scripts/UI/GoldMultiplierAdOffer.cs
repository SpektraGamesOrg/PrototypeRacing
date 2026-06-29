using System;

namespace UI
{
    /// <summary>
    /// Reward-agnostic payload handed to <see cref="ClaimGoldMultiplierWithAdsOverlay"/> (as the OnShowed
    /// uiData) describing a "watch a rewarded ad to multiply a reward" offer. The overlay only knows how to
    /// present the multiplier, run the shared close-time countdown (visualised by the loading bar) and show
    /// the ad; every reward-specific decision is supplied here by the caller (gold pickup, milestone
    /// completion, ...), so the same overlay instance serves every flow.
    ///
    /// Exactly one terminal callback fires per show:
    ///  - <see cref="OnRewarded"/> : the player watched the ad to completion -> grant the multiplied reward,
    ///  - <see cref="OnAdFailed"/> : the player pressed CLAIM but the ad could not be shown / was dismissed,
    ///  - <see cref="OnExpired"/>  : the close-time countdown ran out with no claim.
    /// <see cref="OnClaimInitiated"/> is a non-terminal hook fired the instant CLAIM is pressed (before the
    /// ad), so a coordinating view can freeze its own auto-close while the ad is in flight.
    /// All callbacks are optional.
    /// </summary>
    public sealed class GoldMultiplierAdOffer
    {
        /// <summary>Multiplier shown on the offer, e.g. 3 -> "x3". Sourced from the caller (Gold / milestone).</summary>
        public readonly int Multiplier;

        /// <summary>Seconds the offer stays up before auto-expiring; also drives the loading-bar countdown.</summary>
        public readonly float CloseSeconds;

        public readonly Action OnClaimInitiated;
        public readonly Action OnRewarded;
        public readonly Action OnAdFailed;
        public readonly Action OnExpired;

        public GoldMultiplierAdOffer(
            int multiplier,
            float closeSeconds,
            Action onRewarded,
            Action onAdFailed = null,
            Action onExpired = null,
            Action onClaimInitiated = null)
        {
            Multiplier = multiplier;
            CloseSeconds = closeSeconds;
            OnRewarded = onRewarded;
            OnAdFailed = onAdFailed;
            OnExpired = onExpired;
            OnClaimInitiated = onClaimInitiated;
        }
    }
}
