using System.Text;
using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.Prompts
{
    /// <summary>
    /// Prompts for composing a new email. Validates input and supports 'Cancel' to abort.
    /// </summary>
    public class NewEmailPrompt : IPrompt<NewEmailModel>
    {
        private readonly IInputReaderService _inputReaderService;
        private readonly IOutputWriterService _outputWriterService;

        public NewEmailPrompt(
            IInputReaderService inputReaderService,
            IOutputWriterService outputWriterService)
        {
            _inputReaderService = inputReaderService;
            _outputWriterService = outputWriterService;
        }

        public NewEmailModel Run()
        {
            _outputWriterService.WriteLine("Type 'Cancel' at any prompt to return to the main menu.");

            var to = PromptAddresses("To (semicolon-separated): ", true);

            var cc = PromptAddresses("Cc (semicolon-separated, optional): ", false);

            var subject = PromptNonEmpty("Subject: ");

            _outputWriterService.WriteLine("Body (end with a single dot on its own line):");
            var body = ReadMultiline();

            return new NewEmailModel
            {
                To = to,
                Cc = cc,
                Subject = subject,
                Body = body
            };
        }

        private List<string> PromptAddresses(string prompt, bool requireAtLeastOne)
        {
            while(true)
            {
                _outputWriterService.Write(prompt);
                var input = _inputReaderService.ReadLine()?.Trim() ?? string.Empty;
                if(IsCancel(input))
                    throw new OperationCanceledException();

                var list = ParseAddresses(input);
                if(!requireAtLeastOne || list.Count > 0)
                    return list;

                _outputWriterService.WriteLine("Please enter at least one address or type 'Cancel'.");
            }
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
                _outputWriterService.WriteLine("Input cannot be empty. Please try again or type 'Cancel'.");
            }
        }

        private string ReadMultiline()
        {
            var sb = new StringBuilder();
            while(true)
            {
                var line = _inputReaderService.ReadLine();
                if(line == null || line == ".")
                    break;
                if(IsCancel(line.Trim()))
                    throw new OperationCanceledException();
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        private List<string> ParseAddresses(string input)
        {
            var list = new List<string>();
            foreach(var addr in input.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = addr.Trim();
                if(!string.IsNullOrWhiteSpace(trimmed))
                    list.Add(trimmed);
            }
            return list;
        }

        private bool IsCancel(string input)
            => string.Equals(input, "Cancel", StringComparison.OrdinalIgnoreCase);
    }
}
