namespace MailClient.App.Models
{
    public class EmailRawListModel
    {
        public string ServerAddress { get; set; }
        public List<EmailRawModel> RawEmails { get; set; }
    }
}
