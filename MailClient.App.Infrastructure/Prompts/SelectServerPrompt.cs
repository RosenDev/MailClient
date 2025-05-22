using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.Prompts
{
    /// <summary>
    /// Prompts the user to select an existing server credential by display name,
    /// or cancel to return to the main menu.
    /// </summary>
    public class SelectServerPrompt : IAsyncPrompt<ServerModel>
    {
        private readonly IServerCredentialsService _serverCredentialsService;
        private readonly IInputReaderService _inputReaderService;
        private readonly IOutputWriterService _outputWriterService;

        public SelectServerPrompt(
            IServerCredentialsService serverCredentialsService,
            IInputReaderService inputReaderService,
            IOutputWriterService outputWriterService)
        {
            _serverCredentialsService = serverCredentialsService;
            _inputReaderService = inputReaderService;
            _outputWriterService = outputWriterService;
        }

        public async Task<ServerModel> RunAsync(CancellationToken ct)
        {
            var saved = await _serverCredentialsService.FetchServersAsync(ct);
            var list = saved.Servers;

            _outputWriterService.WriteLine("Type 'Cancel' to return to the main menu.");
            while(!ct.IsCancellationRequested)
            {
                _outputWriterService.WriteLine("Saved servers:");
                foreach(var creds in list)
                {
                    _outputWriterService.WriteLine($"  {creds.Id}. {creds.DisplayName}");
                }
                _outputWriterService.Write("Enter the number of the server to select: ");

                var input = _inputReaderService.ReadLine()?.Trim() ?? string.Empty;
                if(IsCancel(input))
                    throw new OperationCanceledException();

                if(int.TryParse(input, out int id))
                {
                    var match = list.FirstOrDefault(s => s.Id == id);
                    if(match != null)
                        return match;
                }

                _outputWriterService.WriteLine("Invalid selection. Please enter a valid number or 'Cancel'.");
            }

            throw new OperationCanceledException(ct);
        }

        private bool IsCancel(string input)
            => string.Equals(input, "Cancel", StringComparison.OrdinalIgnoreCase);
    }
}
