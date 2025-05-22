using MailClient.App.CommandsAndQueries.Commands;
using MailClient.App.Data;
using MailClient.App.Infrastructure;
using MailClient.App.Infrastructure.Common;
using MailClient.App.Infrastructure.Constants;
using MailClient.App.Infrastructure.Contracts;
using MailClient.App.Infrastructure.Contracts.MailClient.App.Infrastructure.Contracts;
using MailClient.App.Infrastructure.Prompts;
using MailClient.App.Infrastructure.ViewUpdaters;
using MailClient.App.Models;
using MailClient.App.Services;
using MailClient.App.Services.Contracts;
using MailClient.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SQLitePCL;

namespace MailClient.App
{
    public static class AppServices
    {
        public static IServiceProvider RegisterAppServices()
        {
            var logger = new LoggerConfiguration()
            .WriteTo
            .File("applicationLog.txt")
            .MinimumLevel
            .Information()
            .CreateLogger();

            var configuration = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

            var sqliteOptions = configuration.GetSection("SqliteSettings")
                .Get<SqliteConnectionOptions>();

            raw.SetProvider(new SQLite3Provider_e_sqlcipher());
            Batteries.Init();

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = sqliteOptions.DataSource,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Password = sqliteOptions.Password
            };

            string connectionString = builder.ToString();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<ILogger>(logger)
                .AddSingleton<ISmtpConnectionFactory, TcpSmtpConnectionFactory>()
                .AddTransient<IImapClient, ImapClient>()
                .AddTransient<ISmtpClient, SmtpClient>()
                .AddSingleton<IInputReaderService, ConsoleInputReaderService>()
                .AddSingleton<IOutputWriterService, ConsoleOutputWriterService>()
                .AddSingleton<IServerCredentialsService, ServerCredentialsService>()
                .AddSingleton<IEmailStoreService, EmailStoreService>()
                .AddSingleton<IEmailMessageParser, EmailMessageParser>()
                .AddSingleton<IPrompt<Operation>, SelectOperationPrompt>()
                .AddSingleton<IAsyncPrompt<ServerModel>, SelectServerPrompt>()
                .AddSingleton<IPrompt<NewEmailModel>, NewEmailPrompt>()
                .AddSingleton<IPrompt<ServerCredentialModel>, AddServerPrompt>()
                .AddSingleton<IAsyncPrompt<DeleteServerModel>, DeleteServerPrompt>()
                .AddSingleton<IViewUpdater<ServerModel>, SelectServerViewUpdater>()
                .AddSingleton<IViewUpdater<NewEmailModel>, NewEmailViewUpdater>()
                .AddSingleton<IViewUpdater<ServerCredentialModel>, AddServerViewUpdater>()
                .AddSingleton<IViewUpdater<EmailListModel>, FetchInboxViewUpdater>()
                .AddSingleton<IViewUpdater<DeleteServerModel>, DeleteServerViewUpdater>()
                .AddSingleton<IAppRunner, AppRunner>()
                .AddDbContext<MailClientAppDbContext>(opts =>
                    opts.UseSqlite(connectionString)
                )
                .AddMediatR(x => x.RegisterServicesFromAssembly(typeof(NewEmailCommand).Assembly))
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
