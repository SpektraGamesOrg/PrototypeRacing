using TMPro;
using UIManager;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Display payload for <see cref="EventEntryOverlay"/>. Strings are pre-formatted by
    /// <see cref="Events.EventManager"/> so the overlay stays decoupled from the event/mode types.
    /// </summary>
    public sealed class EventEntryData
    {
        public readonly string Title;       // e.g. "JUMP CHALLENGE - LV 4"
        public readonly string ActionLabel; // "START" or "WATCH AD"

        public EventEntryData(string title, string actionLabel)
        {
            Title = title;
            ActionLabel = actionLabel;
        }
    }

    /// <summary>
    /// The small "approaching an event" pop-up (GDD "Entry"): shows the event title/level and a START (or
    /// WATCH AD) and CLOSE button. It stays up while the player is in the area; <see cref="Events.EventManager"/>
    /// hides it 3 seconds after the player leaves. The overlay is a thin view - the buttons forward straight to
    /// the manager, which owns all the flow.
    /// </summary>
    public sealed class EventEntryOverlay : OverlayBase
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private EnhancedButton startButton;
        [SerializeField] private EnhancedButton closeButton;

        [Tooltip("Label on the start button; text swaps to WATCH AD for Watch & Earn.")]
        [SerializeField] private TMP_Text startButtonLabel;

        protected override void Awake()
        {
            base.Awake();
            if (startButton) startButton.onClick.AddListener(OnStartClicked);
            if (closeButton) closeButton.onClick.AddListener(OnCloseClicked);
        }

        protected override void OnShowed(bool immediate, object uiData = null)
        {
            base.OnShowed(immediate, uiData);

            if (uiData is EventEntryData data)
            {
                if (titleText) titleText.text = data.Title;
                if (startButtonLabel) startButtonLabel.text = data.ActionLabel;
            }

            if (startButton) startButton.interactable = true;
        }

        private void OnStartClicked()
        {
            // Lock the button so a rapid double-tap can't start the flow twice; the manager owns the transition.
            if (startButton) startButton.interactable = false;
            if (Events.EventManager.Exists())
                Events.EventManager.Instance.OnEntryStartPressed();
        }

        private void OnCloseClicked()
        {
            if (Events.EventManager.Exists())
                Events.EventManager.Instance.OnEntryClosePressed();
            else
                Hide();
        }
    }
}
