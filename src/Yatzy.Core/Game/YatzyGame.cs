using Yatzy.Core.Models;
using Yatzy.Core.Scoring;

namespace Yatzy.Core.Game;

public sealed class YatzyGame(IDiceRoller diceRoller)
{
    private readonly IDiceRoller _diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller));
    private readonly int?[] _dice = new int?[5];
    private readonly bool[] _held = new bool[5];
    private ScoreCard _scoreCard = new();
    private IReadOnlyList<PlayerState> _players = [];
    private int _activePlayerIndex;

    public bool IsStarted { get; private set; }

    public bool IsGameOver => _players.Count > 0 && _players.All(player => player.ScoreCard.IsComplete);

    public IReadOnlyList<PlayerState> Players => _players;

    public int ActivePlayerIndex => _activePlayerIndex;

    public PlayerState? ActivePlayer => _players.Count == 0 ? null : _players[_activePlayerIndex];

    public int RoundNumber { get; private set; } = 1;

    public int RollCount { get; private set; }

    public int RollsRemaining => 3 - RollCount;

    public ScoreCard ScoreCard => ActivePlayer?.ScoreCard ?? _scoreCard;

    public IReadOnlyList<DieState> Dice => Enumerable.Range(0, _dice.Length)
        .Select(index => new DieState(_dice[index], _held[index]))
        .ToArray();

    public IReadOnlyDictionary<ScoreCategory, int> SuggestedScores =>
        RollCount == 0
            ? new Dictionary<ScoreCategory, int>()
            : ScoreCalculator.CalculateAll(_dice.Select(value => value!.Value).ToArray());

    public void StartNewGame()
    {
        StartNewGame([new PlayerSetup("Spiller 1", "#d7ad5a")]);
    }

    public GameActionResult StartNewGame(IReadOnlyList<PlayerSetup> playerSetups)
    {
        if (playerSetups is null || playerSetups.Count is < 1 or > 4)
        {
            return GameActionResult.Failed(GameErrorCode.InvalidPlayerCount);
        }

        if (playerSetups.Any(player => string.IsNullOrWhiteSpace(player.Name)))
        {
            return GameActionResult.Failed(GameErrorCode.InvalidPlayerName);
        }

        if (playerSetups.Any(player => string.IsNullOrWhiteSpace(player.Color)))
        {
            return GameActionResult.Failed(GameErrorCode.InvalidPlayerColor);
        }

        if (playerSetups.Select(player => player.Color).Distinct(StringComparer.OrdinalIgnoreCase).Count() != playerSetups.Count)
        {
            return GameActionResult.Failed(GameErrorCode.DuplicatePlayerColor);
        }

        _players = playerSetups
            .Select(player => new PlayerState(player.Name.Trim(), player.Color))
            .ToArray();
        _activePlayerIndex = 0;
        RoundNumber = 1;
        Array.Clear(_dice);
        Array.Clear(_held);
        _scoreCard = _players[0].ScoreCard;
        RollCount = 0;
        IsStarted = true;
        return GameActionResult.Succeeded();
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

        var scoreCard = ScoreCard;
        if (scoreCard.IsFilled(category))
        {
            return GameActionResult.Failed(GameErrorCode.CategoryAlreadyUsed);
        }

        var values = _dice.Select(value => value!.Value).ToArray();
        var score = setToZero ? 0 : ScoreCalculator.Calculate(category, values);
        scoreCard.TryRegister(category, score);

        if (!IsGameOver)
        {
            var previousPlayerIndex = _activePlayerIndex;
            do
            {
                _activePlayerIndex = (_activePlayerIndex + 1) % _players.Count;
                if (_activePlayerIndex == 0 && previousPlayerIndex == _players.Count - 1)
                {
                    RoundNumber++;
                }
            }
            while (_players[_activePlayerIndex].ScoreCard.IsComplete);

            Array.Clear(_dice);
            Array.Clear(_held);
            RollCount = 0;
        }

        return GameActionResult.Succeeded();
    }
}