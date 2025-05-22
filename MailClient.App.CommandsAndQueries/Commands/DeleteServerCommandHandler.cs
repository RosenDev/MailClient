using MailClient.App.Services.Contracts;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class DeleteServerCommandHandler : IRequestHandler<DeleteServerCommand>
    {
        private readonly IServerCredentialsService _serverCredentialsService;

        public DeleteServerCommandHandler(IServerCredentialsService serverCredentialsService)
        {
            _serverCredentialsService = serverCredentialsService;
        }
        public async Task Handle(DeleteServerCommand request, CancellationToken cancellationToken)
        {
            await _serverCredentialsService.DeleteServerAsync(request.ServerId, cancellationToken);
        }
    }
}