using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class DeleteServerCommand : IRequest
    {
        public int ServerId { get; set; }
    }

}
