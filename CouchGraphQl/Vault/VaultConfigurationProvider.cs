namespace CouchGraphQl.Vault;

using Serilog;

using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp.V1.Commons;

internal class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly IVaultClient client;
    private readonly VaultOptions options;

    public VaultConfigurationProvider(VaultOptions options)
    {
        this.options = options;
        this.client = this.MakeClient();
    }

    public override void Load()
    {
        this.LoadAsync().Wait();
    }

    private async Task GetDatabaseCredentialsAsync()
    {
        try
        {
            Secret<SecretData>? secret =
                await this.client.V1.Secrets.KeyValue.V2.ReadSecretAsync(this.options.Path, mountPoint: this.options.MountPoint);

            IDictionary<string, string> dictionary = new DictionaryFlattener().Flatten(secret.Data.Data);

            foreach ((string? key, string? value) in dictionary)
            {
                this.Data.Add(key, value);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "{Message}", ex.Message);
        }
    }

    private async Task LoadAsync()
    {
        await this.GetDatabaseCredentialsAsync().ConfigureAwait(false);
    }

    private IVaultClient MakeClient()
    {
        IAuthMethodInfo authInfo = new UserPassAuthMethodInfo(this.options.UserName, this.options.Password);

        var clientSettings = new VaultClientSettings(this.options.Address, authInfo);

        var vaultClient = new VaultClient(clientSettings);

        return vaultClient;
    }
}