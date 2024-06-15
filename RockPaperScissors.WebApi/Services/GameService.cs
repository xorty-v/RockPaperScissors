using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using RockPaperScissors.WebApi.Entities;
using RockPaperScissors.WebApi.Entities.Constants;
using RockPaperScissors.WebApi.Extensions;

namespace RockPaperScissors.WebApi.Services;

public class GameService : IGameService
{
    private readonly IDistributedCache _cache;

    public GameService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<Guid> CreateGame(Player player1, Player player2)
    {
        var game = new Game
        {
            Player1 = player1,
            Player2 = player2
        };

        var gameId = Guid.NewGuid();
        var gameString = JsonSerializer.Serialize(game);

        await _cache.SetStringAsync(CacheOptions.GamePrefix + gameId, gameString, CacheOptions.ExpirationTime);

        return gameId;
    }

    public async Task<Game> GetGameById(string gameId)
    {
        var gameString = await _cache.GetStringAsync(CacheOptions.GamePrefix + gameId);

        return gameString == null ? null : JsonSerializer.Deserialize<Game>(gameString);
    }

    public async Task SaveGame(Game game, string gameId)
    {
        var gameString = JsonSerializer.Serialize(game);

        await _cache.SetStringAsync(CacheOptions.GamePrefix + gameId, gameString, CacheOptions.ExpirationTime);
    }

    public async Task MakeMove(Game game, string gameId, string playerId, Move move)
    {
        var player = game.GetPlayerSession(playerId);
        player.CurrentMove = move;

        var gameString = JsonSerializer.Serialize(game);
        await _cache.SetStringAsync(CacheOptions.GamePrefix + gameId, gameString, CacheOptions.ExpirationTime);
    }

    public async Task<GameStatus> GetRoundResult(Game game, string gameId, string playerId)
    {
        var player = game.GetPlayerSession(playerId);

        var playerChoice = player.CurrentMove;
        var opponentChoice = game.GetOpponentSession(playerId).CurrentMove;
        
        if (playerChoice == opponentChoice)
        {
            return GameStatus.Draw;
        }

        if ((playerChoice == Move.Rock && opponentChoice == Move.Scissors) ||
            (playerChoice == Move.Paper && opponentChoice == Move.Rock) ||
            (playerChoice == Move.Scissors && opponentChoice == Move.Paper))
        {
            player.Score++;
            
            var gameString = JsonSerializer.Serialize(game);
            await _cache.SetStringAsync(CacheOptions.GamePrefix + gameId, gameString, CacheOptions.ExpirationTime);

            return GameStatus.Win;
        }

        return GameStatus.Lose;
    }

    public async Task ClearMoves(Game game, string gameId)
    {
        game.Player1.CurrentMove = null;
        game.Player2.CurrentMove = null;
        
        var gameString = JsonSerializer.Serialize(game);
        await _cache.SetStringAsync(CacheOptions.GamePrefix + gameId, gameString, CacheOptions.ExpirationTime);
    }
}