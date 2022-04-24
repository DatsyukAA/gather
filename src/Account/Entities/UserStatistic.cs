using System.ComponentModel.DataAnnotations;
using Account.Data;

namespace Account.Entities
{
    public class UserStatistic : Entity
    {
        [Required]
        public User Referrer { get; set; } = new();
        public List<IPHistory> IpHistory { get; set; } = new();
    }
}