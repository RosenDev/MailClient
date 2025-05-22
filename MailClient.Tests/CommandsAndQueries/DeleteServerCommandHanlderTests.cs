using FluentAssertions;
using MailClient.App.CommandsAndQueries.Commands;
using MailClient.App.Services.Contracts;
using Moq;

namespace MailClient.Tests.CommandsAndQueries
{
    public class DeleteServerCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_CallsDeleteServerOnce()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var handler = new DeleteServerCommandHandler(serviceMock.Object);
            var command = new DeleteServerCommand { ServerId = 123 };
            await handler.Handle(command, CancellationToken.None);
            serviceMock.Verify(s => s.DeleteServerAsync(123, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ServiceThrows_ExceptionPropagates()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var handler = new DeleteServerCommandHandler(serviceMock.Object);
            var command = new DeleteServerCommand { ServerId = 456 };
            serviceMock
                .Setup(s => s.DeleteServerAsync(456, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("delete fail"));
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("delete fail");
        }

        [Fact]
        public async Task Handle_PassesCancellationToken_ToService()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var handler = new DeleteServerCommandHandler(serviceMock.Object);
            var command = new DeleteServerCommand { ServerId = 789 };
            using var cts = new CancellationTokenSource();
            await handler.Handle(command, cts.Token);
            serviceMock.Verify(s => s.DeleteServerAsync(789, cts.Token), Times.Once);
        }
    }
}