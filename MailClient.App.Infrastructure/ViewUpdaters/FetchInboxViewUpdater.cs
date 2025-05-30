using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.ViewUpdaters
{
    public class FetchInboxViewUpdater : IViewUpdater<EmailListModel>
    {
        private readonly IOutputWriterService _outputWriterService;

        public FetchInboxViewUpdater(IOutputWriterService outputWriterService)
        {
            _outputWriterService = outputWriterService;
        }

        public void UpdateView(EmailListModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));

            _outputWriterService.WriteLine($"Inbox content:");
            _outputWriterService.WriteLine("----------------------------------------");

            var emails = model.Emails;
            if(emails == null || emails.Count == 0)
            {
                _outputWriterService.WriteLine("(No messages)");
                return;
            }

            for(int i = 0; i < emails.Count; i++)
            {
                var email = emails[i];
                _outputWriterService.WriteLine($"{i + 1}.");
                _outputWriterService.WriteLine($"  From: {email.From}");
                _outputWriterService.WriteLine($"  To: {string.Join(", ", email.To)}");

                if(email.Cc != null && email.Cc.Count > 0)
                    _outputWriterService.WriteLine($"  Cc: {string.Join(", ", email.Cc)}");

                _outputWriterService.WriteLine($"  Subject: {email.Subject}");
                //_outputWriterService.WriteLine();
                //_outputWriterService.WriteLine(email.Body);
                //_outputWriterService.WriteLine();
            }
        }
    }
}
