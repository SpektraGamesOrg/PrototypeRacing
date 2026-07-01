namespace Events
{
    /// <summary>
    /// The three in-game event types (GDD "3. In-Game Events"). An <see cref="EventArea"/> in the world is
    /// tagged with one of these; the actual level played is resolved from the player's per-mode progress
    /// (see <see cref="EventManager"/>), not from the area itself.
    /// </summary>
    public enum EventType
    {
        JumpChallenge = 0,
        TimeTrial = 1,
        WatchAndEarn = 2,
    }
}
