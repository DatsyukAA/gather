namespace Account.Data;

public interface IRepository<TEntity> where TEntity : IEntity
{
    public TEntity? Single(Func<TEntity, bool>? predicate = null);
    public IEnumerable<TEntity> List(Func<TEntity, bool>? predicate = null, int skip = default, int lastId = default, int take = default);
    public TEntity Insert(TEntity entity);
    public TEntity? Update(int Id, TEntity entity);
    public TEntity? Delete(int Id);
}