using MailClient.App.Models;

namespace MailClient.App.Services.Contracts
{
    /// <summary>
    /// Defines methods for persisting and retrieving raw email messages from storage.
    /// </summary>
    public interface IEmailStoreService
    {
        /// <summary>
        /// Persists a batch of raw emails to storage.
        /// </summary>
        /// <param name="emails">
        /// A <see cref="EmailRawListModel"/> containing the server address and the list of
        /// <see cref="EmailRawModel"/> instances to store.
        /// </param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        Task StoreEmailsAsync(EmailRawListModel emails, CancellationToken ct);

        /// <summary>
        /// Retrieves all raw emails for the specified server from storage.
        /// </summary>
        /// <param name="serverName">The identifier of the server whose emails to fetch.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>
        /// An <see cref="EmailRawListModel"/> containing the server address and the list of
        /// <see cref="EmailRawModel"/> instances retrieved.
        /// </returns>
        Task<EmailRawListModel> GetEmailsAsync(string serverName, CancellationToken ct);

        /// <summary>
        /// Retrieves a single raw email by its UID from storage.
        /// </summary>
        /// <param name="serverName">The identifier of the server where the email is stored.</param>
        /// <param name="emailId">The UID of the email to retrieve.</param>
        /// <param name="ct">A cancellation token to cancel the operation.</param>
        /// <returns>An <see cref="EmailRawModel"/> containing the UID and raw content of the email.</returns>
        Task<EmailRawModel> GetEmailByIdAsync(string serverName, string emailId, CancellationToken ct);
    }
}
