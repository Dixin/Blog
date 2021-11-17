namespace Examples.Security.Cryptography;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Examples.Common;

public static class X509Certificate2Helper
{
    public static X509Certificate2? Find(
        Func<X509Certificate2, bool> predicate,
        StoreLocation location = StoreLocation.CurrentUser)
    {
        predicate.NotNull();

        using X509Store store = new(location);
        store.Open(OpenFlags.ReadOnly);
        X509Certificate2? certificate = store.Certificates.OfType<X509Certificate2>().FirstOrDefault(predicate);

        return certificate;
    }

    public static string Encrypt(this X509Certificate2 x509, string value, Encoding encoding)
    {
        encoding.NotNull();

        using RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.NotNull().PublicKey.Key;
        byte[] bytesToEncrypt = encoding.GetBytes(value);
        byte[] encryptedBytes = rsa.Encrypt(bytesToEncrypt, false);

        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(this X509Certificate2 x509, string value,  Encoding encoding)
    {
        encoding.NotNull();

        RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)(x509.NotNull().PrivateKey ?? throw new ArgumentOutOfRangeException(nameof(x509)));
        byte[] bytesToDecrypt = Convert.FromBase64String(value);
        byte[] decryptedBytes = rsa.Decrypt(bytesToDecrypt, false);
        return encoding.GetString(decryptedBytes);
    }
}