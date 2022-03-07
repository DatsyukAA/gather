namespace Account.Middlewares
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseUserStatisticMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<UserStatisticMiddleware>();
        }
    }
}
