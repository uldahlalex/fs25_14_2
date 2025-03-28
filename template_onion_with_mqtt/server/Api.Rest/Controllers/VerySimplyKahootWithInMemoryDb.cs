using Application.Interfaces.Infrastructure.Websocket;
using Microsoft.AspNetCore.Mvc;

namespace Api.Rest.Controllers;

public class VerySimplyKahootWithInMemoryDb(
    IConnectionManager connectionManager) : ControllerBase
{
    [HttpPost(nameof(JoinLobby))]
    public async Task<ActionResult> JoinLobby([FromQuery] string clientId)
    {
        await connectionManager.AddToTopic("lobby", clientId);
        var allClients = await connectionManager.GetMembersFromTopicId("lobby");
        await connectionManager.BroadcastToTopic("lobby",
            new
            {
                Message =
                    "This broadcast indicates someone has joined the lobby and is broadcast to everyone in the lobby",
                eventType = "lobby",
                AllClientIds = allClients
            });
        return Ok();
    }

    [HttpPost(nameof(StartGame))]
    public async Task<ActionResult> StartGame()
    {
        var game = new
        {
            gameId = 1
        };
        var clients = await connectionManager.GetMembersFromTopicId("lobby");
        foreach (var client in clients)
        {
            await connectionManager.AddToTopic("games/" + game.gameId, client);
            await connectionManager.RemoveFromTopic("lobby", client);
        }

        var serverAddsClientToGameDto = new
        {
            Message = "This indicates that a user has been moved from lobby to game (first round not stated yet...)",
            GameId = game.gameId,
            eventType = "game"
        };
        await connectionManager.BroadcastToTopic("games/" + game.gameId, serverAddsClientToGameDto);
        return Ok();
    }

    [HttpPost(nameof(PlayThroughRounds))]
    public async Task<ActionResult> PlayThroughRounds()
    {
        var game = new
        {
            gameId = 1,
            Q1Id = 1,
            Q1 = "What is the capital of France?",
            A1 = "Paris",

            Q2 = "What is 2+2?",
            Q2Id = 2,
            A2 = "4",

            P1 = "Alice",
            P2 = "Bob"
        };
        var nextRound = Round.Rounds.FirstOrDefault(r => r.IsDone == false);
        if (nextRound is null)
        {
            await FinishGameBySendingResults(game.gameId);
            return Ok();
        }

        //notice: this runs only if there is a nextRound object since the scope above does "return Ok();" 
        await PlayNextRound(nextRound, game.gameId);

        return Ok();
    }

    private async Task FinishGameBySendingResults(int gameId)
    {
        await connectionManager.BroadcastToTopic("games/" + gameId, new
        {
            Result = Round.Rounds,
            eventType = "round"
        });
    }

    private async Task PlayNextRound(Round nextRound, int gameId)
    {
        await connectionManager.BroadcastToTopic("games/" + gameId, new
        {
            Q = nextRound.QuestionId == 1 ? "What is the capital of france?" : "What is 2+2?",
            questionId = nextRound.QuestionId,
            eventType = "round"
        });
        await Task.Delay(20_000); //This is the timeframe where users can submit answers using the "SubmitAnswer" controller method
        nextRound.IsDone = true;
        await connectionManager.BroadcastToTopic("games/" + gameId, new
        {
            Result = Round.Rounds,
            eventType = "round"
        });
    }

    [HttpPost(nameof(SubmitAnswer))]
    public Task<ActionResult> SubmitAnswer(
        [FromQuery] string player,
        [FromQuery] string answer,
        [FromQuery] int questionId)
    {
        var pa = new PlayerAnswers { Player = player, Answer = answer };
        var round = Round.Rounds.First(r => r.QuestionId == questionId);
        round.PlayerAnswersList.Add(new PlayerAnswers { Answer = answer, Player = player });
        return Task.FromResult<ActionResult>(Ok(pa));
    }
}

public class Round
{
    public static List<Round> Rounds = new()
    {
        new Round
        {
            QuestionId = 1,
            IsDone = false,
            PlayerAnswersList = new List<PlayerAnswers>()
        },
        new Round
        {
            QuestionId = 2,
            IsDone = false,
            PlayerAnswersList = new List<PlayerAnswers>()
        }
    };

    public int QuestionId { get; set; }
    public bool IsDone { get; set; }

    // ReSharper disable once CollectionNeverQueried.Global
    public List<PlayerAnswers> PlayerAnswersList { get; set; } = new();
}

public class PlayerAnswers
{
    public string Player { get; set; } = null!;
    public string Answer { get; set; } = null!;
}