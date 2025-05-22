using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class GetServerCredentialsCommandHandler : IRequestHandler<GetServerCredentialsCommand, ServerCredentialModel>
    {
        private readonly IServerCredentialsService _serverCredentialsService;

        public GetServerCredentialsCommandHandler(IServerCredentialsService serverCredentialsService)
        {
            _serverCredentialsService = serverCredentialsService;
        }

        public async Task<ServerCredentialModel> Handle(GetServerCredentialsCommand request, CancellationToken cancellationToken)
        {
            return await _serverCredentialsService.GetCredentialsByServerAsync(request.Id, cancellationToken);
        }
    }
}
