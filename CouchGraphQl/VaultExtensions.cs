namespace CouchGraphQl;

using JetBrains.Annotations;

internal static class VaultExtensions
{
    [PublicAPI]
    public static IConfigurationBuilder AddVault(this IConfigurationBuilder builder)
    {
        IConfiguration configuration = builder.Build();
        IConfigurationSection configurationSection = configuration.GetSection("Vault");

        if (configurationSection.Exists() is false)
        {
            return builder;
        }

        void ConfigureOptions(VaultOptions options) => configurationSection.Bind(options);

        var configurationSource = new VaultConfigurationSource(ConfigureOptions);
        builder.Add(configurationSource);
        
        return builder;
    }
}