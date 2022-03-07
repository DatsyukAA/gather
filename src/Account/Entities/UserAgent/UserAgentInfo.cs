using Account.Data;
using UAParser;

namespace Account.Entities.UserAgent
{
    public class UserAgentInfo : Entity
    {
        public UserAgentInfo() { }
        public UserAgentInfo(string uaString)
        {
            var uaParser = Parser.GetDefault();
            ClientInfo c = uaParser.Parse(uaString);

            Device = new Device
            {
                Brand = c.Device.Brand,
                Family = c.Device.Family,
                Model = c.Device.Model,
                IsSpider = c.Device.IsSpider
            };

            UserAgent = new UserAgent
            {
                Family = c.UA.Family,
                Patch = c.UA.Patch,
                Major = c.UA.Major,
                Minor = c.UA.Minor
            };

            OS = new OS
            {
                Family = c.OS.Family,
                Patch = c.OS.Patch,
                Major = c.OS.Major,
                Minor = c.OS.Minor,
                PatchMinor = c.OS.PatchMinor
            };
        }

        public Device Device { get; set; } = new Device();
        public UserAgent UserAgent { get; set; } = new UserAgent();
        public OS OS { get; set; } = new OS();
    }
}