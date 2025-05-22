using MailClient.App.Models;
using MailClient.App.Services.Constants;
using MailClient.App.Services.Contracts;

namespace MailClient.App.Services
{
    /// <summary>
    /// Service for storing and retrieving raw email files under
    /// %APPDATA%\<see cref="StorageConstants.AppFolderName"/>\<ServerAddress>\Emails\*.eml.
    /// </summary>
    public class EmailStoreService : IEmailStoreService
    {
        private string GetServerEmailDirectory(string serverName)
        {
            if(string.IsNullOrWhiteSpace(serverName))
                throw new ArgumentException("Server name must be provided", nameof(serverName));

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(
                appData,
                StorageConstants.AppFolderName,
                serverName,
                StorageConstants.EmailsSubFolder);
        }

        /// <summary>
        /// Retrieves and returns a single raw email by its UID from the store.
        /// </summary>
        /// <param name="serverName">The identifier of the server (folder) where emails are stored.</param>
        /// <param name="emailId">The UID of the email to retrieve.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>An <see cref="EmailRawModel"/> containing the UID and raw content of the email.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="emailId"/> is null or empty.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the email directory or the specific .eml file does not exist.
        /// </exception>
        public async Task<EmailRawModel> GetEmailByIdAsync(string serverName, string emailId, CancellationToken ct)
        {
            if(string.IsNullOrWhiteSpace(emailId))
                throw new ArgumentException("Email ID must be provided", nameof(emailId));

            var dir = GetServerEmailDirectory(serverName);
            if(!Directory.Exists(dir))
                throw new FileNotFoundException("No emails folder for server", dir);

            var filePath = Path.Combine(dir, emailId + ".eml");
            if(!File.Exists(filePath))
                throw new FileNotFoundException($"Email file not found: {emailId}.eml", filePath);

            var raw = await File.ReadAllTextAsync(filePath, ct).ConfigureAwait(false);
            return new EmailRawModel
            {
                Uid = emailId,
                RawContent = raw
            };
        }

        /// <summary>
        /// Retrieves all raw emails stored for the specified server.
        /// </summary>
        /// <param name="serverName">The identifier of the server (folder) where emails are stored.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// An <see cref="EmailRawListModel"/> containing the server name and a list of all
        /// <see cref="EmailRawModel"/> entries (UID + raw content).
        /// </returns>
        public async Task<EmailRawListModel> GetEmailsAsync(string serverName, CancellationToken ct)
        {
            var dir = GetServerEmailDirectory(serverName);
            if(!Directory.Exists(dir))
            {
                return new EmailRawListModel
                {
                    ServerAddress = serverName,
                    RawEmails = new List<EmailRawModel>()
                };
            }

            var files = Directory.GetFiles(dir, "*.eml");
            var emails = new List<EmailRawModel>(files.Length);

            foreach(var file in files)
            {
                ct.ThrowIfCancellationRequested();

                var uid = Path.GetFileNameWithoutExtension(file);
                var raw = await File.ReadAllTextAsync(file, ct).ConfigureAwait(false);

                emails.Add(new EmailRawModel
                {
                    Uid = uid,
                    RawContent = raw
                });
            }

            return new EmailRawListModel
            {
                ServerAddress = serverName,
                RawEmails = emails
            };
        }

        /// <summary>
        /// Stores a batch of raw emails to the filesystem under the server's email directory.
        /// </summary>
        /// <param name="emails">
        /// An <see cref="EmailRawListModel"/> containing the server name and the list of
        /// <see cref="EmailRawModel"/> to persist.
        /// </param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="emails"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="emails.ServerAddress"/> is null or empty,
        /// or if any email UID or RawContent is invalid.</exception>
        public async Task StoreEmailsAsync(EmailRawListModel emails, CancellationToken ct)
        {
            if(emails == null)
                throw new ArgumentNullException(nameof(emails));
            if(string.IsNullOrWhiteSpace(emails.ServerAddress))
                throw new ArgumentException("Server address must be provided", nameof(emails));

            var dir = GetServerEmailDirectory(emails.ServerAddress);
            Directory.CreateDirectory(dir);

            foreach(var email in emails.RawEmails ?? Enumerable.Empty<EmailRawModel>())
            {
                ct.ThrowIfCancellationRequested();

                if(string.IsNullOrWhiteSpace(email.Uid))
                    throw new ArgumentException("Email UID cannot be null or empty", nameof(email.Uid));
                if(email.RawContent == null)
                    throw new ArgumentException("RawContent cannot be null", nameof(email.RawContent));

                var filePath = Path.Combine(dir, email.Uid + ".eml");
                await File.WriteAllTextAsync(filePath, email.RawContent, ct).ConfigureAwait(false);
            }
        }
    }
}
