using System.ComponentModel.DataAnnotations;

namespace Account.Data;

public class Entity : IEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime CreationDate { get; } = DateTime.UtcNow;
}