using MailClient.App.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class NewEmailCommand : IRequest
    {
        public NewEmailModel NewEmail { get; set; }

        public int ServerId { get; set; }
    }
}
