namespace Yatzy.Core.Models;

public sealed record ScoreCategoryDefinition(ScoreCategory Category, string DanishName, bool IsUpperSection);

public static class ScoreCategoryCatalog
{
    public static IReadOnlyList<ScoreCategoryDefinition> All { get; } =
    [
        new(ScoreCategory.Ones, "Enere", true),
        new(ScoreCategory.Twos, "Toere", true),
        new(ScoreCategory.Threes, "Treere", true),
        new(ScoreCategory.Fours, "Firere", true),
        new(ScoreCategory.Fives, "Femmere", true),
        new(ScoreCategory.Sixes, "Seksere", true),
        new(ScoreCategory.OnePair, "Ét par", false),
        new(ScoreCategory.TwoPairs, "To par", false),
        new(ScoreCategory.ThreeOfAKind, "Tre ens", false),
        new(ScoreCategory.FourOfAKind, "Fire ens", false),
        new(ScoreCategory.SmallStraight, "Lille straight", false),
        new(ScoreCategory.LargeStraight, "Stor straight", false),
        new(ScoreCategory.FullHouse, "Fuldt hus", false),
        new(ScoreCategory.Chance, "Chancen", false),
        new(ScoreCategory.Yatzy, "Yatzy", false)
    ];
}