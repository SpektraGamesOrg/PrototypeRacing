using TMPro;
using UIManager;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Display payload for <see cref="EventHudOverlay"/>. Time Trial shows the running clock; Jump Challenge
    /// shows a fixed prompt. Built by <see cref="Events.EventManager"/> so the overlay stays type-agnostic.
    /// </summary>
    public sealed class EventHudData
    {
        public readonly bool ShowTimer;
        public readonly string Prompt;

        public EventHudData(bool showTimer, string prompt)
        {
            ShowTimer = showTimer;
            Prompt = prompt;
        }
    }

    /// <summary>
    /// The minimal in-run HUD shown while a level is being played: a countdown clock for Time Trials, or a short
    /// prompt for Jump Challenge. <see cref="Events.EventManager"/> pushes the remaining time each frame via
    /// <see cref="SetTimer"/> while the run is live (a single text write - cheap).
    /// </summary>
    public sealed class EventHudOverlay : OverlayBase
    {
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text promptText;

        protected override void OnShowed(bool immediate, object uiData = null)
        {
            base.OnShowed(immediate, uiData);

            bool showTimer = false;
            string prompt = string.Empty;
            if (uiData is EventHudData data)
            {
                showTimer = data.ShowTimer;
                prompt = data.Prompt;
            }

            if (timerText) timerText.gameObject.SetActive(showTimer);
            if (promptText)
            {
                promptText.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
                promptText.text = prompt;
            }
        }

        /// <summary>Updates the on-screen clock to the given remaining seconds, formatted m:ss.</summary>
        public void SetTimer(float secondsRemaining)
        {
            if (!timerText)
                return;

            if (secondsRemaining < 0f)
                secondsRemaining = 0f;

            int minutes = (int)(secondsRemaining / 60f);
            int seconds = (int)(secondsRemaining % 60f);
            timerText.text = $"{minutes}:{seconds:00}";
        }
    }
}
