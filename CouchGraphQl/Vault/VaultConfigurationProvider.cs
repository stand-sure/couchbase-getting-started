namespace CouchGraphQl.Vault;

using Serilog;

using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.UserPass;
using VaultSharp.V1.Commons;

internal class VaultConfigurationProvider : ConfigurationProvider
{
    private readonly VaultOptions options;
    private readonly IVaultClient client;

    public VaultConfigurationProvider(VaultOptions options)
    {
        this.options = options;

        this.client = this.MakeClient();
    }

    private IVaultClient MakeClient()
    {
       IAuthMethodInfo authInfo = new UserPassAuthMethodInfo(this.options.UserName, this.options.Password);
        
        var clientSettings = new VaultClientSettings(this.options.Address, authInfo);

        var vaultClient = new VaultClient(clientSettings);
        
        return vaultClient;
    }

    public override void Load()
    {
        this.LoadAsync().Wait();
    }

    private async Task LoadAsync()
    {
        await this.GetDatabaseCredentialsAsync().ConfigureAwait(false);
    }

    private async Task GetDatabaseCredentialsAsync()
    {
        try
        {
            Secret<SecretData>? secret =
                await this.client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: this.options.Path, mountPoint: this.options.MountPoint);

            string? username = secret.Data.Data.TryGetValue(nameof(username), out object? name) ? name?.ToString() : null;
            string? password = secret.Data.Data.TryGetValue(nameof(password), out object? pass) ? pass?.ToString() : null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            this.Data.Add($"couchbase:{nameof(username)}", username);
            this.Data.Add($"couchbase:{nameof(password)}", password);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "{Message}", ex.Message);
        }
    }
}