namespace BowlingLogic;

/// <summary>
/// Convenience extension methods for <see cref="ScoreCard"/>
/// </summary>
public static class ScoreCardExtensions
{
    public static bool IsGameComplete(this ScoreCard card) =>
        card.Frames.All(f => f.IsComplete);

    /// <summary>
    /// Returns the first incomplete frame, or null if the game is complete.
    /// </summary>
    public static ScoreFrame? GetCurrentFrame(this ScoreCard card) =>
        card.Frames.FirstOrDefault(f => !f.IsComplete);

    /// <summary>
    /// Returns the zero-based index of the current frame, or null if the game is complete.
    /// </summary>
    public static int? GetCurrentFrameIndex(this ScoreCard card)
    {
        var current = card.GetCurrentFrame();
        return current is not null ? card.Frames.IndexOf(current) : null;
    }
}
