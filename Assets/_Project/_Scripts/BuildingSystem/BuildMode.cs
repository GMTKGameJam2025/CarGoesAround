/// <summary>
/// Represents the active mode of the building system.
/// </summary>
public enum BuildMode
{
    /// <summary>Building system is inactive.</summary>
    None,
    /// <summary>Player is placing a new build piece.</summary>
    Build,
    /// <summary>Player is removing an existing build piece.</summary>
    Remove,
}

/// <summary>
/// Fired via <see cref="EventBus"/> whenever the active <see cref="BuildMode"/> changes.
/// </summary>
public struct BuildModeChangedEvent : IGameEvent
{
    /// <summary>The new build mode that is now active.</summary>
    public BuildMode currentBuildMode;

    /// <param name="mode">The build mode that just became active.</param>
    public BuildModeChangedEvent(BuildMode mode)
    {
        currentBuildMode = mode;
    }
}
