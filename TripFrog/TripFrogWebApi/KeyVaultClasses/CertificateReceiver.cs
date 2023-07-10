using System.Security.Cryptography.X509Certificates;

namespace TripFrogWebApi.KeyVaultClasses;

public static class CertificateReceiver
{
    public static X509Certificate2 GetCertificate(string thumbprint)
    {
        var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        try
        {
            store.Open(OpenFlags.ReadOnly);
            var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (certificateCollection.Count == 0)
            {
                throw new Exception("Certificate is not valid");
            }

            return certificateCollection[0];
        }
        finally
        {
            store.Close();
        }
    }
}
