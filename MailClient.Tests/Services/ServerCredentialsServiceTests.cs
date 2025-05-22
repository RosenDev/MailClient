using FluentAssertions;
using MailClient.App.Data;
using MailClient.App.Domain;
using MailClient.App.Models;
using MailClient.App.Services;
using Microsoft.EntityFrameworkCore;

namespace MailClient.Tests.Services
{
    public class ServerCredentialsServiceTests
    {
        private MailClientAppDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<MailClientAppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new MailClientAppDbContext(options);
        }

        [Fact]
        public async Task AddServerAsync_NullModel_ThrowsArgumentNullException()
        {
            var context = CreateContext("AddNull");
            var service = new ServerCredentialsService(context);
            Func<Task> act = () => service.AddServerAsync(null, CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddServerAsync_ValidModel_AddsEntity()
        {
            var context = CreateContext("AddValid");
            var service = new ServerCredentialsService(context);
            var model = new ServerCredentialModel
            {
                ImapServerAddress = "imap",
                ImapPort = 1,
                SmtpServerAddress = "smtp",
                SmtpPort = 2,
                Username = "u",
                Password = "p",
                DisplayName = "d"
            };
            await service.AddServerAsync(model, CancellationToken.None);
            var entity = await context.ServerCredentials.SingleAsync();
            entity.ImapServerAddress.Should().Be("imap");
            entity.DisplayName.Should().Be("d");
        }

        [Fact]
        public async Task DeleteServerAsync_Nonexistent_ThrowsKeyNotFoundException()
        {
            var context = CreateContext("DelNon");
            var service = new ServerCredentialsService(context);
            Func<Task> act = () => service.DeleteServerAsync(42, CancellationToken.None);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Server with ID 42 not found.*");
        }

        [Fact]
        public async Task DeleteServerAsync_Existing_RemovesEntity()
        {
            var context = CreateContext("DelExist");
            context.ServerCredentials.Add(new ServerCredential
            {
                ImapServerAddress = "imap",
                ImapPort = 1,
                SmtpServerAddress = "smtp",
                SmtpPort = 2,
                Username = "u",
                Password = "p",
                DisplayName = "d"
            });
            await context.SaveChangesAsync();
            var service = new ServerCredentialsService(context);
            await service.DeleteServerAsync(1, CancellationToken.None);
            context.ServerCredentials.Any(e => e.Id == 1).Should().BeFalse();
        }

        [Fact]
        public async Task FetchServersAsync_Empty_ReturnsEmptyList()
        {
            var context = CreateContext("FetchEmpty");
            var service = new ServerCredentialsService(context);
            var result = await service.FetchServersAsync(CancellationToken.None);
            result.Servers.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchServersAsync_WithEntities_ReturnsSortedList()
        {
            var context = CreateContext("FetchSorted");
            context.ServerCredentials.Add(new ServerCredential
            {
                Id = 1,
                ImapServerAddress = "imap",
                ImapPort = 1,
                SmtpServerAddress = "smtp",
                SmtpPort = 2,
                Username = "u",
                Password = "p",
                DisplayName = "B"
            });
            context.ServerCredentials.Add(new ServerCredential
            {
                Id = 2,
                ImapServerAddress = "imap",
                ImapPort = 1,
                SmtpServerAddress = "smtp",
                SmtpPort = 2,
                Username = "u",
                Password = "p",
                DisplayName = "A"
            });
            await context.SaveChangesAsync();
            var service = new ServerCredentialsService(context);
            var result = await service.FetchServersAsync(CancellationToken.None);
            result.Servers.Select(s => s.DisplayName).Should().ContainInOrder("A", "B");
        }

        [Fact]
        public async Task GetCredentialsByServerAsync_Nonexistent_ThrowsKeyNotFoundException()
        {
            var context = CreateContext("GetNon");
            var service = new ServerCredentialsService(context);
            Func<Task> act = () => service.GetCredentialsByServerAsync(10, CancellationToken.None);
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Server credentials for ID 10 not found.*");
        }

        [Fact]
        public async Task GetCredentialsByServerAsync_Existing_ReturnsModel()
        {
            var context = CreateContext("GetExist");
            context.ServerCredentials.Add(new ServerCredential
            {
                Id = 5,
                ImapServerAddress = "i",
                ImapPort = 3,
                SmtpServerAddress = "s",
                SmtpPort = 4,
                Username = "u",
                Password = "p",
                DisplayName = "d"
            });
            await context.SaveChangesAsync();
            var service = new ServerCredentialsService(context);
            var result = await service.GetCredentialsByServerAsync(5, CancellationToken.None);
            result.ImapServerAddress.Should().Be("i");
            result.DisplayName.Should().Be("d");
        }
    }
}
