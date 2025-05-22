namespace MailClient.App.Models
{
    public class EmailDetailsModel
    {
        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public List<string> Subject { get; set; }
        public List<string> Body { get; set; }
    }
}
