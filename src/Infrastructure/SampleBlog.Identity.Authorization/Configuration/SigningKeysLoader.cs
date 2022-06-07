using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;

namespace SampleBlog.Identity.Authorization.Configuration;

internal sealed partial class SigningKeysLoader
{
    public static X509Certificate2 LoadFromFile(string path, string password, X509KeyStorageFlags keyStorageFlags)
    {
        try
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"There was an error loading the certificate. The file '{path}' was not found.");
            }
            else if (password == null)
            {
                throw new InvalidOperationException("There was an error loading the certificate. No password was provided.");
            }

            return new X509Certificate2(path, password, keyStorageFlags);
        }
        catch (CryptographicException e)
        {
            var message = "There was an error loading the certificate. Either the password is incorrect or the process does not have permisions to " +
                          $"store the key in the Keyset '{keyStorageFlags}'";
            throw new InvalidOperationException(message, e);
        }
    }

    public static RSA LoadDevelopment(string path, bool createIfMissing)
    {
        var fileExists = File.Exists(path);

        if (!fileExists && !createIfMissing)
        {
            throw new InvalidOperationException($"Couldn't find the file '{path}' and creation of a development key was not requested.");
        }

        if (fileExists)
        {
            var rsa = JsonConvert.DeserializeObject<RSAKeyParameters>(File.ReadAllText(path));
            return rsa.GetRSA();
        }

        var parameters = RSAKeyParameters.Create();
        var directory = Path.GetDirectoryName(path);

        if (false == Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonConvert.SerializeObject(parameters));

        return parameters.GetRSA();
    }

    public static X509Certificate2 LoadFromStoreCert(
        string subject,
        string storeName,
        StoreLocation storeLocation,
        DateTimeOffset currentTime)
    {
        using (var store = new X509Store(storeName, storeLocation))
        {
            X509Certificate2Collection? storeCertificates = null;
            X509Certificate2? foundCertificate = null;

            try
            {
                store.Open(OpenFlags.ReadOnly);
                storeCertificates = store.Certificates;

                var foundCertificates = storeCertificates.Find(X509FindType.FindBySubjectDistinguishedName, subject, validOnly: false);

                foundCertificate = foundCertificates
                    .OfType<X509Certificate2>()
                    .Where(certificate => certificate.NotBefore <= currentTime && certificate.NotAfter > currentTime)
                    .MinBy(certificate => certificate.NotAfter);

                if (null == foundCertificate)
                {
                    throw new InvalidOperationException("Couldn't find a valid certificate with " +
                                                        $"subject '{subject}' on the '{storeLocation}\\{storeName}'");
                }

                return foundCertificate;
            }
            finally
            {
                DisposeCertificates(storeCertificates, except: foundCertificate);
            }
        }
    }

    private static void DisposeCertificates(X509Certificate2Collection? certificates, X509Certificate2? except)
    {
        if (null == certificates)
        {
            return;
        }

        foreach (var certificate in certificates)
        {
            if (null == except || false == certificate.Equals(except))
            {
                certificate.Dispose();
            }
        }
    }
}