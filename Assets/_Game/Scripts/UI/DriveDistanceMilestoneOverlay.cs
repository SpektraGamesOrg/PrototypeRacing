using System;
using Milestones;
using TMPro;
using UIManager;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    /// <summary>
    /// In-game HUD overlay that shows the current distance milestone (the next one being driven toward) and
    /// live progress toward it. It is shown for the duration of gameplay through <see cref="OverlayBase"/>
    /// (see <c>GameSceneLoader</c> for the show and <c>GameManager.OnDestroy</c> for the hide) instead of
    /// living inside the gameplay screen.
    ///
    /// While shown it POLLS each frame in <see cref="Update"/> rather than listening to a save event, because
    /// the saved odometer only changes at commit boundaries (despawn / pause / quit), which would leave the
    /// bar frozen while driving. Instead it reads the milestone service's live, fractional distance
    /// (<see cref="DistanceMilestoneManager.LiveDistanceKm"/>), fed continuously by the active
    /// VehicleKmTracker. The fill eases toward its target via <see cref="Mathf.SmoothDamp"/> so it never
    /// snaps; the label strings are rebuilt only when the displayed whole-km / reward value actually changes,
    /// so there is no per-frame allocation.
    ///
    /// Reward granting is unaffected: this overlay is display-only. If the milestone system is unavailable
    /// (no table / no valid milestone) or anything throws, the bar (the overlay's <see cref="UIViewBase.Content"/>)
    /// is hidden while the overlay itself stays shown.
    /// </summary>
    public sealed class DriveDistanceMilestoneOverlay : OverlayBase
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private TMP_Text rewardAmountText;

        [Tooltip("Approximate seconds for the fill to ease to its target. Higher = smoother/slower. " +
                 "0 = instant (no smoothing).")]
        [SerializeField, Min(0f)] private float fillSmoothTime = 0.25f;

        // Cached last-shown values so the (allocating) label strings are only rebuilt on a real change.
        // The progress value is tracked in hundredths of a km (the 2-decimal display unit) so the string
        // rebuilds only when a visible digit changes, not every frame.
        private int _lastShownHundredths = -1;
        private int _lastShownTargetKm = -1;
        private int _lastShownReward = -1;

        // Fill-smoothing state. _snapNext makes the next paint jump straight to the target (no ease) so the
        // bar doesn't visibly sweep when the overlay (re)appears or after the bar was hidden.
        private float _fillVelocity;
        private bool _snapNext = true;

        protected override void OnShowed(bool immediate, object uiData = null)
        {
            base.OnShowed(immediate, uiData);
            // Snap the fill to its value when the overlay appears so it doesn't sweep up from a stale value.
            _snapNext = true;
        }

        private void Update()
        {
            // Only poll while the overlay is on screen. When hidden the base has already disabled the content,
            // so there is nothing to repaint and no reason to recompute the milestone every frame.
            if (!ShowingOrShown)
                return;

            Refresh();
        }

        // Re-reads the live milestone state and repaints. Hides the bar (content) on any error or when there
        // is no valid milestone to show, while the overlay itself remains shown.
        private void Refresh()
        {
            try
            {
                // Single snapshot per frame (avoids recomputing the milestone / re-reading distance twice).
                DistanceMilestoneInfo milestone = DistanceMilestoneManager.LiveCurrentMilestone;
                if (!milestone.IsValid)
                {
                    SetBarActive(false);
                    _snapNext = true;
                    return;
                }

                SetBarActive(true);

                int target = milestone.ThresholdKm;
                float liveKm = DistanceMilestoneManager.LiveDistanceKm;
                float targetFill = target > 0 ? Mathf.Clamp01(liveKm / target) : 0f;

                if (fillImage)
                {
                    if (_snapNext)
                    {
                        // First valid frame after appearing/becoming valid: jump to the value, no sweep.
                        fillImage.fillAmount = targetFill;
                        _fillVelocity = 0f;
                    }
                    else
                    {
                        // Ease toward the target so filling (and the drop when the target milestone
                        // advances) is smooth instead of stepping. unscaledDeltaTime so it animates even
                        // while the game is paused.
                        fillImage.fillAmount = Mathf.SmoothDamp(
                            fillImage.fillAmount, targetFill, ref _fillVelocity, fillSmoothTime,
                            Mathf.Infinity, Time.unscaledDeltaTime);
                    }
                }

                _snapNext = false;

                // Show the live fractional distance with 2 decimals (e.g. "0.76 / 1 KM"); clamp so it can
                // never read past the target. Cache on the rounded hundredths-of-a-km so the (allocating)
                // string is only rebuilt when a visible digit actually changes, not every frame.
                float shownKm = Mathf.Min(liveKm, target);
                int shownHundredths = Mathf.RoundToInt(shownKm * 100f);
                if (progressText && (shownHundredths != _lastShownHundredths || target != _lastShownTargetKm))
                {
                    _lastShownHundredths = shownHundredths;
                    _lastShownTargetKm = target;
                    progressText.text = $"{DistanceFormat.Km(shownHundredths / 100f)} / {DistanceFormat.KmTarget(target)} KM";
                }

                if (rewardAmountText && milestone.RewardGold != _lastShownReward)
                {
                    _lastShownReward = milestone.RewardGold;
                    rewardAmountText.text = GoldFormat.Abbreviate(milestone.RewardGold);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DriveDistanceMilestoneOverlay] Failed to refresh milestone display: {e}", this);
                SetBarActive(false);
                _snapNext = true;
            }
        }

        // Toggles the bar visuals (the overlay's content) without touching the overlay's shown/hidden state,
        // so the bar can disappear when there is no valid milestone while the overlay stays open. Re-activating
        // snaps the fill so it doesn't sweep from a stale value.
        private void SetBarActive(bool active)
        {
            if (!content)
                return;

            if (content.gameObject.activeSelf == active)
                return;

            content.gameObject.SetActive(active);
            if (active)
                _snapNext = true;
        }

    }
}
