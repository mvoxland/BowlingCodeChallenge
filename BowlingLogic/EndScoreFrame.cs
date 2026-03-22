namespace BowlingLogic;

/// <summary>
/// Final frame of a bowling game.
/// Inherits from ScoreFrame and customized as follows:
/// Allows up to 3 rolls if the extra is earned by getting a a strike or spare in the first two rolls.
/// No subsequent rolls are needed for calculation, this frame is self-contained for scoring.
/// </summary>
public class EndScoreFrame : ScoreFrame
{
    public EndScoreFrame(int pinCount = 10) : base(pinCount)
    {
    }

    public override int MaxRolls => CalculateMaxRolls();
    private int CalculateMaxRolls()
    {
        if (Rolls.Count < 2)
            return 3; //Max is 3 until explicitly not earned

        if (EarnedThirdRoll)
            return 3;

        return 2;
    }

    public override bool IsComplete
    {
        get
        {
            if (Rolls.Count < 2)
                return false;

            if (Rolls.Count == 2 && !EarnedThirdRoll)
                return true;

            return Rolls.Count == 3;
        }
    }
    private bool EarnedThirdRoll =>
        (Rolls.Count > 0 && Rolls[0] == PinCount)//strike first frame
        || (Rolls.Count > 1 && Rolls[0] + Rolls[1] == PinCount);//spare first two frames

    public override int GetBonusRolls() => 0; //The final frame never needs later bowls as a bonus

    /// <summary>
    /// Returns pins already knocked down in the current "set" of rolls (used for validation).
    /// In the final frame the set can reset from a strike/spare, so track from last time all pins were down.
    /// </summary>
    protected override int GetCurrentRollPinsUsed()
    {
        if (Rolls.Count == 0)
            return 0;

        int totalUsed = 0;
        foreach (var roll in Rolls)
        {
            totalUsed += roll;

            if (totalUsed == PinCount)//pins were all knocked over
            {
                totalUsed = 0;
            }
        }

        return totalUsed;
    }
}
