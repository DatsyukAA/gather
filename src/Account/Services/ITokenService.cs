using Account.Entities;

namespace Account.Services
{
    public interface ITokenService
    {
        RefreshToken GenerateRefreshToken(string ip);
        string GenerateJwtToken(User user);
    }
}