using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.DataProtection;

namespace TripFrogWebApi.KeyVaultClasses
{
    public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
    {
        private readonly string _prefix;

        public PrefixKeyVaultSecretManager(string prefix)
        {
            _prefix = $"{prefix}-";
        }

        public override bool Load(SecretProperties secret) => secret.Name.StartsWith(_prefix);
        public override string GetKey(KeyVaultSecret secret) => secret.Name.Substring(_prefix.Length)
            .Replace("--", ConfigurationPath.KeyDelimiter);
    }
}
