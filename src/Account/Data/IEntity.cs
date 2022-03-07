namespace Account.Data;

public interface IEntity
{
    public string Id { get; }
    public DateTime CreationDate { get; }
}