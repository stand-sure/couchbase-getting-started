namespace CouchGraphQl;

using Serilog;

internal static partial class ProgramConfiguration
{
    private const string MessageTemplate = $"{nameof(ProgramConfiguration)}: {{Message}}";

    private static void LoggerConfiguration(HostBuilderContext context, IServiceProvider provider, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(provider);
    }
}