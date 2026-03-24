namespace BowlingLogic.Tests;

public class ScoreFrameTests
{
    // ── Construction ──────────────────────────────────────────

    [Fact]
    public void Constructor_DefaultPinCount_Is10()
    {
        var frame = new ScoreFrame();
        Assert.Equal(10, frame.PinCount);
    }

    [Fact]
    public void Constructor_CustomPinCount_IsStored()
    {
        var frame = new ScoreFrame(100);
        Assert.Equal(100, frame.PinCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidPinCount_Throws(int pins)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ScoreFrame(pins));
    }

    // ── AddRoll Validation ────────────────────────────────────

    [Fact]
    public void AddRoll_NegativePins_Throws()
    {
        var frame = new ScoreFrame();
        Assert.Throws<ArgumentOutOfRangeException>(() => frame.AddRoll(-1));
    }

    [Fact]
    public void AddRoll_ExceedsPinCount_Throws()
    {
        var frame = new ScoreFrame();
        Assert.Throws<ArgumentOutOfRangeException>(() => frame.AddRoll(11));
    }

    [Fact]
    public void AddRoll_ExceedsRemainingPins_Throws()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(7);
        Assert.Throws<ArgumentOutOfRangeException>(() => frame.AddRoll(4));
    }

    [Fact]
    public void AddRoll_AfterComplete_Throws()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        Assert.Throws<InvalidOperationException>(() => frame.AddRoll(1));
    }

    [Fact]
    public void AddRoll_AfterStrike_Throws()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.Throws<InvalidOperationException>(() => frame.AddRoll(0));
    }

    // ── Strike Behavior ─────────────────────────────────────────

    [Fact]
    public void AllPinsFirstRoll_CompletesFrame()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.True(frame.IsComplete);
    }

    [Fact]
    public void AllPinsFirstRoll_CustomPinCount_CompletesFrame()
    {
        var frame = new ScoreFrame(100);
        frame.AddRoll(100);
        Assert.True(frame.IsComplete);
    }

    [Fact]
    public void PartialFirstRoll_DoesNotCompleteFrame()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(9);
        Assert.False(frame.IsComplete);
    }

    // ── Spare Behavior ────────────────────────────────────────

    [Fact]
    public void TwoRollsClearAllPins_GetBonusRolls_Returns1()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(7);
        frame.AddRoll(3);
        Assert.Equal(1, frame.GetBonusRolls());
    }

    [Fact]
    public void Strike_GetBonusRolls_Returns2_NotOne()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.Equal(2, frame.GetBonusRolls());
    }

    // ── Open Frame Behavior ───────────────────────────────────

    [Fact]
    public void OpenFrame_GetBonusRolls_Returns0()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        Assert.Equal(0, frame.GetBonusRolls());
    }

    [Fact]
    public void Spare_GetBonusRolls_ReturnsOne_NotZero()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(6);
        frame.AddRoll(4);
        Assert.Equal(1, frame.GetBonusRolls());
    }

    // ── Completion ────────────────────────────────────────────

    [Fact]
    public void IsComplete_NoRolls_False()
    {
        var frame = new ScoreFrame();
        Assert.False(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_OneRoll_False()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(5);
        Assert.False(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_TwoRolls_True()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        Assert.True(frame.IsComplete);
    }

    [Fact]
    public void IsComplete_Strike_True()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.True(frame.IsComplete);
    }

    // ── Scoring ───────────────────────────────────────────────

    [Fact]
    public void GetRawScore_SumOfRolls()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(5);
        Assert.Equal(8, frame.GetRawScore());
    }

    [Fact]
    public void GetBonusRolls_Strike_Returns2()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.Equal(2, frame.GetBonusRolls());
    }

    [Fact]
    public void GetBonusRolls_Spare_Returns1()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(6);
        frame.AddRoll(4);
        Assert.Equal(1, frame.GetBonusRolls());
    }

    [Fact]
    public void GetBonusRolls_Open_Returns0()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        Assert.Equal(0, frame.GetBonusRolls());
    }

    // ── Rolls List ────────────────────────────────────────────

    [Fact]
    public void Rolls_RecordsAllRolls()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(4);
        frame.AddRoll(5);
        Assert.Equal([4, 5], frame.Rolls);
    }

    [Fact]
    public void Rolls_Strike_SingleEntry()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.Single(frame.Rolls);
        Assert.Equal(10, frame.Rolls[0]);
    }

    // ── MaxRolls ──────────────────────────────────────────────

    [Fact]
    public void MaxRolls_IsAlways2()
    {
        var frame = new ScoreFrame();
        Assert.Equal(2, frame.MaxRolls);
        frame.AddRoll(3);
        Assert.Equal(2, frame.MaxRolls);
    }

    // ── RemoveLastRoll ──────────────────────────────────────

    [Fact]
    public void RemoveLastRoll_NoRolls_Throws()
    {
        var frame = new ScoreFrame();
        Assert.Throws<InvalidOperationException>(() => frame.RemoveLastRoll());
    }

    [Fact]
    public void RemoveLastRoll_OneRoll_RemovesIt()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(5);
        frame.RemoveLastRoll();
        Assert.Empty(frame.Rolls);
        Assert.False(frame.IsComplete);
    }

    [Fact]
    public void RemoveLastRoll_TwoRolls_RemovesSecond()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(3);
        frame.AddRoll(4);
        frame.RemoveLastRoll();
        Assert.Equal([3], frame.Rolls);
        Assert.False(frame.IsComplete);
    }

    [Fact]
    public void RemoveLastRoll_Strike_ReopensFrame()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(10);
        Assert.True(frame.IsComplete);
        frame.RemoveLastRoll();
        Assert.False(frame.IsComplete);
        Assert.Empty(frame.Rolls);
    }

    [Fact]
    public void RemoveLastRoll_Spare_ReopensFrame()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(7);
        frame.AddRoll(3);
        Assert.True(frame.IsComplete);
        frame.RemoveLastRoll();
        Assert.False(frame.IsComplete);
        Assert.Equal([7], frame.Rolls);
    }

    [Fact]
    public void RemoveLastRoll_ThenAddRoll_WorksCorrectly()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(6);
        frame.AddRoll(2);
        frame.RemoveLastRoll();
        frame.AddRoll(4); // 6 + 4 = spare
        Assert.True(frame.IsComplete);
        Assert.Equal([6, 4], frame.Rolls);
        Assert.Equal(10, frame.GetRawScore());
    }

    [Fact]
    public void RemoveLastRoll_UpdatesRawScore()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(4);
        frame.AddRoll(5);
        Assert.Equal(9, frame.GetRawScore());
        frame.RemoveLastRoll();
        Assert.Equal(4, frame.GetRawScore());
    }

    [Fact]
    public void RemoveLastRoll_CustomPinCount_WorksCorrectly()
    {
        var frame = new ScoreFrame(100);
        frame.AddRoll(100); // strike
        Assert.True(frame.IsComplete);
        frame.RemoveLastRoll();
        Assert.False(frame.IsComplete);
        Assert.Empty(frame.Rolls);
    }

    // ── Partial / Edge Scoring ────────────────────────────────

    [Fact]
    public void GetRawScore_IncompleteFrame_ReturnsPartialSum()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(7);
        Assert.Equal(7, frame.GetRawScore());
    }

    [Fact]
    public void AddRoll_GutterThenHit_Allowed()
    {
        var frame = new ScoreFrame();
        frame.AddRoll(0);
        frame.AddRoll(7);
        Assert.Equal([0, 7], frame.Rolls);
        Assert.Equal(7, frame.GetRawScore());
        Assert.True(frame.IsComplete);
    }
}
