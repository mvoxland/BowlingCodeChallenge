namespace BowlingLogic.Tests;

public class ScoreCardTests
{
    // ── Helper ────────────────────────────────────────────────

    /// <summary>
    /// Rolls multiple values in sequence.
    /// </summary>
    private static void RollMany(ScoreCard card, params int[] rolls)
    {
        foreach (var pins in rolls)
            card.Roll(pins);
    }

    /// <summary>
    /// Rolls the same value repeatedly.
    /// </summary>
    private static void RollRepeated(ScoreCard card, int pins, int count)
    {
        for (int i = 0; i < count; i++)
            card.Roll(pins);
    }

    // ── Construction ──────────────────────────────────────────

    [Fact]
    public void Constructor_Default_Creates10Frames()
    {
        var card = new ScoreCard();
        Assert.Equal(10, card.TotalFrames);
        Assert.Equal(10, card.PinCount);
        Assert.Equal(10, card.Frames.Count);
    }

    [Fact]
    public void Constructor_LastFrameIsEndScoreFrame()
    {
        var card = new ScoreCard();
        Assert.IsType<EndScoreFrame>(card.Frames[^1]);
        for (int i = 0; i < 9; i++)
            Assert.IsNotType<EndScoreFrame>(card.Frames[i]);
    }

