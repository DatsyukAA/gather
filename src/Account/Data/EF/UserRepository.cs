using Account.Entities;
using Security;

namespace Account.Data.EF;
public class UserRepository : IRepository<User>
{
    private readonly AccountContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AccountContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public User? Delete(int Id)
    {
        _logger.LogInformation($"Trying to delete user with id {Id}.");
        var entity = _context.Users.Where(x => x.Id == Id).SingleOrDefault();
        if (entity != null)
        {
            _context.Users.Remove(entity);
            _logger.LogInformation($"User with id {Id} was removed.");
            _context.SaveChanges();
        }
        else
        {
            _logger.LogWarning($"Trying to delete not existing user with id {Id}.");
        }
        return entity;
    }

    public User Insert(User entity)
    {
        _logger.LogInformation($"Trying to insert user with username {entity.Username}");
        _logger.LogInformation($"Generate hash for user password.");
        entity.Password = Authentication.HashPassword(entity.Password);
        _logger.LogInformation($"Hash generated. Saving user to store.");
        var result = _context.Users.Add(entity);
        _context.SaveChanges();
        return result.Entity;
    }

    public IEnumerable<User> List(Func<User, bool>? predicate = null, int skip = default, int lastId = default, int take = default)
    {
        _logger.LogInformation($"Trying to fetch users.\nPredicate is null: {predicate == null}\nTake: {take}\nSkip:{skip}\nPrevious last id: {lastId}");
        var result = predicate != null ? _context.Users.Where(predicate) : _context.Users;
        if (skip != default) result.Skip(skip);
        if (lastId != default) result.SkipWhile(entity => entity.Id != lastId).Skip(1);
        if (take != default) result.Take(take);
        _logger.LogInformation($"Users fetched. Count: {result.Count()}");
        return result;
    }

    public User? Single(Func<User, bool>? predicate = null)
    {
        _logger.LogInformation($"Trying to fetch user.\nPredicate is null: {predicate == null}");
        var elements = _context.Users;
        var result = predicate != null ? _context.Users.SingleOrDefault(predicate) : _context.Users.SingleOrDefault();
        if (result != null)
            _logger.LogInformation($"User with id {result?.Id.ToString() ?? "null"} fetched.");
        else
            _logger.LogInformation($"User doesn't exists.");
        return result;
    }

    public User? Update(int Id, User entity)
    {
        _logger.LogInformation($"Trying to update user with id {Id}");
        var ent = _context.Users.Where(x => x.Id == Id).SingleOrDefault();
        if (ent != null)
        {
            ent.Id = Id;
            _context.Users.Update(ent);
            _logger.LogInformation($"User with id {Id} was removed.");
            _context.SaveChanges();
        }
        else
        {
            _logger.LogWarning($"Trying to update not existing user with id {Id}.");
        }
        return ent;
    }
}