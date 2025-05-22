using FluentAssertions;
using MailClient.Contracts;
using MailClient.Models;
using Moq;
using Serilog;

namespace MailClient.Tests
{
    public class ImapClientTests
    {
        private const string Host = "imap.example.com";
        private const int Port = 993;
        private const string Username = "user";
        private const string Password = "pass";
        private const string Mailbox = "INBOX";

        // Helper classes to track disposal
        private class TrackingReader : TextReader
        {
            private readonly Queue<string> _lines;
            public bool IsDisposed { get; private set; }
            public TrackingReader(IEnumerable<string> lines)
            {
                _lines = new Queue<string>(lines);
            }
            public override ValueTask<string> ReadLineAsync(CancellationToken ct)
            {
                if(_lines.Count == 0) return ValueTask.FromResult<string>(null);
                return ValueTask.FromResult(_lines.Dequeue());
            }
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

        private FetchMessagesRequest CreateRequest() => new FetchMessagesRequest
        {
            Host = Host,
            Port = Port,
            Username = Username,
            Password = Password,
            Mailbox = Mailbox
        };

        [Fact]
        public async Task FetchAllMessagesAsync_NoMessages_ReturnsEmptyList()
        {
            // Arrange
            var lines = new[] {
                "* OK IMAP4rev1 Service Ready",
                // Login echo and tag
                "A001 OK LOGIN completed",
                // Select echo and tag
                "A002 OK SELECT completed",
                // SEARCH returns no ids
                "* SEARCH",
                "A003 OK SEARCH completed"
            };
            var reader = new TrackingReader(lines);
            var writer = new TrackingWriter();

            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(Host, Port, It.IsAny<CancellationToken>()))
                .ReturnsAsync((reader, writer));

            var loggerMock = new Mock<ILogger>();
            var client = new ImapClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 5
            };

            // Act
            var result = await client.FetchAllMessagesAsync(CreateRequest(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task FetchAllMessagesAsync_WithMessages_ReturnsRawEmailResponses()
        {
            // Arrange
            var uidList = new[] { "100", "200" };
            var lines = new List<string>
            {
                "* OK IMAP4rev1 Service Ready",
                // LOGIN responses
                "A001 OK LOGIN completed",
                // SELECT responses
                "A002 OK SELECT completed",
                // UID SEARCH responses
                "* SEARCH ALL 100 200",
                "A003 OK SEARCH completed",
                // FETCH start for two messages
                "* 1 FETCH (UID 100 RFC822 {123}",
                "Subject: Test1",
                "Body line1",
                // Next FETCH
                "* 2 FETCH (UID 200 RFC822 {456}",
                "Subject: Test2",
                "Body line2",
                // Final tag
                "A004 OK FETCH completed"
            };
            var reader = new TrackingReader(lines);
            var writer = new TrackingWriter();

            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(Host, Port, It.IsAny<CancellationToken>()))
                .ReturnsAsync((reader, writer));

            var loggerMock = new Mock<ILogger>();
            var client = new ImapClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 5
            };

            // Act
            var result = await client.FetchAllMessagesAsync(CreateRequest(), CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result[0].Uid.Should().Be("100");
            result[0].RawContent.Should().Contain("Subject: Test1").And.Contain("Body line1");
            result[1].Uid.Should().Be("200");
            result[1].RawContent.Should().Contain("Subject: Test2").And.Contain("Body line2");
        }

        [Fact]
        public async Task FetchAllMessagesAsync_CanceledBeforeTimeout_ThrowsOperationCanceledException()
        {
            // Arrange: simulate a hang on ConnectAsync by never returning
            var factoryMock = new Mock<ISmtpConnectionFactory>();
            factoryMock
                .Setup(f => f.CreateSecureConnectionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns<string, int, CancellationToken>(async (h, p, ct) =>
                {
                    await Task.Delay(Timeout.Infinite, ct);
                    return (null, null);
                });
            var loggerMock = new Mock<ILogger>();
            var client = new ImapClient(factoryMock.Object, loggerMock.Object)
            {
                DefaultTimeoutSeconds = 1
            };
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(100));

            // Act
            Func<Task> act = () => client.FetchAllMessagesAsync(CreateRequest(), cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