    [Fact]
    public void Constructor_CustomGame_5Frames100Pins()
    {
        var card = new ScoreCard(totalFrames: 5, pinCount: 100);
        Assert.Equal(5, card.TotalFrames);
        Assert.Equal(100, card.PinCount);
        Assert.Equal(5, card.Frames.Count);
        Assert.IsType<EndScoreFrame>(card.Frames[^1]);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    public void Constructor_InvalidArgs_Throws(int frames, int pins)
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new ScoreCard(frames, pins));
    }

    [Fact]
    public void Constructor_SingleFrame_OnlyEndScoreFrame()
    {
        var card = new ScoreCard(totalFrames: 1);
        Assert.Single(card.Frames);
        Assert.IsType<EndScoreFrame>(card.Frames[0]);
    }

    // ── Gutter Game (all zeros) ───────────────────────────────

    [Fact]
    public void GutterGame_Score0()
    {
        var card = new ScoreCard();
        RollRepeated(card, 0, 20);
        Assert.Equal(0, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    // ── All Ones ──────────────────────────────────────────────

    [Fact]
    public void AllOnes_Score20()
    {
        var card = new ScoreCard();
        RollRepeated(card, 1, 20);
        Assert.Equal(20, card.GetScore());
    }

    // ── Perfect Game (all strikes) ────────────────────────────

    [Fact]
    public void PerfectGame_Score300()
    {
        var card = new ScoreCard();
        // 12 strikes: 9 regular frames + 3 in the 10th frame
        RollRepeated(card, 10, 12);
        Assert.Equal(300, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    // ── All Spares with 5s ────────────────────────────────────

    [Fact]
    public void AllSpares_With5s_Score150()
    {
        var card = new ScoreCard();
        // 10 frames of (5, 5) + one bonus roll of 5
        RollRepeated(card, 5, 21);
        Assert.Equal(150, card.GetScore());
    }

    // ── Single Spare ──────────────────────────────────────────

    [Fact]
    public void SingleSpare_ScoresCorrectly()
    {
        var card = new ScoreCard();
        // Frame 1: spare (5+5=10), bonus = next roll (3) -> frame score = 13
        card.Roll(5);
        card.Roll(5);
        // Frame 2: open (3+0=3) -> frame score = 3
        card.Roll(3);
        card.Roll(0);
        // Frames 3-10: all zeros
        RollRepeated(card, 0, 16);
        Assert.Equal(16, card.GetScore());
    }

    // ── Single Strike ─────────────────────────────────────────

    [Fact]
    public void SingleStrike_ScoresCorrectly()
    {
        var card = new ScoreCard();
        // Frame 1: strike (10), bonus = next 2 rolls (3+4=7) -> frame score = 17
        card.Roll(10);
        // Frame 2: open (3+4=7)
        card.Roll(3);
        card.Roll(4);
        // Frames 3-10: all zeros
        RollRepeated(card, 0, 14);
        Assert.Equal(24, card.GetScore());
    }

    // ── Two Consecutive Strikes ───────────────────────────────

    [Fact]
    public void TwoConsecutiveStrikes_ScoresCorrectly()
    {
        var card = new ScoreCard();
        // Frame 1: strike, bonus = 10 + 3 = 13 -> frame score = 23
        card.Roll(10);
        // Frame 2: strike, bonus = 3 + 4 = 7 -> frame score = 17
        card.Roll(10);
        // Frame 3: open (3 + 4 = 7)
        card.Roll(3);
        card.Roll(4);
        // Frames 4-10: all zeros
        RollRepeated(card, 0, 14);
        Assert.Equal(47, card.GetScore());
    }

    // ── 10th Frame: Strike Gets 2 More Rolls ─────────────────

    [Fact]
    public void TenthFrame_Strike_Gets2MoreRolls()
    {
        var card = new ScoreCard();
        // Frames 1-9: all zeros
        RollRepeated(card, 0, 18);
        // Frame 10: strike + 2 bonus rolls
        card.Roll(10);
        card.Roll(7);
        card.Roll(2);
        Assert.True(card.IsGameComplete());
        Assert.Equal(19, card.GetScore());
    }

    // ── 10th Frame: Spare Gets 1 More Roll ────────────────────

    [Fact]
    public void TenthFrame_Spare_Gets1MoreRoll()
    {
        var card = new ScoreCard();
        // Frames 1-9: all zeros
        RollRepeated(card, 0, 18);
        // Frame 10: spare + 1 bonus roll
        card.Roll(6);
        card.Roll(4);
        card.Roll(8);
        Assert.True(card.IsGameComplete());
        Assert.Equal(18, card.GetScore());
    }

    // ── 10th Frame: Open Ends Game ────────────────────────────

    [Fact]
    public void TenthFrame_Open_GameEnds()
    {
        var card = new ScoreCard();
        // Frames 1-9: all zeros
        RollRepeated(card, 0, 18);
        // Frame 10: open
        card.Roll(3);
        card.Roll(4);
        Assert.True(card.IsGameComplete());
        Assert.Equal(7, card.GetScore());
    }

    // ── Roll After Game Complete ──────────────────────────────

    [Fact]
    public void Roll_AfterGameComplete_Throws()
    {
        var card = new ScoreCard();
        RollRepeated(card, 0, 20);
        Assert.Throws<InvalidOperationException>(() => card.Roll(0));
    }

    // ── Frame Advancement ─────────────────────────────────────

    [Fact]
    public void GetCurrentFrameIndex_AdvancesAfterCompleteFrame()
    {
        var card = new ScoreCard();
        Assert.Equal(0, card.GetCurrentFrameIndex());
        card.Roll(3);
        Assert.Equal(0, card.GetCurrentFrameIndex());
        card.Roll(4);
        Assert.Equal(1, card.GetCurrentFrameIndex());
    }

    [Fact]
    public void GetCurrentFrameIndex_AdvancesAfterStrike()
    {
        var card = new ScoreCard();
        card.Roll(10);
        Assert.Equal(1, card.GetCurrentFrameIndex());
    }

    [Fact]
    public void GetCurrentFrameIndex_StaysOnLastFrame()
    {
        var card = new ScoreCard();
        // Roll 9 strikes (frames 1-9)
        RollRepeated(card, 10, 9);
        Assert.Equal(9, card.GetCurrentFrameIndex());
        // Still on frame 10 after first bonus roll
        card.Roll(10);
        Assert.Equal(9, card.GetCurrentFrameIndex());
    }

    [Fact]
    public void GetCurrentFrame_IsNull_WhenGameComplete()
    {
        var card = new ScoreCard();
        RollRepeated(card, 0, 20);
        Assert.Null(card.GetCurrentFrame());
    }

    [Fact]
    public void GetCurrentFrameIndex_IsNull_WhenGameComplete()
    {
        var card = new ScoreCard();
        RollRepeated(card, 0, 20);
        Assert.Null(card.GetCurrentFrameIndex());
    }

    [Fact]
    public void GetCurrentFrame_IsNotNull_DuringGame()
    {
        var card = new ScoreCard();
        Assert.NotNull(card.GetCurrentFrame());
        card.Roll(10);
        Assert.NotNull(card.GetCurrentFrame());
    }

    // ── Cumulative Score ──────────────────────────────────────

    [Fact]
    public void GetScore_ThroughFrame_PartialGame()
    {
        var card = new ScoreCard();
        card.Roll(3);
        card.Roll(4); // frame 0: 7
        card.Roll(10); // frame 1: strike, need to look ahead
        card.Roll(2);
        card.Roll(5); // frame 2: 7, frame 1 bonus = 2+5 = 7, frame 1 score = 17

        Assert.Equal(7, card.GetScore(card.Frames[0]));
        Assert.Equal(24, card.GetScore(card.Frames[1])); // 7 + 17
        Assert.Equal(31, card.GetScore(card.Frames[2])); // 7 + 17 + 7
    }

    [Fact]
    public void GetScore_ThroughFrame_IncompleteFrame_ReturnsNull()
    {
        var card = new ScoreCard();
        card.Roll(10); // strike, but no subsequent rolls yet for bonus
        Assert.Null(card.GetScore(card.Frames[0]));
    }

    // ── Mixed Game with Known Score ───────────────────────────

    [Fact]
    public void MixedGame_ScoresCorrectly()
    {
        // A realistic game with a mix of strikes, spares, and open frames.
        // Frame 1: 10 (strike)        -> 10 + 7 + 3 = 20   | cumulative: 20
        // Frame 2: 7, 3 (spare)       -> 10 + 9 = 19        | cumulative: 39
        // Frame 3: 9, 0 (open)        -> 9                   | cumulative: 48
        // Frame 4: 10 (strike)        -> 10 + 0 + 8 = 18    | cumulative: 66
        // Frame 5: 0, 8 (open)        -> 8                   | cumulative: 74
        // Frame 6: 8, 2 (spare)       -> 10 + 0 = 10         | cumulative: 84
        // Frame 7: 0, 6 (open)        -> 6                   | cumulative: 90
        // Frame 8: 10 (strike)        -> 10 + 10 + 10 = 30  | cumulative: 120
        // Frame 9: 10 (strike)        -> 10 + 10 + 8 = 28   | cumulative: 148
        // Frame 10: 10, 8, 1          -> 19                   | cumulative: 167
        var card = new ScoreCard();
        RollMany(card, 10, 7, 3, 9, 0, 10, 0, 8, 8, 2, 0, 6, 10, 10, 10, 8, 1);
        Assert.Equal(167, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    // ── Non-Standard Games ────────────────────────────────────

    [Fact]
    public void NonStandard_5Frames_PerfectGame()
    {
        // 5 frames, 10 pins: 4 regular strikes + 3 strikes in the end frame
        // Frame 1: 10 + 10 + 10 = 30
        // Frame 2: 10 + 10 + 10 = 30
        // Frame 3: 10 + 10 + 10 = 30
        // Frame 4: 10 + 10 + 10 = 30 (bonus from end frame's first two rolls)
        // Frame 5 (end): 10 + 10 + 10 = 30 (raw, no bonus)
        // Total: 150
        var card = new ScoreCard(totalFrames: 5, pinCount: 10);
        RollRepeated(card, 10, 7); // 4 regular + 3 in end frame
        Assert.Equal(150, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    [Fact]
    public void NonStandard_5Frames_AllGutters()
    {
        var card = new ScoreCard(totalFrames: 5, pinCount: 10);
        RollRepeated(card, 0, 10); // 4 frames * 2 + 1 end frame * 2
        Assert.Equal(0, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    [Fact]
    public void NonStandard_100PinBowling()
    {
        // 3 frames, 100 pins each
        var card = new ScoreCard(totalFrames: 3, pinCount: 100);
        // Frame 1: 50 + 50 = spare, bonus = next roll (100) -> 200
        card.Roll(50);
        card.Roll(50);
        // Frame 2: strike (100), bonus = next 2 rolls -> 100 + 100 + 60 = 260
        card.Roll(100);
        // Frame 3 (end): 100 + 60 + 40 = 200
        card.Roll(100);
        card.Roll(60);
        card.Roll(40);
        Assert.Equal(660, card.GetScore());
    }

    [Fact]
    public void NonStandard_SingleFrame_Game()
    {
        // Just 1 frame (the end frame)
        var card = new ScoreCard(totalFrames: 1);
        card.Roll(10);
        card.Roll(10);
        card.Roll(10);
        Assert.Equal(30, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    // ── Validation ────────────────────────────────────────────

    [Fact]
    public void Roll_NegativePins_Throws()
    {
        var card = new ScoreCard();
        Assert.Throws<ArgumentOutOfRangeException>(() => card.Roll(-1));
    }

    [Fact]
    public void Roll_ExceedsPinCount_Throws()
    {
        var card = new ScoreCard();
        Assert.Throws<ArgumentOutOfRangeException>(() => card.Roll(11));
    }

    // ── GetFrameScore Indeterminate ───────────────────────────

    [Fact]
    public void GetFrameScore_StrikeWithoutSubsequentRolls_ReturnsNull()
    {
        var card = new ScoreCard();
        card.Roll(10);
        Assert.Null(card.GetFrameScore(card.Frames[0]));
    }

    [Fact]
    public void GetFrameScore_SpareWithoutSubsequentRoll_ReturnsNull()
    {
        var card = new ScoreCard();
        card.Roll(5);
        card.Roll(5);
        Assert.Null(card.GetFrameScore(card.Frames[0]));
    }

    [Fact]
    public void GetFrameScore_OpenFrame_ReturnsScore()
    {
        var card = new ScoreCard();
        card.Roll(3);
        card.Roll(4);
        Assert.Equal(7, card.GetFrameScore(card.Frames[0]));
    }

    [Fact]
    public void GetFrameScore_IncompleteFrame_ReturnsNull()
    {
        var card = new ScoreCard();
        card.Roll(3);
        Assert.Null(card.GetFrameScore(card.Frames[0]));
    }
}
