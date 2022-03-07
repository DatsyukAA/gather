using System.ComponentModel.DataAnnotations;
using Account.Data;
using Account.Entities.UserAgent;

namespace Account.Entities
{
    public class UserStatistic : Entity
    {
        [Required]
        public User Referrer { get; set; } = new();
        public List<IPHistory> IpHistory { get; set; } = new();
        public List<UserAgentInfo> UAHistory { get; set; } = new();
        public List<string> Paths { get; set; } = new();
    }
}