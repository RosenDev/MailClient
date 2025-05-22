using MailClient.App.Services.Contracts;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class AddServerCommandHandler : IRequestHandler<AddServerCommand>
    {
        private readonly IServerCredentialsService _serverCredentialsService;

        public AddServerCommandHandler(IServerCredentialsService serverCredentialsService)
        {
            _serverCredentialsService = serverCredentialsService;
        }
        public async Task Handle(AddServerCommand request, CancellationToken cancellationToken)
        {
            await _serverCredentialsService.AddServerAsync(request.ServerCredentials, cancellationToken);
        }
    }
}
