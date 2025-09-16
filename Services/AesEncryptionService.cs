using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // Encryption Service
    public class AesEncryptionService
    {
        private const int KeySize = 128;
        private const int IvSize = 16; // 128 bits

        public static string Encrypt(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(password))
                return string.Empty;

            byte[] encrypted;
            byte[] iv = new byte[IvSize];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(iv);
            }

            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.Key = DeriveKeyFromPassword(password);
                aes.IV = iv;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }

            // Combine IV and encrypted data
            var result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(password))
                return string.Empty;

            try
            {
                var fullCipher = Convert.FromBase64String(cipherText);

                // Extract IV
                var iv = new byte[IvSize];
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);

                // Extract encrypted data
                var encrypted = new byte[fullCipher.Length - IvSize];
                Buffer.BlockCopy(fullCipher, IvSize, encrypted, 0, encrypted.Length);

                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.Key = DeriveKeyFromPassword(password);
                    aes.IV = iv;

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var msDecrypt = new MemoryStream(encrypted))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Decryption failed: {ex.Message}");
                return string.Empty;
            }
        }

        private static byte[] DeriveKeyFromPassword(string password)
        {
            // Use a fixed salt for key derivation (in production, consider using a random salt per user)
            var salt = Encoding.UTF8.GetBytes("SoldiersFormFramework2024");

            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return rfc2898.GetBytes(KeySize / 8); // 128 bits = 16 bytes
            }
        }
    }
}
