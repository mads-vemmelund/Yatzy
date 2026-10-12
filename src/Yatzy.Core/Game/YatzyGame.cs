using Yatzy.Core.Models;
using Yatzy.Core.Scoring;

namespace Yatzy.Core.Game;

public sealed class YatzyGame(IDiceRoller diceRoller)
{
    private readonly IDiceRoller _diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller));
    private readonly int?[] _dice = new int?[5];
    private readonly bool[] _held = new bool[5];
    private ScoreCard _scoreCard = new();

    public bool IsStarted { get; private set; }

    public bool IsGameOver => _scoreCard.IsComplete;

    public int RollCount { get; private set; }

    public int RollsRemaining => 3 - RollCount;

    public ScoreCard ScoreCard => _scoreCard;

    public IReadOnlyList<DieState> Dice => Enumerable.Range(0, _dice.Length)
        .Select(index => new DieState(_dice[index], _held[index]))
        .ToArray();

    public IReadOnlyDictionary<ScoreCategory, int> SuggestedScores =>
        RollCount == 0
            ? new Dictionary<ScoreCategory, int>()
            : ScoreCalculator.CalculateAll(_dice.Select(value => value!.Value).ToArray());

    public void StartNewGame()
    {
        Array.Clear(_dice);
        Array.Clear(_held);
        _scoreCard = new ScoreCard();
        RollCount = 0;
        IsStarted = true;
    }

    public GameActionResult Roll()
    {
        if (!IsStarted)
        {
            return GameActionResult.Failed(GameErrorCode.GameNotStarted);
        }

        if (IsGameOver)
        {
            return GameActionResult.Failed(GameErrorCode.GameOver);
        }

        if (RollCount >= 3)
        {
            return GameActionResult.Failed(GameErrorCode.NoRollsRemaining);
        }

        var valuesToRoll = Enumerable.Range(0, _dice.Length)
            .Where(index => !_held[index])
            .Select(_ => _diceRoller.RollDie())
            .ToArray();

        if (valuesToRoll.Any(value => value is < 1 or > 6))
        {
            throw new InvalidOperationException("Terningekasteren returnerede en ugyldig værdi.");
        }

        var nextValue = 0;
        for (var index = 0; index < _dice.Length; index++)
        {
            if (!_held[index])
            {
                _dice[index] = valuesToRoll[nextValue++];
            }
        }

        RollCount++;
        return GameActionResult.Succeeded();
    }

    public GameActionResult ToggleHold(int dieIndex)
    {
        if (!IsStarted)
        {
            return GameActionResult.Failed(GameErrorCode.GameNotStarted);
        }

        if (IsGameOver)
        {
            return GameActionResult.Failed(GameErrorCode.GameOver);
        }

        if (dieIndex is < 0 or >= 5)
        {
            return GameActionResult.Failed(GameErrorCode.InvalidDieIndex);
        }

        if (RollCount == 0 || RollCount >= 3 || !_dice[dieIndex].HasValue)
        {
            return GameActionResult.Failed(GameErrorCode.HoldUnavailable);
        }

        _held[dieIndex] = !_held[dieIndex];
        return GameActionResult.Succeeded();
    }

    public GameActionResult ChooseCategory(ScoreCategory category, bool setToZero = false)
    {
        if (!IsStarted)
        {
            return GameActionResult.Failed(GameErrorCode.GameNotStarted);
        }

        if (IsGameOver)
        {
            return GameActionResult.Failed(GameErrorCode.GameOver);
        }

        if (RollCount == 0)
        {
            return GameActionResult.Failed(GameErrorCode.RollRequired);
        }

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category));
        }

        if (_scoreCard.IsFilled(category))
        {
            return GameActionResult.Failed(GameErrorCode.CategoryAlreadyUsed);
        }

        var values = _dice.Select(value => value!.Value).ToArray();
        var score = setToZero ? 0 : ScoreCalculator.Calculate(category, values);
        _scoreCard.TryRegister(category, score);

        if (!IsGameOver)
        {
            Array.Clear(_dice);
            Array.Clear(_held);
            RollCount = 0;
        }

        return GameActionResult.Succeeded();
    }
}