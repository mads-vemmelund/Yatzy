namespace Yatzy.Core.Game;

public sealed record GameActionResult(bool Success, GameErrorCode ErrorCode)
{
    public static GameActionResult Succeeded() => new(true, GameErrorCode.None);

    public static GameActionResult Failed(GameErrorCode errorCode) => new(false, errorCode);
}