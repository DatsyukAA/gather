namespace Account.Data;

public interface IEntity
{
    public int Id { get; }
    public DateTime CreationDate { get; }
}