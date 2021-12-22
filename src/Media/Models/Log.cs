using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Media.Models
{
    public class Log
    {
        private readonly string _name;
        private readonly string _version;
        public Log()
        {
            _name = Assembly.GetExecutingAssembly().GetName().Name ?? "";
            _version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "";
        }
        public string Sender { get => $"{_name}_{_version}"; }
        public DateTime Time { get => DateTime.UtcNow; }
        public int Code { get; set; } = 0;
        public string Message { get; set; } = "";

        public LogLevel LogLevel = LogLevel.Information;
    }
}
