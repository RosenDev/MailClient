using FluentAssertions;
using MailClient.App.Models;
using MailClient.App.Services;
using MailClient.App.Services.Constants;

namespace MailClient.Tests.Services
{
    public class EmailStoreServiceTests : IDisposable
    {
        private readonly string _appDataRoot;
        private readonly string _storeRoot;

        public EmailStoreServiceTests()
        {
            _appDataRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_appDataRoot);
            Environment.SetEnvironmentVariable("APPDATA", _appDataRoot);

            _storeRoot = Path.Combine(
                _appDataRoot,
                StorageConstants.AppFolderName);
        }

        public void Dispose()
        {
            if(Directory.Exists(_appDataRoot))
                Directory.Delete(_appDataRoot, true);
        }

        private EmailStoreService CreateService() => new EmailStoreService();

        [Fact]
        public async Task GetEmailByIdAsync_NullOrWhitespaceId_ThrowsArgumentException()
        {
            var svc = CreateService();
            Func<Task> act1 = () => svc.GetEmailByIdAsync("server", null, CancellationToken.None);
            Func<Task> act2 = () => svc.GetEmailByIdAsync("server", "  ", CancellationToken.None);

            await act1.Should().ThrowAsync<ArgumentException>().WithParameterName("emailId");
            await act2.Should().ThrowAsync<ArgumentException>().WithParameterName("emailId");
        }

        [Fact]
        public async Task GetEmailByIdAsync_NonexistentDirectory_ThrowsFileNotFoundException()
        {
            var svc = CreateService();
            await svc.Invoking(s => s.GetEmailByIdAsync("noserver", "id", CancellationToken.None))
                .Should().ThrowAsync<FileNotFoundException>()
                .WithMessage("No emails folder for server*");
        }

        [Fact]
        public async Task GetEmailByIdAsync_FileNotFound_ThrowsFileNotFoundException()
        {
            var server = "srv";
            var dir = Path.Combine(_storeRoot, server, StorageConstants.EmailsSubFolder);
            Directory.CreateDirectory(dir);

            var svc = CreateService();

            await svc.Invoking(s => s.GetEmailByIdAsync(server, "unknown", CancellationToken.None))
                .Should().ThrowAsync<FileNotFoundException>()
                .WithMessage("Email file not found: unknown.eml*");
        }

        [Fact]
        public async Task GetEmailsAsync_NonexistentDirectory_ReturnsEmptyList()
        {
            var server = "noemailsrv";
            var svc = CreateService();

            var result = await svc.GetEmailsAsync(server, CancellationToken.None);

            result.ServerAddress.Should().Be(server);
            result.RawEmails.Should().BeEmpty();
        }


        [Fact]
        public async Task StoreEmailsAsync_NullModel_ThrowsArgumentNullException()
        {
            var svc = CreateService();
            Func<Task> act = () => svc.StoreEmailsAsync(null, CancellationToken.None);
            await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("emails");
        }

        [Fact]
        public async Task StoreEmailsAsync_NullOrWhitespaceServer_ThrowsArgumentException()
        {
            var svc = CreateService();
            var model1 = new EmailRawListModel { ServerAddress = null, RawEmails = new() { new EmailRawModel { Uid = "u", RawContent = "c" } } };
            var model2 = new EmailRawListModel { ServerAddress = " ", RawEmails = new() { new EmailRawModel { Uid = "u", RawContent = "c" } } };

            await svc.Invoking(s => s.StoreEmailsAsync(model1, CancellationToken.None))
                .Should().ThrowAsync<ArgumentException>().WithParameterName("emails");
            await svc.Invoking(s => s.StoreEmailsAsync(model2, CancellationToken.None))
                .Should().ThrowAsync<ArgumentException>().WithParameterName("emails");
        }

        [Fact]
        public async Task StoreEmailsAsync_InvalidEmailUid_ThrowsArgumentException()
        {
            var svc = CreateService();
            var model = new EmailRawListModel
            {
                ServerAddress = "srv",
                RawEmails = new() { new EmailRawModel { Uid = "", RawContent = "c" } }
            };

            await svc.Invoking(s => s.StoreEmailsAsync(model, CancellationToken.None))
                .Should().ThrowAsync<ArgumentException>().WithMessage("Email UID cannot be null or empty*");
        }

        [Fact]
        public async Task StoreEmailsAsync_NullRawContent_ThrowsArgumentException()
        {
            var svc = CreateService();
            var model = new EmailRawListModel
            {
                ServerAddress = "srv",
                RawEmails = new() { new EmailRawModel { Uid = "u", RawContent = null } }
            };

            await svc.Invoking(s => s.StoreEmailsAsync(model, CancellationToken.None))
                .Should().ThrowAsync<ArgumentException>().WithMessage("RawContent cannot be null*");
        }
    }
}