using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


namespace archivsoftware.DataAccess
{
    public static class ConnectionTest
    {
        public static void TestConnection()
        {
            string connectionString =
                "Server=OFFICE\\SQLEXPRESS;" +
                "Database=DocumentArchive;" +
                "Trusted_Connection=True;" +
                "TrustServerCertificate=True;";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    Debug.WriteLine("Verbindung erfolgreich!"); 
                    Debug.WriteLine($"Database: {connection.Database}"); 
                    Debug.WriteLine($"Server: {connection.DataSource}"); 
                    Debug.WriteLine($"State: {connection.State}"); 
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler bei der Verbindung:"); 
                Debug.WriteLine($"   {ex.Message}"); 
            }
        }
    }
}