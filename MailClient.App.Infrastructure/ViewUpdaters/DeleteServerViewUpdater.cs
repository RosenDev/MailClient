using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.ViewUpdaters
{
    /// <summary>
    /// Updates the view after deleting a server.
    /// </summary>
    public class DeleteServerViewUpdater : IViewUpdater<DeleteServerModel>
    {
        private readonly IOutputWriterService _outputWriterService;

        public DeleteServerViewUpdater(IOutputWriterService outputWriterService)
        {
            _outputWriterService = outputWriterService;
        }

        public void UpdateView(DeleteServerModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            _outputWriterService.WriteLine($"Successfully deleted server: {model.Id}");
        }
    }
}
