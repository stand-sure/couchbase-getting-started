namespace CouchGraphQl;

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
        this.GetDatabaseCredentials().Wait();
    }

    private async Task GetDatabaseCredentials()
    {
        try
        {
            Secret<SecretData>? secret =
                await this.client.V1.Secrets.KeyValue.V2.ReadSecretAsync(path: this.options.Path, mountPoint: this.options.MountPoint);

            string? username = secret.Data.Data[nameof(username)].ToString();
            string? password = secret.Data.Data[nameof(password)].ToString();

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