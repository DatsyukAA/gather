using Account.Data;

namespace Account.Entities.UserAgent
{
    public class UserAgent : Entity
    {
        public string? Family { get; set; }
        public string? Major { get; set; }
        public string? Minor { get; set; }
        public string? Patch { get; set; }
    }
}