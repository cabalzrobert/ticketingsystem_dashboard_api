using System;
using System.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Comm.Securities
{
    public static class Security
    {
        private static SecureString Seed = ConvertToSecureString("Th3Qu1ckBrownF0xJumpsOv3rTh3L@zYD0g");

        public static string ConvertToUnsecureString(this SecureString securePassword)
        {
            try
            {
                if (securePassword == null)
                    throw new ArgumentNullException("Secure password cannot be null");

                IntPtr unmanangedString = IntPtr.Zero;
                try
                {
                    unmanangedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                    return Marshal.PtrToStringUni(unmanangedString);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanangedString);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static SecureString ConvertToSecureString(this string password)
        {
            try
            {
                if (password == null)
                    throw new ArgumentNullException("Password cannot be null");

                SecureString sc = new SecureString();

                foreach (char item in password)
                {
                    sc.AppendChar(item);
                }

                sc.MakeReadOnly();
                return sc;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ReturnEncryptedPassword(this SecureString unencryptedPassword, string salt)
        {
            try
            {
                //System.Windows.Forms.Clipboard.Clear();
                //System.Windows.Forms.Clipboard.SetText(CryptoAES.Encrypt(unencryptedPassword, Seed));
                return CryptoAES.Encrypt(unencryptedPassword, salt, Seed);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
    public static class CryptoAES
    {
        private const int SALT_BYTE_SIZE = 24;

        public static string Encrypt(this SecureString strText, string salt, SecureString strEncrKey)
        {
            try
            {
                AesCryptoServiceProvider aesSP = new AesCryptoServiceProvider();

                SHA256 sha = SHA256.Create();

                aesSP.Key = sha.ComputeHash(UnicodeEncoding.Unicode.GetBytes(strEncrKey.ConvertToUnsecureString()));
                aesSP.IV = new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xab, 0xcd, 0xef, 0x2d, 0x3a, 0x5e, 0x17, 0xcb, 0xf, 0x41, 0x44 };

                byte[] bite = UnicodeEncoding.Unicode.GetBytes(salt + strText.ConvertToUnsecureString());
                ICryptoTransform t = aesSP.CreateEncryptor();
                return Convert.ToBase64String(t.TransformFinalBlock(bite, 0, bite.Length));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CreateSalt()
        {
            byte[] salt = new byte[SALT_BYTE_SIZE];
            using (RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }
    }
}