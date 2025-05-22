using FluentAssertions;
using MailClient.App.CommandsAndQueries.Commands;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using Moq;

namespace MailClient.Tests.CommandsAndQueries
{
    public class AddServerCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_CallsAddServerOnce()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var handler = new AddServerCommandHandler(serviceMock.Object);
            var model = new ServerCredentialModel { DisplayName = "d" };
            var command = new AddServerCommand { ServerCredentials = model };
            await handler.Handle(command, CancellationToken.None);
            serviceMock.Verify(s => s.AddServerAsync(model, CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ServiceThrows_ExceptionPropagates()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var handler = new AddServerCommandHandler(serviceMock.Object);
            var model = new ServerCredentialModel();
            var command = new AddServerCommand { ServerCredentials = model };
            serviceMock
                .Setup(s => s.AddServerAsync(model, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("fail");
        }

        [Fact]
        public async Task Handle_PassesCancellationToken_ToService()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var handler = new AddServerCommandHandler(serviceMock.Object);
            var model = new ServerCredentialModel();
            var command = new AddServerCommand { ServerCredentials = model };
            using var cts = new CancellationTokenSource();
            await handler.Handle(command, cts.Token);
            serviceMock.Verify(s => s.AddServerAsync(model, cts.Token), Times.Once);
        }
    }


}