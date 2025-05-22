using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.ViewUpdaters
{
    /// <summary>
    /// After a server is selected, writes the selection to the view.
    /// </summary>
    public class SelectServerViewUpdater : IViewUpdater<ServerModel>
    {
        private readonly IOutputWriterService _outputWriterService;

        public SelectServerViewUpdater(IOutputWriterService outputWriterService)
        {
            _outputWriterService = outputWriterService;
        }

        public void UpdateView(ServerModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));

            _outputWriterService.WriteLine($"Selected server: {model.DisplayName}");
        }
    }
}
