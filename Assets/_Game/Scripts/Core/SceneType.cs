namespace Core
{
    /// <summary>
    /// Identifies a loadable scene. Used instead of raw scene-name strings everywhere so callers
    /// can't typo a name and the set of scenes is discoverable from code.
    ///
    /// These are NOT persisted, so values may be reordered freely. The actual scene name each value
    /// maps to is configured on <see cref="CustomSceneManager"/> (and must match Build Settings).
    /// </summary>
    public enum SceneType
    {
        Starter = -1,
        
        MainMenu = 0,
        Game = 1,
    }
}
