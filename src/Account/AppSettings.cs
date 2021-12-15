namespace Account;

public class AppSettings
{
    public string Secret { get; set; }
    public int RefreshTokenExpireDays { get; set; }
    public int AccessTokenExpireMinutes { get; set; }
    public string NotificationHost { get; set; }
}