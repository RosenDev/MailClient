namespace MailClient.Models
{
    public class FetchMessagesRequest
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Mailbox { get; set; } = "INBOX";
    }
}
