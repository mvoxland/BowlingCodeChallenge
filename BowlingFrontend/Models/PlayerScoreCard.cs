using BowlingLogic;

namespace BowlingFrontend.Models;

public class PlayerScoreCard
{
    public required string Name { get; set; }
    public required ScoreCard ScoreCard { get; set; }
}
