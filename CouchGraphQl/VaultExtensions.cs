namespace CouchGraphQl;

internal static class VaultExtensions
{
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder builder, Action<VaultOptions> configureOptions)
    {
        var configurationSource = new VaultConfigurationSource(configureOptions);
        builder.Add(configurationSource);
        return builder;
    }
}