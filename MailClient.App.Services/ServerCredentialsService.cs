using MailClient.App.Data;
using MailClient.App.Domain;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MailClient.App.Services
{
    /// <summary>
    /// Provides CRUD operations over stored server credentials in the application database.
    /// </summary>
    public class ServerCredentialsService : IServerCredentialsService
    {
        private readonly MailClientAppDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of <see cref="ServerCredentialsService"/>.
        /// </summary>
        /// <param name="dbContext">The EF Core <see cref="MailClientAppDbContext"/> to use.</param>
        public ServerCredentialsService(MailClientAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new server credential record to the database.
        /// </summary>
        /// <param name="model">The <see cref="ServerCredentialModel"/> containing the server details to store.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A task that completes when the record has been saved.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="model"/> is null.</exception>
        public async Task AddServerAsync(ServerCredentialModel model, CancellationToken ct)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model));

            var entity = new ServerCredential
            {
                ImapServerAddress = model.ImapServerAddress,
                ImapPort = model.ImapPort,
                SmtpServerAddress = model.SmtpServerAddress,
                SmtpPort = model.SmtpPort,
                Username = model.Username,
                Password = model.Password,
                DisplayName = model.DisplayName
            };

            _dbContext.ServerCredentials.Add(entity);
            await _dbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Deletes an existing server credential record by its identifier.
        /// </summary>
        /// <param name="serverId">The unique identifier of the server credential to delete.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>A task that completes when the record has been removed.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if no <see cref="ServerCredential"/> with the given <paramref name="serverId"/> exists.
        /// </exception>
        public async Task DeleteServerAsync(int serverId, CancellationToken ct)
        {
            var entity = await _dbContext.ServerCredentials.FindAsync(new object[] { serverId }, ct);
            if(entity == null)
                throw new KeyNotFoundException($"Server with ID {serverId} not found.");

            _dbContext.ServerCredentials.Remove(entity);
            await _dbContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// Retrieves all stored server credentials.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> to cancel the operation.</param>
        /// <returns>
        /// A <see cref="ServerListModel"/> containing a list of <see cref="ServerModel"/> items,
        /// each representing one saved server credential.
        /// </returns>
        public async Task<ServerListModel> FetchServersAsync(CancellationToken ct)
        {
            var entities = await _dbContext.ServerCredentials
                .AsNoTracking()
                .OrderBy(x => x.DisplayName)
                .ToListAsync(ct);

            return new ServerListModel
            {
                Servers = entities
                    .Select(x => new ServerModel
                    {
                        Id = x.Id,
                        DisplayName = x.DisplayName
                    })
                    .ToList()
            };
        }

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
        public async Task<ServerCredentialModel> GetCredentialsByServerAsync(int serverId, CancellationToken ct)
        {
            var entity = await _dbContext.ServerCredentials
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == serverId, ct);

            if(entity == null)
                throw new KeyNotFoundException($"Server credentials for ID {serverId} not found.");

            return new ServerCredentialModel
            {
                ImapServerAddress = entity.ImapServerAddress,
                ImapPort = entity.ImapPort,
                SmtpServerAddress = entity.SmtpServerAddress,
                SmtpPort = entity.SmtpPort,
                Username = entity.Username,
                Password = entity.Password,
                DisplayName = entity.DisplayName
            };
        }
    }
}
