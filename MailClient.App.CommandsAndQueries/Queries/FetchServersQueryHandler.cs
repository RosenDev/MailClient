using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Queries
{
    public class FetchServersQueryHandler : IRequestHandler<FetchServersQuery, ServerListModel>
    {
        private readonly IServerCredentialsService _userCredentialService;

        public FetchServersQueryHandler(IServerCredentialsService userCredentialService)
        {
            _userCredentialService = userCredentialService;
        }

        public async Task<ServerListModel> Handle(FetchServersQuery request, CancellationToken cancellationToken)
        {
            return await _userCredentialService.FetchServersAsync(cancellationToken);
        }
    }
}
