namespace Account.Data;

public interface IRepository<TEntity> where TEntity : IEntity
{
    public TEntity? Single(Func<TEntity, bool>? predicate = null);
    public IEnumerable<TEntity> List(Func<TEntity, bool>? predicate = null, int skip = default, string? lastId = default, int take = default);
    public TEntity Insert(TEntity entity);
    public TEntity? Update(string Id, TEntity entity);
    public TEntity? Delete(string Id);
}