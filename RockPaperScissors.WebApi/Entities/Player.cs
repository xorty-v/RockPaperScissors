namespace RockPaperScissors.WebApi.Entities;

public class Player
{
    public string ConnectionId { get; set; }
    public int Score { get; set; }
    public Move? CurrentMove { get; set; }
}