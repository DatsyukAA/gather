using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Account.Data;
using Microsoft.EntityFrameworkCore;

namespace Account.Entities
{

    [Owned]
    public class RefreshToken : Entity
    {
        [Key]
        [JsonIgnore]
        new public int Id { get; set; }

        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; }
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; } = null;
        public string? ReplacedByToken { get; set; } = null;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}