namespace BowlingLogic;

/// <summary>
/// Standard bowling frame.
/// Can be inherited from and overrriden to allow for customization, such as the bonus roll on the final frame.
/// The standard frame:
/// Max of 2 rolls allowed, with a strike ending it early.
/// If a strike ends it early (or it's a spare after the second bowl) it needs later frames' rolls to calculate score.
/// </summary>
public class ScoreFrame
{
    public int PinCount { get; }
    public List<int> Rolls { get; } = [];

    public ScoreFrame(int pinCount = 10)
    {
        if (pinCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(pinCount), "Pin count must be greater than zero.");

        PinCount = pinCount;
    }

    /// <summary>
    /// Records a roll in this frame.
    /// </summary>
    /// <param name="pins">Pins knocked down.</param>
    public void AddRoll(int pins)
    {
        if (IsComplete)
            throw new InvalidOperationException("This frame is already complete.");

        if (pins < 0)
            throw new ArgumentOutOfRangeException(nameof(pins), "Cannot knock down a negative number of pins.");

        int remaining = PinCount - GetCurrentRollPinsUsed();

        if (pins > remaining)
            throw new ArgumentOutOfRangeException(nameof(pins),
                $"Cannot knock down {pins} pins. Only {remaining} pins remaining.");

        Rolls.Add(pins);
    }

    /// <summary>
    /// Returns the sum of pins knocked down in this frame (no bonuses).
    /// </summary>
    public int GetRawScore() => Rolls.Sum();


    // Below this, everything is virtual so it can be overridden by children (like the final frame)


    public virtual int MaxRolls => 2;
    public virtual bool IsComplete =>
        Rolls.Count >= MaxRolls || Rolls.FirstOrDefault() == PinCount;

    /// <summary>
    /// Returns the number of later rolls needed to add as a bonus when calculating the score of this frame.
    /// </summary>
    public virtual int GetBonusRolls()
    {
        if (Rolls.FirstOrDefault() == PinCount) return 2;
        if (Rolls.Count == 2 && Rolls[0] + Rolls[1] == PinCount) return 1;
        return 0;
    }

    /// <summary>
    /// Returns pins already knocked down in the current "set" of rolls (used for validation).
    /// For standard frame this is just sum of rolls, since there is no reset mid-frame.
    /// </summary>
    protected virtual int GetCurrentRollPinsUsed() => Rolls.Sum();
}
