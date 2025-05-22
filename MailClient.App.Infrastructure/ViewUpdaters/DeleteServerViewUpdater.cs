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
        private readonly IOutputWriterService _output;

        public DeleteServerViewUpdater(IOutputWriterService outputWriterService)
        {
            _output = outputWriterService;
        }

        public void UpdateView(DeleteServerModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));
            _output.WriteLine($"Successfully deleted server: {model.Id}");
        }
    }
}
