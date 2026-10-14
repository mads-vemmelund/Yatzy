namespace Yatzy.Core.Models;

public sealed class PlayerState(string name, string color)
{
    public string Name { get; } = name;

    public string Color { get; } = color;

    public ScoreCard ScoreCard { get; } = new();
}

public sealed record PlayerSetup(string Name, string Color);