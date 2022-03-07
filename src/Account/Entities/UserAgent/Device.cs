using Account.Data;

namespace Account.Entities.UserAgent
{
    public class Device : Entity
    {
        public string? Family { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public bool IsSpider { get; set; }
    }
}