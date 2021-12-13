using Microsoft.AspNetCore.Mvc;

namespace Account.Utils
{
    public static class WebUtils
    {
        public static string IpAddress(this ControllerBase controller)
        {
            if (controller.Request.Headers.ContainsKey("X-Forwarded-For"))
                return controller.Request.Headers["X-Forwarded-For"];
            else
                return controller?.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString() ?? "";
        }

        public static void setCookie(this ControllerBase controller, string name, string value, DateTime? expires = default)
        {
            if (expires == null) expires = DateTime.UtcNow.AddDays(7);
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires
            };
            controller.Response.Cookies.Append(name, value, cookieOptions);
        }
    }
}