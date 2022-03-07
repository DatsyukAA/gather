namespace Account.Models
{
    public class Message
    {
        public string Id { get; set; } = string.Empty;
        public string MediaService { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Attachments { get; set; } = string.Empty;
    }
}
