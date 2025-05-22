using MailClient.App.CommandsAndQueries.Commands;
using MailClient.App.CommandsAndQueries.Queries;
using MailClient.App.Infrastructure.Constants;
using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Models;
using MailClient.App.Services.Contracts;
using MediatR;
using Serilog;

namespace MailClient.App.Infrastructure
{
    public class AppRunner : IAppRunner
    {
        private readonly IMediator _mediator;
        private readonly IAsyncPrompt<ServerModel> _selectServerPrompt;
        private readonly IAsyncPrompt<DeleteServerModel> _deleteServerPrompt;
        private readonly IPrompt<NewEmailModel> _newEmailPrompt;
        private readonly IPrompt<ServerCredentialModel> _addServerPrompt;
        private readonly IPrompt<Operation> _operationSelectionPrompt;
        private readonly IViewUpdater<ServerCredentialModel> _addServerViewUpdater;
        private readonly IViewUpdater<NewEmailModel> _addEmailViewUpdater;
        private readonly IViewUpdater<EmailListModel> _fetchInboxViewUpdater;
        private readonly IViewUpdater<ServerModel> _selectServerViewUpdater;
        private readonly IViewUpdater<DeleteServerModel> _deleteServerViewUpdater;
        private readonly IOutputWriterService _outputWriterService;
        private readonly ILogger _logger;
        private ServerModel _selectedServer;

        public AppRunner(
            IMediator mediator,
            IAsyncPrompt<ServerModel> selectServerPrompt,
            IAsyncPrompt<DeleteServerModel> deleteServerPrompt,
            IPrompt<NewEmailModel> newEmailPrompt,
            IPrompt<ServerCredentialModel> addServerPrompt,
            IPrompt<Operation> operationSelectionPrompt,
            IViewUpdater<ServerCredentialModel> addServerViewUpdater,
            IViewUpdater<NewEmailModel> addEmailViewUpdater,
            IViewUpdater<EmailListModel> fetchInboxViewUpdater,
            IViewUpdater<ServerModel> selectServerViewUpdater,
            IViewUpdater<DeleteServerModel> deleteServerViewUpdater,
            IOutputWriterService outputWriterService,
            ILogger logger
            )
        {
            _mediator = mediator;
            _selectServerPrompt = selectServerPrompt;
            _deleteServerPrompt = deleteServerPrompt;
            _newEmailPrompt = newEmailPrompt;
            _addServerPrompt = addServerPrompt;
            _operationSelectionPrompt = operationSelectionPrompt;
            _addServerViewUpdater = addServerViewUpdater;
            _addEmailViewUpdater = addEmailViewUpdater;
            _fetchInboxViewUpdater = fetchInboxViewUpdater;
            _selectServerViewUpdater = selectServerViewUpdater;
            _deleteServerViewUpdater = deleteServerViewUpdater;
            _outputWriterService = outputWriterService;
            _logger = logger;
        }

        public async Task RunAsync(CancellationToken ct)
        {
            while(!ct.IsCancellationRequested)
            {
                try
                {
                    var selectedOperation = _operationSelectionPrompt.Run();

                    switch(selectedOperation)
                    {
                        case Operation.AddServer:
                        var serverCredentials = _addServerPrompt.Run();
                        await _mediator.Send(new AddServerCommand
                        {
                            ServerCredentials = serverCredentials
                        }, ct);
                        _addServerViewUpdater.UpdateView(serverCredentials);
                        break;

                        case Operation.SelectServer:
                        var server = await _selectServerPrompt.RunAsync(ct);
                        _selectedServer = server;
                        _selectServerViewUpdater.UpdateView(_selectedServer);
                        break;

                        case Operation.SendEmail:
                        var emailInput = _newEmailPrompt.Run();
                        await _mediator.Send(new NewEmailCommand
                        {
                            NewEmail = emailInput,
                            ServerId = _selectedServer.Id
                        }, ct);
                        _addEmailViewUpdater.UpdateView(emailInput);
                        break;

                        case Operation.FetchInbox:
                        var result = await _mediator.Send(new FetchEmailsQuery
                        {
                            ServerId = _selectedServer.Id
                        }, ct);
                        _fetchInboxViewUpdater.UpdateView(result);
                        break;

                        case Operation.DeleteServer:
                        var serverForDelete = await _deleteServerPrompt.RunAsync(ct);
                        await _mediator.Send(new DeleteServerCommand
                        {
                            ServerId = serverForDelete.Id
                        }, ct);
                        if(_selectedServer?.Id == serverForDelete.Id)
                            _selectedServer = null;
                        _deleteServerViewUpdater.UpdateView(serverForDelete);
                        break;

                        case Operation.Exit:
                        Environment.Exit(0);
                        break;
                    }
                }
                catch(OperationCanceledException)
                {
                    _outputWriterService.WriteLine("Operation was cancelled.");
                }
                catch(Exception ex)
                {
                    _logger.Error(ex, $"Error during '{ex.TargetSite?.Name}': {ex.Message}");
                    _outputWriterService.WriteLine($"Error: {ex.Message}");
                    _outputWriterService.WriteLine("Please try again.\n");
                }
            }
        }
    }
}