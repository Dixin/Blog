namespace Dixin.Security.Cryptography
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public static class X509Certificate2Helper
    {
        public static X509Certificate2 Find(
            Func<X509Certificate2, bool> predicate,
            StoreLocation location = StoreLocation.CurrentUser)
        {
            Contract.Requires<ArgumentNullException>(predicate != null);

            X509Certificate2 certificate;
            using (X509Store store = new X509Store(location))
            {
                store.Open(OpenFlags.ReadOnly);
                certificate = store.Certificates.OfType<X509Certificate2>().FirstOrDefault(predicate);
                store.Close();
            }

            return certificate;
        }

        public static string Encrypt(this X509Certificate2 x509, string value, Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(x509 != null);

            byte[] encryptedBytes;
            using (RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.PublicKey.Key)
            {
                byte[] bytesToEncrypt = encoding.GetBytes(value);
                encryptedBytes = rsa.Encrypt(bytesToEncrypt, false);
            }

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(this X509Certificate2 x509, string value,  Encoding encoding)
        {
            Contract.Requires<ArgumentNullException>(x509 != null);

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)x509.PrivateKey;
            byte[] bytesToDecrypt = Convert.FromBase64String(value);
            byte[] decryptedBytes = rsa.Decrypt(bytesToDecrypt, false);
            return encoding.GetString(decryptedBytes);
        }
    }
}
