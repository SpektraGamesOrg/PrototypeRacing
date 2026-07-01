using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UIManager;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// The minimal 3-2-1 countdown shown at the start of a level (GDD "Level Start sequence"). Driven by
    /// <see cref="Events.EventManager"/>, which awaits <see cref="PlayAsync"/> before handing control back to
    /// the player. Uses unscaled time so the countdown is unaffected by any time-scale changes.
    /// </summary>
    public sealed class EventCountdownOverlay : OverlayBase
    {
        [SerializeField] private TMP_Text countdownText;

        [Tooltip("Seconds the \"GO!\" flash stays up after the count reaches zero.")]
        [SerializeField, Min(0f)] private float goFlashSeconds = 0.6f;

        /// <summary>
        /// Runs a <paramref name="fromSeconds"/>..1 countdown then a brief "GO!" flash. Awaitable; cancellation
        /// (e.g. scene tear-down) stops it mid-count.
        /// </summary>
        public async UniTask PlayAsync(int fromSeconds, CancellationToken token = default)
        {
            Show(immediate: true);

            for (int n = fromSeconds; n >= 1; n--)
            {
                if (countdownText)
                    countdownText.text = n.ToString();

                await UniTask.Delay(TimeSpan.FromSeconds(1f), DelayType.UnscaledDeltaTime, cancellationToken: token);
            }

            if (countdownText)
                countdownText.text = "GO!";

            await UniTask.Delay(TimeSpan.FromSeconds(goFlashSeconds), DelayType.UnscaledDeltaTime, cancellationToken: token);

            Hide(immediate: true);
        }
    }
}
