namespace Yatzy.Core.Game;

public sealed class RandomDiceRoller : IDiceRoller
{
    public int RollDie() => Random.Shared.Next(1, 7);
}