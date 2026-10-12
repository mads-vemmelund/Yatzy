using Yatzy.Core.Models;
using Yatzy.Core.Scoring;

namespace Yatzy.Tests.Scoring;

public sealed class ScoreCalculatorTests
{
    [Theory]
    [InlineData(ScoreCategory.Ones, 2)]
    [InlineData(ScoreCategory.Twos, 2)]
    [InlineData(ScoreCategory.Threes, 3)]
    [InlineData(ScoreCategory.Fours, 4)]
    [InlineData(ScoreCategory.Fives, 0)]
    [InlineData(ScoreCategory.Sixes, 0)]
    public void Upper_section_sums_matching_dice(ScoreCategory category, int expected)
    {
        Assert.Equal(expected, ScoreCalculator.Calculate(category, [1, 1, 2, 3, 4]));
    }

    [Fact]
    public void One_pair_uses_highest_available_pair()
    {
        Assert.Equal(12, ScoreCalculator.Calculate(ScoreCategory.OnePair, [6, 6, 5, 5, 2]));
    }

    [Fact]
    public void Two_pairs_sums_two_highest_distinct_pairs()
    {
        Assert.Equal(22, ScoreCalculator.Calculate(ScoreCategory.TwoPairs, [6, 6, 5, 5, 2]));
        Assert.Equal(14, ScoreCalculator.Calculate(ScoreCategory.TwoPairs, [2, 2, 2, 5, 5]));
    }

    [Fact]
    public void Two_pairs_returns_zero_without_two_pairs()
    {
        Assert.Equal(0, ScoreCalculator.Calculate(ScoreCategory.TwoPairs, [6, 6, 6, 3, 2]));
    }

    [Fact]
    public void Three_of_a_kind_scores_only_three_matching_dice()
    {
        Assert.Equal(15, ScoreCalculator.Calculate(ScoreCategory.ThreeOfAKind, [5, 5, 5, 5, 2]));
    }

    [Fact]
    public void Four_of_a_kind_scores_four_matching_dice()
    {
        Assert.Equal(20, ScoreCalculator.Calculate(ScoreCategory.FourOfAKind, [5, 5, 5, 5, 2]));
    }

    [Fact]
    public void Straights_require_exact_sequences()
    {
        Assert.Equal(15, ScoreCalculator.Calculate(ScoreCategory.SmallStraight, [5, 1, 4, 3, 2]));
        Assert.Equal(20, ScoreCalculator.Calculate(ScoreCategory.LargeStraight, [6, 2, 5, 3, 4]));
        Assert.Equal(0, ScoreCalculator.Calculate(ScoreCategory.SmallStraight, [1, 2, 3, 4, 4]));
    }

    [Fact]
    public void Full_house_requires_three_and_a_pair()
    {
        Assert.Equal(16, ScoreCalculator.Calculate(ScoreCategory.FullHouse, [2, 2, 2, 5, 5]));
        Assert.Equal(0, ScoreCalculator.Calculate(ScoreCategory.FullHouse, [5, 5, 5, 5, 2]));
        Assert.Equal(0, ScoreCalculator.Calculate(ScoreCategory.FullHouse, [2, 2, 2, 5, 6]));
    }

    [Fact]
    public void Chance_sums_all_dice()
    {
        Assert.Equal(22, ScoreCalculator.Calculate(ScoreCategory.Chance, [6, 6, 5, 3, 2]));
    }

    [Fact]
    public void Yatzy_scores_fifty_only_for_five_equal_dice()
    {
        Assert.Equal(50, ScoreCalculator.Calculate(ScoreCategory.Yatzy, [4, 4, 4, 4, 4]));
        Assert.Equal(0, ScoreCalculator.Calculate(ScoreCategory.Yatzy, [4, 4, 4, 4, 3]));
    }

    [Fact]
    public void Calculate_all_returns_every_category()
    {
        var scores = ScoreCalculator.CalculateAll([6, 6, 5, 3, 2]);

        Assert.Equal(15, scores.Count);
        Assert.Equal(12, scores[ScoreCategory.Sixes]);
        Assert.Equal(12, scores[ScoreCategory.OnePair]);
        Assert.Equal(22, scores[ScoreCategory.Chance]);
    }

    [Fact]
    public void Invalid_dice_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => ScoreCalculator.Calculate(ScoreCategory.Chance, [1, 2, 3, 4]));
        Assert.Throws<ArgumentOutOfRangeException>(() => ScoreCalculator.Calculate(ScoreCategory.Chance, [1, 2, 3, 4, 7]));
    }
}