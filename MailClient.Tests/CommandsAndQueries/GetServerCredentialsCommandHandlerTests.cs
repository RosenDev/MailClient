using FluentAssertions;
using MailClient.App.CommandsAndQueries.Commands;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using Moq;

namespace MailClient.Tests.CommandsAndQueries
{
    public class GetServerCredentialsCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsModel()
        {
            var model = new ServerCredentialModel { ImapServerAddress = "imap" };
            var serviceMock = new Mock<IServerCredentialsService>();
            serviceMock
                .Setup(s => s.GetCredentialsByServerAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);
            var handler = new GetServerCredentialsCommandHandler(serviceMock.Object);
            var command = new GetServerCredentialsCommand { Id = 5 };
            var result = await handler.Handle(command, CancellationToken.None);
            result.Should().BeSameAs(model);
        }

        [Fact]
        public async Task Handle_ServiceThrows_ExceptionPropagates()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            serviceMock
                .Setup(s => s.GetCredentialsByServerAsync(6, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("not found"));
            var handler = new GetServerCredentialsCommandHandler(serviceMock.Object);
            var command = new GetServerCredentialsCommand { Id = 6 };
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("not found");
        }

        [Fact]
        public async Task Handle_PassesCancellationToken_ToService()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var expected = new ServerCredentialModel();
            serviceMock
                .Setup(s => s.GetCredentialsByServerAsync(7, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
            var handler = new GetServerCredentialsCommandHandler(serviceMock.Object);
            var command = new GetServerCredentialsCommand { Id = 7 };
            using var cts = new CancellationTokenSource();
            await handler.Handle(command, cts.Token);
            serviceMock.Verify(s => s.GetCredentialsByServerAsync(7, cts.Token), Times.Once);
        }
    }
}