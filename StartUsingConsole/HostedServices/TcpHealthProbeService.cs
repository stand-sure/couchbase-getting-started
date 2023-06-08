namespace StartUsingConsole.HostedServices;

using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TcpHealthProbeService : BackgroundService
{
    private const int DefaultHealthCheckPort = 5555;
    private const int HealthCheckRefreshSeconds = 1;

    private const string MessageTemplate = $"{nameof(TcpHealthProbeService)}: {{Message}}";
    private const string TcpPortConfigKey = "HealthProbe:TcpPort";

    private readonly HealthCheckService healthCheckService;
    private readonly TcpListener listener;
    private readonly ILogger<TcpHealthProbeService> logger;

    public TcpHealthProbeService(
        HealthCheckService healthCheckService,
        ILogger<TcpHealthProbeService> logger,
        IConfiguration? configuration)
    {
        this.healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        int port = configuration?.GetValue<int?>(TcpHealthProbeService.TcpPortConfigKey) ?? TcpHealthProbeService.DefaultHealthCheckPort;

        this.listener = new TcpListener(IPAddress.Any, port);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation(TcpHealthProbeService.MessageTemplate, "Started.");
        await Task.Yield();
        this.listener.Start();

        while (stoppingToken.IsCancellationRequested is false)
        {
            await this.UpdateHeartbeatAsync(stoppingToken).ConfigureAwait(false);
            Thread.Sleep(TimeSpan.FromSeconds(TcpHealthProbeService.HealthCheckRefreshSeconds));
        }
    }

    private async Task UpdateHeartbeatAsync(CancellationToken token)
    {
        try
        {
            HealthReport result = await this.healthCheckService.CheckHealthAsync(token).ConfigureAwait(false);
            bool isHealthy = result.Status == HealthStatus.Healthy;

            if (isHealthy is false)
            {
                this.listener.Stop();
                this.logger.LogInformation(TcpHealthProbeService.MessageTemplate, "Service is unhealthy. Listener stopped.");
                return;
            }

            this.listener.Start();

            while (this.listener.Server.IsBound && this.listener.Pending())
            {
                TcpClient client = await this.listener.AcceptTcpClientAsync(token).ConfigureAwait(false);
                client.Close();
                this.logger.LogInformation(TcpHealthProbeService.MessageTemplate, "Successfully process health check request.");
            }

            this.logger.LogDebug(TcpHealthProbeService.MessageTemplate, "Heartbeat executed.");
        }
        catch (Exception e)
        {
            this.logger.LogCritical(e, TcpHealthProbeService.MessageTemplate, "An error occurred while checking heartbeat.");
        }
    }
}