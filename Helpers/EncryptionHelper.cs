using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides methods for encryption and decryption of text.
/// </summary>
/// 
namespace Ihelpers.Helpers
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// Encrypts the specified clear text.
        /// </summary>
        /// <param name="clearText">The clear text.</param>
        /// <returns>The encrypted text.</returns>
        public static async Task<string> Encrypt(string clearText, string? encryptKey = null)
        {
            try
            {
                string EncryptionKey = encryptKey ?? GetEncryptKey();
                byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                            cs.Close();
                        }
                        clearText = Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return clearText;
        }

        /// <summary>
        /// Decrypts the specified encripted text.
        /// </summary>
        /// <param name="cipherText">The cipher text.</param>
        /// <returns>The decrypted text.</returns>
        public static async Task<string> Decrypt(string cipherText, string? encryptKey = null)
        {
            try
            {
                string EncryptionKey = encryptKey ?? GetEncryptKey();
                cipherText = cipherText.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return cipherText;
        }

        /// <summary>
        /// Returns the configured EncryptKey
        /// </summary>
        /// <returns></returns>
        private static string GetEncryptKey()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .Build();
            var AESKEY = configuration.GetValue("EncryptKey:DefaultKey", "");

            return AESKEY;
        }
    }
}
