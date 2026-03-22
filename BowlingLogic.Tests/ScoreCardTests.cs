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

    // ── GetScore / GetFrameScore with Foreign Frame ───────────

    [Fact]
    public void GetScore_ThroughFrame_NotInList_Throws()
    {
        var card = new ScoreCard();
        var foreignFrame = new ScoreFrame();
        Assert.Throws<ArgumentException>(() => card.GetScore(foreignFrame));
    }

    [Fact]
    public void GetFrameScore_NotInList_Throws()
    {
        var card = new ScoreCard();
        var foreignFrame = new ScoreFrame();
        Assert.Throws<ArgumentException>(() => card.GetFrameScore(foreignFrame));
    }

    // ── Partial Running Total (no throughFrame) ───────────────

    [Fact]
    public void GetScore_NoParam_ReturnsPartialRunningTotal()
    {
        var card = new ScoreCard();
        // Frame 0: open (3+4=7) -> scorable
        card.Roll(3);
        card.Roll(4);
        // Frame 1: strike -> indeterminate (no subsequent rolls yet)
        card.Roll(10);
        // GetScore() without throughFrame should return 7 (partial total up to last scorable frame)
        Assert.Equal(7, card.GetScore());
        // But GetScore(throughFrame: Frames[1]) should return null because frame 1 is indeterminate
        Assert.Null(card.GetScore(card.Frames[1]));
    }

    // ── Scoring Interactions ──────────────────────────────────

    [Fact]
    public void StrikeFollowedBySpare_ScoresCorrectly()
    {
        var card = new ScoreCard();
        // Frame 1: strike (10), bonus = 6+4 = 10 -> frame score = 20
        card.Roll(10);
        // Frame 2: spare (6+4=10), bonus = next roll (5) -> frame score = 15
        card.Roll(6);
        card.Roll(4);
        // Frame 3: open (5+2=7)
        card.Roll(5);
        card.Roll(2);
        // Frames 4-10: all zeros
        RollRepeated(card, 0, 14);

        Assert.Equal(20, card.GetFrameScore(card.Frames[0]));
        Assert.Equal(15, card.GetFrameScore(card.Frames[1]));
        Assert.Equal(7, card.GetFrameScore(card.Frames[2]));
        Assert.Equal(42, card.GetScore());
    }

    [Fact]
    public void ThreeConsecutiveStrikes_ScoresCorrectly()
    {
        var card = new ScoreCard();
        // Frame 1: strike, bonus = 10+10 = 20 -> frame score = 30
        card.Roll(10);
        // Frame 2: strike, bonus = 10+3 = 13 -> frame score = 23
        card.Roll(10);
        // Frame 3: strike, bonus = 3+4 = 7 -> frame score = 17
        card.Roll(10);
        // Frame 4: open (3+4=7)
        card.Roll(3);
        card.Roll(4);
        // Frames 5-10: all zeros
        RollRepeated(card, 0, 12);

        Assert.Equal(30, card.GetFrameScore(card.Frames[0]));
        Assert.Equal(23, card.GetFrameScore(card.Frames[1]));
        Assert.Equal(17, card.GetFrameScore(card.Frames[2]));
        Assert.Equal(7, card.GetFrameScore(card.Frames[3]));
        Assert.Equal(77, card.GetScore());
    }

    [Fact]
    public void SpareInFrame9_BonusFromFrame10()
    {
        var card = new ScoreCard();
        // Frames 1-8: all zeros
        RollRepeated(card, 0, 16);
        // Frame 9: spare (7+3=10), bonus = next roll from frame 10 (6) -> frame score = 16
        card.Roll(7);
        card.Roll(3);
        // Frame 10: open (6+2=8)
        card.Roll(6);
        card.Roll(2);

        Assert.Equal(16, card.GetFrameScore(card.Frames[8]));
        Assert.Equal(8, card.GetFrameScore(card.Frames[9]));
        Assert.Equal(24, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    [Fact]
    public void StrikeInFrame9_BonusFromFrame10()
    {
        var card = new ScoreCard();
        // Frames 1-8: all zeros
        RollRepeated(card, 0, 16);
        // Frame 9: strike (10), bonus = next 2 rolls from frame 10 (8+1=9) -> frame score = 19
        card.Roll(10);
        // Frame 10: open (8+1=9)
        card.Roll(8);
        card.Roll(1);

        Assert.Equal(19, card.GetFrameScore(card.Frames[8]));
        Assert.Equal(9, card.GetFrameScore(card.Frames[9]));
        Assert.Equal(28, card.GetScore());
        Assert.True(card.IsGameComplete());
    }

    [Fact]
    public void TenthFrame_StrikeThenSpare()
    {
        var card = new ScoreCard();
        // Frames 1-9: all zeros
        RollRepeated(card, 0, 18);
        // Frame 10: strike, then spare (10 + 3 + 7 = 20)
        card.Roll(10);
        card.Roll(3);
        card.Roll(7);
        Assert.True(card.IsGameComplete());
        Assert.Equal(20, card.GetScore());
    }

    [Fact]
    public void TenthFrame_DoubleStrikeThenNonStrike()
    {
        var card = new ScoreCard();
        // Frames 1-9: all zeros
        RollRepeated(card, 0, 18);
        // Frame 10: strike, strike, 5 (10 + 10 + 5 = 25)
        card.Roll(10);
        card.Roll(10);
        card.Roll(5);
        Assert.True(card.IsGameComplete());
        Assert.Equal(25, card.GetScore());
    }

    // ══════════════════════════════════════════════════════════
    // Full Game Walk-Throughs with Per-Frame Assertions
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Mixed game: strikes, spares, opens, and gutters in varied positions.
    /// Verifies GetFrameScore, GetScore(throughFrame), and null for indeterminate frames along the way.
    /// 
    /// Frame  1: 8, 1       = 9    (open)          cumulative:   9
    /// Frame  2: 10         = 20   (strike: 10+6+4) cumulative:  29
    /// Frame  3: 6, 4       = 15   (spare: 10+5)   cumulative:  44
    /// Frame  4: 5, 3       = 8    (open)           cumulative:  52
    /// Frame  5: 10         = 20   (strike: 10+10+0) cumulative: 72
    /// Frame  6: 10         = 10   (strike: 10+0+0) cumulative:  82
    /// Frame  7: 0, 0       = 0    (gutter)         cumulative:  82
    /// Frame  8: 3, 7       = 20   (spare: 10+10)   cumulative: 102
    /// Frame  9: 10         = 20   (strike: 10+7+3) cumulative: 122
    /// Frame 10: 7, 3, 9    = 19   (spare+bonus)    cumulative: 141
    /// </summary>
    [Fact]
    public void FullGame_Mixed_WithPerFrameAssertions()
    {
        var card = new ScoreCard();

        // -- Frame 1: 8, 1 (open) --
        card.Roll(8);
        Assert.Null(card.GetFrameScore(card.Frames[0])); // incomplete
        card.Roll(1);
        Assert.Equal(9, card.GetFrameScore(card.Frames[0]));
        Assert.Equal(9, card.GetScore(card.Frames[0]));

        // -- Frame 2: strike --
        card.Roll(10);
        Assert.Null(card.GetFrameScore(card.Frames[1])); // strike, no subsequent rolls yet
        Assert.Null(card.GetScore(card.Frames[1]));       // indeterminate through frame 1
        Assert.Equal(9, card.GetScore());                  // partial running total = frame 0 only

        // -- Frame 3: 6, 4 (spare) --
        card.Roll(6);
        Assert.Null(card.GetFrameScore(card.Frames[1])); // strike still needs 1 more roll
        card.Roll(4);
        Assert.Equal(20, card.GetFrameScore(card.Frames[1])); // 10 + 6 + 4 = 20
        Assert.Null(card.GetFrameScore(card.Frames[2]));      // spare, needs next roll

        // -- Frame 4: 5, 3 (open) --
        card.Roll(5);
        Assert.Equal(15, card.GetFrameScore(card.Frames[2])); // 10 + 5 = 15
        card.Roll(3);
        Assert.Equal(8, card.GetFrameScore(card.Frames[3]));
        Assert.Equal(9, card.GetScore(card.Frames[0]));
        Assert.Equal(29, card.GetScore(card.Frames[1]));
        Assert.Equal(44, card.GetScore(card.Frames[2]));
        Assert.Equal(52, card.GetScore(card.Frames[3]));

        // -- Frame 5: strike --
        card.Roll(10);
        Assert.Null(card.GetFrameScore(card.Frames[4]));

        // -- Frame 6: strike --
        card.Roll(10);
        Assert.Null(card.GetFrameScore(card.Frames[4])); // still needs 1 more after frame 6's first roll
        Assert.Null(card.GetFrameScore(card.Frames[5])); // needs 2 more

        // -- Frame 7: 0, 0 (gutter frame) --
        card.Roll(0);
        Assert.Equal(20, card.GetFrameScore(card.Frames[4])); // 10 + 10 + 0 = 20
        Assert.Null(card.GetFrameScore(card.Frames[5]));      // still needs 1 more roll
        card.Roll(0);
        Assert.Equal(10, card.GetFrameScore(card.Frames[5])); // 10 + 0 + 0 = 10
        Assert.Equal(0, card.GetFrameScore(card.Frames[6]));
        Assert.Equal(72, card.GetScore(card.Frames[4]));
        Assert.Equal(82, card.GetScore(card.Frames[5]));
        Assert.Equal(82, card.GetScore(card.Frames[6]));

        // -- Frame 8: 3, 7 (spare) --
        card.Roll(3);
        card.Roll(7);
        Assert.Null(card.GetFrameScore(card.Frames[7])); // spare, needs next roll

        // -- Frame 9: strike --
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[7])); // 10 + 10 = 20
        Assert.Null(card.GetFrameScore(card.Frames[8]));       // strike, needs 2 more

        // -- Frame 10: 7, 3, 9 (spare + bonus) --
        card.Roll(7);
        Assert.Null(card.GetFrameScore(card.Frames[8])); // still needs 1 more
        card.Roll(3);
        Assert.Equal(20, card.GetFrameScore(card.Frames[8])); // 10 + 7 + 3 = 20
        Assert.Null(card.GetFrameScore(card.Frames[9]));       // spare in 10th, needs bonus
        card.Roll(9);
        Assert.Equal(19, card.GetFrameScore(card.Frames[9])); // 7 + 3 + 9 = 19

        // -- Final cumulative assertions --
        Assert.True(card.IsGameComplete());
        Assert.Equal(9, card.GetScore(card.Frames[0]));
        Assert.Equal(29, card.GetScore(card.Frames[1]));
        Assert.Equal(44, card.GetScore(card.Frames[2]));
        Assert.Equal(52, card.GetScore(card.Frames[3]));
        Assert.Equal(72, card.GetScore(card.Frames[4]));
        Assert.Equal(82, card.GetScore(card.Frames[5]));
        Assert.Equal(82, card.GetScore(card.Frames[6]));
        Assert.Equal(102, card.GetScore(card.Frames[7]));
        Assert.Equal(122, card.GetScore(card.Frames[8]));
        Assert.Equal(141, card.GetScore(card.Frames[9]));
        Assert.Equal(141, card.GetScore());
    }

    /// <summary>
    /// Streaky game: hot start with 4 strikes, cools off with opens, finishes with a spare in the 10th.
    /// 
    /// Frame  1: 10         = 30   (strike: 10+10+10) cumulative:  30
    /// Frame  2: 10         = 30   (strike: 10+10+10) cumulative:  60
    /// Frame  3: 10         = 23   (strike: 10+10+3)  cumulative:  83
    /// Frame  4: 10         = 14   (strike: 10+3+1)   cumulative:  97
    /// Frame  5: 3, 1       = 4    (open)             cumulative: 101
    /// Frame  6: 0, 5       = 5    (open)             cumulative: 106
    /// Frame  7: 2, 3       = 5    (open)             cumulative: 111
    /// Frame  8: 4, 4       = 8    (open)             cumulative: 119
    /// Frame  9: 1, 0       = 1    (open)             cumulative: 120
    /// Frame 10: 6, 4, 10   = 20   (spare+bonus)      cumulative: 140
    /// </summary>
    [Fact]
    public void FullGame_StreakyHotStart_WithPerFrameAssertions()
    {
        var card = new ScoreCard();

        // -- Frames 1-4: four strikes --
        card.Roll(10);
        card.Roll(10);
        card.Roll(10);
        card.Roll(10);

        // After 4 strikes: frames 0-1 have enough subsequent rolls to score, frames 2-3 do not
        Assert.Equal(30, card.GetFrameScore(card.Frames[0])); // 10+10+10
        Assert.Equal(30, card.GetFrameScore(card.Frames[1])); // 10+10+10
        Assert.Null(card.GetFrameScore(card.Frames[2]));       // needs 1 more subsequent roll
        Assert.Null(card.GetFrameScore(card.Frames[3]));       // needs 2 more subsequent rolls

        // -- Frame 5: 3, 1 (open) --
        card.Roll(3);
        Assert.Null(card.GetFrameScore(card.Frames[3])); // still needs 1 more

        card.Roll(1);
        Assert.Equal(30, card.GetFrameScore(card.Frames[0]));
        Assert.Equal(30, card.GetFrameScore(card.Frames[1]));
        Assert.Equal(23, card.GetFrameScore(card.Frames[2])); // 10+10+3
        Assert.Equal(14, card.GetFrameScore(card.Frames[3])); // 10+3+1
        Assert.Equal(4, card.GetFrameScore(card.Frames[4]));  // 3+1

        // -- Frame 6: 0, 5 (open) --
        card.Roll(0);
        card.Roll(5);
        Assert.Equal(5, card.GetFrameScore(card.Frames[5]));

        // -- Frame 7: 2, 3 (open) --
        card.Roll(2);
        card.Roll(3);
        Assert.Equal(5, card.GetFrameScore(card.Frames[6]));

        // -- Frame 8: 4, 4 (open) --
        card.Roll(4);
        card.Roll(4);
        Assert.Equal(8, card.GetFrameScore(card.Frames[7]));

        // -- Frame 9: 1, 0 (open) --
        card.Roll(1);
        card.Roll(0);
        Assert.Equal(1, card.GetFrameScore(card.Frames[8]));

        // -- Frame 10: 6, 4, 10 (spare + bonus) --
        card.Roll(6);
        card.Roll(4);
        Assert.Null(card.GetFrameScore(card.Frames[9])); // spare, needs bonus
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[9]));

        // -- Final cumulative assertions --
        // Recalculated:
        // F0: 30, F1: 30, F2: 23, F3: 14, F4: 4, F5: 5, F6: 5, F7: 8, F8: 1, F9: 20
        // Cumul: 30, 60, 83, 97, 101, 106, 111, 119, 120, 140
        Assert.True(card.IsGameComplete());
        Assert.Equal(30, card.GetScore(card.Frames[0]));
        Assert.Equal(60, card.GetScore(card.Frames[1]));
        Assert.Equal(83, card.GetScore(card.Frames[2]));
        Assert.Equal(97, card.GetScore(card.Frames[3]));
        Assert.Equal(101, card.GetScore(card.Frames[4]));
        Assert.Equal(106, card.GetScore(card.Frames[5]));
        Assert.Equal(111, card.GetScore(card.Frames[6]));
        Assert.Equal(119, card.GetScore(card.Frames[7]));
        Assert.Equal(120, card.GetScore(card.Frames[8]));
        Assert.Equal(140, card.GetScore(card.Frames[9]));
        Assert.Equal(140, card.GetScore());
    }

    /// <summary>
    /// Low-scoring game: lots of gutters and low rolls, no strikes or spares.
    /// 
    /// Frame  1: 0, 0  = 0   cumulative:  0
    /// Frame  2: 1, 0  = 1   cumulative:  1
    /// Frame  3: 0, 2  = 2   cumulative:  3
    /// Frame  4: 3, 3  = 6   cumulative:  9
    /// Frame  5: 0, 0  = 0   cumulative:  9
    /// Frame  6: 1, 1  = 2   cumulative: 11
    /// Frame  7: 0, 0  = 0   cumulative: 11
    /// Frame  8: 2, 0  = 2   cumulative: 13
    /// Frame  9: 0, 1  = 1   cumulative: 14
    /// Frame 10: 1, 2  = 3   cumulative: 17
    /// </summary>
    [Fact]
    public void FullGame_LowScoring_WithPerFrameAssertions()
    {
        var card = new ScoreCard();

        RollMany(card, 0, 0);
        Assert.Equal(0, card.GetFrameScore(card.Frames[0]));
        Assert.Equal(0, card.GetScore(card.Frames[0]));

        RollMany(card, 1, 0);
        Assert.Equal(1, card.GetFrameScore(card.Frames[1]));
        Assert.Equal(1, card.GetScore(card.Frames[1]));

        RollMany(card, 0, 2);
        Assert.Equal(2, card.GetFrameScore(card.Frames[2]));
        Assert.Equal(3, card.GetScore(card.Frames[2]));

        RollMany(card, 3, 3);
        Assert.Equal(6, card.GetFrameScore(card.Frames[3]));
        Assert.Equal(9, card.GetScore(card.Frames[3]));

        RollMany(card, 0, 0);
        Assert.Equal(0, card.GetFrameScore(card.Frames[4]));
        Assert.Equal(9, card.GetScore(card.Frames[4]));

        RollMany(card, 1, 1);
        Assert.Equal(2, card.GetFrameScore(card.Frames[5]));
        Assert.Equal(11, card.GetScore(card.Frames[5]));

        RollMany(card, 0, 0);
        Assert.Equal(0, card.GetFrameScore(card.Frames[6]));
        Assert.Equal(11, card.GetScore(card.Frames[6]));

        RollMany(card, 2, 0);
        Assert.Equal(2, card.GetFrameScore(card.Frames[7]));
        Assert.Equal(13, card.GetScore(card.Frames[7]));

        RollMany(card, 0, 1);
        Assert.Equal(1, card.GetFrameScore(card.Frames[8]));
        Assert.Equal(14, card.GetScore(card.Frames[8]));

        RollMany(card, 1, 2);
        Assert.Equal(3, card.GetFrameScore(card.Frames[9]));
        Assert.Equal(17, card.GetScore(card.Frames[9]));

        Assert.True(card.IsGameComplete());
        Assert.Equal(17, card.GetScore());
    }

    /// <summary>
    /// Alternating strikes and spares throughout the game.
    /// 
    /// Frame  1: 10          (strike)  bonus=6+4=10  -> 20  cumulative:  20
    /// Frame  2: 6, 4        (spare)   bonus=10      -> 20  cumulative:  40
    /// Frame  3: 10          (strike)  bonus=7+3=10  -> 20  cumulative:  60
    /// Frame  4: 7, 3        (spare)   bonus=10      -> 20  cumulative:  80
    /// Frame  5: 10          (strike)  bonus=8+2=10  -> 20  cumulative: 100
    /// Frame  6: 8, 2        (spare)   bonus=10      -> 20  cumulative: 120
    /// Frame  7: 10          (strike)  bonus=5+5=10  -> 20  cumulative: 140
    /// Frame  8: 5, 5        (spare)   bonus=10      -> 20  cumulative: 160
    /// Frame  9: 10          (strike)  bonus=9+1=10  -> 20  cumulative: 180
    /// Frame 10: 9, 1, 10    (spare+bonus)           -> 20  cumulative: 200
    /// </summary>
    [Fact]
    public void FullGame_AlternatingStrikesAndSpares_WithPerFrameAssertions()
    {
        var card = new ScoreCard();

        // -- Frame 1: strike --
        card.Roll(10);
        Assert.Null(card.GetFrameScore(card.Frames[0]));

        // -- Frame 2: 6, 4 (spare) --
        card.Roll(6);
        Assert.Null(card.GetFrameScore(card.Frames[0])); // still needs 1 more bonus roll
        card.Roll(4);
        Assert.Equal(20, card.GetFrameScore(card.Frames[0])); // 10+6+4=20
        Assert.Null(card.GetFrameScore(card.Frames[1]));       // spare needs next roll

        // -- Frame 3: strike --
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[1])); // 10+10=20
        Assert.Null(card.GetFrameScore(card.Frames[2]));

        // -- Frame 4: 7, 3 (spare) --
        card.Roll(7);
        card.Roll(3);
        Assert.Equal(20, card.GetFrameScore(card.Frames[2])); // 10+7+3=20
        Assert.Null(card.GetFrameScore(card.Frames[3]));

        // -- Frame 5: strike --
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[3])); // 10+10=20
        Assert.Null(card.GetFrameScore(card.Frames[4]));

        // -- Frame 6: 8, 2 (spare) --
        card.Roll(8);
        card.Roll(2);
        Assert.Equal(20, card.GetFrameScore(card.Frames[4])); // 10+8+2=20
        Assert.Null(card.GetFrameScore(card.Frames[5]));

        // -- Frame 7: strike --
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[5])); // 10+10=20
        Assert.Null(card.GetFrameScore(card.Frames[6]));

        // -- Frame 8: 5, 5 (spare) --
        card.Roll(5);
        card.Roll(5);
        Assert.Equal(20, card.GetFrameScore(card.Frames[6])); // 10+5+5=20
        Assert.Null(card.GetFrameScore(card.Frames[7]));

        // -- Frame 9: strike --
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[7])); // 10+10=20
        Assert.Null(card.GetFrameScore(card.Frames[8]));

        // -- Frame 10: 9, 1, 10 (spare + bonus) --
        card.Roll(9);
        Assert.Null(card.GetFrameScore(card.Frames[8])); // needs 1 more
        card.Roll(1);
        Assert.Equal(20, card.GetFrameScore(card.Frames[8])); // 10+9+1=20
        Assert.Null(card.GetFrameScore(card.Frames[9]));       // spare needs bonus
        card.Roll(10);
        Assert.Equal(20, card.GetFrameScore(card.Frames[9])); // 9+1+10=20

        // -- Final cumulative assertions --
        Assert.True(card.IsGameComplete());
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(20, card.GetFrameScore(card.Frames[i]));
            Assert.Equal(20 * (i + 1), card.GetScore(card.Frames[i]));
        }
        Assert.Equal(200, card.GetScore());
    }

    // ── IsGameComplete Mid-Game ───────────────────────────────

    [Fact]
    public void IsGameComplete_MidGame_ReturnsFalse()
    {
        var card = new ScoreCard();
        // Roll a few frames but don't finish
        card.Roll(10); // frame 1
        card.Roll(3);
        card.Roll(4);  // frame 2
        Assert.False(card.IsGameComplete());
    }

    // ── GetScore on Fresh Card ────────────────────────────────

    [Fact]
    public void GetScore_NoRolls_Returns0()
    {
        var card = new ScoreCard();
        Assert.Equal(0, card.GetScore());
    }

    // ── GetScore(throughFrame) Null When Earlier Frame Is Indeterminate ──

    [Fact]
    public void GetScore_ThroughLaterFrame_NullWhenEarlierFrameIndeterminate()
    {
        var card = new ScoreCard();
        // Frame 0: strike (needs 2 bonus rolls -> indeterminate)
        card.Roll(10);
        // Frame 1: strike (needs 2 bonus rolls -> indeterminate)
        card.Roll(10);
        // Frame 2: open (3+4=7) -> this frame itself is complete and scorable,
        //   but cumulative score through frame 2 requires frames 0 & 1 to be
        //   determinate first, and frame 0 is still missing its second bonus roll.

        // Frame 0: 10 + 10 + ? = indeterminate (only 1 of 2 bonus rolls known)
        Assert.Null(card.GetScore(card.Frames[0]));
        // Frame 1: also indeterminate (0 of 2 bonus rolls known)
        Assert.Null(card.GetScore(card.Frames[1]));

        // Now give frame 2 its first roll — this is frame 0's second bonus roll
        card.Roll(3);
        // Frame 0 is now determinate: 10 + 10 + 3 = 23
        Assert.Equal(23, card.GetFrameScore(card.Frames[0]));
        // Frame 1 still indeterminate (has only 1 of 2 bonus rolls: the 3)
        Assert.Null(card.GetFrameScore(card.Frames[1]));
        // Cumulative through frame 1 is null because frame 1 is indeterminate
        Assert.Null(card.GetScore(card.Frames[1]));

        // Complete frame 2
        card.Roll(4);
        // Frame 1 now determinate: 10 + 3 + 4 = 17
        Assert.Equal(17, card.GetFrameScore(card.Frames[1]));
        // Cumulative through frame 2: 23 + 17 + 7 = 47
        Assert.Equal(47, card.GetScore(card.Frames[2]));
    }
}
