using System.IdentityModel.Tokens.Jwt;
using Account.Entities;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Account.Services.Impl;

public class TokenService : ITokenService
{
    private RandomNumberGenerator _numberGenerator;
    private AppSettings _settings;
    public TokenService(AppSettings settings, RandomNumberGenerator numberGenerator)
    {
        _numberGenerator = numberGenerator;
        _settings = settings;
    }
    public RefreshToken GenerateRefreshToken(string ip)
    {
        var randomBytes = new byte[64];
        _numberGenerator.GetBytes(randomBytes);
        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            Expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpireDays),
            Created = DateTime.UtcNow,
            CreatedByIp = ip
        };
    }

    public string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_settings.Secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpireMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}