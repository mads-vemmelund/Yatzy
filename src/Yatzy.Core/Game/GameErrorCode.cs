namespace Yatzy.Core.Game;

public enum GameErrorCode
{
    None,
    GameNotStarted,
    GameOver,
    NoRollsRemaining,
    RollRequired,
    InvalidDieIndex,
    HoldUnavailable,
    CategoryAlreadyUsed,
    InvalidPlayerCount,
    InvalidPlayerName,
    InvalidPlayerColor,
    DuplicatePlayerColor
}