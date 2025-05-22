using MailClient.App.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Queries
{
    public class FetchEmailsQuery : IRequest<EmailListModel>
    {
        public int ServerId { get; set; }
    }
}
