using IntegrationGatewayProcessor.Helpers;
using IntegrationGatewayProcessor.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {

        // Load configuration from environment variables (Azure Functions settings)
        config.AddEnvironmentVariables();
    })
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.AddHttpClient();
        s.AddScoped<IAzureRelaySenderService, AzureRelaySenderService>();
        s.AddScoped<IAzureRelayServiceHelper, AzureRelayServiceHelper>();

    })
    
    .Build();

host.Run();
