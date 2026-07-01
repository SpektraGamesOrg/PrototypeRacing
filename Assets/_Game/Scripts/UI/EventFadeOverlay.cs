using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UIManager;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Full-screen black scrim used for the short fade in/out around an event's teleport and tear-down
    /// (GDD "On START, a short fade animation plays"). It lives inside the existing UI system (an
    /// <see cref="OverlayBase"/> under the GameUIManager canvas - not a parallel canvas), but drives its own
    /// <see cref="UIViewBase.CanvasGroup"/> alpha directly instead of the show/hide animation, so a caller can
    /// await a real timed fade. All timing is unscaled so it works even if the game is paused.
    ///
    /// Resolved by <see cref="Events.EventManager"/> via GameUIManager.GetOverlayUI; place it last in the
    /// overlay hierarchy so it renders on top of everything else while black.
    /// </summary>
    public sealed class EventFadeOverlay : OverlayBase
    {
        /// <summary>Fades to fully opaque black over <paramref name="duration"/> seconds (unscaled).</summary>
        public UniTask ToBlackAsync(float duration, CancellationToken token = default) => FadeAsync(1f, duration, token);

        /// <summary>Fades back to fully transparent over <paramref name="duration"/> seconds (unscaled).</summary>
        public UniTask ToClearAsync(float duration, CancellationToken token = default) => FadeAsync(0f, duration, token);

        private async UniTask FadeAsync(float targetAlpha, float duration, CancellationToken token)
        {
            if (!CanvasGroup || !Content)
                return;

            Content.gameObject.SetActive(true);
            CanvasGroup.blocksRaycasts = targetAlpha > 0f;

            float startAlpha = CanvasGroup.alpha;

            if (duration <= 0f)
            {
                CanvasGroup.alpha = targetAlpha;
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    token.ThrowIfCancellationRequested();
                    elapsed += Time.unscaledDeltaTime;
                    CanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                CanvasGroup.alpha = targetAlpha;
            }

            // Fully transparent -> deactivate so it never eats input while idle.
            if (targetAlpha <= 0f)
                Content.gameObject.SetActive(false);
        }
    }
}
