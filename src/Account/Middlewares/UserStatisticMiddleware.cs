using Account.Data;
using Account.Entities;
using System.Security.Claims;

namespace Account.Middlewares
{
    public class UserStatisticMiddleware : IMiddleware
    {
        private readonly IRepository<UserStatistic> _userStatisticRepository;
        private readonly IRepository<User> _userRepository;

        public UserStatisticMiddleware(IRepository<UserStatistic> userStatisticRepository, IRepository<User> userRepository)
        {
            _userStatisticRepository = userStatisticRepository;
            _userRepository = userRepository;
        }
        public string IpAddress(HttpContext context)
        {
            return context.Request.Headers["X-Forwarded-For"].ToString() ?? (context.Connection?.RemoteIpAddress?.MapToIPv4().ToString() ?? "");
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            string? userId = context.User.Claims.FirstOrDefault(element => element.Type == ClaimTypes.Name)?.Value;
            if (userId == null)
            {
                await next(context);
                return;
            }
            User? user = _userRepository.Single(element => element.Id == userId);
            if (user == null)
            {
                await next(context);
                return;
            }
            UserStatistic? statistic = _userStatisticRepository.Single(element => element.Referrer.Id == userId);

            if (statistic == null)
                _userStatisticRepository.Insert(new()
                {
                    Referrer = user,
                    IpHistory = new List<IPHistory> {
                        new(){
                            Ip = IpAddress(context)
                        }
                    },
                });
            else
            {
                statistic.IpHistory.Add(new IPHistory
                {
                    Ip = IpAddress(context)
                });
                _userStatisticRepository.Update(statistic.Id, statistic);
            }
            await next(context);
        }
    }
}
