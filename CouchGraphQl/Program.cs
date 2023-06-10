namespace CouchGraphQl;

using JetBrains.Annotations;

/// <summary>
/// </summary>
[UsedImplicitly]
public class Program
{
    /// <summary>
    /// </summary>
    protected Program()
    {
    }

    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Host.ConfigureHostBuilder();

        builder.Services.ConfigureServices(builder.Configuration, builder.Environment);

        WebApplication app = builder.Build();

        app.ConfigureApplicationBuilder();

        app.ConfigureEndpointRouteBuilder();

        app.Run();
    }
}