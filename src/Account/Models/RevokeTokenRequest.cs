using System.ComponentModel.DataAnnotations;

namespace Account.Models
{
    public class RevokeTokenRequest
    {
        public string Token { get; set; }
    }
}