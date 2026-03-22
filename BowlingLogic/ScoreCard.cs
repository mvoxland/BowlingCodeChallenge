namespace BowlingLogic;

/// <summary>
/// Score Card to allow for easy scoring of a bowling game.
/// Constructor allows for some non-standard games like 5-frame and 100-pin games.
/// </summary>
public class ScoreCard
{
    public int TotalFrames { get; }
    public int PinCount { get; }
    public List<ScoreFrame> Frames { get; }

    /// <summary>
    /// Create a new ScoreCard for a game. 
    /// </summary>
    /// <param name="totalFrames">Frames in this scorecard. Default is standard bowling 10.</param>
    /// <param name="pinCount">Pins available to bowl. Default is standard bowling 10.</param>
    public ScoreCard(int totalFrames = 10, int pinCount = 10)
    {
        if (totalFrames < 1)
            throw new ArgumentOutOfRangeException(nameof(totalFrames), "Games must have at least 1 frame.");
        if (pinCount < 1)
            throw new ArgumentOutOfRangeException(nameof(pinCount), "Games must have at least 1 pin.");

        TotalFrames = totalFrames;
        PinCount = pinCount;

        Frames = [];
        for (int i = 0; i < totalFrames - 1; i++)
        {
            Frames.Add(new ScoreFrame(pinCount));
        }
        Frames.Add(new EndScoreFrame(pinCount));
    }

    /// <summary>
    /// Records a roll.
    /// </summary>
    /// <param name="pins">Number of pins knocked down.</param>
    public void Roll(int pins)
    {
        var currentFrame = Frames.FirstOrDefault(f => !f.IsComplete);

        if(currentFrame is null)
        {
            throw new InvalidOperationException("The game is already complete. No more rolls allowed.");
        }

        currentFrame.AddRoll(pins);
    }

    /// <summary>
    /// Calculates a score total.
    /// If <paramref name="throughFrame"/> is provided: 
    ///     returns the score as of that frame, 
    ///     or null if the score is indeterminate as of that frame. 
    /// If <paramref name="throughFrame"/> omitted, returns the current total score.
    /// </summary>
    /// <param name="throughFrame">Frame to calculate score as of.</param>
    public int? GetScore(ScoreFrame? throughFrame = null)
    {
        if (throughFrame is not null && !Frames.Contains(throughFrame))
            throw new ArgumentException("The provided frame is not in the frames list.", nameof(throughFrame));

        int total = 0;
        foreach (var frame in Frames)
        {
            var frameScore = GetFrameScore(frame);

            if (frameScore is null)
            {
                if(throughFrame is not null)
                {
                    return null;
                }
                else
                {
                    return total;
                }
            }

            total += frameScore ?? 0;

            if (frame == throughFrame)
            {
                break;
            }
        }

        return total;
    }

    /// <summary>
    /// Returns the score for a single frame (including strike/spare bonuses),
    /// or null if the score cannot be determined without further rolls (incomplete, or needs more for bonus calculation).
    /// </summary>
    /// <param name="frame">Frame to score.</param>
    public int? GetFrameScore(ScoreFrame frame)
    {
        if (!Frames.Contains(frame))
            throw new ArgumentException("The specified frame is not in the frames list.", nameof(frame));

        if (!frame.IsComplete)
            return null;

        var score = frame.GetRawScore();

        int bonusRollsNeeded = frame.GetBonusRolls();
        if (bonusRollsNeeded > 0)
        {
            var bonusRolls = GetSubsequentRolls(frame, bonusRollsNeeded);

            if (bonusRolls.Count < bonusRollsNeeded)
                return null;

            score += bonusRolls.Sum();
        }

        return score;
    }

    /// <summary>
    /// Collects up to <paramref name="count"/> rolls (as available) from frames after the given frame, and returns each of their totals.
    /// Used for computing strike/spare bonuses.
    /// </summary>
    private List<int> GetSubsequentRolls(ScoreFrame afterFrame, int count)
    {
        return Frames
            .SkipWhile(f => f != afterFrame)
            .Skip(1)
            .SelectMany(f => f.Rolls)
            .Take(count)
            .ToList();
    }
}
