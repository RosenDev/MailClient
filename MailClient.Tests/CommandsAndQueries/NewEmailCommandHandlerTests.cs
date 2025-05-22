using FluentAssertions;
using MailClient.App.CommandsAndQueries.Commands;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using MailClient.Contracts;
using MailClient.Models;
using Moq;

namespace MailClient.Tests.CommandsAndQueries
{
    public class NewEmailCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_CallsSmtpMethodsInOrder()
        {
            var smtpMock = new Mock<ISmtpClient>();
            var credsModel = new ServerCredentialModel
            {
                SmtpServerAddress = "smtp.ex",
                SmtpPort = 587,
                Username = "u",
                Password = "p"
            };
            var credServiceMock = new Mock<IServerCredentialsService>();
            credServiceMock
                .Setup(s => s.GetCredentialsByServerAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(credsModel);
            var handler = new NewEmailCommandHandler(smtpMock.Object, credServiceMock.Object);
            var newEmail = new NewEmailModel
            {
                From = "from@ex",
                To = new List<string> { "to@ex" },
                Cc = new List<string> { "cc@ex" },
                Subject = "sub",
                Body = "body"
            };
            var command = new NewEmailCommand { ServerId = 1, NewEmail = newEmail };
            await handler.Handle(command, CancellationToken.None);
            credServiceMock.Verify(s => s.GetCredentialsByServerAsync(1, CancellationToken.None), Times.Once);
            smtpMock.Verify(s => s.ConnectAsync("smtp.ex", 587, CancellationToken.None), Times.Once);
            smtpMock.Verify(s => s.AuthenticateAsync("u", "p", CancellationToken.None), Times.Once);
            smtpMock.Verify(s => s.SendMailAsync(It.Is<EmailMessageRequest>(r =>
                r.From == "from@ex" &&
                r.To.SequenceEqual(newEmail.To) &&
                r.Cc.SequenceEqual(newEmail.Cc) &&
                r.Subject == "sub" &&
                r.Body == "body"), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task Handle_ServiceThrows_ExceptionPropagates()
        {
            var smtpMock = new Mock<ISmtpClient>();
            var credServiceMock = new Mock<IServerCredentialsService>();
            credServiceMock
                .Setup(s => s.GetCredentialsByServerAsync(2, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("creds fail"));
            var handler = new NewEmailCommandHandler(smtpMock.Object, credServiceMock.Object);
            var command = new NewEmailCommand { ServerId = 2, NewEmail = new NewEmailModel() };
            Func<Task> act = () => handler.Handle(command, CancellationToken.None);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("creds fail");
        }

        [Fact]
        public async Task Handle_PassesCancellationToken_ToAllCalls()
        {
            var smtpMock = new Mock<ISmtpClient>();
            var credsModel = new ServerCredentialModel
            {
                SmtpServerAddress = "smtp.ex",
                SmtpPort = 25,
                Username = "u",
                Password = "p"
            };
            var credServiceMock = new Mock<IServerCredentialsService>();
            credServiceMock
                .Setup(s => s.GetCredentialsByServerAsync(3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(credsModel);
            var handler = new NewEmailCommandHandler(smtpMock.Object, credServiceMock.Object);
            var command = new NewEmailCommand { ServerId = 3, NewEmail = new NewEmailModel() };
            using var cts = new CancellationTokenSource();
            await handler.Handle(command, cts.Token);
            credServiceMock.Verify(s => s.GetCredentialsByServerAsync(3, cts.Token), Times.Once);
            smtpMock.Verify(s => s.ConnectAsync(It.IsAny<string>(), It.IsAny<int>(), cts.Token), Times.Once);
            smtpMock.Verify(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), cts.Token), Times.Once);
            smtpMock.Verify(s => s.SendMailAsync(It.IsAny<EmailMessageRequest>(), cts.Token), Times.Once);
        }
    }
}