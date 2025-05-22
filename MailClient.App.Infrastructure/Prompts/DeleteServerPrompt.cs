using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.Prompts
{
    /// <summary>
    /// Prompts the user to select an existing server credential to delete,
    /// or cancel to return to the main menu.
    /// </summary>
    public class DeleteServerPrompt : IAsyncPrompt<DeleteServerModel>
    {
        private readonly IServerCredentialsService _serverCredentialsService;
        private readonly IInputReaderService _input;
        private readonly IOutputWriterService _output;

        public DeleteServerPrompt(
            IServerCredentialsService serverCredentialsService,
            IInputReaderService inputReaderService,
            IOutputWriterService outputWriterService)
        {
            _serverCredentialsService = serverCredentialsService;
            _input = inputReaderService;
            _output = outputWriterService;
        }

        public async Task<DeleteServerModel> RunAsync(CancellationToken ct)
        {
            var saved = await _serverCredentialsService.FetchServersAsync(ct);
            var list = saved.Servers;

            _output.WriteLine("Type 'Cancel' to return to the main menu.");
            while(!ct.IsCancellationRequested)
            {
                _output.WriteLine("Saved servers:");
                foreach(var creds in list)
                {
                    _output.WriteLine($"  {creds.Id}. {creds.DisplayName}");
                }
                _output.Write("Enter the number of the server to delete: ");

                var input = _input.ReadLine()?.Trim() ?? string.Empty;
                if(IsCancel(input))
                    throw new OperationCanceledException();

                if(int.TryParse(input, out int id))
                {
                    var match = list.FirstOrDefault(s => s.Id == id);
                    if(match != null)
                        return new DeleteServerModel { Id = match.Id };
                }

                _output.WriteLine("Invalid selection. Please enter a valid number or 'Cancel'.");
            }

            throw new OperationCanceledException(ct);
        }

        private bool IsCancel(string input)
            => string.Equals(input, "Cancel", StringComparison.OrdinalIgnoreCase);
    }
}
