using MailClient.App.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class AddServerCommand : IRequest
    {
        public ServerCredentialModel ServerCredentials { get; set; }
    }
}
