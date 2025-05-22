using MailClient.App;
using MailClient.App.Data;
using MailClient.App.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

try
{
    using var cts = new CancellationTokenSource();

    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    var serviceProvider = AppServices.RegisterAppServices();

    using var scope = serviceProvider.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<MailClientAppDbContext>();

    await db.Database.MigrateAsync(cts.Token);

    var appRunner = scope.ServiceProvider.GetRequiredService<IAppRunner>();

    await appRunner.RunAsync(cts.Token);
}
catch(Exception)
{
    Environment.Exit(0);
}