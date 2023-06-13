namespace CouchGraphQl.GraphQl;

using HotChocolate.Execution;

using JetBrains.Annotations;

using Serilog;

[UsedImplicitly]
internal class DetailRemovingErrorFilter : IErrorFilter
{
    private readonly IHostEnvironment environment;

    public DetailRemovingErrorFilter(IHostEnvironment environment)
    {
        this.environment = environment;
    }

    public IError OnError(IError error)
    {
        Exception? e = error.Exception;
        var location = $"{e?.TargetSite?.DeclaringType}.{e?.TargetSite?.Name}";
        Log.Error(e, "Error: {Location} {Message} {Stack}", location, e?.Message, e?.StackTrace);

        return this.environment.IsDevelopment() ? error : error.RemoveLocations().RemoveCode().RemoveExtension("stackTrace").WithException(new QueryException(error.Message)).WithMessage(error.Message);
    }
}