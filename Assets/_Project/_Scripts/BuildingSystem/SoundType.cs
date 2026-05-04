/// <summary>
/// Identifies the category of UI/gameplay sound to play via <see cref="SoundFeedback"/>.
/// </summary>
public enum SoundType
{
    /// <summary>Short UI click — used for rotation feedback.</summary>
    Click,
    /// <summary>Played when a build piece is successfully placed.</summary>
    Place,
    /// <summary>Played when a build piece is successfully removed.</summary>
    Remove,
    /// <summary>Played when a placement or removal attempt is invalid.</summary>
    wrongPlacement,
}
