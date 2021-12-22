namespace Media.Models
{
    public class Message
    {
        public string Id { get; set; }
        public string MediaService { get; set; }
        public string Channel { get; set; }
        public string Text { get; set; }
        public string Sender { get; set; }
        public string Target { get; set; }
        public string Attachments { get; set; }
    }
}
