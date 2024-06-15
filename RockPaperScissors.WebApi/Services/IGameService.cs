using RockPaperScissors.WebApi.Entities;

namespace RockPaperScissors.WebApi.Services;

public interface IGameService
{
    public Task<Guid> CreateGame(Player player1, Player player2);
    public Task<Game> GetGameById(string gameId);
    public Task SaveGame(Game game, string gameId);
    public Task MakeMove(Game game, string gameId, string playerId, Move move);
    public Task<GameStatus> GetRoundResult(Game game, string gameId, string playerId);
    public Task ClearMoves(Game game, string gameId);
}