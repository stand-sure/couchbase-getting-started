namespace CouchGraphQl.GraphQl;

using HotChocolate.Execution;

using JetBrains.Annotations;

[UsedImplicitly]
internal class DetailRemovingErrorFilter : IErrorFilter
{
    private readonly IHostEnvironment environment;
    private readonly ILogger<DetailRemovingErrorFilter> logger;

    public DetailRemovingErrorFilter(IHostEnvironment environment, ILogger<DetailRemovingErrorFilter> logger)
    {
        this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IError OnError(IError error)
    {
        Exception? e = error.Exception;
        var location = $"{e?.TargetSite?.DeclaringType}.{e?.TargetSite?.Name}";
        this.logger.LogError(e, "Error: {Location} {Message} {Stack}", location, e?.Message, e?.StackTrace);

        return this.environment.IsDevelopment() ? error : error.RemoveLocations().RemoveCode().RemoveExtension("stackTrace").WithException(new QueryException(error.Message)).WithMessage(error.Message);
    }
}