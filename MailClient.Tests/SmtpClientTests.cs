using System.Text;
using FluentAssertions;
using MailClient.Constants;
using MailClient.Contracts;
using MailClient.Tests.Common;
using Moq;
using Serilog;

namespace MailClient.Tests
{
    public class SmtpClientTests
    {
        private const string Host = "smtp.example.com";
        private const int Port = 465;
        private const string Username = "user";
        private const string Password = "pass";

        private SmtpClient CreateClientWithStreams(TrackingReader reader, TrackingWriter writer, Mock<ILogger> loggerMock)
        {
            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(Host, Port, It.IsAny<CancellationToken>()))
                .ReturnsAsync((reader, writer));

            var client = new SmtpClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 5
            };
            return client;
        }

        [Fact]
        public async Task ConnectAsync_LogsGreetingAndSetsStreams()
        {
            var greeting = "220 Service ready";
            var reader = new TrackingReader(new[] { greeting });
            var writer = new TrackingWriter();
            var loggerMock = new Mock<ILogger>();
            var client = CreateClientWithStreams(reader, writer, loggerMock);

            await client.ConnectAsync(Host, Port, CancellationToken.None);

            loggerMock.Verify(l => l.Information(greeting), Times.Once);
            writer.ToString().Should().BeEmpty();
        }

        [Fact]
        public async Task ConnectAsync_FactoryThrows_LogsAndRethrows()
        {
            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Connect failed"));
            var loggerMock = new Mock<ILogger>();
            var client = new SmtpClient(factoryMock.Object, loggerMock.Object);

            Func<Task> act = () => client.ConnectAsync(Host, Port, CancellationToken.None);

            await act.Should().ThrowAsync<IOException>();
            loggerMock.Verify(l => l.Error("Connect failed"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_SendsAuthCommandAndLogsResponse()
        {
            var authResponse = "235 Authentication successful";
            var reader = new TrackingReader(new[] { authResponse });
            var writer = new TrackingWriter();
            var loggerMock = new Mock<ILogger>();
            var client = CreateClientWithStreams(reader, writer, loggerMock);
            await client.ConnectAsync(Host, Port, CancellationToken.None);

            await client.AuthenticateAsync(Username, Password, CancellationToken.None);

            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("\0" + Username + "\0" + Password));
            writer.ToString().Should().Contain($"{SmtpCommands.AuthPlain}{payload}");
            loggerMock.Verify(l => l.Information(authResponse), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_SendsQuitAndDisposesStreams_OnlyOnce()
        {
            var greeting = "220 Ready";
            var reader = new TrackingReader(new[] { greeting, null });
            var writer = new TrackingWriter();
            var loggerMock = new Mock<ILogger>();
            var client = CreateClientWithStreams(reader, writer, loggerMock);

            await client.ConnectAsync(Host, Port, CancellationToken.None);

            await client.DisposeAsync();
            await client.DisposeAsync();


            writer.ToString().Should().Contain(SmtpCommands.Quit);
            reader.IsDisposed.Should().BeTrue();
            writer.IsDisposed.Should().BeTrue();
        }
    }
}
