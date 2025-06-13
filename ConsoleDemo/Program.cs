using Core.Interfaces;
using Infrastructure.Options;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<ExternalApiOptions>(context.Configuration.GetSection("ExternalApi"));
                services.AddHttpClient<IUserService, ExternalUserService>();
            })
            .Build();
var service = host.Services.GetRequiredService<IUserService>();
try
{
    Console.WriteLine("Fetching all users...");
    var allUsers = await service.GetAllUsersAsync();
    foreach (var user in allUsers)
    {
        Console.WriteLine($"{user.Id}: {user.FirstName} {user.LastName} - {user.Email}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}