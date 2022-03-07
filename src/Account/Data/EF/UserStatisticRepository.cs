using Account.Entities;

namespace Account.Data.EF
{
    public class UserStatisticRepository : IRepository<UserStatistic>
    {
        private readonly AccountContext _context;
        private readonly ILogger<UserStatisticRepository> _logger;

        public UserStatisticRepository(AccountContext context, ILogger<UserStatisticRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public UserStatistic? Delete(string Id)
        {
            _logger.LogInformation($"Trying to delete statistic row with id {Id}.");
            var entity = _context.UsersStatistics.Where(x => x.Id == Id).SingleOrDefault();
            if (entity != null)
            {
                _context.UsersStatistics.Remove(entity);
                _logger.LogInformation($"Statistic row with id {Id} was removed.");
                _context.SaveChanges();
            }
            else
            {
                _logger.LogWarning($"Trying to delete not existing statistic row with id {Id}.");
            }
            return entity;
        }

        public UserStatistic Insert(UserStatistic entity)
        {
            _logger.LogInformation($"Trying to insert statistic for user {entity.Referrer.Username}");
            var result = _context.UsersStatistics.Add(entity);
            _context.SaveChanges();
            return result.Entity;
        }

        public IEnumerable<UserStatistic> List(Func<UserStatistic, bool>? predicate = null, int skip = 0, string? lastId = null, int take = 0)
        {
            _logger.LogInformation($"Trying to fetch statistic.\nPredicate is null: {predicate == null}\nTake: {take}\nSkip:{skip}\nPrevious last id: {lastId}");
            var result = predicate != null ? _context.UsersStatistics.Where(predicate) : _context.UsersStatistics;
            if (skip != default) result.Skip(skip);
            if (lastId != default) result.SkipWhile(entity => entity.Id != lastId).Skip(1);
            if (take != default) result.Take(take);
            _logger.LogInformation($"Rows fetched. Count: {result.Count()}");
            return result;
        }

        public UserStatistic? Single(Func<UserStatistic, bool>? predicate = null)
        {
            _logger.LogInformation($"Trying to fetch statistic.\nPredicate is null: {predicate == null}");
            var elements = _context.UsersStatistics;
            var result = predicate != null ? _context.UsersStatistics.SingleOrDefault(predicate) : _context.UsersStatistics.SingleOrDefault();
            if (result != null)
                _logger.LogInformation($"Statistic row with id: {result?.Id.ToString() ?? "null"} fetched.");
            else
                _logger.LogInformation($"Statistic row doesn't exists.");
            return result;
        }

        public UserStatistic? Update(string Id, UserStatistic entity)
        {
            _logger.LogInformation($"Trying to update user with id {Id}");
            var ent = _context.UsersStatistics.Where(x => x.Id == Id).SingleOrDefault();
            if (ent != null)
            {
                ent.Id = Id;
                _context.UsersStatistics.Update(ent);
                _logger.LogInformation($"User with id {Id} was updated.");
                _context.SaveChanges();
            }
            else
            {
                _logger.LogWarning($"Trying to update not existing user with id {Id}.");
            }
            return ent;
        }
    }
}
