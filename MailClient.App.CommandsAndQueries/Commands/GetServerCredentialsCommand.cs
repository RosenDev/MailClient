using MailClient.App.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class GetServerCredentialsCommand : IRequest<ServerCredentialModel>
    {
        public int Id { get; set; }
    }
}
