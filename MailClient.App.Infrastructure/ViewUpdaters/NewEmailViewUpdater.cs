using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.ViewUpdaters
{
    public class NewEmailViewUpdater : IViewUpdater<NewEmailModel>
    {
        private readonly IOutputWriterService _outputWriterService;

        public NewEmailViewUpdater(IOutputWriterService outputWriterService)
        {
            _outputWriterService = outputWriterService;
        }

        public void UpdateView(NewEmailModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));

            _outputWriterService.WriteLine($"Sucessfully sent email");
        }
    }
}
