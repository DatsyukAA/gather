namespace Account.Data;

public class Entity : IEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreationDate { get; } = DateTime.UtcNow;
}