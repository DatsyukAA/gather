namespace DotaCG.Models;

public class Player
{
    public long SystemId { get; set; }
    public long SteamId { get; set; }
    public long DotaId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Donate>? Donates { get; set; }
    public Rating? Rating { get; set; }
    public List<Player>? LinkedAccounts { get; set; }
    public PlayerItemStatistic Items { get; set; }
    public PlayerMatchStatistic Matches { get; set; }
}