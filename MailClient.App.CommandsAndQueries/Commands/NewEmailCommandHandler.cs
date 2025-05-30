using MailClient.App.Services.Contracts;
using MailClient.Contracts;
using MailClient.Models;
using MediatR;

namespace MailClient.App.CommandsAndQueries.Commands
{
    public class NewEmailCommandHandler : IRequestHandler<NewEmailCommand>
    {
        private readonly ISmtpClient _smtpClient;
        private readonly IServerCredentialsService _userCredentialService;


        public NewEmailCommandHandler(ISmtpClient smtpClient, IServerCredentialsService userCredentialService)
        {
            _smtpClient = smtpClient;
            _userCredentialService = userCredentialService;
        }
        public async Task Handle(NewEmailCommand request, CancellationToken cancellationToken)
        {
            var credentials = await _userCredentialService.GetCredentialsByServerAsync(request.ServerId, cancellationToken);

            await _smtpClient.ConnectAsync(credentials.SmtpServerAddress, (int)credentials.SmtpPort, cancellationToken);
            await _smtpClient.AuthenticateAsync(credentials.Username, credentials.Password, cancellationToken);

            request.NewEmail.From = credentials.Username;

            var email = new EmailMessageRequest
            {
                From = request.NewEmail.From,
                To = request.NewEmail.To,
                Cc = request.NewEmail.Cc,
                Subject = request.NewEmail.Subject,
                Body = request.NewEmail.Body,
            };

            await _smtpClient.SendMailAsync(email, cancellationToken);
        }
    }
}
