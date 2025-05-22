using FluentAssertions;
using MailClient.App.CommandsAndQueries.Queries;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using Moq;

namespace MailClient.Tests.CommandsAndQueries
{
    public class FetchServersQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsServerList()
        {
            var expected = new ServerListModel { Servers = new() { new ServerModel { Id = 1, DisplayName = "d" } } };
            var serviceMock = new Mock<IServerCredentialsService>();
            serviceMock
                .Setup(s => s.FetchServersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
            var handler = new FetchServersQueryHandler(serviceMock.Object);
            var command = new FetchServersQuery();
            var result = await handler.Handle(command, CancellationToken.None);
            result.Should().BeSameAs(expected);
        }

        [Fact]
        public async Task Handle_ServiceThrows_ExceptionPropagates()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            serviceMock
                .Setup(s => s.FetchServersAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));
            var handler = new FetchServersQueryHandler(serviceMock.Object);
            Func<Task> act = () => handler.Handle(new FetchServersQuery(), CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("fail");
        }

        [Fact]
        public async Task Handle_PassesCancellationToken_ToService()
        {
            var serviceMock = new Mock<IServerCredentialsService>();
            var expected = new ServerListModel();
            serviceMock
                .Setup(s => s.FetchServersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);
            var handler = new FetchServersQueryHandler(serviceMock.Object);
            using var cts = new CancellationTokenSource();
            await handler.Handle(new FetchServersQuery(), cts.Token);
            serviceMock.Verify(s => s.FetchServersAsync(cts.Token), Times.Once);
        }
    }
}
