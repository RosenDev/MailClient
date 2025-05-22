using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.Prompts
{
    /// <summary>
    /// Prompts the user to enter a new IMAP and SMTP server configuration including credentials.
    /// Validates input and allows cancellation by typing 'Cancel'.
    /// </summary>
    public class AddServerPrompt : IPrompt<ServerCredentialModel>
    {
        private readonly IInputReaderService _inputReaderService;
        private readonly IOutputWriterService _outputWriterService;

        public AddServerPrompt(
            IInputReaderService inputReaderService,
            IOutputWriterService outputWriterService)
        {
            _inputReaderService = inputReaderService;
            _outputWriterService = outputWriterService;
        }

        public ServerCredentialModel Run()
        {
            _outputWriterService.WriteLine("Type 'Cancel' at any prompt to return to the main menu.");

            var imapAddress = PromptNonEmpty("IMAP Server address (e.g. imap.example.com): ");

            var imapPort = PromptPort("IMAP Port [993]: ", 993);

            var smtpAddress = PromptNonEmpty("SMTP Server address (e.g. smtp.example.com): ");

            var smtpPort = PromptPort("SMTP Port [465]: ", 465);

            var username = PromptNonEmpty("Username: ");

            _outputWriterService.Write("Password: ");
            var password = _inputReaderService.ReadSecureField() ?? string.Empty;
            if(IsCancel(password)) throw new OperationCanceledException();
            if(string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");
            _outputWriterService.WriteLine();

            _outputWriterService.Write("Display name (optional): ");
            var displayNameInput = _inputReaderService.ReadLine()?.Trim() ?? string.Empty;
            if(IsCancel(displayNameInput)) throw new OperationCanceledException();
            var displayName = string.IsNullOrWhiteSpace(displayNameInput) ? imapAddress : displayNameInput;

            return new ServerCredentialModel
            {
                ImapServerAddress = imapAddress,
                ImapPort = imapPort,
                SmtpServerAddress = smtpAddress,
                SmtpPort = smtpPort,
                DisplayName = displayName,
                Username = username,
                Password = password
            };
        }

        private string PromptNonEmpty(string prompt)
        {
            while(true)
            {
                _outputWriterService.Write(prompt);
                var input = _inputReaderService.ReadLine()?.Trim() ?? string.Empty;
                if(IsCancel(input))
                    throw new OperationCanceledException();
                if(!string.IsNullOrWhiteSpace(input))
                    return input;
                _outputWriterService.WriteLine("Input cannot be empty. Please try again.");
            }
        }

        private uint PromptPort(string prompt, uint defaultPort)
        {
            while(true)
            {
                _outputWriterService.Write(prompt);
                var input = _inputReaderService.ReadLine()?.Trim();
                if(IsCancel(input))
                    throw new OperationCanceledException();
                if(string.IsNullOrEmpty(input))
                    return defaultPort;
                if(uint.TryParse(input, out var port) && port > 0)
                    return port;
                _outputWriterService.WriteLine("Invalid port. Please enter a valid number or press Enter for default.");
            }
        }

        private bool IsCancel(string input)
            => string.Equals(input, "Cancel", StringComparison.OrdinalIgnoreCase);
    }
}
