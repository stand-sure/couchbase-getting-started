namespace CouchGraphQl;

internal class VaultConfigurationSource : IConfigurationSource
{
    private readonly VaultOptions options;

    public VaultConfigurationSource(Action<VaultOptions> config)
    {
        this.options = new VaultOptions();
        config.Invoke(this.options);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new VaultConfigurationProvider(this.options);
    }
}