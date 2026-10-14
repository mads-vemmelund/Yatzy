using Yatzy.Core.Game;
using Yatzy.Core.Models;

namespace Yatzy.Tests.Game;

public sealed class YatzyGameTests
{
    [Fact]
    public void New_game_requires_start_and_first_roll_before_scoring()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1, 2, 3, 4, 5));

        Assert.Equal(GameErrorCode.GameNotStarted, game.Roll().ErrorCode);
        game.StartNewGame();
        Assert.Equal(GameErrorCode.RollRequired, game.ChooseCategory(ScoreCategory.Ones).ErrorCode);
        Assert.Empty(game.SuggestedScores);
    }

    [Fact]
    public void Roll_keeps_held_dice_and_allows_unholding_before_next_roll()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1, 2, 3, 4, 5, 6, 6, 6, 6, 6, 6, 6));
        game.StartNewGame();
        game.Roll();
        game.ToggleHold(0);
        game.ToggleHold(1);

        game.Roll();
        Assert.Equal([1, 2, 6, 6, 6], game.Dice.Select(die => die.Value).ToArray());
        Assert.True(game.Dice[0].IsHeld);

        game.ToggleHold(0);
        game.Roll();
        Assert.Equal([6, 2, 6, 6, 6], game.Dice.Select(die => die.Value).ToArray());
    }

    [Fact]
    public void Fourth_roll_is_rejected_without_changing_dice()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1, 2, 3, 4, 5, 6, 6, 6, 6, 6, 1, 1, 1, 1, 1));
        game.StartNewGame();
        game.Roll();
        game.Roll();
        game.Roll();
        var before = game.Dice.Select(die => die.Value).ToArray();

        Assert.Equal(GameErrorCode.NoRollsRemaining, game.Roll().ErrorCode);
        Assert.Equal(before, game.Dice.Select(die => die.Value).ToArray());
        Assert.Equal(0, game.RollsRemaining);
    }

    [Fact]
    public void Choosing_category_registers_score_and_starts_next_turn()
    {
        var game = new YatzyGame(new SequenceDiceRoller(6, 6, 5, 3, 2));
        game.StartNewGame();
        game.Roll();

        Assert.True(game.ChooseCategory(ScoreCategory.Sixes).Success);
        Assert.Equal(12, game.ScoreCard.GetScore(ScoreCategory.Sixes));
        Assert.Equal(0, game.RollCount);
        Assert.All(game.Dice, die => Assert.Null(die.Value));
        Assert.Equal(3, game.RollsRemaining);
    }

    [Fact]
    public void Multiplayer_turns_rotate_with_separate_scorecards_and_rounds()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1, 1, 1, 1, 1, 2, 2, 2, 2, 2));
        game.StartNewGame([
            new PlayerSetup("Ada", "#d7ad5a"),
            new PlayerSetup("Bo", "#78b5a0")
        ]);

        Assert.Equal("Ada", game.ActivePlayer?.Name);
        game.Roll();
        game.ChooseCategory(ScoreCategory.Ones);

        Assert.Equal("Bo", game.ActivePlayer?.Name);
        Assert.Equal(5, game.Players[0].ScoreCard.GetScore(ScoreCategory.Ones));
        Assert.Null(game.Players[1].ScoreCard.GetScore(ScoreCategory.Ones));
        Assert.Equal(1, game.RoundNumber);

        game.Roll();
        game.ChooseCategory(ScoreCategory.Twos);

        Assert.Equal("Ada", game.ActivePlayer?.Name);
        Assert.Equal(2, game.RoundNumber);
        Assert.Equal(10, game.Players[1].ScoreCard.GetScore(ScoreCategory.Twos));
        Assert.Equal(0, game.RollCount);
    }

    [Fact]
    public void Game_finishes_only_after_every_player_fills_all_categories()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1));
        game.StartNewGame([
            new PlayerSetup("Ada", "#d7ad5a"),
            new PlayerSetup("Bo", "#78b5a0")
        ]);

        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            game.Roll();
            game.ChooseCategory(category, setToZero: true);
        }

        Assert.False(game.IsGameOver);
        Assert.Equal("Bo", game.ActivePlayer?.Name);

        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            game.Roll();
            game.ChooseCategory(category, setToZero: true);
        }

        Assert.True(game.IsGameOver);
    }

    [Fact]
    public void Invalid_player_setup_does_not_change_an_active_game()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1, 2, 3, 4, 5));
        game.StartNewGame();
        game.Roll();
        var before = game.Dice.Select(die => die.Value).ToArray();

        var result = game.StartNewGame([
            new PlayerSetup("Ada", "#d7ad5a"),
            new PlayerSetup("Bo", "#D7AD5A")
        ]);

        Assert.Equal(GameErrorCode.DuplicatePlayerColor, result.ErrorCode);
        Assert.Equal(1, game.RollCount);
        Assert.Equal(before, game.Dice.Select(die => die.Value).ToArray());
        Assert.Single(game.Players);
    }

    [Fact]
    public void Player_setup_requires_a_color_for_each_player()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1));

        var result = game.StartNewGame([new PlayerSetup("Ada", " ")]);

        Assert.Equal(GameErrorCode.InvalidPlayerColor, result.ErrorCode);
        Assert.False(game.IsStarted);
        Assert.Empty(game.Players);
    }

    [Fact]
    public void Category_can_be_sacrificed_for_zero_and_cannot_be_reused()
    {
        var game = new YatzyGame(new SequenceDiceRoller(6, 6, 5, 3, 2));
        game.StartNewGame();
        game.Roll();

        Assert.True(game.ChooseCategory(ScoreCategory.Sixes, setToZero: true).Success);
        Assert.Equal(0, game.ScoreCard.GetScore(ScoreCategory.Sixes));
        game.Roll();
        Assert.Equal(GameErrorCode.CategoryAlreadyUsed, game.ChooseCategory(ScoreCategory.Sixes).ErrorCode);
    }

    [Fact]
    public void Upper_bonus_requires_all_six_categories_and_at_least_63_points()
    {
        var diceSequence = Enumerable.Range(1, 6).SelectMany(face => Enumerable.Repeat(face, 5)).ToArray();
        var game = new YatzyGame(new SequenceDiceRoller(diceSequence));
        game.StartNewGame();

        foreach (var category in ScoreCategoryCatalog.All.Where(item => item.IsUpperSection).Select(item => item.Category))
        {
            game.Roll();
            game.ChooseCategory(category);
        }

        Assert.Equal(105, game.ScoreCard.UpperSectionSum);
        Assert.Equal(50, game.ScoreCard.UpperBonus);
        Assert.Equal(155, game.ScoreCard.TotalScore);
    }

    [Fact]
    public void Upper_bonus_is_awarded_at_exactly_63_points()
    {
        var rounds = new[]
        {
            Enumerable.Repeat(2, 5),
            Enumerable.Repeat(2, 5),
            Enumerable.Repeat(3, 5),
            Enumerable.Repeat(4, 5),
            Enumerable.Repeat(4, 5),
            new[] { 6, 6, 6, 1, 1 }
        };
        var game = new YatzyGame(new SequenceDiceRoller(rounds.SelectMany(round => round).ToArray()));
        game.StartNewGame();

        foreach (var category in ScoreCategoryCatalog.All.Where(item => item.IsUpperSection).Select(item => item.Category))
        {
            game.Roll();
            game.ChooseCategory(category);
        }

        Assert.Equal(63, game.ScoreCard.UpperSectionSum);
        Assert.Equal(50, game.ScoreCard.UpperBonus);
    }

    [Fact]
    public void Bonus_waits_until_upper_section_is_complete_even_when_sum_exceeds_threshold()
    {
        var rounds = Enumerable.Range(1, 5).SelectMany(face => Enumerable.Repeat(face, 5)).ToArray();
        var game = new YatzyGame(new SequenceDiceRoller(rounds));
        game.StartNewGame();
        var upperCategories = ScoreCategoryCatalog.All.Where(item => item.IsUpperSection).Select(item => item.Category).ToArray();

        foreach (var category in upperCategories.Take(5))
        {
            game.Roll();
            game.ChooseCategory(category);
        }

        Assert.True(game.ScoreCard.UpperSectionSum >= 63);
        Assert.Equal(0, game.ScoreCard.UpperBonus);

        game.Roll();
        game.ChooseCategory(upperCategories[5]);
        Assert.Equal(50, game.ScoreCard.UpperBonus);
    }

    [Fact]
    public void Bonus_is_not_awarded_below_threshold()
    {
        var game = new YatzyGame(new SequenceDiceRoller(Enumerable.Repeat(1, 30).ToArray()));
        game.StartNewGame();

        foreach (var category in ScoreCategoryCatalog.All.Where(item => item.IsUpperSection).Select(item => item.Category))
        {
            game.Roll();
            game.ChooseCategory(category);
        }

        Assert.Equal(5, game.ScoreCard.UpperSectionSum);
        Assert.Equal(0, game.ScoreCard.UpperBonus);
    }

    [Fact]
    public void Game_finishes_after_fifteen_categories_and_can_restart()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1));
        game.StartNewGame();

        foreach (var category in Enum.GetValues<ScoreCategory>())
        {
            game.Roll();
            Assert.True(game.ChooseCategory(category, setToZero: true).Success);
        }

        Assert.True(game.IsGameOver);
        Assert.Equal(0, game.ScoreCard.TotalScore);
        Assert.Equal(GameErrorCode.GameOver, game.Roll().ErrorCode);

        game.StartNewGame();
        Assert.False(game.IsGameOver);
        Assert.Equal(0, game.ScoreCard.FilledCount);
        Assert.Equal(3, game.RollsRemaining);
    }

    [Fact]
    public void Hold_cannot_be_changed_before_a_roll_or_after_third_roll()
    {
        var game = new YatzyGame(new SequenceDiceRoller(1));
        game.StartNewGame();
        Assert.Equal(GameErrorCode.HoldUnavailable, game.ToggleHold(0).ErrorCode);

        game.Roll();
        game.Roll();
        game.Roll();
        Assert.Equal(GameErrorCode.HoldUnavailable, game.ToggleHold(0).ErrorCode);
    }

    private sealed class SequenceDiceRoller(params int[] values) : IDiceRoller
    {
        private int _index;

        public int RollDie() => values[_index++ % values.Length];
    }
}