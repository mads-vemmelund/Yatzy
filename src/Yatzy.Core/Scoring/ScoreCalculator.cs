using Yatzy.Core.Models;

namespace Yatzy.Core.Scoring;

public static class ScoreCalculator
{
    public static IReadOnlyDictionary<ScoreCategory, int> CalculateAll(IReadOnlyList<int> dice)
    {
        ValidateDice(dice);
        return Enum.GetValues<ScoreCategory>()
            .ToDictionary(category => category, category => CalculateValidated(category, dice));
    }

    public static int Calculate(ScoreCategory category, IReadOnlyList<int> dice)
    {
        ValidateDice(dice);
        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category));
        }

        return CalculateValidated(category, dice);
    }

    private static int CalculateValidated(ScoreCategory category, IReadOnlyList<int> dice)
    {
        var groups = dice.GroupBy(value => value)
            .ToDictionary(group => group.Key, group => group.Count());

        return category switch
        {
            >= ScoreCategory.Ones and <= ScoreCategory.Sixes =>
                dice.Where(value => value == (int)category + 1).Sum(),
            ScoreCategory.OnePair => groups.Where(group => group.Value >= 2)
                .Select(group => group.Key * 2).DefaultIfEmpty().Max(),
            ScoreCategory.TwoPairs => ScoreTwoPairs(groups),
            ScoreCategory.ThreeOfAKind => groups.Where(group => group.Value >= 3)
                .Select(group => group.Key * 3).DefaultIfEmpty().Max(),
            ScoreCategory.FourOfAKind => groups.Where(group => group.Value >= 4)
                .Select(group => group.Key * 4).DefaultIfEmpty().Max(),
            ScoreCategory.SmallStraight => IsStraight(dice, [1, 2, 3, 4, 5]) ? 15 : 0,
            ScoreCategory.LargeStraight => IsStraight(dice, [2, 3, 4, 5, 6]) ? 20 : 0,
            ScoreCategory.FullHouse => groups.Values.Order().SequenceEqual([2, 3]) ? dice.Sum() : 0,
            ScoreCategory.Chance => dice.Sum(),
            ScoreCategory.Yatzy => groups.Values.Contains(5) ? 50 : 0,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };
    }

    private static bool IsStraight(IReadOnlyList<int> dice, int[] expected) =>
        dice.Order().SequenceEqual(expected);

    private static int ScoreTwoPairs(IReadOnlyDictionary<int, int> groups)
    {
        var pairs = groups.Where(group => group.Value >= 2)
            .Select(group => group.Key * 2)
            .OrderDescending()
            .Take(2)
            .ToArray();

        return pairs.Length == 2 ? pairs.Sum() : 0;
    }

    private static void ValidateDice(IReadOnlyList<int> dice)
    {
        ArgumentNullException.ThrowIfNull(dice);
        if (dice.Count != 5)
        {
            throw new ArgumentException("Præcis fem terninger skal angives.", nameof(dice));
        }

        if (dice.Any(value => value is < 1 or > 6))
        {
            throw new ArgumentOutOfRangeException(nameof(dice), "Terningværdier skal være fra 1 til 6.");
        }
    }
}