using System.Text;
using FluentAssertions;
using MailClient.Constants;
using MailClient.Contracts;
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

        // Helper classes
        private class TrackingReader : TextReader
        {
            private readonly Queue<string> _lines;
            public bool IsDisposed { get; private set; }
            public TrackingReader(IEnumerable<string> lines) => _lines = new Queue<string>(lines);
            public override ValueTask<string> ReadLineAsync(CancellationToken ct) =>
                ValueTask.FromResult(_lines.Count > 0 ? _lines.Dequeue() : null);
            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        private class TrackingWriter : StringWriter
        {
            public bool IsDisposed { get; private set; }
            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        [Fact]
        public async Task ConnectAsync_LogsGreetingAndSetsStreams()
        {
            // Arrange
            var greeting = "220 Service ready";
            var reader = new TrackingReader(new[] { greeting });
            var writer = new TrackingWriter();

            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(Host, Port, It.IsAny<CancellationToken>()))
                .ReturnsAsync((reader, writer));

            var loggerMock = new Mock<ILogger>();
            var client = new SmtpClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 5
            };

            // Act
            await client.ConnectAsync(Host, Port, CancellationToken.None);

            // Assert
            writer.ToString().Should().BeEmpty();
            loggerMock.Verify(l => l.Information(greeting), Times.Once);
        }

        [Fact]
        public async Task ConnectAsync_FactoryThrows_LogsAndRethrows()
        {
            // Arrange
            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Connect failed"));

            var loggerMock = new Mock<ILogger>();
            var client = new SmtpClient(factoryMock.Object, loggerMock.Object);

            // Act
            Func<Task> act = () => client.ConnectAsync(Host, Port, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<IOException>();
            loggerMock.Verify(l => l.Error("Connect failed"), Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_SendsAuthCommandAndLogsResponse()
        {
            // Arrange
            var response = "235 Authentication successful";
            var reader = new TrackingReader(new[] { response });
            var writer = new TrackingWriter();

            var factoryMock = new Mock<ISmtpConnectionFactory>();
            var client = new SmtpClient(factoryMock.Object, Mock.Of<ILogger>());
            // inject streams
            typeof(SmtpClient).GetField("_reader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, reader);
            typeof(SmtpClient).GetField("_writer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, writer);

            var loggerMock = new Mock<ILogger>();
            typeof(SmtpClient).GetField("_logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, loggerMock.Object);

            // Act
            await client.AuthenticateAsync(Username, Password, CancellationToken.None);

            // Assert
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes("\0" + Username + "\0" + Password));
            writer.ToString().Should().Contain($"{SmtpCommands.AuthPlain}{payload}");
            loggerMock.Verify(l => l.Information(response), Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_SendsQuitAndDisposesStreams_OnlyOnce()
        {
            // Arrange
            var reader = new TrackingReader(Array.Empty<string>());
            var writer = new TrackingWriter();
            var client = new SmtpClient(Mock.Of<ISmtpConnectionFactory>(), Mock.Of<ILogger>());
            typeof(SmtpClient).GetField("_reader", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, reader);
            typeof(SmtpClient).GetField("_writer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(client, writer);

            // Act
            await client.DisposeAsync();
            await client.DisposeAsync();

            // Assert
            writer.ToString().Should().Contain(SmtpCommands.Quit);
            reader.IsDisposed.Should().BeTrue();
            writer.IsDisposed.Should().BeTrue();
        }
    }
}