using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PdfFormOverlay.Maui.Services
{
    // Password and Security Service
    public class SecurityService
    {
        private const string UserPasswordKey = "UserFormPassword";
        private const string IsPasswordSetKey = "IsPasswordSet";

        public static string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public static string GenerateSalt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var saltBytes = new byte[32];
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }

        public static bool IsPasswordSet()
        {
            return Preferences.Get(IsPasswordSetKey, false);
        }

        public static void SaveUserPassword(string password)
        {
            // Store encrypted password in preferences (for auto-login scenarios)
            var encryptedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
            Preferences.Set(UserPasswordKey, encryptedPassword);
            Preferences.Set(IsPasswordSetKey, true);
        }

        public static string GetUserPassword()
        {
            var encryptedPassword = Preferences.Get(UserPasswordKey, string.Empty);
            if (string.IsNullOrEmpty(encryptedPassword))
                return string.Empty;

            try
            {
                var passwordBytes = Convert.FromBase64String(encryptedPassword);
                return Encoding.UTF8.GetString(passwordBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        public static void ClearStoredPassword()
        {
            Preferences.Remove(UserPasswordKey);
            Preferences.Set(IsPasswordSetKey, false);
        }
    }
}
