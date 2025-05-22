using MailClient.App.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Queries
{
    public class FetchServersQuery : IRequest<ServerListModel>
    {
    }
}
