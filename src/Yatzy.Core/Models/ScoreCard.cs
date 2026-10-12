namespace Yatzy.Core.Models;

public sealed class ScoreCard
{
    private readonly int?[] _scores = new int?[Enum.GetValues<ScoreCategory>().Length];

    public bool IsComplete => _scores.All(score => score.HasValue);

    public int FilledCount => _scores.Count(score => score.HasValue);

    public int UpperSectionSum => SumWhere(definition => definition.IsUpperSection);

    public int LowerSectionSum => SumWhere(definition => !definition.IsUpperSection);

    public bool IsUpperSectionComplete => ScoreCategoryCatalog.All
        .Where(definition => definition.IsUpperSection)
        .All(definition => IsFilled(definition.Category));

    public int UpperBonus => IsUpperSectionComplete && UpperSectionSum >= 63 ? 50 : 0;

    public int TotalScore => UpperSectionSum + LowerSectionSum + UpperBonus;

    public bool IsFilled(ScoreCategory category) => _scores[(int)category].HasValue;

    public int? GetScore(ScoreCategory category) => _scores[(int)category];

    public IReadOnlyList<int?> GetScores() => _scores.ToArray();

    internal bool TryRegister(ScoreCategory category, int score)
    {
        if (!Enum.IsDefined(category) || score is < 0 or > 50 || IsFilled(category))
        {
            return false;
        }

        _scores[(int)category] = score;
        return true;
    }

    private int SumWhere(Func<ScoreCategoryDefinition, bool> predicate) =>
        ScoreCategoryCatalog.All
            .Where(predicate)
            .Sum(definition => GetScore(definition.Category) ?? 0);
}