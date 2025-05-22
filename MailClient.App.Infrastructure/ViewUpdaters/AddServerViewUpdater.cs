using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.ViewUpdaters
{
    /// <summary>
    /// Updates the view after adding a new server.
    /// </summary>
    public class AddServerViewUpdater : IViewUpdater<ServerCredentialModel>
    {
        private readonly IOutputWriterService _outputWriterService;

        public AddServerViewUpdater(IOutputWriterService outputWriterService)
        {
            _outputWriterService = outputWriterService;
        }

        public void UpdateView(ServerCredentialModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            _outputWriterService.WriteLine($"Successfully added server: {model.DisplayName}");
        }
    }
}
