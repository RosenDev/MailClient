namespace MailClient.App.Domain
{
    public class EntityBase
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
