using Account.Entities;
using Account.Models;

namespace Account.Services
{
    public interface IAccountService
    {
        AuthenticateResponse? Authenticate(string login, string password, string ip);
        AuthenticateResponse? RefreshToken(string token, string ip);
        bool RevokeToken(string token, string ip);
        IEnumerable<User> GetAll();
        User? GetById(int id);
        AuthenticateResponse? Register(string login, string password, string email, string name, string ip);
    }
}