using MailClient.App.Models;

namespace MailClient.App.Services.Contracts
{
    /// <summary>
    /// Provides CRUD operations over stored server credentials in the application database.
    /// </summary>
    public interface IServerCredentialsService
    {
        /// <summary>
        /// Adds a new server credential record to the database.
        /// </summary>
        /// <param name="model">The <see cref="ServerCredentialModel"/> containing the server details to store.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A task that completes when the record has been saved.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
        Task AddServerAsync(ServerCredentialModel serverCredentialModel, CancellationToken ct);

        /// <summary>
        /// Deletes an existing server credential record by its identifier.
        /// </summary>
        /// <param name="serverId">The unique identifier of the server credential to delete.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A task that completes when the record has been removed.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no <see cref="ServerCredential"/> with the given <paramref name="serverId"/> exists.
        /// </exception>
        Task DeleteServerAsync(int serverId, CancellationToken ct);

        /// <summary>
        /// Retrieves the full credentials for a specific server by its identifier.
        /// </summary>
        /// <param name="serverId">The unique identifier of the server credential to fetch.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>
        /// A <see cref="ServerCredentialModel"/> containing the saved server's connection
        /// and authentication details.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no credentials with the given <paramref name="serverId"/> exist.
        /// </exception>
        Task<ServerCredentialModel> GetCredentialsByServerAsync(int serverId, CancellationToken ct);

        /// <summary>
        /// Retrieves all stored server credentials.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>
        /// A <see cref="ServerListModel"/> containing a list of <see cref="ServerModel"/> items,
        /// each representing one saved server credential.
        /// </returns>
        Task<ServerListModel> FetchServersAsync(CancellationToken ct);
    }
}
