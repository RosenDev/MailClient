using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Infrastructure.ViewUpdaters
{
    public class FetchInboxViewUpdater : IViewUpdater<EmailListModel>
    {
        private readonly IOutputWriterService _output;

        public FetchInboxViewUpdater(IOutputWriterService outputWriterService)
        {
            _output = outputWriterService;
        }

        public void UpdateView(EmailListModel model)
        {
            if(model == null) throw new ArgumentNullException(nameof(model));

            _output.WriteLine($"Inbox content:");
            _output.WriteLine("----------------------------------------");

            var emails = model.Emails;
            if(emails == null || emails.Count == 0)
            {
                _output.WriteLine("(No messages)");
                return;
            }

            for(int i = 0; i < emails.Count; i++)
            {
                var email = emails[i];
                _output.WriteLine($"{i + 1}.");
                _output.WriteLine($"  From: {email.From}");
                _output.WriteLine($"  To: {string.Join(", ", email.To)}");

                if(email.Cc != null && email.Cc.Count > 0)
                    _output.WriteLine($"  Cc: {string.Join(", ", email.Cc)}");

                _output.WriteLine($"  Subject: {email.Subject}");
                _output.WriteLine();
            }
        }
    }
}
