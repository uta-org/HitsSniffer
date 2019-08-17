using System.Data;
using System.Data.SqlClient;
using Colorful;

namespace HitsSniffer.Controller
{
    public static class SqlWorker
    {
        public static void InsertQuery(string rawData)
        {
            // TODO: Get connectionString from ENV
            //string connectionString =
            //    "Server=localhost;" +
            //    "Database=jardineria;" +
            //    "User ID=usuario;" +
            //    "Password=usuario;" +
            //    "Pooling=false";

            using (IDbConnection dbcon = new SqlConnection(connectionString))
            {
                dbcon.Open();
                using (IDbCommand dbcmd = dbcon.CreateCommand())
                {
                    string sql =
                        "SELECT * FROM gamasproducto";
                    dbcmd.CommandText = sql;
                    using (IDataReader reader = dbcmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine("Gama: {0}", reader["Gama"].ToString());
                            Console.WriteLine("Descripción: {0}", reader["DescripcionTexto"].ToString());
                        }
                    }
                }
            }
        }
    }
}