namespace Account;

public class AppSettings
{
    public string Secret { get; set; } = string.Empty;
    public int RefreshTokenExpireDays { get; set; }
    public int AccessTokenExpireMinutes { get; set; }
    public string NotificationHost { get; set; } = string.Empty;
}