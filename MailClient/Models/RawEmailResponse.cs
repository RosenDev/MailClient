namespace MailClient.Models
{
    public class RawEmailResponse
    {
        public string Uid { get; }
        public string RawContent { get; }

        public RawEmailResponse(string uid, string rawContent)
        {
            Uid = uid;
            RawContent = rawContent;
        }
    }
}
