using Account.Models.Authenticate;
using Microsoft.Extensions.Options;
using Account.Entities;
using Account.Data;
using Security;
using System.Security.Claims;

namespace Account.Services.Impl
{
    public class AccountService : IAccountService
    {
        private readonly IRepository<User> _accounts;
        private readonly ITokenService _tokens;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountService(
            IRepository<User> accountRepos,
            IOptions<AppSettings> appSettings,
            ITokenService tokens,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _accounts = accountRepos;
            _tokens = tokens;
        }
        public AuthenticateResponse? RefreshToken(string token, string ip)
        {
            var user = _accounts.Single(u => u.RefreshTokens.Any(t => t.Token == token));

            if (user == null) return null;
            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive) return null;

            var newRefreshToken = _tokens.GenerateRefreshToken(ip);
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ip;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            user.RefreshTokens.Add(newRefreshToken);
            _accounts.Update(user.Id, user);

            var jwtToken = _tokens.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
        }

        public AuthenticateResponse? Authenticate(string login, string password, string ip)
        {
            password = Authentication.HashPassword(password);
            var user = _accounts.Single(x => x.Username == login && x.Password == password);

            if (user == null) return null;

            var jwtToken = _tokens.GenerateJwtToken(user);
            var refreshToken = _tokens.GenerateRefreshToken(ip);

            user.RefreshTokens.Add(refreshToken);
            _accounts.Update(user.Id, user);

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public bool RevokeToken(string token, string ip)
        {
            var user = _accounts.Single(u => u.RefreshTokens.Any(t => t.Token == token));
            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ip;
            _accounts.Update(user.Id, user);

            return true;
        }

        public IEnumerable<User> GetAll()
        {
            return _accounts.List();
        }

        public User? GetById(string id)
        {
            return _accounts.Single(x => x.Id == id);
        }

        public AuthenticateResponse? Register(string login, string password, string email, string name, string ip)
        {
            var names = name.Split(' ');
            if (_accounts.List(x => x.Username == login || x.Email == email).Any()) return null;

            var insertResult = _accounts.Insert(new User
            {
                Username = login,
                Password = Authentication.HashPassword(password),
                Email = email,
                FirstName = names[0] ?? "",
                LastName = names[1] ?? ""
            });
            return Authenticate(login, password, ip);
        }

        public User? Update(User user, string ip)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.Claims
            .FirstOrDefault(claim => claim.Type == ClaimTypes.Name)?.Value;
            var currentUser = _accounts.Single(user => user.Id.ToString() == currentUserId);
            var result = _accounts.Update(user.Id, user);
            if (result == null) return null;
            return result;
        }
    }
}

