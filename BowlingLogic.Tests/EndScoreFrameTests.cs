namespace BowlingLogic.Tests;

public class EndScoreFrameTests
{
    // ── Construction ──────────────────────────────────────────

    [Fact]
    public void Constructor_InheritsFromScoreFrame()
    {
        var frame = new EndScoreFrame();
        Assert.IsAssignableFrom<ScoreFrame>(frame);
    }

    [Fact]
    public void Constructor_DefaultPinCount_Is10()
    {
        var frame = new EndScoreFrame();
        Assert.Equal(10, frame.PinCount);
    }

    // ── Completion: Open Frame ────────────────────────────────

    [Fact]
    public void IsComplete_OpenFrame_TwoRolls_True()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        Assert.True(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_OneRoll_False()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(5);
        Assert.False(frame.IsComplete);
    }

    // ── Completion: Strike ────────────────────────────────────

    [Fact]
    public void IsComplete_Strike_TwoMoreRolls_True()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10); // strike
        frame.AddRoll(5);
        frame.AddRoll(3);
        Assert.True(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_Strike_OneMoreRoll_False()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10); // strike
        frame.AddRoll(5);
        Assert.False(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_ThreeStrikes_True()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10);
        frame.AddRoll(10);
        frame.AddRoll(10);
        Assert.True(frame.IsComplete);
    }

    // ── Completion: Spare ─────────────────────────────────────

    [Fact]
    public void IsComplete_Spare_OneMoreRoll_True()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(6);
        frame.AddRoll(4); // spare
        frame.AddRoll(7);
        Assert.True(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_Spare_NoMoreRoll_False()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(6);
        frame.AddRoll(4); // spare
        Assert.False(frame.IsComplete);
    }

    // ── Pin Reset After Strike ────────────────────────────────

    [Fact]
    public void AddRoll_StrikeThenFullPins_Allowed()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10); // strike, pins reset
        frame.AddRoll(10); // second strike, pins reset
        frame.AddRoll(10); // third strike
        Assert.Equal(30, frame.GetRawScore());
    }

    [Fact]
    public void AddRoll_StrikeThenPartialRolls_Allowed()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10); // strike, pins reset
        frame.AddRoll(3);
        frame.AddRoll(5);  // 3 + 5 = 8, valid
        Assert.Equal(18, frame.GetRawScore());
    }

    [Fact]
    public void AddRoll_StrikeThenExceedNewPins_Throws()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10); // strike, pins reset
        frame.AddRoll(7);
        Assert.Throws<ArgumentOutOfRangeException>(() => frame.AddRoll(4)); // 7 + 4 = 11 > 10
    }

    // ── Pin Reset After Spare ─────────────────────────────────

    [Fact]
    public void AddRoll_SpareThenFullPins_Allowed()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(7);  // spare, pins reset
        frame.AddRoll(10); // full 10 allowed
        Assert.Equal(20, frame.GetRawScore());
    }

    [Fact]
    public void AddRoll_SpareThenPartial_Allowed()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(4);
        frame.AddRoll(6);  // spare, pins reset
        frame.AddRoll(5);
        Assert.Equal(15, frame.GetRawScore());
    }

    // ── Validation ────────────────────────────────────────────

    [Fact]
    public void AddRoll_NegativePins_Throws()
    {
        var frame = new EndScoreFrame();
        Assert.Throws<ArgumentOutOfRangeException>(() => frame.AddRoll(-1));
    }

    [Fact]
    public void AddRoll_ExceedsPinCount_Throws()
    {
        var frame = new EndScoreFrame();
        Assert.Throws<ArgumentOutOfRangeException>(() => frame.AddRoll(11));
    }

    [Fact]
    public void AddRoll_OpenFrame_ThirdRoll_Throws()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4); // open frame, game over
        Assert.Throws<InvalidOperationException>(() => frame.AddRoll(1));
    }

    [Fact]
    public void AddRoll_AfterThreeStrikes_Throws()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10);
        frame.AddRoll(10);
        frame.AddRoll(10);
        Assert.Throws<InvalidOperationException>(() => frame.AddRoll(1));
    }

    // ── Bonus Rolls ───────────────────────────────────────────

    [Fact]
    public void GetBonusRolls_AlwaysReturns0()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10);
        Assert.Equal(0, frame.GetBonusRolls());
    }

    // ── Raw Score ─────────────────────────────────────────────

    [Fact]
    public void GetRawScore_OpenFrame_SumOfRolls()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        Assert.Equal(7, frame.GetRawScore());
    }

    [Fact]
    public void GetRawScore_SpareWithBonus_SumOfThreeRolls()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(7);
        frame.AddRoll(3);
        frame.AddRoll(8);
        Assert.Equal(18, frame.GetRawScore());
    }

    [Fact]
    public void GetRawScore_ThreeStrikes_30()
    {
        var frame = new EndScoreFrame();
        frame.AddRoll(10);
        frame.AddRoll(10);
        frame.AddRoll(10);
        Assert.Equal(30, frame.GetRawScore());
    }

    // ── Custom Pin Count ──────────────────────────────────────

    [Fact]
    public void CustomPinCount_100Pins_StrikeThenBonus()
    {
        var frame = new EndScoreFrame(100);
        frame.AddRoll(100); // strike
        frame.AddRoll(50);
        frame.AddRoll(30);
        Assert.Equal(180, frame.GetRawScore());
    }

    [Fact]
    public void CustomPinCount_100Pins_SpareBonus()
    {
        var frame = new EndScoreFrame(100);
        frame.AddRoll(60);
        frame.AddRoll(40); // spare
        frame.AddRoll(75);
        Assert.Equal(175, frame.GetRawScore());
    }
}
