using Microsoft.AspNetCore.SignalR;
using RockPaperScissors.WebApi.Entities;
using RockPaperScissors.WebApi.Entities.Constants;
using RockPaperScissors.WebApi.Extensions;
using RockPaperScissors.WebApi.Services;

namespace RockPaperScissors.WebApi.Hubs;

public class GameHub : Hub
{
    private static Player? _waitingPlayer;

    private readonly IGameService _gameService;

    public GameHub(IGameService gameService)
    {
        _gameService = gameService;
    }

    public async Task PlayGame(string gameId, int move)
    {
        var game = await _gameService.GetGameById(gameId);

        await _gameService.MakeMove(game, gameId, Context.ConnectionId, (Move)move);

        var player = game.GetPlayerSession(Context.ConnectionId);
        var opponent = game.GetOpponentSession(player.ConnectionId);

        if (opponent.CurrentMove.HasValue)
        {
            var playerResult = await _gameService.GetRoundResult(game, gameId, Context.ConnectionId);
            var opponentResult = await _gameService.GetRoundResult(game, gameId, opponent.ConnectionId);

            if (player.Score == 3 || opponent.Score == 3)
            {
                var playerGame = player.Score == 3 ? GameStatus.Win : GameStatus.Lose;
                var opponentGame = opponent.Score == 3 ? GameStatus.Win : GameStatus.Lose;

                await Clients.Client(player.ConnectionId)
                    .SendAsync(GameEvents.GameFinished, playerGame.ToString(), opponent.CurrentMove.Value.ToString());

                await Clients.Client(opponent.ConnectionId)
                    .SendAsync(GameEvents.GameFinished, opponentGame.ToString(), player.CurrentMove.Value.ToString());
            }
            else
            {
                await Clients.Client(player.ConnectionId)
                    .SendAsync(GameEvents.RoundFinished, playerResult.ToString(),
                        opponent.CurrentMove.Value.ToString());

                await Clients.Client(opponent.ConnectionId)
                    .SendAsync(GameEvents.RoundFinished, opponentResult.ToString(),
                        player.CurrentMove.Value.ToString());

                await _gameService.ClearMoves(game, gameId);
            }
        }
        else
        {
            await Clients.Client(player.ConnectionId).SendAsync(GameEvents.WaitingForMove);
            await Clients.Client(opponent.ConnectionId).SendAsync(GameEvents.OpponentMadeMove);
        }
    }

    public async Task JoinRandomGame()
    {
        if (_waitingPlayer == null)
        {
            _waitingPlayer = new Player { ConnectionId = Context.ConnectionId };
            await Clients.Client(Context.ConnectionId).SendAsync(GameEvents.WaitingForPlayer);
        }
        else
        {
            var currentPlayer = new Player { ConnectionId = Context.ConnectionId };
            var gameId = await _gameService.CreateGame(_waitingPlayer, currentPlayer);

            await Clients.Client(_waitingPlayer.ConnectionId).SendAsync(GameEvents.GameStarted, gameId);
            await Clients.Client(currentPlayer.ConnectionId).SendAsync(GameEvents.GameStarted, gameId);

            _waitingPlayer = null;
        }
    }

    public async Task JoinFriendGame(string gameId)
    {
        var existingGame = await _gameService.GetGameById(gameId);

        if (existingGame == null)
        {
            existingGame = new Game();
            existingGame.Player1 = new Player { ConnectionId = Context.ConnectionId };

            await _gameService.SaveGame(existingGame, gameId);

            await Clients.Client(Context.ConnectionId).SendAsync(GameEvents.WaitingForPlayer);
        }
        else
        {
            existingGame.Player2 = new Player { ConnectionId = Context.ConnectionId };

            await _gameService.SaveGame(existingGame, gameId);

            await Clients.Client(existingGame.Player1.ConnectionId).SendAsync(GameEvents.GameStarted, gameId);
            await Clients.Client(existingGame.Player2.ConnectionId).SendAsync(GameEvents.GameStarted, gameId);
        }
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (_waitingPlayer.ConnectionId == Context.ConnectionId)
        {
            _waitingPlayer = null;
        }

        return base.OnDisconnectedAsync(exception);
    }
}