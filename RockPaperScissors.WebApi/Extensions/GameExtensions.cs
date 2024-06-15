using RockPaperScissors.WebApi.Entities;

namespace RockPaperScissors.WebApi.Extensions;

public static class GameExtensions
{
    public static Player GetOpponentSession(this Game game, string playerId)
    {
        if (game.Player1.ConnectionId == playerId)
        {
            return game.Player2;
        }

        return game.Player1;
    }

    public static Player GetPlayerSession(this Game game, string playerId)
    {
        if (game.Player1.ConnectionId == playerId)
        {
            return game.Player1;
        }

        return game.Player2;
    }

    /*public static GameStatus GetGameResult(this Game game, string playerId)
    {
        var playerChoice = game.GetPlayerSession(playerId).CurrentMove;
        var opponentChoice = game.GetOpponentSession(playerId).CurrentMove;
        
        if (playerChoice == opponentChoice)
        {
            return GameStatus.Draw;
        }

        if ((playerChoice == Move.Rock && opponentChoice == Move.Scissors) ||
            (playerChoice == Move.Paper && opponentChoice == Move.Rock) ||
            (playerChoice == Move.Scissors && opponentChoice == Move.Paper))
        {
            return GameStatus.Win;
        }

        return GameStatus.Lose;
    }*/
}