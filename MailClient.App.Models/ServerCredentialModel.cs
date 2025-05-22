namespace MailClient.App.Models
{
    public class ServerCredentialModel
    {
        public string DisplayName { get; set; }
        public string ImapServerAddress { get; set; }
        public uint ImapPort { get; set; }
        public string SmptServerAddress { get; set; }
        public uint SmtpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
